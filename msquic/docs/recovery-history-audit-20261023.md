# MsQuic recovery history audit — 2026-10-23

The retained evidence establishes recurring unsuccessful recovery outcomes; it
does **not** establish 87 translation discrepancies or an increase in compiler
defects. The frozen four-campaign matrix has **489 strict successes and 87
strict failures**, all recognized by the default warning policy: 39 dual-live
rebinding failures and 48 decreasing-ceiling failures. Stronger recovery and full
semantic parity remain unqualified. Historical failures cannot explain away the
fresh unsupported native terminal.

This is a read-only history audit, with no transport execution, rebuild, compiler,
runtime, harness or classifier changes. It follows the
[translation baseline](../../docs/plans/translation-baseline.md) and
[MsQuic plan](PLAN.md). Its only new files are this document and
[`history/`](../artifacts/recovery-parity-investigation-20261023/history/).

## Counts with their actual denominators

The ordinary matrix is 2 forms × 2 runtimes × 3 endpoint arrangements × 2 IP
families × 2 AES suites × 12 scenarios = **576**. Roles `both`, `client`, and
`server` mean managed/managed, managed client/native server, and native
client/managed server. The 16 form/runtime/IP/cipher profiles become 48 when the
three arrangements are included. Recovery uses ECDSA, P256 and QUIC v1; RSA
coverage in the prerequisite peer suite does not add recovery profiles. `native`
is an additional control role, absent from the ordinary 576. In that role, `raw`
and `aot` are matching managed-profile labels: both executables are native C.

Recounted receipts, preserving literal `case.passed`:

- **September 12 positive subset:** 480/480 successes, ten scenarios in all 48
  profiles; `targeted_passed=true`, `passed=false`. Dual-live rebinding and
  decreasing ceilings were deliberately outside this subset.
- **September 24 complete collection:** ordinary 576 = **495 successes + 81
  failures** (33 rebinding, 48 ceiling). Twelve separately added native controls
  contribute 4 successes and 8 failures, making the actual receipt **588 =
  499 + 89**. Comparing 89/588 directly with 87/576 mixes populations.
- **September 27 complete collection:** **576 = 488 + 88**: 40 rebinding failures,
  47 ceiling idle warnings and one unexpected mixed FIN deadline. Thus 87
  warnings did not mean only 87 strict failures. Exit was nonzero.
- **October 18 complete collection:** **576 = 491 + 85**: 37 rebinding warnings,
  47 ceiling idle warnings and one recognized managed FIN deadline. The local
  `msquic/artifacts/recovery/results.json` is this historical run, confirmed by
  its hash in the CPython campaign's `msquic-composed-result.json`.
- **October 23 frozen four-campaign collection:** **576 = 489 + 87**: 39
  rebinding warnings and 48 ceiling idle warnings, zero unexpected outcomes.
  Its receipt hash is `97ed563106849c02575eb0f45193b612a10cfe8d3885d2982909c6335d6dfb5e`.
  The private checkout was intentionally removed; the receipt remains in
  `msquic/artifacts/post-repair-20261023/private-evidence.tar.gz`, member
  `repo/msquic/artifacts/recovery/results.json`.

All four complete ordinary collections passed the same 480 positive cases.
After matching all 576 case keys, September 24 → October 23 has 11 pass-to-fail
and 5 fail-to-pass transitions; September 27 → October 23 has 5 and 6; October
18 → October 23 has 9 and 7. **Every strict-outcome transition is rebinding.**
The corresponding net strict-failure changes are +6, −1 and +2. Normalized proxy
configurations match for every paired case after removing the assigned server
port. These are different executions, certificates, managed products and peer
baselines, not packet/timer replays. Neither the +2 count nor the earlier −1
count establishes a change in failure probability or its cause.

## What actually changed in the tests

`git log/show` and source/AST identities are retained in
[`source-history.json`](../artifacts/recovery-parity-investigation-20261023/history/source-history.json).

