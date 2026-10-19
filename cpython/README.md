# CPython on DotCC

A22 enables exactly twelve test-only socket dependencies after B26's real service
proof. All41 policy tests, unchanged socket-unit emissions and16 real C
resolver/self-pipe raw/processed JIT/AOT cells pass. Actual fixture PE/PDB/ILC
bindings and negative controls pass. These are source/fixture results; actual
Python asyncio and full product qualification remain open.

Canonical286 remains hostname receipt `20261018-045653-2ea433b1`; embedded193
remains oldV11 `20261018-020814-eb353b40`. Separate whole286 V15 generation and
fresh actual consumer D10/ABI proof precede the early required-suite retry.
B22 retains1999 executed/1995 passed, one error, one failure, one required skip,
one interruption and2868 unrun among4867 required cases. No classification changes.
See [current validation](docs/validation.md) and
[A22 handoff](artifacts/owner-a-r22-20261018/a22-handoff.json). P0 is in progress;
P1–P6 remain open. Historical results follow with their original artifact scope.

Implementation in progress for exact-pin CPython 3.13.15, profile `embedded-gil`.
The full 193-unit real C library emits, links, builds and publishes both forms.
Frozen embedded receipt `20261017-233444-23350c71` passes the raw and processed JIT
owning consumer for two cycles each, with exact JSON, callbacks, errors and
finalization. Both forms have source/PE-bound address and constant-data proof.
The separate 286-unit test profile now emits, links, builds and publishes both
forms under a consistent test-only fork-hook policy (receipt
`20261018-013343-0e127aa3`); process creation remains rejected. Its readiness and
actual controls have their own PE-bound evidence, with frozen upstream tests
still unrun. Fresh full runtime/ABI/matrix/rooted-AOT and production
preservation gates remain open; tools are provisional and P0–P6 is not complete.

See [PLAN](docs/PLAN.md), [implementation handoff](docs/IMPLEMENTATION.md),
[validation](docs/validation.md) and [blockers](docs/blockers.md). The product must
retain CPython's parser/evaluator/object algorithms, pymalloc, cyclic GC and real
GIL-backed threads, using shared DotCC.Libc services. The managed internal ABI has
unsigned 16-bit `wchar_t`; ordinary Linux native controls have 32-bit `wchar_t`.

From any working directory, standard helpers dispatch through the shared campaign
framework (Python 3.11+, Bash, .NET 10). Native generator preparation additionally
requires Linux, a C compiler and make; Windows execution/prepared-generator reuse
is not yet implemented. Fetch policy does not control NuGet restore.

```bash
bash Scripts/campaign.sh layout cpython --check
bash cpython/scripts/fetch.sh --fetch missing
bash cpython/scripts/test.sh --suite native --fetch never
bash cpython/scripts/test.sh --suite inventory --fetch never
flock --close /tmp/dotcc-cpython-shared-build.lock bash cpython/scripts/translate.sh --fetch never --jobs 2 --form all
```

`inventory` deliberately freezes source and stdlib metadata from the same-pin native
build/import inputs; review its changes before qualification. Native tools live
under ignored `build/native`, logs/receipts under ignored `artifacts`. Acquired
`ref` sources remain unmodified. The native interpreter is solely a generator and
oracle; it is never a runtime backend. The authored consumer uses `CPythonProject`
to select the published raw or processed library. The owning API guide is
[usage.md](docs/usage.md).

All shared dotnet/compiler builds and publication commands in this shared
checkout must use the shown lock. Native helpers acquire their separate native
lock and run at most `make -j2`. No commits are made in this review round.
