# Observed native dependencies and embedding work

`scripts/audit-native-dependencies.py` audits the configured native interpreter
from `scripts/native-oracle.sh`. It relinks existing native objects with a GNU ld
map and requires the resulting executable to be byte-identical to the tested
native binary. It does not recompile Blink or run a shared compiler build.

```sh
python3 blink/scripts/audit-native-dependencies.py
```

The tracked [native closure](../config/native-closure.json) records source and
object hashes, each extraction's requesting object and triggering symbol,
unresolved object symbols, dynamic imports, and state candidates. Full GNU ld,
`nm`, and `readelf` evidence is under `artifacts/dependencies/`. The native binary
SHA-256 is `fac5c0d9db337243d2cb90579d22cef25116122414aaf21df010f3b013bc6e7b`.

## Closure and its limits

| Evidence | Observed count |
| --- | ---: |
| Members in `blink.a` | 147 |
| Members extracted by the native CLI link | 89 |
| Separate CLI entry object, `blink.o` | 1 |
| Archive members not extracted | 58 |
| Dynamic undefined imports, including weak native startup symbols | 185 |
| Distinct unresolved symbols in extracted objects and CLI entry | 185 |
| Writable storage symbol candidates | 151 |
| Relocation-readonly pointer/table candidates, kept separate | 28 |

The two symbol counts happen to match; their sets differ. `environ`, `atexit`,
`_GLOBAL_OFFSET_TABLE_`, `__popcountdi2`, `__divmodti4`, and `__udivmodti4` occur
at the object boundary but are handled through copied data, native runtime, or
linker/compiler support. Conversely, native executable startup adds imports.
Both sets remain in the manifest rather than treating dynamic imports as a
complete C host-call inventory.

Archive extraction proves that an object is linked; it does not prove every
function in it executes for the service, or that the whole object belongs in the
managed product. For example, disabled feature translation units can still
provide rejection handlers. Conversely, native linker extraction can hide source
dependencies that whole-library managed generation will expose.

The manifest explicitly excludes CLI entry/helpers (`blink`, `commandv`,
`getopt`, `prog`, `startdir`) and TUI/device files from proposed embedding input
candidates. Five of these exclusions occur in the actual native closure. There
are 85 remaining candidate sources, including 18 diagnostic/trace files marked
for separate review. These are **not a completed managed source closure**.
Removing CLI helpers requires adapting references; for example `debug.o` still
references `g_progname` from `prog.o`. `cp437.o` is included by `strace.o` for
formatting, so its character table is classified with optional diagnostics even
though the TUI also uses it. `blinkenlights.c` is never linked into this oracle.

## Native host boundary

| Import category | Count | Managed action |
| --- | ---: | --- |
| Filesystem and descriptors | 50 | Route guest-visible operations through instance filesystem/descriptor policy; separate internal diagnostics |
| Identity, environment, resource and scheduler | 29 | Virtualize required guest values and limits; reject unqualified mutations |
| libc algorithms, locale and errors | 26 | Reuse or extend generic managed libc where semantics match; preserve guest error conversion |
| Network | 14 | Typed BCL socket host operations and guest/native address conversion |
| Terminal and device | 13 | Qualify required ioctl behavior; explicitly reject unsupported terminal/device operations |
| Clocks and timers | 11 | Instance host clocks, deadlines, deterministic test substitutions |
| Process lifecycle | 10 | Convert guest stop/fault/exit into instance results; remove native subprocess behavior |
| Signals | 10 | Separate guest signal semantics from native process signal installation |
| Memory | 9 | BCL allocation and guest page translation; no direct native mappings |
| Native startup/toolchain | 7 | Eliminate from product imports; qualify generated runtime startup independently |
| Nonlocal control flow | 4 | Prove synchronous unwind and cleanup; do not fake host signal-mask capture |
| Readiness | 1 | Cancellable instance readiness contract |
| Raw host syscall | 1 | Replace reviewed call site with the appropriate typed host service |

These are native ABI imports, not the service's guest syscall inventory. The
service's actual native Blink syscall trace is separately preserved under
`artifacts/guest/native-blink/`. Full closure imports include functionality that
the six service requests never use.

Concrete link evidence identifies several embedding hazards:

- `fork` and `execv` are still imported solely by `demangle.o`, which launches a
  host symbol-demangling tool. `--disable-fork` disables guest fork support; it
  does not remove this diagnostic subprocess path. The managed profile must
  remove or replace that diagnostic behavior.
- `exit` is referenced by `biosrom.o`, `blink.o`, `errfd.o`, `loader.o`,
  `memorymalloc.o`, and `syscall.o`. Configuring the interpreter is insufficient
  to make guest termination or load failure safe for an owning library.
