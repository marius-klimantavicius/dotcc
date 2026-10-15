# Bounded two-unit execution and production CLI

[The immutable receipt](../acceptance/program/report.json) passes 51 expected
outcomes: 49 successful phases and two expected open-unit budget rejections.
SHA256: `08bcd2ec1f0283032edb7006be2c74d1be5778377b6c6e6ebb95744c46845ddf`.
Artifacts are under `/mnt/storage/roslynobj-csharp-20261015/t2/program-reproduction-2`.
The earlier `program-reproduction` attempt is preserved incomplete: its driver
assumed a default dependency binary path after a redirected checker build. The
corrected driver explicitly builds and hashes the generated dependency before
checking its copy. No semantic implementation changed for that correction.

| Genuine pair | Native/direct JIT/direct AOT/CLI JIT/CLI AOT | Exact observations |
| --- | --- | --- |
| C | 1150 | All ten values, twice per generated checker |
| C++ | 1212 | All eight values, twice per generated checker |

Accepted public ProgramAssembly identities select definitions and drive every
function/data reference, address fixup and canonical wrapper. Display names do
not rebind semantics. Both pairs prove separate same-spelling internal functions
and storage, one external allocation shared across units, and canonical function
and data addresses across declarations. The C pair also passes an opaque pointer
between incomplete/complete views and uses cross-unit address constants. C++
overloads remain distinct captured identities. C++ display names remain readable
as local_helper/local_helper__2 and choose/choose__2 with deterministic naming.

The production CLI inventories sequentially, rehashes each admitted immutable
input, then opens at most the explicitly requested two-unit budget for emission.
It passes ProgramAssembly options to the accepted selection implementation;
required imports are fulfilled only through selected definitions, never a second
name-based CLI filter. Eight required C declarations and seven C++ declarations
are fulfilled through recorded selection. The initial program path is
self-contained: required cross-unit native providers, variadics and aggregate
boundaries are not accepted. Single-TU pinned-provider behavior stays separate.

Source/object/native-TU/compiler/build hashes, exact selection/allocation/address
audits, direct and CLI deterministic repeats, completion input/file hashes and
managed/native dependency closure are recorded. Generated runtime manifests
contain only Generated and Runtime, with no reader/SQLite or native application
body/provider. Budgets reject two units when only one simultaneous unit is
allowed, with output unpublished. Publication remains atomic. Reader/emitter
source inputs were frozen throughout the final receipt.

This proves two small units with fixed scalar/pointer boundaries and integer
callback identity. It does not prove scale, C++ ODR/full pointee equivalence,
general pointer-callback adaptation, native aggregate calls, multi-unit variadics,
dynamic initialization or cleanup/EH. T2 remains open for the remaining ABI and
readability gates; SQLite T3/T4 has not run.

Reproduce with a fresh directory and complete source freeze:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t2-program.py \
  --objects /mnt/storage/roslynobj-csharp-20261015/t2/program-execution/export \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-PROGRAM-ATTEMPT
```
