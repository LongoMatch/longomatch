NuGet.exe:
	curl -k -L -o "NuGet.exe" "http://nuget.org/nuget.exe"

restore: NuGet.exe
	mono NuGet.exe install $(top_srcdir)/LongoMatch.Core/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/LongoMatch.Services/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/LongoMatch.DB/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/Tests/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../

all: restore

.PHONY: restore
