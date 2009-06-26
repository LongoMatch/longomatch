export MONO_INSTALL_PATH=/e/Mono-2.4/
$MONO_INSTALL_PATH/bin/gacutil -i LongoMatch/bin/Release/CesarPlayer.dll
$MONO_INSTALL_PATH/bin/gacutil -i LongoMatch/bin/Release/Db4objects.Db4o.dll
export PKG_CONFIG_PATH=$MONO_INSTALL_PATH/lib/pkgconfig/
mkdir -p obj
windres LongoMatch/minilogo.rc obj/minilogo.o
$MONO_INSTALL_PATH/bin/mono.exe $MONO_INSTALL_PATH/lib/mono/2.0/mkbundle.exe LongoMatch/bin/Release/LongoMatch.exe --deps -c -o obj/temp.c -oo obj/temp.o
gcc -mno-cygwin -g -o obj/LongoMatch.exe -Wall obj/temp.c `pkg-config --cflags --libs mono|dos2unix`  obj/minilogo.o obj/temp.o 
