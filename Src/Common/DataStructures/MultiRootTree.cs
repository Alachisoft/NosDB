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
using System.Text;
using System.Collections;

namespace Alachisoft.NosDB.Common.DataStructures
{

    public class MultiRootTree
    {
        //Act as attribute name
        string _currentAttribute;
        List<string> _attributeNames;
        //contains attributeValue-listofkeys/subtree
        Hashtable _ht;
        int _levels;

        public int Levels
        {
            get { return _levels; }
        }

        public MultiRootTree(int levels, List<string> attributeNames)
        {
            _ht = new Hashtable();
            _levels = levels;
            _attributeNames = attributeNames;
            _currentAttribute = attributeNames[attributeNames.Count - _levels];
        }

        public void Add(KeyValuesContainer value)
        {
            if (_levels == 1)
            {
                if (_ht[value.Values[_currentAttribute]] is ArrayList)
                    ((ArrayList)_ht[value.Values[_currentAttribute]]).Add(value.Key);
                else
                {
                    ArrayList al = new ArrayList();
                    al.Add(value.Key);
                    _ht[value.Values[_currentAttribute]] = al;
                }
            }
            else
            {
                if (_ht[value.Values[_currentAttribute]] is MultiRootTree)
                    ((MultiRootTree)_ht[value.Values[_currentAttribute]]).Add(value);
                else
                {
                    MultiRootTree mrt = new MultiRootTree(_levels - 1, _attributeNames);
                    mrt.Add(value);
                    _ht[value.Values[_currentAttribute]] = mrt;

                }
            }
        }

        private int GenerateRecordSet(RecordSet recordSet)
        {
            IDictionaryEnumerator ide = _ht.GetEnumerator();
            int rowsAdded = 0;
            while (ide.MoveNext())
            {
                if (ide.Value is ArrayList)
                {
                    rowsAdded++;
                    recordSet.AddRow();
                    if (recordSet.GetColumnDataType(_currentAttribute) == ColumnDataType.Object)
                        recordSet.SetColumnDataType(_currentAttribute, RecordSet.ToColumnDataType(ide.Key));
                    recordSet.Add(ide.Key, recordSet.RowCount - 1, _currentAttribute);
                    recordSet.Add(ide.Value, recordSet.RowCount - 1, recordSet.ColumnCount - 1);
                }
                else
                {
                    rowsAdded = (ide.Value as MultiRootTree).GenerateRecordSet(recordSet);
                    for (int i = 0; i < rowsAdded; i++)
                    {
                        recordSet.Add(ide.Key, recordSet.RowCount - i - 1, _currentAttribute);
                        if (recordSet.GetColumnDataType(_currentAttribute) == ColumnDataType.Object)
                            recordSet.SetColumnDataType(_currentAttribute, RecordSet.ToColumnDataType(ide.Key));
                    }
                }
            }
            return rowsAdded;
        }
        
        public void ToRecordSet(RecordSet recordSet)
        {
            recordSet.AddColumn("",true);
            GenerateRecordSet(recordSet);
        }
    }
}
