# Managed library output

The compiler's `--emit=managedlib` output is an ordinary reusable C# class library.
It exposes translated functions through `DotCcLib`, C globals through
`DotCcGlobals`, and aggregate/enum types as public declarations. This is a low-level
unsafe API, including generated helpers; C ownership and lifetime rules still
apply. Function-pointer signatures use managed `delegate*` calling conventions.

A C# extension registers its static callback explicitly through the translated
registration API, retaining its callback code and context for the registration's
lifetime. It unregisters before releasing that context. No native export wrappers,
native SQLite dependency, native import bindings, or extension-discovery/loading
path is generated. The shared dotcc libc implementation remains supplied in the
assembly; optional unused OS/dlfcn runtime methods are not an extension host.

The existing `-shared` output remains a separate native-export mode.

From `sqlite/`, the engine entry command is:

```sh
scripts/emit-engine.sh
dotnet build generated/TranslatedSqlite/TranslatedSqlite.csproj -c Release
```

`src/engine.c` includes the unchanged amalgamation and memory VFS as one logical
translation unit. The script supplies the shared feature configuration. dotcc emits offsetof
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


The actual `tests/ManagedConsumer` project now references the complete generated
SQLite library and passes under the JIT. It checks SQLite 3.50.4, JSONB including
Unicode, FTS absence and explicit C# scalar-function registration. The callback
re-enters SQL on the same database, forces GC while its nested statement is live,
and returns the expected values. A 50,000-iteration capture/allocation loop checks
fresh managed function pointers against saved identities across collections.
Unregistering calls the destructor exactly once; context, statements, database and
VFS resources are released. This is observed JIT stress, not proof of a particular
tiering event. Run `scripts/test-managed-consumer.sh`; set `SQLITE_AOT=1` to repeat
with NativeAOT. The actual linux-x64 NativeAOT publish and execution also pass
all of these checks (12.00 seconds publish; runtime under 0.01 seconds).
