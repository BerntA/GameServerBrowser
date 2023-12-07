using GameServerList.Model;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace GameServerList.Services;

public class SteamServerBrowserApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly int _querySize;

    public SteamServerBrowserApiService(IConfiguration config, IMemoryCache memoryCache)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(config["SteamAPIUrl"]);
        _cache = memoryCache;
        _apiKey = config["SteamAPIKey"];
        _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };
        _querySize = int.TryParse(config["QuerySize"], out var size) ? size : 1000;
    }

    public async Task<List<GameServerItem>> FetchServers(Game? game)
    {
        if (game is null)
            return new List<GameServerItem>();

        var key = $"{game.AppId}{(string.IsNullOrEmpty(game.GameDir) ? string.Empty : $"-{game.GameDir}")}";

        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            var gamedirFilter = string.IsNullOrEmpty(game.GameDir) ? string.Empty : $"\\gamedir\\{game.GameDir}";

            var servers = await Fetch<GameServerItem>(
                $"IGameServersService/GetServerList/v1/?key={_apiKey}&limit={_querySize}&filter=appid\\{game.AppId}{gamedirFilter}"
            );

            return servers
                .OrderByDescending(s => s.CurrentPlayers)
                .ToList();
        });
    }

    private async Task<List<T>> Fetch<T>(string url)
    {
        try
        {
            using var request = await _httpClient.GetAsync(url);

            request.EnsureSuccessStatusCode();
            var response = await request.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(response)!;
            var data = JsonConvert.DeserializeObject<List<T>>(result.response.servers.ToString(), _jsonSerializerSettings)!;

            return data;
        }
        catch
        {
            return new List<T>();
        }
    }
}