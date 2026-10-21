# Independent final review — bounded M1 literals, builtins and input

Reviewer: `/root/manual_edg_supervisor/review_m3`, 2026-10-21.
**Accepted for the implemented and executed bounds below.** No known unresolved
critical/high finding remains in this reviewed checkpoint. This does not
complete M1 or the campaign; all remaining PLAN/inventory work continues.

The reviewer independently read produced source/tests, actual EDG routines,
the filesystem implementation/handoff and executed receipts. The reviewer
performed no builds/tests, native-oracle/compiler executions, Git operations
or production edits. Both shared writers were held for final validation. The
separate reference worktree is unintegrated and is not accepted by this report.

## Executed evidence and identity

- `m1-literals-build-round4.json`: Release succeeds, zero warnings/errors.
- `m1-literals-tests-round4.json`: direct dotnet-hosted DLL, **79/79 groups,
  zero failures/skips**.
- `m1-literals-nativeaot-publish-round4.json` and
  `m1-literals-nativeaot-tests-round4.json`: Linux x64 publication and execution
  succeed; **79/79 groups, zero failures/skips**.
- All four recorded **53-file maps are identical**, and every entry was
  independently compared with the actual held bytes. The three output sizes
  and SHA256 values in `m1-literals-built-artifacts.json` independently match
  disk. `m1-literals-held-snapshot.json` records the holds and actual Linux
  x86-64/glibc2.39/unprivileged host. The runtime guard's other glibc versions
  have predicate controls, not executed platform qualification.
- The exact `m1-character-gate.cpp/.h` native fixture and included witness both
  accept with status0 under strict C++20 and explicit used instantiation.
  Recorded source/header SHA256 values match the actual managed fixture bytes.
  The witness distinguishes `id(CHAR_VALUE)` as char and `id(TARGET_CHAR)` as
  char16_t, and checks values66/65/65/66. The compiler SHA256 is the retained
  pinned `e669059e2068f623052ad8a21c146e776ce7cfb331fa849795208e3bcfdf07c3`.
- All16 upstream/doc hashes in `source-m1-literals-round2.json` were checked
  against the consulted files. Separate filesystem ABI/runtime references and
  installed-header hashes are retained in `filesystem-provider-handoff.md`
  and the round2 reviewer report.

The source-text managed gate inspects actual character nodes/types/code units,
conversion ranks and selected functions, distinct template argument identities,
computed constexpr facts and retained macro/include/body-token origins after
session disposal. It exercises the new subsystems together. The earlier
changed-stream difficult M4 gate, rollback/lifetime/session tests and complete
retained32-label upstream VAOPT matrix also pass in these79 groups.

## Canonical finding closure

Earlier round reports describe in-flight code and provisional assumptions.
The following IDs are unique and canonical; no old ID is reassigned.

