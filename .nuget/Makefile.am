NuGet.exe:
	curl -k -L -o "NuGet.exe" "http://nuget.org/nuget.exe"

restore: NuGet.exe
	mono NuGet.exe restore ../LongoMatch.sln -Source "https://www.nuget.org/api/v2/;$(CERBERO_PREFIX)/share/nuget" -NonInteractive

all: restore

.PHONY: restore
