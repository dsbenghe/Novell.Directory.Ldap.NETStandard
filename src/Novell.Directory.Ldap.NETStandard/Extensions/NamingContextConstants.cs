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
// Novell.Directory.Ldap.Extensions.NamingContextConstants.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

namespace Novell.Directory.Ldap.Extensions
{
    /*
        * public class NamingContextConstants
        */

    /// <summary> Contains a collection of constants used by the Novell Ldap extensions.</summary>
    public class NamingContextConstants
    {
        /// <summary> A constant for the createNamingContextRequest OID.</summary>
        public const string CreateNamingContextReq = "2.16.840.1.113719.1.27.100.3";

        /// <summary> A constant for the createNamingContextResponse OID.</summary>
        public const string CreateNamingContextRes = "2.16.840.1.113719.1.27.100.4";

        /// <summary> A constant for the mergeNamingContextRequest OID.</summary>
        public const string MergeNamingContextReq = "2.16.840.1.113719.1.27.100.5";

        /// <summary> A constant for the mergeNamingContextResponse OID.</summary>
        public const string MergeNamingContextRes = "2.16.840.1.113719.1.27.100.6";

        /// <summary> A constant for the addReplicaRequest OID.</summary>
        public const string AddReplicaReq = "2.16.840.1.113719.1.27.100.7";

        /// <summary> A constant for the addReplicaResponse OID.</summary>
        public const string AddReplicaRes = "2.16.840.1.113719.1.27.100.8";

        /// <summary> A constant for the refreshServerRequest OID.</summary>
        public const string RefreshServerReq = "2.16.840.1.113719.1.27.100.9";

        /// <summary> A constant for the refreshServerResponse OID.</summary>
        public const string RefreshServerRes = "2.16.840.1.113719.1.27.100.10";

        /// <summary> A constant for the removeReplicaRequest OID.</summary>
        public const string DeleteReplicaReq = "2.16.840.1.113719.1.27.100.11";

        /// <summary> A constant for the removeReplicaResponse OID.</summary>
        public const string DeleteReplicaRes = "2.16.840.1.113719.1.27.100.12";

        /// <summary> A constant for the namingContextEntryCountRequest OID.</summary>
        public const string NamingContextCountReq = "2.16.840.1.113719.1.27.100.13";

        /// <summary> A constant for the namingContextEntryCountResponse OID.</summary>
        public const string NamingContextCountRes = "2.16.840.1.113719.1.27.100.14";

        /// <summary> A constant for the changeReplicaTypeRequest OID.</summary>
        public const string ChangeReplicaTypeReq = "2.16.840.1.113719.1.27.100.15";

        /// <summary> A constant for the changeReplicaTypeResponse OID.</summary>
        public const string ChangeReplicaTypeRes = "2.16.840.1.113719.1.27.100.16";

        /// <summary> A constant for the getReplicaInfoRequest OID.</summary>
        public const string GetReplicaInfoReq = "2.16.840.1.113719.1.27.100.17";

        /// <summary> A constant for the getReplicaInfoResponse OID.</summary>
        public const string GetReplicaInfoRes = "2.16.840.1.113719.1.27.100.18";

        /// <summary> A constant for the listReplicaRequest OID.</summary>
        public const string ListReplicasReq = "2.16.840.1.113719.1.27.100.19";

        /// <summary> A constant for the listReplicaResponse OID.</summary>
        public const string ListReplicasRes = "2.16.840.1.113719.1.27.100.20";

        /// <summary> A constant for the receiveAllUpdatesRequest OID.</summary>
        public const string ReceiveAllUpdatesReq = "2.16.840.1.113719.1.27.100.21";

        /// <summary> A constant for the receiveAllUpdatesResponse OID.</summary>
        public const string ReceiveAllUpdatesRes = "2.16.840.1.113719.1.27.100.22";

        /// <summary> A constant for the sendAllUpdatesRequest OID.</summary>
        public const string SendAllUpdatesReq = "2.16.840.1.113719.1.27.100.23";

        /// <summary> A constant for the sendAllUpdatesResponse OID.</summary>
        public const string SendAllUpdatesRes = "2.16.840.1.113719.1.27.100.24";

        /// <summary> A constant for the requestNamingContextSyncRequest OID.</summary>
        public const string NamingContextSyncReq = "2.16.840.1.113719.1.27.100.25";

        /// <summary> A constant for the requestNamingContextSyncResponse OID.</summary>
        public const string NamingContextSyncRes = "2.16.840.1.113719.1.27.100.26";

        /// <summary> A constant for the requestSchemaSyncRequest OID.</summary>
        public const string SchemaSyncReq = "2.16.840.1.113719.1.27.100.27";

        /// <summary> A constant for the requestSchemaSyncResponse OID.</summary>
        public const string SchemaSyncRes = "2.16.840.1.113719.1.27.100.28";

        /// <summary> A constant for the abortNamingContextOperationRequest OID.</summary>
        public const string AbortNamingContextOpReq = "2.16.840.1.113719.1.27.100.29";

