using Alachisoft.NoSQL.Common.RPCFramework;
using Alachisoft.NoSQL.Common.RPCFramework.DotNetRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Alachisoft.NoSQL.Core.Monitoring
{
    class MonitorSessionProvider
    {
        private static MonitorServer _monitor;
        private static RPCService<MonitorServer> _monitorRpcService;

        public static RPCService<MonitorServer> MonitorRpcService
        {
            get { return _monitorRpcService; }
        }

        public static MonitorServer Provider
        {
            get { return _monitor; }
            set
            {
                _monitor = value;
                _monitorRpcService = new Common.RPCFramework.RPCService<MonitorServer>(new TargetObject<MonitorServer>(value));
            }
        }
    }
}
