This separate fixture follows the actual pass3 public facts from51 failing
SQLite bodies and one separately retained representation diagnostic. It adds
only natural unsigned8/16/32/64 and signed32/64 pointer offsets, unsigned32
compound shifts, signed64 shift counts, and signed32/binary64 volatile memory.
The previous24-observation memory fixture remains separate.

The27 exact observations cover every new offset representation, target shift
values, signed32 constant/expression initialization, binary64 initialization,
rounding across2^53, qualified field loads/stores, and assignment/compound
results. The two evaluated initializers and two selected compound places each
run once. A captured volatile scalar local initializer emits a primitive store;
address-only qualification emits no load. Scalar access requires a naturally
aligned runtime address. Other volatile widths, aggregates, bitfields and
atomic-qualified objects remain unsupported; the unsigned64 volatile negative
fixture remains rejected despite admitting binary64 memory of the same size.

Coordinator-only reproduction, using a selected isolated project snapshot:

```sh
python3 edgcpp/roslyn-csharp/tests/MemoryWidths/run-memory-widths.py \
  --project-root /absolute/snapshot/edgcpp/roslyn-csharp \
  --operator-facts /mnt/storage/roslynobj-csharp-20261015/t3/sqlite-operator-facts-3.json \
  --aot-cc clang-18 --artifacts /absolute/new/memory-width-artifacts
```

The driver pins the frozen genuine producer, exports the focused positive and
negative inputs, checks public operation inventory, and compares the complete
native/JIT/NativeAOT outputs twice each. It records compiler, source, object,
build, generated-source, binary and dependency hashes, verifies deterministic
emission and source/input stability, and checks the unsupported variant before
output publication. `--objects` can reuse standard-named frozen objects while
explicitly recording that export was not reproduced. The native application is
only an independent oracle; generated execution depends on Generated and the
C# runtime. This proof adds no production CLI or full SQLite execution claim.
