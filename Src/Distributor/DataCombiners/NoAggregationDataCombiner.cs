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
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Distributor.DataCombiners
{
    public class NoAggregationDataCombiner : IDataCombiner
    {
        private IJSONDocument _document;
        private string _combinerName;

        public NoAggregationDataCombiner(IEnumerable<IEvaluable> attributes)
        {
            _combinerName = "NoAggregationDataCombiner";
        }

        #region IDataCombiner Methods
        public object Initialize(object initialValue)
        {
            if (!(initialValue is IJSONDocument)) throw new Exception("At NoAggregationDataCombiner: Document needs to be in IJSONDocument format");
            
            _document = (IJSONDocument)initialValue;
            return _document;
        }

        public object Combine(object value)
        {
            if (!(value is IJSONDocument)) throw new Exception("At NoAggregationDataCombiner: Document needs to be in IJSONDocument format");

            return _document;
        }

        public void Reset()
        {
            _document = new JSONDocument();
        }

        public string Name
        {
            get { return _combinerName; }
        }

        #endregion
    }
}
