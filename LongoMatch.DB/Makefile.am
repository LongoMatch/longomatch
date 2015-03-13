ASSEMBLY = LongoMatch.DB
TARGET = library

LINK = $(REF_DEP_LONGOMATCH_DB)

SOURCES = AssemblyInfo.cs \
	CouchbaseStorage.cs \
	DocumentsSerializer.cs

RESOURCES = 

EXTRA_BUNDLE = $(COUCHBASE_LIBS_PATHS)

include $(top_srcdir)/build/build.mk
