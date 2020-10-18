# H3MP
A **WIP** multiplayer mod for Hot Dogs, Horseshoes, and Hand Grenades.

## Features

- [x] Discord Rich Presence

- [x] Scene loading

- [ ] Items

- [ ] Player

  - [X] Movement
  
  - [ ] Held items

  - [ ] Avatar

    - [X] Basic primitive
    
    - [ ] Advanced model
    
# Installation
There are no prebuilt binaries because this mod is not ready. **You must build it yourself.**

1. Have [Discord](https://discord.com/download) installed & running. Discord RPC is currently used for joining & inviting other players.

2. Download the [most recent Discord SDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide#step-1-get-the-thing) and extract `lib\x86_64\discord_game_sdk.dll` into your H3VR root folder.

3. Download the [most recent x64 release of BepInEx](https://github.com/BepInEx/BepInEx/releases/latest) and extract that to your H3VR root folder.

4. Then put the following three files into the BepInEx plugin folder.
   - H3MP.dll
   - H3MP.Networking.dll
   - LiteNetLib.dll
   
5. Ensure you have 7777 UDP open if you are hosting.

6. To host, send an invite via Discord after loading the main menu. 
   - Players can join on a running instance of H3VR
   - Players can also join without H3VR running, the game will launch and connect.

7. Enjoy **barely functional** multiplayer.

# Other Information
- Target Game: *Hot Dogs, Horseshoes, and Hand Grenades* ([Website](http://h3vr.com/); [Steam](https://store.steampowered.com/app/450540/Hot_Dogs_Horseshoes__Hand_Grenades/))  
- Networking transport: [LiteNetLib](https://github.com/RevenantX/LiteNetLib)  
- Networking protocol: Custom. Inspired by:  
  - [Mirror](https://github.com/vis2k/Mirror)
  - [MLAPI](https://github.com/MidLevel/MLAPI)
