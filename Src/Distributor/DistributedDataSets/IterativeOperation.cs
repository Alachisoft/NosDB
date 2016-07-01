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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Distributor.DataCombiners;
using Alachisoft.NosDB.Distributor.DataSelectors;

namespace Alachisoft.NosDB.Distributor.DistributedDataSets
{
    /// <summary>
    /// Helps in storing data-chunks returned by shards in case of queries and ReadNext() operations
    /// </summary>
    class IterativeOperation
    {
        private IDataSelector _dataSelector;
        private string _lastReaderId;
        private int _lastSentChunkId;
        private IDataChunk _lastSentDataChunk;  // If chunk doesnot reach the client properly then it asks for it again. Therefore we keep it until the next request
        private int _autoChunkId;
        private int _numOfRequiredDocuments;    // Only required number of documents are returned by to the client

        public IterativeOperation(IList<ISet> sets, List<IDataCombiner> combiners, IDataSelector dataSelector, IComparer comparer)
        {
            _lastReaderId = null;
            _lastSentChunkId = -1;
            _dataSelector = dataSelector;
            IList<ISet> setsToBeRemoved = new List<ISet>();
            foreach (var set in sets)
            {
                if (set.IsFixedSize == true && set.GetTopElement() == null)
                {
                    setsToBeRemoved.Add(set);
                    set.DisposeReader();
                }
            }

            ListUtilMethods.RemoveMultipleItemsFromList(sets,setsToBeRemoved);

            _dataSelector.Initialize(sets, combiners, comparer);
            _lastSentDataChunk = null;
            _autoChunkId = -1;
        }

        /// <summary>
        /// Returns the DataChunk that will be sent to the client, who requested for it, inside the Response
        /// </summary>
        /// <param name="chunkId"></param>
        /// <param name="numOfRequiredDocuments"></param>
        /// <returns></returns>
        internal IDataChunk GetDataChunk(int chunkId, int numOfRequiredDocuments)
        {
            if (chunkId != _lastSentChunkId)    // If last Chunk was not received properly at client end
            { 
                if(_lastSentDataChunk == null) {throw new Exception("Invalid ChunkId"); }
                return _lastSentDataChunk;
            }

            _numOfRequiredDocuments = numOfRequiredDocuments;

            IDataChunk dataChunk = new DataChunk();
            _autoChunkId++;
            dataChunk.ChunkId = _autoChunkId;

            for (int i = 0; i < numOfRequiredDocuments; i++)
            {
                if (_dataSelector.MoveNext())
                {
                    ISetElement element = (ISetElement)_dataSelector.Current;
                    dataChunk.Documents.Add((IJSONDocument)element.Value);
                }
                else
                {
                    dataChunk.IsLastChunk = true;
                    return dataChunk;
                }
            }

            _lastSentChunkId = dataChunk.ChunkId;
            _lastSentDataChunk = dataChunk;
            return dataChunk;
        }

        internal void DiposeOperation()
        {
            _dataSelector.Reset();
        }
    }
}
