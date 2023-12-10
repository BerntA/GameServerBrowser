using GameServerList.Common.Model.A2S;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace GameServerList.Common.External;

public static class A2SQuery
{
    private static readonly byte[] ServerRequest = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
    private static readonly byte[] PlayerRequest = { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF };

    public static async Task<ServerInfo?> QueryServerInfo(string address, int timeout = 2500)
    {
        using var udpClient = new UdpClient();
        using var cancellationToken = new CancellationTokenSource(timeout);

        try
        {
            var endPoint = GetIPEndPoint(address);
            var buffer = await GetData(udpClient, endPoint, ServerRequest, cancellationToken.Token);

            if (buffer.Length == 25 && buffer[4] == 0x41)
            {
                buffer[4] = 0x54;
                buffer = await GetData(udpClient, endPoint, buffer, cancellationToken.Token);
            }

            var ms = new MemoryStream(buffer);
            var br = new BinaryReader(ms, Encoding.UTF8);

            ms.Seek(4, SeekOrigin.Begin);
            var info = new ServerInfo(ref br);

            br.Close();
            ms.Close();

            return info;
        }
        catch
        {
            return null;
        }
        finally
        {
            udpClient.Close();
        }
    }

    public static async Task<List<PlayerInfo>> QueryPlayerInfo(string address, int timeout = 2500)
    {
        using var udpClient = new UdpClient();
        using var cancellationToken = new CancellationTokenSource(timeout);

        try
        {
            var endPoint = GetIPEndPoint(address);
            var buffer = await GetData(udpClient, endPoint, PlayerRequest, cancellationToken.Token);

            if (buffer.Length == 9 && buffer[4] == 0x41)
            {
                buffer[4] = 0x55;
                buffer = await GetData(udpClient, endPoint, buffer, cancellationToken.Token);

                var ms = new MemoryStream(buffer);
                var br = new BinaryReader(ms, Encoding.UTF8);

                ms.Seek(4, SeekOrigin.Begin);
                _ = br.ReadByte(); // skip header

                var playerInfo = new PlayerInfo[br.ReadByte()];
                for (var i = 0; i < playerInfo.Length; i++)
                    playerInfo[i] = new PlayerInfo(ref br);

                br.Close();
                ms.Close();

                return [.. playerInfo];
            }

            return null;
        }
        catch
        {
            return null;
        }
        finally
        {
            udpClient.Close();
        }
    }

    private static async Task<byte[]> GetData(UdpClient client, IPEndPoint endpoint, byte[] data, CancellationToken cancellationToken)
    {
        await client.SendAsync(data, data.Length, endpoint);
        var response = await client.ReceiveAsync(cancellationToken);
        return response.Buffer;
    }

    private static IPEndPoint GetIPEndPoint(string address)
    {
        var items = address.Split(":");

        _ = IPAddress.TryParse(items[0], out var ip);
        _ = ushort.TryParse(items[1], out var port);

        return new IPEndPoint(ip, port);
    }
}