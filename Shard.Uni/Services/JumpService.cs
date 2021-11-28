using Shard.Uni.Models;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Shard.Uni.Services
{
    public class JumpService
    {
        private Wormhole _wormholes;
        private HttpClient _client;
        private bool _authenticated;
        public JumpService(Wormhole wormhole, IHttpClientFactory httpClientFactory)
        {
            _wormholes = wormhole;
            _client = httpClientFactory.CreateClient();
            _authenticated = false;
        }

        public KeyValuePair<string, WormholeData> GetShardData(string shard)
        {
            KeyValuePair<string, WormholeData> theShard;
            try
            {
                theShard = _wormholes.shards.First(Shard => Shard.Key == shard);
            }
            catch (InvalidOperationException)
            {
                throw new Exception($"Could not find any shard matching with: {shard}");
            }
            catch (Exception)
            {
                throw;
            }
            return theShard;
        }

        public void SetAuthorization(KeyValuePair<string, WormholeData> theShard)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"shard-{theShard.Value.User}:{theShard.Value.SharedPassword}")
                )
            );
        }

        public async Task PutUserInDistantShard(User user, string shard)
        {
            var theShard = GetShardData(shard);
            var body = JsonContent.Create(user);
            
            HttpResponseMessage httpResponse = await _client.PutAsync($"{theShard.Value.BaseUri}/users/{user.Id}", body);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }
        }

        public async Task<string> PutUnitInDistantShard(string userId, Unit unit, string shard)
        {
            var theShard = GetShardData(shard);
            var body = JsonContent.Create(unit, unit.GetType());

            HttpResponseMessage httpResponse = await _client.PutAsync($"{theShard.Value.BaseUri}/users/{userId}/units/{unit.Id}", body);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception(httpResponse.StatusCode.ToString());
            }
            return $"{theShard.Value.BaseUri}/users/{userId}/units/{unit.Id}";
        }

        public async Task SystemAndPlanetExists(string system, string planet, string shard)
        {
            var theShard = GetShardData(shard);
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await _client.GetAsync($"{theShard.Value.BaseUri}/Systems/{system}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error on request {ex.Message}");
            }
            if (!httpResponse.IsSuccessStatusCode) 
            {
                throw new Exception($"System {system} not found");
            }
            else
            {
                if(planet != null)
                {
                    HttpResponseMessage httpResponsePlanet = await _client.GetAsync($"{theShard.Value.BaseUri}/Systems/{system}/planets/planet");
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new Exception($"Planet {planet} not found in system {system}");
                    }
                }
            }
        }

        public async Task<string> Jump(Unit unit, User user, string shard)
        {
            await PutUserInDistantShard(user, shard);
            return await PutUnitInDistantShard(user.Id, unit, shard);
        }

        public void DeleteUnitFromCurrentShard(Unit unit)
        {
            
        }
    }
}
