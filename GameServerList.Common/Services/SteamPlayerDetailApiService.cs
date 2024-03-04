using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Model.A2S;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace GameServerList.Common.Services;

public class SteamPlayerDetailApiService
{
    private readonly IMemoryCache _cache;

    public SteamPlayerDetailApiService(IConfiguration config, IMemoryCache memoryCache)
    {
        _cache = memoryCache;
    }

    public async Task<List<PlayerInfo>> FetchPlayerDetails(GameServerItem server)
    {
        if (server.CurrentPlayers <= 0)
            return [];

        var address = server.Address;
        var key = $"PlayerDetails-{address}";
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);

            var details = new List<PlayerInfo>();
            for (var i = 0; i < 3; i++) // X tries
            {
                details = await A2SQuery.QueryPlayerInfo(address, 5000);
                if (details.Count > 0)
                    break;
            }

            if (details is null || details.Count == 0)
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1);

            return details
                .OrderByDescending(p => p.Score)
                .ToList();
        });
    }
}