# H3MP ![functions: barely](https://img.shields.io/badge/functions-barely-c28411?style=for-the-badge) ![made with: pain](https://img.shields.io/badge/made%20with-pain-red?style=for-the-badge)
A **WIP** multiplayer mod for Hot Dogs, Horseshoes, and Hand Grenades.

## Features
- Configs  
  - [x] Player limit  
  - [x] Scene loading/reloading permissions
  - [x] Tick rate  
  - *refer to `BepInEx\config\Ash.H3MP.cfg` for additional config options*
- Player  
  - [x] Movement  
  - [ ] Held items  
  - [ ] Avatar  
    - [x] Basic sosig body
    - [ ] Advanced sosig body
- Miscellaneous
  - [x] Discord Rich Presence  
  - [x] Scene loading  
  - [ ] Items  

## Installation
1. Have [Discord](https://discord.com/download) installed and running. Discord Rich Presence is the only way to join or invite other players.
2. Download the [most recent x64 release of BepInEx](https://github.com/BepInEx/BepInEx/releases/latest) and extract it to your **H3VR** directory.
3. Download the [most recent release of H3MP](https://github.com/ash-hat/H3MP/releases/latest) and extract it to your **H3VR** directory.
4. If you are hosting, you must have port **7777 UDP** open and forwarded to your computer. The desired port number can also be changed in the config.
5. Start the game  
    - Join a party by clicking the "Join" button on a Discord invite. In the future, you can join off of invites and the game will start automatically.
    - Invite players to your party by clicking the plus button in a Discord text channel, and select "Invite ... to Play H3MP".



## Uninstallation
To uninstall, you only need to delete the `BepInEx\plugins\H3MP` directory.

## Documentation
Installation and uninstallation have been listed here for convenience, but further documentation is included in the [docs](docs/) directory.

# Other Information
- Target Game: *Hot Dogs, Horseshoes, and Hand Grenades* ([Website](http://h3vr.com/); [Steam](https://store.steampowered.com/app/450540/Hot_Dogs_Horseshoes__Hand_Grenades/))  
- Networking  
  - Transport: [LiteNetLib](https://github.com/RevenantX/LiteNetLib)  
  - Protocol: custom, inspired by  
    - [Mirror](https://github.com/vis2k/Mirror)  
    - [MLAPI](https://github.com/MidLevel/MLAPI)  
  - Resources  
    - [Valve Developer Community Wiki: Source Multiplayer Networking](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking)  
    - Overwatch ([developer update](https://www.youtube.com/watch?v=vTH2ZPgYujQ) [abstract]; [GDC 2017 talk](https://youtu.be/W3aieHjyNvw?t=1341) [detailed])
