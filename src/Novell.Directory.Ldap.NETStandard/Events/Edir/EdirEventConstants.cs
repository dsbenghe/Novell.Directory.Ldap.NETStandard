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
// Novell.Directory.Ldap.Events.Edir.EdirEventConstants.cs
//
// Author:
//   Anil Bhatia (banil@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

namespace Novell.Directory.Ldap.Events.Edir
{
    /// <summary>
    ///     Enumeration for types of Edir event data
    /// </summary>
    public enum EdirEventDataType
    {
        EdirTagEntryEventData = 1,
        EdirTagValueEventData,
        EdirTagGeneralEventData,
        EdirTagSkulkData,
        EdirTagBinderyEventData,
        EdirTagDsesevInfo,
        EdirTagModuleStateData,
        EdirTagNetworkAddress,
        EdirTagConnectionState,
        EdirTagChangeServerAddress,
        EdirTagChangeConfigParam,
        EdirTagNoData,
        EdirTagStatusLog,
        EdirTagDebugEventData
    }

    /// <summary>
    ///     Enumeration for types of Edir event results
    /// </summary>
    public enum EdirEventResultType
    {
        EvtStatusAll,
        EvtStatusSuccess,
        EvtStatusFailure
    }

    /// <summary>
    ///     Enumeration for types of Edir events
    /// </summary>
    public enum EdirEventType
    {
        EvtInvalid = 0,
        EvtCreateEntry = 1,
        EvtDeleteEntry = 2,
        EvtRenameEntry = 3,
        EvtMoveSourceEntry = 4,
        EvtAddValue = 5,
        EvtDeleteValue = 6,
        EvtCloseStream = 7,
        EvtDeleteAttribute = 8,
        EvtSetBinderyContext = 9,
        EvtCreateBinderyObject = 10,
        EvtDeleteBinderyObject = 11,
        EvtCheckSev = 12,
        EvtUpdateSev = 13,
        EvtMoveDestEntry = 14,
        EvtDeleteUnusedExtref = 15,
        EvtRemoteServerDown = 17,
        EvtNcpRetryExpended = 18,
        EvtPartitionOperationEvent = 20,
        EvtChangeModuleState = 21,
        EvtDbAuthen = 26,
        EvtDbBacklink = 27,
        EvtDbBuffers = 28,
        EvtDbColl = 29,
        EvtDbDsagent = 30,
        EvtDbEmu = 31,
        EvtDbFragger = 32,
        EvtDbInit = 33,
        EvtDbInspector = 34,
        EvtDbJanitor = 35,
        EvtDbLimber = 36,
        EvtDbLocking = 37,
        EvtDbMove = 38,
        EvtDbMin = 39,
        EvtDbMisc = 40,
        EvtDbPart = 41,
        EvtDbRecman = 42,
        EvtDbResname = 44,
        EvtDbSap = 45,
        EvtDbSchema = 46,
        EvtDbSkulker = 47,
        EvtDbStreams = 48,
        EvtDbSyncIn = 49,
        EvtDbThreads = 50,
        EvtDbTimevector = 51,
        EvtDbVclient = 52,
        EvtAgentOpenLocal = 53,
        EvtAgentCloseLocal = 54,
        EvtDsErrViaBindery = 55,
        EvtDsaBadVerb = 56,
        EvtDsaRequestStart = 57,
        EvtDsaRequestEnd = 58,
        EvtMoveSubtree = 59,
        EvtNoReplicaPtr = 60,
        EvtSyncInEnd = 61,
        EvtBklinkSev = 62,
        EvtBklinkOperator = 63,
        EvtDeleteSubtree = 64,
        EvtReferral = 67,
        EvtUpdateClassDef = 68,
        EvtUpdateAttrDef = 69,
        EvtLostEntry = 70,
        EvtPurgeEntryFail = 71,
        EvtPurgeStart = 72,
        EvtPurgeEnd = 73,
        EvtLimberDone = 76,
        EvtSplitDone = 77,
        EvtSyncSvrOutStart = 78,
        EvtSyncSvrOutEnd = 79,
        EvtSyncPartStart = 80,
        EvtSyncPartEnd = 81,
        EvtMoveTreeStart = 82,
        EvtMoveTreeEnd = 83,
        EvtJoinDone = 86,
        EvtPartitionLocked = 87,
        EvtPartitionUnlocked = 88,
        EvtSchemaSync = 89,
        EvtNameCollision = 90,
        EvtNlmLoaded = 91,
        EvtLumberDone = 94,
        EvtBacklinkProcDone = 95,
        EvtServerRename = 96,
        EvtSyntheticTime = 97,
        EvtServerAddressChange = 98,
        EvtDsaRead = 99,
        EvtLogin = 100,
        EvtChgpass = 101,
        EvtLogout = 102,
        EvtAddReplica = 103,
        EvtRemoveReplica = 104,
        EvtSplitPartition = 105,
        EvtJoinPartitions = 106,
        EvtChangeReplicaType = 107,
        EvtRemoveEntry = 108,
        EvtAbortPartitionOp = 109,
        EvtRecvReplicaUpdates = 110,
        EvtRepairTimeStamps = 111,
        EvtSendReplicaUpdates = 112,
        EvtVerifyPass = 113,
        EvtBackupEntry = 114,
        EvtRestoreEntry = 115,
        EvtDefineAttrDef = 116,
        EvtRemoveAttrDef = 117,
        EvtRemoveClassDef = 118,
        EvtDefineClassDef = 119,
        EvtModifyClassDef = 120,
        EvtResetDsCounters = 121,
        EvtRemoveEntryDir = 122,
        EvtCompareAttrValue = 123,
        EvtStream = 124,
        EvtListSubordinates = 125,
        EvtListContClasses = 126,
        EvtInspectEntry = 127,
        EvtResendEntry = 128,
        EvtMutateEntry = 129,
        EvtMergeEntries = 130,
        EvtMergeTree = 131,
        EvtCreateSubref = 132,
        EvtListPartitions = 133,
        EvtReadAttr = 134,
        EvtReadReferences = 135,
        EvtUpdateReplica = 136,
        EvtStartUpdateReplica = 137,
        EvtEndUpdateReplica = 138,
        EvtSyncPartition = 139,
        EvtSyncSchema = 140,
        EvtCreateBacklink = 141,
        EvtCheckConsoleOperator = 142,
        EvtChangeTreeName = 143,
        EvtStartJoin = 144,
        EvtAbortJoin = 145,
        EvtUpdateSchema = 146,
        EvtStartUpdateSchema = 147,
        EvtEndUpdateSchema = 148,
        EvtMoveTree = 149,
        EvtReloadDs = 150,
        EvtAddProperty = 151,
        EvtDeleteProperty = 152,
        EvtAddMember = 153,
        EvtDeleteMember = 154,
        EvtChangePropSecurity = 155,
        EvtChangeObjSecurity = 156,
        EvtConnectToAddress = 158,
        EvtSearch = 159,
        EvtPartitionStateChg = 160,
        EvtRemoveBacklink = 161,
        EvtLowLevelJoin = 162,
        EvtCreateNamebase = 163,
        EvtChangeSecurityEquals = 164,
        EvtDbNcpeng = 166,
        EvtCrcFailure = 167,
        EvtAddEntry = 168,
        EvtModifyEntry = 169,
        EvtOpenBindery = 171,
        EvtCloseBindery = 172,
        EvtChangeConnState = 173,
        EvtNewSchemaEpoch = 174,
        EvtDbAudit = 175,
        EvtDbAuditNcp = 176,
        EvtDbAuditSkulk = 177,
        EvtModifyRdn = 178,
        EvtEntryidSwap = 181,
        EvtInsideNcpRequest = 182,
        EvtDbLostEntry = 183,
        EvtDbChangeCache = 184,
        EvtLowLevelSplit = 185,
        EvtDbPurge = 186,
        EvtEndNamebaseTransaction = 187,
        EvtAllowLogin = 188,
        EvtDbClientBuffers = 189,
        EvtDbWanman = 190,
        EvtLocalReplicaChange = 197,
        EvtDbDrl = 198,
        EvtMoveEntrySource = 199,
        EvtMoveEntryDest = 200,
        EvtNotifyRefChange = 201,
        EvtDbAlloc = 202,
        EvtConsoleOperation = 203,
        EvtDbServerPacket = 204,
        EvtDbObit = 207,
        EvtReplicaInTransition = 208,
        EvtDbSyncDetail = 209,
        EvtDbConnTrace = 210,
        /*
        EVT_CHANGE_CONFIG_PARM = 211,
        EVT_COMPUTE_CONN_SEV_INLINE = 212,
        */
        EvtBeginNamebaseTransaction = 213,
        EvtDbDirxml = 214,
        EvtVrDriverStateChange = 215,
        EvtReqUpdateServerStatus = 216,
        EvtDbDirxmlDrivers = 217,
        EvtDbNdsmon = 218,
        EvtChangeServerAddrs = 219,
        EvtDbDns = 220,
        EvtDbRepair = 221,
        EvtDbRepairDebug = 222,
        EvtIterator = 224,
        EvtDbSchemaDetail = 225,
        EvtLowLevelJoinBegin = 226,
        EvtDbInSyncDetail = 227,
        EvtPreDeleteEntry = 228,
        EvtDbSsl = 229,
        EvtDbPki = 230,
        EvtDbHttpstk = 231,
        EvtDbLdapstk = 232,
        EvtDbNiciext = 233,
        EvtDbSecretStore = 234,
        EvtDbNmas = 235,
        EvtDbBacklinkDetail = 236,
        EvtDbDrlDetail = 237,
        EvtDbObjectProducer = 238,
        EvtDbSearch = 239,
        EvtDbSearchDetail = 240,
        EvtStatusLog = 241,
        EvtDbNpkiApi = 242,
        EvtMaxEvents
    }

    /// <summary>
    ///     Enumeration for types of Edir event Debug parameters
    /// </summary>
    public enum DebugParameterType
    {
        Entryid = 1,
        String,
        Binary,
        Integer,
        Address,
        Timestamp,
        Timevector
    }

    /// <summary>
    ///     Enumeration for fields of Edir General event
    /// </summary>
    public enum GeneralEventField
    {
        EvtTagGenDstime = 1,
        EvtTagGenMillisec,
        EvtTagGenVerb,
        EvtTagGenCurrproc,
        EvtTagGenPerp,
        EvtTagGenIntegers,
        EvtTagGenStrings
    }

    public class EventOids
    {
        /* Oid for requests */

        public const string NldapMonitorEventsRequest =
            "2.16.840.1.113719.1.27.100.79";

        public const string NldapMonitorEventsResponse =
            "2.16.840.1.113719.1.27.100.80";

        public const string NldapEventNotification =
            "2.16.840.1.113719.1.27.100.81";

        public const string NldapFilteredMonitorEventsRequest =
            "2.16.840.1.113719.1.27.100.84";
    }
}