using System.Security.Cryptography;

namespace Novell.Directory.Ldap.Sasl.Clients
{
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms - Yes, MD5 is broken. But the LDAP Standard uses it.
    public class CramMD5Client : BaseSaslClient
    {
        public override DebugId DebugId { get; } = DebugId.ForType<CramMD5Client>();
        private readonly string _username;
        private readonly byte[] _password;
        private State _currentState = State.Initial;

        public CramMD5Client(SaslRequest saslRequest)
            : base(saslRequest)
        {
            if (string.IsNullOrEmpty(saslRequest.AuthorizationId) || saslRequest.Credentials.IsEmpty())
            {
                throw new SaslException("Authorization ID and password must be specified");
            }

            _username = saslRequest.AuthorizationId;
            _password = saslRequest.Credentials; // Clone?
        }

        public override string MechanismName => SaslConstants.Mechanism.CramMd5;

        /// <summary>
        /// No initial response for CRAM-MD5.
        /// </summary>
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
                    if (challenge.IsEmpty())
                    {
                        throw new SaslException(nameof(challenge) + " was empty");
                    }
                    else
                    {
                        response = CreateCramMd5Response(challenge).ToUtf8Bytes();
                        _currentState = State.CramMd5ResponseSent;
                    }

                    break;
                case State.CramMd5ResponseSent:
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

        private string CreateCramMd5Response(byte[] challenge)
        {
            var digest = HMacMD5(_password, challenge);
            return _username + " " + digest;
        }

        // Most failures (Invalid Credentials etc.) won't even reach us here anyway.
        private static bool CheckServerResponseAuth(byte[] serverResponse)
            => serverResponse.IsEmpty();

        private static string HMacMD5(byte[] key, byte[] input)
        {
            // TODO: Use BouncyCastle's HMAC MD5, as it's fully managed
            using (var hmd5 = new HMACMD5(key))
            {
                var hash = hmd5.ComputeHash(input);
                return hash.ToHexString();
            }
        }

        private enum State
        {
            Initial = 0,
            CramMd5ResponseSent,
            ValidServerResponse,
            InvalidServerResponse,
            Disposed,
        }
    }
}
