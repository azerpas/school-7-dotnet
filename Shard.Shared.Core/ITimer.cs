﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.Shared.Core
{
    public interface ITimer
    {
        bool Change(int dueTime, int period);
        bool Change(long dueTime, long period);
        bool Change(TimeSpan dueTime, TimeSpan period);
        bool Change(uint dueTime, uint period);
        void Dispose();
        bool Dispose(WaitHandle notifyObject);
        ValueTask DisposeAsync();
    }
}