set isDown=%Down%
set Env=%Environment%
if %isDown% == Choose goto empty 
if %isDown% == Yes goto envCheck
if %isDown% == No goto envCheck

:envCheck
echo "Environment Check : %Env%"
if %Env% == ENV1 goto env1
if %Env% == ENV2 goto env2
if %Env% == ENV3 goto env3
if %Env% == ENV4 goto env4
if %Env% == DEV goto dev
if %Env% == PROD goto production
goto end

:dev
set envName=AllDEV
.\Fsc\Source\Deployment\DevOffline.bat %isDown%
goto end

:production
.\Fsc\Source\Deployment\ProdOffline.bat %isDown%
goto end

:env1
set envName=Env1
set servName=Usidc2wvpcatev1
goto do

:env2
set envName=Env2
set servName=Usidc2wvpcatev2
goto do

:env3
set envName=Env3
set servName=Usidc2wvpcatev3
goto do

:env4
set envName=Env4
set servName=Usidc2wvpcatev4
goto do

:do 
if %isDown% == Yes goto copy
if %isDown% == No goto delete

:copy
echo Copying offline.htm taking down %Env%
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\%servName%\WebApps\Fsc\Api\app_offline.htm" /y
echo ErrorLevel %ERRORLEVEL%
if NOT %ERRORLEVEL% == 0 goto error
goto end

:delete
echo Deleting offline.htm from %ENV%
del /q \\%servName%\WebApps\Fsc\Api\app_offline.htm
if NOT %ERRORLEVEL% == 0 goto error
echo Removed offline file,run the FSC_Deployment job to ensure that latest code is deployed.
goto end

:error
echo Failed with error #%errorlevel%.
goto end

:empty
echo Down parameter is not set.
goto end

:end