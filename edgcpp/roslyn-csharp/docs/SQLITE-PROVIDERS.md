# SQLite provider preparation

The accepted inventory has 103 required external requirements: 88 system functions and
15 recorded compiler intrinsics. This preparation preserves every captured
identity and selected signature. It enables no production binding and proves no
provider calling, error, lifetime or thread behavior.

`acceptance/sqlite/prepare-providers.py` resolves candidate exports from exact
Linux x64 libc/libm paths, records the actual `dladdr` image, hashes provider
files and retains the public selected ABI descriptions from the accepted audit.
Its successful measured attempt is
`/mnt/storage/roslynobj-csharp-20261015/t3/provider-preparation-3`; report SHA256
`82980f8840bac35976d06d79daf4e733a945ee5dc774b656551677c5b2626060`.
The unchanged report is committed at `acceptance/sqlite/provider-plan.json`.
All 88 system exports resolve on glibc 2.39. The 15 builtins are implementation
requirements, with no attempted native symbol lookup.

| Actual file | SHA256 |
| --- | --- |
| `/usr/lib/x86_64-linux-gnu/libc.so.6` | `3a15d66867d83762c7f2f1e37359cb8f6c5743edb369c65285cb0b1c4f7498bf` |
| `/usr/lib/x86_64-linux-gnu/libm.so.6` | `fce00b6f25f459cf4ae0b7fae4257909a1a8a86f9149e7c029a5d617baf1ccd0` |

`time` and `gettimeofday` actually resolve to kernel `linux-vdso.so.1` images.
The report records these non-file images and the kernel release as ordinary
Linux host dependencies of the hash-pinned libc provider. Their absence of a
backing file adds no separate admission requirement; actual time/error behavior
still needs testing. The first
attempt assumed every resolved image was a file and failed on this observation;
its measured failure remains under `t3/provider-preparation-1-run`.

The exact nine captured names below require explicit provider aliases. These
identities remain unchanged in program assembly. The pinned producer recipe
does not define `__USER_LABEL_PREFIX__`; the backend records EDG's actual asm
name. This finding does not authorize re-exporting or changing the input.

| Captured identity | Candidate host entry |
| --- | --- |
| `__USER_LABEL_PREFIX__open64` | `open64` |
| `__USER_LABEL_PREFIX__stat64` | `stat64` |
| `__USER_LABEL_PREFIX__fstat64` | `fstat64` |
| `__USER_LABEL_PREFIX__ftruncate64` | `ftruncate64` |
| `__USER_LABEL_PREFIX__fcntl64` | `fcntl64` |
| `__USER_LABEL_PREFIX__pread64` | `pread64` |
| `__USER_LABEL_PREFIX__pwrite64` | `pwrite64` |
| `__USER_LABEL_PREFIX__mmap64` | `mmap64` |
| `__USER_LABEL_PREFIX__lstat64` | `lstat64` |

The report separates proposed contracts for every function: exact buffer and
allocation ownership, descriptor/mapping lifetime, native return values and
partial I/O, pthread return codes, borrowed static/TLS storage, callback entry
and canonical address behavior. These declarations still need execution proof
from generated code. Native layout probes must cover the captured structures
and selected variadic call shapes before admission.

In particular, direct P/Invoke followed by a later `__errno_location` call does
not establish preservation of the preceding native error across managed
transitions. Native error capture must occur before that transition, with an
explicit tested lifetime and callback/reentry protocol. `dlerror` has its own
per-thread read/clear and borrowed-text protocol. HostContracts preparation will
measure baseline and experimental protection separately, including allocation,
nested native/managed callbacks and independent foreign pthread state. A benign
baseline result cannot establish a general preservation guarantee.

No generated SQLite emission or SQL execution has occurred. T2's aggregate
boundary gate remains open; this provider work prepares later T3 implementation.
