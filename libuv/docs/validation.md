# Implementation evidence — 2026-09-29

P0/P1 are in progress. No default translated product, managed consumer workflow,
all-platform source closure or required execution matrix is qualified.

Repository baseline: `cbbef56d7bf195824c3e7027113729f086347ec7`. At implementation
start `libuv/` and unrelated `lwip/` were untracked. The separate lwIP session's
files and later shared changes are not libuv baseline regressions.

Validated checkpoints: campaign `d1b364c0`, native binding `a8613b10`, shared compiler
`6b58b2b8` / `65de432a`, actual source closures `cbb4b71c`, foreign headers `ff35c497`.

Available host: Linux x86_64, kernel `6.8.0-138-generic`, .NET SDK `10.0.111`,
.NET runtime `10.0.11`, GCC `13.3.0`, CMake `3.28.3`. Native controls use the pinned
reference recorded in `config/sources.json`, not installed native libuv.

| Check | Result and evidence |
| --- | --- |
| Source acquisition | Shared `python3 Scripts/campaign-reference.py libuv` acquired pinned source; observed archive SHA-256 is now recorded in `config/sources.json`. `--fetch never` works with retained inputs. |
| Layout | `python3 Scripts/campaign.py layout libuv --write` and `--check` pass after authored consumer creation. Consumer is explicitly incomplete, not a libuv replacement. |
| Inventories | `python3 libuv/scripts/inventory.py` freezes 118 product C/H files, 87 C units, 3,672 textual conditional directives, 318 `UV_EXTERN` API declarations, handle/request macro maps and all test files. There are 530 textual test/helper definitions (including `TEST_FS_IMPL`) and 514 textual registrations. Platform preprocessing controls actual case applicability. |
| Platform source assignment | 19 source sets in `config/platforms.json` cover all 87 C units; compiler/architecture/SDK/OS-version branch variants and ABI readiness remain unresolved. |
| Native first workflow | **Passed**; `artifacts/campaign/20260929-073101-2d1c5464/receipt.json`. Binary two-frame TCP echo: 17 bytes; asynchronous temporary file write/read/unlink: 17 bytes; localhost resolver; worker result 5050; cross-thread async callback; bounded timer; 2 writes, 5 close callbacks, `UV_EBUSY` before drain and successful loop close after drain. Assertions enabled in Release. |
| Native Linux ABI | **Measured, comparison pending**; same receipt's `native-layout.json`: 154 sizes/alignments/offsets/constants, every public handle/request family, pthread/synchronization objects, stat/dirent/sockaddr/addrinfo/timespec/epoll_event and selected accessed fields. This is not emitted/native equivalence. |
| Full native upstream runner | **501 passed, 0 failed, 6 skipped**, all 507 runtime-listed cases accounted for. `artifacts/campaign/20260929-073004-199b85db/receipt.json`, `native-upstream-results.json` and `native-upstream-run.log`. Test inputs were ordinary private copies. After strengthening the suite, the existing transcript and receipt were revalidated: native process exit 0, unique complete TAP sequence 1–507, and exact listed/tested name equality. |
| Native import candidates | `python3 libuv/scripts/import-inventory.py`: 227 Linux glibc x64 archive externs after subtracting archive definitions, plus 26 direct string-named dynamic lookups across all sources. No unresolved native `uv_*` imports. This is an unaudited candidate inventory, not a complete binding contract or foreign import closure. |
| Initial actual-source probes | Worker reports original synthetic-header path emits `src/strtok.c`; version/strscpy/timer/Unix core/Linux fail at `uv/unix.h:132` because `pthread_rwlock_t` is absent (pwd/semaphore headers also incomplete). Genuine host preprocessing exposed postfix GNU attribute parser handling; focused compiler repair/testing is active. Initial logs: `artifacts/compiler-probes/initial-source-survey.json` and `native-header-source-survey.json`. Compiler milestone `6b58b2b8` passed 83 focused unit tests and 5 functional native tests. Its 35-unit genuine-header retry still emits **0/35** (`artifacts/host-source-survey/result.json`); next blockers remain visible, so source translation is not claimed. |
| Native identity/binding foundation | **Passed JIT and NativeAOT on Linux glibc x64**; `artifacts/campaign/20260929-074236-a382d209/receipt.json`. Frozen actual process provider, lazy/concurrent module loading, optional/required absent module/symbol behavior, native memory across GC, getpid, close/errno, and actual `LD_PRELOAD` mixed-provider rejection. Authored-only; no translated backend dispatch qualification. See [native foundation](native-foundation.md). |
| Foreign source/header feasibility | **430/430 actual source configurations preprocess and pass native target syntax** across 12 Windows/Darwin/Linux/BSD ABI profiles, including x86 ILP32, with native AST/layout receipts at `artifacts/campaign/20260929-075954-073ef186`. Whole-header DotCC attempts remain 0/12; exact native dependency closures of actual version.c emit 4/4. See [foreign headers](foreign-headers.md); no foreign runtime or complete backend claim. |
| Exact native declaration closures | Milestone `cbb4b71c` validates actual common source bodies plus the translated upstream timer algorithm with 116 emitted/native layout measurements and raw/processed × JIT/AOT behavior. This progress is distinct from the retained failing whole-header survey. See [compiler/native evidence](compiler-native.md). |
| Managed native-boundary probes | Separate compiler/native worker records results in `artifacts/native-boundary/result.json`. These are authored boundary / small actual-emission feasibility probes, not translated libuv consumer success. |

Native upstream skips are preserved individually: recursive filesystem directory
watching unsupported on Linux; `poll_close_doesnt_corrupt_stack`, `poll_closesocket`
and `spawn_quoted_path` are Windows-only; `spawn_setuid_setgid` requires root;
`tty` could not open `/dev/tty`. No translated case exclusions are inferred.

Reproduction of completed campaign checks:

```bash
bash libuv/scripts/fetch.sh --fetch never
bash Scripts/campaign.sh layout libuv --check
bash libuv/scripts/test.sh --suite inventory --suite native-baseline --jobs 4 --fetch never
bash libuv/scripts/test.sh --suite native-upstream --jobs 4 --fetch never
python3 libuv/scripts/import-inventory.py
```

`inventory --check` treats only provenance hash changes according to `--hashes`
(`warn` default, `off`, or explicit `strict`). Structural closure changes require
regenerating and reviewing the inventory. Ordinary translation/probing does not
reject an edited usable reference based on a historical hash. Required suites
without implementation fail explicitly; native suites cannot qualify the product.

Remaining required work includes all-platform emitted source/branch/API/import
closure, ABI equivalence, native-call fidelity, all public behavior, owning
consumer, seven-target raw/processed × JIT/AOT execution, and reproduction with
final tools. See `blockers.md` and the phase gates in `PLAN.md`.
