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
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer.Enumerators
{
    public class BucketKeysEnumerator : IEnumerator<DocumentKey>
    {
        private Alachisoft.NosDB.Common.Server.Engine.IQueryResponse response;
        private DocumentKey currentKey;
        private bool isLastChunk;
        private IEnumerator dataChunkEnumerator;
        private string _dbName;
        private string _colName;
        private IDatabasesManager _databasesManager; 
        

        Alachisoft.NosDB.Common.Server.Engine.IDataChunk lastDataChunk = null;

        public BucketKeysEnumerator(string dbName, string colName, string queryString, NodeContext context)
        {
            _dbName = dbName;
            _colName = colName;
            if(context != null)
                _databasesManager = context.DatabasesManager;
            
            Alachisoft.NosDB.Common.Server.Engine.Impl.Query query = new Alachisoft.NosDB.Common.Server.Engine.Impl.Query();
            query.QueryText = queryString;

            Alachisoft.NosDB.Common.Server.Engine.Impl.ReadQueryOperation readQueryOperation = new Alachisoft.NosDB.Common.Server.Engine.Impl.ReadQueryOperation();
            readQueryOperation.Database = dbName;
            readQueryOperation.Collection = colName;

            readQueryOperation.Query = query;
            if (_databasesManager != null)
            {
                    response = _databasesManager.ExecuteReader(readQueryOperation);
            }

            if (response == null || !response.IsSuccessfull)
            {
                throw new Exception(Common.ErrorHandling.ErrorMessages.GetErrorMessage(response.ErrorCode)+":Query failed on collection" + colName);
            }

            if (response != null && response.DataChunk != null && response.DataChunk.Documents != null && response.DataChunk.Documents.Count > 0)
            {
                lastDataChunk = response.DataChunk;
                isLastChunk = response.DataChunk.IsLastChunk;
                this.dataChunkEnumerator = response.DataChunk.Documents.GetEnumerator();
            }
        }

        public DocumentKey Current
        {
            get { return currentKey; }
        }

        public void Dispose()
        {
            this.response = null;
        }

        object IEnumerator.Current
        {
            get { return currentKey; }
        }

        public bool MoveNext()
        {
            if (dataChunkEnumerator == null) return false;

            if (dataChunkEnumerator.MoveNext())
            {
                try
                {
                    Alachisoft.NosDB.Common.JSONDocument document = dataChunkEnumerator.Current as Alachisoft.NosDB.Common.JSONDocument;
                    
                    StateTransferKey stateTxferKey=document.Parse<StateTransferKey>();
                    currentKey = stateTxferKey != null ? stateTxferKey.DocKey : null;

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                if (this.isLastChunk) return false;

                else
                {
                    Alachisoft.NosDB.Common.Server.Engine.Impl.GetChunkOperation getChunkOperation = new Alachisoft.NosDB.Common.Server.Engine.Impl.GetChunkOperation();
                    getChunkOperation.Database = this._dbName;
                    getChunkOperation.Collection = this._colName;
                    getChunkOperation.ReaderUID = lastDataChunk.ReaderUID;
                    getChunkOperation.LastChunkId = lastDataChunk.ChunkId;

                    Alachisoft.NosDB.Common.Server.Engine.Impl.GetChunkResponse getChunkResponse = null;
                    try
                    {
                        if (_databasesManager != null)
                            getChunkResponse = (Alachisoft.NosDB.Common.Server.Engine.Impl.GetChunkResponse)_databasesManager.GetDataChunk(getChunkOperation);
                    }
                    catch (Exception)
                    {
                        return false;
                    }

                    if (getChunkResponse !=null && getChunkResponse.IsSuccessfull)
                    {
                        this.lastDataChunk = getChunkResponse.DataChunk;
                        this.isLastChunk = lastDataChunk.IsLastChunk;
                        this.dataChunkEnumerator = lastDataChunk.Documents.GetEnumerator();

                        if (dataChunkEnumerator.MoveNext())
                        {
                            try
                            {
                                Alachisoft.NosDB.Common.JSONDocument document = dataChunkEnumerator.Current as Alachisoft.NosDB.Common.JSONDocument;

                                StateTransferKey stateTxferKey = document.Parse<StateTransferKey>();
                                currentKey = stateTxferKey != null ? stateTxferKey.DocKey : null;
                                
                                return true;
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                        }

                    }
                    else
                    {
                        return false;
                    }

                }
            }
            return false;
        }

        public void Reset()
        {
            response = null;
        }
    }
}
