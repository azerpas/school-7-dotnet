using System.Threading;
using System.Threading.Tasks;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock
    {
        private class BaseDelayEvent
        {
            private TaskCompletionSource<object> taskCompletionSource;
            public Task Task => taskCompletionSource.Task;

            public BaseDelayEvent(CancellationToken cancellationToken)
            {
                taskCompletionSource = new TaskCompletionSource<object>();
                cancellationToken.Register(
                    () => taskCompletionSource.TrySetCanceled(cancellationToken));
            }

            public void Trigger()
            {
                taskCompletionSource.SetResult(this);
            }
        }
    }
}