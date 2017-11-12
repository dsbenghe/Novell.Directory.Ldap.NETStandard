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
// Novell.Directory.Ldap.Events.SearchResultEventArgs.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.Text;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This class represents the EventArgs corresponding to
    ///     LdapSearchResult notification sent by Ldap Server.
    /// </summary>
    public class SearchResultEventArgs : LdapEventArgs
    {
        public SearchResultEventArgs(LdapMessage sourceMessage,
                                     EventClassifiers classification,
                                     LdapEventType type)
            : base(sourceMessage, EventClassifiers.CLASSIFICATION_LDAP_PSEARCH, type)
        {
        }

        public LdapEntry Entry => (ContianedEventInformation as LdapSearchResult).Entry;

        public override string ToString()
        {
            var buf = new StringBuilder();

            buf.AppendFormat("[{0}:", GetType());
            buf.AppendFormat("(Classification={0})", EventClassification);
            buf.AppendFormat("(Type={0})", GetChangeTypeString());
            buf.AppendFormat("(EventInformation:{0})", GetStringRepresentaionOfEventInformation());
            buf.Append("]");

            return buf.ToString();
        }

        private string GetStringRepresentaionOfEventInformation()
        {
            var buf = new StringBuilder();
            LdapSearchResult result = ContianedEventInformation as LdapSearchResult;

            buf.AppendFormat("(Entry={0})", result.Entry);
            var controls = result.Controls;

            if (controls != null)
            {
                buf.Append("(Controls=");
                for (int i = 0; i < controls.Length; i++)
                {
                    buf.Append($"(Control{++i}={controls[i].ToString()})");
                }
                buf.Append(")");
            }

            return buf.ToString();
        }

        private string GetChangeTypeString()
        {
            switch (EventType)
            {
                case LdapEventType.LDAP_PSEARCH_ADD:
                    return "ADD";

                case LdapEventType.LDAP_PSEARCH_DELETE:
                    return "DELETE";

                case LdapEventType.LDAP_PSEARCH_MODIFY:
                    return "MODIFY";

                case LdapEventType.LDAP_PSEARCH_MODDN:
                    return "MODDN";

                default:
                    return "No change type: " + EventType;
            }
        }
    }
}