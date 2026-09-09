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

From `sqlite/`, the eventual engine command uses this form after compiler blockers
have been resolved:

```sh
dotnet ../DotCC/bin/Release/net10.0/dotcc.dll engine.c \
  --emit=managedlib -c -o generated/TranslatedSqlite \
  --offset-generator "$PWD/generators/DotCC.OffsetGenerator/bin/Release/netstandard2.0/DotCC.OffsetGenerator.dll"
```

`engine.c` above represents the configured amalgamation plus adapter translation
unit, not an existing completed translation. The campaign scripts continue to
record the actual current parser/lowering stage until that engine is ready.
`-c` builds the generated project; omit it to emit the reviewable source/project.
A consumer references the resulting project or assembly normally.

`--offset-generator` resolves an explicit analyzer DLL path and sets
`DOTCC_OFFSET_GENERATOR` in the generated project, causing Roslyn to supply layout
constants from the embedded metadata. Without it, the same declarations are
materialized by dotcc and the generated project is standalone. The option applies
to C# project output, including managed libraries and ordinary programs.

The library API equivalents are `Compiler.EmitCSharp(..., emit:
EmitMode.ManagedLib)` and `Compiler.BuildGeneratedCsproj(managedLibrary: true,
offsetGeneratorAssembly: path)`. Current object fragments also retain public types,
so `Compiler.LinkObjects(..., emit: EmitMode.ManagedLib)` needs no generated-source
rewriting. Explicit native import/archive bindings are rejected in managed mode.

`ManagedLibraryTests` compiles the translated library and its C# consumer into
separate assemblies. It checks public aggregates, inline-array wrappers, enums,
globals, static callback-table initialization and explicit callback registration.
Both code images are rooted in a noncollectible test load context; callbacks run
across forced collections, and registration is cleared before its stack context
expires. This validates the compiler seam; SQLite's actual reusable assembly and
extension registration remain campaign integration work until translation succeeds.

The CLI integration smoke also built a generated library with the actual offset
analyzer enabled and ran a separate C# consumer under both the JIT and NativeAOT
(`linux-x64`). Both returned `42 8`: the explicit managed callback result and the
generated offset of the callback table's context field. Combining managed mode
with `-shared`, `--shared`, or an explicit native library binding failed as intended.
These checks exercise a reduced callback table, not the SQLite engine.
