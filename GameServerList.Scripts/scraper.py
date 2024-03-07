import requests as req
import json

def query_gameservers_cs2(page=1):
    try:
        return req.get(
            'https://cs2browser.com/api/gameservers/?limit=40&page=%s' % page
        ).json()
    except:
        return []

def scan_cs2_servers():
    print("Scanning for CS2 servers")
    servers = []
    page = 1
    while True:
        res = query_gameservers_cs2(page)
        if len(res) == 0:
            break
        servers.extend(res)
        print("Found %s servers, on page %s" % (len(res), page))
        page += 1
        
    return servers

servers = scan_cs2_servers()
servers = ['%s:%s' % (s['ip'], s['port']) for s in servers if s['banned'] is None] # filter out banned servers, select ip:port

with open('./community_servers.json', 'r') as f:
    servers.extend([s['ip'] for s in json.load(f)]) # add community servers from the launcher

servers = list(set(servers)) # only keep unique servers

with open('./servers.json', 'w') as f:
    json.dump(servers, f, indent=4)
    
print("Finished writing servers.json!")