using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Utils;

namespace GameServerList.Analyzer;

public static class PruneServers
{
    public static async Task Execute(Game game, int timeout)
    {
        var oldServerList = FileUtils.LoadDataFromFile<List<string>>($"./out/{game.AppId}_addresses.json");
        var newServerList = new List<string>();

        if (oldServerList is null || oldServerList.Count == 0)
        {
            Console.WriteLine("No servers found!");
            return;
        }

        foreach (var address in oldServerList)
        {
            var serverInfo = await A2SQuery.QueryServerInfo(address, timeout);
            if (serverInfo is null)
            {
                Console.WriteLine($"{address} is not responding -> removed.");
                continue;
            }
            newServerList.Add(address);
        }

        if (oldServerList.Count == newServerList.Count)
            Console.WriteLine("No pruning needed, all servers responded!");
        else
        {
            Console.WriteLine($"Pruned server list from {oldServerList.Count} to {newServerList.Count}!");
            FileUtils.WriteDataToFile($"./out/{game.AppId}_addresses_pruned.json", newServerList);
        }

        Console.ReadKey();
    }
}