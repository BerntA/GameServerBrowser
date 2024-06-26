﻿using GameServerList.Common.External;
using GameServerList.Common.Model;
using GameServerList.Common.Model.A2S;
using GameServerList.Common.Utils;

var appId = 0UL;
var timeout = 1800000;
var maxServersPerIp = 5;

Console.Write("Please specify the appId: ");

while (!ulong.TryParse(Console.ReadLine(), out appId) || appId == 0UL)
    Console.Write("Invalid appId specified, try again: ");

Console.WriteLine($"Scanning AppId {appId} for servers...");

var game = new Game
{
    AppId = appId,
};

var servers = await A2SQuery.QueryServerList(MasterServer.Source, game, timeout);

Console.WriteLine($"Found {servers.Count} servers, filtering out dupes...");

// Only allow up to X servers per unique IP
servers = servers
    .GroupBy(g => g.IP)
    .SelectMany(g => g.Take(maxServersPerIp).ToList())
    .ToList();

Console.WriteLine($"Reduced servers found to {servers.Count}, fetching server info now...");

var serversWithInfo = await Task.WhenAll(servers.Select(s => A2SQuery.QueryServerInfo(s.Address, 5000)));

var serversWithInfoFiltered = serversWithInfo
    .Where(s => s.HasValue && s.Value.MaxPlayers <= 128)
    .Select(s => s.Value)
    .ToList();

var filteredIPs = serversWithInfoFiltered
    .Select(s => s.Address)
    .ToList();

Console.WriteLine($"Writing {filteredIPs.Count} servers to disk!");

Directory.CreateDirectory("./out");

FileUtils.WriteDataToFile($"./out/{appId}_servers.json", serversWithInfoFiltered);
FileUtils.WriteDataToFile($"./out/{appId}_addresses.json", filteredIPs);

Console.WriteLine("Scanning successful, check 'out' folder for any generated data.");
Console.ReadKey();