| ID | Reviewed defect | Final bounded closure |
| --- | --- | --- |
| M1L-01 | Literal/macro-produced header confusion; generic comments consumed filename data; first closing greater-than bounds | Lazy controlling operands let the actual expander request physical header scanning after builtin '(' and exhausted replacement frames. No alias-name prediction remains. Direct/alias opaque headers, generated-token expansion, pasted builtin, closed/unclosed-looking comment/slash filenames, ordinary stringization, newline/extra-token rejection and configured search execute. |
| M1L-02 | Non-ASCII unknown escapes silently used CLR code units | Both escaped and unescaped non-ASCII character paths explicitly diagnose Unsupported. The U-prefixed escaped discriminator executes; full encoding is not claimed. |
| M1L-03 | Character ud-suffix split into a macro-eligible identifier | Adjacent suffix remains one preprocessing token. `_tag` protection and active Unsupported semantics execute. |
| M1L-04 | Character/bool PP operands incorrectly widened like numeric literals | Representation retains witnessed target32 promotions/unsignedness for characters and bool, while numeric tokens remain intmax/uintmax64. Prefix comparisons, unsigned32 wrap and char/true/comparison shift32 rejection execute. Actual graph and PP tests also discriminate signed first-entity multichar packing. |
| M1L-05 | Warnings invalidated template prototype/replay | ErrorCount determines validity/completion; diagnostic Count still supports truncation. Warning-only template source succeeds and its concrete instance is Complete. |
| M1L-06 | Directory candidate failure stopped normal include fallback | Directory Probe/Read is Missing; a later configured regular header is found. Both presence and actual include/constant2 graph controls execute, consistent with native witnesses. |
| M1L-07 | FIFO could block before a post-open seekability test | Authorized isolated Linux filesystem adapter uses nonblocking CLOEXEC open, fstat on the same owning SafeFileHandle, regular-file validation and same-handle FileStream without pathname reopen. Error/size/decoder paths dispose correctly. Bounded child with FIFO/no writer, FIFO symlink, socket and normal/loop symlinks passes under direct dotnet and NativeAOT. |
| M1L-08 | Prepass comment state corrupted a later raw token after an opaque header | Uniform phase2 splice maps precede actual-context raw recognition/restoration. Post-header raw spelling, physical contributors and retained/common-buffer ranges execute. Native ordinary frontend sizeof/index witness proves restoration semantics; its PP printer is lossy and is not an exact-spelling oracle. |
| M1L-09 | #line bias used the last significant token before multiline trailing trivia | Lexer records the logical line's actual terminating newline; directive bias maps that endpoint physically. Exact trailing-comment control produces100 and existing multiline-comment grouping remains passing. |
| M1L-10 | Exact linux-x64 RID rejected the intended distro runtime | Host qualification checks Linux/X64/LP64/little-endian, GNU identity in the running symbol namespace and bounded glibc-major2/version>=2.33; operations bind the same libc.so.6 ABI. Malformed/old/future-major/missing-identity/wrong-platform predicates reject. Actual glibc2.39 filesystem paths and child now execute successfully. |

## Preserved distinctions and limits

Character conversion follows inspected `conv_single_char`/`conv_char_literal`
branches: target masks, signed first-entity packing, malformed/range/count
diagnostics and distinct ordinary/wide/UTF character types. Unknown ASCII
escape `\q` is a warning with value q in the pinned native profile, not an
invented invalid-source error. Promotions use target representability, including
char32_t→unsigned int, and do not alias these types to CLR character types.

Date/time use the authorized actual UTC timestamp captured once per session,
with immutable explicit override and retained effective configuration. This
supersedes the historical UnixEpoch-default suggestion. English month/space
padding, offset normalization, hosted mode, reserved names and independent
session values execute. It is an explicit session policy, not exact replication
of an ambient native process clock.

Configured includes have distinct per-analysis probe/read caches, typed
Missing/Failure, ordered explicit roots and returned canonical source identity.
Stat-only probes do not decode, activate macros/once/#line, create sources or
include frames. Native EDG can open/check BOM during resolution; the authorized
managed presence contract is deliberately metadata-only. Strict UTF-8 and
BOM-selected UTF-16 decoding, malformed/unsupported encodings, byte/UTF-16-unit
limits, access failures, stable per-analysis results, concurrency and retained
query-time no-IO controls execute. No implicit host fallback exists.

The optional libc binding is filesystem IO only, not native frontend/compiler
fallback. Its stat layout/flags/errno and SafeHandle ownership were independently
checked. Other OS/architectures/libcs remain unsupported/unverified. Lexical
roots are not a symlink security boundary; host files are not atomic snapshots;
nonblocking FIFO open does not guarantee wall-clock cancellation of normal or
network filesystem operations. General encoding/Unicode/UCN, full literal and
special-operator repertoire, realistic headers, broader targets and M2–M6 remain
required inventory work.

## Historical failures are excluded from final pass claims

Round1 failed compilation before tests. Round2 records67/79 with12 filesystem
UnsupportedHost failures. Round3 records78/79: its last failure expected line6
for a five-line source; native preprocessing confirms `7 5`, and only that
expectation/handoff changed before the final passing run. This was not another
implementation finding.

Retained exploratory native mismatches are not positive controls: unknown
escape rejection was disproved; character shift32 acceptance was disproved;
HI=ID(__has_include) rejects, and ID caller-tail ended with catastrophic
status4. The pasted-name header case is instead a verified positive control.
Managed raw spelling is richer than EDG's preprocessing printer; ordinary
frontend value witnesses establish the native raw semantics.

Documentation reconciliation must use the final79/53 receipts and the bounded
scope above, retaining all open inventory rows. The complete PLAN continues
immediately after this accepted checkpoint.
