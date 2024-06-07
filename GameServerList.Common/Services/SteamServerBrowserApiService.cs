using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Model.A2S;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Concurrent;
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

            if (game.NoBackgroundService ?? false)
                return await Query(game);
            else if (game.UsesBackgroundService())
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

    public static async Task<List<GameServerItem>> Query(Game game, int timeoutServers = 1500, int timeoutMasterServer = 15000)
    {
        if (game.UseDefinedServerList ?? false)
            return await QueryServers(game, game.Servers, timeoutServers);
        else if (game.MasterServer.HasValue)
        {
            var legacyServers = await A2SQuery.QueryServerList(game.MasterServer.Value, game, timeoutMasterServer);
            return await QueryServers(game, legacyServers.Select(s => s.Address).ToList(), timeoutServers);
        }
        return [];
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

    private static bool IsServerValid(Game game, ServerInfo? server)
    {
        if (server is null || !server.HasValue)
            return false;

        if (server.Value.MaxPlayers > 128 || server.Value.MaxPlayers <= 1 || server.Value.Players > server.Value.MaxPlayers)
            return false;

        if (game.MasterServer.HasValue && game.MasterServer.Value == MasterServer.GoldSrc && !server.Value.Version.EndsWith("/Stdio"))
            return false;

        return true;
    }

    private static async Task<List<GameServerItem>> QueryServers(Game game, List<string> servers, int timeout = 1500)
    {
        var items = new ConcurrentBag<GameServerItem>();
        await Parallel.ForEachAsync(servers, async (address, _) =>
        {
            var obj = await A2SQuery.QueryServerInfo(address, timeout);
            if (IsServerValid(game, obj))
                items.Add(obj.Value.MapToGameServerItem(game));
        });
        return [.. items];
    }
}