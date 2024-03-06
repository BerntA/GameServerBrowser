using GameServerList.Common.Utils;

namespace GameServerList.Common.Model.A2S;

public struct PlayerInfo
{
    public PlayerInfo(BinaryReader br)
    {
        Index = br.ReadByte();
        Name = StringUtils.ReadNullTerminatedString(br);
        Score = br.ReadInt32();
        Duration = br.ReadSingle();
    }

    public byte Index { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public float Duration { get; set; }

    public string GetDurationPlayed()
    {
        if (float.IsNaN(Duration))
            return string.Empty;

        if (Duration > 86400)
            return "Days";

        return TimeSpan
            .FromSeconds(Duration)
            .ToString(@"hh\:mm\:ss");
    }

    public override string ToString()
    {
        return Name;
    }
}