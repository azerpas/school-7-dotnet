using System;
using System.Threading;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock
    {
        private class DelayEvent : BaseDelayEvent, IEvent
        {
            public DateTime TriggerTime { get; }

            public DelayEvent(DateTime triggerTime, CancellationToken cancellationToken)
                : base(cancellationToken)
            {
                TriggerTime = triggerTime;
            }
        }
    }
}