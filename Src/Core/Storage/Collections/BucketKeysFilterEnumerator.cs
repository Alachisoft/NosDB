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
using System.Collections;
using System.Collections.Generic;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
using Alachisoft.NosDB.Core.Storage.Indexing;

namespace Alachisoft.NosDB.Core.Storage.Collections
{
    public class BucketKeysFilterEnumerator : IEnumerator<DocumentKey>
    {
        private int bucketId;
        private IEnumerator<long> rowIDsEnumerator;
        private IDistribution distribution;
        private MetadataIndex metadataIndex;

        private DocumentKey currentKey = null;

        public BucketKeysFilterEnumerator(int bucketId, MetadataIndex metadataIndex, IDistribution distribution)
        {
            if (metadataIndex == null) throw new Exception("Metadata Index can not be null");

            this.bucketId = bucketId;
            this.metadataIndex = metadataIndex;
            this.rowIDsEnumerator = metadataIndex.GetEnumerator();
            this.distribution = distribution;
            if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
            {
                LoggerManager.Instance.StateXferLogger.Debug("StateXfer", "BucketKeysFilterEnumerator created for BucketId: " + bucketId);
            }
        }

        public DocumentKey Current
        {
            get { return currentKey; }
        }

        public void Dispose()
        {
            if (rowIDsEnumerator != null)
                rowIDsEnumerator.Dispose();
        }

        object IEnumerator.Current
        {
            get { return currentKey; }
        }

        public bool MoveNext()
        {
            while (rowIDsEnumerator.MoveNext())
            {
                long rowID = rowIDsEnumerator.Current;
                DocumentKey key = metadataIndex.GetDocKey(rowID);

                if (key != null)
                {
                    if (distribution != null && distribution.GetDistributionRouter() != null && distribution.GetDistributionRouter().GetBucketForDocument(key).BucketId == bucketId)
                    {
                        this.currentKey = key;
                        return true;
                    }
                }
            }
            if (LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
            {
                LoggerManager.Instance.StateXferLogger.Debug("StateXfer", "MoveNext() for BucketId: " + bucketId + "finished data");
            }
            return false;
        }

        public void Reset()
        {
            rowIDsEnumerator.Reset();
        }
    }
}