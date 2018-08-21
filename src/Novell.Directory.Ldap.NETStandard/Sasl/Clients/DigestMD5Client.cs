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
        public override DebugId DebugId { get; } = DebugId.ForType<CramMD5Client>();

        public DigestMD5Client(string serverName, Hashtable props) : base(serverName, props)
        {
        }

        public override string MechanismName => SaslConstants.Mechanism.DigestMd5;

        public override bool HasInitialResponse => throw new NotImplementedException();

        public override bool IsComplete => throw new NotImplementedException();

        public override byte[] EvaluateChallenge(byte[] challenge)
        {
            throw new NotImplementedException();
        }
    }
}