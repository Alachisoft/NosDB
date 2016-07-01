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
using Alachisoft.NosDB.Common.JSON.Indexing;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Results
{
    public class AllField : Field
    {
        private AttributeValue attvalue;

        public AllField(FieldType type) : base(null, type)
        {
            var _attributeList = new List<AttributeValue>();
            _attributeList.Add(new SingleAttributeValue(_fieldId.ToString()));
            _attributeList.Add(new AllValue());
            attvalue = new MultiAttributeValue(_attributeList);
        }

        public override bool Equals(object obj)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return int.MinValue;
        }

        public override bool GetAttributeValue(IJSONDocument document, out AttributeValue value)
        {
            value = attvalue;
            return true;
        }

        public override bool FillWithAttributes(IJSONDocument source, IJSONDocument target)
        {
            //if (target != null)
            //{
            //    foreach (var attribute in source.GetAttributes())
            //    {
            //        target[attribute] = source[attribute];
            //    }
            //}
            return true;
        }

        public override string ToString()
        {
            return "*";
        }

        public override void Print(TextWriter output)
        {
            output.Write("AllField");
        }
    }
}
