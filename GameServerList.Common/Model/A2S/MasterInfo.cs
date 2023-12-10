namespace GameServerList.Common.Model.A2S;

public struct MasterInfo
{
    public MasterInfo(ref BinaryReader binReader)
    {
        var octets = new byte[4];
        for (var i = 0; i < octets.Length; i++)
            octets[i] = binReader.ReadByte();

        ushort port;
        byte portByte1 = binReader.ReadByte();
        byte portByte2 = binReader.ReadByte();

        if (BitConverter.IsLittleEndian)
            port = BitConverter.ToUInt16([portByte2, portByte1], 0);
        else
            port = BitConverter.ToUInt16([portByte1, portByte2], 0);

        Address = $"{string.Join(".", octets)}:{port}";
        IsSeed = Address.EndsWith(":0");
    }

    public string Address { get; set; }
    public bool IsSeed { get; set; }
}