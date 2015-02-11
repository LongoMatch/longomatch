ASSEMBLY = LongoMatch.Multimedia
TARGET = library

LINK = $(REF_DEP_LONGOMATCH_MULTIMEDIA)

SOURCES = Capturer/FakeCapturer.cs \
	Capturer/GstCameraCapturer.cs \
	Capturer/LiveSourceTimer.cs \
	Capturer/ObjectManager.cs \
	Common/Constants.cs \
	Common/Enum.cs \
	Common/Handlers.cs \
	Converter/GstVideoConverter.cs \
	Converter/ObjectManager.cs \
	Editor/GstVideoSplitter.cs \
	Editor/VideoSegment.cs \
	Player/GstPlayer.cs \
	Player/ObjectManager.cs \
	Remuxer/GstRemuxer.cs \
	Remuxer/ObjectManager.cs \
	Utils/GStreamer.cs \
	Utils/GstDiscoverer.cs \
	Utils/MultimediaFactory.cs \
	Utils/Seeker.cs \
	Utils/TimeString.cs \
	Utils/Devices.cs \
	Utils/WindowHandle.cs

DLLCONFIG = LongoMatch.Multimedia.dll.config


include $(top_srcdir)/build/build.mk

CLEANFILES += $(DLLCONFIG)
