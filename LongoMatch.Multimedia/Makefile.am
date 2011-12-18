ASSEMBLY = LongoMatch.Multimedia
TARGET = library

LINK = $(REF_DEP_LONGOMATCH_MULTIMEDIA)

SOURCES = \
	Capturer/FakeCapturer.cs \
	Capturer/GstCameraCapturer.cs \
	Capturer/LiveSourceTimer.cs \
	Capturer/ObjectManager.cs \
	Common/Constants.cs \
	Common/Enum.cs \
	Common/Handlers.cs \
	Editor/VideoSegment.cs \
	Editor/GstVideoSplitter.cs \
	Interfaces/ICapturer.cs \
	Interfaces/IPlayer.cs \
	Player/GstPlayer.cs \
	Player/ObjectManager.cs \
	Utils/FramesCapturer.cs \
	Utils/GStreamer.cs \
	Utils/IMetadataReader.cs \
	Utils/MpegRemuxer.cs \
	Utils/PreviewMediaFile.cs \
	Utils/TimeString.cs \
	Utils/VideoDevice.cs \
	MultimediaFactory.cs


include $(top_srcdir)/build/build.mk

EXTRA_DIST += LongoMatch.Multimedia.dll.config
