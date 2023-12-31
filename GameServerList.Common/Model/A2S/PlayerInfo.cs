﻿using GameServerList.Common.Utils;

namespace GameServerList.Common.Model.A2S;

public struct PlayerInfo
{
    public PlayerInfo(ref BinaryReader binReader)
    {
        Index = binReader.ReadByte();
        Name = StringUtils.ReadNullTerminatedString(ref binReader);
        Score = binReader.ReadInt32();
        Duration = binReader.ReadSingle();
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