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
    - [x] Basic primitive  
    - [ ] Advanced model
- Miscellaneous
  - [x] Discord Rich Presence  
  - [x] Scene loading  
  - [ ] Items  

## Installation
1. Have [Discord](https://discord.com/download) installed and running. Discord Rich Presence is the only way to join or invite other players.  
2. Download the [most recent x64 release of BepInEx](https://github.com/BepInEx/BepInEx/releases/latest) and extract it to your H3VR directory.
3. Download the [most recent release of H3MP](https://github.com/ash-hat/H3MP/releases/latest) and extract it to your H3VR directory.  
4. Start the game  
    - Join a party by clicking the "Join" button on a Discord invite. In the future, you can join off of invites and the game will start automatically.
    - Invite players to your party by clicking the plus button in a Discord text channel, and select "Invite ... to Play H3MP".

If you are on BepInEx 5.3 or prior, not all of the DLL files can be put in the plugins directory. This is because of a plugin loading bug that [has been fixed](https://github.com/BepInEx/BepInEx/commit/4d7e5cac2bff602c5af6a5af5adfc0e8fbe41fd9) for BepInEx 5.4 (not released yet) and further.

## Uninstallation
To uninstall, you only need to delete the `BepInEx\plugins\H3MP` directory. `discord_game_sdk.dll` will not be loaded if H3MP is not present, just like how BepInEx is not loaded if `winhttp.dll` is not present.

## Building
:warning: All scripts listed here must be ran the root git directory (has the scripts directory in it). **Do not run a script by double clicking on it, or with the current directory set to `scripts`**. :warning:

### Get the Dependencies
#### 1. Download the [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide#step-1-get-the-thing) and extract `lib/x86_64/discord_game_sdk.dll` to `src/H3MP/Discord/`
This can be done using `scripts/get_discord_gamesdk.sh`.  
This script is compatible with any environment, given that POSIX compliant `curl`, `unzip`, and `rm` commands are present. This includes and has been tested against Git Bash for Windows.

#### 2. Create a symlink named `game` that points to the H3VR directory. 
- Windows: `scripts/link_game.bat` does this for you. If it cannot find the game itself, it will ask for input.  
- POSIX platforms: `ln -s <H3VR directory> game`

#### 3. Restore NuGet packages
Most IDEs automatically do this, but if you do not have such an IDE, run `nuget restore`.

### Build the Source Code
#### 1. Build the `H3MP` project
Or the entire solution, they do the same thing. Your choice.

#### 2. (Windows only) Consider symlinking the `Debug` builds
If you are planning on debugging your build, it is tedious to keep selecting, copying, and pasting the same 3 files over and over again. For this reason, you can run `scripts/link_builds.bat`. It will symlink your `Debug` builds back over to the game so any changes in your builds is reflected in the game.

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
