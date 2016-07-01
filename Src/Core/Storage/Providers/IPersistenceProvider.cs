using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Storage.Transactions;
using Alachisoft.NosDB.Core.Configuration;
using Alachisoft.NosDB.Core.Storage.Collections;
using Alachisoft.NosDB.Core.Storage.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Storage.Providers
{
    public interface IPersistenceProvider : IDisposable, ITransactable
    {
        long CurrentDataSize { get; }

        bool Initialize(StorageConfiguration configuration);
        bool CreateCollection(string collection, Type keyType, Type valueType);
        void DropCollection(string collection);
        bool IsDatabaseFull();

        long CollectionSize(string collection);
        long CollectionDocumentCount(string collection);

        int StartDefragmentation();
        bool DefragmentationNeeded();

        StorageResult<TValue> StoreDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue value);
        StorageResult<TValue> UpdateDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key, TValue update);
        StorageResult<TValue> GetDocument<TKey, TValue>(string collection, TKey key);
        StorageResult<TValue> DeleteDocument<TKey, TValue>(ITransaction transaction, string collection, TKey key);

        IDataReader<TKey, TValue> GetAllDocuments<TKey, TValue>(string collection);

        void Destroy();
    }
}
