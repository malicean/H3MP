# H3MP
A **WIP** multiplayer mod for Hot Dogs, Horseshoes, and Hand Grenades.

# Installation
There are no prebuilt binaries because this mod is not ready. **You must build it yourself.**

## Client
1. Download the [most recent x64 release of BepInEx](https://github.com/BepInEx/BepInEx/releases/latest) and extract that to your H3VR root folder.

2. Then put the following three files into the BepInEx plugin folder.
   - H3MP.Common.dll
   - H3MP.Client.dll
   - LiteNetLib.dll

3. Run the game and enjoy **nonfunctional multiplayer**.

## Server
This is super easy because it's .NET Core and a standalone application:
```bash
dotnet publish --configuration Release src/H3MP.Server
```

# Other Information
- Target Game: *Hot Dogs, Horseshoes, and Hand Grenades* ([Website](http://h3vr.com/); [Steam](https://store.steampowered.com/app/450540/Hot_Dogs_Horseshoes__Hand_Grenades/))  
- Networking transport: [LiteNetLib](https://github.com/RevenantX/LiteNetLib)  
- Networking protocol: Custom. Inspired by:  
  - [Mirror](https://github.com/vis2k/Mirror)
  - [MLAPI](https://github.com/MidLevel/MLAPI)
