@echo off

set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=2016.3.1
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
        set nuget=tools\nuget.exe
)

%nuget% restore ReSharper.Xao.sln
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild" ReSharper.Xao.sln /t:Rebuild /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

set package_id="ReSharper.Xao"
%nuget% pack "ReSharper.Xao.nuspec" -NoPackageAnalysis -Version %version% -Properties "Configuration=%config%;ReSharperDep=Wave;ReSharperVer=[7.0];PackageId=%package_id%"