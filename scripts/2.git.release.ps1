cd ..
$types = @{
    '1' = @('major', '主版本')
    '2' = @('minor', '此版本')
    '3' = @('patch', '修订版本')
}
$prefix = ''
while ($null -eq $types[$prefix])
{
    $prefix = Read-Host "请选择版本类型`n" $( & { param($i) $i | ForEach-Object { "$_ : $( $types[$_][0] )（$( $types[$_][1] )）`n" } } $types.Keys | Sort-Object )
}
git checkout main
git branch -D release
git checkout -b release
./node_modules/.bin/standard-version -r $types[$prefix][0]
cd ./scripts
./code.clean.ps1
git commit --amend --no-edit -a
git push --follow-tags --force origin release
Start-Process -FilePath "https://github.com/nsnail/dot/compare/main...release"
Write-Host "按『Enter』回到主分支，『Ctrl+C』退出"
Pause
git checkout main
git pull
git branch -D release