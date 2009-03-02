/c/Archivos\ de\ programa/Mono-2.0/bin/gacutil -i LongoMatch/bin/Release/CesarPlayer.dll
/c/Archivos\ de\ programa/Mono-2.0/bin/gacutil -i LongoMatch/bin/Release/Db4objects.Db4o.dll
export PKG_CONFIG_PATH=/e/mono/lib/pkgconfig/
mkdir -p obj
windres LongoMatch/minilogo.rc obj/minilogo.o
/c/Archivos\ de\ programa/Mono-2.0/bin/mkbundle2 LongoMatch/bin/Release/LongoMatch.exe --deps -c -o obj/temp.c -oo obj/temp.o
gcc -mno-cygwin -g -o obj/LongoMatch.exe -Wall obj/temp.c `pkg-config --cflags --libs mono|dos2unix`  obj/minilogo.o obj/temp.o 
