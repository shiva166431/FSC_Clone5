set publishFolder=.\Fsc\Source\ServiceCatalogWepApi\bin\Release\netcoreapp3.1\publish
set configPath=.\Fsc\Source\Deployment\Settings

echo "Environment" %Environment%
if %Environment% == Env1 goto env1
if %Environment% == Env2 goto env2
if %Environment% == Env3 goto env3
if %Environment% == Env4 goto env4
if %Environment% == Int1 goto int1
if %Environment% == Production goto production
goto end


:env1
set envName=Env1
set servName=Usidc2wvpcatev1
set settingsTarget=\\usidc2wvpcatev1\WebApps\FSC\Api
goto copy

:env2
set envName=Env2
set servName=Usidc2wvpcatev2
set settingsTarget=\\Usidc2wvpcatev2\WebApps\FSC\Api
goto copy

:env3
set envName=Env3
set servName=Usidc2wvpcatev3
set settingsTarget=\\Usidc2wvpcatev3\WebApps\FSC\Api
goto copy

:env4
set envName=Env4
set servName=Usidc2wvpcatev4
set settingsTarget=\\Usidc2wvpcatev4\WebApps\FSC\Api
goto copy

:int1
set envName=Int1
set servName=Usidc2wvpcatev05
set settingsTarget=\\Usidc2wvpcatev05\WebApps\FSC\Api
goto copy

:production
.\Fsc\Source\Deployment\CopyDevToProduction.bat
goto end



:copy
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\%servName%\WebApps\Fsc\Api\app_offline.htm" /y
waitfor /t 10 SomethingThatNeverHappens
xcopy "%publishFolder%\*" "\\%servName%\WebApps\Fsc\Api\" /s /y /i
	del /q \\%servName%\WebApps\Fsc\Api\app_offline.htm 
	
copy %configPath%\%envName%settings.json %settingsTarget%\appsettings.json /y

goto end

:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%

:end