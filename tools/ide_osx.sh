#!/bin/sh
export CERBERO_PREFIX=~/oneplay-build/dist/darwin_x86_64/
export PATH=$PATH:~/oneplay-build/build-tools-darwin-x86_64/bin/:$CERBERO_PREFIX/bin/
export PKG_CONFIG_PATH=$CERBERO_PREFIX/lib/pkgconfig/
export MONO_PATH=$CERBERO_PREFIX/lib/mono/5.5/:$CERBERO_PREFIX/lib/mono/4.5/Facades
export DYLD_FALLBACK_LIBRARY_PATH=$CERBERO_PREFIX/lib
open -n /Applications/Xamarin\ Studio.app

