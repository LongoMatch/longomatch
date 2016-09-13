#!/bin/bash
export CERBERO_PREFIX=/opt/longomatch-build_linux_linux_x86_64/dist/linux_x86_64/
export PATH=$PATH:/opt/longomatch-build_linux_linux_x86_64/build-tools/bin/
export PKG_CONFIG_PATH=$CERBERO_PREFIX/lib/pkgconfig/
export MONO_PATH=$CERBERO_PREFIX/lib/mono/4.5/:$CERBERO_PREFIX/lib/mono/4.5/Facades
export LD_LIBRARY_PATH=$CERBERO_PREFIX/lib
monodevelop $@ &

