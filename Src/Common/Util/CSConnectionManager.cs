using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Alachisoft.NosDB.Common.Communication;
using Alachisoft.NosDB.Common.Configuration.RPC;
using Alachisoft.NosDB.Common.Configuration.Services;
using Alachisoft.NosDB.Common.Configuration.Services.Client;
using Alachisoft.NosDB.Common.ErrorHandling;
using Alachisoft.NosDB.Common.Exceptions;
using Alachisoft.NosDB.Common.Security.Client;

namespace Alachisoft.NosDB.Common.Util
{
    public class CSConnectionManager
    {
        public static IConfigurationSession Connect(string[] configServers, int configServerPort, string cluster, out IConfigurationServer remote, IChannelFormatter channelFormatter, IClientAuthenticationCredential clientAuthenticationCredential, bool remoteRouter = false)
        {
            Exception exception = null;
            DatabaseRPCService rpc = null;
            remote = null;
            IConfigurationSession configurationSession = null;
            int csPort = configServerPort;
            Boolean found = false;
            foreach (String current in configServers)
            {
                if (found)
                    break;

                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        if (configurationSession != null)
                        {
                            configurationSession.Close();
                            configurationSession = null;
                        }
                        rpc = new DatabaseRPCService(current, csPort);

                        //if (remote == null)
                        {
                            remote = rpc.GetConfigurationServer(new TimeSpan(0, 0, 90), SessionTypes.Client, channelFormatter);
                        }

                        if (remoteRouter)
                        {
                            remote.MarkDistributorSession();
                        }

                        configurationSession = remote.OpenConfigurationSession(clientAuthenticationCredential);

                        if (configurationSession != null)
                        {
                            List<Alachisoft.NosDB.Common.Net.Address> csServers = configurationSession.GetConfServers(cluster);

                            if (csServers == null || csServers.Count < 1)
                            {
                                throw new DatabaseException(Common.ErrorHandling.ErrorCodes.Distributor.CLUSTER_INFO_UNAVAILABLE, new[] { cluster });
                            }

                            foreach (Alachisoft.NosDB.Common.Net.Address add in csServers)
                                if (add.ip.Equals(current)) { found = true; }

                            if (!found)
                            {
                                configurationSession.Close();
                                configurationSession = null;

                                foreach (Alachisoft.NosDB.Common.Net.Address cur in csServers)
                                {
                                    try
                                    {
                                        rpc = new DatabaseRPCService(cur.ip, csPort);
                                        remote = rpc.GetConfigurationServer(new TimeSpan(0, 0, 90), SessionTypes.Client, channelFormatter);

                                        if (remoteRouter)
                                        {
                                            remote.MarkDistributorSession();
                                        }
                                        configurationSession = remote.OpenConfigurationSession(clientAuthenticationCredential);
                                        found = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        exception = ex;
                                    }

                                }
                            }

                        }

                        if (found)
                            break;
                    }
                    catch (Alachisoft.NosDB.Common.Exceptions.TimeoutException)
                    {
                        if (configurationSession != null)
                        {
                            configurationSession.Close();
                            configurationSession = null;
                        }

                        exception = new DistributorException(ErrorCodes.Distributor.CONFIGURATION_SERVER_NOTRESPONDING);
                        retries--;
                        if (retries == 0) break;
                    }
                    catch (Exception e)
                    {
                        if (configurationSession != null)
                        {
                            configurationSession.Close();
                            configurationSession = null;
                        }

                        exception = e;
                        retries--;
                        if (retries == 0) break;

                    }
                }
            }

            if (configurationSession != null)
            {
                return configurationSession;
            }
            else if (exception != null)
                throw exception;
            return null;
        }
    }
}
