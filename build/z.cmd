@echo off

del ..\src\win\bin\release\*.* /Q
del ..\src\win\obj\x86\release\*.* /Q

%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /p:Configuration=Release ..\src\win\mutefm.sln

del ..\src\win\bin\release\*.pdb
del ..\src\win\bin\release\mute_fm_web
rmdir ..\src\win\bin\release\mute_fm_web>nul

pause

"C:\Program Files (x86)\Inno Setup 5\ISCC.exe" mutefm_setup.iss
copy output\mutefm_setup.exe output\mutefm_setup_0_9_14.exe