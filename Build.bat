@echo off
echo Building K3po Library (Release) ...
MSBuild /t:ReBuild K3po.sln /p:Configuration=Release
if not errorlevel 1 goto buildwpa
echo An error occurred during Build K3po (Release). check buildLog.txt for details
goto error

:buildwpa
echo Building K3po Windows Phone Library (Release) ...
MSBuild /t:ReBuild WindowsPhone/K3po.WindowsPhone.sln /p:Configuration=Release
if not errorlevel 1 goto package
echo An error occurred during Build K3po Windows Phone (Release). check buildLog.txt for details
goto error

:buildxamarin
echo SKIP Building K3po Xamarin Library (Release) ...
echo DUE TO Xamarin License limitation
MSBuild /t:ReBuild Xamarin/K3po.Xamarin.sln /p:Configuration=Release
if not errorlevel 1 goto package
echo An error occurred during Build K3po Xamarin (Release). check buildLog.txt for details
goto error

:package
nuget pack K3po.nuspec
if not errorlevel 1 goto dist
echo An error occurred during nuget pack.
goto error
:dist
If not Exist dist (
mkdir dist
) ELSE (
del /s /f /q dist\*
)
move Kaazing.K3po.*.nupkg dist\

:done
echo Build Successfully
goto end
:error
echo Build Failed

:end
