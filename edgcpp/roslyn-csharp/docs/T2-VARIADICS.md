# Primitive SysV variadic execution

T2 remains open. [The immutable receipt](../acceptance/variadics/report.json)
passes all62 phases from a complete freeze of50 implementation inputs. SHA256:
`32976461fe72821a2cdc560188315477df2d31d0d1b8c4867e9e2bc9e6e761b2`.
Artifacts are under `/mnt/storage/roslynobj-csharp-20261015/t2/variadic-reproduction`.

| Genuine input | Native/JIT/NativeAOT | Executed evidence |
| --- | --- | --- |
| variadics | 1870 | Promoted integer/double/pointer tails, GP/SSE overflow, nested forwarding and independent va_copy cursors |
| interleaved | 314 | Independent interleaved GP/SSE exhaustion; fixed prefix consumes six GP/eight SSE registers plus two named stack slots; discarded pointer va_arg advances cursor |
| addressed | 216 | Canonical entry address/echo/managed dispatch, native borrowed va_list reentry returning55/120, identity1; actual generated/runtime cursor size24, alignment8 and offsets0/4/8/16 |
| outbound | 627 | Unknown native indirect and explicit direct variadic provider calls; all ten integer11..20/double10.5..19.5 observations match after register/overflow exhaustion |

Application bodies stay ordinary translated C#. Concrete helper packs use the
already-selected promoted argument types and pinned SysV GP/SSE/overflow storage;
no source promotion analysis occurs. Incoming concrete fixed-prefix C entries use
va_start, synchronously forward the borrowed cursor to a fixed managed bridge,
and va_end. Outbound concrete fixed-signature C wrappers call the selected native
variadic address. Generated C contains only ABI forwarding, no application
algorithms. Native C++ exception propagation remains unsupported.

The generated ABI library is compiled from Generated.Abi.c and copied to build
and publish outputs. The receipt hashes the compiler executables/versions, C
source, binary and identical output/published copies; exact exported entry and
registration names are checked against an allowlist. System ELF dependencies,
all native test provider sources/binaries, explicit import reports and generated
runtime dependencies are recorded. Generated programs run with LD_LIBRARY_PATH
removed, proving assembly-relative ABI library loading. Separate observation
checkers receive an explicit test-adapter path. Generated dependencies remain
Generated plus Runtime, with no reader/SQLite dependency. Repeat emission is
identical for all four inputs.

The accepted boundary is ordinary natural primitive integer/pointer/double
representations on Linux SysV AMD64, one TU. General effective typeref alignment
is not accepted: the current guard checks unwrapped size/alignment, and paired
effective/raw checks are required in the next snapshot before broader aliases or
over-aligned inputs. Aggregate/vector/extended variadic classes, lvalue/xvalue
va_arg slots, native exception crossing and effectful sequencing remain rejected
or unaccepted. The initial addressed physical-checker failure was a test pointer
cast syntax error; the corrected checker passes JIT/NativeAOT. No required
SQLite API or addressed variadic entry is silently omitted, and no SQLite
translation/SQL execution is claimed.

Reproduce with an unused artifact directory and a complete source freeze:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t2-variadics.py \
  --objects /mnt/storage/roslynobj-csharp-20261015/t2/export \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-VARIADIC-ATTEMPT
```

Typed planning and ProgramAssembly emitter helpers compile but are still
unintegrated in this checkpoint. Genuine sequencing inputs/native traces are
ready; the original aggregate candidate is preserved as an unresolved
[semantic differential](../acceptance/semantic-differentials/aggregate.json),
including its exact public trace. Retained roots predict248 while GCC observes600;
the initial430 source prediction was wrong. This is not classified as translator
success or failure from GCC alone. A separate explicit named-local snapshot input
has native248 and awaits generated execution. Generated multi-unit execution and
native aggregate-by-value ABI likewise remain open.
