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

using System.Collections.Generic;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This class contains strings that may be associated with Exceptions generated
    ///     by the Ldap API libraries.
    ///     Two entries are made for each message, a String identifier, and the
    ///     actual error string.  Parameters are identified as {0}, {1}, etc.
    /// </summary>
    public class ExceptionMessages // : System.Resources.ResourceManager
    {
        // static strings to aide lookup and guarantee accuracy:
        // DO NOT include these strings in other Locales
        public const string Tostring = "TOSTRING";

        public const string ServerMsg = "SERVER_MSG";
        public const string MatchedDn = "MATCHED_DN";
        public const string FailedReferral = "FAILED_REFERRAL";
        public const string ReferralItem = "REFERRAL_ITEM";
        public const string ConnectionError = "CONNECTION_ERROR";
        public const string ConnectionImpossible = "CONNECTION_IMPOSSIBLE";
        public const string ConnectionWait = "CONNECTION_WAIT";
        public const string ConnectionFinalized = "CONNECTION_FINALIZED";
        public const string ConnectionClosed = "CONNECTION_CLOSED";
        public const string ConnectionReader = "CONNECTION_READER";
        public const string DupError = "DUP_ERROR";
        public const string ReferralError = "REFERRAL_ERROR";
        public const string ReferralLocal = "REFERRAL_LOCAL";
        public const string ReferenceError = "REFERENCE_ERROR";
        public const string ReferralSend = "REFERRAL_SEND";
        public const string ReferenceNofollow = "REFERENCE_NOFOLLOW";
        public const string ReferralBind = "REFERRAL_BIND";
        public const string ReferralBindMatch = "REFERRAL_BIND_MATCH";
        public const string NoDupRequest = "NO_DUP_REQUEST";
        public const string ServerConnectError = "SERVER_CONNECT_ERROR";
        public const string NoSupProperty = "NO_SUP_PROPERTY";
        public const string EntryParamError = "ENTRY_PARAM_ERROR";
        public const string DnParamError = "DN_PARAM_ERROR";
        public const string RdnParamError = "RDN_PARAM_ERROR";
        public const string OpParamError = "OP_PARAM_ERROR";
        public const string ParamError = "PARAM_ERROR";
        public const string DecodingError = "DECODING_ERROR";
        public const string EncodingError = "ENCODING_ERROR";
        public const string IoException = "IO_EXCEPTION";
        public const string InvalidEscape = "INVALID_ESCAPE";
        public const string ShortEscape = "SHORT_ESCAPE";
        public const string InvalidCharInFilter = "INVALID_CHAR_IN_FILTER";
        public const string InvalidCharInDescr = "INVALID_CHAR_IN_DESCR";
        public const string InvalidEscInDescr = "INVALID_ESC_IN_DESCR";
        public const string UnexpectedEnd = "UNEXPECTED_END";
        public const string MissingLeftParen = "MISSING_LEFT_PAREN";
        public const string MissingRightParen = "MISSING_RIGHT_PAREN";
        public const string ExpectingRightParen = "EXPECTING_RIGHT_PAREN";
        public const string ExpectingLeftParen = "EXPECTING_LEFT_PAREN";
        public const string NoOption = "NO_OPTION";
        public const string InvalidFilterComparison = "INVALID_FILTER_COMPARISON";
        public const string NoMatchingRule = "NO_MATCHING_RULE";
        public const string NoAttributeName = "NO_ATTRIBUTE_NAME";
        public const string NoDnNorMatchingRule = "NO_DN_NOR_MATCHING_RULE";
        public const string NotAnAttribute = "NOT_AN_ATTRIBUTE";
        public const string UnequalLengths = "UNEQUAL_LENGTHS";
        public const string ImproperReferral = "IMPROPER_REFERRAL";
        public const string NotImplemented = "NOT_IMPLEMENTED";
        public const string NoMemory = "NO_MEMORY";
        public const string ServerShutdownReq = "SERVER_SHUTDOWN_REQ";
        public const string InvalidAddress = "INVALID_ADDRESS";
        public const string UnknownResult = "UNKNOWN_RESULT";
        public const string OutstandingOperations = "OUTSTANDING_OPERATIONS";
        public const string WrongFactory = "WRONG_FACTORY";
        public const string NoTlsFactory = "NO_TLS_FACTORY";
        public const string NoStarttls = "NO_STARTTLS";
        public const string StoptlsError = "STOPTLS_ERROR";
        public const string MultipleSchema = "MULTIPLE_SCHEMA";
        public const string NoSchema = "NO_SCHEMA";
        public const string ReadMultiple = "READ_MULTIPLE";
        public const string CannotBind = "CANNOT_BIND";
        public const string SslProviderMissing = "SSL_PROVIDER_MISSING";

        internal static readonly Dictionary<string, string> MessageMap = new Dictionary<string, string>
        {
            { Tostring, "{0}: {1} ({2}) {3}" },
            { ServerMsg, "{0}: Server Message: {1}" },
            { MatchedDn, "{0}: Matched DN: {1}" },
            { FailedReferral, "{0}: Failed Referral: {1}" },
            { ReferralItem, "{0}: Referral: {1}" },
            { ConnectionError, "Unable to connect to server {0}:{1}" },
            { ConnectionImpossible, "Unable to reconnect to server, application has never called connect()" },
            { ConnectionWait, "Connection lost waiting for results from {0}:{1}" },
            { ConnectionFinalized, "Connection closed by the application finalizing the object" },
            { ConnectionClosed, "Connection closed by the application disconnecting" },
            { ConnectionReader, "Reader thread terminated" },
            { DupError, "RfcLdapMessage: Cannot duplicate message built from the input stream" },
            { ReferenceError, "Error attempting to follow a search continuation reference" },
            { ReferralError, "Error attempting to follow a referral" },
            { ReferralLocal, "LdapSearchResults.{0}(): No entry found & request is not complete" },
            { ReferralSend, "Error sending request to referred server" },
            { ReferenceNofollow, "Search result reference received, and referral following is off" },
            { ReferralBind, "LdapBind.bind() function returned null" },
            { ReferralBindMatch, "Could not match LdapBind.bind() connection with Server Referral URL list" },
            { NoDupRequest, "Cannot duplicate message to follow referral for {0} request, not allowed" },
            { ServerConnectError, "Error connecting to server {0} while attempting to follow a referral" },
            { NoSupProperty, "Requested property is not supported." },
            { EntryParamError, "Invalid Entry parameter" },
            { DnParamError, "Invalid DN parameter" },
            { RdnParamError, "Invalid DN or RDN parameter" },
            { OpParamError, "Invalid extended operation parameter, no OID specified" },
            { ParamError, "Invalid parameter" },
            { DecodingError, "Error Decoding responseValue" },
            { EncodingError, "Encoding Error" },
            { IoException, "I/O Exception on host {0}, port {1}" },
            { InvalidEscape, "Invalid value in escape sequence \"{0}\"" },
            { ShortEscape, "Incomplete escape sequence" },
            { UnexpectedEnd, "Unexpected end of filter" },
            { MissingLeftParen, "Unmatched parentheses, left parenthesis missing" },
            { NoOption, "Semicolon present, but no option specified" },
            { MissingRightParen, "Unmatched parentheses, right parenthesis missing" },
            { ExpectingRightParen, "Expecting right parenthesis, found \"{0}\"" },
            { ExpectingLeftParen, "Expecting left parenthesis, found \"{0}\"" },
            { NoAttributeName, "Missing attribute description" },
            { NoDnNorMatchingRule, "DN and matching rule not specified" },
            { NoMatchingRule, "Missing matching rule" },
            { InvalidFilterComparison, "Invalid comparison operator" },
            { InvalidCharInFilter, "The invalid character \"{0}\" needs to be escaped as \"{1}\"" },
            { InvalidEscInDescr, "Escape sequence not allowed in attribute description" },
            { InvalidCharInDescr, "Invalid character \"{0}\" in attribute description" },
            { NotAnAttribute, "Schema element is not an LdapAttributeSchema object" },
            { UnequalLengths, "Length of attribute Name array does not equal length of Flags array" },
            { ImproperReferral, "Referral not supported for command {0}" },
            { NotImplemented, "Method LdapConnection.startTLS not implemented" },
            { NoMemory, "All results could not be stored in memory, sort failed" },
            { ServerShutdownReq, "Received unsolicited notification from server {0}:{1} to shutdown" },
            { InvalidAddress, "Invalid syntax for address with port; {0}" },
            { UnknownResult, "Unknown Ldap result code {0}" },
            {
                OutstandingOperations,
                "Cannot start or stop TLS because outstanding Ldap operations exist on this connection"
            },
            {
                WrongFactory,
                "StartTLS cannot use the set socket factory because it does not implement LdapTLSSocketFactory"
            },
            { NoTlsFactory, "StartTLS failed because no LdapTLSSocketFactory has been set for this Connection" },
            { NoStarttls, "An attempt to stopTLS on a connection where startTLS had not been called" },
            { StoptlsError, "Error stopping TLS: Error getting input & output streams from the original socket" },
            { MultipleSchema, "Multiple schema found when reading the subschemaSubentry for {0}" },
            { NoSchema, "No schema found when reading the subschemaSubentry for {0}" },
            { ReadMultiple, "Read response is ambiguous, multiple entries returned" },
            { CannotBind, "Cannot bind. Use PoolManager.getBoundConnection()" },
            { SslProviderMissing, "Please ensure that SSL Provider is properly installed." },
        };

        public static string GetErrorMessage(string code)
        {
            if (MessageMap.ContainsKey(code))
            {
                return MessageMap[code];
            }

            return code;
        }
    } // End ExceptionMessages
}
