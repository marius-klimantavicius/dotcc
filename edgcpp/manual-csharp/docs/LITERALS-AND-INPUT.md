# Character literals, predefined environment and host input

This bounded remaining-M1 extension passes 79/79 groups in direct-dotnet managed
execution and Linux-x64 NativeAOT, zero failures/skips. Release has zero
warnings/errors. Four round4 receipts retain identical 53-file maps matching
held bytes; independent bounded acceptance is `review-m1-literals-final.md` in
evidence. Canonical findings
are M1L-01..10 in the independent review reports; full M1/PLAN is not complete.

| EDG contract | Authored implementation and adaptation | Remaining bounds |
| --- | --- | --- |
| `literals.c:conv_single_char`, `conv_single_wide_char`, `conv_char_literal` | One target-aware decoder supplies language graph nodes and PP operands; distinct wchar/char8/char16/char32 types, explicit code units, strict numeric escape limits and target multichar packing | Unicode/UCN and complete encoding/string/floating/UDL semantics remain required |
| `types.c:type_after_integral_promotion`, `exprutil.c` arithmetic conversions | Distinct character identity survives deduction; target widths/ranges select promotions independently of CLR types | Other target models and complete conversion sequences remain required |
| `preproc.c` controlling expressions, `macro.c:make_pp_int_constant` | Numeric PP tokens use intmax/uintmax64; actual character and bool operands retain witnessed promoted 32-bit behavior | No blanket claim for every literal/dialect branch |
| `macro.c:scan_has_include`, expansion dispatch, `preproc.c:get_header_name` | Expansion frames request physical header mode before operand lexing; literal headers remain opaque, macro-produced headers expand tokens, ordinary macro comments retain normal phase behavior | Other special builtin operators remain required |
| `lexical.c` original-line modifications and raw-token recognition | Uniform splice maps retain physical contributors; actual raw recognition restores r-char spelling in its real context | Native PP printing is lossy here; the ordinary native literal-value witness establishes semantics |
| `macro.c:init_date_and_time_macros`, hosted predefined initialization | UTC timestamp captured once per session by default, immutable injectable override, English space-padded date; retained result exposes effective timestamp/hosted value | This explicit session policy is not a claim to reproduce ambient native process timing |
| `lexical.c:header_can_be_found`, `resolve_header`; `host_envir.c` input/error contracts | Shared deterministic search with distinct per-analysis probe/read caches, typed missing/failure and retained canonical source identity | General pragma/extension/header compatibility remains required |

Configured in-memory analysis never falls back to the host filesystem. The
opt-in filesystem provider owns immutable explicit base/allowed roots and strict
UTF-8 or BOM-marked UTF-16 decoding; UTF-32 and other encodings are explicitly
unsupported. Probe checks presence without reading/decoding content or activating
macros, include frames, pragma-once or source buffers. Pure graph queries perform
no provider I/O. Missing and typed failure stop/continue search differently.

The explicitly authorized isolated Linux-x64 filesystem adapter qualifies actual
Linux/X64/LP64/little-endian GNU libc major2 >=2.33, independently of portable or
distro RID spelling. It inspects the process GNU identity and bounded version
string before using explicit libc.so.6 bindings. Missing/unknown/musl/other ABI
is UnsupportedHost, not a passing skip. The adapter uses nonblocking
open followed by regular-file validation on that same owned descriptor. It is
host I/O only: there is no native frontend, compiler process or semantic fallback.
Its ABI/platform qualification is separate from the C++ target profile. Lexical
allowed-root checks are not a symlink security boundary; network/filesystem
latency and atomic external snapshots are not guaranteed. Byte/character bounds
do not promise wall-clock I/O cancellation. Exact ABI references, ownership,
errno/race policies and the bounded FIFO/socket child test are recorded in
`../evidence/filesystem-provider-handoff.md`.

Source provenance is `../evidence/source-m1-literals-round2.json` (16 files).
The included
`m1-character-gate.cpp/.h` path couples character tokens, distinct template
instances, selected int promotion, constexpr execution and retained macro/include
origins. Exact native fixture/witness receipts include direct char/char16_t
deduced-result assertions and values66/65/65/66. Final command/maps/artifacts are
`m1-literals-{build,tests,nativeaot-publish,nativeaot-tests}-round4.json`,
`m1-literals-held-snapshot.json` and `m1-literals-built-artifacts.json` in evidence.

Retained failed evidence is explicit: round1 compile error, round2 exact-RID
host rejection (67/79), round3 test-only final-line expectation mismatch (78/79).
Exact native source confirms line5; no production source was altered for that
expectation correction. Earlier unknown-escape rejection, character-shift
acceptance and speculative ID(__has_include) positives were disproved; those
receipts receive no pass credit. Native raw preprocessing serialization loses
spelling that its ordinary frontend value witness restores; managed spelling is
deliberately retained rather than compared to that lossy printer.
Broader M1 and every remaining PLAN milestone remain required.
