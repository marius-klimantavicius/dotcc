#!/usr/bin/env bash
# Compile copied sources from two translations into ONE consumer assembly.
source "$(dirname "$0")/common.sh"
output="$SQLITE_ROOT/artifacts/copied-translations"
mkdir -p "$output"
cat > "$output/api.c" <<'C'
#include <stddef.h>
#include <errno.h>
typedef struct { int head; long tail; } Record;
typedef int (*Callback)(int);
static int counter;
int add(int n) { return n + ++counter; }
Callback pointer(void) { return add; }
int offset(void) { return offsetof(Record, tail); }
int set_error(int n) { errno=n; return errno; }
int get_error(void) { return errno; }
C
if [[ -n "${DOTCC_EXECUTABLE:-}" ]]; then
  compiler=("$DOTCC_EXECUTABLE")
else
  compiler=(dotnet "$DOTCC_ROOT/DotCC/bin/Release/net10.0/dotcc.dll")
fi
"${compiler[@]}" "$output/api.c" --emit=managedlib --nest-types --runtime=c \
  --namespace Managed.Database --class-name CopyA --split=function -o "$output/CopyA"
"${compiler[@]}" "$output/api.c" --emit=obj -o "$output/api.o"
"${compiler[@]}" "$output/api.o" --emit=managedlib --nest-types --runtime=auto \
  --namespace Managed.Database --class-name CopyB --split=size --split-size=1024 -o "$output/CopyB"
cat > "$output/Copied.csproj" <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks><Nullable>disable</Nullable>
  </PropertyGroup>
</Project>
XML
cat > "$output/Program.cs" <<'CS'
using Managed.Database;
internal static unsafe class Program
{
    private static int Main()
    {
        CopyA.Record a=default; CopyB.Record b=default;
        a.tail=11; b.tail=19;
        if(CopyA.add(41)!=42 || CopyB.add(41)!=42 || CopyA.add(40)!=42) return 1;
        if(CopyA.pointer()!=CopyA.CopyAFunctionPointers.add || CopyB.pointer()!=CopyB.CopyBFunctionPointers.add) return 2;
        CopyA.set_error(17); CopyB.set_error(23);
        if(CopyA.get_error()!=17 || CopyB.get_error()!=23 || a.tail+b.tail!=30) return 3;
        if(CopyA.offset()!=8 || CopyB.offset()!=8) return 4;
        System.Console.WriteLine("PASS copied translations: shared namespace, isolated types/runtime/globals and canonical callbacks");
        return 0;
    }
}
CS
# The consumer includes both generated source trees; no ProjectReference exists.
dotnet build "$output/Copied.csproj" -c Release --nologo > "$output/build.log" 2>&1
run_sqlite_process dotnet "$output/bin/Release/net10.0/Copied.dll"
if [[ "${SQLITE_AOT:-0}" == 1 ]]; then
  dotnet publish "$output/Copied.csproj" -c Release -r linux-x64 -p:PublishAot=true \
    -o "$SQLITE_ROOT/build/copied-translations-aot" --nologo > "$output/aot-build.log" 2>&1
  run_sqlite_process "$SQLITE_ROOT/build/copied-translations-aot/Copied"
fi
