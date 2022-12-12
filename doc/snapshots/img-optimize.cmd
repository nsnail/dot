for %%i in (*.png) do pngquant %%i --force --output %%i --skip-if-larger
for %%i in (*.jpg) do jpegtran -copy none -optimize -perfect %%i %%i