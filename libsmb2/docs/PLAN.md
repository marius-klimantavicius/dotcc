# Translate libsmb2 to C# with dotcc

Status: **Implementation authorized and in progress.** P0's source/profile,
native-client/Samba oracle, and baseline compiler tests are established. The
coordinator and sub-agents closed the complete frontend: all 53 units now
preprocess, lex, and parse cleanly after regression-backed repairs. All 53 units now emit and link, and the default pipeline builds raw and
post-processed products. Real host fixtures pass native/JIT/NativeAOT; generated
crypto/ABI and NTLMSSP SMB interoperability checks pass in the scoped Linux
campaign. Full acceptance remains open; translated upstream tests and Kerberos
on Windows/Linux are now required follow-up milestones, and domain DFS path
resolution is an added product target. The initial
[lexer/parser probe](parse-probe.md) remains preserved; current results
and remaining gates are tracked in [validation.md](validation.md).
Campaign working directory: `<repo>/libsmb2/`.

This plan follows the [SQLite](../../sqlite/docs/PLAN.md),
[picotls](../../picotls/docs/PLAN.md), and [MsQuic](../../msquic/docs/PLAN.md)
campaigns: translate actual upstream C, repair shared compiler/runtime defects,
provide explicit host services, and validate real managed consumers under JIT
and NativeAOT. Their historical results do not establish libsmb2 correctness.
The user authorized this campaign with a coordinator and sub-agents, committing
after each significant milestone. Keep tested milestone commits local; do not push.

**Updated test scope:** ignore fault-injection work unless the pinned upstream
library contains an explicit corresponding test. Record the upstream file and
the exact behavior before including such a test. Related coverage alone is not
equivalence: upstream's injected `readv` error does not automatically authorize
a different custom TCP-reset scenario. Custom transport reset and synthetic
failed-operation probes are excluded from the default campaign. Normal API,
ownership, cancellation, invalid-input and known-answer tests remain functional
validation. The unfinished additional cleanup investigation is paused.

**Expanded requirements:** translate applicable upstream C test programs and
provide a C# test runtime/runner preserving their checks. Add Kerberos with both
explicit credentials and existing tickets/current-user sign-in on Windows and
Linux. Investigate and deliver domain DFS namespace resolution for paths such as
`\\work.example\a\department\file`. These are requirements, not claims about
the current NTLMSSP-only product. See [the implementation extension](enterprise-client.md)
for evidence, boundaries, milestones, and acceptance criteria.

**Current execution scope:** P7 upstream tests have a committed partial execution
matrix; see [upstream-tests.md](upstream-tests.md) for the remaining cases. The
2026-09-22 request adds the [managed async transport plan](async-transport.md),
with implementation not yet started by that planning request. P8 Kerberos and P9
DFS remain on hold pending the user's decision. Their requirements and candidate
designs remain recorded; do not begin further research or implementation for
either feature until the user resumes that work.

## Objective and fixed delivery requirements

Translate upstream libsmb2 into a reusable unsafe C# SMB2/SMB3 client library.
Keep protocol encoding/decoding, negotiation, authentication, request queues,
credit accounting, signing, encryption, and file operations translated from C.
A wrapper around native libsmb2, an OS-mounted share, or a separately implemented
C# SMB client does not satisfy this objective. Native implementations are separate
test processes. Parsing or compiling the library alone is not completion.

The following are mandatory deliverables, including when helper scripts are used:

- **`libsmb2/ManagedConsumer.slnx` must be created for a working sample.** It must
  include the generated library, any authored host/facade projects, and
  `samples/ManagedConsumer/ManagedConsumer.csproj`, using ordinary project
  references. The solution belongs directly under `libsmb2/`.
- **`libsmb2/scripts/translate.sh` must perform the full translation.** Its default
  invocation prepares verified inputs and tools, translates and links the entire
  configured source closure, emits the managed project, runs the existing semantic
  C# postprocessor, and builds the final project. No separate manual emission,
  copying, source repair, or postprocessing command may be required.
