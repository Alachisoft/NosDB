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
namespace Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer
{
    /// <summary>
    /// Specifies how data is to be transferred from a source node to a target node.
    /// </summary>
    public enum StateTransferType:byte
    {
        /// <summary>
        /// Data is to be transferred from source node to target node and
        /// at the completion of transfer it is removed from the source node.
        /// </summary>
        INTER_SHARD,

        /// <summary>
        /// Data is replicated from source node to the target node.
        /// </summary>
        INTRA_SHARD,

    }
}