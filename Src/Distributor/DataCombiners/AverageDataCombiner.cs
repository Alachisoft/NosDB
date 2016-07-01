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
    public class AverageDataCombiner : IDataCombiner
    {
        private IJSONDocument _document;
        private readonly IEnumerable<IEvaluable> _attributes;
        //private string _combinerName;
        private string _userDefinedName;

        public AverageDataCombiner(IEnumerable<IEvaluable> attributes, string userDefinedName,bool aliasExists)
        {
            _attributes = attributes;
            if (aliasExists)
            {
                _userDefinedName = userDefinedName;
                //_combinerName = userDefinedName;
            }
            else
            {
                //_combinerName = "avg(";
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
            if (!(initialValue is IJSONDocument)) throw new Exception("At AverageDataCombiner: Document needs to be in IJSONDocument format");

            IJSONDocument document = (IJSONDocument)initialValue;
            foreach (var attribute in _attributes)
            {
                double average;
                var type = document.GetAttributeDataType(_userDefinedName);
                if (type.Equals(ExtendedJSONDataTypes.Array))
                {
                    double[] sumAndCountDoc = document.GetArray<double>(_userDefinedName);
                    document.Add("$" + _userDefinedName, sumAndCountDoc);
                    double sum = sumAndCountDoc[0]; //.GetAsDouble("$SUM");
                    double count = sumAndCountDoc[1]; //.GetAsDouble("$COUNT");
                    average = sum/count;
                }
                else
                {
                    average = 0;
                }
                document[_userDefinedName] = average;
            }
            _document = document;
            return document;
        }

        public object Combine(object value)
        {
            if (!(value is IJSONDocument)) throw new Exception("At AverageDataCombiner: Document needs to be in IJSONDocument format");

            bool isUpdateApplicable = false;
            IJSONDocument document = (IJSONDocument)value;
            IJSONDocument updateDoc = new JSONDocument();
            foreach (var attribute in _attributes)
            {
                IJSONDocument sumAndCount = new JSONDocument();
                if (document.GetAttributeDataType(_userDefinedName) == ExtendedJSONDataTypes.Array)
                {
                    if (_document.Contains("$" + _userDefinedName) && _document.GetAttributeDataType("$" + _userDefinedName) == ExtendedJSONDataTypes.Array)
                    {
                        double[] sumAndCountDoc = document.GetArray<double>(_userDefinedName);
                        double sum = sumAndCountDoc[0];
                        double count = sumAndCountDoc[1];

                        double[] existingSumAndCount = _document.GetArray<double>("$" + _userDefinedName);
                        double sum1 = existingSumAndCount[0];
                        double count1 = existingSumAndCount[1];

                        double combinedSum = sum + sum1;
                        double combinedCount = count + count1;

                        sumAndCountDoc[0] = combinedSum;
                        sumAndCountDoc[1] = combinedCount;

                        updateDoc.Add("$" + _userDefinedName, sumAndCountDoc);
                        updateDoc.Add(_userDefinedName, combinedSum / combinedCount);
                        isUpdateApplicable = true;
                    }
                }
            }

            if(isUpdateApplicable)
                JsonDocumentUtil.Update(_document, updateDoc);
            return _document;
        }

        public void Reset()
        {
            _document = new JSONDocument();
        }

        public string Name
        {
            get { return _userDefinedName; }
        }
        #endregion
    }
}
