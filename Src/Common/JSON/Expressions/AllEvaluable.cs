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
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.Common.JSON.Expressions
{
    public class AllEvaluable : IEvaluable
    {
        #region IEvaluable members.

        public string InString
        {
            get { return ToString(); }
        }

        public string CaseSensitiveInString
        {
            get { return ToString(); }
        }
        public bool Evaluate(out IJsonValue value, IJSONDocument document)
        {
            List<IJsonValue> allValues = new List<IJsonValue>();

            foreach (var attribute in document.GetAttributes())
            {
                allValues.Add(JsonWrapper.Wrap(document[attribute]));
            }

            value = new ArrayJsonValue(allValues.ToArray());
            return true;
        }

        public IComparable Add(IEvaluable evaluable, Server.Engine.IJSONDocument document)
        {
            return null;
        }

        public IComparable Subtract(IEvaluable evaluable, Server.Engine.IJSONDocument document)
        {
            return null;
        }

        public IComparable Multiply(IEvaluable evaluable, Server.Engine.IJSONDocument document)
        {
            return null;
        }

        public IComparable Divide(IEvaluable evaluable, Server.Engine.IJSONDocument document)
        {
            return null;
        }

        public IComparable Modulate(IEvaluable evaluable, Server.Engine.IJSONDocument document)
        {
            return null;
        }

        public void AssignConstants(IList<Server.Engine.IParameter> parameters)
        {

        }

        public EvaluationType EvaluationType
        {
            get { return EvaluationType.MultiVariable; }
        }

        public List<Attribute> Attributes
        {
            get { return new List<Attribute>();}
        }

        public List<Function> Functions
        {
            get { return new List<Function>();}
        }

        #endregion

        public void Print(System.IO.TextWriter output)
        {
            output.Write("AllEvaluable");
        }

        public override string ToString()
        {
            return "*";
        }


    }
}
