dotnet build
dotnet publish -c Release -r win-x64 --sc -p:"PublishSingleFile=true" -o ./build/win-x64
Remove-Item -r ./build/temp