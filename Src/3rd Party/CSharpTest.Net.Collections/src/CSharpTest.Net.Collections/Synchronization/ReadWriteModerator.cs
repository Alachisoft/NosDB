using System;
using System.Collections.Generic;
using System.Threading;

namespace CSharpTest.Net.Synchronization
{
    public class ReadWriteModerator
    {

        private IDictionary<int, object> readSyncs;
        private IDictionary<int, object> writeSyncs;

        private readonly object _lock = new object();

        public enum Mode
        {
            Read,
            Write
        }

        public class Locker : IDisposable
        {
            private ReadWriteModerator _parent;
            private Mode _mode;

            public Locker(ReadWriteModerator parent, Mode mode)
            {
                _parent = parent;
                _mode = mode;

                bool noBlock;
                object syncObj = new object();

                switch (mode)
                {
                    case Mode.Read:
                        lock (_parent._lock)
                        {
                            noBlock = (_parent.writeSyncs.Count == 0);
                            if (!_parent.readSyncs.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                                _parent.readSyncs.Add(Thread.CurrentThread.ManagedThreadId, syncObj);
                            Monitor.Enter(syncObj);
                        }
                        if (!noBlock)
                            Monitor.Wait(syncObj);
                        break;
                    case Mode.Write:
                        lock (_parent._lock)
                        {
                            noBlock = (_parent.readSyncs.Count == 0 && _parent.writeSyncs.Count == 0);
                            if (!_parent.writeSyncs.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                                _parent.writeSyncs.Add(Thread.CurrentThread.ManagedThreadId, syncObj);
                            Monitor.Enter(syncObj);
                        }
                        if (!noBlock)
                            Monitor.Wait(syncObj);
                        break;
                }
            }

            public void Dispose()
            {
                lock (_parent._lock)
                {
                    switch (_mode)
                    {
                        case Mode.Read:
                            _parent.readSyncs.Remove(Thread.CurrentThread.ManagedThreadId);
                            break;

                        case Mode.Write:
                            _parent.writeSyncs.Remove(Thread.CurrentThread.ManagedThreadId);
                            break;
                    }
                    if (_parent.writeSyncs.Count != 0)
                    {
                        object syncObj;
                        using (var enumerator = _parent.writeSyncs.GetEnumerator())
                        {
                            enumerator.MoveNext();
                            syncObj = enumerator.Current.Value;
                        }
                        Monitor.Enter(syncObj);
                        Monitor.PulseAll(syncObj);
                        Monitor.Exit(syncObj);
                    }
                    else if (_parent.readSyncs.Count != 0)
                    {
                        foreach (var sync in _parent.readSyncs.Values)
                        {
                            Monitor.Enter(sync);
                            Monitor.PulseAll(sync);
                            Monitor.Exit(sync);
                        }
                    }
                    return;
                }
            }
        }

        public ReadWriteModerator()
        {
            readSyncs = new Dictionary<int, object>();
            writeSyncs = new Dictionary<int, object>();
        }

        public Locker ReaderLock
        {
            get { return new Locker(this, Mode.Read); }
        }

        public Locker WriterLock
        {
            get { return new Locker(this, Mode.Write); }
        }
    }
}
