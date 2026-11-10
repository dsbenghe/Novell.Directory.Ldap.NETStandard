#if NET7_0_OR_GREATER
using System;
using System.Buffers;
using System.Net;
using System.Net.Security;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    /// <summary>
    /// <para>
    /// GSSAPI (Kerberos V5) as defined in RFC 4752:
    /// https://tools.ietf.org/html/rfc4752.
    /// </para>
    /// <para>
    /// Kerberos itself is handled by <see cref="NegotiateAuthentication"/>, i.e. by the
    /// platform GSS-API/SSPI implementation.
    /// </para>
    /// <para>
    /// Only the "no security layer" option is negotiated (there is no support for
    /// auth-int/auth-conf, same as for the other mechanisms). If the server requires
    /// signing or sealing, use TLS.
    /// </para>
    /// </summary>
    public class GssApiClient : BaseSaslClient
    {
        private const byte NoSecurityLayer = 0x01;
        private const int SecurityLayerMessageLength = 4;

        public override DebugId DebugId { get; } = DebugId.ForType<GssApiClient>();
        private readonly string _targetName;
        private readonly NetworkCredential _credential;
        private readonly byte[] _authorizationId;
        private NegotiateAuthentication _authentication;
        private State _currentState = State.Initial;

        public GssApiClient(SaslRequest saslRequest)
            : base(saslRequest)
        {
            if (!(saslRequest is SaslGssApiRequest gr))
            {
                throw new ArgumentException($"{nameof(saslRequest)} must be of type {nameof(SaslGssApiRequest)}, but was of type {saslRequest.GetType().Name}");
            }

            _targetName = "ldap/" + gr.Host;
            _credential = gr.Credential ?? CredentialCache.DefaultNetworkCredentials;
            _authorizationId = gr.AuthorizationId.IsNotEmpty() ? gr.AuthorizationId.ToUtf8Bytes() : Array.Empty<byte>();
        }

        public override string MechanismName => SaslConstants.Mechanism.GssApi;

        public override bool HasInitialResponse => true;

        public override bool IsComplete => _currentState == State.Completed
                                        || _currentState == State.Disposed;

        public override byte[] EvaluateChallenge(byte[] challenge)
        {
            byte[] response = null;
            switch (_currentState)
            {
                case State.Initial:
                    _authentication = new NegotiateAuthentication(new NegotiateAuthenticationClientOptions
                    {
                        // Plain Kerberos, the GSSAPI mechanism doesn't accept SPNEGO tokens.
                        Package = "Kerberos",
                        TargetName = _targetName,
                        Credential = _credential,

                        // Needed for wrap/unwrap of the security layer messages.
                        RequiredProtectionLevel = ProtectionLevel.Sign,
                        RequireMutualAuthentication = true,
                    });
                    _currentState = State.ContextNegotiation;
                    response = NextToken(challenge);
                    break;
                case State.ContextNegotiation:
                    response = NextToken(challenge);
                    break;
                case State.ContextEstablished:
                    response = CreateSecurityLayerResponse(challenge);
                    _currentState = State.SecurityLayerSent;
                    break;
                case State.SecurityLayerSent:
                    // Final bind response; a failed bind doesn't reach us.
                    _currentState = State.Completed;
                    break;
                case State.Completed:
                    throw new SaslException("Authentication sequence is complete");
                case State.Disposed:
                    throw new SaslException("Client has already been disposed");
                default:
                    throw new SaslException("Unknown client state.");
            }

            return response;
        }

        private byte[] NextToken(byte[] challenge)
        {
            var token = _authentication.GetOutgoingBlob(challenge, out var statusCode);
            switch (statusCode)
            {
                case NegotiateAuthenticationStatusCode.ContinueNeeded:
                    break;
                case NegotiateAuthenticationStatusCode.Completed:
                    _currentState = State.ContextEstablished;
                    break;
                default:
                    throw new SaslException($"GSSAPI security context could not be established: {statusCode}");
            }

            // Once the context is established the client still has to answer, with an empty
            // response if there is no token left, to get the security layer message (RFC 4752 3.1).
            return token ?? Array.Empty<byte>();
        }

        private byte[] CreateSecurityLayerResponse(byte[] challenge)
        {
            var serverMessage = new ArrayBufferWriter<byte>(SecurityLayerMessageLength);
            var statusCode = _authentication.Unwrap(challenge, serverMessage, out _);
            if (statusCode != NegotiateAuthenticationStatusCode.Completed)
            {
                throw new SaslException($"Could not unwrap the security layer message from server: {statusCode}");
            }

            if (serverMessage.WrittenCount != SecurityLayerMessageLength)
            {
                throw new SaslException("Invalid security layer message from server");
            }

            if ((serverMessage.WrittenSpan[0] & NoSecurityLayer) == 0)
            {
                throw new SaslException("Server does not allow binding without a security layer, which is the only one supported - use TLS");
            }

            // Chosen security layer, max buffer size (0 without a security layer) and the authorization id.
            var clientMessage = new byte[SecurityLayerMessageLength + _authorizationId.Length];
            clientMessage[0] = NoSecurityLayer;
            _authorizationId.CopyTo(clientMessage, SecurityLayerMessageLength);

            var response = new ArrayBufferWriter<byte>();
            statusCode = _authentication.Wrap(clientMessage, response, false, out _);
            if (statusCode != NegotiateAuthenticationStatusCode.Completed)
            {
                throw new SaslException($"Could not wrap the security layer response: {statusCode}");
            }

            return response.WrittenSpan.ToArray();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _authentication?.Dispose();
            }

            _currentState = State.Disposed;
        }

        private enum State
        {
            Initial,
            ContextNegotiation,
            ContextEstablished,
            SecurityLayerSent,
            Completed,
            Disposed,
        }
    }
}
#endif
