using GameServerList.Common.Model.A2S;

namespace GameServerList.Common.Model;

public class Game
{
    public string? Name { get; set; }
    public string? GameDir { get; set; }
    public string? Icon { get; set; }
    public long? AppId { get; set; }
    public MasterServer? MasterServer { get; set; }
    public bool? UniqueIPPerServer { get; set; }
    public string? Filters { get; set; }
}