using GameServerList.Common.Model.A2S;
using System.Net.Sockets;
using System.Net;
using System.Text;
using GameServerList.Common.Utils;
using GameServerList.Common.Model;

namespace GameServerList.Common.External;

public static class A2SQuery
{
    private static readonly byte[] ServerRequest = { 0xFF, 0xFF, 0xFF, 0xFF, 0x54, 0x53, 0x6F, 0x75, 0x72, 0x63, 0x65, 0x20, 0x45, 0x6E, 0x67, 0x69, 0x6E, 0x65, 0x20, 0x51, 0x75, 0x65, 0x72, 0x79, 0x00 };
    private static readonly byte[] PlayerRequest = { 0xFF, 0xFF, 0xFF, 0xFF, 0x55, 0xFF, 0xFF, 0xFF, 0xFF };

    public const string SourceMasterServer = "208.64.200.65:27015";
    public const string GoldSrcMasterServer = "208.78.164.209:27011";

    public static string GetMasterServerAddress(MasterServer masterServer)
    {
        return masterServer switch
        {
            MasterServer.Source => SourceMasterServer,
            MasterServer.GoldSrc => GoldSrcMasterServer,
            _ => throw new ArgumentException("Invalid master server!"),
        };
    }

    public static async Task<ServerInfo?> QueryServerInfo(string address, int timeout = 2000)
    {
        using var udpClient = new UdpClient();
        using var cancellationToken = new CancellationTokenSource(timeout);

        try
        {
            var endPoint = GetIPEndPoint(address);
            var buffer = await GetData(udpClient, endPoint, ServerRequest, cancellationToken.Token);

            // Handle S2C_CHALLENGE, append 4 byte challenge
            if (buffer.Length == 9 && (buffer[4] == 0x41 || buffer[4] == 0x65))
            {
                var packetWithChallenge = ServerRequest.Concat(buffer[5..]).ToArray();
                buffer = await GetData(udpClient, endPoint, packetWithChallenge, cancellationToken.Token);
            }

            var ms = new MemoryStream(buffer);
            var br = new BinaryReader(ms, Encoding.UTF8);
            ms.Seek(4, SeekOrigin.Begin);

            var info = new ServerInfo(address, ref br);

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

            return [];
        }
        catch
        {
            return [];
        }
        finally
        {
            udpClient.Close();
        }
    }

    public static async Task<List<MasterInfo>> QueryServerList(string masterServerAddress, Game targetGame, int timeout = 15000)
    {
        using var udpClient = new UdpClient();
        using var cancellationToken = new CancellationTokenSource(timeout);
        var servers = new List<MasterInfo>();

        try
        {
            var info = new MasterInfo { Address = "0.0.0.0:0", IsSeed = false };
            var endPoint = GetIPEndPoint(masterServerAddress);

            do
            {
                var bytes = GetMasterQueryRequest(info.Address, targetGame);
                var buffer = await GetData(udpClient, endPoint, bytes, cancellationToken.Token);

                var ms = new MemoryStream(buffer);
                var br = new BinaryReader(ms, Encoding.UTF8);
                ms.Seek(6, SeekOrigin.Begin);

                while (ms.Position < ms.Length)
                {
                    info = new MasterInfo(ref br);
                    if (info.IsSeed)
                        break; // break if last item was read out.
                    servers.Add(info);
                }

                br.Close();
                ms.Close();
            } while (!info.IsSeed);

            return servers;
        }
        catch
        {
            return servers;
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

    private static byte[] GetMasterQueryRequest(string address, Game game)
    {
        var bytes = new List<byte>
        {
            0x31, // default
            0xFF // region 255 (world - any)
        };

        bytes.AddRange(StringUtils.WriteNullTerminatedString(address)); // initial IP = 0.0.0.0:0, if we find '0.0.0.0:0' in the returned buffer = end of list, otherwise use last ip:port to query for more servers!
        bytes.AddRange(GetMasterQueryFilters(game)); // can be extended to support more filters.

        return [.. bytes];
    }

    private static byte[] GetMasterQueryFilters(Game game)
    {
        if (game is null)
            return [0x00];

        var bldr = new StringBuilder();

        bldr.Append($"\\appid\\{game.AppId}");

        if (!string.IsNullOrEmpty(game.GameDir))
            bldr.Append($"\\gamedir\\{game.GameDir}");

        if (!string.IsNullOrEmpty(game.Filters))
            bldr.Append(game.Filters);

        return StringUtils.WriteNullTerminatedString(bldr.ToString());
    }
}