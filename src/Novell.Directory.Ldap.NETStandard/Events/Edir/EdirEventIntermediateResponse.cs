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
        public EdirEventIntermediateResponse(RfcLdapMessage message)
            : base(message)
        {
            ProcessMessage(GetValue());
        }

        public EdirEventIntermediateResponse(byte[] message)
            : base(new RfcLdapMessage(new Asn1Sequence()))
        {
            ProcessMessage(SupportClass.ToSByteArray(message));
        }

        /// <summary>
        ///     Type of Edir event.
        /// </summary>
        public EdirEventType EventType { get; private set; }

        /// <summary>
        ///     Type of Edir event result.
        /// </summary>
        public EdirEventResultType EventResultType { get; private set; }

        /// <summary>
        ///     The response data object associated with Edir event.
        /// </summary>
        public BaseEdirEventData EventResponseDataObject { get; private set; }

        private void ProcessMessage(sbyte[] returnedValue)
        {
            var decoder = new LberDecoder();
            var sequence = (Asn1Sequence)decoder.Decode(returnedValue);

            EventType = (EdirEventType)((Asn1Integer)sequence.get_Renamed(0)).IntValue();
            EventResultType = (EdirEventResultType)((Asn1Integer)sequence.get_Renamed(1)).IntValue();

            if (sequence.Size() > 2)
            {
                var objTagged = (Asn1Tagged)sequence.get_Renamed(2);

                switch ((EdirEventDataType)objTagged.GetIdentifier().Tag)
                {
                    case EdirEventDataType.EdirTagEntryEventData:
                        EventResponseDataObject = new EntryEventData(
                            EdirEventDataType.EdirTagEntryEventData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagValueEventData:
                        EventResponseDataObject = new ValueEventData(
                            EdirEventDataType.EdirTagValueEventData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagDebugEventData:
                        EventResponseDataObject = new DebugEventData(
                            EdirEventDataType.EdirTagDebugEventData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagGeneralEventData:
                        EventResponseDataObject = new GeneralDsEventData(
                            EdirEventDataType.EdirTagGeneralEventData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagSkulkData:
                        EventResponseDataObject = null;
                        break;

                    case EdirEventDataType.EdirTagBinderyEventData:
                        EventResponseDataObject = new BinderyObjectEventData(
                            EdirEventDataType.EdirTagBinderyEventData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagDsesevInfo:
                        EventResponseDataObject = new SecurityEquivalenceEventData(
                            EdirEventDataType.EdirTagDsesevInfo,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagModuleStateData:
                        EventResponseDataObject = new ModuleStateEventData(
                            EdirEventDataType.EdirTagModuleStateData,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagNetworkAddress:
                        EventResponseDataObject = new NetworkAddressEventData(
                            EdirEventDataType.EdirTagNetworkAddress,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagConnectionState:
                        EventResponseDataObject = new ConnectionStateEventData(
                            EdirEventDataType.EdirTagConnectionState,
                            objTagged.TaggedValue);
                        break;

                    case EdirEventDataType.EdirTagChangeServerAddress:
                        EventResponseDataObject =
                            new ChangeAddressEventData(
                                EdirEventDataType.EdirTagChangeServerAddress,
                                objTagged.TaggedValue);
                        break;

                    /*
                          case EdirEventDataType.EDIR_TAG_CHANGE_CONFIG_PARAM :
                              responsedata =
                                  new ChangeConfigEventData(
                                      taggedobject.TaggedValue);

                              break;

                          case EdirEventDataType.EDIR_TAG_STATUS_LOG :
                              responsedata =
                                  new StatusLogEventData(taggedobject.TaggedValue);

                              break;
                    */
                    case EdirEventDataType.EdirTagNoData:
                        EventResponseDataObject = null;
                        break;

                    default:
                        // unhandled data.
                        throw new IOException();
                }
            }
            else
            {
                // NO DATA
                EventResponseDataObject = null;
            }
        }
    }
}