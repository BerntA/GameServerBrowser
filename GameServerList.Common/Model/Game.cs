﻿namespace GameServerList.Common.Model;

public class Game
{
    public string? Name { get; set; }
    public string? GameDir { get; set; }
    public string? Icon { get; set; }
    public long? AppId { get; set; }
    public bool? UseLegacyLookup { get; set; }
}