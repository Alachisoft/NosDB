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
using System;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.Serialization;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Distribution
{
    /// <summary>
    /// As the name suggests, this provides a mechanism to idenfity to which
    /// shard a document belongs according the current distribution 
    /// </summary>
    public interface IDistributionRouter : ICompactSerializable
    {
        /// <summary>
        /// Gets the bucket to which a document belongs
        /// </summary>
        /// <param name="documentKey">Document key</param>
        /// <returns>Bucket</returns>
        HashMapBucket GetBucketForDocument(DocumentKey documentKey);

        /// <summary>
        /// Gets the bucket to which a document belongs
        /// </summary>
        /// <param name="documentKey">Document key</param>
        /// <returns>Bucket</returns>
        Int32 GetBucketID(DocumentKey documentKey);

        /// <summary>
        /// Gets the shard name to which given document belongs
        /// </summary>
        /// <param name="documentKey"></param>
        /// <returns>Name of the shard</returns>
        string GetShardForDocument(DocumentKey documentKey);    //: Expecting this to throw an exception if the shard doesnot exist against the provided documentkey
    }
}
