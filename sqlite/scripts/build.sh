#!/usr/bin/env bash
# Fetch pinned inputs, build dotcc and its generator, then translate/build SQLite.
# No tests, native oracle, consumer execution, or AOT publishing.
source "$(dirname "$0")/common.sh"
python3 "$SQLITE_ROOT/scripts/fetch.py"
dotnet build "$DOTCC_ROOT/DotCC/DotCC.csproj" -c Release -p:UseLocalLalrCc=false --nologo
dotnet build "$SQLITE_ROOT/generators/DotCC.OffsetGenerator/DotCC.OffsetGenerator.csproj" -c Release --nologo
"$SQLITE_ROOT/scripts/emit-engine.sh"
dotnet build "$SQLITE_ROOT/generated/TranslatedSqlite/TranslatedSqlite.csproj" -c Release --nologo
