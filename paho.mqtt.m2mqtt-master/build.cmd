
MSBuild.exe M2Mqtt.sln /p:Configuration=Release

IF NOT EXIST ".\Build\Packages" MKDIR ".\Build\Packages"

.\Tools\NuGet\NuGet.exe pack M2Mqtt.nuspec -OutputDirectory ".\Build\Packages"
