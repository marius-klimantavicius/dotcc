# Valid guest-memory lifecycle

This harness exercises the retained upstream guest page-table algorithms through
`NewSystem`, `NewMachine`, `AllocatePageTable`, `ReserveVirtual`,
`CopyToUserWrite`, `CopyFromUserRead`, `ProtectVirtual` and `FreeVirtual`.
It does not replace those algorithms or test only the host allocation adapter.

Each of two sequential lifecycles:

1. Creates a long-mode System/Machine and actual root page table; reserves two
   read/write, non-executable guest pages and reads all bytes as initially zero.
2. Adds two adjacent pages and a separate fifth page. Two 96-byte transfers cross
   page boundaries; a separate-page transfer and surrounding zero canaries check
   exact data placement. Copy read/write address and size metadata is checked.
3. Changes the first two pages from RW/NX to R/NX, inspects page-table permission
   bits and performs only reads. It then restores RW/NX and repeats a valid write.
4. Unmaps one page, checks the page-table unmapped metadata without accessing it,
   remaps at the same address, verifies fresh zeroes, refills it, and verifies
   neighboring/separate data survived.
5. Unmaps all five pages, checks virtual-page accounting returns to zero, then
   frees the Machine and its now-orphaned System through the normal upstream path.

There are no invalid buffers, forbidden accesses, allocation exhaustion, custom
fault injection, ELF inputs, instruction traps or service/worker APIs. In
particular, `Copy*Read/Write` records access metadata but is not itself a
permission-enforcement probe. This gate covers permission metadata and permitted
accesses; it does not claim rejection/fault behavior for forbidden accesses.

Guest `vss` and `rss` are upstream page counts, including page-table allocations
in resident accounting. Their deterministic progression is printed and compared
exactly between original native Blink and all four managed modes. The managed
host slab pool is a separate ownership/accounting domain: after each upstream
lifecycle its retained allocation count/bytes must be positive and within the
active 64 MiB owner budget, and the second identical lifecycle must retain
exactly the same mapping count and byte count as the first. Retained slabs are expected until the complete worker
state is discarded. The final owner disposal happens once, after both lifecycles
and exit callbacks; no later translated upstream calls occur. Bindings are then
released by the managed consumer. This does not claim every generic `malloc`
allocation is charged to that host mapping budget.

Run only against the reviewed current canonical assembly receipt, after the
shared compiler has been frozen and the serial validation slot is released:

```sh
python3 blink/tests/GuestMemory/run.py --assembly-receipt <canonical-assembly-receipt>
```

`--native-only` produces partial native evidence, never a managed pass. Each
mode runs in a fresh process. The managed consumer forces compacting GC before
entering the C lifecycle and uses the copied owning Host bindings.

The runner checks the original native archive/config against the profile pins,
snapshots native headers and archive, builds the same authored probe natively,
and retains its exact ten-line transcript. It replaces only the canonical
`authored/managed-driver.c` object with this frontend. All other canonical
objects, including HostMemory and guest memory algorithms, retain their original
producer hashes. The emission uses the original canonical header snapshot;
no shared profile, generated C# or upstream source is edited.

Raw linked C# remains separate from the copy receiving normal semantic
postprocessing. Raw/optimized JIT/NativeAOT must each match the native transcript
and exit successfully. Receipts record original/new object identities, compiler,
postprocessor, native tools, source/header/host snapshots and execution binary
hashes before/after, plus closed command log hashes. The owned runner/probe and
canonical inputs must remain unchanged throughout. This is frontend-derived
qualification, not a fresh translation of every retained core object.

## Qualified result

On 2026-09-19 the native probe and raw/optimized JIT/NativeAOT all passed against
canonical assembly `03789a247c2d723303c55875538c4b46e1bb0d8aa298e5061c001ea3f8bf7511`.
The derived library retained 108 of its 109 objects and replaced only the
original authored frontend. Both cycles produced the same guest accounting:

| Phase | Virtual pages | Resident pages |
| --- | ---: | ---: |
| Initial two pages, zero-read | 2 | 6 |
| Grown to five pages | 5 | 10 |
| One-page unmap/remap and refill | 5 | 10 |
| All guest mappings removed, before System cleanup | 0 | 5 |

The resident pages remaining in the final row are page tables; that observation
precedes `FreeMachine`/`FreeSystem`. All ten native transcript rows matched every
managed mode. Every managed process passed the positive/budget-bounded pool
checks and exact equal retained mapping/byte counts after the two cycles.
Final worker disposal was invoked after cleanup. No post-disposal counter query
was made, so this result does not claim an observed zero pool after disposal.

Receipts, relative to `blink/`:

| Evidence | SHA256 |
| --- | --- |
| Native-only `artifacts/guest-memory/attempt-6lnwse49/receipt.json` | `7d0189286295c4efd07c7e84d47a44cb5581e25154a647bf681a951d045fe7a7` |
| Native and all four managed modes `artifacts/guest-memory/attempt-gbyg6j74/receipt.json` | `3e6289d234e4ad96bf9e26162671142648c759c5deef05ead92a57fec251cb02` |

Compiler, postprocessor, native tools, source/header/host snapshots, retained
objects and execution binary identity checks all passed. No failures or generated
source repairs occurred. This bounded valid-lifecycle result neither runs nor
reinterprets excluded historical fault cases and does not expand the permission
enforcement claim described above.
