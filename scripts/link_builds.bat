:: Creates a symlink linking game plugin binaries the built plugin binaries for an easy debugging setup.

@echo off
setlocal EnableDelayedExpansion

set "H3MP_DEFAULT_H3VR_PATH=C:\Program Files (x86)\Steam\steamapps\common\H3VR"
set "H3MP_BINS=discord_game_sdk.dll H3MP.dll LiteNetLib.dll"

if exist "%H3MP_DEFAULT_H3VR_PATH%" (
    echo Found the game at the default path: "%H3MP_DEFAULT_H3VR_PATH%"
    set "H3MP_H3VR_PATH=%H3MP_DEFAULT_H3VR_PATH%"
) else (
    echo Could not find H3VR in the default Steam library path.
    echo.
    echo You must specify the path of one of your choosing:

    :user_path_menu
    echo 1 ^) Steam library that houses H3VR
    echo 2 ^) Path to H3VR's root directory
    echo.
    set /p "H3MP_MODE=Option (default 1): " || set "H3MP_MODE=1"

    if not "!H3MP_MODE!" == "1" if not "!H3MP_MODE!" == "2" (
        echo "!H3MP_MODE!"
        echo Invalid option
        goto user_path_menu
    )

    :user_path_menu_success
    set /p H3MP_PATH=Path ^(no quotes or trailing backslash^): 

    if "!H3MP_MODE!" == "1" (
        set "H3MP_H3VR_PATH=!H3MP_PATH!\steamapps\common\H3VR"
    ) else if "!H3MP_MODE!" == "2" (
        set "H3MP_H3VR_PATH=!H3MP_PATH!"
    )
    
    if not exist "!H3MP_H3VR_PATH!" (
        echo Path not found: "!H3MP_H3VR_PATH!"
        goto user_path_menu
    )

    echo Set game path to: "!H3MP_H3VR_PATH!"
)

echo.

if exist "%H3MP_H3VR_PATH%\h3vr.exe" if exist "%H3MP_H3VR_PATH%\h3vr_Data" (
    goto is_h3vr_true
)
    echo The target path does not match H3VR's directory structure.
    echo.
    goto user_path_menu
:is_h3vr_true

if not exist "%H3MP_H3VR_PATH%\BepInEx\plugins" (
    echo BepInEx, needed to run H3MP, doesn't seem to be installed.
    exit /b 2
)

set "H3MP_PLUGIN=%H3MP_H3VR_PATH%\BepInEx\plugins\H3MP"
if not exist "%H3MP_PLUGIN%" (
    mkdir "%H3MP_PLUGIN%"
)

for %%f in (%H3MP_BINS%) do (
    set "H3MP_LINK=%H3MP_PLUGIN%\%%f"

    if exist "!H3MP_LINK!" (
        echo A binary already exists. This script will not overwrite it. Please move, rename, or delete it.
        echo Path: "!H3MP_LINK!"

        exit /b 3
    )
)

:mklink_calls
for %%f in (%H3MP_BINS%) do (
    set "H3MP_LINK=%H3MP_PLUGIN%\%%f"
    mklink "!H3MP_LINK!" "src\H3MP\bin\Debug\%%f"

    if not exist "!H3MP_LINK!" if not %errorLevel% == 0 (
        net session >nul 2>&1
        if not %errorLevel% == 0 (
            echo.
            echo Detected a failed mklink call in a user session.
            echo If you are on Windows 10 Creators ^(1703^) or later, you can enable developer mode.
            echo If you are not, run this file as administrator.
            echo.
            echo You can retry the mklink calls ^(after you enable developer mode^) by pressing any key. Ctrl-C to quit.

            pause >nul
            echo.
            goto mklink_calls
        )
    )
)
