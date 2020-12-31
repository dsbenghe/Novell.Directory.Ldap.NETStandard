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

using Novell.Directory.Ldap.Rfc2251;
using System;
using System.Reflection;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     The base class for Ldap request and response messages.
    ///     Subclassed by response messages used in asynchronous operations.
    /// </summary>
    public class LdapMessage : IDebugIdentifier
    {
        public virtual DebugId DebugId { get; } = DebugId.ForType<LdapMessage>();

        /// <summary>
        ///     A bind request operation.
        ///     BIND_REQUEST = 0.
        /// </summary>
        public const int BindRequest = 0;

        /// <summary>
        ///     A bind response operation.
        ///     BIND_RESPONSE = 1.
        /// </summary>
        public const int BindResponse = 1;

        /// <summary>
        ///     An unbind request operation.
        ///     UNBIND_REQUEST = 2.
        /// </summary>
        public const int UnbindRequest = 2;

        /// <summary>
        ///     A search request operation.
        ///     SEARCH_REQUEST = 3.
        /// </summary>
        public const int SearchRequest = 3;

        /// <summary>
        ///     A search response containing data.
        ///     SEARCH_RESPONSE = 4.
        /// </summary>
        public const int SearchResponse = 4;

        /// <summary>
        ///     A search result message - contains search status.
        ///     SEARCH_RESULT = 5.
        /// </summary>
        public const int SearchResult = 5;

        /// <summary>
        ///     A modify request operation.
        ///     MODIFY_REQUEST = 6.
        /// </summary>
        public const int ModifyRequest = 6;

        /// <summary>
        ///     A modify response operation.
        ///     MODIFY_RESPONSE = 7.
        /// </summary>
        public const int ModifyResponse = 7;

        /// <summary>
        ///     An add request operation.
        ///     ADD_REQUEST = 8.
        /// </summary>
        public const int AddRequest = 8;

        /// <summary>
        ///     An add response operation.
        ///     ADD_RESONSE = 9.
        /// </summary>
        public const int AddResponse = 9;

        /// <summary>
        ///     A delete request operation.
        ///     DEL_REQUEST = 10.
        /// </summary>
        public const int DelRequest = 10;

        /// <summary>
        ///     A delete response operation.
        ///     DEL_RESONSE = 11.
        /// </summary>
        public const int DelResponse = 11;

        /// <summary>
        ///     A modify RDN request operation.
        ///     MODIFY_RDN_REQUEST = 12.
        /// </summary>
        public const int ModifyRdnRequest = 12;

        /// <summary>
        ///     A modify RDN response operation.
        ///     MODIFY_RDN_RESPONSE = 13.
        /// </summary>
        public const int ModifyRdnResponse = 13;

        /// <summary>
        ///     A compare result operation.
        ///     COMPARE_REQUEST = 14.
        /// </summary>
        public const int CompareRequest = 14;

        /// <summary>
        ///     A compare response operation.
        ///     COMPARE_RESPONSE = 15.
        /// </summary>
        public const int CompareResponse = 15;

        /// <summary>
        ///     An abandon request operation.
        ///     ABANDON_REQUEST = 16.
        /// </summary>
        public const int AbandonRequest = 16;

        /// <summary>
        ///     A search result reference operation.
        ///     SEARCH_RESULT_REFERENCE = 19.
        /// </summary>
        public const int SearchResultReference = 19;

        /// <summary>
        ///     An extended request operation.
        ///     EXTENDED_REQUEST = 23.
        /// </summary>
        public const int ExtendedRequest = 23;

        /// <summary>
        ///     An extended response operation.
        ///     EXTENDED_RESONSE = 24.
        /// </summary>
        public const int ExtendedResponse = 24;

        /// <summary>
        ///     An intermediate response operation.
        ///     INTERMEDIATE_RESONSE = 25.
        /// </summary>
        public const int IntermediateResponse = 25;

        /// <summary> Lock object to protect counter for message numbers.</summary>
        /// <summary>
        ///     Counters used to construct request message #'s, unique for each request
        ///     Will be enabled after ASN.1 conversion.
        /// </summary>
        /*
        private static int msgNum = 0; // Ldap Request counter
        */
        private int _imsgNum = -1; // This instance LdapMessage number

        private int _messageType = -1;

        /* application defined tag to identify this message */
        private string _stringTag;

        /// <summary> A request or response message for an asynchronous Ldap operation.</summary>
        protected internal RfcLdapMessage Message { get; }

        /// <summary> Dummy constructor.</summary>
        internal LdapMessage()
        {
        }

        /// <summary>
        ///     Creates an LdapMessage when sending a protocol operation and sends
        ///     some optional controls with the message.
        /// </summary>
        /// <param name="op">
        ///     The operation type of message.
        /// </param>
        /// <param name="controls">
        ///     The controls to use with the operation.
        /// </param>
        /// <seealso cref="Type">
        /// </seealso>
        /*package*/
        internal LdapMessage(int type, IRfcRequest op, LdapControl[] controls)
        {
            // Get a unique number for this request message
            _messageType = type;
            RfcControls asn1Ctrls = null;
            if (controls != null)
            {
                // Move LdapControls into an RFC 2251 Controls object.
                asn1Ctrls = new RfcControls();
                for (var i = 0; i < controls.Length; i++)
                {
                    // asn1Ctrls.add(null);
                    asn1Ctrls.Add(controls[i].Asn1Object);
                }
            }

            // create RFC 2251 LdapMessage
            Message = new RfcLdapMessage(op, asn1Ctrls);
        }

        /// <summary>
        ///     Creates an Rfc 2251 LdapMessage when the libraries receive a response
        ///     from a command.
        /// </summary>
        /// <param name="message">
        ///     A response message.
        /// </param>
        protected internal LdapMessage(RfcLdapMessage message)
        {
            Message = message;
        }

        /// <summary> Returns the LdapMessage request associated with this response.</summary>
        internal LdapMessage RequestingMessage => Message.RequestingMessage;

        /// <summary> Returns any controls in the message.</summary>
        public virtual LdapControl[] Controls
        {
            get
            {
                /*              LdapControl[] controls = null;
                                RfcControls asn1Ctrls = message.Controls;

                                if (asn1Ctrls != null)
                                {
                                    controls = new LdapControl[asn1Ctrls.size()];
                                    for (int i = 0; i < asn1Ctrls.size(); i++)
                                    {
                                        RfcControl rfcCtl = (RfcControl) asn1Ctrls.get_Renamed(i);
                                        System.String oid = rfcCtl.ControlType.stringValue();
                                        byte[] value_Renamed = rfcCtl.ControlValue.byteValue();
                                        bool critical = rfcCtl.Criticality.booleanValue();

                                        controls[i] = controlFactory(oid, critical, value_Renamed);
                                    }
                                }

                                return controls;
                */
                LdapControl[] controls = null;
                var asn1Ctrls = Message.Controls;

                // convert from RFC 2251 Controls to LDAPControl[].
                if (asn1Ctrls != null)
                {
                    controls = new LdapControl[asn1Ctrls.Size()];
                    for (var i = 0; i < asn1Ctrls.Size(); i++)
                    {
                        /*
                                                * At this point we have an RfcControl which needs to be
                                                * converted to the appropriate Response Control.  This requires
                                                * calling the constructor of a class that extends LDAPControl.
                                                * The controlFactory method searches the list of registered
                                                * controls and if a match is found calls the constructor
                                                * for that child LDAPControl. Otherwise, it returns a regular
                                                * LDAPControl object.
                                                *
                                                * Question: Why did we not call the controlFactory method when
                                                * we were parsing the control. Answer: By the time the
                                                * code realizes that we have a control it is already too late.
                                                */
                        var rfcCtl = (RfcControl)asn1Ctrls.get_Renamed(i);
                        var oid = rfcCtl.ControlType.StringValue();
                        var valueRenamed = rfcCtl.ControlValue.ByteValue();
                        var critical = rfcCtl.Criticality.BooleanValue();

                        /* Return from this call should return either an LDAPControl
                        * or a class extending LDAPControl that implements the
                        * appropriate registered response control
                        */
                        controls[i] = ControlFactory(oid, critical, valueRenamed);
                    }
                }

                return controls;
            }
        }

        /// <summary>
        ///     Returns the message ID.  The message ID is an integer value
        ///     identifying the Ldap request and its response.
        /// </summary>
        public virtual int MessageId
        {
            get
            {
                if (_imsgNum == -1)
                {
                    _imsgNum = Message.MessageId;
                }

                return _imsgNum;
            }
        }

        /// <summary>
        ///     Returns the Ldap operation type of the message.
        ///     The type is one of the following:.
        ///     <ul>
        ///         <li>BIND_REQUEST            = 0;</li>
        ///         <li>BIND_RESPONSE           = 1;</li>
        ///         <li>UNBIND_REQUEST          = 2;</li>
        ///         <li>SEARCH_REQUEST          = 3;</li>
        ///         <li>SEARCH_RESPONSE         = 4;</li>
        ///         <li>SEARCH_RESULT           = 5;</li>
        ///         <li>MODIFY_REQUEST          = 6;</li>
        ///         <li>MODIFY_RESPONSE         = 7;</li>
        ///         <li>ADD_REQUEST             = 8;</li>
        ///         <li>ADD_RESPONSE            = 9;</li>
        ///         <li>DEL_REQUEST             = 10;</li>
        ///         <li>DEL_RESPONSE            = 11;</li>
        ///         <li>MODIFY_RDN_REQUEST      = 12;</li>
        ///         <li>MODIFY_RDN_RESPONSE     = 13;</li>
        ///         <li>COMPARE_REQUEST         = 14;</li>
        ///         <li>COMPARE_RESPONSE        = 15;</li>
        ///         <li>ABANDON_REQUEST         = 16;</li>
        ///         <li>SEARCH_RESULT_REFERENCE = 19;</li>
        ///         <li>EXTENDED_REQUEST        = 23;</li>
        ///         <li>EXTENDED_RESPONSE       = 24;</li>
        ///         <li>INTERMEDIATE_RESPONSE   = 25;</li>
        ///     </ul>
        /// </summary>
        /// <returns>
        ///     The operation type of the message.
        /// </returns>
        public virtual int Type
        {
            get
            {
                if (_messageType == -1)
                {
                    _messageType = Message.Type;
                }

                return _messageType;
            }
        }

        /// <summary>
        ///     Indicates whether the message is a request or a response.
        /// </summary>
        /// <returns>
        ///     true if the message is a request, false if it is a response,
        ///     a search result, or a search result reference.
        /// </returns>
        public bool Request => Message.IsRequest();

        /// <summary> Returns the RFC 2251 LdapMessage composed in this object.</summary>
        internal RfcLdapMessage Asn1Object => Message;

        private string Name
        {
            get
            {
                string name;
                switch (Type)
                {
                    case SearchResponse:
                        name = "LdapSearchResponse";
                        break;

                    case SearchResult:
                        name = "LdapSearchResult";
                        break;

                    case SearchRequest:
                        name = "LdapSearchRequest";
                        break;

                    case ModifyRequest:
                        name = "LdapModifyRequest";
                        break;

                    case ModifyResponse:
                        name = "LdapModifyResponse";
                        break;

                    case AddRequest:
                        name = "LdapAddRequest";
                        break;

                    case AddResponse:
                        name = "LdapAddResponse";
                        break;

                    case DelRequest:
                        name = "LdapDelRequest";
                        break;

                    case DelResponse:
                        name = "LdapDelResponse";
                        break;

                    case ModifyRdnRequest:
                        name = "LdapModifyRDNRequest";
                        break;

                    case ModifyRdnResponse:
                        name = "LdapModifyRDNResponse";
                        break;

                    case CompareRequest:
                        name = "LdapCompareRequest";
                        break;

                    case CompareResponse:
                        name = "LdapCompareResponse";
                        break;

                    case BindRequest:
                        name = "LdapBindRequest";
                        break;

                    case BindResponse:
                        name = "LdapBindResponse";
                        break;

                    case UnbindRequest:
                        name = "LdapUnbindRequest";
                        break;

                    case AbandonRequest:
                        name = "LdapAbandonRequest";
                        break;

                    case SearchResultReference:
                        name = "LdapSearchResultReference";
                        break;

                    case ExtendedRequest:
                        name = "LdapExtendedRequest";
                        break;

                    case ExtendedResponse:
                        name = "LdapExtendedResponse";
                        break;

                    case IntermediateResponse:
                        name = "LdapIntermediateResponse";
                        break;

                    default:
                        throw new Exception("LdapMessage: Unknown Type " + Type);
                }

                return name;
            }
        }

        /// <summary>
        ///     Retrieves the identifier tag for this message.
        ///     An identifier can be associated with a message with the.
        ///     <code>setTag</code> method.
        ///     Tags are set by the application and not by the API or the server.
        ///     If a server response. <code>isRequest() == false</code> has no tag,
        ///     the tag associated with the corresponding server request is used.
        /// </summary>
        /// <returns>
        ///     the identifier associated with this message or. <code>null</code>
        ///     if none.
        /// </returns>
        /// <summary>
        ///     Sets a string identifier tag for this message.
        ///     This method allows an API to set a tag and later identify messages
        ///     by retrieving the tag associated with the message.
        ///     Tags are set by the application and not by the API or the server.
        ///     Message tags are not included with any message sent to or received
        ///     from the server.
        ///     Tags set on a request to the server
        ///     are automatically associated with the response messages when they are
        ///     received by the API and transferred to the application.
        ///     The application can explicitly set a different value in a
        ///     response message.
        ///     To set a value in a server request, for example an
        ///     {@link LdapSearchRequest}, you must create the object,
        ///     set the tag, and use the
        ///     {@link LdapConnection.SendRequest LdapConnection.sendRequest()}
        ///     method to send it to the server.
        /// </summary>
        /// <param name="stringTag">
        ///     the String assigned to identify this message.
        /// </param>
        public string Tag
        {
            get
            {
                if (_stringTag != null)
                {
                    return _stringTag;
                }

                if (Request)
                {
                    return null;
                }

                var m = RequestingMessage;
                if (m == null)
                {
                    return null;
                }

                return m._stringTag;
            }

            set => _stringTag = value;
        }

        /// <summary>
        ///     Returns a mutated clone of this LdapMessage,
        ///     replacing base dn, filter.
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
        internal LdapMessage Clone(string dn, string filter, bool reference)
        {
            return new LdapMessage((RfcLdapMessage)Message.DupMessage(dn, filter, reference));
        }

        /// <summary>
        ///     Instantiates an LdapControl.  We search through our list of
        ///     registered controls.  If we find a matchiing OID we instantiate
        ///     that control by calling its contructor.  Otherwise we default to
        ///     returning a regular LdapControl object.
        /// </summary>
        private LdapControl ControlFactory(string oid, bool critical, byte[] valueRenamed)
        {
            // throw new NotImplementedException();
            var regControls = LdapControl.RegisteredControls;
            try
            {
                /*
                * search through the registered extension list to find the
                * response control class
                */
                var respCtlClass = regControls.FindResponseControl(oid);

                // Did not find a match so return default LDAPControl
                if (respCtlClass == null)
                {
                    return new LdapControl(oid, critical, valueRenamed);
                }

                /* If found, get LDAPControl constructor */
                Type[] argsClass = { typeof(string), typeof(bool), typeof(byte[]) };
                object[] args = { oid, critical, valueRenamed };
                Exception ex = null;
                try
                {
                    var ctlConstructor = respCtlClass.GetConstructor(argsClass);

                    try
                    {
                        /* Call the control constructor for a registered Class*/
                        object ctl = null;

                        // ctl = ctlConstructor.newInstance(args);
                        ctl = ctlConstructor.Invoke(args);
                        return (LdapControl)ctl;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        ex = e;
                    }
                    catch (TargetInvocationException e)
                    {
                        ex = e;
                    }
                    catch (Exception e)
                    {
                        // Could not create the ResponseControl object
                        // All possible exceptions are ignored. We fall through
                        // and create a default LDAPControl object
                        ex = e;
                    }
                }
                catch (MethodAccessException e)
                {
                    // bad class was specified, fall through and return a
                    // default LDAPControl object
                    ex = e;
                }
            }
            catch (FieldAccessException ex)
            {
                // No match with the OID
                // Do nothing. Fall through and construct a default LDAPControl object.
                Logger.Log.LogWarning("Exception swallowed", ex);
            }

            // If we get here we did not have a registered response control
            // for this oid.  Return a default LDAPControl object.
            return new LdapControl(oid, critical, valueRenamed);
        }

        /// <summary>
        ///     Creates a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representation for this LdapMessage.
        /// </returns>
        public override string ToString()
        {
            return Name + "(" + MessageId + "): " + Message;
        }
    }
}
