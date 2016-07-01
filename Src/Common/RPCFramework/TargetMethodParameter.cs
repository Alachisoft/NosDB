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
//using System.Linq;
using System.Text;
using System.Collections;
#if JAVA
using Alachisoft.TayzGrid.Runtime.Serialization;
#else
using Alachisoft.NosDB.Common.Serialization;
#endif
#if JAVA
using Runtime = Alachisoft.TayzGrid.Runtime;
#else
using Runtime = Alachisoft.NosDB.Common;
#endif
namespace Alachisoft.NosDB.Common.RPCFramework
{
    [Serializable]
    public class TargetMethodParameter : ICompactSerializable
    {
        ArrayList _parameterList = new ArrayList();

        public ArrayList ParameterList
        {
            get { return _parameterList; }            
        }

        public void TargetMethodArguments()
        {
            _parameterList = new ArrayList();
        }

        public void TargetMethodArguments(ArrayList parameter)
        {
            _parameterList = parameter;
        }

        public void AddParameter(object parameter)
        {
            _parameterList.Add(parameter);
        }

        #region ICompactSerializable Members

        public void Deserialize(Common.Serialization.IO.CompactReader reader)
        {
            int length = reader.ReadInt32();
           _parameterList = new ArrayList();
             bool isByteArray = false;
            for (int i = 0; i < length; i++)
            {
                isByteArray = reader.ReadBoolean();
                if (isByteArray)
                {
                    int count = reader.ReadInt32();
                    _parameterList.Add(reader.ReadBytes(count));
                }
                else
                    _parameterList.Add(reader.ReadObject());
            }

            //_parameterList = (ArrayList)reader.ReadObject();
        }

        public void Serialize(Common.Serialization.IO.CompactWriter writer)
        {
            writer.Write(_parameterList.Count);

            bool isByteArray = false;
            for (int i = 0; i < _parameterList.Count; i++)
            {
                isByteArray = _parameterList[i] != null && _parameterList[i].GetType() == typeof(byte[]);
                writer.Write(isByteArray);
               
                if(isByteArray)
                {
                    byte[] buffer = _parameterList[i] as byte[];
                    writer.Write(buffer.Length);
                    writer.Write(buffer);
                }
                else
                    writer.WriteObject(_parameterList[i]);
            }

            //writer.WriteObject(_parameterList);
        }

        #endregion
    }
}
