using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer.Operations
{
    public class LocalCollectionConfiguration : Alachisoft.NosDB.Common.Configuration.DOM.CollectionConfiguration, Alachisoft.NosDB.Common.Server.Engine.ICollectionConfiguration
    {
        public Common.Server.Engine.ReplicationPreference ReplicationPreference
        {
            get
            {
                return Common.Server.Engine.ReplicationPreference.SYNC;
            }
            set
            {
                
            }
        }
    }
}
