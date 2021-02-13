using System;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    /// <summary>
    /// The PLAIN SASL mechanism does not provide a security layer.
    ///
    /// The PLAIN mechanism should not be used without adequate data security
    /// protection as this mechanism affords no integrity or confidentiality
    /// protections itself.  The mechanism is intended to be used with data
    /// security protections provided by application-layer protocol,
    /// generally through its use of Transport Layer Security (TLS)
    /// services.
    ///
    /// RFC 4616:
    /// https://tools.ietf.org/html/rfc4616.
    /// </summary>
    public class PlainClient : BaseSaslClient
    {
        public override DebugId DebugId { get; } = DebugId.ForType<PlainClient>();
        private readonly byte[] _username;
        private readonly byte[] _password;
        private State _currentState = State.Initial;

        public override string MechanismName => SaslConstants.Mechanism.Plain;

        public override bool HasInitialResponse => true;

        public override bool IsComplete => _currentState == State.ValidServerResponse
                                        || _currentState == State.InvalidServerResponse
                                        || _currentState == State.Disposed;

        public PlainClient(SaslRequest saslRequest)
            : base(saslRequest)
        {
            _username = saslRequest.AuthorizationId.ToUtf8Bytes();
            _password = saslRequest.Credentials;
        }

        public override byte[] EvaluateChallenge(byte[] challenge)
        {
            byte[] response = null;
            switch (_currentState)
            {
                case State.Initial:
                    response = new byte[_username.Length + _password.Length + 2];
                    response[0] = 0x00;
                    Array.Copy(_username, 0, response, 1, _username.Length);
                    response[_username.Length + 1] = 0x00;
                    Array.Copy(_password, 0, response, _username.Length + 2, _password.Length);
                    _currentState = State.CredentialsSent;
                    break;
                case State.CredentialsSent:
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

        // Most failures (Invalid Credentials etc.) won't even reach us here anyway.
        private static bool CheckServerResponseAuth(byte[] serverResponse)
            => serverResponse.IsEmpty();

        protected override void Dispose(bool disposing)
        {
            _currentState = State.Disposed;
        }

        private enum State
        {
            Initial,
            CredentialsSent,
            ValidServerResponse,
            InvalidServerResponse,
            Disposed,
        }
    }
}
