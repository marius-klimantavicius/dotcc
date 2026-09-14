# Blink implementation progress

Campaign started 2026-09-14 on branch `sqlite`. The approved plan is [PLAN.md](PLAN.md).

## Current gate

P0 is complete to its native baseline gate; recorded toolchain packaging limits remain qualification work. P1–P6 have not passed. No translated Blink execution or service runner is claimed.

## Ownership

- Coordinator: source/host inventory, dotcc baseline and translation probes, integration, validation, and milestone/significant-progress commits.
- Inputs worker: ordinary and signal-aware nonlocal jumps, host storage ABI qualification (after completing source/native baseline).
- Guest worker: actual interpreter closure translation, measured host header contracts and reduced compiler blockers (after completing native guest fixture).

Workers share one worktree with disjoint authored-file ownership. Shared compiler edits and repository suites are serialized by the coordinator. Generated/ref/build/artifacts content is disposable and ignored; reproducible scripts and durable summaries are committed.

## Milestones

| Milestone | State | Evidence / remaining work |
| --- | --- | --- |
| P0 | Passed | Immutable sources verified offline; native Blink and 25 assembly cases pass; six HTTP cases pass on Linux and Blink; exact native archive/import/global audit and initial translation failures recorded. |
| P1 | Pending | Needs actual upstream instruction execution through managed embedding, ABI, faults, JIT/AOT. |
| P2 | Pending | Complete selected closure and rooted raw/optimized libraries. |
| P3 | Pending | CPU/memory/ELF behavior corpus. |
| P4 | Pending | Real host contracts and service startup. |
| P5 | Pending | Worker/controller lifecycle and two-instance HTTP qualification. |
| P6 | Pending | Faults, Linux/Windows runtime matrix, reproduction and regression qualification. |

## Observed environment

.NET SDK 10.0.111 is available. Baseline build disables automatic sibling LALR.CC substitution using `-p:UseLocalLalrCc=false`. Test TMPDIR is isolated to `blink/artifacts/tmp`.

## Next actions

Continue actual core translation with individually recorded units and measured host declarations. Qualify the signal-aware unwind adapter, then integrate host callbacks into the bounded interpreter seam. Private filesystem/TCP modules need C marshalling and common descriptor ownership before a guest-service gate can run.

## Observed validation (initial campaign baseline)

- Release solution build passed with `UseLocalLalrCc=false`.
- Unit tests: 2218 passed, zero failed.
- Functional tests: 490 passed, 1009 skipped, zero failed. Skips are preserved, not counted as executed passes.
- Native Blink interpreter: 25 selected upstream assembly cases agree with direct Linux execution.
- Static musl service: six exact-wire HTTP cases, including 128 KiB response and fragmented requests, pass direct Linux and Blink; clean exit.
- Full source/checksum verification passes offline. Host distribution toolchain packages are not yet archived.
- First actual translation blocker (comma-separated bit-fields) reduced and repaired generically; five focused cases pass. Full post-fix regression run is active. Next decoder blocker is multidimensional array parameters.

Receipts are under `artifacts/`; durable inputs and interpretations are in SOURCES.md, GUEST.md, HOST-CONTRACT.md and BLOCKERS.md.

## P0 exit evidence

Native link audit observes 147 archive members, 89 extracted by the CLI, 185 dynamic imports and 151 writable state candidates. `DEPENDENCIES.md` separates this native CLI closure from the future managed embedding; it does not infer isolation from compile flags. The disabled-thread profile makes `g_machine` and `g_siginfo` ordinary process globals. P0 is complete; P1 is active with a real native bounded interpreter seam under investigation and managed decoder/compiler blockers still open.

## Significant progress: generic bit-field declaration fix

Post-fix Release build passed; 2218 unit tests passed; 491 functional tests passed with 1011 explicitly skipped. The new native-checked fixture verifies adjacent byte-width fields, anonymous padding, signed values, compound initialization and actual storage bytes. P0 baseline committed as `5f6ced0`.

## P1 decoder progress

