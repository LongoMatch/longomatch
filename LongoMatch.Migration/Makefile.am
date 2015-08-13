PROJECT_FILE = LongoMatch.Migration.csproj
ASSEMBLY=LongoMatch.Migration.exe
OUTPUT_DIR = $(pkglibdir)/migration
include $(top_srcdir)/build/build.mk

install-data-hook:
	mv $(OUTPUT_DIR)/$(ASSEMBLY) $(OUTPUT_DIR)/LongoMatch.exe
	mv $(OUTPUT_DIR)/$(ASSEMBLY).mdb $(OUTPUT_DIR)/LongoMatch.exe.mdb
