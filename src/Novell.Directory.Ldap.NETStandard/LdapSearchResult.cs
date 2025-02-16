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
using Novell.Directory.Ldap.Rfc2251;
using System;
using System.Threading;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Encapsulates a single search result that is in response to an asynchronous
    ///     search operation.
    /// </summary>
    /// <seealso cref="LdapConnection.SearchAsync(LdapUrl,CancellationToken)"/>
    /// <seealso cref="LdapConnection.SearchAsync(LdapUrl,LdapSearchConstraints,CancellationToken)"/>
    /// <seealso cref="LdapConnection.SearchAsync(string,int,string,string[],bool,CancellationToken)"/>
    /// <seealso cref="LdapConnection.SearchAsync(string,int,string,string[],bool,LdapSearchQueue,CancellationToken)"/>
    /// <seealso cref="LdapConnection.SearchAsync(string,int,string,string[],bool,LdapSearchConstraints,CancellationToken)"/>
    /// <seealso cref="LdapConnection.SearchAsync(string,int,string,string[],bool,LdapSearchQueue,LdapSearchConstraints,CancellationToken)"/>
    public class LdapSearchResult : LdapMessage
    {
        public override DebugId DebugId { get; } = DebugId.ForType<LdapSearchResult>();
        private LdapEntry _entry;

        /// <summary>
        ///     Constructs an LdapSearchResult object.
        /// </summary>
        /// <param name="message">
        ///     The RfcLdapMessage with a search result.
        /// </param>
        /*package*/
        internal LdapSearchResult(RfcLdapMessage message)
            : base(message)
        {
        }

        /// <summary>
        ///     Constructs an LdapSearchResult object from an LdapEntry.
        /// </summary>
        /// <param name="entry">
        ///     the LdapEntry represented by this search result.
        /// </param>
        /// <param name="cont">
        ///     controls associated with the search result.
        /// </param>
        public LdapSearchResult(LdapEntry entry, LdapControl[] cont)
        {
            _entry = entry ?? throw new ArgumentException("Argument \"entry\" cannot be null");
        }

        /// <summary>
        ///     Returns the entry of a server's search response.
        /// </summary>
        /// <returns>
        ///     The LdapEntry associated with this LdapSearchResult.
        /// </returns>
        public LdapEntry Entry
        {
            get
            {
                if (_entry == null)
                {
                    var attrs = new LdapAttributeSet();

                    var attrList = ((RfcSearchResultEntry)Message.Response).Attributes;

                    var seqArray = attrList.ToArray();
                    for (var i = 0; i < seqArray.Length; i++)
                    {
                        var seq = (Asn1Sequence)seqArray[i];
                        var attr = new LdapAttribute(((Asn1OctetString)seq[0]).StringValue());

                        var set = (Asn1Set)seq[1];
                        foreach (Asn1OctetString octectString in set)
                        {
                            attr.AddValue(octectString.ByteValue());
                        }

                        attrs.Add(attr);
                    }

                    _entry = new LdapEntry(((RfcSearchResultEntry)Message.Response).ObjectName.StringValue(), attrs);
                }

                return _entry;
            }
        }

        /// <summary>
        ///     Return a String representation of this object.
        /// </summary>
        /// <returns>
        ///     a String representing this object.
        /// </returns>
        public override string ToString()
        {
            string str;
            if (_entry == null)
            {
                str = base.ToString();
            }
            else
            {
                str = _entry.ToString();
            }

            return str;
        }
    }
}
