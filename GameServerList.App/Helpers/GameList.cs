using GameServerList.Common.Model;
using GameServerList.Common.Utils;
using Newtonsoft.Json;

namespace GameServerList.Helpers;

public static class GameList
{
    public static List<Game> Games { get; set; }

    public static Game? GetGameByIndex(int index)
    {
        if (index < 0 || index >= Games.Count)
            return null;

        return Games[index];
    }

    public static void LoadGameList()
    {
        var gameData = FileUtils.LoadDataFromFile<List<Game>>("Data/games.json");
        if (gameData is null)
            return;

        Games = [.. gameData.OrderBy(g => g.Name)];

        foreach (var game in Games)
        {
            var loadServerList = (game.UseDefinedServerList ?? false);
            if (!loadServerList) continue;

            var serverListData = FileUtils.LoadDataFromFile<List<string>>($"Data/{game.AppId}_addresses.json");
            if (serverListData is null) continue;

            game.Servers = serverListData;
        }
    }
}