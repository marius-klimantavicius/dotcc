# Semantic C# emission verification

`verify.py` consumes previously exported packages and a built managed emitter.
It does not build or invoke EDG. Export `tests/plain.c`, `tests/probe.cpp`, and
this directory's `scalars.c`, `pointers.c`, `global.c`, `volatile.c`, `static-local.c`, and
`external-call.c`, `mixed-compound.c`, `cleanup.c`, and `unsigned-negate.c` into separate package
directories with those basename names.
Export `cleanup.c` with `--c11 --gcc` so EDG accepts the GNU attribute syntax.

```sh
python3 edgcpp/semantic-export/tests/emission-tests/verify.py \
  --emitter 'dotnet /absolute/path/Edg.Semantic.dll' \
  --packages /absolute/path/packages \
  --output /absolute/path/fresh-verification-directory
```

The host needs GCC, .NET 10, and its NativeAOT prerequisites. The default AOT
runtime is `linux-x64`; override `--runtime` for a compatible host. Every command,
stdout/stderr stream, source, build output, malformed package, and the final
`results.json` is retained under the fresh output directory.

To verify a NativeAOT build of the emitter after a complete JIT-emitter run,
pass its executable as `--emitter` and add
`--emission-only --compare-generated /absolute/path/previous-verification`.
This repeats generation and rejection checks, requires byte-identical generated
programs, and reuses their previous native/JIT/NativeAOT execution evidence.

All three supported C fixtures run against three implementations: native C compiled
from the package's exact source snapshot, generated C# under JIT, and generated
C# under NativeAOT. The managed projects enable checked arithmetic globally to
verify that generated C unsigned wrap and narrowing casts remain unchecked.
The oracle cases avoid undefined C behavior, including signed overflow, invalid
pointer access, and excessive shift counts.

The cases cover record layout and pointer field access, empty/negative loop
bounds, `continue` in a `for` loop, `break`, `while` and `do`, direct calls,
integer truthiness, comparison results used as integers, short-circuit side
effects, nested `if`/`else` binding, nested loop jump boundaries, signed
division/remainder, unsigned shifts and wrap, 64-bit constant boundaries,
narrowing casts, C# keywords, shadowed C locals, and null/non-null pointer
conditions (including logical negation and short-circuit evaluation).

`plain.golden.cs` is reviewed output for the existing source-correspondence
probe: named `Item.value`, `sum`, its parameters/locals, and structured
`for`/`if`/`continue` must remain visible. Output must also be byte-identical on
repeated emission. Update the golden only after reviewing the resulting source;
execution equality alone is insufficient.

Unsupported C++, globals, volatile memory, static locals, unresolved calls,
mixed-type compound assignment, unsigned 64-bit negation, and GNU cleanup attributes must fail with a
controlled diagnostic, create no output, and preserve existing output.
Malformed graphs test identity/reference integrity, unknown operators,
missing operands, statement-list cycles, and package graph hashes. Graph mutation
tests copy packages and recompute their graph hashes, except for the intentional
hash-mismatch test, so semantic validation is exercised past package integrity.
Self-referencing boolean expressions and missing source/integrity metadata must
also produce controlled rejections instead of crashes or successful output.

These checks establish the bounded emitter profile only. They do not establish
complete C/C++ coverage or the broader production backend acceptance criteria.