- **`libsmb2/generated/TranslatedLibsmb2/` is the final generation output for
  translated and post-processed C# sources.** The product project is
  `libsmb2/generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj`. The sample must
  reference this project. Raw comparison output must use a different directory;
  a diagnostic/raw invocation must never replace the final product with raw code.
- **The managed sample and any authored managed API must use named constants
  and enum members from the translated/generated source whenever those values
  are exported.** Do not duplicate available dialect, authentication, flag,
  status, or file-type values as numeric literals. If the generated surface does
  not expose a needed value, document the fallback and its source/ABI; ordinary
  sample payload sizes and test data are not protocol constants.
- **The managed transport must support async/await without a poll/select wait
  loop or Task.Run wrapping network waits.** Register `smb2_fd_event_callbacks`
  and service the translated protocol from C# socket completions. Supply socket
  functions in authored C# included in the generated project, with a distinct
  int-backed `t_socket` struct with explicit conversion to int and implicit
  conversion from int. Prefer defines and translation-profile bindings; a typedef
  plus struct declaration in `build/managed/config.h` is an allowed fallback.
  Socket functions remain C#; add no C implementation or source/output rewrites.
  See [async-transport.md](async-transport.md)
  for the compiler feasibility gate, ownership model and compatibility boundary.

Use the repository's .NET 10/C# 14 baseline, dotcc headers/libc, source/object
linking, direct layout constants, nested API types, static callback pointers, and
split-source emission where appropriate. These capabilities are starting points,
not evidence that libsmb2 already works. Preserve NativeAOT compatibility and
keep Roslyn tooling out of the translated application's runtime dependencies.

## Upstream baseline and reproducible inputs

At implementation start, select an official release or immutable upstream commit;
record its exact revision, archive URL, SHA-256, licenses, and provenance in
`docs/source.md`. Fetch and verify that snapshot on every regeneration. Do not
build from a moving branch or let CMake autodetection silently select dependencies.

