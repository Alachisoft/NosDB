using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Server.Engine;
using Alachisoft.NosDB.Common.Configuration.DOM;
using Alachisoft.NosDB.Common.Security.Interfaces;
using Alachisoft.NosDB.Common.Toplogies.Impl.Distribution;
namespace Alachisoft.NosDB.Core.Toplogies.Impl.StateTransfer
{
    class LocalCreateCollectionOperation : LocalOperation, ICreateCollectionOperation
    {
        public CollectionConfiguration Configuration { get; set; }
        public override IDBResponse CreateResponse()
        {
            return new LocalResponse();
        }
        public IDistributionStrategy Distribution { get; set; }

        /// <summary>
        /// used to identify which client is performing an operatoin on database engine
        /// </summary>
        public ISessionId SessionId { set; get; } 
        
    }
}
