rem Creates a SQL export of the PCAT DB for the specified environment


set SID=
set user=fsc_dba
set pwd=%FSC_PW%

if %1.==. GOTO supplyEnv

set Environment=%1
echo "Environment" %Environment%
if %Environment% == Dev1 goto dev1
if %Environment% == Env4 goto env4
if %Environment% == Env1 goto env1
if %Environment% == Env3 goto env3
if %Environment% == Env2 goto env2
goto supplyEnv

:env4
set envName=Env4
set SID=SWFT04E

goto copy


:env1
set envName=Env1
set SID=swft01e

goto copy


:env3
set envName=Env3
set SID=SWFT03E

goto copy


:env2
set envName=Env2
set SID=swft02e

goto copy

:dev1
set envName=Dev1
set SID=SWIFTDEV

goto copy


:copy
exp %user%/%pwd%@%SID%.world file=e:\FSC\DB_Dumps\%envName%_dump.dmp owner=%user% statistics=none feedback=1000
echo ErrorLevel %ERRORLEVEL%
if %ERRORLEVEL% == 1  goto error


goto end

:error
echo %envName% Failed with error #%errorlevel%.
exit /b %errorlevel%

:supplyEnv
echo Provide the environment (Env1, Env2 etc.)
:end
EXIT 0
