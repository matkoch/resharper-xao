@echo off
set package_id="ReSharper.Xao"

set config=%1
if "%config%" == "" (
   set config=Release
)
 
set version=0.1.0
if not "%PackageVersion%" == "" (
   set version=%PackageVersion%
)

set nuget=
if "%nuget%" == "" (
        set nuget=src\.nuget\nuget.exe
)

%nuget% restore src\ReSharper.Xao.sln
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild src\ReSharper.Xao.sln /t:Rebuild /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Minimal /nr:false

%nuget% pack "src\plugin.nuspec" -NoPackageAnalysis -Version %version% -Properties "Configuration=%config%;ReSharperDep=ReSharper;ReSharperVer=[8.1,8.3);PackageId=%package_id%"
%nuget% pack "src\plugin.R90.nuspec" -NoPackageAnalysis -Version %version% -Properties "Configuration=%config%;ReSharperDep=Wave;ReSharperVer=[2.0];PackageId=%package_id%.R90"