using GameServerList.Analyzer;
using GameServerList.Common.Model;

ulong appId;
AnalyzerTasks task;

Console.Write("Please specify the appId: ");

while (!ulong.TryParse(Console.ReadLine(), out appId) || appId == 0UL)
    Console.Write("Invalid appId specified, try again: ");

Console.WriteLine($"Which task do you wish to perform? ({string.Join(", ", Enum.GetNames<AnalyzerTasks>())})");

while (!Enum.TryParse(Console.ReadLine(), true, out task))
    Console.Write("Unknown task specified, try again: ");

Console.WriteLine($"Running task: {task}");

var game = new Game
{
    AppId = appId,
};

switch (task)
{
    case AnalyzerTasks.DISCOVER:
        await DiscoverServers.Execute(game, 1800000);
        break;

    case AnalyzerTasks.PRUNE:
        await PruneServers.Execute(game, 4000);
        break;
}