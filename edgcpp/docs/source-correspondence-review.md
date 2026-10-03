# Source-correspondence review

The revised backend is tested for both execution and recognizable source.
Compiler build012 passes all six configured SQLite corpora against native and
checked-in transcripts under JIT and NativeAOT. Separate consumers using the
original public API/type names and exercising independent-owner lifecycle also
pass both runtimes:22 project checks in total. All945 frozen implementation
input hashes are unchanged. Durable evidence is
`evidence/sqlite-source-correspondence-final.json`.
These Linux x64 results do not establish complete C++ or platform coverage.

The retained side-by-side review is
`artifacts/campaign/persistent-continuation/source-review-final-012/review.json`.
It records exact original/generated body hashes and source locations for
`sqlite3_open`, `sqlite3PutVarint`, `putVarint64` and `sqlite3TwoPartName`.
Each body is saved separately beside that index. All four final012 bodies match
the previously reviewed010 bodies byte for byte. The generated project is
`artifacts/campaign/persistent-continuation/sqlite-readable-final-012/core-managed`;
its `source-index.md` links to exact generated declaration lines.

The public-opening implementation remains recognizable:

```csharp
private static int sqlite3_open(sbyte* zFilename, sqlite3** ppDb)
{
    return openDatabase(zFilename, ppDb, 6U, (sbyte*)0);
}
```

Managed-library output exposes `sqlite3_open` publicly and keeps its implementation
in `sqlite3_open__body`, so consumers use source API names without a manifest
lookup. The original flags expression has been folded by EDG; macros are not
reconstructed from the source text.

The final loop of `putVarint64` retains `p`, `i`, `j`, `n` and `buf`:

```csharp
i = 0;
j = (n - 1);
for (; j >= 0; j--, i++)
{
    (p)[i] = buf[j];
}
return n;
```

Its earlier `if` and `do` also remain structured. Local fixed arrays have named
ref views over the aligned frame, which preserves their existing address and
copy semantics. Explicit pointer/address uses retain typed storage pointers;
ordinary fixed-variable indexing uses the named array. Source fields retain
names and EDG offsets. Bitfields use source-named properties with private exact
storage tiles; consumed assignment results still perform required truncation.

The review deliberately permits localized transformations where C# evaluation
or storage differs. For example, `buf[n++] = expression` can retain a saved RHS
and destination pointer to preserve assignment sequencing. Narrowing casts,
ABI wrappers, cleanup regions and the frame itself remain explicit. Ordinary
assignments no longer call numbered assignment helpers, and simple structured
branches/loops are not flattened into dispatch labels. Irreducible transfers,
Duff dispatch and lifetime/setjmp requirements retain legal fallback labels.

At the build010 review checkpoint, source-typed enum declarations were still
pending. Build012 subsequently retains representable enum types in declarations
and signatures, with explicit physical promotions only at C# expression/ABI
boundaries. Its 12 focused checks and existing enum-exception regressions pass.
Enums wider than 64 bits remain named constant containers. Later changes must
preserve this review and the original-C, JIT and NativeAOT regression evidence.

Final compiler012 also passes the14 naming/shadow checks and six layout/navigation
checks recorded in `evidence/source-type-final-regressions.json`. These complement
the enum12 and earlier focused expression/control/array/exception checks; repeated
runs are not counted as additional distinct project coverage. The SQLite profile
uses the deterministic memory VFS and `SQLITE_THREADSAFE=0`. Production OS VFS,
shared-engine concurrent access and execution on Windows/macOS/Arm64 remain
outside this acceptance.
