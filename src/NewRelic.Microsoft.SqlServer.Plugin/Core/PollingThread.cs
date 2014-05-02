using System;
using System.Threading;
using NewRelic.Platform.Sdk.Utils;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    internal class PollingThread
    {
        private static readonly Logger _log = Logger.GetLogger(typeof(MetricCollector).Name);

        private readonly PollingThreadState _threadState;

        public PollingThread(PollingThreadSettings threadSettings)
        {
            ThreadSettings = threadSettings;
            _threadState = new PollingThreadState();
        }

        public bool Running
        {
            get { return _threadState.IsRunning; }
        }

        public PollingThreadSettings ThreadSettings { get; private set; }

        public event Action<Exception> ExceptionThrown = delegate { };

        public void Start()
        {
            if (!_threadState.IsRunning)
            {
                ThreadStart tStart = ThreadStart;
                ThreadSettings.Thread = new Thread(tStart) {Name = string.IsNullOrEmpty(ThreadSettings.Name) ? "PollingThread" : ThreadSettings.Name, IsBackground = true};
                ThreadSettings.Thread.Start();
            }
            else if (_threadState.IsPaused(false))
            {
                ThreadSettings.AutoResetEvent.Set();
                _threadState.Resume();
            }
        }

        public void Pause()
        {
            _threadState.Pause();
        }

        public void Stop(bool waitForThreadExit)
        {
            _threadState.Stop();

            if (ThreadSettings.AutoResetEvent != null)
            {
                ThreadSettings.AutoResetEvent.Set();
            }

            if (ThreadSettings.Thread != null && waitForThreadExit)
            {
                if (!ThreadSettings.Thread.Join(new TimeSpan(0, 0, 20)))
                {
                    ThreadSettings.Thread.Abort();
                }
            }
        }

        private void ThreadStart()
        {
            try
            {
                ThreadLoop();
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            finally
            {
                ThreadSettings.AutoResetEvent = null;
                ThreadSettings.Thread = null;
                _threadState.Stop();
            }
        }

        private void ThreadLoop()
        {
            var errorCount = 0;

            _log.Debug("{0}: Entering Thread Loop", ThreadSettings.Name);

            _threadState.Start();

            while (_threadState.IsRunning)
            {
                if (!_threadState.IsPaused(true))
                {
                    try
                    {
                        ThreadSettings.PollAction();
                    }
                    catch (ThreadAbortException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        ExceptionThrown(ex);

                        if (++errorCount >= 3)
                        {
                            throw;
                        }

                        _log.Debug("Ignoring Error ({0} of 3):\r\n{1}", errorCount, ex);
                    }
                }
                else
                {
                    _log.Debug("{0}: Paused - skipping pass", ThreadSettings.Name);
                }

                var interval = TimeSpan.FromSeconds(ThreadSettings.PollIntervalSeconds);

                _log.Debug("{0}: Sleeping for {1}", ThreadSettings.Name, interval);

                if (ThreadSettings.AutoResetEvent.WaitOne(interval, true))
                {
                    _log.Debug("{0}: Interrupted - woken up early", ThreadSettings.Name);
                }
            }

            _log.Debug("{0}: Exiting Thread Loop", ThreadSettings.Name);
        }

        private class PollingThreadState
        {
            private bool _pauseReset;
            private bool _paused;

            public bool IsRunning { get; private set; }

            public bool IsPaused(bool reset)
            {
                if (_paused)
                {
                    return true;
                }

                if (_pauseReset)
                {
                    if (reset)
                    {
                        _pauseReset = false;
                    }

                    return true;
                }

                return false;
            }

            public void Start()
            {
                IsRunning = true;
                _paused = false;
                _pauseReset = false;
            }

            public void Pause()
            {
                IsRunning = true;
                _paused = true;
                _pauseReset = false;
            }

            public void Resume()
            {
                IsRunning = true;
                _paused = true;
                _pauseReset = true;
            }

            public void Stop()
            {
                IsRunning = false;
                _paused = false;
                _pauseReset = false;
            }
        }
    }
}
