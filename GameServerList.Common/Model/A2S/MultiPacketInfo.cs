namespace GameServerList.Common.Model.A2S;

public struct MultiPacketInfo
{
    public int Header { get; set; }
    public int Id { get; set; }
    public int TotalPackets { get; set; }
    public int PacketNumber { get; set; }
    public short MaximumPacketSize { get; set; }
    public bool IsCompressed { get; set; }
    public int? UncompressedResponseSize { get; set; }
    public int? Crc32Checksum { get; set; }
    public byte[] Payload { get; set; }
}