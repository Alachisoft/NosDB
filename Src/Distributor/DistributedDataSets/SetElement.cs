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
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Distributor.DistributedDataSets
{
    class SetElement : ISetElement
    {
        ISet _set;
        bool _isLastElement;
        IJSONDocument _value;

        internal SetElement(ISet set, bool isLastElement, IJSONDocument document)
        {
            _set = set;
            _isLastElement = isLastElement;
            _value = document;
        }

        public ISet Set
        {
            get { return _set; }
        }

        public IJSONDocument Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public bool IsLastElement
        {
            get { return _isLastElement; }
        }
    }
}
