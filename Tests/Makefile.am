ASSEMBLY = Tests
TARGET = library

LINK = $(REF_DEP_TESTS)

SOURCES = Core/TestColor.cs \
	Core/TestCoordinates.cs \
	Core/TestDashboardButton.cs \
	Core/TestEventType.cs \
	Core/TestFrameDrawing.cs \
	Core/TestHotkey.cs \
	Core/TestImage.cs \
	Core/TestMediaFile.cs \
	Core/TestMediaFileSet.cs \
	Core/TestPenaltyCard.cs \
	Core/TestPoint.cs \
	Core/TestScore.cs \
	Core/TestTime.cs \
	Core/TestTimeNode.cs \
	Core/TestTimelineEvent.cs \
	Utils.cs

RESOURCES = Core/dibujo.svg

include $(top_srcdir)/build/build.mk

if ENABLE_TESTS
check:
	$(NUNIT_CONSOLE) $(ASSEMBLY_FILE)
if HAVE_COVEM
coverage:
	@rm -rf Tests.dll.covcfg.*
	@BABOON_CFG=Tests.dll.covcfg $(COVEM) $(NUNIT_CONSOLE_EXE) $(ASSEMBLY_FILE) || true
	@python print_coverage.py Tests.dll.covcfg.covreport
show-coverage:
	$(COV_GTK) $(ASSEMBLY).dll.covcfg.covdb
endif
endif

CLEANFILES += \
	Tests.dll.covcfg.*
	TestsResult.xml
