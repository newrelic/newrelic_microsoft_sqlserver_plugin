using System;
using System.Threading;

namespace NewRelic.Microsoft.SqlServer.Plugin.Core
{
    internal class PollingThreadSettings
    {
        private long _pollIntervalSeconds = 60;

        public AutoResetEvent AutoResetEvent { get; set; }
        public string Name { get; set; }
        public Thread Thread { get; set; }
        public Action PollAction { get; set; }

        public long PollIntervalSeconds
        {
            get { return _pollIntervalSeconds; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "Value cannot be less than 0");
                }
                _pollIntervalSeconds = value;
            }
        }
    }
}
