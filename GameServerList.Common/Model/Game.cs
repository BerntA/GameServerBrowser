﻿using GameServerList.Common.Model.A2S;
using Newtonsoft.Json;

namespace GameServerList.Common.Model;

public class Game
{
    public string? Name { get; set; }
    public string? GameDir { get; set; }
    public string? Icon { get; set; }
    public ulong? AppId { get; set; }
    public MasterServer? MasterServer { get; set; }
    public bool? UseDefinedServerList { get; set; }
    public bool? NoBackgroundService { get; set; }
    public string? Filters { get; set; }

    [JsonIgnore]
    public List<string>? Servers { get; set; }

    [JsonIgnore]
    public List<GameServerItem>? GameServers { get; set; }

    public bool UsesBackgroundService()
    {
        if (NoBackgroundService ?? false)
            return false;

        if ((UseDefinedServerList ?? false) || MasterServer.HasValue)
            return true;

        return false;
    }
}