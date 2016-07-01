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
using Alachisoft.NosDB.Common.Configuration.DOM;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class CreateIndexOperation :DatabaseOperation, ICreateIndexOperation
    {
        //private Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder _createIndexCommand;
        private Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder _createIndexCommand;
        private IndexConfiguration _indexConfiguration;
        private bool _isAsync;


        public CreateIndexOperation()
        {
            //_createIndexCommand = new Alachisoft.NosDB.Common.Protobuf.CreateIndexCommand.Builder();
            //_indexConfiguration = new IndexConfiguration();
            _createIndexCommand = new Protobuf.CreateIndexCommand.Builder();
            base.Message = this;
        }

        public CreateIndexOperation(Alachisoft.NosDB.Common.Protobuf.Command command):base(command.ToBuilder())
        {
            //_createIndexCommand = command.CreateIndexOperation.ToBuilder();
            base.Message = this;
        }

        internal override void BuildInternal()
        {
            //build command
            _createIndexCommand.IndexName = _indexConfiguration.Name;
            _createIndexCommand.CachePolicy = _indexConfiguration.CachePolicy;
            _createIndexCommand.JournalEnabled = _indexConfiguration.JournalEnabled;


            Protobuf.IndexAttributeProto.Builder _indexAttribute = new Protobuf.IndexAttributeProto.Builder()
            {
                Name = _indexConfiguration.Attributes.Name,
                Order = _indexConfiguration.Attributes.Order
            };
            _createIndexCommand.Attributes = (_indexAttribute.Build());


            base._command.SetCreateIndexCommand(_createIndexCommand);
            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.CREATE_INDEX);



        }



        public override IDBOperation Clone()
        {
            CreateIndexOperation createIndexOperation = new CreateIndexOperation();
            createIndexOperation.Configuration = Configuration;
            createIndexOperation.Database = base.Database;
            createIndexOperation.Collection = base.Collection;
            createIndexOperation.RequestId = base.RequestId;
            createIndexOperation.NoResponse = base.NoResponse;
            createIndexOperation.Source = (Net.Address)base.Source.Clone();
            createIndexOperation.Channel = base.Channel;

            return createIndexOperation;
        }

        

        public bool IsAsync
        {
            get { return _isAsync; }
            set { _isAsync = value; }
        }

        public override IDBResponse CreateResponse()
        {
            DatabaseResponse response = new DatabaseResponse();
            response.RequestId = base.RequestId;
            return response;
        }

        public IndexConfiguration Configuration
        {
            get { return _indexConfiguration; }
            set { _indexConfiguration = value; }
        }
    }
}
