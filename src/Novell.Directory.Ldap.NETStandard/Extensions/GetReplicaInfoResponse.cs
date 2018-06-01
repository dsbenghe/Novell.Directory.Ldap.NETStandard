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
// Novell.Directory.Ldap.Extensions.GetReplicaInfoResponse.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System.IO;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

namespace Novell.Directory.Ldap.Extensions
{
    /// <summary>
    ///     Retrieves the replica information from a GetReplicaInfoResponse object.
    ///     An object in this class is generated from an ExtendedResponse using the
    ///     ExtendedResponseFactory class.
    ///     The getReplicaInfoResponse extension uses the following OID:
    ///     2.16.840.1.113719.1.27.100.18
    /// </summary>
    public class GetReplicaInfoResponse : LdapExtendedResponse
    {
        // Other info as returned by the server
        private readonly int _partitionId;
        private readonly int _replicaState;
        private readonly int _modificationTime;
        private readonly int _purgeTime;
        private readonly int _localPartitionId;
        private readonly string _partitionDn;
        private readonly int _replicaType;
        private readonly int _flags;

        /// <summary>
        ///     Constructs an object from the responseValue which contains the
        ///     replica information.
        ///     The constructor parses the responseValue which has the following
        ///     format:
        ///     responseValue ::=
        ///     partitionID         INTEGER
        ///     replicaState        INTEGER
        ///     modificationTime    INTEGER
        ///     purgeTime           INTEGER
        ///     localPartitionID    INTEGER
        ///     partitionDN       OCTET STRING
        ///     replicaType         INTEGER
        ///     flags               INTEGER
        /// </summary>
        /// <exception>
        ///     IOException The response value could not be decoded.
        /// </exception>
        public GetReplicaInfoResponse(RfcLdapMessage rfcMessage) : base(rfcMessage)
        {
            if (ResultCode == LdapException.Success)
            {
                // parse the contents of the reply
                var returnedValue = Value;
                if (returnedValue == null)
                    throw new IOException("No returned value");

                // Create a decoder object
                var decoder = new LberDecoder();
                if (decoder == null)
                    throw new IOException("Decoding error");

                // Parse the parameters in the order

                var currentPtr = new MemoryStream(SupportClass.ToByteArray(returnedValue));

                // Parse partitionID
                var asn1PartitionId = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1PartitionId == null)
                    throw new IOException("Decoding error");

                _partitionId = asn1PartitionId.IntValue();


                // Parse replicaState
                var asn1ReplicaState = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1ReplicaState == null)
                    throw new IOException("Decoding error");

                _replicaState = asn1ReplicaState.IntValue();

                // Parse modificationTime
                var asn1ModificationTime = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1ModificationTime == null)
                    throw new IOException("Decoding error");

                _modificationTime = asn1ModificationTime.IntValue();

                // Parse purgeTime
                var asn1PurgeTime = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1PurgeTime == null)
                    throw new IOException("Decoding error");

                _purgeTime = asn1PurgeTime.IntValue();

                // Parse localPartitionID
                var asn1LocalPartitionId = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1LocalPartitionId == null)
                    throw new IOException("Decoding error");

                _localPartitionId = asn1LocalPartitionId.IntValue();

                // Parse partitionDN
                var asn1PartitionDn = (Asn1OctetString) decoder.Decode(currentPtr);
                if (asn1PartitionDn == null)
                    throw new IOException("Decoding error");

                _partitionDn = asn1PartitionDn.StringValue();
                if ((object) _partitionDn == null)
                    throw new IOException("Decoding error");


                // Parse replicaType
                var asn1ReplicaType = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1ReplicaType == null)
                    throw new IOException("Decoding error");

                _replicaType = asn1ReplicaType.IntValue();


                // Parse flags
                var asn1Flags = (Asn1Integer) decoder.Decode(currentPtr);
                if (asn1Flags == null)
                    throw new IOException("Decoding error");

                _flags = asn1Flags.IntValue();
            }
            else
            {
                _partitionId = 0;
                _replicaState = 0;
                _modificationTime = 0;
                _purgeTime = 0;
                _localPartitionId = 0;
                _partitionDn = "";
                _replicaType = 0;
                _flags = 0;
            }
        }


        /// <summary>
        ///     Returns the numeric identifier for the partition.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the partition ID.
        /// </returns>
        public virtual int GetpartitionId()
        {
            return _partitionId;
        }

        /// <summary>
        ///     Returns the current state of the replica.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the current state of the replica. See
        ///     ReplicationConstants class for possible values for this field.
        /// </returns>
        /// <seealso cref="ReplicationConstants.LdapRsBeginAdd">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsDeadReplica">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsDyingReplica">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsJs0">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsJs1">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsJs2">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsLocked">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsMasterDone">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsMasterStart">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsSs0">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRsTransitionOn">
        /// </seealso>
        public virtual int GetreplicaState()
        {
            return _replicaState;
        }


        /// <summary>
        ///     Returns the time of the most recent modification.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the last modification time.
        /// </returns>
        public virtual int GetmodificationTime()
        {
            return _modificationTime;
        }


        /// <summary>
        ///     Returns the most recent time in which all data has been synchronized.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the last purge time.
        /// </returns>
        public virtual int GetpurgeTime()
        {
            return _purgeTime;
        }

        /// <summary>
        ///     Returns the local numeric identifier for the replica.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the local ID of the partition.
        /// </returns>
        public virtual int GetlocalPartitionId()
        {
            return _localPartitionId;
        }

        /// <summary>
        ///     Returns the distinguished name of the partition.
        /// </summary>
        /// <returns>
        ///     String value specifying the name of the partition read.
        /// </returns>
        public virtual string GetpartitionDn()
        {
            return _partitionDn;
        }

        /// <summary>
        ///     Returns the replica type.
        ///     See the ReplicationConstants class for possible values for
        ///     this field.
        /// </summary>
        /// <returns>
        ///     Integer identifying the type of the replica.
        /// </returns>
        /// <seealso cref="ReplicationConstants.LdapRtMaster">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRtSecondary">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRtReadonly">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRtSubref">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRtSparseWrite">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapRtSparseRead">
        /// </seealso>
        public virtual int GetreplicaType()
        {
            return _replicaType;
        }

        /// <summary>
        ///     Returns flags that specify whether the replica is busy or is a boundary.
        ///     See the ReplicationConstants class for possible values for
        ///     this field.
        /// </summary>
        /// <returns>
        ///     Integer value specifying the flags for the replica.
        /// </returns>
        /// <seealso cref="ReplicationConstants.LdapDsFlagBusy">
        /// </seealso>
        /// <seealso cref="ReplicationConstants.LdapDsFlagBoundary">
        /// </seealso>
        public virtual int Getflags()
        {
            return _flags;
        }
    }
}