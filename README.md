# H3MP ![functions: barely](https://img.shields.io/badge/functions-barely-c28411?style=for-the-badge) ![made with: pain](https://img.shields.io/badge/made%20with-pain-red?style=for-the-badge)
A **WIP** multiplayer mod for Hot Dogs, Horseshoes, and Hand Grenades.

## Features
- [x] Discord Rich Presence  
- [x] Scene loading  
- [ ] Items  
- Configs  
  - [x] Player limit  
  - [x] Scene loading/reloading permissions
  - [x] Tick rate  
  - *refer to `BepInEx\config\Ash.H3MP.cfg` for additional config options*
- Player  
  - [x] Movement  
  - [ ] Held items  
  - [ ] Avatar  
    - [x] Basic primitive  
    - [ ] Advanced model

## Installation
1. Have [Discord](https://discord.com/download) installed and running. Discord Rich Presence is the only way to join or invite other players.  
2. Download the [most recent x64 release of BepInEx](https://github.com/BepInEx/BepInEx/releases/latest) and extract it to your H3VR root directory.
3. Download the [most recent release of H3MP](https://github.com/ash-hat/H3MP/releases/latest) and extract it to your H3VR root directory.  
4. Start the game  
    - Join a party by clicking the "Join" button on a Discord invite. In the future, you can join off of invites and the game will start automatically.
    - Invite players to your party by clicking the plus button in a Discord text channel, and select "Invite ... to Play H3MP".

If you are on BepInEx 5.3 or prior, not all of the DLL files can be put in the plugins directory. This is because of a plugin loading bug that [has been fixed](https://github.com/BepInEx/BepInEx/commit/4d7e5cac2bff602c5af6a5af5adfc0e8fbe41fd9) for BepInEx 5.4 (not released yet) and further.

## Uninstallation
To uninstall, you only need to delete the `BepInEx\plugins\H3MP` directory. `discord_game_sdk.dll` will not be loaded if H3MP is not present, just like how BepInEx is not loaded if `winhttp.dll` is not present.

## Building
1. Copy or symlink the required dependencies from `h3vr_Data\Managed\` to `src\libs\`  
    - `AssemblyCSharp.dll`  
    - `UnityEngine.dll`  
2. Copy or symlink the required dependencies from `BepInEx\core\` to `src\libs\`  
    - `0Harmony.dll`  
    - `BepInEx.dll`  
3. Build the solution  
    a. This project makes use of NuGet packages. If your IDE does not automatically restore NuGet packages, make sure to restore them manually.

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
