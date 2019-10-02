using System;
using System.Collections.Generic;
using System.Text;

namespace Shard.Shared.Core
{
    public interface IClock
    {
        DateTime Now { get; }
    }
}
