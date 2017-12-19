echo off

set SOLUTIONS=^
	externals/Xamarin.Auth/source/Xamarin.Auth-Library.sln
	
cinst NuGet.Commandline	

for %%s IN (%SOLUTIONS%) DO (
	echo nuget restore %%s
    nuget.exe restore %%s
)

@IF %ERRORLEVEL% NEQ 0 PAUSE


