
if not exist %1 goto File1NotFound
if not exist %2 goto File2NotFound

fc %1 %2 
if %ERRORLEVEL%==0 GOTO NoCopy

echo Files are not the same.  Copying %1 over %2
copy %1 %2 /y & goto END

:NoCopy
echo Files are the same. No Copying done.
goto END

:File1NotFound
echo %1 not found.
goto END

:File2NotFound
echo %2 not found! check the file location!
goto END

:END
echo Done.