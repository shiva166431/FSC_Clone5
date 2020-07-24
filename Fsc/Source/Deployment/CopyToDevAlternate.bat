set publishFolder=.\Fsc\Source\ServiceCatalogWepApi\bin\Release\netcoreapp3.1\publish

REM List of servers are expected to be in DevServers.txt file.
for /F "tokens=1,2" %%a in (.\Fsc\Source\Deployment\DevServers.txt) do (
	copy .\Fsc\Source\Deployment\app_offline.htm \\%%a\WebApps\Fsc\Api2\app_offline.htm /y
)

rem The default shutdownTimeLimit is 10 seconds.  Wait for the dlls to be unloaded and unlocked so we can update them
waitfor /t 10 SomethingThatNeverHappens

for /F "tokens=1,2" %%a in (.\Fsc\Source\Deployment\DevServers.txt) do (
	xcopy %publishFolder%\* \\%%a\WebApps\Fsc\Api2\ /s /y /i
	del /q \\%%a\WebApps\Fsc\Api2\app_offline.htm
)
