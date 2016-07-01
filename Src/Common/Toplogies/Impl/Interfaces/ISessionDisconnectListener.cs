using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alachisoft.NosDB.Common.Configuration.Services;

namespace Alachisoft.NosDB.Common.Toplogies.Impl.Interfaces
{
    public interface ISessionDisconnectListener
    {
        void OnSessionDisconnected(IConfigurationSession session);
    }
}
