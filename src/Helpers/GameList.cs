using GameServerList.Model;
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
        var text = File.ReadAllText("Data/games.json");

        if (string.IsNullOrEmpty(text))
            return;

        var data = JsonConvert.DeserializeObject<dynamic>(text);
        Games = JsonConvert.DeserializeObject<List<Game>>(data.GameList.ToString());
    }
}