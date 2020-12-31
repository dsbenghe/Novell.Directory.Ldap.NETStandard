using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Novell.Directory.Ldap.Sasl.Clients
{
    public partial class DigestMD5Client : BaseSaslClient
    {
        private class ChallengeInfo
        {
            public QualityOfProtection QOP { get; }
            public IReadOnlyList<string> Ciphers { get; }
            public string Nonce { get; }
            public string Algorithm { get; }
            public IReadOnlyList<string> Realms { get; }
            public bool Stale { get; }
            public int? MaxBuf { get; }
            public string Charset { get; }

            // TODO: Move to helper class?
            private static readonly char[] Comma = { ',' };
            private static readonly char[] Quote = { '"' };

            public ChallengeInfo(byte[] challenge)
            {
                // 1#( realm | nonce | qop | stale | maxbuf | charset | algorithm | cipher | auth-param )

                // HACK: I don't want to parse this myself, but the only existing parser is in CacheControlHeaderValue
                // See https://github.com/dotnet/corefx/issues/31858
                var input = Encoding.UTF8.GetString(challenge);
                var values = CacheControlHeaderValue.Parse(input);

                foreach (var val in values.Extensions)
                {
                    var trimmed = val.Value.Trim(Quote);
                    switch (val.Name)
                    {
                        case "realm":
                            Realms = trimmed.Split(Comma, StringSplitOptions.RemoveEmptyEntries);
                            break;
                        case "nonce":
                            Nonce = trimmed;
                            break;
                        case "qop":
                            foreach (var qopVal in trimmed.Split(Comma, StringSplitOptions.RemoveEmptyEntries))
                            {
                                switch (qopVal)
                                {
                                    case "auth":
                                        QOP |= QualityOfProtection.AuthenticationOnly;
                                        break;
                                    case "auth-int":
                                        QOP |= QualityOfProtection.AuthenticationWithIntegrityProtection;
                                        break;
                                    case "auth-conf":
                                        QOP |= QualityOfProtection.AuthenticationWithIntegrityAndPrivacyProtection;
                                        break;
                                }
                            }

                            break;
                        case "stale":
                            Stale = trimmed == "true";
                            break;
                        case "maxbuf":
                            MaxBuf = int.Parse(trimmed);
                            break;
                        case "charset":
                            Charset = trimmed;
                            break;
                        case "algorithm":
                            Algorithm = trimmed;
                            break;
                        case "cipher":
                            Ciphers = trimmed.Split(Comma, StringSplitOptions.RemoveEmptyEntries);
                            break;
                    }
                }
            }
        }

        private class DigestResponse
        {
            public string Username { get; set; }
            public string Realm { get; set; }
            public string Nonce { get; set; }
            public string CNonce { get; set; }
            public int NonceCount { get; set; }
            public QualityOfProtection QOP { get; set; }
            public string DigestUri { get; set; }
            public byte[] Response { get; set; } // 16 Bytes
            public int? MaxBuf { get; set; }
            public string Charset { get; set; }

            public override string ToString()
                => $"charset={Charset},username=\"{Username}\",realm=\"{Realm}\",nonce=\"{Nonce}\",nc={NonceCountString()},cnonce=\"{CNonce}\",digest-uri=\"{DigestUri}\",maxbuf={MaxBuf},response={Response.ToHexString()},qop={GetQOPString(QOP)}";

            public string NonceCountString()
                => NonceCount.ToString("X").PadLeft(8, '0');
        }
    }
}
