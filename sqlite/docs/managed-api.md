# Managed library output

The compiler's `--emit=managedlib` output is an ordinary reusable C# class library.
It exposes translated functions through `DotCcLib`, C globals through
`DotCcGlobals`, and aggregate/enum types as public declarations. This is a low-level
unsafe API, including generated helpers; C ownership and lifetime rules still
apply. Function-pointer signatures use managed `delegate*` calling conventions.

A C# extension registers its static callback explicitly through the translated
registration API, retaining its callback code and context for the registration's
lifetime. It unregisters before releasing that context. No native export wrappers,
native SQLite dependency, native SQLite import bindings, or extension-discovery/loading
path is generated. The shared dotcc libc implementation remains supplied in the
assembly; optional unused OS/dlfcn runtime methods are not an extension host.
The product's [host VFS](host-vfs.md) does use explicitly allowed OS-level P/Invoke
for file locking and durability, alongside BCL file I/O.

The existing `-shared` output remains a separate native-export mode.

From `sqlite/`, the engine entry command is:

```sh
scripts/emit-engine.sh
dotnet build generated/TranslatedSqlite/TranslatedSqlite.csproj -c Release
```

`src/engine.c` includes the unchanged amalgamation and memory VFS as one logical
translation unit. The script supplies the shared feature configuration and the
host-registration switch; `Directory.Build.targets` includes the managed OS
sidecars. `dotcc-host` is the library default, while the named memory adapter
remains available. dotcc emits offsetof
constants directly into the library source. The command produces the working engine; the remaining campaign checks are
recorded in `PLAN.md` and `validation.md`.
The CLI's optional `-c` also builds the emitted project.
A consumer references the resulting project or assembly normally.

The library API equivalents are `Compiler.EmitCSharp(..., emit:
EmitMode.ManagedLib)` and `Compiler.BuildGeneratedCsproj(managedLibrary: true)`. Current object fragments also retain public types,
so `Compiler.LinkObjects(..., emit: EmitMode.ManagedLib)` needs no generated-source
rewriting. Explicit native import/archive bindings are rejected in managed mode.

`ManagedLibraryTests` compiles the translated library and its C# consumer into
separate assemblies. It checks public aggregates, inline-array wrappers, enums,
globals, static callback-table initialization and explicit callback registration.
Both code images are rooted in a noncollectible test load context; callbacks run
across forced collections, and registration is cleared before its stack context
expires. This validates the compiler seam independently of SQLite.

The original CLI integration smoke used the then-optional offset analyzer and ran a separate C# consumer under both the JIT and NativeAOT
(`linux-x64`). Both returned `42 8`: the explicit managed callback result and the
generated offset of the callback table's context field. Combining managed mode
with `-shared`, `--shared`, or an explicit native library binding failed as intended.
These checks exercise a reduced callback table, not the SQLite engine.


The actual `tests/ManagedConsumer` project references the complete generated
SQLite library and selects WAL on its real temporary database. Its JIT and
linux-x64 NativeAOT checks exercise schema,
indexes/views/triggers, prepared inserts, joins/aggregates, correlated queries,
CTEs/windows, JSONB, updates/upserts/deletes, transactions/savepoints, and integrity.
FTS5 coverage includes CRUD/MATCH/highlight, an explicitly registered C# auxiliary
function, and the unicode61 tokenizer API. Auxiliary context destruction is
checked once on close; no extension loading is involved.

The scalar callback re-enters SQL on the same connection and forces GC while its
nested statement is live. Identity stress reuses cached addresses, including the
exported canonical `sqlite3_free` pointer, across allocation and collection cycles.
The consumer also caches each of its own callbacks. Statements, callback contexts,
databases and VFS resources are released. Run `scripts/test-managed-consumer.sh`;
set `SQLITE_AOT=1` to repeat with NativeAOT. Detailed current-phase evidence is in
`validation.md`; the earlier baseline timings are historical measurements.


## Canonical function addresses

Generated function designators and address expressions now read canonical static
readonly fields. In managed-library output, translated definitions (including variadic methods)
are available through `DotCcFunctionPointers`, including functions whose addresses
were not taken by the C input. A C# consumer should reuse that field:

```csharp
private static readonly unsafe delegate*<void*, void> FreePointer =
    DotCcFunctionPointers.sqlite3_free;
```

Capture each application callback once in its own static readonly field and reuse
that value for registration and comparisons. Cache initialization only captures
method addresses; it does not run SQLite initialization or read its globals.
Direct calls continue to use `DotCcLib` normally. Null pointers and integer
sentinels retain their values. CS8909 warnings may still occur at comparisons;
identity comes from reusing the captured value, not from comparing independently
captured method addresses.

Variadic methods now take `params ReadOnlySpan<VaArg>`. Their canonical callback
fields have an explicit `ReadOnlySpan<VaArg>` final parameter; function-pointer
calls supply the span explicitly. `VaList` is a borrowed `ref struct`, with
independent cursors on copies. Recompile generated libraries, object fragments
and consumers together; this changes the managed signature. See
[varargs span API and validation](varargs-span.md) for lifetimes and measurements.
