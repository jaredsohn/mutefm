@echo off

del ..\src\win\bin\release\*.* /Q
del ..\src\win\obj\x86\release\*.* /Q
rmdir ..\src\win\bin\release\mute_fm_web

%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /p:Configuration=Release ..\src\win\mutefmplus.sln

del ..\src\win\bin\release\icudt42.dll
del ..\src\win\bin\release\*.pdb
del ..\src\win\bin\release\*vshost.*
del ..\src\win\bin\release\debug.log
del ..\src\win\bin\release\awesomium_process

pause

"C:\Program Files (x86)\Inno Setup 5\ISCC.exe" mutefmplus_setup.iss
copy output\mutefm_setup.exe output\mutefmplus_setup_0_9_15.exe