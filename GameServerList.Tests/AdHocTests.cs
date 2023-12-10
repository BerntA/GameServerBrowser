using GameServerList.Common.External;

namespace GameServerList.Tests;

public class AdHocTests
{
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
            var serverInfo = await A2SQuery.QueryServerInfo("185.216.147.190:27025");
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
            var playerInfo = await A2SQuery.QueryPlayerInfo("185.216.147.190:27025");
            if (playerInfo is null)
                continue;

            success++;
        }

        Assert.True(success == size);
    }
}