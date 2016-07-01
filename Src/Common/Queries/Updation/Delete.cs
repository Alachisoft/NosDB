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
using Alachisoft.NosDB.Common.JSON;
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Updation
{
    public class Delete : IUpdation
    {
        private IEvaluable _targetField;

        public Delete(IEvaluable targetField)
        {
            _targetField = targetField;
        }

        public bool Apply(IJSONDocument document)
        {
            var attribute = _targetField as Attribute;
            if (attribute != null)
            {
                return Attributor.TryDelete(document, attribute);
            }
            return false;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
        }

        public List<Function> GetFunctions()
        {
            return new List<Function>();
        }

        public UpdateFunction Function
        {
            get { return UpdateFunction.Delete; }
        }

        public UpdateType Type
        {
            get { return UpdateType.Single; }
        }

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Delete:{");
            output.Write("TargetField=");
            if (_targetField != null)
            {
                _targetField.Print(output);
            }
            else
            {
                output.Write("null");
            }
            output.Write("}");
        }


        public string UpdateString
        {
            get
            {
                string updString = string.Empty;
                if (_targetField != null)
                    updString += "DELETE " + _targetField.InString;

                return updString;

            }
        }
    }
}
