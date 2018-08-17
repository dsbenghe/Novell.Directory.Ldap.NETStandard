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
// Novell.Directory.Ldap.Controls.LdapVirtualListControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
    /* The following is the ASN.1 of the VLV Request packet:
        *
        * VirtualListViewRequest ::= SEQUENCE {
        *      beforeCount    INTEGER (0..maxInt),
        *         afterCount     INTEGER (0..maxInt),
        *      CHOICE {
        *          byoffset [0] SEQUENCE {
        *              offset          INTEGER (0 .. maxInt),
        *              contentCount    INTEGER (0 .. maxInt) },
        *          greaterThanOrEqual [1] AssertionValue },
        *      contextID     OCTET STRING OPTIONAL }
        *
        */

    /// <summary>
    ///     LdapVirtualListControl is a Server Control used to specify
    ///     that results from a search are to be returned in pages - which are
    ///     subsets of the entire virtual result set.
    ///     On success, an updated LdapVirtualListResponse object is
    ///     returned as a response Control, containing information on the virtual
    ///     list size and the actual first index. This object can then be used
    ///     by the client with a new requested position or length and sent to the
    ///     server to obtain a different segment of the virtual list.
    /// </summary>
    public class LdapVirtualListControl : LdapControl
    {
        /* The ASN.1 for the VLV Request has CHOICE field. These private
        * variables represent differnt ids for these different options
        */
        private static readonly int Byoffset = 0;
        private static readonly int Greaterthanorequal = 1;

        /// <summary> The Request OID for a VLV Request.</summary>
        private static readonly string RequestOid = "2.16.840.1.113730.3.4.9";

        /*
        * The Response stOID for a VLV Response
        */
        private static readonly string ResponseOid = "2.16.840.1.113730.3.4.10";
        private int _mAfterCount;

        /* Private instance variables go here.
        * These variables are used to store copies of various fields
        * that can be set in a VLV control. One could have managed
        * without really defining these private variables by reverse
        * engineering each field from the ASN.1 encoded control.
        * However that would have complicated and slowed down the code.
        */
        private int _mBeforeCount;
        private int _mContentCount = -1;
        private string _mContext;
        private string _mJumpTo;
        private int _mStartIndex;

        /*
        * The encoded ASN.1 VLV Control is stored in this variable
        */
        private Asn1Sequence _mVlvRequest;

        static LdapVirtualListControl()
        {
            /*
            * This is where we register the control responses
            */
            {
                /* Register the VLV Sort Control class which is returned by the server
                * in response to a VLV Sort Request
                */
                try
                {
                    Register(ResponseOid, Type.GetType("Novell.Directory.Ldap.Controls.LdapVirtualListResponse"));
                }
                catch (Exception e)
                {
                    Logger.Log.LogWarning("Exception swallowed", e);
                }
            }
        }

        /// <summary>
        ///     Constructs a virtual list control using the specified filter
        ///     expression.
        ///     The expression specifies the first entry to be used for the
        ///     virtual search results. The other two paramers are the number of
        ///     entries before and after a located index to be returned.
        /// </summary>
        /// <param name="jumpTo">
        ///     A search expression that defines the first
        ///     element to be returned in the virtual search results. The filter
        ///     expression in the search operation itself may be, for example,
        ///     "objectclass=person" and the jumpTo expression in the virtual
        ///     list control may be "cn=m*", to retrieve a subset of entries
        ///     starting at or centered around those with a common name beginning
        ///     with the letter "M".
        /// </param>
        /// <param name="beforeCount">
        ///     The number of entries before startIndex (the
        ///     reference entry) to be returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after startIndex to be
        ///     returned.
        /// </param>
        public LdapVirtualListControl(string jumpTo, int beforeCount, int afterCount)
            : this(jumpTo, beforeCount, afterCount, null)
        {
        }

        /// <summary>
        ///     Constructs a virtual list control using the specified filter
        ///     expression along with an optional server context.
        ///     The expression specifies the first entry to be used for the
        ///     virtual search results. The other two paramers are the number of
        ///     entries before and after a located index to be returned.
        /// </summary>
        /// <param name="jumpTo">
        ///     A search expression that defines the first
        ///     element to be returned in the virtual search results. The filter
        ///     expression in the search operation itself may be, for example,
        ///     "objectclass=person" and the jumpTo expression in the virtual
        ///     list control may be "cn=m*", to retrieve a subset of entries
        ///     starting at or centered around those with a common name beginning
        ///     with the letter "M".
        /// </param>
        /// <param name="beforeCount">
        ///     The number of entries before startIndex (the
        ///     reference entry) to be returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after startIndex to be
        ///     returned.
        /// </param>
        /// <param name="context">
        ///     Used by some implementations to process requests
        ///     more efficiently. The context should be null on the first search,
        ///     and thereafter it should be whatever was returned by the server in the
        ///     virtual list response control.
        /// </param>
        public LdapVirtualListControl(string jumpTo, int beforeCount, int afterCount, string context)
            : base(RequestOid, true, null)
        {
            /* Save off the fields in local variables
                        */
            _mBeforeCount = beforeCount;
            _mAfterCount = afterCount;
            _mJumpTo = jumpTo;
            _mContext = context;

            /* Call private method to build the ASN.1 encoded request packet.
            */
            BuildTypedVlvRequest();

            /* Set the request data field in the in the parent LdapControl to
            * the ASN.1 encoded value of this control.  This encoding will be
            * appended to the search request when the control is sent.
            */
            SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
        }

        /// <summary>
        ///     Use this constructor to fetch a subset when the size of the
        ///     virtual list is known,.
        /// </summary>
        /// <param name="beforeCount">
        ///     The number of entries before startIndex (the
        ///     reference entry) to be returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after startIndex to be
        ///     returned.
        /// </param>
        /// <param name="startIndex">
        ///     The index of the reference entry to be returned.
        /// </param>
        /// <param name="contentCount">
        ///     The total number of entries assumed to be in the
        ///     list. This is a number returned on a previous search, in the
        ///     LdapVirtualListResponse. The server may use this number to adjust
        ///     the returned subset offset.
        /// </param>
        public LdapVirtualListControl(int startIndex, int beforeCount, int afterCount, int contentCount)
            : this(startIndex, beforeCount, afterCount, contentCount, null)
        {
        }

        /// <summary>
        ///     Use this constructor to fetch a subset when the size of the
        ///     virtual list is known,.
        /// </summary>
        /// <param name="beforeCount">
        ///     The number of entries before startIndex (the
        ///     reference entry) to be returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after startIndex to be
        ///     returned.
        /// </param>
        /// <param name="startIndex">
        ///     The index of the reference entry to be
        ///     returned.
        /// </param>
        /// <param name="contentCount">
        ///     The total number of entries assumed to be in the
        ///     list. This is a number returned on a previous search, in the
        ///     LdapVirtualListResponse. The server may use this number to adjust
        ///     the returned subset offset.
        /// </param>
        /// <param name="context">
        ///     Used by some implementations to process requests
        ///     more efficiently. The context should be null on the first search,
        ///     and thereafter it should be whatever was returned by the server in the
        ///     virtual list response control.
        /// </param>
        public LdapVirtualListControl(int startIndex, int beforeCount, int afterCount, int contentCount, string context)
            : base(RequestOid, true, null)
        {
            /* Save off the fields in local variables
                        */
            _mBeforeCount = beforeCount;
            _mAfterCount = afterCount;
            _mStartIndex = startIndex;
            _mContentCount = contentCount;
            _mContext = context;

            /* Call private method to build the ASN.1 encoded request packet.
            */
            BuildIndexedVlvRequest();

            /* Set the request data field in the in the parent LdapControl to
            * the ASN.1 encoded value of this control.  This encoding will be
            * appended to the search request when the control is sent.
            */
            SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
        }

        /// <summary>
        ///     Returns the number of entries after the top/center one to return per
        ///     page of results.
        /// </summary>
        public virtual int AfterCount => _mAfterCount;

        /// <summary>
        ///     Returns the number of entries before the top/center one to return per
        ///     page of results.
        /// </summary>
        public virtual int BeforeCount => _mBeforeCount;

        /// <summary>
        ///     Returns the size of the virtual search results list. For a newly
        ///     constructed control - one which is not the result of parseResponse on
        ///     a control returned by a server - the method returns -1.
        /// </summary>
        /// <summary>
        ///     Sets the assumed size of the virtual search results list. This will
        ///     typically be a number returned on a previous virtual list request in
        ///     an LdapVirtualListResponse.
        /// </summary>
        public virtual int ListSize
        {
            get => _mContentCount;

            set
            {
                _mContentCount = value;

                /* since we just changed a field we need to rebuild the ber
                * encoded control
                */
                BuildIndexedVlvRequest();

                /* Set the request data field in the in the parent LdapControl to
                * the ASN.1 encoded value of this control.  This encoding will be
                * appended to the search request when the control is sent.
                */
                SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
            }
        }

        /// <summary>
        ///     Returns the cookie used by some servers to optimize the processing of
        ///     virtual list requests.
        /// </summary>
        /// <summary>
        ///     Sets the cookie used by some servers to optimize the processing of
        ///     virtual list requests. It should be the context field returned in a
        ///     virtual list response control for the same search.
        /// </summary>
        public virtual string Context
        {
            get => _mContext;

            set
            {
                /* Index of the context field if one exists in the ber
                */
                var contextidindex = 3;

                /* Save off the new value in private variable
                */
                _mContext = value;

                /* Is there a context field that is already in the ber
                */
                if (_mVlvRequest.Size() == 4)
                {
                    /* If YES then replace it */
                    _mVlvRequest.set_Renamed(contextidindex, new Asn1OctetString(_mContext));
                }
                else if (_mVlvRequest.Size() == 3)
                {
                    /* If no then add a new one */
                    _mVlvRequest.Add(new Asn1OctetString(_mContext));
                }

                /* Set the request data field in the in the parent LdapControl to
                * the ASN.1 encoded value of this control.  This encoding will be
                * appended to the search request when the control is sent.
                */
                SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
            }
        }

        /// <summary>
        ///     Private method used to construct the ber encoded control
        ///     Used only when using the typed mode of VLV Control.
        /// </summary>
        private void BuildTypedVlvRequest()
        {
            /* Create a new Asn1Sequence object */
            _mVlvRequest = new Asn1Sequence(4);

            /* Add the beforeCount and afterCount fields to the sequence */
            _mVlvRequest.Add(new Asn1Integer(_mBeforeCount));
            _mVlvRequest.Add(new Asn1Integer(_mAfterCount));

            /* The next field is dependent on the type of indexing being used.
            * A "typed" VLV request uses a ASN.1 OCTET STRING to index to the
            * correct object in the list.  Encode the ASN.1 CHOICE corresponding
            * to this option (as indicated by the greaterthanOrEqual field)
            * in the ASN.1.
            */
            _mVlvRequest.Add(new Asn1Tagged(
                new Asn1Identifier(Asn1Identifier.Context, false, Greaterthanorequal),
                new Asn1OctetString(_mJumpTo), false));

            /* Add the optional context string if one is available.
            */
            if (_mContext != null)
            {
                _mVlvRequest.Add(new Asn1OctetString(_mContext));
            }
        }

        /// <summary>
        ///     Private method used to construct the ber encoded control
        ///     Used only when using the Indexed mode of VLV Control.
        /// </summary>
        private void BuildIndexedVlvRequest()
        {
            /* Create a new Asn1Sequence object */
            _mVlvRequest = new Asn1Sequence(4);

            /* Add the beforeCount and afterCount fields to the sequence */
            _mVlvRequest.Add(new Asn1Integer(_mBeforeCount));
            _mVlvRequest.Add(new Asn1Integer(_mAfterCount));

            /* The next field is dependent on the type of indexing being used.
            * An "indexed" VLV request uses a ASN.1 SEQUENCE to index to the
            * correct object in the list.  Encode the ASN.1 CHOICE corresponding
            * to this option (as indicated by the byoffset fieldin the ASN.1.
            */
            var byoffset = new Asn1Sequence(2);
            byoffset.Add(new Asn1Integer(_mStartIndex));
            byoffset.Add(new Asn1Integer(_mContentCount));

            /* Add the ASN.1 sequence to the encoded data
            */
            _mVlvRequest.Add(
                new Asn1Tagged(new Asn1Identifier(Asn1Identifier.Context, true, Byoffset), byoffset, false));

            /* Add the optional context string if one is available.
            */
            if (_mContext != null)
            {
                _mVlvRequest.Add(new Asn1OctetString(_mContext));
            }
        }

        /// <summary>
        ///     Sets the center or starting list index to return, and the number of
        ///     results before and after.
        /// </summary>
        /// <param name="listIndex">
        ///     The center or starting list index to be
        ///     returned.
        /// </param>
        /// <param name="beforeCount">
        ///     The number of entries before "listIndex" to be
        ///     returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after "listIndex" to be
        ///     returned.
        /// </param>
        public virtual void SetRange(int listIndex, int beforeCount, int afterCount)
        {
            /* Save off the fields in local variables
                        */
            _mBeforeCount = beforeCount;
            _mAfterCount = afterCount;
            _mStartIndex = listIndex;

            /* since we just changed a field we need to rebuild the ber
            * encoded control
            */
            BuildIndexedVlvRequest();

            /* Set the request data field in the in the parent LdapControl to
            * the ASN.1 encoded value of this control.  This encoding will be
            * appended to the search request when the control is sent.
            */
            SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
        }

        // PROPOSED ADDITION TO NEXT VERSION OF DRAFT (v7)
        /// <summary>
        ///     Sets the center or starting list index to return, and the number of
        ///     results before and after.
        /// </summary>
        /// <param name="jumpTo">
        ///     A search expression that defines the first
        ///     element to be returned in the virtual search results. The filter
        ///     expression in the search operation itself may be, for example,
        ///     "objectclass=person" and the jumpTo expression in the virtual
        ///     list control may be "cn=m*", to retrieve a subset of entries
        ///     starting at or centered around those with a common name
        ///     beginning with the letter "M".
        /// </param>
        /// <param name="beforeCount">
        ///     The number of entries before "listIndex" to be
        ///     returned.
        /// </param>
        /// <param name="afterCount">
        ///     The number of entries after "listIndex" to be
        ///     returned.
        /// </param>
        public virtual void SetRange(string jumpTo, int beforeCount, int afterCount)
        {
            /* Save off the fields in local variables
            */
            _mBeforeCount = beforeCount;
            _mAfterCount = afterCount;
            _mJumpTo = jumpTo;

            /* since we just changed a field we need to rebuild the ber
            * encoded control
            */
            BuildTypedVlvRequest();

            /* Set the request data field in the in the parent LdapControl to
            * the ASN.1 encoded value of this control.  This encoding will be
            * appended to the search request when the control is sent.
            */
            SetValue(_mVlvRequest.GetEncoding(new LberEncoder()));
        }
    }
}