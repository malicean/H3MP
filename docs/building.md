# Building
:warning: All scripts listed here must be ran the root git directory (has the scripts directory in it). **Do not run a script by double clicking on it, or with the current directory set to `scripts`**. :warning:

## Get the Dependencies
### 1. Download the [Discord GameSDK](https://discord.com/developers/docs/game-sdk/sdk-starter-guide#step-1-get-the-thing) and extract `lib/x86_64/discord_game_sdk.dll` to `src/H3MP/Discord/`
This can be done using `scripts/get_discord_gamesdk.sh`.  
This script  has been successfully tested against Git Bash for Windows.

### 2. Create a symlink named `game` that points to the H3VR directory. 
- Windows: `scripts/link_game.bat` does this for you. If it cannot find the game itself, it will ask for input.  
- POSIX platforms: `ln -s <H3VR directory> game`

### 3. Restore NuGet packages
Most IDEs automatically do this, but if you do not have such an IDE, run `nuget restore`.

## Build the Source Code
### 1. Build the `H3MP` project
Or the entire solution, they do the same thing. Your choice.

### 2. (Windows only) Consider symlinking the `Debug` builds
If you are planning on debugging your build, it is tedious to keep selecting, copying, and pasting the same 3 files over and over again. For this reason, you can run `scripts/link_builds.bat`. It will symlink your `Debug` builds back over to the game so any changes in your builds is reflected in the game.