The upstream [README](https://github.com/sahlberg/libsmb2/blob/master/README)
describes synchronous, asynchronous, and raw client APIs, built-in NTLMSSP,
optional Kerberos, SMB signing/encryption, and server support. This campaign's
first product is a client. Server hosting remains deferred; Kerberos is now a
required additional client profile.
The planning links describe upstream structure; replace them with immutable
revision links when pinning inputs.

Derive an explicit source/header/define manifest from the pinned
[library build list](https://github.com/sahlberg/libsmb2/blob/master/lib/CMakeLists.txt).
Inventory transitive includes as well as translation units: current share-enum
wrappers include code from `libdcerpc/`, although the full DCE/RPC library is a
separate product. Account for portable crypto sources, optional Kerberos wrappers,
and platform AES selection. Never assume that compiling only `libsmb2.c` creates
the library, or that every dependency resides in `lib/`.

Preserve downloaded sources unchanged under `ref/`. Keep feature headers and
host declarations in `config/`, and small adapters in `src/`. If an existing
boundary cannot express a required host operation, document the smallest
reproducible, hash-checked adaptation in a generated input copy. Do not alter SMB
algorithms or generated C# to bypass compiler defects.

```text
libsmb2/
  README.md
  ManagedConsumer.slnx              sample solution at the campaign root
  docs/
    PLAN.md                        this plan and completion checkboxes
    source.md                      immutable inputs, hashes, licenses
    configuration.md               source closure, defines, ABI, feature matrix
    host-contract.md                sockets, entropy, time, ownership, errors
    api.md                         managed API, lifetimes, cancellation
    blockers.md                    reduced failures, fixes, full-source retries
    validation.md                  commands, case results, exclusions, evidence
    usage.md                       clean translation and consumer instructions
  config/                          source manifest and managed host headers
  scripts/
    translate.sh                   complete default source-to-product pipeline
    fetch.sh                       verified source acquisition
    build.sh                       build existing final output without translating
    test.sh                        local validation and managed consumer checks
    oracle.sh                      pinned native client and server test setup
    verify.sh                      clean regeneration and full acceptance campaign
  src/
    Host/                          C declarations and BCL host implementation
    Managed/                       owning facade, if a separate project is needed
  samples/ManagedConsumer/
    ManagedConsumer.csproj          sample referencing the final product
    Program.cs
  tests/                           ABI, vectors, codecs, host, lifecycle, interop
  ref/                             verified unchanged source/oracle inputs; ignored
  generated/
    TranslatedLibsmb2/              final translated + post-processed C# and project
    TranslatedLibsmb2.Raw/          separate unprocessed comparison project
  build/                           native/JIT/AOT build and staging outputs; ignored
  artifacts/                       logs, manifests, diagnostics, results; ignored
```

All campaign-specific files live under `libsmb2/`; generic compiler/runtime fixes
and tests stay in their existing repository projects. Ignore fetched sources,
generated output, build products, credentials, and artifacts. Commit authored
recipes, adapters, fixtures, and documentation during implementation. Preserve
unrelated workspace changes; do not push as part of this campaign.

## Required client profile

Freeze exact capabilities against the pinned headers and implementation in P0.
These are acceptance targets, not claims about a completed port.

| Area | Required behavior |
| --- | --- |
| Dialects | SMB 2.0.2, 2.1, 3.0, 3.0.2, and 3.1.1 negotiation and file access, subject to confirming the pinned source's support; exercise each explicitly. |
| Authentication | Built-in NTLMSSP plus required Kerberos with explicit credentials and existing tickets/current-user sign-in on Windows and Linux. Explicit mechanism selection; no silent NTLM or guest fallback when Kerberos is required. |
| Namespace resolution | Domain DFS UNC paths resolved to their backing server/share/path, with authentication to each selected target; implementation and qualification pending. |
| Integrity and confidentiality | Required signing and SMB3 encryption profiles using algorithms actually implemented by the pin. Test SMB 3.1.1 preauthentication/key derivation and enforce requested signing/encryption without silent downgrade. |
| File API | Connect/disconnect; directory enumeration; stat/fstat/statvfs; create/open/close; offset reads/writes; flush/truncate; mkdir/rmdir; rename/unlink; EOF and ordinary error paths. |
| Request processing | Actual upstream asynchronous requests, compounds, credits, partial I/O, large transfers and deadlines. New transport uses fd-event callbacks and awaitable completions; multiple outstanding operations remain a separate API gate. Managed synchronous conveniences may wait on the async core; upstream poll-based synchronous APIs require an explicit legacy profile. |
| Managed API | Owning connection, file, and directory handles; Task-based operations; explicit cancellation, errors, and asynchronous cleanup. Retain the low-level translated C-style API. |
| Transport | Real BCL async TCP/DNS, IPv4/IPv6, fd-event callbacks, configurable test port, bounded buffers and cleanup; authored C# host with typed socket handles distinct from Libc fds. |
| Platforms | Linux x64 first with dotcc's LP64 ABI; Windows x64 is also required for the expanded Kerberos/DFS profile. Linux arm64 and macOS arm64 require separate execution evidence. |

Inventory every exposed API, option, and command as required, translated but
unqualified, or deferred. Initially defer SMB server hosting, full libdcerpc,
durable/persistent-handle recovery, multichannel, RDMA, and application-level
lease/oplock caching. Preserve
required break handling for capabilities actually negotiated; do not request
unimplemented caching guarantees. Share enumeration and optional raw commands
need their own qualification before the facade advertises them. A required
feature gap remains a blocker until resolved or the plan is explicitly revised.

## Translation and host boundaries

Translate the portable upstream cryptographic implementation with the protocol
code, including primitives needed by authentication, signing, and sealing. This
avoids inventing a provider abstraction or assuming every legacy primitive is
available in the BCL. Use secure BCL randomness for live sessions. Audit platform
accelerators and select a real portable backend explicitly; no application-owned
native SMB, portable SMB-crypto, or socket imports belong in the managed product.
Normal implementation dependencies of the BCL are acceptable. The new Kerberos
profile may use a pinned managed Kerberos provider. Existing OS ticket integration
needs a separately documented dependency decision: any necessary platform
authentication adapter must remain explicit and must never become a native SMB
backend. Preserve the native-free explicit-credential path as the preferred design.
Validate primitives with known-answer/native vectors and protocol protection
with real peers.

Host adapters supply execution services, not replacement SMB logic. Prefer shared
dotcc libc implementations for non-socket services when semantics match. The
new async transport redirects socket execution to authored C# through defines and
typed profile bindings. Define the required socket, DNS, scatter/gather I/O,
errno, time and allocation contracts from actual imports. Keep upstream
`socket.c` framing and request servicing translated; the host seam leaves protocol
framing and queues in C. The only allowed new C surface for this transport is the
fallback handle typedef/struct declaration in generated `build/managed/config.h`;
all host function implementations remain C#.

The [public header](https://github.com/sahlberg/libsmb2/blob/master/include/smb2/libsmb2.h)
defines context, completion callback, and iovec ownership contracts. Audit the
pinned header and implementations before wrapping them. Map managed sockets to
stable opaque handles, distinguish would-block from EOF and failure, and retain
buffers until the actual operation completes. Check native and generated sizes,
alignments, offsets, integer widths, callback signatures, and actual storage.
Do not use Windows' native C ABI as an LP64 layout oracle.

Serialize servicing and mutation of each context while allowing independent
contexts to progress concurrently. The completion executor honors fd-event
callbacks, partial sends/receives and actual one-shot deadlines. It must not poll
for readiness or block a thread while waiting. Never implement a synchronous wait
on the execution path needed to finish that request. C-facing socket calls remain
nonblocking synchronous functions over owned buffers; BCL async I/O supplies their
data and capacity. See the dedicated transport plan for pointer lifetimes.

Use canonical static managed `delegate*` registrations and rooted opaque state;
never store movable managed references in C structs or retain stack-lived buffers.
Specify ownership of copied credentials, paths, PDUs, user buffers, callback data,
file handles, and errors. Preserve originating allocator/free pairs. Map exceptions
at the managed boundary and ensure each operation completes at most once.
Cancellation must follow the actual upstream contract: document when it abandons
only the caller's wait versus terminates a request/context, and drain retained
state before freeing it. Do not claim a cancelled write was rolled back.

## Full translation and postprocessing contract

`scripts/translate.sh` must resolve paths from its own location and work from the
repository root, `libsmb2/`, or an unrelated working directory. Default execution:

1. Verify prerequisites, fetch/check pinned inputs, and build the compiler and
   `DotCC.PostProcess` tools with recorded versions and hashes.
2. Materialize the explicit configuration, preprocess and translate every selected
   translation unit, link the complete library, and fail on unresolved imports.
   Record source closure, defines, ABI and layout metadata.
3. Emit `TranslatedLibsmb2.csproj` and split C# sources into isolated staging.
   Add authored project references without generating duplicate host definitions.
   Restore/build dependencies and compile the raw library as a baseline.
4. Preserve an isolated raw comparison project, then invoke the existing
   [semantic postprocessor](../../docs/postprocess.md) explicitly with `--in-place`
   on the staged product. Restrict evaluated editable inputs to staged/generated
   files; referenced authored projects must not be rewritten. Do not use regex or
   hand edits. Build the post-processed result and verify relocated references.
5. Promote the validated product to `generated/TranslatedLibsmb2/`, remove only
   obsolete campaign-owned generated files, and write a success receipt with
   input/tool/output hashes and postprocessing results. If a stage fails, exit
   nonzero; preserve the previous valid output and identify it as stale for the
   attempted inputs. Never label a partially regenerated directory successful.

Diagnostic switches may select raw-only output or reuse verified tools, but the
default command always completes all stages. Raw comparison output and temporary
postprocessor snapshots cannot become the solution's default reference. Repeated
translation must not accumulate stale C# files, duplicate types, or machine-specific
absolute references. Verify postprocessing idempotence and raw/processed behavior
in both JIT and NativeAOT; optimization cannot be used to repair invalid emission.

`build.sh` builds existing output; `test.sh` validates it; `verify.sh` orchestrates
fresh translation and acceptance tests. They must not hide required translation
steps outside `translate.sh`. Translation itself must not require a live SMB
server or native oracle. The user-facing recipe after prerequisites are installed
uses these entry points (their current validation scope is documented below):

```sh
./libsmb2/scripts/translate.sh
dotnet build libsmb2/ManagedConsumer.slnx -c Release
dotnet run --project libsmb2/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -- --help
dotnet publish libsmb2/samples/ManagedConsumer/ManagedConsumer.csproj -c Release -r linux-x64 -p:PublishAot=true
./libsmb2/scripts/verify.sh
```

## Failure-driven implementation workflow

For each real preprocessing, parsing, linking, C# compilation, or runtime failure:

1. Save the complete command, input/tool hashes, stage, source location, and first
   diagnostic under `artifacts/`; summarize it in `docs/blockers.md`.
2. Reduce the cause to valid minimal C and demonstrate a failing regression before
   fixing shared code. Use `DotCC.Tests` for focused compiler checks and
   `DotCC.FunctionalTests/Fixtures` for emitted C# compilation/execution. Verify
   semantic expectations with a matching-ABI native C compiler.
3. Fix the structural compiler/runtime issue, not the library source or emitted
   text. Treat candidate issues such as packed layouts, endianness, pointer tables,
   cross-unit statics, flexible tails, and integer promotions as hypotheses until
   demonstrated. Rebuild parser tables fully after grammar changes.
4. Run focused tests and affected suites serially, retry the full pinned source
   closure, and record the new result. Add library integration coverage for
   semantic defects. Commit coherent tested fixes locally during implementation.

Do not introduce success stubs, skip function bodies, remove required algorithms,
or disable authentication to advance a milestone. Test scaffolds remain isolated
and cannot satisfy execution gates. Record baseline repository failures separately.

## Milestones and acceptance gates

### P0 — Freeze sources, dependencies, and native controls

- [x] Pin source and test inputs, licenses, source manifest, defines, and ABI.
- [x] Inventory APIs, portable crypto, host imports, and actual dialect/algorithm
      support; publish the required/deferred feature matrix.
- [x] Build a native client from the same pin and establish authenticated file
      operations against a pinned local Samba server using disposable accounts.
- [x] Record the existing compiler/runtime test baseline and attempt translation
      of the complete configured library, preserving actual first blockers.

Gate: reproducible inputs, working native reference and independent server,
explicit scope, and an evidence-based blocker ledger.

### P1 — Prove compiler, ABI, and host feasibility

- [x] Reduce real failures and compare public/internal layouts and callback probes
      against native, including actual storage and high-bit status values.
- [x] Demonstrate BCL TCP/DNS/readiness, short I/O, error mapping, entropy, and
      retained-buffer lifetimes under JIT and NativeAOT.
- [x] Verify portable crypto selection and initial authentication/signing/sealing
      vectors; document any necessary minimal host adaptation.

Gate: usable host services and callback ABI, known crypto/source closure, and no
unexamined native dependency needed to reach execution.

### P2 — Translate, link, and postprocess the complete library

- [x] Fix evidenced compiler/libc gaps using the regression-first workflow.
- [x] Build every configured unit into a reusable raw library; audit imports,
      static initialization, public constants, and symbol/callback identity.
- [x] Implement the full `libsmb2/scripts/translate.sh` contract and final
      `libsmb2/generated/TranslatedLibsmb2/` output, plus separate raw output.
- [x] Build raw/processed variants under JIT and whole-assembly-rooted NativeAOT;
      verify native layouts and postprocessor idempotence/equivalence.

Gate: one default command reproducibly produces the complete, buildable final
post-processed project. This gate does not establish SMB interoperability.

### P3 — Integrate real transport, authentication, and protection

- [ ] Connect the translated event/request machinery to the BCL host and run
      negotiation, authentication, tree connection, echo, and disconnect.
- [x] Test each required dialect and actual signing/encryption algorithm against
      native controls and Samba, with matching negotiated settings.
- [ ] Exercise wrong credentials, missing shares, required-signing/encryption
      rejection, corrupt signatures/tags, malformed negotiation/authentication,
      preauthentication mismatch, partial initialization, and disconnect cleanup.

Gate: authenticated real TCP sessions with enforced protection in raw/processed
× JIT/NativeAOT. No insecure fallback may turn a negative case into success.

### P4 — Qualify file operations, errors, and lifetimes

- [ ] Compare the required file/directory API against native using isolated shares:
      exact contents, lengths, offsets, metadata where deterministic, and errors.
- [ ] Cover empty/large files, Unicode paths, short reads/writes, EOF, sparse
      offsets where supported, compound requests, credit exhaustion/replenishment,
      multiple in-flight requests, independent connections, and backpressure.
- [ ] Exercise access denial, nonexistent paths, sharing conflicts, peer closure,
      bounded timeout/cancellation, callback reentrancy, GC stress, allocation
      failures, repeated disposal, and cleanup with operations still pending.
      Apply the updated upstream-only rule to injected failures; cases without
      an explicit upstream counterpart are outside the current scope.
- [ ] Run bounded malformed/truncated/oversized PDU and partial-stream corpora;
      compare deterministic decoding with native and verify bounded resources.

Gate: correct operations, status mapping, at-most-once completion, and clean
resource drain in all four generated/runtime combinations.

### P5 — Deliver the owning API and sample solution

- [x] Create **`libsmb2/ManagedConsumer.slnx`**, referencing the final generated
      project, required host/facade projects, and managed sample project.
- [ ] Implement the owning API with documented credentials, cancellation, buffer
      lifetimes, concurrency, errors, and disposal ordering.
- [x] Audit the managed API and sample for numeric protocol/ABI literals; use
      exported translated constants or enum members wherever available, and
      document only the necessary unavailable-constant fallbacks.
- [x] Provide a sample that connects to a configured server/share, lists entries,
      writes and reads back a uniquely named test file, verifies bytes, and cleans
      up its own file. Accept credentials through a documented non-logged input;
      do not embed secrets or modify unrelated share contents.
- [x] Build the solution, run the sample under JIT, and publish/run it with trimmed
      NativeAOT. Verify ordinary project references without compiler internals or
      manually copied generated source. Document the complete usage recipe.

Gate: a separate managed application performs authenticated file operations using
`generated/TranslatedLibsmb2/TranslatedLibsmb2.csproj` under both runtimes.

### P6 — Clean regeneration, regression, and delivery qualification

- [ ] From clean campaign outputs, run translation, solution build, raw/processed
      × JIT/NativeAOT tests, native differentials, and the independent-server matrix.
      Verify invocation from outside the repository and failed-stage/stale-output
      handling without destroying unrelated files.
- [ ] Audit authored/generated code and published dependencies for native SMB or
      crypto backends, undeclared authentication dependencies, dynamic code, and
      missing AOT roots.
- [ ] Run full unit/functional tests after the final shared compiler changes,
      relevant postprocessor tests, and fresh SQLite, picotls, and MsQuic regression
      campaigns affected by those changes. Exercise the NuGet LALR.CC build path;
      record exact compiler receipts rather than reusing earlier pass claims.
- [ ] Measure transfer throughput, allocations, retained memory and shutdown
      against equivalent native workloads; investigate results without inventing
      performance thresholds. Inventory applicable upstream tests and omissions.
- [ ] Execute available additional platform/server targets, including Windows SMB
      interoperability when a runner exists; label unavailable rows unverified.
      Publish final commands, pass/fail/skip counts, limitations, and source hashes.

Gate: reproducible delivery and passing required Linux x64 behavior with explicit
platform evidence. An unavailable required local server or a skipped mandatory
test leaves acceptance incomplete; it must not produce an unconditional pass.

Dependencies: P0 → P1 → P2 → P3 → P4 → P5 → P6. API and host design may inform
earlier phases; their completion still depends on real translated execution.

### P7 — Execute translated upstream tests

- [ ] Translate pinned upstream C tests and required example executables; preserve
      assertions, callbacks and exit behavior, with a C# runtime/fixture runner.
- [ ] Compare native and raw/processed JIT/NativeAOT runs. Map every upstream shell
      case to execution, an explicit adaptation, or an explained skip.
- [ ] Preserve the upstream-only fault-injection scope; distinguish native-only
      instrumentation from managed evidence.

### P8 — Kerberos on Windows and Linux

- [ ] Qualify a pinned Kerberos.NET provider first: tokens, mutual authentication,
      correct SMB session keys, signing/encryption, and trimmed NativeAOT.
- [ ] Integrate the translated authentication boundary and owning API with
      explicit credentials and existing tickets/current-user sign-in.
- [ ] Execute real KDC/SMB interoperability on both Windows and Linux; record
      supported cache types, dependencies, expiry behavior and mechanism selection.

### P9 — Domain DFS namespaces

- [ ] Select and document the missing referral implementation while retaining
      translated SMB transport/protocol operations; constants alone are insufficient.
- [ ] Resolve domain/root/link referrals, preserve path suffixes, cache by TTL,
      and obtain Kerberos service tickets for actual target hosts.
- [ ] Verify two namespace paths targeting different servers, direct-share parity,
      nested links and ordinary target selection on both Windows and Linux.

### P10 — Managed async transport without polling

- [x] Shared compiler prerequisites: `externalTypes` with optional size/alignment,
      repeatable `--type-name`, and signature-preserving `functionOverrides` with
      managed-method targets and object contracts (`10c2c60`). See
      [external types](../../docs/external-types.md) and
      [function overrides](../../docs/function-overrides.md).
- [x] A0: prove defines/host bindings and the authored four-byte int-backed socket type,
      including upstream integer scratch paths and generic compiler support needed.
- [x] A1: implement the C# socket host with bounded async send/receive, DNS
      preparation, socket options, errors and explicit lifetime ownership.
- [x] A2: replace the facade's blocking pump with fd callbacks, a serialized
      completion executor, owned awaitable requests and asynchronous disposal.
- [ ] A3: qualify raw/processed JIT/NativeAOT, real SMB interoperability, idle/load
      behavior, handle-domain separation and clean generation/project inclusion.

Detailed gates and files are in [async-transport.md](async-transport.md).
P10 implementation is authorized and in progress as of 2026-09-23, coordinated
through subagents with commits after each significant tested milestone. It is
independent of the paused P8/P9 decisions. P7 results remain valid for their
recorded transport profile; P6 acceptance must qualify the new product host
separately and distinguish any legacy upstream-test transport.

## Validation rules and final acceptance

Track dialect × protection mode × generated variant × runtime, plus IPv4/IPv6,
API, failure, and lifecycle coverage. Use native libsmb2 as a differential client
and Samba as an independent protocol implementation. Keep fixtures, server
configuration, identities, random seeds, and timeouts reproducible. Compare
deterministic vectors/bytes/errors exactly; compare live authentication outcomes
and protocol invariants without normalizing away protection failures or corruption.

Completion requires the actual translated client, real host services, all required
features and negative tests, and the working sample. Specifically,
`libsmb2/scripts/translate.sh` must regenerate the final translated and
post-processed sources at `libsmb2/generated/TranslatedLibsmb2/`, and
`libsmb2/ManagedConsumer.slnx` must build and exercise that product. Report only
validated capabilities; no native wrapper, handwritten SMB replacement, successful
compile alone, or stale generated artifact can satisfy these requirements.
