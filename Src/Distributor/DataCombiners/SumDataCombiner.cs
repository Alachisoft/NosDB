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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Distributor.DataCombiners
{
    class SumDataCombiner : IDataCombiner
    {
        private IJSONDocument _document;
        private readonly IEnumerable<IEvaluable> _attributes;
        //private string _combinerName;
        private string _userDefinedName;

        public string Name
        {
            get { return _userDefinedName; }
        }

        public SumDataCombiner(IEnumerable<IEvaluable> attributes, string userDefinedName, bool aliasExists)
        {
            _attributes = attributes;

            if (aliasExists)
            {
                _userDefinedName = userDefinedName;
                //_combinerName = userDefinedName;
            }
            else
            {
                //_combinerName = userDefinedName + "(";
                _userDefinedName = userDefinedName + "(";
                bool isFirst = true;
                foreach (var attribute in _attributes)
                {
                    if (!isFirst)
                    {
                        //_combinerName += ",";
                        _userDefinedName += ",";
                    }
                    //_combinerName = _combinerName + attribute;
                    _userDefinedName = _userDefinedName + attribute;
                    isFirst = false;
                }
                //_combinerName += ")";
                _userDefinedName += ")";
            }
        }

        #region IDataCombiner Methods
        public object Initialize(object initialValue)
        {
            if (!(initialValue is IJSONDocument)) throw new Exception("At SumDataCombiner: Document needs to be in IJSONDocument format");
            
            IJSONDocument document = (IJSONDocument)initialValue;
            foreach (var attribute in _attributes)
            {
                double value1 = document.GetAsDouble(_userDefinedName);
                document[_userDefinedName] = value1;
            }
            _document = document;
            return _document;
        }

        public object Combine(object value)
        {
            if (!(value is IJSONDocument)) throw new Exception("At SumDataCombiner: Document needs to be in IJSONDocument format");

            IJSONDocument document = (IJSONDocument)value;
            IJSONDocument updateDoc = new JSONDocument(); 
            foreach (var attribute in _attributes)
            {
                double value1 = document.GetAsDouble(_userDefinedName);
                double value2 = _document.GetAsDouble(_userDefinedName);
                updateDoc.Add(_userDefinedName, value1 + value2);
            }
            JsonDocumentUtil.Update(_document, updateDoc);
            //_document.Update(updateDoc);
            return _document;
        }

        public void Reset()
        {
            _document = new JSONDocument();
        }

        #endregion
    }
}
