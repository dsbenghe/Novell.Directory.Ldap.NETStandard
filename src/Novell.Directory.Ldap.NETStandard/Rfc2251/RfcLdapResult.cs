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
using System.IO;

namespace Novell.Directory.Ldap.Rfc2251
{
    /// <summary>
    ///     Represents an LdapResult.
    ///     <pre>
    ///         LdapResult ::= SEQUENCE {
    ///         resultCode      ENUMERATED {
    ///         success                      (0),
    ///         operationsError              (1),
    ///         protocolError                (2),
    ///         timeLimitExceeded            (3),
    ///         sizeLimitExceeded            (4),
    ///         compareFalse                 (5),
    ///         compareTrue                  (6),
    ///         authMethodNotSupported       (7),
    ///         strongAuthRequired           (8),
    ///         -- 9 reserved --
    ///         referral                     (10),  -- new
    ///         adminLimitExceeded           (11),  -- new
    ///         unavailableCriticalExtension (12),  -- new
    ///         confidentialityRequired      (13),  -- new
    ///         saslBindInProgress           (14),  -- new
    ///         noSuchAttribute              (16),
    ///         undefinedAttributeType       (17),
    ///         inappropriateMatching        (18),
    ///         constraintViolation          (19),
    ///         attributeOrValueExists       (20),
    ///         invalidAttributeSyntax       (21),
    ///         -- 22-31 unused --
    ///         noSuchObject                 (32),
    ///         aliasProblem                 (33),
    ///         invalidDNSyntax              (34),
    ///         -- 35 reserved for undefined isLeaf --
    ///         aliasDereferencingProblem    (36),
    ///         -- 37-47 unused --
    ///         inappropriateAuthentication  (48),
    ///         invalidCredentials           (49),
    ///         insufficientAccessRights     (50),
    ///         busy                         (51),
    ///         unavailable                  (52),
    ///         unwillingToPerform           (53),
    ///         loopDetect                   (54),
    ///         -- 55-63 unused --
    ///         namingViolation              (64),
    ///         objectClassViolation         (65),
    ///         notAllowedOnNonLeaf          (66),
    ///         notAllowedOnRDN              (67),
    ///         entryAlreadyExists           (68),
    ///         objectClassModsProhibited    (69),
    ///         -- 70 reserved for CLdap --
    ///         affectsMultipleDSAs          (71), -- new
    ///         -- 72-79 unused --
    ///         other                        (80) },
    ///         -- 81-90 reserved for APIs --
    ///         matchedDN       LdapDN,
    ///         errorMessage    LdapString,
    ///         referral        [3] Referral OPTIONAL }
    ///     </pre>
    /// </summary>
    public class RfcLdapResult : Asn1Sequence, IRfcResponse
    {
        /// <summary> Context-specific TAG for optional Referral.</summary>
        public const int Referral = 3;

        // *************************************************************************
        // Constructors for RfcLdapResult
        // *************************************************************************

        /// <summary>
        ///     Constructs an RfcLdapResult from parameters.
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation.
        /// </param>
        /// <param name="matchedDn">
        ///     the matched DN returned from the server.
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server.
        /// </param>
        public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDn matchedDn, RfcLdapString errorMessage)
            : this(resultCode, matchedDn, errorMessage, null)
        {
        }

        /// <summary>
        ///     Constructs an RfcLdapResult from parameters.
        /// </summary>
        /// <param name="resultCode">
        ///     the result code of the operation.
        /// </param>
        /// <param name="matchedDn">
        ///     the matched DN returned from the server.
        /// </param>
        /// <param name="errorMessage">
        ///     the diagnostic message returned from the server.
        /// </param>
        /// <param name="referral">
        ///     the referral(s) returned by the server.
        /// </param>
        public RfcLdapResult(Asn1Enumerated resultCode, RfcLdapDn matchedDn, RfcLdapString errorMessage,
            RfcReferral referral)
            : base(4)
        {
            Add(resultCode);
            Add(matchedDn);
            Add(errorMessage);
            if (referral != null)
            {
                Add(referral);
            }
        }

        /// <summary> Constructs an RfcLdapResult from the inputstream.</summary>
        public RfcLdapResult(IAsn1Decoder dec, Stream inRenamed, int len)
            : base(dec, inRenamed, len)
        {
            // Decode optional referral from Asn1OctetString to Referral.
            if (Size() > 3)
            {
                var obj = (Asn1Tagged)get_Renamed(3);
                var id = obj.GetIdentifier();
                if (id.Tag == Referral)
                {
                    var content = ((Asn1OctetString)obj.TaggedValue).ByteValue();
                    var bais = new MemoryStream(content);
                    set_Renamed(3, new RfcReferral(dec, bais, content.Length));
                }
            }
        }

        // *************************************************************************
        // Accessors
        // *************************************************************************

        /// <summary>
        ///     Returns the result code from the server.
        /// </summary>
        /// <returns>
        ///     the result code.
        /// </returns>
        public Asn1Enumerated GetResultCode()
        {
            return (Asn1Enumerated)get_Renamed(0);
        }

        /// <summary>
        ///     Returns the matched DN from the server.
        /// </summary>
        /// <returns>
        ///     the matched DN.
        /// </returns>
        public RfcLdapDn GetMatchedDn()
        {
            return new RfcLdapDn(((Asn1OctetString)get_Renamed(1)).ByteValue());
        }

        /// <summary>
        ///     Returns the error message from the server.
        /// </summary>
        /// <returns>
        ///     the server error message.
        /// </returns>
        public RfcLdapString GetErrorMessage()
        {
            return new RfcLdapString(((Asn1OctetString)get_Renamed(2)).ByteValue());
        }

        /// <summary>
        ///     Returns the referral(s) from the server.
        /// </summary>
        /// <returns>
        ///     the referral(s).
        /// </returns>
        public RfcReferral GetReferral()
        {
            return Size() > 3 ? (RfcReferral)get_Renamed(3) : null;
        }
    }
}
