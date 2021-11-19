using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shard.Shared.Core;
using Shard.Uni.Models;

namespace Shard.Uni.Services
{
    // TODO: HostedService
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio#timed-background-tasks
    public class FightService : IHostedService, IDisposable
    {
        private UserService _userService;
        private IClock _clock;

        private Task FightTask { get; set; }
        private Task ClearTask { get; set; }

        public FightService(UserService userService, IClock clock)
        {
            _userService = userService;
            _clock = clock;
            // TODO: remove
            // Make damages
            FightTask = InflictDamages();
            // Clear units
            ClearTask = ClearUnits();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            // TODO:
            // Make damages
            // FightTask = InflictDamages();
            // Clear units
            // ClearTask = ClearUnits();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            // _timer?.Dispose();
        }

        public async Task InflictDamages()
        {
            while (true)
            {
                List<Unit> units = _userService.GetFighterUnits();
                List<Unit> targets;
                foreach (Unit unit in units)
                {
                    if(unit.Planet != null)
                    {
                        targets = _userService.GetUnitsOnPlanet(unit.Planet);
                    }
                    else
                    {
                        targets = _userService.GetUnitsInSystem(unit.System);
                    }
                }
                await _clock.Delay(new TimeSpan(0, 0, 1));
            }
        }

        public async Task ClearUnits()
        {
            while (true)
            {
                List<Unit> units = _userService.GetAllUnits();
                foreach (Unit unit in units)
                {

                }
                await _clock.Delay(new TimeSpan(0, 0, 1));
            }
        }
    }
}

