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
using System.Collections;
using System.Data.Common;
using System.Globalization;

namespace Alachisoft.NosDB.ADO.NETProvider
{
    public class NosDataParameterCollection : DbParameterCollection, System.Data.IDataParameterCollection
    {
        private ArrayList _list = new ArrayList();
        
        public override int Add(object value)
        {
            return this._list.Add(value);
        }
        
        public override void AddRange(Array values)
        {
            this._list.AddRange(values);
        }

        public override void Clear()
        {
            this._list.Clear();
        }

        public override bool Contains(string value)
        {
            return this._list.Contains(value);
        }

        public override bool Contains(object value)
        {
            return this._list.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            this._list.CopyTo(array, index);
        }

        public override int Count
        {
            get { return this._list.Count; }
        }

        public override IEnumerator GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return (DbParameter)this._list[IndexOf(parameterName)];
        }

        protected override DbParameter GetParameter(int index)
        {
            return (DbParameter)this._list[index];
        }

        public override int IndexOf(string parameterName)
        {
            return this._list.IndexOf(parameterName);
        }

        public override int IndexOf(object value)
        {
            return this._list.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            this._list.Insert(index, value);
        }

        public override bool IsFixedSize
        {
            get { return this._list.IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return this._list.IsReadOnly; }
        }

        public override bool IsSynchronized
        {
            get { return this._list.IsSynchronized; }
        }

        public override void Remove(object value)
        {
            this._list.Remove(value);
        }

        public override void RemoveAt(string parameterName)
        {
            this._list.RemoveAt(IndexOf(parameterName));
        }

        public override void RemoveAt(int index)
        {
            this._list.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            //TODO
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            //TODO
        }

        public override object SyncRoot
        {
            get { return this._list.SyncRoot; }
        }
    }
}