        /// <summary> A constant for the abortNamingContextOperationResponse OID.</summary>
        public const string AbortNamingContextOpRes = "2.16.840.1.113719.1.27.100.30";

        /// <summary> A constant for the getContextIdentityNameRequest OID.</summary>
        public const string GetIdentityNameReq = "2.16.840.1.113719.1.27.100.31";

        /// <summary> A constant for the getContextIdentityNameResponse OID.</summary>
        public const string GetIdentityNameRes = "2.16.840.1.113719.1.27.100.32";

        /// <summary> A constant for the getEffectivePrivilegesRequest OID.</summary>
        public const string GetEffectivePrivilegesReq = "2.16.840.1.113719.1.27.100.33";

        /// <summary> A constant for the getEffectivePrivilegesResponse OID.</summary>
        public const string GetEffectivePrivilegesRes = "2.16.840.1.113719.1.27.100.34";

        /// <summary> A constant for the setReplicationFilterRequest OID.</summary>
        public const string SetReplicationFilterReq = "2.16.840.1.113719.1.27.100.35";

        /// <summary> A constant for the setReplicationFilterResponse OID.</summary>
        public const string SetReplicationFilterRes = "2.16.840.1.113719.1.27.100.36";

        /// <summary> A constant for the getReplicationFilterRequest OID.</summary>
        public const string GetReplicationFilterReq = "2.16.840.1.113719.1.27.100.37";

        /// <summary> A constant for the getReplicationFilterResponse OID.</summary>
        public const string GetReplicationFilterRes = "2.16.840.1.113719.1.27.100.38";

        /// <summary> A constant for the createOrphanNamingContextRequest OID.</summary>
        public const string CreateOrphanNamingContextReq = "2.16.840.1.113719.1.27.100.39";

        /// <summary> A constant for the createOrphanNamingContextResponse OID.</summary>
        public const string CreateOrphanNamingContextRes = "2.16.840.1.113719.1.27.100.40";

        /// <summary> A constant for the removeOrphanNamingContextRequest OID.</summary>
        public const string RemoveOrphanNamingContextReq = "2.16.840.1.113719.1.27.100.41";

        /// <summary> A constant for the removeOrphanNamingContextResponse OID.</summary>
        public const string RemoveOrphanNamingContextRes = "2.16.840.1.113719.1.27.100.42";

        /// <summary> A constant for the triggerBackLinkerRequest OID.</summary>
        public const string TriggerBklinkerReq = "2.16.840.1.113719.1.27.100.43";

        /// <summary> A constant for the triggerBackLinkerResponse OID.</summary>
        public const string TriggerBklinkerRes = "2.16.840.1.113719.1.27.100.44";

        /// <summary> A constant for the triggerJanitorRequest OID.</summary>
        public const string TriggerJanitorReq = "2.16.840.1.113719.1.27.100.47";

        /// <summary> A constant for the triggerJanitorResponse OID.</summary>
        public const string TriggerJanitorRes = "2.16.840.1.113719.1.27.100.48";

        /// <summary> A constant for the triggerLimberRequest OID.</summary>
        public const string TriggerLimberReq = "2.16.840.1.113719.1.27.100.49";

        /// <summary> A constant for the triggerLimberResponse OID.</summary>
        public const string TriggerLimberRes = "2.16.840.1.113719.1.27.100.50";

        /// <summary> A constant for the triggerSkulkerRequest OID.</summary>
        public const string TriggerSkulkerReq = "2.16.840.1.113719.1.27.100.51";

        /// <summary> A constant for the triggerSkulkerResponse OID.</summary>
        public const string TriggerSkulkerRes = "2.16.840.1.113719.1.27.100.52";

        /// <summary> A constant for the triggerSchemaSyncRequest OID.</summary>
        public const string TriggerSchemaSyncReq = "2.16.840.1.113719.1.27.100.53";

        /// <summary> A constant for the triggerSchemaSyncResponse OID.</summary>
        public const string TriggerSchemaSyncRes = "2.16.840.1.113719.1.27.100.54";

        /// <summary> A constant for the triggerPartitionPurgeRequest OID.</summary>
        public const string TriggerPartPurgeReq = "2.16.840.1.113719.1.27.100.55";

        /// <summary> A constant for the triggerPartitionPurgeResponse OID.</summary>
        public const string TriggerPartPurgeRes = "2.16.840.1.113719.1.27.100.56";

        /// <summary>
        ///     A constant that specifies that all servers in a replica ring must be
        ///     running for a naming context operation to proceed.
        /// </summary>
        public const int LdapEnsureServersUp = 1;

        /// <summary>
        ///     Identifies this replica as the master replica of the naming context.
        ///     On this type of replica, entries can be modified, and naming context
        ///     operations can be performed.
        /// </summary>
        public const int LdapRtMaster = 0;

        /// <summary>
        ///     Identifies this replica as a secondary replica of the naming context.
        ///     On this type of replica, read and write operations can be performed,
        ///     and entries can be modified.
        /// </summary>
        public const int LdapRtSecondary = 1;

