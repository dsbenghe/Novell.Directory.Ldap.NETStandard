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
// Novell.Directory.Ldap.Events.Edir.EdirEventIntermediateResponse.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//
using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Events.Edir.EventData;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Events.Edir
{
    /// <summary>
    ///     This class represents the intermediate response corresponding to edir events.
    /// </summary>
    public class EdirEventIntermediateResponse : LdapIntermediateResponse
    {
        /// <summary>
        ///     Type of Edir event.
        /// </summary>
        public EdirEventType EventType { get; protected set; }
        /// <summary>
        ///     Type of Edir event result.
        /// </summary>
        public EdirEventResultType EventResultType { get; protected set; }
        /// <summary>
        ///     The response data object associated with Edir event.
        /// </summary>
        public BaseEdirEventData EventResponseDataObject { get; protected set; }

        public EdirEventIntermediateResponse(RfcLdapMessage message)
            : base(message)
        {
            ProcessMessage(Value);
        }

        public EdirEventIntermediateResponse(byte[] message)
            : base(new RfcLdapMessage(new Asn1Sequence()))
        {
            ProcessMessage(message);
        }

        protected void ProcessMessage(byte[] returnedValue)
        {
            var decoder = new LBERDecoder();
            var sequence = decoder.Decode(returnedValue) as Asn1Sequence;

            EventType = (EdirEventType)(sequence[0] as Asn1Integer).IntValue;
            EventResultType = (EdirEventResultType)(sequence[1] as Asn1Integer).IntValue;

            if (sequence.Count > 2)
            {
                var objTagged = sequence[2] as Asn1Tagged;

                switch ((EdirEventDataType)objTagged.Identifier.Tag)
                {
                    case EdirEventDataType.EDIR_TAG_ENTRY_EVENT_DATA:
                        EventResponseDataObject = new EntryEventData(EdirEventDataType.EDIR_TAG_ENTRY_EVENT_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_VALUE_EVENT_DATA:
                        EventResponseDataObject = new ValueEventData(EdirEventDataType.EDIR_TAG_VALUE_EVENT_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_DEBUG_EVENT_DATA:
                        EventResponseDataObject = new DebugEventData(EdirEventDataType.EDIR_TAG_DEBUG_EVENT_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_GENERAL_EVENT_DATA:
                        EventResponseDataObject = new GeneralDSEventData(EdirEventDataType.EDIR_TAG_GENERAL_EVENT_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_SKULK_DATA:
                        EventResponseDataObject = null;
                        break;

                    case EdirEventDataType.EDIR_TAG_BINDERY_EVENT_DATA:
                        EventResponseDataObject = new BinderyObjectEventData(EdirEventDataType.EDIR_TAG_BINDERY_EVENT_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_DSESEV_INFO:
                        EventResponseDataObject = new SecurityEquivalenceEventData(EdirEventDataType.EDIR_TAG_DSESEV_INFO,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_MODULE_STATE_DATA:
                        EventResponseDataObject = new ModuleStateEventData(EdirEventDataType.EDIR_TAG_MODULE_STATE_DATA,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_NETWORK_ADDRESS:
                        EventResponseDataObject = new NetworkAddressEventData(EdirEventDataType.EDIR_TAG_NETWORK_ADDRESS,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_CONNECTION_STATE:
                        EventResponseDataObject = new ConnectionStateEventData(EdirEventDataType.EDIR_TAG_CONNECTION_STATE,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EDIR_TAG_CHANGE_SERVER_ADDRESS:
                        EventResponseDataObject =
                            new ChangeAddressEventData(EdirEventDataType.EDIR_TAG_CHANGE_SERVER_ADDRESS,
                                objTagged.TaggedValue);
                        break;

                    /*
                          case EdirEventDataType.EDIR_TAG_CHANGE_CONFIG_PARAM :
                              responsedata =
                                  new ChangeConfigEventData(
                                      taggedobject.taggedValue());
              
                              break;
              
                          case EdirEventDataType.EDIR_TAG_STATUS_LOG :
                              responsedata =
                                  new StatusLogEventData(taggedobject.taggedValue());
              
                              break;
                    */
                    case EdirEventDataType.EDIR_TAG_NO_DATA:
                        EventResponseDataObject = null;
                        break;

                    default:
                        //unhandled data.
                        throw new IOException();
                }
            }
            else
            {
                //NO DATA
                EventResponseDataObject = null;
            }
        }
    }
}