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
using Alachisoft.NosDB.Common.Serialization;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.StateTransfer.Operations
{
    /// <summary>
    /// OperationParam for State Transfer Operations, serve  as a container for different parameters
    /// </summary>
    public class OperationParam:ICompactSerializable
    {
        IDictionary<ParamName,Object> parameters = null;

        public void SetParamValue(ParamName name, Object value)
        {
            if (parameters == null) parameters = new Dictionary<ParamName, Object>();
            parameters[name] = value;
        }

        public void RemoveParam(ParamName name)
        {
            if (parameters != null)
                parameters.Remove(name);
        }

        public Object GetParamValue(ParamName name)
        {
            if (parameters != null)
                return parameters[name];

            return null;
        }

        #region ICompactSerializable Implementation
        
        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            parameters = Alachisoft.NosDB.Common.Util.SerializationUtility.DeserializeDictionary<ParamName, Object>(reader);
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            Alachisoft.NosDB.Common.Util.SerializationUtility.SerializeDictionary(parameters, writer);
        }

        #endregion
    }
}
