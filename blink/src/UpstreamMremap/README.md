# Narrow mremap source-range validation

The correction now passes source staging, compilation and the actual normal static-musl .NET stack-discovery path. No generated C# or immutable upstream source is edited. General remapping remains unsupported as described below.

Actual static-musl .NET startup reached the pinned `SysMremap` unconditional `ENOMEM` fallback after guest threading and the configured GC reservation. The musl main-thread stack-discovery loop calls flags-zero `mremap(old_page, 4096, 8192, 0)`, descending through pages until an absent source produces `EFAULT`. The previous translated diagnostic exhausted its 20-million-instruction budget without that distinction. Its receipt is `blink/artifacts/dotnet-threaded-guest-execution/attempt-2fpjx0g2/receipt.json` (SHA256 `419e2d6a7da05a65ef9692e6217f5384c477c5192238c7fb40cbd2a21f8e047b`); the result SHA256 is `37d4f889be1d5ec6008fa062875f92ce184fe6e613a4a0d88eaa551e3c00a079`. The retained main-thread trace contains 4,007 mremap rows, all `ENOMEM`, within its first 4,096 observations; its 440,030 total syscall count is not a complete retained trace.

The patch adds only source validation before the existing fallback:

- Eligible requests have flags zero, positive old/new lengths bounded by the existing 48-bit guest address space, an aligned canonical old address, and valid page-rounded old/new intervals. Bounding lengths before rounding prevents unsigned overflow; resulting sizes fit signed `i64`.
- Under the existing `BEGIN_NO_PAGE_FAULTS` / `END_NO_PAGE_FAULTS` discipline and `System.mmap_lock`, `IsFullyMapped` checks the old interval's actual guest page-table `PAGE_V` bits, including reserved pages. It does not access guest contents or fault in pages.
- The lock is released and prior no-fault state restored before returning `EFAULT` for an absent old mapping. Mapped requests and requests outside the guard retain the **existing unsupported `ENOMEM` fallback**.

This does not implement shrink, growth, relocation, mapping duplication, permission changes, or resource accounting. In particular, a mapped request's fallback `ENOMEM` is not claimed to prove a measured extension collision. There is no address special case, success stub, host memory pinning, or hardcoded .NET result. `IsFullyUnmapped` and the allocating/faulting `FindPageTableEntry` path are not used. Linux VMA boundary semantics are not replaced by a claim that guest page-table presence implements all of mremap.

The primary Linux reference is [`mm/mremap.c` at Linux v6.12](https://github.com/torvalds/linux/blob/v6.12/mm/mremap.c), specifically the initial `vma_lookup(mm, addr)` and missing-mapping `-EFAULT` result in `SYSCALL_DEFINE5(mremap)` (lines 974–978 in that revision). This reference supports the source-absence distinction; Linux's subsequent resize/move implementation is outside this patch's scope. No Linux implementation is copied into Blink.

## Immutable derivation

`stage.py` accepts the exact output of the reviewed GuestThreads adaptation **before** the host-binding preamble:

```sh
python3 blink/src/UpstreamMremap/stage.py \
  --predecessor blink/generated/threaded-core/PROFILE/upstream/guest-threads/syscall.c \
  --predecessor-receipt blink/generated/threaded-core/PROFILE/guest-threads-boundary.json \
  --output blink/generated/threaded-core/PROFILE/upstream/mremap \
  --receipt blink/generated/threaded-core/PROFILE/mremap-boundary.json
```

All output files must be fresh and confined to campaign `generated/` or `artifacts/`. The original upstream license text and all other bytes remain unchanged. The helper replays `UpstreamExecutionStop`, `UpstreamGuestRuntime`, and `UpstreamGuestThreads` in memory from the pinned original sources, checks their reviewed patches/current script identities and predecessor receipts, and verifies frozen inputs, thread header overlays, and profile constraints. It writes only the derived `syscall.c`, exact checked `mremap.patch`, and provenance receipt. It does not stage a profile, invoke a compiler, or publish a product on its own.

| Input | SHA256 |
| --- | --- |
| Pinned `blink/syscall.c` | `4eb3f54173ba37341b300e7668cc4ba3650578cc3d23d713573ffa486ae0e2c3` |
| Pinned `blink/memorymalloc.c` used to reproduce predecessor | `589becd0e214d5f422e75a9b63b1bf5d5280b3f8ca4e00dc212ede120e945b12` |
| Reviewed GuestThreads `syscall.c` predecessor | `b744c1a33c984b04f9809f534e364a1fd0535041c019b63b67a152653e2be05c` |
| Exact original `SysMremap` block | `1ac9b2d866051b6ae2d2554ab8ebdf0f5992474df744d76ea67cd62188034f97` |
| Reviewed patch | `ca1f55b0bf8b6c0a9ddc4d03ff447868d4dfe2a0d4049ad345606ee940228e03` |
| Prospective derived source, calculated during patch authoring | `5e0b3ced63c24284ec65771afefeaa5f7bc9eeec3c819f8593ff993dc2722be3` |

The upstream revision is `f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`. The receipt kind is `reviewed-mremap-source-range-validation`, with `upstream`, `stage_sha256`, `patch_sha256`, `frozen_inputs`, predecessor source/receipt identities, inherited required/forbidden defines and required headers, and `sources.syscall.c = {source_sha256, predecessor_sha256, predecessor_block_sha256, staged_sha256}`. Its qualification field remains source-only; execution evidence belongs in separate actual-run receipts.

No pinned upstream mremap test was found in the pinned `test/` tree. Qualification is therefore scoped to the existing frozen static-musl .NET service's ordinary startup and native trace, followed by the same actual translated guest diagnostic. No custom absent-address, fault-injection, exhaustion, or malformed-image tests are added. New successful remap behavior is not claimed or tested because this patch introduces none.

## Observed normal guest result

The derivative reuses107 unchanged core objects and recompiles only syscall.c.
Delivery `threaded-delivery/attempt-wwrpenct/receipt.json` has SHA256
`77c8f2c18fef1df9289c8e1a0e10add0d34fee88724d5de1d97ea2400251f030`.
The unchanged real .NET guest completes2048 stack-discovery calls:2047 ENOMEM
results then EFAULT at guest source page0x4fffff7ff000. Its trace is complete
(main2159 observations, child7), and it advances to epoll_create1, the next
unsupported boundary. Receipt `dotnet-threaded-guest-execution/attempt-n9cjykot/receipt.json`
has SHA256 `270edb094faf8b72fc9858b0bf0f86bdd2c781d198cca19a4853c7998c878a37`.
The guest aborts after epoll ENOSYS before HTTP, with all workers joined and
memory released. These observed counts reflect this guest's actual8MiB stack;
they are not forced to match the native process's differently sized stack.
This qualifies the selected source-absence distinction, not successful resizing,
relocation, all mremap errors, or complete .NET service execution.
