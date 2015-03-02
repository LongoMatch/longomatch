ASSEMBLY = Tests
TARGET = library

LINK = $(REF_DEP_TESTS)

SOURCES = Core/TestColor.cs \
	Core/TestCoordinates.cs \
	Core/TestDashboardButton.cs \
	Core/TestFrameDrawing.cs \
	Core/TestImage.cs \
	Core/TestTime.cs \
	Core/TestTimeNode.cs \
	Core/TestTimelineEvent.cs \
	Utils.cs

RESOURCES = Core/dibujo.svg

include $(top_srcdir)/build/build.mk

check:
	nunit-console $(ASSEMBLY_FILE) --result=$(ASSEMBLY)-test-results.xml
