using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Model.A2S;
using GameServerList.Common.Utils;
using System.Collections.Concurrent;

namespace GameServerList.Tests;

public class AdHocTests
{
    private const int timeout = 20000;

    public AdHocTests()
    {
    }

    [Fact]
    public async Task ServerInfoStressTest()
    {
        var size = 10;
        var success = 0;

        for (var i = 0; i < size; i++)
        {
            var serverInfo = await A2SQuery.QueryServerInfo("185.216.147.190:27025", timeout);
            if (serverInfo is null)
                continue;

            success++;
        }

        Assert.True(success == size);
    }

    [Fact]
    public async Task PlayerInfoStressTest()
    {
        var size = 10;
        var success = 0;

        for (var i = 0; i < size; i++)
        {
            var playerInfo = await A2SQuery.QueryPlayerInfo("185.216.147.190:27025", timeout);
            if (playerInfo is null)
                continue;

            success++;
        }

        Assert.True(success == size);
    }

    [Fact]
    public async Task MasterServerListTest()
    {
        var servers = await A2SQuery.QueryServerList(
            MasterServer.Source,
            new Game { AppId = 215, GameDir = "hidden" },
            timeout
        );

        var tasks = servers.Select(s => A2SQuery.QueryServerInfo(s.Address));
        var serverInfo = await Task.WhenAll(tasks);

        Assert.True(serverInfo?.Any(s => s.HasValue) ?? false);
    }

    [Fact]
    public async Task MasterServerHLTest()
    {
        var servers = await A2SQuery.QueryServerList(
            MasterServer.GoldSrc,
            new Game { AppId = 70, GameDir = "valve" },
            timeout
        );
        Assert.True(servers?.Any() ?? false);
    }

    [Fact]
    public async Task QueryServerInfoCS2Test()
    {
        var info = await A2SQuery.QueryServerInfo("216.52.148.47:27015", timeout);
        Assert.NotNull(info);
    }

    [ManualFact]
    public async Task ValidateServerList()
    {
        var servers = FileUtils.LoadDataFromFile<List<string>>("../../../../GameServerList.App/Data/730_addresses.json");
        var validated = new ConcurrentBag<string>();

        await Parallel.ForEachAsync(servers, async (s, _) =>
        {
            var obj = await A2SQuery.QueryServerInfo(s, 5000);
            if (obj is not null && obj.HasValue)
                validated.Add(s);
        });

        FileUtils.WriteDataToFile("./validated.json", validated.Distinct().ToList());
    }
}