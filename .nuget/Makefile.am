NuGet.exe:
	python -c "from urllib import urlretrieve;import ssl;context = ssl._create_unverified_context();urlretrieve('http://nuget.org/nuget.exe', 'NuGet.exe', context=context);"

restore: NuGet.exe
	mono NuGet.exe install $(top_srcdir)/LongoMatch.Core/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/LongoMatch.Services/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/LongoMatch.DB/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/Tests/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../
	mono NuGet.exe install $(top_srcdir)/Weavers/packages.config -source "https://www.nuget.org/api/v2/;$(expanded_datadir)/nuget" -NonInteractive -solutionDir ../

all: restore

.PHONY: restore