The actual upstream decoder now emits, compiles and executes in raw/optimized JIT/NativeAOT on Linux x64. All six native/staged/raw/optimized output files have identical SHA-256, checking 11 instruction encodings and actual bit-field/operand/decoded-record layout. `scripts/probe-decoder.sh` reproduces this and records compiler/config/source/generated hashes. This is decoding, not instruction execution, so P1 remains open. Native bounded core probe separately exercises actual ExecuteInstruction with guest page tables, normal arithmetic/memory, invalid instruction, unmapped access and budget exhaustion; managed core port remains blocked.

## Significant progress: array declarators and tagged function returns

Generic named/unnamed array parameter support now retains inner row dimensions and discards only the outer bound; `sizeof(*parameter)`, pointer arithmetic, indexing and writes match native C. Function specifiers before struct/union/enum return tags preserve inline flags and qualifiers. Three new reduced native-checked fixtures pass. Full Release build passes, with 2218 unit tests and 494 functional tests passed; 1017 functional rows explicitly skipped. Actual upstream decoder/core retries clear both defects.

Decoder qualification rerun after the final parser rebuild: raw/optimized JIT/NativeAOT again match the untouched native and staged-native output exactly. Current compiler/config/generated identities are captured in the refreshed decoder receipt. The core profile now has its own explicit exclusions; its host header profile remains declarations-only and cannot satisfy runtime gates.

## Significant progress: native embedding and explicit host storage

Native actual-ExecuteInstruction harness passes eight rows (four cases repeated with create/destroy), with an observed 83-source archive extraction. The core has an independent feature profile and hash-verified staged inputs; pureconst is removed only as a required-match optimization hint, with ABI attributes retained. Host declarations isolate 53 operations, with 87 native layout/offset checks, 252 constant checks and seven unresolved-symbol checks passed. Host declarations remain unimplemented and cannot enter runtime gates. Native host storage baseline committed as `be2d56e`; emitted storage comparison is in progress.

The first long full-core retry was invalidated by editing the running shell script, which caused a duplicated invocation. The duplicate was terminated and this run is recorded as interrupted, not as a clean compiler timeout or pass. Subsequent diagnosis uses immutable per-attempt scripts/profile snapshots and bounded translation-unit isolation.

## P4 feasibility work while core translation proceeds

Implemented a private memory-backed filesystem host module with typed error results, immutable cloned images, bounded writable storage/descriptors, path lookup, shared-cursor dup/close, sparse zero-fill, short writes, seek, append and cleanup. A separate consumer passes four assertion groups under Linux JIT and NativeAOT, including two-instance isolation and resource failure atomicity. The C callback/guest descriptor/socket integration is not implemented, so P4 remains pending.

Host storage ABI qualification also now passes all 174 emitted measurements in raw/optimized JIT/NativeAOT against native authored storage, including actual field-address offsets, aggregate placement and array strides. This is storage compatibility, not implementation of the 53 declared host operations.

## Significant progress: safe unmanaged jumps and C declarations

Ordinary C jump buffers now store a numeric identity instead of a CLR reference in unmanaged memory. Lowered handlers capture a freshly armed identity and evaluate buffer expressions once. Allocated, nested, rearmed, zero-value and compacting-GC cases pass; the independent jump consumer agrees with native output under raw/optimized JIT/NativeAOT, with managed-pointer warnings treated as errors. Signal-aware mask saving remains separate work.

Function-form parameters now adjust to function pointers. Global array typedefs retain full storage, including multidimensional arrays; thread-local arrays have per-thread pinned managed roots. Reduced direct/object-linked tests cover two simultaneous threads and compacting GC. Full Release regression after all three compiler repairs passes 2222 unit tests and 500 functional tests, with 1025 functional rows explicitly skipped and zero failures.

## Significant progress: isolated TCP host

The managed TCP module now uses real sockets with per-instance virtual bindings
and explicit physical loopback publication. Independent instances reuse guest
port 8080, exchange exact fragmented/128 KiB traffic through separate clients,
and retain separate endpoint metadata and descriptor limits. Actual socket
backpressure, cancellation, close during receive and disposal of pending accept
operations pass bounded tests. Four assertion groups pass Linux JIT/NativeAOT;
both host suites now record source, assembly, executable and output hashes.
This remains a typed host module without guest callbacks or a unified guest
descriptor table, so P4 and P5 remain open.
