# Musl source audit for a BCL-backed libc

Status: source feasibility audit, 2026-10-02. No musl sources have been
translated, built, or run as acceptance tests. The implementation campaign
permits only synthetic inputs and EDG tests until the full plan is complete.

## Decision

Keep musl ahead of newlib as the upstream algorithm source. **A complete musl
port is not a small syscall replacement.** Continue the audited BCL bridge for
platform services and incrementally adopt selected musl algorithms behind one
portable libc ABI. Do not select wholesale musl adoption or claim its standard
library coverage based on this audit.

The initial bounded route is generic string routines, sorting, integer
conversion, and selected generic math algorithms. Formatting and floating
conversion are attractive next candidates, but require a coherent stream,
locale, portable-varargs and floating-point environment contract first. Keep
the existing NativeMemory allocator; do not translate musl's allocator,
dynamic loader, process startup or native pthread implementation merely to
obtain libc algorithms.

This conclusion preserves the plan's preference for musl while applying its
fallback when a full port would require extensive Linux emulation. No newlib
evaluation is needed for the next implementation milestone.

## Pinned audit source and provenance

The [official musl homepage](https://musl.libc.org/) identified 1.2.6 as the
current release at audit time. The exact source inspected was:

| Item | Value |
| --- | --- |
| Release archive | [musl-1.2.6.tar.gz](https://musl.libc.org/releases/musl-1.2.6.tar.gz) |
| SHA-256 | `d585fd3b613c66151fc3249e8ed44f77020cb5e6c1e635a616d3f9f82460512a` |
| Official Git tag | `v1.2.6` |
| Tag ref from official `git ls-remote` | `9fa28ece75d8a2191de7c5bb53bed224c5947417` |
| Git source | `https://git.musl-libc.org/git/musl` |
| COPYRIGHT SHA-256 | `b870108ec5e7790e9f9919064f1b9421d62d5f9b0e6c230c6adf7ea2da62e97b` |

The archive, detached signature, homepage snapshot, extracted source,
dependency scan and provenance JSON are retained under
[`artifacts/campaign/musl-feasibility-20261002-214747`](../artifacts/campaign/musl-feasibility-20261002-214747/).
The signature was downloaded but was **not cryptographically verified** in
this audit. The SHA-256 identifies the bytes inspected; it is not a claim of
signature verification.

The [release COPYRIGHT file](https://git.musl-libc.org/cgit/musl/tree/COPYRIGHT?h=v1.2.6)
provides an overall MIT license and identifies additional file-level notices,
including permissively licensed math code and BSD-licensed regex code. Any
subsequent vendoring must retain the relevant notices and track copied files
and patches. This audit does not vendor musl into the durable runtime.

The official homepage also identifies advisories affecting 1.2.6:
[CVE-2026-6042, iconv](https://www.openwall.com/lists/oss-security/2026/04/09/19)
and [CVE-2026-40200, 32-bit architectures](https://www.openwall.com/lists/musl/2026/04/10/3).
Before adopting affected code, pin and review the upstream fixes. The current
translated target is 64-bit, but that does not remove the iconv adoption gate.
This release is an audit baseline, not approval to ship every unpatched file.

## What the source shows

Counts below come from the downloaded archive. “Generic C” counts only
`src/<area>/*.c`, excluding architecture subdirectories. They indicate source
size, not the number of supported functions or a compatibility percentage.

| Area | Generic C files / lines | Port implications |
| --- | --- | --- |
| String | 74 / 1,575 | Many self-contained algorithms; allocation, locale and weak aliases still need their selected dependencies. |
| Math | 232 / 17,618 | Generic implementations exist, but require fixed representations, helper tables, long-double support and floating-environment semantics. |
| Stdio | 116 / 4,444 | Buffer algorithms share internal FILE layout; platform operations, locking, stream lists and formatting cannot be treated independently. |
| Stdlib | 22 / 644 | Small public entry points can hide substantial dependencies in internal scanners. |

The lexical scan in `dependency-scan.json` includes architecture-specific
`.c`, `.h`, `.s` and `.S` files. It finds syscall-token references in 15 stdio
files, 67 thread files, 72 unistd files and 24 network files. It finds
lock/atomic-token references in 34 stdio files. These are search hits, not a
complete dependency graph: macros, indirect calls and implementation-private
headers introduce further dependencies.

### Memory and strings

Generic implementations under `src/string/` are the clearest reuse candidates.
For example, `strlen.c` uses byte alignment, word scans and unsigned arithmetic;
it does not call the kernel. `memcpy`, `memmove`, `memcmp`, `strstr` and related
algorithms can be audited independently of a Linux descriptor layer.

Select generic source files explicitly. Do not pull architecture assembly
overrides into the translated project. Honor assumptions such as
`__may_alias__`, alignment and legal read extents when lowering word scans.
Keep the existing BCL memory-copy/fill/search operations where they are faster
and equally correct; upstream reuse is not a requirement to replace efficient
BCL primitives with translated loops.

Not every file in this directory is stateless: duplication functions allocate,
locale-sensitive comparisons depend on locale policy, and tokenizer state
must be instancified. Weak aliases need explicit symbol resolution in the
multi-TU linker rather than incidental source-name matching.

### Math, scanning and formatting

`src/internal/libm.h` selects long-double layout using mantissa/exponent widths
and byte order. It explicitly requests non-nearest rounding support.
`src/math/sqrt.c` combines integer arithmetic, helper data and floating
operations with floating-environment behavior. Select the generic `fp_arch.h`
contract, not host-architecture inline assembly.

Our current Float80 value representation is useful, but round-to-nearest
software operators plus BCL `Math` calls do not establish complete C `fenv`
conformance. Exception flags, dynamic rounding, subnormals, signed zero and
errno must be verified independently. NativeAOT compatibility does not imply
bitwise equivalence across processor architectures.

`src/stdlib/strtod.c` constructs an internal FILE object and calls
`__floatscan`; it is not an isolated decimal-parser wrapper. The scanner
depends on `shgetc`, locale/character rules, errno, long double and math
helpers. `src/stdio/vfprintf.c` likewise depends on FILE, portable `va_list`,
wide-character conversion, integer and floating formatting. Direct use of
`double.Parse` or `.ToString` is not a demonstrated replacement for these C
contracts.

### FILE, descriptors and buffering

`src/internal/stdio_impl.h` embeds buffer pointers, flags, fd, lock fields,
locale, linked-list pointers and read/write/seek/close function pointers.
Those function pointers provide a promising adaptation boundary.

However, several operations bypass that boundary: `fopen.c` calls `sys_open`
and `fcntl`; `__fdopen.c` performs `fcntl` and terminal `ioctl` operations;
`__stdio_read.c` issues `readv`/`read`; `__stdio_write.c` handles `writev`
partial writes. `__lockfile.c` obtains a native pthread thread id and uses
atomic operations plus futex waits/wakes. A generic replacement for the
numeric Linux `syscall` interface would turn into a Linux emulation layer.

A bounded port should instead replace those platform-facing entry points,
and adapt FILE locks and open-stream ownership explicitly. Use one FILE and
descriptor implementation for all translated libraries. Do not pass opaque
BCL-bridge handles to musl code that dereferences the native FILE layout.

### Allocator, TLS, threads, locale and startup

`src/malloc/mallocng/glue.h` depends on `brk`, mmap/mremap, page sizes, atomics,
libc process state and auxiliary-vector randomness. The existing generated
owner heap already supplies NativeMemory allocation and deterministic owner
cleanup. Reusing musl's allocator adds a second ownership domain and virtual
memory emulation without being necessary for libc semantics.

`src/errno/__errno_location.c` accesses the native pthread structure.
`src/internal/locale_impl.h` also obtains the current locale through that
structure. Replace these with owner/thread state. The existing pinned errno
cell is the appropriate storage category; malloc/new and temporary buffers
should remain native allocations.

Native pthread creation/cancellation, Linux futexes, signal delivery, TLS
descriptors, ELF loading and crt startup are not portable BCL algorithms.
Use managed threading/synchronization plus explicit generated callback and
lifetime contracts. Select unavoidable OS APIs at runtime. A C# backend
must not inherit host Linux layouts or compile-time platform selection just
because its frontend was built on Linux.

## Proposed adoption sequence and verification gates

1. Complete the current exact-signature import and owner-lifetime foundation.
   Preserve the documented portable LP64 widths and keep native ABI adapters
   separate from translated ABI signatures.
2. Implement a BCL FILE/descriptor vertical slice with synthetic binary data,
   short reads, EOF/error flags, seek/tell, append, flush, owner cleanup and
   concurrency tests. Prefer FileStream/Stream and native temporary buffers.
   Decide whether future musl formatting uses an adapted internal FILE or an
   explicit sink adapter; do not expose two incompatible FILE providers.
3. Complete portable varargs, locale and floating contracts. Use synthetic
   integer/floating parse/format vectors, including width, precision, `%n`,
   positional arguments, round trips, range failures and long double.
4. After the user's full-plan acceptance gate permits upstream translation,
   select and pin a narrow musl source manifest, including dependency closure
   and applicable advisory patches. Begin with generic algorithms; retain the
   BCL platform layer and shared native allocator.
5. Measure unchanged source count, patch size, unresolved symbols, generated
   code size and runtime performance. Compare equivalent synthetic contracts
   against native oracles and test JIT/NativeAOT on Windows, Linux and macOS,
   including two owners and multiple threads.

Only after these measurements should the plan choose a larger musl port.
The source audit supports selective reuse and identifies concrete adaptation
boundaries; it does not demonstrate a complete translated musl libc or prove
that the full port is cheaper than the audited BCL bridge.
