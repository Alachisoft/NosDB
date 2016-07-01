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
using System.Collections.Generic;
using System.IO;
using Alachisoft.NosDB.Common.DataStructures;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class CompoundField : Field
    {
        private LinkMap<string, Field> _individualFields;


        public CompoundField(FieldType type) : base(null, type)
        {
            _individualFields = new LinkMap<string, Field>();
        }

        public void AddField(Field field)
        {
            _individualFields.Add(field.ToString(), field);
        }
        
        public void RemoveField(string fieldId)
        {
            _individualFields.Remove(fieldId);
        }

        public bool ContainsField(string fieldId)
        {
            return _individualFields.ContainsKey(fieldId);
        }

        public Field[] Fields
        {
            get
            {
                Field[] _fields = new Field[_individualFields.Count];
                _individualFields.Values.CopyTo(_fields, 0);
                return _fields;
            }
        }

        public int FieldCount { get { return _individualFields.Count; } }

        public override bool GetAttributeValue(IJSONDocument document, out AttributeValue value)
        {
            value = NullValue.Null;
            var _listOfComparables = new List<AttributeValue>();
            _listOfComparables.Add(new SingleAttributeValue(_fieldId.ToString()));
            if (document != null)
            {
                for (int i = 0; i < _individualFields.Count; i++)
                {
                    AttributeValue obj;
                    if (!_individualFields[i].GetValue(document, out obj))
                    {
                        return false;
                    }
                    _listOfComparables.Add(obj);
                }
                value = new MultiAttributeValue(_listOfComparables);
                return true;
            }
            return false;
        }

        public override bool FillWithAttributes(IJSONDocument source, IJSONDocument target)
        {
            if (source != null && target != null)
            {
                for (int i = 0; i < _individualFields.Count; i++)
                {
                    _individualFields[i].FillWithAttributes(source, target);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (var value in _individualFields)
                {
                    hash = hash * 31 + value.GetHashCode();
                }
                return hash;
            }
        }

        public override string ToString()
        {
            string str = string.Empty;
            for (int i = 0; i < _individualFields.Count; i++)
            {
                str += _individualFields.ToString() + (i + 1 == _individualFields.Count ? "" : "|");
            }
            return str;
        }

        public override void Print(TextWriter output)
        {
            output.Write("CompoundField:{");
            output.Write("FieldType="+Type.ToString());
            output.Write(",Fields=");
            if (_individualFields != null)
            {
                output.Write("[");
                for (int i = 0; i < _individualFields.Count; i++)
                {
                    _individualFields[i].Print(output);
                    if (i != _individualFields.Count - 1)
                        output.Write(",");
                }
                output.Write("]");
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }
    }
}
