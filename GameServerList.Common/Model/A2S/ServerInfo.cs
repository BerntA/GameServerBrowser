using GameServerList.Common.Utils;

namespace GameServerList.Common.Model.A2S;

public struct ServerInfo
{
    public ServerInfo(string address, BinaryReader br)
    {
        Address = address;
        Header = br.ReadByte();
        Protocol = br.ReadByte();
        Name = StringUtils.ReadNullTerminatedString(br);
        Map = StringUtils.ReadNullTerminatedString(br);
        Folder = StringUtils.ReadNullTerminatedString(br);
        Game = StringUtils.ReadNullTerminatedString(br);
        Id = br.ReadInt16();
        Players = br.ReadByte();
        MaxPlayers = br.ReadByte();
        Bots = br.ReadByte();
        ServerType = (ServerTypeFlags)br.ReadByte();
        Environment = (EnvironmentFlags)br.ReadByte();
        Visibility = (VisibilityFlags)br.ReadByte();
        Vac = (VacFlags)br.ReadByte();
        Version = StringUtils.ReadNullTerminatedString(br);

        if (br.BaseStream.Position != br.BaseStream.Length)
            ExtraDataFlag = (ExtraDataFlags)br.ReadByte();
        else
            ExtraDataFlag = ExtraDataFlags.None;

        GameId = 0;
        SteamId = 0;
        Keywords = null;
        Spectator = null;
        SpectatorPort = 0;
        Port = 0;

        if (ExtraDataFlag.HasFlag(ExtraDataFlags.Port))
        {
            Port = br.ReadInt16();
        }

        if (ExtraDataFlag.HasFlag(ExtraDataFlags.SteamId))
        {
            SteamId = br.ReadUInt64();
        }

        if (ExtraDataFlag.HasFlag(ExtraDataFlags.Spectator))
        {
            SpectatorPort = br.ReadInt16();
            Spectator = StringUtils.ReadNullTerminatedString(br);
        }

        if (ExtraDataFlag.HasFlag(ExtraDataFlags.Keywords))
        {
            Keywords = StringUtils.ReadNullTerminatedString(br);
        }

        if (ExtraDataFlag.HasFlag(ExtraDataFlags.GameId))
        {
            GameId = br.ReadUInt64();
        }
    }

    public string Address { get; set; }
    public byte Header { get; set; }
    public byte Protocol { get; set; }
    public string Name { get; set; }
    public string Map { get; set; }
    public string Folder { get; set; }
    public string Game { get; set; }
    public short Id { get; set; }
    public byte Players { get; set; }
    public byte MaxPlayers { get; set; }
    public byte Bots { get; set; }
    public ServerTypeFlags ServerType { get; set; }
    public EnvironmentFlags Environment { get; set; }
    public VisibilityFlags Visibility { get; set; }
    public VacFlags Vac { get; set; }
    public string Version { get; set; }
    public ExtraDataFlags ExtraDataFlag { get; set; }

    [Flags]
    public enum ExtraDataFlags : byte
    {
        None = 0x00,
        GameId = 0x01,
        SteamId = 0x10,
        Keywords = 0x20,
        Spectator = 0x40,
        Port = 0x80
    }

    public enum VacFlags : byte
    {
        Unsecured = 0,
        Secured = 1
    }

    public enum VisibilityFlags : byte
    {
        Public = 0,
        Private = 1
    }

    public enum EnvironmentFlags : byte
    {
        Linux = 0x6C,
        Windows = 0x77,
        Mac = 0x6D,
        MacOsX = 0x6F
    }

    public enum ServerTypeFlags : byte
    {
        Dedicated = 0x64,
        NonDedicated = 0x6C,
        SourceTv = 0x70
    }

    public ulong GameId { get; set; }
    public ulong SteamId { get; set; }
    public string Keywords { get; set; }
    public string Spectator { get; set; }
    public short SpectatorPort { get; set; }
    public short Port { get; set; }

    public GameServerItem MapToGameServerItem(Game game)
    {
        return new GameServerItem
        {
            Name = this.Name,
            Address = this.Address,
            AppId = game.AppId,
            GameDir = game.GameDir,
            CurrentPlayers = this.Players,
            MaxPlayers = this.MaxPlayers,
            Map = this.Map,
            OperatingSystem = (this.Environment == EnvironmentFlags.Windows) ? "w" : "l",
            IsDedicatedServer = (this.ServerType == ServerTypeFlags.Dedicated),
            IsVACEnabled = (this.Vac == VacFlags.Secured),
            Bots = this.Bots,
            Version = this.Version,
        };
    }
}