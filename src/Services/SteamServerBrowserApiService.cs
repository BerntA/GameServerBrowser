using GameServerList.Model;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace GameServerList.Services;

public class SteamServerBrowserApiService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly string _apiKey;
    private readonly int _querySize;
    private readonly JsonSerializerSettings _jsonSerializerSettings;

    public SteamServerBrowserApiService(IConfiguration config, IMemoryCache memoryCache)
    {
        var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

        _httpClient = new HttpClient(httpClientHandler);
        _httpClient.BaseAddress = new Uri(config["SteamAPIUrl"]);

        _cache = memoryCache;
        _apiKey = config["SteamAPIKey"];
        _querySize = int.TryParse(config["QuerySize"], out var size) ? size : 1000;

        _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
        };
    }

    public async Task<List<GameServerItem>> FetchServers(Game? game)
    {
        if (game is null)
            return [];

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
            return [];
        }
    }
}