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
// Novell.Directory.Ldap.LdapExtendedOperation.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

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
    public class LdapExtendedOperation : ICloneable
    {

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
            Id = oid;
            Value = vals;
        }

        /// <summary>
        ///     Returns a clone of this object.
        /// </summary>
        /// <returns>
        ///     clone of this object.
        /// </returns>
        public object Clone()
        {
            var newObj = MemberwiseClone() as LdapExtendedOperation;
            Array.Copy(Value, 0, newObj.Value, 0, Value.Length);
            return newObj;
        }

        /// <summary>
        ///     Returns the unique identifier of the operation.
        /// </summary>
        /// <returns>
        ///     The OID (object ID) of the operation.
        /// </returns>
        public virtual string Id { get; set; }

        /// <summary>
        ///     Returns a reference to the operation-specific data.
        /// </summary>
        /// <returns>
        ///     The operation-specific data.
        /// </returns>
        public virtual byte[] Value { get; set; }
    }
}