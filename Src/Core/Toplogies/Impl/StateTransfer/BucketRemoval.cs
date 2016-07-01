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
using System.Collections.Generic;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Logger;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Server.Engine.Impl;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    class BucketRemoval//Task : Alachisoft.NosDB.Common.Threading.IThreadPoolTask
    {
        //BucketRemovalInfo info;
        //IDatabasesManager _databasesManager;

        //public BucketRemoval(BucketRemovalInfo info, NodeContext context)
        //{
        //    this.info = info;
        //    if (context != null)
        //        _databasesManager = context.DatabasesManager;

        //}

        public static void Execute(BucketRemovalInfo info, IDatabasesManager _databasesManager, bool forLocal)
        {
            if (info.KeysEnumrator == null)
            {
                if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsDebugEnabled)
                    LoggerManager.Instance.StateXferLogger.Debug("AysncBucketRemoval.Execute", "Keys for Bucket " + info.BucketID + " is null");
                return;
            }

            var operation = new LocalDeleteOperation();
            operation.OperationType = DatabaseOperationType.StateTransferDelete;
            operation.Collection = info.Collection;
            operation.Database = info.Database;
            operation.Documents = new List<IJSONDocument>();
            operation.Context = new OperationContext();
            operation.Context.Add(Common.Enum.ContextItem.AllowCappedDelete, true);
            if (forLocal)
            {
                operation.Context.Add(ContextItem.DoNotLog, true);
            }
            while (info.KeysEnumrator.MoveNext())
            {
                var docKey = info.KeysEnumrator.Current;
                if (docKey == null) continue;


                var jdoc = new JSONDocument { Key = docKey.Value as string };


                operation.Documents.Add(jdoc);


            }
            try
            {
                IDBResponse resposne = null;
                if (_databasesManager != null)
                    resposne = _databasesManager.DeleteDocuments(operation);

                if (resposne != null && !resposne.IsSuccessfull)
                {
                    throw new DatabaseException(ErrorMessages.GetErrorMessage(resposne.ErrorCode, resposne.ErrorParams));
                }
            }
            catch (Exception ex)
            {
                //Log AsyncBucketRemovalTask.Excetue fail to delete key on state txfer completed
                LoggerManager.Instance.StateXferLogger.Error("AsyncBucketRemovalTask", ex);
            }

            //try
            //{
            //    OperationParam param = new OperationParam();
            //    param.SetParamValue(ParamName.BucketID, info.BucketID);

            //    if (_databasesManager != null)
            //        _databasesManager.OnOperationRecieved(new StateTransferOperation(info.TaskIdentity, StateTransferOpCode.RemoveLocalBucket, param));
            //}
            //catch (Exception ex)
            //{
            //    if (LoggerManager.Instance.StateXferLogger != null && LoggerManager.Instance.StateXferLogger.IsErrorEnabled)
            //        LoggerManager.Instance.StateXferLogger.Error("AysncBucketRemoval.Execute", ex.Message);
            //}
        }
    }
}