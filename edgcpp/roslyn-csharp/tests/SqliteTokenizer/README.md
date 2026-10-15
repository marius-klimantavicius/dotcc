# Finite legacy FTS5 tokenizer interface

The fixture registers one test-owned lowercase ASCII-space tokenizer through the
actual engine-provided `fts5_api.xCreateTokenizer` pointer. It gets the API with
`SELECT fts5(?1)` and `sqlite3_bind_pointer` of a live pointer slot tagged
`fts5_api_ptr`; the statement finalizes before the slot/tag lifetime ends. The
test never calls an internal FTS5 routine by a guessed source name.

The accepted public record is `fts5_api` 48 bytes/alignment 8: version at 0,
legacy tokenizer registration at 8, and other retained slots at 16/24/32/40.
Legacy `fts5_tokenizer` is 24/8 with xCreate/xDelete/xTokenize at 0/8/16.
`Fts5Tokenizer` is captured incomplete and passed as an opaque pointer. The new
validator checks those exact public layouts and finds matching actual generated
declarations before emitting aliases. It verifies 24 ordinary APIs and six exact
Cdecl function shapes, including the nested engine token callback and registry
destructor. Main also checks actual generated field offsets/sizeof physically.

The callback contracts are the pinned sqlite3.h public FTS5 comments and
declarations at lines 14100–14215 and 14342–14438. Callback implementations are
test code, not native SQLite application bodies or provider helpers. xCreate
allocates a native owner; xDelete frees it once. The registration context,
counters, method table and fixed event storage outlive database close. Every
callback catches exceptions. xTokenize accepts only the bounded test ASCII byte
alphabet, emits slices of the original input and original byte offsets with
token flags 0, calls the supplied engine xToken pointer synchronously, and
immediately propagates a nonzero result. No borrowed bytes/context escape.

Eight SQL statements create/populate, MATCH/highlight, prefix/snippet,
update/query and delete/query the FTS table. The double-space document proves
offsets use original bytes. All exact typed rows and callback events are native
observations. The workload SHA256 is
`bbdc900bcdd58bf38723bffd1c5fe2bf44153cd5d92135504ee9da676b6b2579`.

The first measured pinned native oracle passed at
`/mnt/storage/roslynobj-csharp-20261015/t4/native-tokenizer-1/native-tokenizer.json`:
84 ordered events, one create/delete pair and one registry destructor. Its 20
tokenization calls include document 4, query 1, prefix 3 and auxiliary 8 flags;
41 tokens preserve exact text/offset/result observations. These counts belong to
this finite input/provider, not general creation/callback count guarantees.

The coordinator owns execution/build scheduling. Reproduction starts with the
separate pinned native oracle process:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteTokenizer/run-native.py \
  --build-receipt edgcpp/roslyn-csharp/acceptance/sqlite/native-oracle-build.json \
  --output "$NEW_NATIVE_REPORT"
```

Use the actual readable production project and baseline API report:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteTokenizer/ValidateApi.py \
  --generated-project "$GENERATED_PROJECT" --base-api-report "$BASE_API_REPORT" \
  --aliases "$API_ALIASES" --report "$API_REPORT"

dotnet build edgcpp/roslyn-csharp/tests/SqliteTokenizer/Checks.csproj \
  -c Release --disable-build-servers -p:UseSharedCompilation=false \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$JIT_OUTPUT"

dotnet "$JIT_OUTPUT/Checks.dll" edgcpp/roslyn-csharp/tests/SqliteTokenizer/workload.json \
  "$NEW_GENERATED_REPORT" "$NATIVE_REPORT" "$API_REPORT"

dotnet publish edgcpp/roslyn-csharp/tests/SqliteTokenizer/Checks.csproj \
  -c Release -r linux-x64 -p:PublishAot=true --disable-build-servers \
  -p:UseSharedCompilation=false -p:CppCompilerAndLinker=/usr/lib/llvm-18/bin/clang \
  -p:GeneratedProject="$GENERATED_PROJECT" -p:BaseApiReport="$BASE_API_REPORT" \
  -p:ApiAliases="$API_ALIASES" -p:ApiReport="$API_REPORT" -o "$AOT_OUTPUT"

"$AOT_OUTPUT/Checks" edgcpp/roslyn-csharp/tests/SqliteTokenizer/workload.json \
  "$NEW_AOT_REPORT" "$NATIVE_REPORT" "$API_REPORT"
```

The generated process directly executes generated SQLite bodies, compares the
complete native semantic object after close, and rejects loaded native SQLite
libraries. JsonArray entries use explicit JsonNode overloads for AOT. Generated
execution awaits measured coordinator evidence. Locale/v2 tokenizers, synonyms,
custom auxiliary functions, FTS3 tokenizer variants and broader failure/error
paths remain separate. This successful SQL slice does not force a nonzero engine
token callback return, although the implementation propagates it.


Coordinator verification now passes native, JIT and NativeAOT: all five measured
phases, exact semantic comparison, immutable sources/pins and generated runtime
dependency separation. See [the compact receipt](../../acceptance/sqlite/tokenizer-report.json).
The final publication is source 13; broad unexercised variants remain explicit
in the receipt and current T4 ledger.
