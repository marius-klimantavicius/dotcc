# Blink implementation progress

Campaign started 2026-09-14 on branch `sqlite`. The approved plan is [PLAN.md](PLAN.md).

## Current gate

P0 is complete to its native baseline gate; recorded toolchain packaging limits remain qualification work. P1–P6 have not passed. No translated Blink execution or service runner is claimed.

## Ownership

- Coordinator: source/host inventory, dotcc baseline and translation probes, integration, validation, and milestone/significant-progress commits.
- Inputs worker: checksum-pinned immutable upstream, license ledger, native interpreter and upstream corpus.
- Guest worker: static musl HTTP fixture, reproducible compiler recipe, real native service requests, and observed syscall inventory.

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

Run the existing build and regression baseline serially while workers establish native inputs. Probe the actual upstream decoder and selected core with dotcc, preserve the first failure, and adapt only explicit host seams or generic compiler defects. Record each observed limitation before attempting subsequent gates.

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
