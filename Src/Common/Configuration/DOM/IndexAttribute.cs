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
using Alachisoft.NosDB.Common.Configuration;
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.Serialization;
using System;

using Alachisoft.NosDB.Common.Server.Engine;
using Newtonsoft.Json;


namespace Alachisoft.NosDB.Common.Configuration.DOM
{

    public class IndexAttribute : ICloneable, ICompactSerializable, IIndexAttribute
    {
        private string _name;
        private SortOrder _sortOrder = SortOrder.ASC;

        [ConfigurationAttribute("name")]
        [JsonProperty(PropertyName = "Name")]
        public string Name 
        {
            get { return _name; }
            set { _name = value; }
        }

        [ConfigurationAttribute("sort-order")]
        [JsonProperty(PropertyName = "Order")]
        public string Order
        {
            get { return _sortOrder == Common.Enum.SortOrder.DESC ? "DESC" : "ASC"; }
            set { _sortOrder = value.Equals("DESC", StringComparison.CurrentCultureIgnoreCase) ? Common.Enum.SortOrder.DESC : Common.Enum.SortOrder.ASC; }
        }

        [JsonProperty(PropertyName = "SortOrder")]
        public SortOrder SortOrder
        {
            get { return _sortOrder; }
            set { _sortOrder = value;}
        }

        public override string ToString()
        {
            return _name;
        }

        public static void ValidateConfiguration(IndexAttribute configuration)
        {
            if (configuration == null)
                throw new Exception("Index attribute cannot be null.");
            if (configuration.Name == null)
                throw new Exception("Index attribute name cannot be null.");
            if (configuration.Name.Trim() == "")
                throw new Exception("Index attribute name cannot be empty string.");
        }

        #region ICompactSerializable Members
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            Name = reader.ReadObject() as string;
            Order = reader.ReadObject() as string;
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(Name);
            writer.WriteObject(Order);
        }
        #endregion

        #region ICloneable Member
        public object Clone()
        {
            IndexAttribute indexAttribute = new IndexAttribute();
            indexAttribute.Name = Name;
            indexAttribute.Order = Order;
            indexAttribute.SortOrder = SortOrder == SortOrder.ASC ? SortOrder.ASC : SortOrder.DESC;
            return indexAttribute;
        }
        #endregion
    }
}
