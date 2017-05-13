@setlocal 
@set local=%~dp0
@ECHO off

@set CtxErrCode=0

@REM Get path to MSBuild Binaries
if exist "%windir%\Microsoft.NET\Framework64\v4.0.30319" SET MSBUILDEXEDIR=%windir%\Microsoft.NET\Framework64\v4.0.30319
if exist "%windir%\Microsoft.NET\Framework\v4.0.30319" SET MSBUILDEXEDIR=%windir%\Microsoft.NET\Framework\v4.0.30319
if exist "%ProgramFiles(x86)%\MSBuild\14.0\bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\MSBuild\14.0\bin
if exist "%ProgramFiles%\MSBuild\14.0\bin" SET MSBUILDEXEDIR=%ProgramFiles%\MSBuild\14.0\bin
if exist "%ProgramFiles(x86)%\MSBuild\15.0\bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\MSBuild\15.0\bin
if exist "%ProgramFiles%\MSBuild\15.0\bin" SET MSBUILDEXEDIR=%ProgramFiles%\MSBuild\15.0\bin
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin
if exist "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin" SET MSBUILDEXEDIR=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin

@REM Can't multi-block if statement when check condition contains '(' and ')' char, so do as single line checks
if NOT "%MSBUILDEXEDIR%" == "" SET MSBUILDEXE=%MSBUILDEXEDIR%\MSBuild.exe
if "%MSBUILDEXEDIR%" == "" GOTO :MsBuildNotFound
if NOT "%MSBUILDEXEDIR%" == "" GOTO :MsBuildFound


:MsBuildFound
@ECHO MsBuild Location = %MSBUILDEXE%
@goto build


:build
@ECHO Installing packages...
"%local%.nuget\NuGet.exe" restore "%local%LetsEncryptCentral.sln"
@ECHO Building...
"%MSBUILDEXE%" "%local%LetsEncryptCentral.sln" /t:Rebuild /P:Configuration=Release
IF ERRORLEVEL 1 GOTO :end
@goto copy



:copy
robocopy "%local%src\LetsEncryptCentral\bin\Release" "%local%release" /e
@goto pack




:pack
@rem %local%\.nuget\NuGet pack %local%\src\MyProj\MyProj.nuspec  -OutputDirectory %local%\release
@goto end



:MsBuildNotFound
@echo Could not found msbuild
@SET CtxErrCode=2
@goto end


:end
@pushd %local%
IF NOT "%CtxErrCode%" == "0" EXIT /B %CtxErrCode%
EXIT /B %ERRORLEVEL%