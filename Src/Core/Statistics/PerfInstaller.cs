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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

namespace Alachisoft.NosDB.Core.Statistics
{
    [RunInstaller(true)]
    public partial class PerfInstaller : System.Configuration.Install.Installer
    {
        private System.Diagnostics.PerformanceCounterInstaller pcInstaller;

        public PerfInstaller()
        {
            InitializeComponent();
            InitializeCounters();
        }

        #region Initialize Counters

        private void InitializeCounters()
        {
            this.pcInstaller = new System.Diagnostics.PerformanceCounterInstaller();
            this.pcInstaller.CategoryName = "NosDB";

            this.pcInstaller.Counters.AddRange(new System.Diagnostics.CounterCreationData[] {
                //Database Counters
            new System.Diagnostics.CounterCreationData("Inserts/sec", "Number of Insert operations per second..", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Fetches/sec", "Number of Get Operations per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Updates/sec", "Number of Update operations per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Deletes/sec", "Number of Delete operations per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Cache Hits /sec", "Number of successful gets from cache per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Cache Misses/sec", "umber of failed gets from cache per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Requests/sec", "Number of requests per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Average Fetch Time (µs)", "Average time in microseconds, taken to complete one fetch operation.", System.Diagnostics.PerformanceCounterType.AverageTimer32),
            new System.Diagnostics.CounterCreationData("Average Fetch Time base", "Base counter for average fetch time", System.Diagnostics.PerformanceCounterType.AverageBase),
            new System.Diagnostics.CounterCreationData("Average Insert Time (µs)", "Average time in microseconds, taken to complete one add operation.", System.Diagnostics.PerformanceCounterType.AverageTimer32),
            new System.Diagnostics.CounterCreationData("Average Insert Time base", "Base counter for average Insert time", System.Diagnostics.PerformanceCounterType.AverageBase),
            new System.Diagnostics.CounterCreationData("Average Update Time (µs)", "Average time in microseconds, taken to complete one update operation.", System.Diagnostics.PerformanceCounterType.AverageTimer32),
            new System.Diagnostics.CounterCreationData("Average Update Time base", "Base counter for average Update time", System.Diagnostics.PerformanceCounterType.AverageBase),
            new System.Diagnostics.CounterCreationData("Average Delete Time (µs)", "Average time in microseconds, taken to complete one remove operation.", System.Diagnostics.PerformanceCounterType.AverageTimer32),
            new System.Diagnostics.CounterCreationData("Average Delete Time base", "Base counter for average Delete time", System.Diagnostics.PerformanceCounterType.AverageBase),
            new System.Diagnostics.CounterCreationData("Average Query Execution Time (µs)", "Average time to execute a query.", System.Diagnostics.PerformanceCounterType.AverageTimer32),
            new System.Diagnostics.CounterCreationData("Average Query Execution Time base", "Base counter for average Query Execution time", System.Diagnostics.PerformanceCounterType.AverageBase),
            new System.Diagnostics.CounterCreationData("Pending Persistent Documents", "Number of documents that are not persisted uptill.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Documents Persisted/sec", "Documents Persisted per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("Documents Count", "Total Documents in the database.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Average Document Size", "Average document size in bytes.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Database Size", "Total database size in bytes.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Cache Count", "Number of items in Cache.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Cache Size", "Size of Cache.", System.Diagnostics.PerformanceCounterType.NumberOfItems64),
            new System.Diagnostics.CounterCreationData("Cache Evicitions/sec", "Cache evicitions per second.", System.Diagnostics.PerformanceCounterType.SampleCounter),
           
            //StateTxfer Counters            
            new System.Diagnostics.CounterCreationData("Data Balance/sec", "Number of data balance per sec.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            new System.Diagnostics.CounterCreationData("State Transfer/sec", "Number of state transfer per sec.", System.Diagnostics.PerformanceCounterType.SampleCounter),
            
            //Replication Counters
         //   new System.Diagnostics.CounterCreationData("Pending Replicated Operations", "Number of pending Replicated Operations present at primary Node.", System.Diagnostics.PerformanceCounterType.NumberOfItems64)
            });

            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.pcInstaller});
        }
        #endregion
    }
}
