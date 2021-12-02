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
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
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
            if(FightTask.IsCompleted || FightTask.IsFaulted || FightTask.IsCanceled)
            {
                FightTask.Dispose();
            }
            if (ClearTask.IsCompleted || ClearTask.IsFaulted || ClearTask.IsCanceled)
            {
                ClearTask.Dispose();
            }
        }

        public async Task InflictDamages()
        {
            while (true)
            {
                List<Unit> units = _userService.GetFighterUnits();
                List<Unit> targets;
                long currentSecond = new DateTimeOffset(_clock.Now).ToUnixTimeSeconds();
                foreach (FightingUnit unit in units)
                {
                    int damage;

                    // Time requirement & define damage inflicted
                    if (currentSecond % unit.Timeout == 0)
                    {
                        damage = unit.Damage;
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
                    Unit target = GetTarget(unit.GetType(), unit.Id, targets);

                    // Protection(s)
                    if (target.GetType() == typeof(Bomber) && unit.GetType() == typeof(Cruiser)) damage = damage / 10;

                    // Inflict damages
                    (target as FightingUnit).Health -= damage;
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
                    List<Unit> unitsToRemove = keyValue.Value
                        .FindAll(unit => FightingUnit.GetFightingTypes.Contains(unit.GetType()))
                        .FindAll(Unit => (Unit as FightingUnit).Health <= 0);
                    foreach(Unit unit in unitsToRemove)
                    {
                        _userService.Units[keyValue.Key].Remove(unit);
                    }
                }
                await _clock.Delay(100);
            }
        }

        public Unit GetTarget(Type unitType, string unitId, List<Unit> units)
        {
            Unit unit;
            switch (unitType.Name.ToString())
            {
                case "Fighter":
                    unit = units.Find(u => u.GetType() == typeof(Bomber) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Fighter) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Cruiser) && u.Id != unitId);
                    if (unit != null) return unit;
                    break;

                case "Bomber":
                    unit = units.Find(u => u.GetType() == typeof(Cruiser) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Bomber) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Fighter) && u.Id != unitId);
                    if (unit != null) return unit;
                    break;

                case "Cruiser":
                    unit = units.Find(u => u.GetType() == typeof(Fighter) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Cruiser) && u.Id != unitId);
                    if (unit != null) return unit;
                    unit = units.Find(u => u.GetType() == typeof(Bomber) && u.Id != unitId);
                    if (unit != null) return unit;
                    break;
                default:
                    throw new Exception("Could not find a target in the given list");
            }
            throw new Exception($"Could not find a target in the given list for unit type {unitType}");
        }
    }
}

