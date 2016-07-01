// /*
// * Copyright (c) 2016, Alachisoft. All Rights Reserved.
// *
// * Licensed under the Apache License, Version 2.0 (the "License");
// * you may not use this file except in compliance with the License.
// * You may obtain a copy of the License at
// *
// * http://www.apache.org/licenses/LICENSE-2.0
// *
// * Unless required by applicable law or agreed to in writing, software
// * distributed under the License is distributed on an "AS IS" BASIS,
// * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// * See the License for the specific language governing permissions and
// * limitations under the License.
// */
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Util;

namespace Alachisoft.NosDB.ConfigurationService
{
    public partial class ConfigurationService : ServiceBase
    {
        Alachisoft.NosDB.Core.Configuration.Services.ConfigurationHost cHost = null;
        //private static string _moduleName = "Configuration Service";
        private object _stop_mutex = new object();

        public ConfigurationService()
        {
            InitializeComponent();
            //if (!System.Diagnostics.EventLog.SourceExists("NosDB"))
            //{
            //    System.Diagnostics.EventLog.CreateEventSource(
            //        "NosDB", "Configuration Logs");
            //}
            //configurationEventLog.Source = "NosDB";
            //configurationEventLog.Log = "Configuration Logs";
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppUtil.LogEvent(AppUtil.EventLogSource, e.ExceptionObject.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.UnhandledException);
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Thread thread = new Thread(() => StartConfigurationService());
            //thread.Start();
            try
            {
                //writer.AutoFlush = true;
                //writer.WriteLine("Service Begin");
                // AppUtil.LogEvent("Configuration Service Begin::", EventLogEntryType.Information);   
                //AppUtil.LogEvent("111Configuration Service is trying to start: eeeennn", EventLogEntryType.Information);
                cHost = new Alachisoft.NosDB.Core.Configuration.Services.ConfigurationHost();
                cHost.Start();
                //AppUtil.LogEvent("111Configuration Service started successfully", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                //writer.WriteLine("Exception:::" + ex.ToString()+ "\n InnerException"+ex.InnerException);
                //AppUtil.LogEvent(AppUtil.EventLogSource, "lifaj "+ex.StackTrace.ToString()+"\n"+ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
                throw ex;
            }
        }

        protected override void OnStop()
        {
            lock (_stop_mutex)
            {
                try
                {
                    Thread thread = new Thread(new ThreadStart(StopHosting));
                    thread.Name = "CSStopHostingThread";
                    thread.IsBackground = true;
                    thread.Start();
                    bool pulsed = Monitor.Wait(_stop_mutex, 18000); //we wait for 18 seconds. Default service timeout is 20000 (ms)
                    if (!pulsed)
                    {
                        AppUtil.LogEvent(AppUtil.EventLogSource, "Failed to stop configuration service", EventLogEntryType.Warning, EventCategories.Error, EventID.GeneralInformation);
                    }
                    if (thread.IsAlive) thread.Abort();
                }
                catch (Exception)
                {

                }  
            }
           

        }
        private void StopHosting()
        {
            try
            {
                if (cHost != null)
                    cHost.Stop();
            }
            catch (ThreadAbortException te)
            {
                return;

            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(AppUtil.EventLogSource, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
                throw ex;
            }
            finally
            {
                try
                {
                    lock (_stop_mutex)
                    {
                        Monitor.PulseAll(_stop_mutex);
                    }
                }
                catch (Exception) { }
            }
        }

        //private void StartConfigurationService()
        //{
        //    //System.IO.StreamWriter writer = new StreamWriter(@"D:\CSConfigurationLog.txt");
            
        //    try
        //    {
        //        //writer.AutoFlush = true;
        //        //writer.WriteLine("Service Begin");
        //       // AppUtil.LogEvent("Configuration Service Begin::", EventLogEntryType.Information);   
        //          cHost = new Alachisoft.NosDB.Core.Configuration.Services.ConfigurationHost();                 
        //          cHost.Start();                  
        //          AppUtil.LogEvent("Configuration Service started successfully", EventLogEntryType.Information);                                     
        //    }
        //    catch (Exception ex)
        //    {
        //        //writer.WriteLine("Exception:::" + ex.ToString()+ "\n InnerException"+ex.InnerException);
        //        AppUtil.LogEvent(_moduleName, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
        //        throw ex;
        //    }

        //}

    }
}