- `b98b039fd` initially declared eleven scenarios: a full selection would be
  528 cases. Its early native rebinding "pass" in `recovery-path-diagnostics`
  checked delivery but lacked native active-path evidence; rebinding validation
  applied only when the server was managed. `5b7dd6ef6` added exact native
  core-header snapshots and required validation of the actual new active path
  for every server. Its added expired-mapping control expanded the matrix by
  48 to 576. The subsequent native private-path case failed the stronger gate.
  These two historical native results test different assertions.
- `dddca0b19` added process-lifetime evidence for proxy I/O errors;
  `582a4c852` retained the server binding until proxy drain; `8c4235183` made
  ceiling increase follow an actual oversized drop; `f4392cdbe` waited for both
  actual send-FIN acknowledgments before releasing connection shutdown. Earlier
  positive attempts stopped after 36, 33, 79 and 362 successes respectively.
  Those partial receipts cannot be treated as full matrices.
- `e8cfd5961` introduced a five-comparison classifier for one selected
  optimized/JIT/IPv4/AES-128 profile. Its `observation_validated=true` did not
  establish all-profile qualification. `101cf0a30` required all sixteen
  profiles, five independent rebinding attempts per role/profile, retained
  successes, duplicate-execution rejection and explicit managed deadline
  evidence. It also accepted a validated active path despite an aggregate
  old-path failure counter; new-path validation remained mandatory.
- `d36af8eb6` added default warning continuation and mixed-role case validation.
  It preserved failed case results. The official observation matrix still
  compares native/native with managed/managed. `059b46d3e` added `--keep-going`
  and bound current product closure evidence. `42c0f0300` routed subprocesses
  through production support. This explains changes in exit/collection flags;
  it does not turn unsuccessful recovery into success.

The `exchange()` AST is unchanged since the FIN barrier. Proxy bytes are
unchanged since the drop trigger, and native peer source bytes since the FIN
barrier. These checks cover sampled historical revisions through HEAD.
Classifier bytes changed for mixed-role ownership checks on September 26;
since then its hash is
`b9f2e0c00ae8ddc22b0ca9c78ec38542548153a5373567a50ceb7772a239728c`,
including the frozen October 23 campaign. The original driver without warnings,
default warning continuation, diagnostic collection and official classifier are
distinct acceptance/collection contracts.

## Repetitions and unsuccessful-outcome support

The fixed classifier campaign contains **288 observations in 80 logical
profile/scenario groups**: 160 rebinding attempts, 32 ceiling decreases and 96
positive controls. October 23 reuses 80 managed/managed rows from the 576 matrix
and executes 208 fresh rows. The union is **784 exchanges**, not 864. Sampling
was fixed before execution; the native unsupported outcome did not trigger more
attempts. Exact controller commands and input hashes are retained in
[`sampling-and-terminal-evidence.json`](../artifacts/recovery-parity-investigation-20261023/history/sampling-and-terminal-evidence.json).

September's repeated rebinding observations were native 19 successes/61 failures
and managed 13/67; October's are native 11/69 and managed 12/68. Every repeated
decreasing-ceiling transfer failed in both campaigns. September had one
unmatched managed deadline; October has the unsupported native terminal below.
The September classifier completed 80 comparisons but kept
`observation_validated=false`; October's official classifier aborts before
emitting any comparisons, with `complete_evidence_validated=false`.

The separate
[`accounting/results.json`](../artifacts/recovery-parity-investigation-20261023/accounting/results.json)
checks the 288 fixed observations plus 160 mixed-role observations. It accepts
447/448 per-case shapes and leaves one native case unclassified. Of the original
87 strict failures, **84 have same-profile/configuration native checked-kind
witnesses**: 39/39 rebinding and 45/48 ceiling cases. Three ceiling role-cases in
raw/AOT/IPv6/AES-128 lack an exact native kind witness.

The often cited **79/80 measures one-way support for managed failure kinds**.
Bidirectional observed kind sets agree in 72/80 groups. Seven rebinding groups
have a strict success observed only on one side (five native-only groups,
six success observations; two managed-only groups, three observations); one
ceiling group contains the unknown native terminal. Five samples cannot prove
an outcome impossible on the other side. These figures demonstrate neither
divergence nor complete parity, and cannot replace the rejected official run.

