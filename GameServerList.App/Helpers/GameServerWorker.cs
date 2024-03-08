using GameServerList.Common.External;
using GameServerList.Common.Model.A2S;
using GameServerList.Common.Model;
using System.Collections.Concurrent;

namespace GameServerList.Helpers;

public class GameServerWorker : BackgroundService
{
    public GameServerWorker()
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var nextUpdateAt = DateTime.MinValue;
        var games = GameList
            .Games
            .Where(g => (g.UseDefinedServerList.HasValue && g.UseDefinedServerList.Value) || g.MasterServer.HasValue)
            .ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow <= nextUpdateAt)
            {
                Thread.Sleep(100); // put the thread on hold while waiting
                continue;
            }

            foreach (var game in games)
            {
                try
                {
                    List<GameServerItem> servers = null;

                    if (game.UseDefinedServerList ?? false)
                        servers = await QueryServers(game, game.Servers);
                    else if (game.MasterServer.HasValue)
                    {
                        var legacyServers = await A2SQuery.QueryServerList(
                            A2SQuery.GetMasterServerAddress(game.MasterServer.Value), game
                        );
                        servers = await QueryServers(game, legacyServers.Select(s => s.Address).ToList());
                    }

                    game.GameServers = servers;
                }
                catch
                {
                    // skip
                }
            }

            nextUpdateAt = DateTime.UtcNow.AddMinutes(15);
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