dotnet build
dotnet publish ./src/dot.csproj -f net7.0-windows -c Release -r win-x64 --sc -p:"PublishSingleFile=true" -o ./build/win-x64
dotnet publish ./src/dot.csproj -f net7.0 -c Release -r linux-x64 --sc -p:"PublishSingleFile=true" -o ./build/linux-x64
Remove-Item -r ./build/temp