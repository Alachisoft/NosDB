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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Alachisoft.NosDB.Core.Recovery.Persistence
{
    /// <summary>
    /// Base class responsible for recovery persistence 
    /// </summary>
    public abstract class PersistenceIOBase : IDisposable
    {
        private PersistenceContext _context = null;       
        private Thread _persistenceThread = null;
        private string _name;
        private string _role;

        public PersistenceIOBase()
        { }

        public PersistenceIOBase(string name, string role)
        {
            _name = name;
            _role = role;
        }

        internal PersistenceContext Context
        {
            get { return _context; }

        }

        #region virtual methods
        internal virtual bool Initialize(PersistenceContext context)
        {
            _context = context;
            return true;
        }
        internal virtual void Run()
        {

        }

        internal virtual object JobStatistics()
        {
            return null;
        }
        #endregion

        #region Internal methods
        internal bool IsActive
        {
            get
            {
                if (_persistenceThread != null)
                {
                    if (_persistenceThread.ThreadState == ThreadState.Running)
                        return true;
                }

                return false;
            }
        }

        internal void Start()
        {
            if (_persistenceThread == null)
            {
                _persistenceThread = new Thread(new ThreadStart(Run));
                _persistenceThread.Name = _name + "_" + _role + "_Thread";
                _persistenceThread.IsBackground = true;
                _persistenceThread.Start();
            }
        }

        internal void Stop()
        {
            if (_persistenceThread != null)
            {            
                _persistenceThread.Abort();
                _persistenceThread = null;
                //_context.BackupFile.SaveHeader();
                //_context.BackupFile.Close();
            }
        }


        #endregion

        #region IDisposable
        public void Dispose()
        {
            if (_persistenceThread != null)
            {
                _persistenceThread.Abort();
                _persistenceThread = null;
            }


        }
        #endregion
    }
}
