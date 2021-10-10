using Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock
    {
        private class BaseDelayEvent
        {
            private readonly TaskCompletionSource<object> taskCompletionSource;
            private readonly AsyncTrackingSyncContext asyncTestSyncContext;

            public Task Task => taskCompletionSource.Task;

            public BaseDelayEvent(AsyncTrackingSyncContext asyncTestSyncContext, CancellationToken cancellationToken)
            {
                taskCompletionSource = new TaskCompletionSource<object>();
                cancellationToken.Register(
                    () => taskCompletionSource.TrySetCanceled(cancellationToken));

                this.asyncTestSyncContext = asyncTestSyncContext;
            }

            public async Task TriggerAsync()
            {
                taskCompletionSource.SetResult(this);
                // We want to ensure all tasks ready to start 
                // are triggered before we move on
                await asyncTestSyncContext.WaitForCompletionAsync();
            }
        }
    }
}