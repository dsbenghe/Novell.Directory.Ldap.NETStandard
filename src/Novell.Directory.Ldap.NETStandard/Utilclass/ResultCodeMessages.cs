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

//
// Novell.Directory.Ldap.Utilclass.ResultCodeMessages.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Novell.Directory.Ldap.Utilclass
{
    /// <summary>
    ///     This class contains strings corresponding to Ldap Result Codes.
    ///     The resources are accessed by the String representation of the result code.
    /// </summary>
    public class ResultCodeMessages // :System.Resources.ResourceManager
    {
        private static readonly IReadOnlyDictionary<LdapResultCode, string> ErrorCodes = new ReadOnlyDictionary<LdapResultCode, string>(new Dictionary<LdapResultCode, string>
        {
            {LdapResultCode.Success, "Success" },
            {LdapResultCode.OperationsError, "Operations Error" },
            {LdapResultCode.ProtocolError, "Protocol Error" },
            {LdapResultCode.TimeLimitExceeded, "Timelimit Exceeded" },
            {LdapResultCode.SizeLimitExceeded, "Sizelimit Exceeded" },
            {LdapResultCode.CompareFalse, "Compare False" },
            {LdapResultCode.CompareTrue, "Compare True" },
            {LdapResultCode.AuthMethodNotSupported, "Authentication Method Not Supported" },
            {LdapResultCode.StrongAuthRequired, "Strong Authentication Required" },
            {LdapResultCode.LdapPartialResults, "Partial Results" },
            {LdapResultCode.Referral, "Referral" },
            {LdapResultCode.AdminLimitExceeded, "Administrative Limit Exceeded" },
            {LdapResultCode.UnavailableCriticalExtension, "Unavailable Critical Extension" },
            {LdapResultCode.ConfidentialityRequired, "Confidentiality Required" },
            {LdapResultCode.SaslBindInProgress, "SASL Bind In Progress" },
            {LdapResultCode.NoSuchAttribute, "No Such Attribute" },
            {LdapResultCode.UndefinedAttributeType, "Undefined Attribute Type" },
            {LdapResultCode.InappropriateMatching, "Inappropriate Matching" },
            {LdapResultCode.ConstraintViolation, "Constraint Violation" },
            {LdapResultCode.AttributeOrValueExists, "Attribute Or Value Exists" },
            {LdapResultCode.InvalidAttributeSyntax, "Invalid Attribute Syntax" },
            {LdapResultCode.NoSuchObject, "No Such Object" },
            {LdapResultCode.AliasProblem, "Alias Problem" },
            {LdapResultCode.InvalidDNSyntax, "Invalid DN Syntax" },
            {LdapResultCode.IsLeaf, "Is Leaf" },
            {LdapResultCode.AliasDereferencingProblem, "Alias Dereferencing Problem" },
            {LdapResultCode.InappropriateAuthentication, "Inappropriate Authentication" },
            {LdapResultCode.InvalidCredentials, "Invalid Credentials" },
            {LdapResultCode.InsufficientAccessRights, "Insufficient Access Rights" },
            {LdapResultCode.Busy, "Busy" },
            {LdapResultCode.Unavailable, "Unavailable" },
            {LdapResultCode.UnwillingToPerform, "Unwilling To Perform" },
            {LdapResultCode.LoopDetect, "Loop Detect" },
            {LdapResultCode.NamingViolation, "Naming Violation" },
            {LdapResultCode.ObjectClassViolation, "Object Class Violation" },
            {LdapResultCode.NotAllowedOnNonLeaf, "Not Allowed On Non-leaf" },
            {LdapResultCode.NotAllowedOnRDN, "Not Allowed On RDN" },
            {LdapResultCode.EntryAlreadyExists, "Entry Already Exists" },
            {LdapResultCode.ObjectClassModsProhibited, "Object Class Modifications Prohibited" },
            {LdapResultCode.AffectsMultipleDsas, "Affects Multiple DSAs" },
            {LdapResultCode.Other, "Other" },
            {LdapResultCode.ServerDown, "Server Down" },
            {LdapResultCode.LocalError, "Local Error" },
            {LdapResultCode.EncodingError, "Encoding Error" },
            {LdapResultCode.DecodingError, "Decoding Error" },
            {LdapResultCode.LdapTimeout, "Ldap Timeout" },
            {LdapResultCode.AuthUnknown, "Authentication Unknown" },
            {LdapResultCode.FilterError, "Filter Error" },
            {LdapResultCode.UserCancelled, "User Cancelled" },
            {LdapResultCode.ParameterError, "Parameter Error" },
            {LdapResultCode.NoMemory, "No Memory" },
            {LdapResultCode.ConnectError, "Connect Error" },
            {LdapResultCode.NotSupported, "Ldap Not Supported" },
            {LdapResultCode.ControlNotFound, "Control Not Found" },
            {LdapResultCode.NoResultsReturned, "No Results Returned" },
            {LdapResultCode.MoreResultsToReturn, "More Results To Return" },
            {LdapResultCode.ClientLoop, "Client Loop" },
            {LdapResultCode.ReferralLimitExceeded, "Referral Limit Exceeded" },
            {LdapResultCode.TlsNotSupported, "TLS not supported" },
            {LdapResultCode.SslHandshakeFailed, "SSL handshake failed" },
            {LdapResultCode.SslProviderNotFound, "SSL Provider not found" }
        });

        public static string GetResultCode(LdapResultCode code)
        {
            return ErrorCodes[code];
        }
    } // End ResultCodeMessages
}