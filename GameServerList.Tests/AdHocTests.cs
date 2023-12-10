using GameServerList.Common.External;
using GameServerList.Common.Model;

namespace GameServerList.Tests;

public class AdHocTests
{
    private const int timeout = 10000;

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
            A2SQuery.MasterAddress,
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
            A2SQuery.MasterAddress,
            new Game { AppId = 70, GameDir = "valve" },
            timeout
        );
        Assert.True(servers?.Any() ?? false);
    }
}