#!/bin/bash
export CERBERO_PREFIX=~/oneplay-build/dist/linux_x86_64/
export PATH=$PATH:~/oneplay-build/build-tools-linux-x86_64/bin/
export PKG_CONFIG_PATH=$CERBERO_PREFIX/lib/pkgconfig/
export MONO_PATH=$CERBERO_PREFIX/lib/mono/4.5/:$CERBERO_PREFIX/lib/mono/4.5/Facades
export LD_LIBRARY_PATH=$CERBERO_PREFIX/lib
monodevelop $@

