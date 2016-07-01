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
using Alachisoft.NosDB.Common.Protobuf;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    public class CreateCollectionOperation : DatabaseOperation, ICreateCollectionOperation
    {
        private Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder _createCollectionCommand;
        private CollectionConfiguration _collectionConfiguration;
        private IDistributionStrategy _distribution;

        
        public CreateCollectionOperation()
        {
            _createCollectionCommand = new Alachisoft.NosDB.Common.Protobuf.CreateCollectionCommand.Builder();
            _collectionConfiguration = new CollectionConfiguration();
            _distribution = null;
            base.Message = this;
        }

        public CreateCollectionOperation(Alachisoft.NosDB.Common.Protobuf.Command command): base(command.ToBuilder())
        {
            _createCollectionCommand = command.CreateCollectionCommand.ToBuilder();

           // _collectionConfiguration.ReplicationPreference = new ReplicationPreferenceMapper().MapReplicationPreference(_collectionConfigCommand.ReplicationPreference);
            base.Message = this;
        }

       

        internal override void BuildInternal()
        {
            _createCollectionCommand.CollectionName = _collectionConfiguration.CollectionName;
            List < IndexConfiguration > IndicesList = _collectionConfiguration.Indices.IndexConfigurations.Values.ToList();
          //  List<UdfConfiguration> UdfList = _collectionConfiguration.UserDefinedFunctionConfigurations.UdfConfigurations.Values.ToList();
            foreach (IndexConfiguration iC in IndicesList)
            {
                _createCollectionCommand.IndicesConfig.CreateIndexCommandList.Add(ConvertIndexProto(iC));
            }
            
            
            
            
            base._command.SetCreateCollectionCommand(_createCollectionCommand);

            base._command.SetType(Alachisoft.NosDB.Common.Protobuf.Command.Types.Type.CREATE_COLLECTION);
        }

        
        private Protobuf.CreateIndexCommand ConvertIndexProto(IndexConfiguration indexConfig)
        {
            Protobuf.CreateIndexCommand.Builder _createIndexCommand = new CreateIndexCommand.Builder();
            _createIndexCommand.IndexName = indexConfig.Name;
            _createIndexCommand.CachePolicy = indexConfig.CachePolicy;
            _createIndexCommand.JournalEnabled = indexConfig.JournalEnabled;
            Protobuf.IndexAttributeProto.Builder _indexAttribute = new Protobuf.IndexAttributeProto.Builder() { Name = indexConfig.Attributes.Name, Order = indexConfig.Attributes.Order };
            _createIndexCommand.Attributes = _indexAttribute.Build();
            
            return _createIndexCommand.Build();
        }
        
        public override IDBOperation Clone()
        {
            //deep clone
            return base.Clone();
        }

        public override IDBResponse CreateResponse()
        {
            DatabaseResponse response = new DatabaseResponse();
            response.RequestId = base.RequestId;
            return response;
        }


        public Configuration.DOM.CollectionConfiguration Configuration
        {
            get { return _collectionConfiguration; }

            set { _collectionConfiguration=value; }

        }

        public IDistributionStrategy Distribution
        {
            get { return _distribution; }
            set { _distribution = value; }
        }
    }
}
