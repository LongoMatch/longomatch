#!/bin/bash

OUTPUT_DIR=/c/gstreamer
OUTPUT_DIR=/c/gstreamer
OUTPUT_DIR_LIB=$OUTPUT_DIR/lib
OUTPUT_DIR_GST_LIB=$OUTPUT_DIR_LIB/gstreamer-0.10
OUTPUT_DIR_BIN=$OUTPUT_DIR/bin
OUTPUT_DIR_INCLUDE=$OUTPUT_DIR/include

mkdir -p $OUTPUT_DIR
mkdir -p $OUTPUT_DIR_LIB
mkdir -p $OUTPUT_DIR_GST_LIB
mkdir -p $OUTPUT_DIR_BIN
mkdir -p $OUTPUT_DIR_INCLUDE

cp ./build/Win32/ReleaseWdkCrt/lib/gstreamer-0.10/lib*dll $OUTPUT_DIR_GST_LIB
cp ./build/Win32/ReleaseWdkCrt/bin/*.dll $OUTPUT_DIR_BIN
cp ./build/Win32/ReleaseWdkCrt/bin/gst*.exe $OUTPUT_DIR_BIN
cp ./build/Win32/ReleaseWdkCrt/lib/*.lib $OUTPUT_DIR_LIB
cp -R ./build/Win32/ReleaseWdkCrt/include/* $OUTPUT_DIR_INCLUDE