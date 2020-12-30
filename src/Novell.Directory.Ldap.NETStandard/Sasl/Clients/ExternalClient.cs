namespace Novell.Directory.Ldap.Sasl.Clients
{
    /// <summary>
    /// SASL External client.
    /// </summary>
    public class ExternalClient : BaseSaslClient
    {
        public override DebugId DebugId { get; } = DebugId.ForType<ExternalClient>();
        private State _currentState = State.Initial;

        public override string MechanismName => SaslConstants.Mechanism.External;

        public override bool HasInitialResponse => false;

        public override bool IsComplete => _currentState == State.ValidServerResponse
                                        || _currentState == State.InvalidServerResponse
                                        || _currentState == State.Disposed;

        public ExternalClient(SaslRequest saslRequest)
            : base(saslRequest)
        {
        }

        public override byte[] EvaluateChallenge(byte[] challenge)
        {
            byte[] response = null;
            switch (_currentState)
            {
                case State.Initial:
                    if (challenge.Length != 0)
                    {
                        _currentState = State.InvalidServerResponse;
                        throw new SaslException("Unexpected non-zero length response.");
                    }
                    else
                    {
                        _currentState = State.ValidServerResponse;
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

        protected override void Dispose(bool disposing)
        {
            _currentState = State.Disposed;
        }

        private enum State
        {
            Initial,
            ValidServerResponse,
            InvalidServerResponse,
            Disposed,
        }
    }
}
