echo got %1 from take offline
set del=%1
if %del% == Yes goto copyDev
if %del% == No goto deleteDev
goto end
:copyDev
echo Copying offline 
REM List of servers are expected to be in DevServers.txt file.
for /F "tokens=1,2" %%a in (.\Fsc\Source\Deployment\DevServers.txt) do (
	copy .\Fsc\Source\Deployment\app_offline.htm \\%%a\WebApps\Fsc\Api\app_offline.htm /y
)
if NOT %ERRORLEVEL% == 0  goto error
goto endDev

:deleteDev
echo deleting offline

for /F "tokens=1,2" %%a in (.\Fsc\Source\Deployment\DevServers.txt) do (	
echo removing offline from %%a 
	del /q \\%%a\WebApps\Fsc\Api\app_offline.htm
)
if NOT %ERRORLEVEL% == 0  goto error
echo Removed offline file,run the FSC_DEV job to ensure that latest code is deployed.
goto endDev

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%

:endDev

exit