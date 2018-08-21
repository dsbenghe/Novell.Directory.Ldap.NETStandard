using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
    public class DigestMD5Client : BaseSaslClient
    {
       
        public override DebugId DebugId { get; } = DebugId.ForType<DigestMD5Client>();
        private readonly string _username;
        private readonly byte[] _password;
        private readonly string _realm;
        private State _currentState = State.Initial;

        public DigestMD5Client(SaslRequest saslRequest)
            : base(saslRequest)
        {
            if (string.IsNullOrEmpty(saslRequest.AuthorizationId) || saslRequest.Credentials.IsEmpty())
            {
                throw new SaslException("Authorization ID and password must be specified");
            }
            _username = saslRequest.AuthorizationId;
            _password = saslRequest.Credentials; // Clone?
            _realm = saslRequest.RealmName;
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
                    // challenge:
                    // 1#( realm | nonce | qop | stale | maxbuf | charset | algorithm | cipher | auth-param )
                    // qop="auth,auth-int,auth-conf",cipher="3des,rc4",algorithm=md5-sess,nonce="+Upgraded+v176b482ddcd22e21e5828127b41734095a6e4fedff738d401df5aad1990bdf973348e1aeeaf096f001d27b9d50e2a32871c4c4a51365a60d8",charset=utf-8,realm="int.devdomains.org"

                    var c = Encoding.UTF8.GetString(challenge);
                    var qa = Utilclass.QueryStringHelper.ParseQueryString(c);
                    response = null;
                    _currentState = State.ValidServerResponse;
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

        private enum State
        {
            Initial = 0,

            ValidServerResponse,
            InvalidServerResponse,
            Disposed
        }
    }
}