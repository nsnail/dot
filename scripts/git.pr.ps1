$branch = $( git branch --show-current )
git add ../
./code.clean.ps1
git add ../
../node_modules/.bin/git-cz.ps1
git pull
git push --set-upstream origin $branch
Start-Process -FilePath "https://github.com/nsnail/dot/compare/main...$branch"
启用写入模式-Host "按『Enter』重建分支，『Ctrl+C』退出"
Pause
./git.rc.ps1