- `siglongjmp` is referenced by `blink.o` and `throw.o`; `__sigsetjmp` is
  referenced by `machine.o`. `longjmp` and `_setjmp` also occur. These need an
  explicit embedding unwind contract, with pending signal and resource cleanup.
- `mmap64` remains referenced by `loader.o` and `map.o`. `NOLINEAR` disables
  direct guest-address mapping, but native allocation/loading still needs a
  host memory implementation.
- The only unresolved `syscall` reference comes from `random.o`; inspection of
  the configured source identifies `syscall(SYS_getrandom, ...)` in `GetRandom`.
  This belongs behind the managed randomness callback, not a generic native
  syscall escape.

## Process state and thread-local behavior

The writable symbol inventory uses ELF `OBJECT`/`TLS` symbols in sections with
the write flag. It records local static storage as well as external globals,
with object ownership, size, and section. `.data.rel.ro` and
`.data.rel.ro.local` tables are reported separately: ELF relocation storage
alone is not evidence that application code mutates a table. This is a candidate
inventory, not a completed source-write or initialization-order audit.

With guest threads disabled, upstream `blink/thread.h` defines `_Thread_local`
to nothing. The actual objects contain **no TLS symbols**: `g_machine` and
`g_siginfo` are ordinary process-global storage in `machine.o`. This strengthens
the initial one-context-per-worker requirement; thread-local isolation cannot be
assumed from the source declaration.

The next ownership audit must address at least:

| Owner | Observed state examples |
| --- | --- |
| `machine.o` | `g_machine`, `g_siginfo` |
| `memorymalloc.o` | `g_allocator`, `g_hostpages`, `g_bssmachine` |
| `bus.o` | `g_bus` |
| `flag.o`, `syscall.o` | `FLAG_*`, `g_blink_path` |
| `overlays.o`, `startdir.o` | `g_overlays`, `g_startdir`, once initialization |
| `log.o`, `strace.o`, `debug.o` | `g_log`, formatting buffers, failure/reentry state |
| `abort.o`, `assert.o` | `g_aborthooks`, recursion guards |
| `demangle.o` | `g_cxxfilt` and formatting buffer |
| `rdrand.o`, `crc32.o` | `g_rdrand`, lazy CRC table and once state |
| `stats.o` | Process-wide counters, including counters for disabled execution paths |

Repeated create/run/destroy must establish which state resets, which immutable
tables initialize once, and which diagnostics disappear. Multiple workers can
separate this state by process; the current evidence does not qualify multiple
simultaneous emulators inside one managed process.

## Actual managed closure and initializer inventory

The frozen109-source format2 profile attempt-gjk_ktle and object link
`e7c33225f0f91a793410b9483f597d230368f6a4ded5f8628e0e599f1fffe43d` build and
execute in all four Linux x64 forms (core-execution/attempt-ny3j_02m). Exact
C/header/bridge/Host inputs and compiler identity are in their receipts.
`config/host-bindings.json` owns the selected host bindings; its headers select
private operations or explicit unsupported-feature contracts. CLI/TUI/native
JIT and demangler subprocess code are excluded. Native Blink is an oracle only.

`tools/BoundaryAudit` inventories every directly declared BlinkCore method and
all type/module initializers in the library and Host assembly without executing
target code. Its self-check-zpqg3xl7 covers root/module/Host imports, state-machine
expansion, malformed IL, budgets and missing dependencies. The raw/optimized
actual-library inventories contain5594/5584 method definitions and zero
traversed native imports. The generic embedded runtime separately declares17
native methods, including POSIX/Windows identity/filesystem/accounting helpers;
none is reached by the inspected direct graph. Unused generic Environment.Exit
helpers also remain in the rooted library, while selected exit bindings throw
instance termination results. Presence and reachability are distinct claims.

The64 direct generic libc entries cover allocation, byte/string/format operations,
math, byte order, calendar conversion and synchronous nonlocal jumps. Allocation
uses NativeMemory/GC pinning, not executable mappings. Host socket callbacks use
BCL Socket APIs. The generic allocator reads DOTCC_DEBUG_HEAP and
DOTCC_DEBUG_HEAP_SCAN at initialization; these are controller debugging switches,
not guest environment reads. Diagnostic printf/perror use Console; the owning
worker must capture/bound these separately from guest descriptor output.

The155 calli sites include instruction/function tables, descriptor operations,
private host callbacks, jump/exit callbacks and libc qsort comparators. Their
concrete C/header/bridge owners are preserved in the frozen input ledger. The
inventory does not resolve their dynamic targets, virtual overrides, ordinary
delegates, arbitrary schedulers or reflection, and does not recursively audit
BCL assemblies. These remain runtime/ownership qualification obligations for
P4/P6. Completed direct inventory is not a hardened isolation claim.
