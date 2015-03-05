ASSEMBLY = Tests
TARGET = library

LINK = $(REF_DEP_TESTS)

SOURCES = Core/Common/TestColor.cs \
	Core/Common/TestImage.cs \
	Core/Store/Drawables/TestAngle.cs \
	Core/Store/Drawables/TestCircle.cs \
	Core/Store/Drawables/TestCounter.cs \
	Core/Store/Drawables/TestCross.cs \
	Core/Store/Drawables/TestDrawable.cs \
	Core/Store/Drawables/TestEllipse.cs \
	Core/Store/Drawables/TestLine.cs \
	Core/Store/Drawables/TestMultipoints.cs \
	Core/Store/Drawables/TestQuadrilateral.cs \
	Core/Store/Drawables/TestRectangle.cs \
	Core/Store/Drawables/TestText.cs \
	Core/Store/Templates/TestTeamTemplate.cs \
	Core/Store/TestCoordinates.cs \
	Core/Store/TestDashboardButton.cs \
	Core/Store/TestEventType.cs \
	Core/Store/TestFrameDrawing.cs \
	Core/Store/TestHotkey.cs \
	Core/Store/TestMediaFile.cs \
	Core/Store/TestMediaFileSet.cs \
	Core/Store/TestPenaltyCard.cs \
	Core/Store/TestPlayer.cs \
	Core/Store/TestPlaysFilter.cs \
	Core/Store/TestPoint.cs \
	Core/Store/TestProject.cs \
	Core/Store/TestProjectDescription.cs \
	Core/Store/TestScore.cs \
	Core/Store/TestTag.cs \
	Core/Store/TestTime.cs \
	Core/Store/TestTimeNode.cs \
	Core/Store/TestTimelineEvent.cs \
	Core/Store/TestTimer.cs \
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
