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
using System.Text;

namespace Novell.Directory.Ldap.Controls
{
    /// <summary>
    ///     LdapPersistSearchControl is a Server Control that allows a client
    ///     to receive notifications from the server of changes to entries within
    ///     the searches result set. The client can be notified when an entry is
    ///     added to the result set, when an entry is deleted from the result set,
    ///     when a DN has been changed or when and attribute value has been changed.
    /// </summary>
    public class LdapPersistSearchControl : LdapControl
    {
        /// <summary>
        ///     Change type specifying that you want to track additions of new entries
        ///     to the directory.
        /// </summary>
        public const int Add = 1;

        /// <summary>
        ///     Change type specifying that you want to track removals of entries from
        ///     the directory.
        /// </summary>
        public const int Delete = 2;

        /// <summary>
        ///     Change type specifying that you want to track modifications of entries
        ///     in the directory.
        /// </summary>
        public const int Modify = 4;

        /// <summary>
        ///     Change type specifying that you want to track modifications of the DNs
        ///     of entries in the directory.
        /// </summary>
        public const int Moddn = 8;

        /* private data members */
        private const int SequenceSize = 3;

        private const int ChangetypesIndex = 0;
        private const int ChangesonlyIndex = 1;
        private const int ReturncontrolsIndex = 2;

        private static readonly LberEncoder SEncoder;

        /// <summary> The requestOID of the persistent search control.</summary>
        private const string RequestOid = "2.16.840.1.113730.3.4.3";

        /// <summary> The responseOID of the psersistent search - entry change control.</summary>
        private const string ResponseOid = "2.16.840.1.113730.3.4.7";

        /// <summary>
        ///     Change type specifying that you want to track any of the above
        ///     modifications.
        /// </summary>
        public static readonly int Any = Add | Delete | Modify | Moddn;

        private readonly Asn1Sequence _mSequence;
        private bool _mChangesOnly;

        private int _mChangeTypes;
        private bool _mReturnControls;

        static LdapPersistSearchControl()
        {
            SEncoder = new LberEncoder();
            /*
            * This is where we register the control response
            */
            {
                /* Register the Entry Change control class which is returned by the
                * server in response to a persistent search request
                */
                Register(ResponseOid, typeof(LdapEntryChangeControl));
            }
        }

        /* public constructors */

        /// <summary>
        ///     The default constructor. A control with changes equal to ANY,
        ///     isCritical equal to true, changesOnly equal to true, and
        ///     returnControls equal to true.
        /// </summary>
        public LdapPersistSearchControl()
            : this(Any, true, true, true)
        {
        }

        /// <summary>
        ///     Constructs an LdapPersistSearchControl object according to the
        ///     supplied parameters. The resulting control is used to specify a
        ///     persistent search.
        /// </summary>
        /// <param name="changeTypes">
        ///     the change types to monitor. The bitwise OR of any
        ///     of the following values:
        ///     <li>                           LdapPersistSearchControl.ADD</li>
        ///     <li>                           LdapPersistSearchControl.DELETE</li>
        ///     <li>                           LdapPersistSearchControl.MODIFY</li>
        ///     <li>                           LdapPersistSearchControl.MODDN</li>
        ///     To track all changes the value can be set to:.
        ///     <li>                           LdapPersistSearchControl.ANY</li>
        /// </param>
        /// <param name="changesOnly">
        ///     true if you do not want the server to return
        ///     all existing entries in the directory that match the search
        ///     criteria. (Use this if you just want the changed entries to be
        ///     returned.).
        /// </param>
        /// <param name="returnControls">
        ///     true if you want the server to return entry
        ///     change controls with each entry in the search results. You need to
        ///     return entry change controls to discover what type of change
        ///     and other additional information about the change.
        /// </param>
        /// <param name="isCritical">
        ///     true if this control is critical to the search
        ///     operation. If true and the server does not support this control,
        ///     the server will not perform the search at all.
        /// </param>
        public LdapPersistSearchControl(int changeTypes, bool changesOnly, bool returnControls, bool isCritical)
            : base(RequestOid, isCritical, null)
        {
            _mChangeTypes = changeTypes;
            _mChangesOnly = changesOnly;
            _mReturnControls = returnControls;

            _mSequence = new Asn1Sequence(SequenceSize);

            _mSequence.Add(new Asn1Integer(_mChangeTypes));
            _mSequence.Add(new Asn1Boolean(_mChangesOnly));
            _mSequence.Add(new Asn1Boolean(_mReturnControls));

            SetValue();
        }

        /// <summary>
        ///     Returns the change types to be monitored as a logical OR of any or
        ///     all of these values: ADD, DELETE, MODIFY, and/or MODDN.
        /// </summary>
        /// <returns>
        ///     the change types to be monitored. The logical or of any of
        ///     the following values: ADD, DELETE, MODIFY, and/or MODDN.
        /// </returns>
        /// <summary>
        ///     Sets the change types to be monitored.
        ///     types  The change types to be monitored as a logical OR of any or all
        ///     of these types: ADD, DELETE, MODIFY, and/or MODDN. Can also be set
        ///     to the value ANY which is defined as the logical OR of all of the
        ///     preceding values.
        /// </summary>
        public int ChangeTypes
        {
            get => _mChangeTypes;

            set
            {
                _mChangeTypes = value;
                _mSequence.set_Renamed(ChangetypesIndex, new Asn1Integer(_mChangeTypes));
                SetValue();
            }
        }

        /// <summary>
        ///     Returns true if entry change controls are to be returned with the
        ///     search results.
        /// </summary>
        /// <returns>
        ///     true if entry change controls are to be returned with the
        ///     search results. Otherwise, false is returned.
        /// </returns>
        /// <summary>
        ///     When set to true, requests that entry change controls be returned with
        ///     the search results.
        /// </summary>
        /// <param name="returnControls">
        ///     true to return entry change controls.
        /// </param>
        public bool ReturnControls
        {
            get => _mReturnControls;

            set
            {
                _mReturnControls = value;
                _mSequence.set_Renamed(ReturncontrolsIndex, new Asn1Boolean(_mReturnControls));
                SetValue();
            }
        }

        /// <summary>
        ///     getChangesOnly returns true if only changes are to be returned.
        ///     Results from the initial search are not returned.
        /// </summary>
        /// <returns>
        ///     true of only changes are to be returned.
        /// </returns>
        /// <summary>
        ///     When set to true, requests that only changes be returned, results from
        ///     the initial search are not returned.
        /// </summary>
        /// <param name="changesOnly">
        ///     true to skip results for the initial search.
        /// </param>
        public bool ChangesOnly
        {
            get => _mChangesOnly;

            set
            {
                _mChangesOnly = value;
                _mSequence.set_Renamed(ChangesonlyIndex, new Asn1Boolean(_mChangesOnly));
                SetValue();
            }
        }

        public override string ToString()
        {
            var data = _mSequence.GetEncoding(SEncoder);

            var buf = new StringBuilder(data.Length);

            for (var i = 0; i < data.Length; i++)
            {
                buf.Append(data[i].ToString());
                if (i < data.Length - 1)
                {
                    buf.Append(",");
                }
            }

            return buf.ToString();
        }

        /// <summary>  Sets the encoded value of the LdapControlClass.</summary>
        private void SetValue()
        {
            SetValue(_mSequence.GetEncoding(SEncoder));
        }
    } // end class LdapPersistentSearchControl
}
