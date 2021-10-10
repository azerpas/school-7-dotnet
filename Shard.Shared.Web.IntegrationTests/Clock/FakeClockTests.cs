using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public class FakeClockTests
    {
        private readonly FakeClock clock = new FakeClock();

        [Fact]
        public void SetNow_RetainsValue()
        {
            var value = new DateTime(2019, 10, 03, 08, 00, 00);
            clock.SetNow(value);
            Assert.Equal(value, clock.Now);
        }

        [Fact]
        public async Task Delay_ThrowsIfBellowMinusOne()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                "delay",
                () => clock.Delay(-2));
        }

        [Fact]
        public async Task Delay_ThrowsIfHighestThanMaxValue()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                "delay",
                () => clock.Delay(TimeSpan.MaxValue));
        }

        [Fact]
        public void Delay_MinusOne_ReturnsPendingTask()
        {
            var task = clock.Delay(-1);
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        [Fact]
        public void Delay_MinusOne_InfiniteWait()
        {
            var task = clock.Delay(-1);
            clock.SetNow(DateTime.MaxValue);
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        [Fact]
        public void Delay_MinusOne_CanBeCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var task = clock.Delay(-1, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        [Fact]
        public void Delay_AlreadyCancelled_ReturnsCanceledTask()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            
            cancellationTokenSource.Cancel();
            var task = clock.Delay(0, cancellationTokenSource.Token);

            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        [Fact]
        public void Delay_Instant_ReturnsCompletedTask()
        {
            var task = clock.Delay(0);
            Assert.Equal(Task.Delay(0).Status, task.Status);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Fact]
        public void Delay_5Sec_ReturnsPendingTask()
        {
            var task = clock.Delay(5000);
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        [Fact]
        public void Delay_5Sec_CanBeCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource();

            var task = clock.Delay(5000, cancellationTokenSource.Token);
            cancellationTokenSource.Cancel();

            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        [Fact]
        public void Delay_5Sec_NotRanJustBefore()
        {
            var task = clock.Delay(5000);
            clock.SetNow(clock.Now.AddMilliseconds(5000 - 1));
            Assert.Equal(TaskStatus.WaitingForActivation, task.Status);
        }

        [Fact]
        public void Delay_5Sec_RanWhenReachingTime()
        {
            var task = clock.Delay(5000);
            clock.SetNow(clock.Now.AddMilliseconds(5000));
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Fact]
        public void Delay_5Sec_RanWhenReachingTime_EvenWithMultiplePushes()
        {
            var task = clock.Delay(5000);
            clock.SetNow(clock.Now.AddMilliseconds(1000));
            clock.SetNow(clock.Now.AddMilliseconds(2000));
            clock.SetNow(clock.Now.AddMilliseconds(1000));
            clock.SetNow(clock.Now.AddMilliseconds(1000));
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);
        }

        [Fact]
        public async Task Delay_5Sec_RanRightOnTime()
        {
            clock.SetNow(new DateTime(2019, 10, 03, 08, 00, 00));

#pragma warning disable IDE0059 // Faux positif
            DateTime? triggerTime = null;
#pragma warning restore IDE0059 // Faux positif

            var task = TestMethod();

            clock.SetNow(clock.Now.AddMilliseconds(1000));
            clock.SetNow(clock.Now.AddMilliseconds(2000));
            clock.SetNow(clock.Now.AddMilliseconds(1000));
            clock.SetNow(clock.Now.AddMilliseconds(2000));
            await task;
            Assert.Equal(new DateTime(2019, 10, 03, 08, 00, 05), triggerTime);

            async Task TestMethod()
            {
                await clock.Delay(5000);
                triggerTime = clock.Now;
            }
        }
    }
}
