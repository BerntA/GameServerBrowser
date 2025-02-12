﻿using GameServerList.Common.Services;

namespace GameServerList.Helpers;

public class GameServerWorker : BackgroundService
{
    public GameServerWorker()
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);
        var nextUpdateAt = DateTime.MinValue;
        var games = GameList
            .Games
            .Where(g => g.UsesBackgroundService())
            .ToList();

        while (!stoppingToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow <= nextUpdateAt)
            {
                await Task.Delay(5000, stoppingToken); // put the thread on hold while waiting
                continue;
            }

            foreach (var game in games)
            {
                try
                {
                    game.GameServers = await SteamServerBrowserApiService.Query(game, 650);
                }
                catch
                {
                    // skip
                }
            }

            nextUpdateAt = DateTime.UtcNow.AddMinutes(30);
        }
    }
}