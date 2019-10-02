using Shard.Shared.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock
    {
        public class Timer : ITimer
        {
            private readonly TimerCallback callback;
            private readonly object state;
            private readonly uint dueTime;
            private readonly uint period;

            public Timer(TimerCallback callback, object state, uint dueTime, uint period)
            {
                this.callback = callback;
                this.state = state;
                this.dueTime = dueTime;
                this.period = period;
            }

            public bool Change(int dueTime, int period)
            {
                if (dueTime < -1)
                    throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
                if (period < -1)
                    throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
                throw new NotImplementedException();
            }

            public bool Change(long dueTime, long period)
            {
                if (dueTime < -1)
                    throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
                if (period < -1)
                    throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
                if (dueTime >= uint.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot exceed " + uint.MaxValue);
                if (period >= uint.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(period), "cannot exceed " + uint.MaxValue);
                throw new NotImplementedException();
            }

            public bool Change(TimeSpan dueTime, TimeSpan period)
                => Change((long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);

            public bool Change(uint dueTime, uint period)
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }

            public bool Dispose(WaitHandle notifyObject)
            {
                throw new NotImplementedException();
            }

            public ValueTask DisposeAsync()
            {
                throw new NotImplementedException();
            }
        }
    }
}
