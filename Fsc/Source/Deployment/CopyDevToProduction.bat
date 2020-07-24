set publishFolder=.\Fsc\Source\ServiceCatalogWepApi\bin\Release\netcoreapp3.1\publish
set configPath=.\Fsc\Source\Deployment\Settings
set envName=Prod

rem take services offline
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat1\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat2\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat3\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat4\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat5\WebApps\Fsc\Api\app_offline.htm" /y
copy ".\Fsc\Source\Deployment\app_offline.htm" "\\Usidc2wvpcat6\WebApps\Fsc\Api\app_offline.htm" /y

waitfor /t 10 SomethingThatNeverHappens

xcopy "%publishFolder%\*" "\\Usidc2wvpcat1\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat1\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat1\WebApps\Fsc\Api\app_offline.htm 

rem copy code to Usidc2wvpcat2
xcopy "%publishFolder%\*" "\\Usidc2wvpcat2\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat2\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat2\WebApps\Fsc\Api\app_offline.htm 

rem copy code to Usidc2wvpcat3
xcopy "%publishFolder%\*" "\\Usidc2wvpcat3\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat3\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat3\WebApps\Fsc\Api\app_offline.htm 

rem copy code to Usidc2wvpcat4
xcopy "%publishFolder%\*" "\\Usidc2wvpcat4\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat4\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat4\WebApps\Fsc\Api\app_offline.htm 	

rem copy code to Usidc2wvpcat5
xcopy "%publishFolder%\*" "\\Usidc2wvpcat5\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat5\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat5\WebApps\Fsc\Api\app_offline.htm 

rem copy code to Usidc2wvpcat6
xcopy "%publishFolder%\*" "\\Usidc2wvpcat6\WebApps\Fsc\Api\" /s /y /i
copy %configPath%\%envName%settings.json \\Usidc2wvpcat6\WebApps\FSC\Api\appsettings.json /y
del /q \\Usidc2wvpcat6\WebApps\Fsc\Api\app_offline.htm 
