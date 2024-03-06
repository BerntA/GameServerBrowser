using GameServerList.Common.Model.A2S;
using ICSharpCode.SharpZipLib.BZip2;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GameServerList.Common.Utils;

public static class PacketUtils
{
    public static bool IsPacketSplit(this byte[] buffer) => buffer[0] == 0xFE;

    public static MultiPacketInfo ParseMultiPacketInfo(this byte[] buffer)
    {
        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms, Encoding.UTF8);

        var header = br.ReadInt32();

        if (!buffer.IsPacketSplit())
        {
            return new MultiPacketInfo
            {
                Header = header,
                Payload = buffer[(int)br.BaseStream.Position..]
            };
        }

        var id = br.ReadInt32();
        var isGoldSource = BitConverter.ToInt32(buffer, 9) == -1;

        if (isGoldSource)
        {
            var packetInformation = br.ReadByte();

            return new MultiPacketInfo
            {
                Header = header,
                Id = id,
                PacketNumber = packetInformation >> 4,
                TotalPackets = packetInformation & 0b1111,
                Payload = buffer[(int)br.BaseStream.Position..],
            };
        }

        var totalPackets = br.ReadByte();
        var currentPacket = br.ReadByte();
        var sizeOfPacket = br.ReadInt16();
        var isCompressed = ((id >> 31) & 1) == 1;

        var packetInfo = new MultiPacketInfo
        {
            Header = header,
            Id = id,
            PacketNumber = currentPacket,
            TotalPackets = totalPackets,
            MaximumPacketSize = sizeOfPacket,
            IsCompressed = isCompressed,
        };

        if (isCompressed)
        {
            packetInfo.UncompressedResponseSize = br.ReadInt32();
            packetInfo.Crc32Checksum = br.ReadInt32();
        }

        packetInfo.Payload = buffer[(int)br.BaseStream.Position..];

        return packetInfo;
    }

    public static async Task<byte[]> SendAndReceiveAsync(this UdpClient client, IPEndPoint endpoint, byte[] data, CancellationToken cancellationToken)
    {
        await client.SendAsync(data, data.Length, endpoint);
        var response = await client.ReceiveAsync(cancellationToken);
        return response.Buffer;
    }

    public static async Task<byte[]> QueryServerAsync(this UdpClient client, IPEndPoint endpoint, byte[] requestData, CancellationToken cancellationToken)
    {
        var buffer = await client.SendAndReceiveAsync(endpoint, requestData, cancellationToken);

        // Handle S2C_CHALLENGE, append 4 byte challenge
        if (buffer.Length == 9 && buffer[4] == 0x41)
        {
            if (requestData[4] == 0x55)
                buffer[4] = 0x55; // do not append challenge to player request.
            else
                buffer = [.. requestData, .. buffer[5..]];

            buffer = await client.SendAndReceiveAsync(endpoint, buffer, cancellationToken);
        }

        var multiPacketHeader = buffer.ParseMultiPacketInfo();

        if (!buffer.IsPacketSplit())
            return multiPacketHeader.Payload;

        var packets = new List<MultiPacketInfo>()
        {
            multiPacketHeader
        };

        for (var i = 1; i < multiPacketHeader.TotalPackets; i++)
        {
            var response = await client.ReceiveAsync(cancellationToken);
            packets.Add(response.Buffer.ParseMultiPacketInfo());
        }

        packets = [.. packets.OrderBy(p => p.PacketNumber)];

        var payload = packets.SelectMany(p => p.Payload).ToArray();

        if (multiPacketHeader.IsCompressed)
        {
            using var compressedMemoryStream = new MemoryStream(payload);
            using var decompressedMemoryStream = new MemoryStream();
            BZip2.Decompress(compressedMemoryStream, decompressedMemoryStream, false);
            payload = decompressedMemoryStream.ToArray();
        }

        return payload;
    }
}