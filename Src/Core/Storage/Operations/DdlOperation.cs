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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Core.Storage.Operations
{
    public class DdlOperation
    {
        private DdlOperationType _ddlOperationType;
        private IDBOperation _dbOperation;
        private IDBResponse _dbResponse;

        public DdlOperation(DdlOperationType ddlOperationType,IDBOperation dbOperation)
        {
            _ddlOperationType = ddlOperationType;
            _dbOperation = dbOperation;
        }

        public IDBResponse CreateDbResponse()
        {
            _dbResponse = _dbOperation.CreateResponse();
            return _dbResponse;
        }

        public DdlOperationType DdlOperationType
        {
            get { return _ddlOperationType; }
            set { _ddlOperationType = value; }
        }

        public IDBOperation DbOperation
        {
            get { return _dbOperation; }
            set { _dbOperation = value; }
        }

        public IDBResponse DbResponse
        {
            get { return _dbResponse; }
            set { _dbResponse = value; }
        }
    }
}
