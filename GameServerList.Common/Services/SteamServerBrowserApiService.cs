using GameServerList.Common.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net;

namespace GameServerList.Common.Services;

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

        var key = $"Servers-{game.AppId}-{game.Name}".ToUpper();

        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            if ((game.UseDefinedServerList ?? false) || game.MasterServer.HasValue)
                return (game.GameServers is null) ? [] : game.GameServers;
            else
            {
                var gamedirFilter = string.IsNullOrEmpty(game.GameDir) ? string.Empty : $"\\gamedir\\{game.GameDir}";
                var extraFilters = string.IsNullOrEmpty(game.Filters) ? string.Empty : game.Filters;

                return await Fetch<GameServerItem>(
                    $"IGameServersService/GetServerList/v1/?key={_apiKey}&limit={_querySize}&filter=appid\\{game.AppId}{gamedirFilter}{extraFilters}"
                );
            }
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