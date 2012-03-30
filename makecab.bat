@echo off

set currentpath=%~dp0

set dt2=%date:~-10%
set dt=%dt2:/=%

rem for /f %%i in ('subwcrev ') do
rem 	set ver=%%i
set ver=0.24

set status=alpha

set outfilename=NiCoMiudon_v%ver%_%status%_%dt%.CAB
echo %outfilename%

if "%ProgramFiles(x86)%" == "" (
	set progfiles="%ProgramFiles%"
) else (
	set progfiles="%ProgramFiles(x86)%"
)

cd "%userprofile%\Program Files\Local\QuickCab"
Cabwiz.exe "%currentpath%CabWiz.inf" /postxml "%currentpath%CabWiz.xml"

cd %currentpath%
ren CabWiz.CAB %outfilename%

pause
