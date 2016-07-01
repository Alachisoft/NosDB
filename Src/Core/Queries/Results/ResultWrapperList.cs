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

namespace Alachisoft.NosDB.Core.Queries.Results
{
    public class ResultWrapperList<T> : ResultWrapper<T>
    {
        private IList<T> _list;

        public ResultWrapperList() : base(default(T))
        {
            _list = new List<T>();
        }

        public ResultWrapperList(ResultWrapper<T> wrapper) : this()
        {
            _list.Add(wrapper.Value);
            HashField = wrapper.HashField;
            SortField = wrapper.SortField;
        } 

        public IList<T> List
        {
            get { return _list; }
            set { _list = value; }
        }

        public override ResultWrapperList<T> ToList()
        {
            return this;
        }
    }
}
