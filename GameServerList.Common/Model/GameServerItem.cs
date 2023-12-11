using GameServerList.Common.Model.A2S;
using Newtonsoft.Json;

namespace GameServerList.Common.Model;

public class GameServerItem
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("addr")]
    public string? Address { get; set; }

    [JsonProperty("appid")]
    public long? AppId { get; set; }

    [JsonProperty("gamedir")]
    public string? GameDir { get; set; }

    [JsonProperty("players")]
    public int? CurrentPlayers { get; set; }

    [JsonProperty("max_players")]
    public int? MaxPlayers { get; set; }

    [JsonProperty("map")]
    public string? Map { get; set; }

    #region ServerInfo
    [JsonProperty("os")]
    public string? OperatingSystem { get; set; }

    [JsonProperty("version")]
    public string? Version { get; set; }

    [JsonProperty("secure")]
    public bool? IsVACEnabled { get; set; }

    [JsonProperty("dedicated")]
    public bool? IsDedicatedServer { get; set; }

    [JsonProperty("bots")]
    public int? Bots { get; set; }
    #endregion

    #region PlayerInfo
    [JsonIgnore]
    public List<PlayerInfo>? ActivePlayers { get; set; }

    [JsonIgnore]
    public bool ShouldShowDetails { get; set; } = false;
    #endregion
}