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
using Alachisoft.NosDB.Common.Serialization;
using System.Collections.Generic;

namespace Alachisoft.NosDB.Common.Configuration.Services
{
    public class ConfigChangeEventArgs : ICompactSerializable
    {
        IDictionary<EventParamName, Object> eventParameters;

        public IDictionary<EventParamName,Object> EventParameters
        {
            get { return eventParameters; }
        }

        public void SetParamValue(EventParamName name, Object value)
        {
            if (eventParameters == null)
                eventParameters = new Dictionary<EventParamName, Object>();
            eventParameters[name] = value;
        }

        public void RemoveParam(EventParamName name)
        {
            if (eventParameters != null)
                eventParameters.Remove(name);
        }

        public T GetParamValue<T>(EventParamName name)
        {
            if (eventParameters != null)
                return (T)eventParameters[name];
            return default(T);
        }
        public ConfigChangeEventArgs(string clusterName, String shardName, ChangeType type)
            : this(clusterName, type)
        {
            SetParamValue(EventParamName.ShardName, shardName);
        }

        public ConfigChangeEventArgs(string clusterName, ChangeType type)
        {
            SetParamValue(EventParamName.ClusterName, clusterName);
            SetParamValue(EventParamName.ConfigurationChangeType, type);
            //this.ClusterName = clusterName;
            //this.ConfigurationChangeType = type;
        }

        public ConfigChangeEventArgs()
        {

        }

        #region ICompactSerializable Members

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            eventParameters = (IDictionary<EventParamName, Object>)reader.ReadObject();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.WriteObject(eventParameters);
        }

        #endregion

    }
}
