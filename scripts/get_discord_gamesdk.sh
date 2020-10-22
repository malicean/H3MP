VERSION="2.5.6"
FILE="discord_game_sdk.zip"
URL="https://dl-game-sdk.discordapp.net/$VERSION/$FILE"

curl -o "$FILE" "$URL"
unzip -j "$FILE" "lib/x86_64/discord_game_sdk.dll" -d "src/H3MP/Discord/"
rm "$FILE"