        /// <summary>
        ///     Identifies this replica as a read-only replica of the naming context.
        ///     Only Novell eDirectory synchronization processes can modifie
        ///     entries on this replica.
        /// </summary>
        public const int LdapRtReadonly = 2;

        /// <summary>
        ///     Identifies this replica as a subordinate reference replica of the
        ///     naming context.
        ///     Novell eDirectory automatically adds these replicas to a server
        ///     when the server does not contain replicas of all child naming contexts.
        ///     Only eDirectory can modify information on these types of replicas.
        /// </summary>
        public const int LdapRtSubref = 3;

        /// <summary>
        ///     Identifies this replica as a read/write replica of the naming context,
        ///     but the replica contains sparse data.
        ///     The replica has been configured to contain only specified object types
        ///     and attributes. On this type of replica, only the attributes and objects
        ///     contained in the sparse data can be modified.
        /// </summary>
        public const int LdapRtSparseWrite = 4;

        /// <summary>
        ///     Identifies this replica as a read-only replica of the naming context,
        ///     but the replica contains sparse data.
        ///     The replica has been configured to contain only specified object types
        ///     and attributes. On this type of replica, only Novell eDirectory
        ///     synchronization processes can modify the sparse data.
        /// </summary>
        public const int LdapRtSparseRead = 5;

        // Replica States

        /// <summary>
        ///     Indicates that the replica is fully functioning and capable of responding
        ///     to requests.
        /// </summary>
        public const int LdapRsOn = 0;

        /// <summary>
        ///     Indicates that a new replica has been added but has not received a full
        ///     download of information from the replica ring.
        /// </summary>
        public const int LdapRsNewReplica = 1;

        /// <summary>
        ///     Indicates that the replica is being deleted and that the request has
        ///     been received.
        /// </summary>
        public const int LdapRsDyingReplica = 2;

        /// <summary>
        ///     Indicates that the replica is locked. The move operation uses this state
        ///     to lock the parent naming context of the child naming context that is moving.
        /// </summary>
        public const int LdapRsLocked = 3;

        /// <summary>
        ///     Indicates that a new replica has finished receiving its download from the
        ///     master replica and is now receiving synchronization updates from other
        ///     replicas.
        /// </summary>
        public const int LdapRsTransitionOn = 6;

        /// <summary>
        ///     Indicates that the dying replica needs to synchronize with another replica
        ///     before being converted either to an external reference, if a root replica,
        ///     or to a subordinate reference, if a non-root replica.
        /// </summary>
        public const int LdapRsDeadReplica = 7;

        /// <summary>
        ///     Indicates that the subordinate references of the new replica are being
        ///     added.
        /// </summary>
        public const int LdapRsBeginAdd = 8;

        /// <summary>
        ///     Indicates that a naming context is receiving a new master replica.
        ///     The replica that will be the new master replica is set to this state.
        /// </summary>
        public const int LdapRsMasterStart = 11;

        /// <summary>
        ///     Indicates that a naming context has a new master replica.
        ///     When the new master is set to this state, Novell eDirectory knows
        ///     that the replica is now the master and changes its replica type to
        ///     master and the old master to read/write.
        /// </summary>
        public const int LdapRsMasterDone = 12;

        /// <summary>
        ///     Indicates that the naming context is going to split into two naming contexts.
        ///     In this state, other replicas of the naming context are informed of the
        ///     pending split.
        /// </summary>
        public const int LdapRsSs0 = 48; // Replica splitting 0

        /// <summary>
        ///     Indicates that that the split naming context operation has started.
        ///     When the split is finished, the state will change to RS_ON.
        /// </summary>
        public const int LdapRsSs1 = 49; // Replica splitting 1

        /// <summary>
        ///     Indicates that that two naming contexts are in the process of joining
        ///     into one naming context.
        ///     In this state, the replicas that are affected are informed of the join
        ///     operation. The master replica of the parent and child naming contexts are
        ///     first set to this state and then all the replicas of the parent and child.
        ///     New replicas are added where needed.
        /// </summary>
        public const int LdapRsJs0 = 64; // Replica joining 0

        /// <summary>
        ///     Indicates that that two naming contexts are in the process of joining
        ///     into one naming context.
        ///     This state indicates that the join operation is waiting for the new
        ///     replicas to synchronize and move to the RS_ON state.
        /// </summary>
        public const int LdapRsJs1 = 65; // Replica joining 1

        /// <summary>
        ///     Indicates that that two naming contexts are in the process of joining
        ///     into one naming context.
        ///     This state indicates that all the new replicas are in the RS_ON state
        ///     and that the rest of the work can be completed.
        /// </summary>
        public const int LdapRsJs2 = 66; // Replica joining 2

        // Values for flags used in the replica info class structure

        /// <summary>
        ///     Indicates that the replica is involved with a partition operation,
        ///     for example, merging a tree or moving a subtree.
        /// </summary>
        public const int LdapDsFlagBusy = 0x0001;

        /// <summary>
        ///     Indicates that this naming context is on the DNS federation boundary.
        ///     This flag is only set on DNS trees.
        /// </summary>
        public const int LdapDsFlagBoundary = 0x0002;
    }
}