## Provenance and remaining objections

The C pin remains `80a065112426bce68c1da42d026478d3e40fd45e`. September 24/27
and October 23 baseline receipts bind the same native `connection.c` hash,
the same OpenSSL P256 profile and the same effective native compilation flags
apart from include-root paths. The native peer source remains
`c36f604686563743eff5c75e6a016479547055bab557e6947ded9dd1ac14b51d`.
September's native peer/library bytes match each other, but differ from
October's rebuilt binaries. A common source pin alone is not binary identity.
[`baseline-identities.json`](../artifacts/recovery-parity-investigation-20261023/history/baseline-identities.json)
retains exact peer/library, closure, certificate, source and compilation identities.
The separate native reference uses quictls
`ff36838bb69801cad56823159a036977bcbe5c75`; the managed product uses translated
picotls `3598470df01264da85157025ed10db0f7e103790` and the BCL host. Those provider
and execution boundaries are part of the comparison, even with a shared C core.

Managed products also changed: September 24 used compiler source `1abb6a813`;
September 27 records compiler library hash
`5e7f4c566aaa7f292fb104c8800866e5dc10b5bfa8b825630b19695adef111a7`
and `--tools reuse --fetch never --hashes strict --form all --mode all --rid
linux-x64 --jobs 2`. The frozen four-campaign compiler source is `123b3e76e`, whose accepted
`DotCC.Lib.dll` hash is
`08a0ecd70213b342bb763a8bd381de64387953a8ba1c587fd8451d8d505d298d`.
Changing these inputs prevents attributing a count delta to one compiler change.
The driver, classifier, proxy, native peer, managed peer and MsQuic source-pin
files are byte-identical at `d49fdbbaf`, `ddc9faf08`, `123b3e76e` and audit HEAD.

Explicit managed deadlines predate October: September's repeated unmatched case
was optimized/AOT/IPv4/AES-256; September 24 recorded raw/AOT/IPv6/AES-128 and
optimized/AOT/IPv6/AES-256; September 27 recorded the mixed managed-client
optimized/AOT/IPv6/AES-256 case; October 18 recorded a managed/managed deadline
in that IPv6 profile. The native harness uses 15,000 one-millisecond sleeps;
the managed harness uses a 15-second monotonic budget. A fixed proxy seed
reproduces jitter for a given packet sequence, not the sequence or worker/timer
schedule. The September 25 late-packet controls establish possible timer/deadline
mechanisms, not the uncaptured cause of each historical event.

The current unknown is specifically the native/native
`raw-aot-native-ipv6-128-payload-ceiling-down`: client connected/unfinished/closed,
status/error/peer-error 0/0/0, server 62/1/0. Its client log has no explicit
deadline marker. Elapsed 16.0775 seconds does not establish a deadline cause.
The same-profile managed terminal is ordinary idle 62/1, not a new managed kind.
The immutable finding is
`/tmp/dotcc-four-campaign-20261023/evidence/msquic/native-unsupported-terminal/finding.json`.
No translated product executes in that native-only control. Its cause remains
unknown; it cannot be classified preexisting from a superficially similar old
event, and the missing exact witness remains a qualification objection.

No compiler divergence is demonstrated by this audit, and full semantic parity
is not demonstrated either. Existing kind validators allow partial byte counts,
packet counts and timings to differ and do not compare complete callback/timer
histories. The original strict failures and all unclassified observations remain
unsuccessful outcomes with those limits.

Reproduce the accounting and source comparison without executing transport:

```sh
python3 msquic/artifacts/recovery-parity-investigation-20261023/history/audit.py
```

The script verifies the retained archive and selected member hashes, recounts
receipts, checks all 576 case identities/configurations, records exact endpoint
and command inventories, and compares historical Git blobs/ASTs. It writes only
its `history/` directory. Input paths, archive members and SHA-256 values are in
[`input-bindings.json`](../artifacts/recovery-parity-investigation-20261023/history/input-bindings.json);
case transitions are in
[`matched-case-comparisons.json`](../artifacts/recovery-parity-investigation-20261023/history/matched-case-comparisons.json).
