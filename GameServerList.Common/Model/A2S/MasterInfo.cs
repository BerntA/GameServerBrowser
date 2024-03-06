namespace GameServerList.Common.Model.A2S;

public struct MasterInfo
{
    public MasterInfo(BinaryReader br)
    {
        var octets = new byte[4];
        for (var i = 0; i < octets.Length; i++)
            octets[i] = br.ReadByte();

        ushort port;
        byte portByte1 = br.ReadByte();
        byte portByte2 = br.ReadByte();

        if (BitConverter.IsLittleEndian)
            port = BitConverter.ToUInt16([portByte2, portByte1], 0);
        else
            port = BitConverter.ToUInt16([portByte1, portByte2], 0);

        IP = string.Join(".", octets);
        Address = $"{IP}:{port}";
        IsSeed = Address.EndsWith(":0");
    }

    public string Address { get; set; }
    public string IP { get; set; }
    public bool IsSeed { get; set; }

    public override readonly string ToString() => Address;
}