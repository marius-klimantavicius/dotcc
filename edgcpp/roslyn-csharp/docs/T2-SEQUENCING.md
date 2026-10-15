# Typed sequencing execution

[The immutable receipt](../acceptance/sequencing/report.json) passes 54 expected
outcomes: 53 successful phases and one expected unsupported-admission rejection.
SHA256: `d82051fbd91fee239b675309c8bb64e8ec144e6dce964b713dfcd54d5f6baaa0`.
The authoritative artifact directory is
`/mnt/storage/roslynobj-csharp-20261015/t2/sequencing-reproduction`.

| Genuine input | Native/JIT/NativeAOT result | Additional exact observations |
| --- | --- | --- |
| branches | 41 | 21 ordered events and final value; conditional/short-circuit/comma effects; loop condition/increment prefixes and continue routing |
| places | 15 | 14 events, 11 values; RHS-before-LHS ordering, once-only receivers, narrow/bitfield stored values and conditional/comma lvalues |
| callee | 18 | Six events and value; retained callee-before-argument order and once-only indirect callee evaluation |
| aggregate-snapshot | 248 | Four events and six values; explicit named-local aggregate copies at recorded initialization/assignment occurrences |

Each generated checker executes the fixture twice and checks every value/event,
then prints the native observation format. JIT and NativeAOT match exactly.
Repeated source/project emission is identical; source/object/compiler/build and
runtime dependency hashes are recorded. The planner consumes actual retained
operand occurrences, typed values/places and effect prefixes. Loop and branch
prefixes execute at their retained occurrences, including continue paths; they
are not hoisted into an inventory schedule.

The genuine over-aligned typedef fixture records effective size/alignment 4/16
and unwrapped integer 4/4. Paired original/raw native representation guards reject
it with CSHARP2100 before creating output. This is unsupported ABI admission,
not a C++ semantic incompatibility claim or a damaged object test. The ordinary
callback guard regression separately passes result 751 and all seven events.

The original aggregate candidate remains excluded as a producer/native
[semantic differential](../acceptance/semantic-differentials/aggregate.json),
with exact source/object/public trace: retained roots predict 248, GCC observes
600, and the initial 430 prediction was wrong. No schedule is changed to match
GCC. This evidence does not classify that disagreement as translator success or
failure. General return-value materialization, partial/different-representation
aggregate copies, conditional bitfield lvalues and combined planned variadic
operators remain unsupported or unaccepted. Source temporary verbosity remains
an explicit readability follow-up. T2 and SQLite execution remain open.

Reproduce under a complete source freeze with a fresh artifact directory:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t2-sequencing.py \
  --objects /mnt/storage/roslynobj-csharp-20261015/t2/sequencing/export \
  --rejection-object /mnt/storage/roslynobj-csharp-20261015/t2/export/over-aligned-variadic.roslynobj \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-SEQUENCING-ATTEMPT
```
