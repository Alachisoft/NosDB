using Alachisoft.NoSDB.Common.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alachisoft.NoSDB.Common.Recovery.Operation
{
    // used in future if config backup is done on secondary not primary
    public class ConfigBackupOpParams:ICompactSerializable
    {

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            throw new NotImplementedException();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
