LM_DEPS = \
	$(top_builddir)/bin/Couchbase.Lite.dll \
	$(top_builddir)/bin/Couchbase.Lite.Storage.SystemSQLite.dll \
	$(top_builddir)/bin/SQLitePCLPlugin_esqlite3.dll \
	$(top_builddir)/bin/Newtonsoft.Json.dll \
	$(top_builddir)/bin/Stateless.dll \
	$(top_builddir)/bin/SQLitePCL.raw.dll \
	$(top_builddir)/bin/SQLitePCL.ugly.dll \
	$(top_builddir)/bin/ICSharpCode.SharpZipLib.Portable.dll \
	$(top_builddir)/bin/PropertyChanged.dll \
	$(top_builddir)/bin/OxyPlot.dll \
	$(top_builddir)/bin/OxyPlot.GtkSharp.dll \
	$(top_builddir)/bin/Mono.Addins.dll

if OSTYPE_OS_X
LM_DEPS += \
	$(top_builddir)/bin/Sparkle.dll \
	$(top_builddir)/bin/Xamarin.Mac.dll
endif