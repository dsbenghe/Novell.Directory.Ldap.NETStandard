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
    public sealed class DigestMD5Client : BaseSaslClient
    {
        public static DigestMD5Client CreateClient(string authorizationId, string serverName, byte[] credentials, Hashtable props)
        {
            return new DigestMD5Client(authorizationId, serverName, credentials, props);
        }
        
        public override DebugId DebugId { get; } = DebugId.ForType<CramMD5Client>();
        private readonly string _username;
        private readonly byte[] _password;
        private State _currentState = State.Initial;

        private DigestMD5Client(string authorizationId, string serverName, byte[] credentials, Hashtable props)
            : base(serverName, props)
        {
            if (string.IsNullOrEmpty(authorizationId) || credentials.IsEmpty())
            {
                throw new SaslException("Authorization ID and password must be specified");
            }
            _username = authorizationId;
            _password = credentials; // Clone?
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
                    // qop="auth,auth-int,auth-conf",cipher="3des,rc4",algorithm=md5-sess,nonce="+Upgraded+v176b482ddcd22e21e5828127b41734095a6e4fedff738d401df5aad1990bdf973348e1aeeaf096f001d27b9d50e2a32871c4c4a51365a60d8",charset=utf-8,realm="int.devdomains.org"
                    response = null;
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