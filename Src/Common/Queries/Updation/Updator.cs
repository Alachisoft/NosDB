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
using Alachisoft.NosDB.Common.JSON.Expressions;
using Alachisoft.NosDB.Common.Server.Engine;

namespace Alachisoft.NosDB.Common.Queries.Updation
{
    public class Updator : IPrintable
    {
        private List<IUpdation> _updations = new List<IUpdation>();

        public void CreateArrayAddition(IEvaluable field, IEvaluable[] values)
        {
            _updations.Add(new Add(field, values, UpdateType.Array));
        }

        public void CreateArrayInsertion(IEvaluable field, IEvaluable[] values)
        {
            _updations.Add(new Insert(field, values, UpdateType.Array));
        }

        public void CreateArrayReplacement(IEvaluable field, KeyValuePair<IEvaluable, IEvaluable>[] _oldToNewValues)
        {
            _updations.Add(new Replace(field, _oldToNewValues, UpdateType.Array));
        }

        public void CreateArrayRemoval(IEvaluable field, IEvaluable[] values)
        {
            _updations.Add(new Remove(field, values, UpdateType.Array));
        }

        public void CreateAttributeDeletion(IEvaluable fieldName)
        {
            _updations.Add(new Delete(fieldName));
        }

        public void CreateAttributeRenaming(IEvaluable oldName, IEvaluable newName)
        {
            _updations.Add(new Rename(oldName, newName));
        }

        public void CreateUpdation(IEvaluable fieldName, IEvaluable value)
        {
            _updations.Add(new Insert(fieldName, new[] {value}, UpdateType.Single));
        }

        public bool TryUpdate(IJSONDocument source, out IJSONDocument newDocument)
        {
            newDocument = null;
            bool anyUpdationOccured = false;
            if (source != null)
            {
                newDocument = source.Clone() as IJSONDocument;

                foreach (var updation in _updations)
                {
                    if (updation.Apply(newDocument))
                        anyUpdationOccured = true;
                }
            }
            return anyUpdationOccured;
        }

        public void AssignConstants(IList<IParameter> parameters)
        {
            foreach (var update in _updations)
            {
                update.AssignConstants(parameters);
            }
        }

        public List<IUpdation> Updations
        {
            get { return _updations; }
        }
             

        public void Print(System.IO.TextWriter output)
        {
            output.Write("Updator:{Updations=[");
            for (int i = 0; i < _updations.Count; i++)
            {
                _updations[i].Print(output);
                if (i != _updations.Count - 1)
                    output.Write(",");
            }
            output.Write("]}");
        }

        public string UpdateString
        {
            get
            {
                string str = string.Empty;
                for (int i = 0; i < _updations.Count; i++)
                {
                    var updString = _updations[i].UpdateString;
                    if (!updString.Equals(string.Empty))
                        str += updString + (i + 1 == _updations.Count ? "" : ",");
                }
                return str;
            }
        }
    }
}
