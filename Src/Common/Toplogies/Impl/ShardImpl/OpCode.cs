// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
namespace Alachisoft.NosDB.Common.Toplogies.Impl.ShardImpl
{
    public enum OpCode
    {
        CreateCollection,
        DropCollection,
        CreateIndex,
        DropIndex,
        GetReplicationChunk,
        GetMinorOperations,
        //-
        PrimaryChanged,

        /// <summary>
        /// State Transfer Operation OpCode
        /// </summary>
        StateTransferOperation,

        /// <summary>
        /// State Transfer Operation OpCode
        /// </summary>
        ShardConnected,


        /// <summary>
        /// Get Last Operation from op-log to start replication
        /// </summary>
        GetLastOperation,

        GatherRollbackStartPointInfo,
        GetRollbackStartPoint,
        PrepareReplicationOperationsReader,
        PulseFromPrimary,
        RestrictPrimary,


        /// <summary>
        /// Insert Document on the target participant.
        /// </summary>
        InsertTaskOutputDocuments,
    
        /// <summary>
        /// Get Document used by Data readers in case of state transfer.
        /// </summary>
        GetDocumentFromPrimary
    }
}
