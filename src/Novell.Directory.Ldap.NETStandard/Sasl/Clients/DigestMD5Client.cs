using Novell.Directory.Ldap.Utilclass;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Novell.Directory.Ldap.Sasl.Clients
{
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
    /// <summary>
    /// <para>
    /// Digest Authentication as defined in RFC 2831:
    /// https://tools.ietf.org/html/rfc2831
    ///
    /// RFC 6331 marks DIGEST-MD5 as obsolete/historic:
    /// https://tools.ietf.org/html/rfc6331
    ///
    /// However, it is still in use.
    /// </para>
    /// </summary>
    public partial class DigestMD5Client : BaseSaslClient
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public string RSPAuthValue { get; private set; }

        public override DebugId DebugId { get; } = DebugId.ForType<DigestMD5Client>();
        private readonly string _username;
        private readonly byte[] _password;
        private readonly string _realm;
        private readonly string _host;
        private State _currentState = State.Initial;

        public DigestMD5Client(SaslRequest saslRequest)
            : base(saslRequest)
        {
            if (saslRequest == null)
            {
                throw new ArgumentNullException(nameof(saslRequest));
            }

            if (!(saslRequest is SaslDigestMd5Request dr))
            {
                throw new ArgumentException($"{nameof(saslRequest)} must be of type {nameof(SaslDigestMd5Request)}, but was of type {saslRequest.GetType().Name}");
            }

            if (string.IsNullOrEmpty(dr.AuthorizationId) || dr.Credentials.IsEmpty())
            {
                throw new SaslException("Authorization ID and password must be specified");
            }

            _username = dr.AuthorizationId;
            _password = dr.Credentials;
            _realm = dr.RealmName;
            _host = dr.Host;
        }

        public override string MechanismName => SaslConstants.Mechanism.DigestMd5;

        public override bool HasInitialResponse => false;

        public override bool IsComplete => _currentState == State.ValidServerResponse
                    || _currentState == State.InvalidServerResponse
                    || _currentState == State.Disposed;

        protected override void Dispose(bool disposing)
        {
            _currentState = State.Disposed;
        }

        public override byte[] EvaluateChallenge(byte[] challenge)
        {
            byte[] response = null;
            switch (_currentState)
            {
                case State.Initial:
                    var challengeInfo = new ChallengeInfo(challenge);
                    response = CreateDigestResponse(challengeInfo);
                    _currentState = State.DigestResponseSent;
                    break;
                case State.DigestResponseSent:
                    if (CheckServerResponseAuth(challenge))
                    {
                        _currentState = State.ValidServerResponse;
                    }
                    else
                    {
                        _currentState = State.InvalidServerResponse;
                        throw new SaslException("Could not validate response-auth " +
                                                "value from server");
                    }

                    break;
                case State.ValidServerResponse:
                case State.InvalidServerResponse:
                    throw new SaslException("Authentication sequence is complete");
                case State.Disposed:
                    throw new SaslException("Client has already been disposed");
                default:
                    throw new SaslException("Unknown client state.");
            }

            return response;
        }

        private bool CheckServerResponseAuth(byte[] serverResponse)
        {
            if (serverResponse.IsEmpty())
            {
                return false;
            }

            // "rspauth=9248d55132cc512a91caa7d9c042aebd"
            var str = serverResponse.ToUtf8String();
            if (str.StartsWith("rspauth="))
            {
                RSPAuthValue = str.Substring("rspauth=".Length);
                return true;
            }

            return false;
        }

        private byte[] CreateDigestResponse(ChallengeInfo challenge)
        {
            if ((challenge.QOP & QualityOfProtection.AuthenticationOnly) == 0)
            {
                throw new SaslException("Currently, DigestMD5Client only supports \"auth\" QOP value.");
            }

            // cipher is only used in "auth-conf", which we don't support yet.
            if (!challenge.Algorithm.EqualsOrdinalCI("md5-sess"))
            {
                throw new SaslException($"Invalid DIGEST-MD5 Algorithm: '{challenge.Algorithm}' - must be 'md5-sess'");
            }

            if (!challenge.Realms.Contains(_realm))
            {
                // Do we care? The server would reject us anyway if the Realm is invalid
            }

            var result = new DigestResponse
            {
                Username = _username,
                Realm = _realm,
                QOP = QualityOfProtection.AuthenticationOnly,
                Charset = "utf-8",
                NonceCount = 1,
                MaxBuf = 65536,
                Nonce = challenge.Nonce,
                DigestUri = "ldap/" + _host,
            };

            var cnonce = new byte[32];
            _rng.GetBytes(cnonce);
            result.CNonce = Base64.Encode(cnonce);

            var ha1 = DigestCalcHa1(result);
            result.Response = DigestCalcResponse(result, ha1);
            var resultStr = result.ToString();
            return resultStr.ToUtf8Bytes();
        }

        private static readonly byte[] Colon = ":".ToUtf8Bytes();

        private byte[] DigestCalcHa1(DigestResponse result)
        {
            var md5 = new MD5Digest();
            byte[] hash = new byte[md5.GetDigestSize()];
            byte[] ha1 = new byte[md5.GetDigestSize()];

            md5.BlockUpdate(result.Username.ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.Realm.ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(_password);
            md5.DoFinal(hash);

            md5.BlockUpdate(hash);
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.Nonce.ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.CNonce.ToUtf8Bytes());
            md5.DoFinal(ha1);
            return ha1;
        }

        private byte[] DigestCalcResponse(DigestResponse result, byte[] ha1)
        {
            var md5 = new MD5Digest();
            var ha2 = new byte[md5.GetDigestSize()];
            var digestResponse = new byte[md5.GetDigestSize()];

            // HA2
            md5.BlockUpdate("AUTHENTICATE:".ToUtf8Bytes());
            md5.BlockUpdate(result.DigestUri.ToUtf8Bytes());
            if (result.QOP != QualityOfProtection.AuthenticationOnly)
            {
                md5.BlockUpdate(":00000000000000000000000000000000".ToUtf8Bytes());
            }

            md5.DoFinal(ha2, 0);

            // The actual Digest Response
            md5.BlockUpdate(ha1.ToHexString().ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.Nonce.ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.NonceCountString().ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(result.CNonce.ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(GetQOPString(result.QOP).ToUtf8Bytes());
            md5.BlockUpdate(Colon);
            md5.BlockUpdate(ha2.ToHexString().ToUtf8Bytes());
            md5.DoFinal(digestResponse);
            return digestResponse;
        }

        private enum State
        {
            Initial = 0,
            DigestResponseSent,
            ValidServerResponse,
            InvalidServerResponse,
            Disposed,
        }
    }
}
