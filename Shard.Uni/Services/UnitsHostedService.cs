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
    public class UnitsHostedService : IHostedService, IDisposable
    {
        private UserService _userService;
        private IClock _clock;

        private Task FightTask { get; set; }
        private Task ClearTask { get; set; }

        public UnitsHostedService(UserService userService, IClock clock)
        {
            _userService = userService;
            _clock = clock;
            // TODO: remove
            // Make damages
            // FightTask = InflictDamages();
            // Clear units
            // ClearTask = ClearUnits();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            // TODO:
            // Make damages
            FightTask = InflictDamages();
            // Clear units
            ClearTask = ClearUnits();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            FightTask.Dispose();
            ClearTask.Dispose();
        }

        public async Task InflictDamages()
        {
            while (true)
            {
                List<Unit> units = _userService.GetFighterUnits();
                List<Unit> targets;
                long currentSecond = new DateTimeOffset(_clock.Now).ToUnixTimeSeconds();
                foreach (Unit unit in units)
                {
                    int damage;

                    // Time requirement & define damage inflicted
                    if(unit.Type == "fighter" && currentSecond % 6 == 0)
                    {
                        damage = 10;
                    }
                    else if(unit.Type == "bomber" && currentSecond % 60 == 0)
                    {
                        damage = 1000;
                    }
                    else if(unit.Type == "cruiser" && currentSecond % 6 == 0)
                    {
                        damage = 6;
                    }
                    else
                    {
                        // will only trigger when the (current second % period) != 0
                        continue;
                    }

                    // Find target
                    if(unit.Planet != null)
                    {
                        targets = _userService.GetUnitsOnPlanet(unit.Planet);
                    }
                    else
                    {
                        targets = _userService.GetUnitsInSystem(unit.System);
                    }
                    Unit target = GetTarget(unit.Type, targets);

                    // Inflict damages
                    target.Health -= damage;
                }
                await _clock.Delay(new TimeSpan(0, 0, 1));
            }
        }

        public async Task ClearUnits()
        {
            while (true)
            {
                foreach (KeyValuePair<string, List<Unit>> keyValue in _userService.Units)
                {
                    List<Unit> unitsToRemove = keyValue.Value.FindAll(Unit => Unit.Health <= 0);
                    foreach(Unit unit in unitsToRemove)
                    {
                        _userService.Units[keyValue.Key].Remove(unit);
                    }
                }
                await _clock.Delay(new TimeSpan(0, 0, 1));
            }
        }

        public Unit GetTarget(string unitType, List<Unit> units)
        {
            Unit unit;
            switch (unitType)
            {
                case "fighter":
                    unit = units.Find(u => u.Type == "bomber");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "fighter");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "cruiser");
                    if (unit != null) return unit;
                    break;

                case "bomber":
                    unit = units.Find(u => u.Type == "cruiser");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "bomber");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "fighter");
                    if (unit != null) return unit;
                    break;

                case "cruiser":
                    unit = units.Find(u => u.Type == "fighter");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "cruiser");
                    if (unit != null) return unit;
                    unit = units.Find(u => u.Type == "bomber");
                    if (unit != null) return unit;
                    break;
                default:
                    throw new Exception("Could not find a target in the given list");
            }
            throw new Exception($"Could not find a target in the given list for unit type {unitType}");
        }
    }
}

