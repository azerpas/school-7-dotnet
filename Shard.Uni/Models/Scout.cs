﻿using System;
namespace Shard.Uni.Models
{
    public class Scout : Unit
    {
        public Scout(string system, string? planet) : base(system, planet) { }
        public Scout(string id, string system, string? planet) : base(id, system, planet) { }
    }
}

