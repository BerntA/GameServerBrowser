import json

servers = []

with open('../GameServerList.App/Data/730_addresses.json', 'r') as f:
    servers = json.load(f)

with open('./servers.json', 'r') as f:
    servers.extend(json.load(f))

servers = list(set(servers)) # ensure unique
servers.sort()

print(len(servers))

with open('./servers_merged.json', 'w') as f:
    json.dump(servers, f, indent=4)

print("Merged server files!")