using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using LightningDB;
using LightningDB.Converters;

namespace Alachisoft.NosDB.Core
{
    public class LMDBDataReader<TKey, TValue> : IDataReader<TKey, TValue>
    {
        private IEnumerator<KeyValuePair<byte[], byte[]>> _enumerator;
        private LightningEnvironment _environment;
        private LightningDatabase _database;

        public IEnumerator<KeyValuePair<byte[], byte[]>> Enumerator
        {
            get { return _enumerator; }
        }

        public LMDBDataReader(IEnumerator<KeyValuePair<byte[], byte[]>> enumerator, LightningEnvironment environment, LightningDatabase db)
        {
            if (enumerator == null)
            {
                throw new ArgumentException("Enumerator can not be null.");
            }
            _enumerator = enumerator;
            _environment = environment;
            _database = db;
        }

        public KeyValuePair<TKey, TValue> Current()
        {
            KeyValuePair<byte[], byte[]> oppResult = _enumerator.Current;
            TKey key = _environment.ConverterStore.GetFromBytes<TKey>().Convert(_database, oppResult.Key);
            TValue value = _environment.ConverterStore.GetFromBytes<TValue>().Convert(_database, oppResult.Value);

            return new KeyValuePair<TKey, TValue>(key, value);
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }


        public void Dispose()
        {
            //todo: dispose and test for LMDB, required for ESENT
        }
    }
}
