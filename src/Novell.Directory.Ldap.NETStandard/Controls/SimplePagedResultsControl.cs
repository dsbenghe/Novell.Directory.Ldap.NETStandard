using JetBrains.Annotations;
using Novell.Directory.Ldap.Asn1;
using System;

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    /// An LDAP client application that needs to control the rate at which results are returned
    /// MAY specify on the searchRequest a <see cref="SimplePagedResultsControl"/> with size set
    /// to the desired page size and cookie set to the zero-length string. The page size specified
    /// MAY be greater than zero and less than the sizeLimit value specified in the
    /// searchRequest. [RFC 2696].
    /// </summary>
    public class SimplePagedResultsControl : LdapControl
    {
        private const string RequestOid = "1.2.840.113556.1.4.319";
        private const string DecodedNotInteger = "Decoded value is not an integer, but should be";
        private const string DecodedNotOctetString = "Decoded value is not an octet string, but should be";

        private static readonly string DecodedNotSequence = $"Failed to construct {nameof(SimplePagedResultsControl)}: " +
                                                            $"provided values might not be decoded as {nameof(Asn1Sequence)}";
        private Asn1Sequence _request;

        static SimplePagedResultsControl()
        {
            try
            {
                Register(RequestOid, typeof(SimplePagedResultsControl));
            }
            catch (Exception ex)
            {
                Logger.Log.LogWarning($"Failed to bind oid <{RequestOid}> to control <{nameof(SimplePagedResultsControl)}>", ex);
            }
        }

        /// <summary>
        /// Constructs <see cref="SimplePagedResultsControl"/> control.
        /// </summary>
        /// <param name="size"><see cref="SimplePagedResultsControl.Size"/>.</param>
        /// <param name="cookie"><see cref="SimplePagedResultsControl.Cookie"/>.</param>
        public SimplePagedResultsControl(int size, [CanBeNull] byte[] cookie)
            : base(RequestOid, true, null)
        {
            Size = size;
            Cookie = cookie ?? GetEmptyCookie;
            BuildTypedPagedRequest();

            // ReSharper disable once VirtualMemberCallInConstructor
            SetValue(_request.GetEncoding(new LberEncoder()));
        }

        [UsedImplicitly]
        public SimplePagedResultsControl(string oid, bool critical, byte[] values)
            : base(oid, critical, values)
        {
            var lberDecoder = new LberDecoder();
            if (lberDecoder == null)
            {
                throw new InvalidOperationException($"Failed to build {nameof(LberDecoder)}");
            }

            var asn1Object = lberDecoder.Decode(values);
            if (!(asn1Object is Asn1Sequence))
            {
                throw new InvalidCastException(DecodedNotSequence);
            }

            var size = ((Asn1Structured)asn1Object).get_Renamed(0);
            if (!(size is Asn1Integer integerSize))
            {
                throw new InvalidOperationException(DecodedNotInteger);
            }

            Size = integerSize.IntValue();

            var cookie = ((Asn1Structured)asn1Object).get_Renamed(1);
            if (!(cookie is Asn1OctetString octetCookie))
            {
                throw new InvalidOperationException(DecodedNotOctetString);
            }

            Cookie = octetCookie.ByteValue();
        }

        /// <summary>
        /// REQUEST: An LDAP client application that needs to control the rate at which
        /// results are returned MAY specify on the searchRequest a
        /// pagedResultsControl with size set to the desired page siz
        ///
        /// RESPONSE: Each time the server returns a set of results to the client when
        /// processing a search request containing the pagedResultsControl, the
        /// server includes the pagedResultsControl control in the
        /// searchResultDone message. In the control returned to the client, the
        /// size MAY be set to the server’s estimate of the total number of
        /// entries in the entire result set. Servers that cannot provide such an
        /// estimate MAY set this size to zero (0).
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// INITIAL REQUEST: empty cookie
        ///
        /// CONSEQUENT REQUEST: cookie from previous response
        ///
        /// RESPONSE: The cookie MUST be set to an empty value if there are no more
        /// entries to return (i.e., the page of search results returned was the last),
        /// or, if there are more entries to return, to an octet string of the server’s
        /// choosing, used to resume the search.
        /// </summary>
        public byte[] Cookie { get; }

        public bool IsEmptyCookie() => Cookie == null || Cookie.Length == 0;

        public static byte[] GetEmptyCookie => Array.Empty<byte>();

        private void BuildTypedPagedRequest()
        {
            _request = new Asn1Sequence(2);
            _request.Add(new Asn1Integer(Size));
            _request.Add(new Asn1OctetString(Cookie));
        }
    }
}
