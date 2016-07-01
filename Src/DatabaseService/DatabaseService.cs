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
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Alachisoft.NosDB.Common;
using Alachisoft.NosDB.Common.Util;
using Alachisoft.NosDB.Core;
using Alachisoft.NosDB.Core.Configuration.Services;
using Alachisoft.NosDB.Core.Toplogies;
using System.Timers;
using Alachisoft.NosDB.Common.Exceptions;



namespace Alachisoft.NosDB.DatabaseService
{
    public partial class DatabaseService : ServiceBase
    {
        private ManagementHost _host;
        private readonly object _stop_mutex = new object();
        //private static string _moduleName = "Database Service";
        private Thread startingthread;
        StringBuilder sb = new StringBuilder();
        private System.IO.TextWriter _writer;
        private System.Timers.Timer _reactWarningTask;
        private static System.Timers.Timer _evalWarningTask;
        private const short MAX_EVALDAYS_REPORTING = 10;
        public DatabaseService()
        {
            InitializeComponent();
        }

        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
                new DatabaseService() 
            };
            ServiceBase.Run(ServicesToRun);
        }

        protected override void OnStart(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            try
            {
                _host = new ManagementHost();
                _host.Initialize();
                _host.Start();
                
                Thread thread=new Thread(()=>startDb());
                thread.Start();

               // _host.StartDbNodes();
                //try
                //{
                //   
                //}
                //catch (Exception e)
                //{
                //    _host.StartLocalDbNode();
                //}
                //bool start = false;
                //do
                //{
                //    start = _host.StartLocalDbNode();


                //} while (!start);
                  

                //if (Licensing.LicenseManager.LicenseMode(null) == Licensing.LicenseManager.LicenseType.InEvaluation)
                //{
                //    _evalWarningTask = new System.Timers.Timer();
                //    _evalWarningTask.Interval = 1000 * 60 * 60 * 12;// 12 hour interval.
                //    _evalWarningTask.Elapsed += new ElapsedEventHandler(NotifyEvalLicense);
                //    _evalWarningTask.Enabled = true;
                //    NotifyEvalLicense(null, null);
                //} 
                //if (Alachisoft.NosDB.Core.Licensing.LicenseManager.Reactivate)
                //{
                //    _reactWarningTask = new System.Timers.Timer();
                //    _reactWarningTask.Interval = 1000 * 60 * 60 * 24;//1 day
                //    _reactWarningTask.Elapsed += new ElapsedEventHandler(NotifyReactivateLicense);
                //    _reactWarningTask.Enabled = true;
                //    NotifyReactivateLicense(null, null);
                //}
                //AppUtil.LogEvent("Database Service started successfully11", EventLogEntryType.Information);                                     
            }
            catch (Exception ex)
            {
                //writer.WriteLine("Exception:::" + ex.ToString() + "\n InnerException" + ex.InnerException);
                AppUtil.LogEvent(AppUtil.EventLogSource, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
                throw ex;
            }
        
        }

        private void startDb()
        {
            try
            {

                _host.StartLocalDbNode();
                _host.StartDbNodes();

            }
            catch(Exception ex)
            {
                AppUtil.LogEvent(AppUtil.EventLogSource, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
                throw ex;
            }
        }


        //private void StartManagementHost()
        //{
        //   // System.IO.StreamWriter writer = new StreamWriter(@"D:\DBConfigurationLog.txt");
        //    try
        //    {
        //        //writer.AutoFlush = true;
        //        //writer.WriteLine("Service begin");
        //        _host = new ManagementHost();
        //        _host.Initialize();
        //        _host.Start();
        //        AppUtil.LogEvent("Database Service started successfully", EventLogEntryType.Information);                                     
        //    }
        //    catch (Exception ex)
        //    {
        //        //writer.WriteLine("Exception:::" + ex.ToString() + "\n InnerException" + ex.InnerException);
        //        AppUtil.LogEvent(_moduleName, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Stop this service.
        /// </summary>
        /// 
        //private void NotifyEvalLicense(object source, ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        Licensing.LicenseManager.LicenseType mode = Licensing.LicenseManager.LicenseMode(null);
        //        if (mode == Licensing.LicenseManager.LicenseType.ActivePerNode || mode == Licensing.LicenseManager.LicenseType.ActivePerProcessor)
        //        {

        //            if (_evalWarningTask != null)
        //            {
        //                _evalWarningTask.Dispose();
        //                _evalWarningTask.Close();
        //                _evalWarningTask = null;
        //            }
        //        }
        //        else if (mode == Licensing.LicenseManager.LicenseType.InEvaluation)
        //        {
        //            TimeSpan evalTime = System.DateTime.Now - Licensing.LicenseManager.EvaluationDt;
        //            double daysRemaining = Licensing.LicenseManager.EvaluationPeriod - evalTime.Days;
        //            bool writeEntry = (daysRemaining <= MAX_EVALDAYS_REPORTING);

        //            if (writeEntry)
        //            {
        //                object value = RegHelper.GetRegValue(RegHelper.ROOT_KEY, EVAL_LASTREPORT_REGKEY, 4);
        //                if (value != null)
        //                {
        //                    DateTime reportingTime = DateTime.MinValue;
        //                    try
        //                    {
        //                        long ticks = Convert.ToInt64(value);
        //                        reportingTime = new DateTime(ticks);
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }

        //                    DateTime today = DateTime.Now;
        //                    if (reportingTime.DayOfYear == today.DayOfYear && reportingTime.Year == today.Year)
        //                    {
        //                        writeEntry = false;
        //                    }
        //                }
        //            }

        //            //EventLog eventLog = new EventLog("Application", ".");
        //            //EventLogEntryCollection eventLogCollection = eventLog.Entries;
        //            //int count = eventLogCollection.Count;
        //            //for (int i = 0; i < count - 1; i++)
        //            //{
        //            //    EventLogEntry entry = eventLogCollection[i];
        //            //    if (entry.Source == "NCache")
        //            //    {
        //            //        if (entry.TimeWritten.Date.Equals(DateTime.Now.Date) && entry.Message.StartsWith("NCache evaluation"))
        //            //        {
        //            //            writeEntry = false;
        //            //            break;
        //            //        }
        //            //    }
        //            //}

        //            if (writeEntry)
        //            {
        //                DateTime dT = DateTime.Now.AddDays(daysRemaining);
        //                if (daysRemaining <= MAX_EVALDAYS_REPORTING && daysRemaining > 1)
        //                {
        //                    string msg = string.Format(_cacheserver + " evaluation of {0} days expires on {1}. Please purchase license keys or extend evaluation period by contacting support@alachisoft.com ", Licensing.LicenseManager.EvaluationPeriod, dT.Date);
        //                    EventLog.WriteEntry(_cacheserver, msg, EventLogEntryType.Warning);
        //                }
        //                else if (daysRemaining == 1 || daysRemaining == 0)
        //                {
        //                    string msg = string.Format(_cacheserver + " evaluation of {0} days expires on {1}. It cannot be extended any more. Therefore, please purchase NCache license from sales@alachisoft.com and activate before expiration.", Licensing.LicenseManager.EvaluationPeriod, dT.Date);
        //                    EventLog.WriteEntry(_cacheserver, msg, EventLogEntryType.Warning);
        //                }

        //                RegHelper.SetRegValue(RegHelper.ROOT_KEY, EVAL_LASTREPORT_REGKEY, DateTime.Now.Ticks, 0);
        //            }
        //        }

        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        protected override void OnStop()
        {
            lock (_stop_mutex)
            {
                try
                {
                    Thread thread = new Thread(new ThreadStart(StopHosting));
                    thread.Name = "DBStopHostingThread";
                    thread.IsBackground = true;
                    thread.Start();
                    bool pulsed = Monitor.Wait(_stop_mutex, 18000); //we wait for 18 seconds. Default service timeout is 20000 (ms)
                    if (!pulsed)
                    {
                        AppUtil.LogEvent(AppUtil.EventLogSource, "Failed to stop Database Service", EventLogEntryType.Warning, EventCategories.Error, EventID.GeneralInformation);
                    }
                    if (thread.IsAlive) thread.Abort();
                }
                catch (Exception)
                {

                }
            }           
        }

        //private void StartDatabaseService()
        //{
        //    try
        //    {
        //        //Console.ReadLine();
        //        ////Thread.Sleep(20000);
        //        //_writer.WriteLine("StartDatabaseService called");
        //        //sb.Append("StartDatabaseService called");
        //        //  _host =new Host();
        //        //  _writer.WriteLine("b4 initialize called");
        //        //  sb.Append("b4 initialize called");
        //        //  _host.Initialize();
        //        //  _writer.WriteLine("After initialize called");
        //        //  sb.Append("After initialize called");
        //        //  _host.Start();
        //        //  _writer.WriteLine("after start called");
        //        //  sb.Append("after start called");
        //        //  while (true)
        //        //  {

        //        //      PrintCurrentMembership(_host.NodeContext);

        //        //      Console.ReadLine();

        //        //      Console.Clear();
        //        //  }
        //        // // PrintCurrentMembership(_host.NodeContext);
        //         // configurationEventLog.WriteEntry("Configuration Service started successfully", EventLogEntryType.Information);
                   
        //    }
        //    catch (Exception ex)
        //    {
        //        _writer.WriteLine("Error :" + ex.ToString());
        //        sb.Append("Error :" + ex.ToString());
        //       // OnStop();
        //        // configurationEventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
        //    }
        //    _writer.Flush();
        //}

        //static void PrintCurrentMembership(NodeContext context)
        //{
        //    try
        //    {
        //        if (context != null)
        //        {
        //            Console.WriteLine("\n\n/////////////////////////////////////////////////////////////////////////////");
        //            Console.WriteLine("/////////////////////////////////////////////////////////////////////////////\n\n");
        //            if (context.ClusterName != null)
        //                Console.WriteLine("-------------------Cluster Name    : " + context.ClusterName);
        //            if (context.LocalShardName != null)
        //                Console.WriteLine("-------------------Shard Name      : " + context.LocalShardName);
        //            if (context.LocalAddress != null)
        //                Console.WriteLine("-------------------Local Address   : {0}:{1}", context.LocalAddress.IpAddress.ToString(), context.LocalAddress.Port.ToString());
        //            if (context.ConfigurationSession != null)
        //            {
        //                Membership membership = context.ConfigurationSession.GetMembershipInfo(context.ClusterName, context.LocalShardName);
        //                if (membership != null)
        //                {
        //                    if (membership.Primary != null)
        //                        Console.WriteLine("-------------------Shard Primary   : " + membership.Primary.Name);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        // throw;
        //    }
        //}
  
        private void StopHosting()
        {
            try
            {
                if (_host != null)
                    _host.Stop();
            }
            catch(ThreadAbortException te)
            {
                return;

            }
            catch (Exception ex)
            {
                AppUtil.LogEvent(AppUtil.EventLogSource, ex.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.GeneralError);
                //throw ex;
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

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppUtil.LogEvent(AppUtil.EventLogSource, e.ExceptionObject.ToString(), EventLogEntryType.Error, EventCategories.Error, EventID.UnhandledException);
        }
    }
}
