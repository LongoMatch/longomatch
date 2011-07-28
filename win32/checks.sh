export GST_REGISTRY=/c/test.reg
# Get the list of plugins
./gst-inspect.exe > /c/checks.log 2>&1
# Check videosink
GST_DEBUG=3 ./gst-launch.exe videotestsrc num-buffers=2 ! d3dvideosink  >> /c/checks.log 2>&1
# Check H264
GST_DEBUG=3 ./gst-launch.exe videotestsrc num-buffers=2 ! x264enc ! decodebin2 ! d3dvideosink  >> /c/checks.log 2>&1
# Check MPEG-4
GST_DEBUG=3 ./gst-launch.exe videotestsrc num-buffers=2 ! xvidenc ! decodebin2 ! d3dvideosink  >> /c/checks.log 2>&1
# Check VP8
GST_DEBUG=3 ./gst-launch.exe videotestsrc num-buffers=2 ! vp8enc ! decodebin2 ! d3dvideosink  >> /c/checks.log 2>&1
# Check Theora
GST_DEBUG=3 ./gst-launch.exe videotestsrc num-buffers=2 ! theoraenc ! decodebin2 ! d3dvideosink  >> /c/checks.log 2>&1
