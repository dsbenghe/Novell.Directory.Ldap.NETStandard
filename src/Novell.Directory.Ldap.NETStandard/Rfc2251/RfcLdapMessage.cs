/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

using Novell.Directory.Ldap.Asn1;
using System;
using System.IO;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an Ldap Message.
    ///     <pre>
    ///         LdapMessage ::= SEQUENCE {
    ///         messageID       MessageID,
    ///         protocolOp      CHOICE {
    ///         bindRequest     BindRequest,
    ///         bindResponse    BindResponse,
    ///         unbindRequest   UnbindRequest,
    ///         searchRequest   SearchRequest,
    ///         searchResEntry  SearchResultEntry,
    ///         searchResDone   SearchResultDone,
    ///         searchResRef    SearchResultReference,
    ///         modifyRequest   ModifyRequest,
    ///         modifyResponse  ModifyResponse,
    ///         addRequest      AddRequest,
    ///         addResponse     AddResponse,
    ///         delRequest      DelRequest,
    ///         delResponse     DelResponse,
    ///         modDNRequest    ModifyDNRequest,
    ///         modDNResponse   ModifyDNResponse,
    ///         compareRequest  CompareRequest,
    ///         compareResponse CompareResponse,
    ///         abandonRequest  AbandonRequest,
    ///         extendedReq     ExtendedRequest,
    ///         extendedResp    ExtendedResponse },
    ///         controls       [0] Controls OPTIONAL }
    ///     </pre>
    ///     Note: The creation of a MessageID should be hidden within the creation of
    ///     an RfcLdapMessage. The MessageID needs to be in sequence, and has an
    ///     upper and lower limit. There is never a case when a user should be
    ///     able to specify the MessageID for an RfcLdapMessage. The MessageID()
    ///     constructor should be package protected. (So the MessageID value
    ///     isn't arbitrarily run up.).
    /// </summary>
    public class RfcLdapMessage : Asn1Sequence
    {
        private readonly Asn1Object _op;
        private RfcControls _controls;

        /// <summary>
        ///     Create an RfcLdapMessage by copying the content array.
        /// </summary>
        /// <param name="origContent">
        ///     the array list to copy.
        /// </param>
        internal RfcLdapMessage(Asn1Object[] origContent, IRfcRequest origRequest, string dn, string filter,
            bool reference)
            : base(origContent, origContent.Length)
        {
            set_Renamed(0, new RfcMessageId()); // MessageID has static counter

            var req = (IRfcRequest)origContent[1];
            var newreq = req.DupRequest(dn, filter, reference);
            _op = (Asn1Object)newreq;
            set_Renamed(1, (Asn1Object)newreq);
        }

        /// <summary> Create an RfcLdapMessage using the specified Ldap Request.</summary>
        public RfcLdapMessage(IRfcRequest op)
            : this(op, null)
        {
        }

        /// <summary> Create an RfcLdapMessage request from input parameters.</summary>
        public RfcLdapMessage(IRfcRequest op, RfcControls controls)
            : base(3)
        {
            _op = (Asn1Object)op;
            _controls = controls;

            Add(new RfcMessageId()); // MessageID has static counter
            Add((Asn1Object)op);
            if (controls != null)
            {
                Add(controls);
            }
        }

        /// <summary> Create an RfcLdapMessage using the specified Ldap Response.</summary>
        public RfcLdapMessage(Asn1Sequence op)
            : this(op, null)
        {
        }

        /// <summary> Create an RfcLdapMessage response from input parameters.</summary>
        public RfcLdapMessage(Asn1Sequence op, RfcControls controls)
            : base(3)
        {
            _op = op;
            _controls = controls;

            Add(new RfcMessageId()); // MessageID has static counter
            Add(op);
            if (controls != null)
            {
                Add(controls);
            }
        }

        /// <summary> Will decode an RfcLdapMessage directly from an InputStream.</summary>
        public RfcLdapMessage(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
            // Decode implicitly tagged protocol operation from an Asn1Tagged type
            // to its appropriate application type.
            var protocolOp = (Asn1Tagged)get_Renamed(1);
            var protocolOpId = protocolOp.GetIdentifier();
            var content = ((Asn1OctetString)protocolOp.TaggedValue).ByteValue();
            var bais = new MemoryStream(content);

            switch (protocolOpId.Tag)
            {
                case LdapMessage.SearchResponse:
                    set_Renamed(1, new RfcSearchResultEntry(dec, bais, content.Length));
                    break;

                case LdapMessage.SearchResult:
                    set_Renamed(1, new RfcSearchResultDone(dec, bais, content.Length));
                    break;

                case LdapMessage.SearchResultReference:
                    set_Renamed(1, new RfcSearchResultReference(dec, bais, content.Length));
                    break;

                case LdapMessage.AddResponse:
                    set_Renamed(1, new RfcAddResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.BindResponse:
                    set_Renamed(1, new RfcBindResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.CompareResponse:
                    set_Renamed(1, new RfcCompareResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.DelResponse:
                    set_Renamed(1, new RfcDelResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.ExtendedResponse:
                    set_Renamed(1, new RfcExtendedResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.IntermediateResponse:
                    set_Renamed(1, new RfcIntermediateResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.ModifyResponse:
                    set_Renamed(1, new RfcModifyResponse(dec, bais, content.Length));
                    break;

                case LdapMessage.ModifyRdnResponse:
                    set_Renamed(1, new RfcModifyDnResponse(dec, bais, content.Length));
                    break;

                default:
                    throw new Exception("RfcLdapMessage: Invalid tag: " + protocolOpId.Tag);
            }

            // decode optional implicitly tagged controls from Asn1Tagged type to
            // to RFC 2251 types.
            if (Size() > 2)
            {
                var controls = (Asn1Tagged)get_Renamed(2);

                // Asn1Identifier controlsId = protocolOp.getIdentifier();
                // we could check to make sure we have controls here....
                content = ((Asn1OctetString)controls.TaggedValue).ByteValue();
                bais = new MemoryStream(content);
                set_Renamed(2, new RfcControls(dec, bais, content.Length));
            }
        }

        /// <summary> Returns this RfcLdapMessage's messageID as an int.</summary>
        public int MessageId => ((Asn1Integer)get_Renamed(0)).IntValue();

        /// <summary> Returns this RfcLdapMessage's message type.</summary>
        public int Type => get_Renamed(1).GetIdentifier().Tag;

        /// <summary>
        ///     Returns the response associated with this RfcLdapMessage.
        ///     Can be one of RfcLdapResult, RfcBindResponse, RfcExtendedResponse
        ///     all which extend RfcResponse. It can also be
        ///     RfcSearchResultEntry, or RfcSearchResultReference.
        /// </summary>
        public Asn1Object Response => get_Renamed(1);

        /// <summary> Returns the optional Controls for this RfcLdapMessage.</summary>
        public RfcControls Controls
        {
            get
            {
                if (Size() > 2)
                {
                    return (RfcControls)get_Renamed(2);
                }

                return null;
            }
        }

        /// <summary> Returns the dn of the request, may be null.</summary>
        public string RequestDn => ((IRfcRequest)_op).GetRequestDn();

        /// <summary>
        ///     returns the original request in this message.
        /// </summary>
        /// <returns>
        ///     the original msg request for this response.
        /// </returns>
        /// <summary>
        ///     sets the original request in this message.
        /// </summary>
        /// <param name="msg">
        ///     the original request for this response.
        /// </param>
        public LdapMessage RequestingMessage { get; set; }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary>
        ///     Returns the request associated with this RfcLdapMessage.
        ///     Throws a class cast exception if the RfcLdapMessage is not a request.
        /// </summary>
        public IRfcRequest GetRequest()
        {
            return (IRfcRequest)get_Renamed(1);
        }

        public bool IsRequest()
        {
            return get_Renamed(1) is IRfcRequest;
        }

        /// <summary>
        ///     Duplicate this message, replacing base dn, filter, and scope if supplied.
        /// </summary>
        /// <param name="dn">
        ///     the base dn.
        /// </param>
        /// <param name="filter">
        ///     the filter.
        /// </param>
        /// <param name="reference">
        ///     true if a search reference.
        /// </param>
        /// <returns>
        ///     the object representing the new message.
        /// </returns>
        public object DupMessage(string dn, string filter, bool reference)
        {
            if (_op == null)
            {
                throw new LdapException("DUP_ERROR", LdapException.LocalError, null);
            }

            var newMsg = new RfcLdapMessage(ToArray(), (IRfcRequest)get_Renamed(1), dn, filter, reference);
            return newMsg;
        }
    }
}
