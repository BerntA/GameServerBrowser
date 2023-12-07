using Newtonsoft.Json;

namespace GameServerList.Model;

public class GameServerItem
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("addr")]
    public string? Address { get; set; }

    [JsonProperty("gameport")]
    public int? Port { get; set; }

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
}