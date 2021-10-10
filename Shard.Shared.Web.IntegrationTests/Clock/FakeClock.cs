using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shard.Shared.Core;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock : IClock
    {
        private interface IEvent
        {
            public DateTime TriggerTime { get; }
            public void Trigger();
        }

        private readonly ConcurrentDictionary<IEvent, IEvent> events
            = new ConcurrentDictionary<IEvent, IEvent>();

        private void AddEvent(IEvent anEvent)
        {
            events.TryAdd(anEvent, anEvent);
        }

        private DateTime now;
        public DateTime Now => now;

        public void SetNow(DateTime now)
        {
            TriggerEventsUpTo(now);
            this.now = now;
        }

        private void TriggerEventsUpTo(DateTime now)
        {
            bool hasAnEventBeenTriggered;
            do
            {
                hasAnEventBeenTriggered = TryTriggerNextEvent(now);
            }
            while (hasAnEventBeenTriggered);
        }

        private bool TryTriggerNextEvent(DateTime now)
        {
            var eventToTrigger = FindNextEvent(now);

            if (eventToTrigger == null || !events.TryRemove(eventToTrigger, out _))
                return false;

            this.now = eventToTrigger.TriggerTime;
            eventToTrigger.Trigger();
            return true;
        }

        private IEvent FindNextEvent(DateTime now)
        {
            return events.Values
                .Where(anEvent => anEvent.TriggerTime <= now)
                .OrderBy(anEvent => anEvent.TriggerTime)
                .FirstOrDefault();
        }

        public Task Delay(int millisecondsDelay)
            => Delay(TimeSpan.FromMilliseconds(millisecondsDelay));

        public Task Delay(int millisecondsDelay, CancellationToken cancellationToken)
            => Delay(TimeSpan.FromMilliseconds(millisecondsDelay), cancellationToken);

        public Task Delay(TimeSpan delay)
            => Delay(delay, CancellationToken.None);

        public Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            if (delay.TotalMilliseconds > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "cannot be be higher than " + int.MaxValue);
            }

            if (cancellationToken.IsCancellationRequested)
                return Task.FromCanceled(cancellationToken);

            if (delay.TotalMilliseconds == 0)
                return Task.CompletedTask;

            if (delay.TotalMilliseconds == -1)
                return InfiniteDelay(cancellationToken);

            if (delay.TotalMilliseconds < 0)
                throw new ArgumentOutOfRangeException(nameof(delay), "cannot be negative except -1 ms");

            var delayEvent = new DelayEvent(Now + delay, cancellationToken);
            AddEvent(delayEvent);
            return delayEvent.Task;
        }
        
        private Task InfiniteDelay(CancellationToken cancellationToken)
            => new BaseDelayEvent(cancellationToken).Task;

        public void Sleep(int millisecondsTimeout)
            => Delay(millisecondsTimeout).Wait();

        public void Sleep(TimeSpan timeout)
            => Delay(timeout).Wait();


        public ITimer CreateTimer(TimerCallback callback)
            => CreateTimer(callback, this, uint.MaxValue, uint.MaxValue);

        public ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
            => CreateTimer(callback, this, (long)dueTime.TotalMilliseconds, (long)period.TotalMilliseconds);

        public ITimer CreateTimer(TimerCallback callback, object state, int dueTime, int period)
        {
            if (dueTime < -1)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
            if (period < -1)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
            return CreateTimer(callback, this, (uint)dueTime, (uint)period);
        }

        public ITimer CreateTimer(TimerCallback callback, object state, long dueTime, long period)
        {
            if (dueTime < -1)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot be lower than -1");
            if (period < -1)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot be lower than -1");
            if (dueTime >= uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(dueTime), "cannot exceed " + uint.MaxValue);
            if (period >= uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(period), "cannot exceed " + uint.MaxValue);

            return CreateTimer(callback, this, dueTime, period);
        }

        public ITimer CreateTimer(TimerCallback callback, object state, uint dueTime, uint period)
        {
            throw new NotImplementedException();
        }
    }
}