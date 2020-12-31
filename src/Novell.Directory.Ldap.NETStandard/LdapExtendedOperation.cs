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

using System;

namespace Novell.Directory.Ldap
{
    /// <summary>
    ///     Encapsulates an ID which uniquely identifies a particular extended
    ///     operation, known to a particular server, and the data associated
    ///     with that extended operation.
    /// </summary>
    /// <seealso cref="LdapConnection.ExtendedOperation">
    /// </seealso>
    public class LdapExtendedOperation
    {
        public virtual DebugId DebugId { get; } = DebugId.ForType<LdapExtendedOperation>();

        private string _oid;
        private byte[] _vals;

        /// <summary>
        ///     Constructs a new object with the specified object ID and data.
        /// </summary>
        /// <param name="oid">
        ///     The unique identifier of the operation.
        /// </param>
        /// <param name="vals">
        ///     The operation-specific data of the operation.
        /// </param>
        public LdapExtendedOperation(string oid, byte[] vals)
        {
            _oid = oid;
            _vals = vals;
        }

        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            try
            {
                var newObj = MemberwiseClone();

                // Array.Copy((System.Array)SupportClass.ToByteArray( this.vals), 0, (System.Array)SupportClass.ToByteArray( ((LdapExtendedOperation) newObj).vals), 0, this.vals.Length);
                Array.Copy(_vals, 0, ((LdapExtendedOperation)newObj)._vals, 0, _vals.Length);
                return newObj;
            }
            catch (Exception ce)
            {
                throw new Exception("Internal error, cannot create clone", ce);
            }
        }

        /// <summary>
        ///     Returns the unique identifier of the operation.
        /// </summary>
        /// <returns>
        ///     The OID (object ID) of the operation.
        /// </returns>
        public string GetId()
        {
            return _oid;
        }

        /// <summary>
        ///     Returns a reference to the operation-specific data.
        /// </summary>
        /// <returns>
        ///     The operation-specific data.
        /// </returns>
        public byte[] GetValue()
        {
            return _vals;
        }

        /// <summary>
        ///     Sets the value for the operation-specific data.
        /// </summary>
        /// <param name="newVals">
        ///     The byte array of operation-specific data.
        /// </param>
        protected void SetValue(byte[] newVals)
        {
            _vals = newVals;
        }

        /// <summary>
        ///     Resets the OID for the operation to a new value.
        /// </summary>
        /// <param name="newoid">
        ///     The new OID for the operation.
        /// </param>
        protected void SetId(string newoid)
        {
            _oid = newoid;
        }
    }
}
