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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Core.Storage.Queries.Util;

namespace Alachisoft.NosDB.Common.JSON
{
    public class BooleanJsonValue : IJsonValue
    {
        private bool _value;

        public BooleanJsonValue(bool value)
        {
            _value = value;
        }
        public FieldDataType DataType
        {
            get { return FieldDataType.Bool; }
        }

        public object Value
        {
            get { return _value; }
        }

        public int CompareTo(object obj)
        {
            IJsonValue secondJsonValue = obj as IJsonValue;
            if (secondJsonValue == null) throw new ArgumentException("Object must be of type IJSONValue");

            if (obj is EmbeddedList)
                return EmbeddedHelper.Compare(this, obj as EmbeddedList);

            return JSONComparer.Compare(_value, DataType, NativeType, secondJsonValue.Value, secondJsonValue.DataType,
                secondJsonValue.NativeType, SortOrder.ASC);
        }

        public override bool Equals(object obj)
        {
            BooleanJsonValue secondJsonValue = obj as BooleanJsonValue;
            if (secondJsonValue == null)
                return false;

            return _value.Equals(secondJsonValue.Value);
        }
        
        public TypeCode NativeType
        {
            get { return TypeCode.Boolean; }
        }
    }
}
