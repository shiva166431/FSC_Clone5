echo got %1 from take offline
set del=%1
if %del% == Yes goto copyDev
if %del% == No goto deleteDev
goto end
:copyProd
echo Copying offline  

copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat1\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat2\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat3\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat4\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat5\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat6\WebApps\Fsc\Api\app_offline.htm" /y
if NOT %ERRORLEVEL% == 0  goto error
goto endDev

:deleteProd
echo deleting offline
	
echo removing offline from Usidc2wvpcat1 
	del /q \\Usidc2wvpcat1\WebApps\Fsc\Api\app_offline.htm
echo removing offline from Usidc2wvpcat2 
	del /q \\Usidc2wvpcat2\WebApps\Fsc\Api\app_offline.htm
echo removing offline from Usidc2wvpcat3 
	del /q \\Usidc2wvpcat3\WebApps\Fsc\Api\app_offline.htm
echo removing offline from Usidc2wvpcat4 
	del /q \\Usidc2wvpcat4\WebApps\Fsc\Api\app_offline.htm
echo removing offline from Usidc2wvpcat5 
	del /q \\Usidc2wvpcat5\WebApps\Fsc\Api\app_offline.htm
echo removing offline from Usidc2wvpcat6 
	del /q \\Usidc2wvpcat6\WebApps\Fsc\Api\app_offline.htm

if NOT %ERRORLEVEL% == 0  goto error
echo Removed offline file,run the FSC_Deployment job to ensure that latest code is deployed.
goto endDev

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%

:endProd

exit