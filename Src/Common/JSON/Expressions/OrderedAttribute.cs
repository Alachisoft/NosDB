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

using System.IO;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class OrderedAttribute : Attribute
    {
        readonly SortOrderConstant _order;

        public OrderedAttribute(string attrib, Common.Enum.SortOrder order = Common.Enum.SortOrder.ASC)
            : base(attrib)
        {
            _order = new SortOrderConstant(order);
        }

        public OrderedAttribute(Attribute attrib, SortOrderConstant order)
            : this(attrib.ToString())
        {
            _order = order;
        }

        public SortOrderConstant Order
        {
            get { return _order; }
        }

        public override void Print(TextWriter output)
        {
            output.Write("OrderedAttribute:{"+_order.ToString()+"}");
        }
    }
}
