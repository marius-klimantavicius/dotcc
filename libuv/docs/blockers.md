# libuv implementation blockers

Required incomplete work remains open. Native-only controls and object-emission
probes never establish translated execution or all-platform coverage.

| ID | Classification | Reproducer / observation | Expected behavior and next check |
| --- | --- | --- | --- |
| UV-001 | Campaign/product integration | `bash libuv/scripts/translate.sh --fetch never --tools reuse`: recipe deliberately rejects publication while aggregation/import closure is missing. | Default product must contain every backend domain and select lazily at runtime. Implement separate emitted ABI domains and shared transactional delivery; no Linux-only product may close this gate. |
| UV-002 | Source/ABI configuration | `config/platforms.json` assigns all 87 C units to 19 platform source sets, but 3,672 source/header conditional directives remain unmapped to emitted variants. | Resolve architecture/compiler/libc/OS-version branches, SDK/header provenance and measured ABI; verify full source closure independently of execution availability. |
| UV-003 | Native import fidelity | `config/native-imports.json` records observed Linux archive externs and string-named dynamic lookups; signatures, ownership, error channels and foreign static imports remain unaudited. | Every required call needs direct compatible native binding or an explicit narrowly justified boundary compromise. No default BCL runtime routing. |
| UV-004 | Runner/toolchain availability | Current environment supplies Linux glibc x64 only. Real MinGW/UCRT and Darwin target headers are now acquired via Zig and all148 source/configuration preprocess/native-syntax checks pass; no foreign execution runner is established. | Required Linux arm64/musl, Windows x64/arm64 and macOS x64/arm64 matrix rows stay unrun until real runners execute the same IL and corresponding AOT consumers. |
| UV-005 | Native source build reconciliation | CMake selects `src/unix/os390-proctitle.c`; autotools selects `src/unix/proctitle.c`, with different OS390 defines/flags. | Retain both source/configuration variants, establish which contracts each represents, and account for zoslib and SDK layout. |
| UV-006 | Runtime boundary feasibility | Signal handlers and post-fork child code have not been proven safe as managed callbacks/code. | Probe exact native bridges and their contracts before enabling these paths; record each actual compromise in `compromises.md`. |

| UV-007 | Compiler headers/parser | Initial actual source `src/version.c`, `src/strscpy.c`, `src/timer.c`, `src/unix/core.c`, `src/unix/linux.c` fail with absent `pthread_rwlock_t` in synthetic runtime headers; genuine host-preprocessed source exposes postfix GNU attributes. | Milestone `6b58b2b8` passed 83 focused unit + 5 functional tests; real-header full 35-unit retry still emits 0/35 (`artifacts/host-source-survey/result.json`). Additional structural/header issues remain open. Synthetic-header `src/strtok.c` success is separate. |

Compiler reduced reproducers, regression tests and actual full-source retries are
recorded by the compiler/native worker; later evidence supersedes initial failures.
