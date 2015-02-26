ASSEMBLY = Tests
TARGET = library

LINK = $(REF_DEP_TESTS)

SOURCES = Core/TestTime.cs \
	Core/TestTimeNode.cs \
	Core/TestTimelineEvent.cs \
	Utils.cs

RESOURCES = 

include $(top_srcdir)/build/build.mk

check:
		nunit-console $(ASSEMBLY_FILE) --result=$(ASSEMBLY)-test-results.xml
