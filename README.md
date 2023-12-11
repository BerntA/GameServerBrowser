## Universal Steam Server Browser WebApp
Primarily used for Source & GoldSrc games/mods!

### How to run
Edit SteamAPIKey in 'GameServerList.App/appsettings.json', then launch app.sln, compile and launch from there.

### Adding new game entries
Edit the 'GameServerList.App/Data/games.json' file.

### Features
- Queries Steam WEB API to retrieve server info for games & mods
- Utilizes A2S queries to retrieve player info, and server info for legacy mods (e.g appId 215 related games)
- Supports querying master server list, player info and server info via A2S

### CI
[![Deploy on Linux](https://github.com/BerntA/GameServerBrowser/actions/workflows/deploy-linux.yml/badge.svg)](https://github.com/BerntA/GameServerBrowser/actions/workflows/deploy-linux.yml)

### Preview
![Game Server Browser WebApp](./preview.png)
