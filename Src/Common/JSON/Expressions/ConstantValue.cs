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

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class ConstantValue : IComparable
    {
        protected object Constant;

        protected ConstantValue() { }

        protected ConstantValue(object con)
        {
            Constant = con;
        }

        public override string ToString()
        {
            return Constant.ToString();
        }

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (Constant is NullValue && obj is NullValue)
                return 0;

            if (Constant is NullValue)
                return -1;

            if (obj is NullValue)
                return 1;
            
            if (obj is ConstantValue)
            {
                ConstantValue other = (ConstantValue)obj;
                return ((IComparable)Constant).CompareTo(other.Constant);
            }

            return ((IComparable)Constant).CompareTo(obj);
        }

        #endregion
    }
}