set GST_PLUGIN_PATH=%CD%\gst-plugins
set GST_LIBS_PATH=%CD%\libs
FOR /F "tokens=3" %%A IN ('reg query "HKEY_LOCAL_MACHINE\Software\Novell\Mono" /v DefaultCLR') DO SET VERSION=%%A
FOR /F "tokens=2* " %%A IN ('reg query "HKEY_LOCAL_MACHINE\Software\Novell\Mono\%VERSION%" /v SdkInstallRoot ^| FIND "SdkInstallRoot" ') DO SET MONOPREFIX=%%B
set PATH=%MONOPREFIX%\bin;%GST_LIBS_PATH%;%PATH%
echo %PATH%

start mono LGM.exe
