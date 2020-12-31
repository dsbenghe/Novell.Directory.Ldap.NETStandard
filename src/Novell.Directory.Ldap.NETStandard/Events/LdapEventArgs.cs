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

using System.Text;

namespace Novell.Directory.Ldap.Events
{
    /// <summary>
    ///     This class represents the EventArgs for Ldap events in general.
    ///     This is also the base class for more specific Ldap events.
    /// </summary>
    /// <seealso cref='Novell.Directory.Ldap.Events.SearchResultEventArgs' />
    /// <seealso cref='Novell.Directory.Ldap.Events.SearchReferralEventArgs' />
    public class LdapEventArgs : DirectoryEventArgs
    {
        public LdapEventArgs(
            LdapMessage sourceMessage,
            EventClassifiers aClassification,
            LdapEventType aType)
            : base(sourceMessage, aClassification)
        {
            EventType = aType;
        }

        public LdapEventType EventType { get; set; }

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.Append("[");
            buf.AppendFormat("{0}:", GetType());
            buf.AppendFormat("(Classification={0})", EventClassification);
            buf.AppendFormat("(Type={0})", EventType);
            buf.AppendFormat("(EventInformation:{0})", LdapMessage);
            buf.Append("]");

            return buf.ToString();
        }
    } // end of class LdapEventArgs
}
