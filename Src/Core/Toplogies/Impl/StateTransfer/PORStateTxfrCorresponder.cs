using System.Collections;
using Alachisoft.NCache.Common.Net;

namespace Alachisoft.NCache.Caching.Topologies.Clustered
{
    class PORStateTxfrCorresponder : StateTxfrCorresponder
    {
        internal PORStateTxfrCorresponder(ClusterCacheBase parent,DistributionManager distMgr,Address requestingNode):base(parent,distMgr,requestingNode,StateTransferType.REPLICATE_DATA)
        {
        }

        protected override StateTxfrInfo GetLoggedData(ArrayList bucketIds)
        {
            return new StateTxfrInfo(true);
        }
    }
}