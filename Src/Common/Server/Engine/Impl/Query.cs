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
using Alachisoft.NosDB.Common.JSON.CustomConverter;
using Alachisoft.NosDB.Common.Serialization;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Common.Server.Engine.Impl
{
    [JsonConverter(typeof(QueryConverter))]
    public class Query : IQuery, ICompactSerializable
    {
        private IList<IParameter> _parameters = new List<IParameter>();

        [JsonProperty(PropertyName = "Query")]
        public string QueryText { get; set; }

        public IList<IParameter> Parameters
        {
            get { return (IList<IParameter>)_parameters; }
            set { _parameters = value ?? new List<IParameter>();}
        }

        #region ICompactSerializable
        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            QueryText = reader.ReadString();
            Parameters = reader.ReadObject() as IList<IParameter>;
            if (Parameters == null)
                Parameters = new List<IParameter>();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(QueryText);
            writer.WriteObject(Parameters);
        }
        #endregion
    }
}
