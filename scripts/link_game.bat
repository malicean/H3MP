:: Creates a symlink linking the game's directory to a local directory for easy and atomic dependency setup.

@echo off
setlocal EnableDelayedExpansion

set "H3MP_LINK_PATH=game"

if exist "%H3MP_LINK_PATH%" (
    echo A path already exists at %H3MP_LINK_PATH%. If this does not lead to the desired target, delete it and rerun this script.
    exit /b 1
)

set "H3MP_DEFAULT_TARGET_PATH=C:\Program Files (x86)\Steam\steamapps\common\H3VR"

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

if exist "%H3MP_H3VR_PATH%\h3vr.exe" if exist "%H3MP_H3VR_PATH%\h3vr_Data\Managed" (
    goto is_h3vr_true
)
    echo The target path does not match H3VR's directory structure.
    echo.
    goto user_path_menu
:is_h3vr_true

if not exist "%H3MP_H3VR_PATH%\BepInEx\core" (
    echo BepInEx, needed to build H3MP, doesn't seem to be installed. 
    echo If you would like to build H3MP without running BepInEx, you can delete the its hook, "winhttp.dll", after installing it.
    exit /b 2
)

:: Does not require elevated permissions
mklink /j "%H3MP_LINK_PATH%" "%H3MP_H3VR_PATH%"