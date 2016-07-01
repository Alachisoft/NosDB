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

namespace Alachisoft.NosDB.Common.JSON.Indexing
{
    public class BoundaryValueMask : AttributeValueMask
    {
        private Bound boundType;
        private AttributeValue state;

        public BoundaryValueMask(FieldDataType type, Bound bound) : base(type)
        {
            boundType = bound;
        }

        public AttributeValue State
        {
            get { return state; }
        }

        public override IComparable this[int index]
        {
            get { return state != null ? state[0] : null; }
        }

        public override int CompareTo(object obj)
        {
            int typeRule = base.CompareTo(obj);
            if (typeRule != 0)
                return typeRule;
            state = (AttributeValue) obj;

            switch (boundType)
            {
                case Bound.Max:
                    return 1;
                case Bound.Min:
                    return -1;
                default:
                    return 0;
            }
        }

        public override bool Equals(object obj)
        {
            if (CompareTo(obj) != 0)
                return false;
            return true;
        }

        public override object Clone()
        {
            return new BoundaryValueMask(type, boundType);
        }
    }
}