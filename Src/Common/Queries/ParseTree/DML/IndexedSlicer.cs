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
using Alachisoft.NosDB.Common.Enum;
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.ParseTree.DML
{
    public class IndexedSlicer : IEvaluable
    {
        private readonly int _start;
        private readonly int _items;

        private readonly IEvaluable _expression;

        public IndexedSlicer(IEvaluable expression, int start, int items)
        {
            _start = start;
            _items = items;
            _expression = expression;
        }

        #region IEvaluable members.

        public string InString
        {
            get { return "(" + _expression.InString + ") SLICE (" + _start + "," + _items + ")"; }
        }

        public string CaseSensitiveInString
        {
            get { return "(" + _expression.CaseSensitiveInString + ") SLICE (" + _start + "," + _items + ")"; }
        }
        public EvaluationType EvaluationType
        {
            get
            {
                return EvaluationType.AllVariable;
            }
        }

        public List<Common.JSON.Expressions.Attribute> Attributes
        {
            get { return _expression.Attributes; }
        }

        public List<Function> Functions
        {
            get { return _expression.Functions; }
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            _expression.AssignConstants(parameters);
        }

        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            value = null;
            IJsonValue outJsonValue;
            //int adjustedItems = _items+1; //slice (1,0) means first item

            if (!_expression.Evaluate(out outJsonValue, document))
            {
                return false;
            }

            if (outJsonValue.DataType != FieldDataType.Array && outJsonValue.DataType != FieldDataType.Embedded)
            {
                return false;
            }

             ArrayJsonValue arrayValue;

             if (outJsonValue.DataType == FieldDataType.Embedded)
                 arrayValue = new ArrayJsonValue(((EmbeddedList)outJsonValue).WrapedValue.ToArray());
             else
                arrayValue = (ArrayJsonValue)outJsonValue;

             int sourceIndex = (_start > 0) ? _start - 1 : (arrayValue.Length + _start) - _items + 1;
            if (sourceIndex < 0)
                return false;

            if (arrayValue.Length < sourceIndex + _items) 
            {
                return false;
            }


            IJsonValue[] returnArray = new IJsonValue[_items];

            Array.Copy(arrayValue.WrapedValue, sourceIndex, returnArray, 0, _items);

            value = new ArrayJsonValue(returnArray);
            return true;
        }

        public IComparable Add(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Subtract(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Multiply(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Divide(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public IComparable Modulate(IEvaluable evaluable, IJSONDocument document)
        {
            return null;
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("IndexedSlicer:{");
            output.Write(_expression.InString + 
                " SLICE:<start:" + _start + "," + "items:" + _items + ">" );
            output.Write("}");
        }

        public override string ToString()
        {
            return _expression.ToString(); 
        }

        #endregion


    }
}
