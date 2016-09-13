#!/bin/sh
export CERBERO_PREFIX=/opt/longomatch-build_darwin_darwin_x86_64/dist/darwin_x86_64/
export PATH=$PATH:/opt/longomatch-build_darwin_darwin_x86_64/build-tools-darwin-x86_64/bin/:$CERBERO_PREFIX/bin/
export PKG_CONFIG_PATH=$CERBERO_PREFIX/lib/pkgconfig/
export MONO_PATH=$CERBERO_PREFIX/lib/mono/4.5/:$CERBERO_PREFIX/lib/mono/4.5/Facades
export DYLD_FALLBACK_LIBRARY_PATH=$CERBERO_PREFIX/lib
open /Applications/Xamarin\ Studio.app $@ -n

