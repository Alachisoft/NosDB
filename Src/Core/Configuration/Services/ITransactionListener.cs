using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Configuration.Services
{
    public interface ITransactionListener
    {
        bool OnPreCommitTransaction(ConfigurationStore.Transaction transaction);
        void OnPostCommitTransaction(ConfigurationStore.Transaction transaction);
    }
}
