# Current validation

Implementation remains in progress through P6. Boundary, translation and current
delivery, native and reproduction gates pass for the recorded provider snapshot.
The complete upstream C/Python gates remain open. Original certificate assertions
exposed an ASN.1 conversion defect; the reviewed repair and48-group provider
qualification now pass for captured0158 bytes. A later private-RSA mismatch
requires another provider freeze and fresh downstream qualification. No build-only, emission-only,
skipped, retried or interrupted workload is counted as a strict protocol pass.
Paths below are relative to this campaign.

## Final ASN.1 provider freeze

The production repair is committed as `5f0b8ff3d`; only `OpenSslAbi.Asn1.cs`
changes in the delivered provider. Freeze
`artifacts/provider/freeze-asn1-20261023T211648Z/receipt.json` records exact source
SHA256 `0158d36c4696b14eada67c1fe64dd47795838ddc55fbcef5e9f1c77f66d0033f`.
All48 native differential groups pass in JIT and NativeAOT at
`artifacts/provider/final48-asn1-211648/receipt.json`, including3,131 ASN.1 vectors
per runtime. The independent private review verifies56 files and the pinned
native type table, legacy5/6-byte UTF8, raw BMP values and failure/output-pointer
semantics: `artifacts/managed-contract/asn1-private-supervisor-review.json`.

Fresh CLI8 builds are complete. Independent audit
`artifacts/managed-contract/final-asn1-cli-eight-supervisor-audit.json` verifies
1,116 distinct files across all eight actual application identities, output,
build, runtime and authored-source closures. JIT provider DLLs are labelled built
outputs: the identity entry returns before provider initialization. NativeAOT
embeds the provider in the new executable. Original TLS family execution remains
a separate gate.

Fresh final-provider main1076/1076 and delivery12/12 pass at
`artifacts/managed/final-provider-asn1-main/report.json` and
`artifacts/managed/final-provider-asn1-delivery/report.json`. Independent repeated
audit verifies all5,023 actual application/runtime/build/source/product/fixture
files at `artifacts/managed-contract/final-asn1-main-delivery-supervisor-audit.json`.
The main matrix contains4consumer,328compatibility,624interop and120lifecycle
rows. Delivery covers copied ordinary consumers in all4 variants and whole-library
rooted AOT in both forms. The final18-family/four-variant Python controller started
once after verified legacy port-owner drain; original upstream qualification
remains separate. The unchanged original certificate all4 refresh now passes488 assertions per
variant at `artifacts/provider/original16-expanded/20261023T211832Z-6c2a4f05/all4.json`.
The old required-C cases must also execute against this freeze; their earlier application outputs are not reused. No C regeneration is
needed: the183 typed declarations and175 emitted engine objects are unchanged.

The persistent0158-provider C campaign has46 passed cases qualified in all four
variants,2 raw-build failures and209 unrun at the recorded checkpoint. Batch006
is naturally draining before the reviewed RSA repair. Its live progress is `build/provider/required257-223542/progress.json`;
earlier30/2 counts belong to the archived old-provider run. Closed cases retain
their executable/runtime bytes while identical pinned BC dependency files may
share a private immutable inode. The bounded three-case storage proof is
`artifacts/provider/dependency-dedupe-closed3-2140/receipt.json`; this is storage
maintenance, not an additional test pass.

## Recorded pre-ASN.1 provider evidence for223542

- Complete175-unit raw/processed publication: `artifacts/campaign/20261023-223542-f7e7054e/receipt.json` passes with the automatic-array compiler repair and183 typed provider declarations. Authored Base64 repair `08796c070` changes no C flags, headers or declarations.
- Full current provider differential suite:47/47 JIT and47/47 NativeAOT, `artifacts/provider/final47-223542/receipt.json`. Focused Base64 evidence includes1,659 native vectors per runtime in `artifacts/provider/base64-contract/receipt.json`.
- Current ABI across raw/processed × JIT/AOT: `artifacts/campaign/20261023-224814-4bc6fd5e/receipt.json` passes, including the canonical translated product and actual emitted header/provider probes.
- Current affected shared regressions:280 unit and23 functional tests pass with zero failures/skips in `artifacts/campaign/20261023-224117-ae267953/receipt.json`.
- Current copied/rooted delivery:12/12 pass at `artifacts/managed/final-delivery-223542-base64/report.json`. Deep audit `artifacts/managed-contract/final-delivery-223542-base64-r3-audit.json` verifies six actual application receipts and1,682 immutable files, including exact rooted-project XML derivation.
- The1076-row ordinary consumer/compatibility/interop/lifecycle matrix passes at `artifacts/managed/final-current-product-223542-base64/report.json`:4consumer,328compatibility,624interop and120lifecycle rows. Together with delivery, the post-native audit verifies5,023 immutable files at `artifacts/managed-contract/post-native238-qualification-audit.json`. These receipts identify the provider before the newly observed ASN.1 repair; they are not evidence for a changed provider.
- Fresh original257 C emissions pass at `build/managed-upstream/all-required-257-emission-223542.json`. Full257 × four-mode assertion execution is running under the persistent controller `build/provider/required257-223542/progress.json`; incomplete batches remain unrun/failed, never passes.
- The unchanged original buffered-send node that overflowed on214654 now passes in all four variants: `artifacts/managed-upstream/python-current-223542-base64/buffered-original-replay-{form}-{mode}.json`. Original broad Python qualification remains open.
- Native C oracle equivalence is independently checked at `artifacts/provider/native-equivalence/supervisor-review.json`:175 actual command-to-TU mappings, per-TU flags,1,321 staged source/header/fixture files and147 pinned OpenSSL headers. The native report stays unchanged; new managed executions are required.
- A separately labelled pinned OpenSSL test CLI passes12 native PSK positive/negative controls in `artifacts/native-peer-psk/proof-v3/report.json`, with byte-identical TLS/crypto archives. It enables execution of the previously skipped351 SHA384 original cases; those old skips remain failures. Details and commands are in [native-controls.md](native-controls.md).

## Inputs and native controls

- Framework acquisition: `artifacts/campaign/20261023-200313-154db7b3/receipt.json`. Source pins/archive hashes are in `config/inputs.json`; references remain pristine. Source inventory:174 upstream C units plus1 authored adapter,36 suites,14SSLv3 entries and78 valid suite/version pairs.
- All644 original inventory entries remain recorded:257 required C cases,18 required Python families,287 explicitly excluded and82 support. The formerly required record-stuffer file has its entire workload under literal `#if0`; it is now a source-checked exclusion, never a zero-assertion pass.
- Final caller-only DHE native product compatibility adapter: [238 passed,0failed,0skipped](../artifacts/native/final-callsite-profile/report.json), including78 pairs through self and both independent roles plus4 authentication controls. All36 suites and14SSLv3 entries execute; binary1027bytes,3RC4records and close_notify are asserted.16 original native controls execute590,537 assertions.
- Pristine deterministic DHE client/server probes preserve256-versus1 failure. Final staged repair changes only the two TLS<=1.2 caller requested lengths, leaving the generic padding helper unchanged. Current probe receipt: `artifacts/native/dhe-callsite-repair/report.json/report.json`; generic helper17 original assertions pass. The old helper-body repair and its broad-control receipts are historical evidence.
- Broader native original C controls: [257/257 pass](../artifacts/native-upstream/all-required-callsite/report.json). Their autodetected branch counts are retained rather than substituted for the product profile.
- Exact214654 product-feature native control: [257/257 pass](../artifacts/native-profile-upstream/all-required-214654-ktls-guards/report.json). All algorithm operations use pinned native OpenSSL; three explicitly labelled test-only ABI/name/capability metadata bridge functions are not delivered. Original support closure comes from actual native object references, and archive extraction preserves tests that include a complete engine C TU. Eight accepted opening-directive guards across recv/shutdown/keyupdate exclude only outside-plan kTLS regions with unchanged assertion bodies and reversible hashes. CBC238 versus broader native406 is an approved composite-acceleration branch difference; ordinary CBC/HMAC suites remain required.

Reproduce from repository root:

```sh
python3 s2n-tls/scripts/source-inventory.py
python3 s2n-tls/scripts/native-control.py --fetch never --selection all --reuse-credentials
python3 s2n-tls/scripts/native-upstream.py --output /absolute/output/native-all.json
python3 s2n-tls/scripts/native-profile-upstream.py --campaign s2n-tls/build/campaign/20261023-214654-406d9780 --output /absolute/output/native-profile.json
```

## Provider, ABI and complete translation

- BCL plus pinned BouncyCastle.Cryptography2.7.0 supplies the declared crypto operations. Provider capability255 is backed by42 native differential groups in JIT/AOT, actual emitted ABI/name/init/callback retention/native ASN1 storage checks and subsequent focused SHAKE/critical-certificate checks. No native TLS/crypto product backend is introduced. [Contract](provider-contract.md), `artifacts/provider/receipt.json`, `artifacts/provider/emitted-probe/receipt.json`.
- Final214654 exact-profile compilation of all175 C units finds178 external libcrypto imports, all covered by183 exact typed declarations, zero missing: `artifacts/provider/closure/20261023-214654-406d9780/receipt.json`.
- Actual BC SHAKE256 XOF repairs the observed original hash40-versus-native45 gap. Native/emitted JIT/AOT probes pass; unchanged original C hash test executes45 RAW/JIT assertions in `artifacts/provider/original16-shake-proof/raw-jit.json`. Old40 failure is preserved. Critical Netscape and intermediate EKU/CRLDP recognition are explicitly decoded, retaining translated chain-intent validation; original intent1783 and time245 match native.
- Four separate JIT/AOT with/without Linux RID lockfiles retain the exact BC2.7.0 content hash. Concurrent locked restores, locked builds and actual locked NativeAOT SHAKE execution pass without lockfile mutations: `artifacts/provider/lock-policy/receipt.json`.
- Complete175-unit raw+processed publication first passed [210200](../artifacts/campaign/20261023-210200-74eee32f/receipt.json). [214654](../artifacts/campaign/20261023-214654-406d9780/receipt.json) adds183 bindings, SHAKE and the final DHE caller repair. Both canonical forms build without generated C# edits.
- Historical canonical ABI raw/processed×JIT/AOT receipt: `artifacts/campaign/20261023-211314-1e36e6dc/receipt.json`. Native/emitted sizes/alignments/offsets/bitfield bytes, actual function-pointer identity and distinct-thread errno agree. Current183-profile qualification is recorded above.

## Historical managed execution and delivery

- [Recorded214654 matrix1076/1076 pass](../artifacts/managed/final-current-product-214654/report.json): each raw/processed×JIT/AOT variant has1 consumer,82 compatibility/authentication,156 independent peer and30 lifecycle rows. Totals4consumer,328compatibility,624interop,120lifecycle; no failed/unrun rows. Actual negotiated IDs/versions, both roles, binary payloads, shutdown and lifecycle controls are asserted.
- Identity audit `artifacts/managed-contract/final-matrix-214654-identity.json` verifies8 actual application receipts and execution/runtime/provider/source/product/native/fixture hashes. `final-current-214654-r3-audit.json` additionally reconciles delivery. Twelve changing intermediate DLL/PDB copies are documented; executed outputs remain identical. Subsequent builds isolate intermediate artifacts per application.
- [Recorded214654 delivery12/12 pass](../artifacts/managed/final-delivery-214654/report.json): copied ordinary consumer in all4 variants and whole-assembly TrimmerRootAssembly AOT in both forms, each actual3forced TLS exchanges plus dependency/native-import/original-reference/notices audits. Actual runtime/output identity receipts are retained.
- Current root solution locked Release build passes: `artifacts/managed-contract/solution-final223542-base64.log` (two existing embedded-runtime unassigned-field warnings,0errors).
- Original discovery report remains443pass/629fail at `artifacts/managed/full-owning-matrix/report.json`.624 failures were an observer engine-name mismatch, corrected only by separate strict transcript/hash revalidation `artifacts/managed-contract/discovery-interop-observation-audit.json`. Five lifecycle expected-error assertions were corrected using native controls and independently30/30 rerun. The original failed report was not rewritten passed.

```sh
python3 s2n-tls/scripts/managed-qualification.py --suite consumer --suite compatibility --suite interop --suite lifecycle --form all --mode all --output /absolute/new-matrix-directory
python3 s2n-tls/scripts/managed-qualification.py --suite delivery --suite rooted-aot --form all --mode all --output /absolute/new-delivery-directory
```

## Original upstream assertions and active findings

- Selected16 unchanged original C controls pass all64 original assertion executions with exact matching native source/adaptation/count comparisons: `artifacts/provider/original16-expanded/20261023T191422Z-2c6e0696/all4.json`. No duplicate C emission/linking or manual generated edits. This214654 discovery is separate from the fresh257 C×4 campaign.
- All257 required original C units emit under214654: `build/managed-upstream/all-required-257-emission-214654.json`; independent source/object/dependency/tool/profile audit `artifacts/managed-contract/all257-emission-214654-r3-audit.json`. This proves emission only, with assertions explicitly unrun. A repaired compiler requires fresh objects rather than silently reusing old serialized C#.
- Original18 Python families preserve original source/assertions and parameter inventory,22,195 selected cases with zero S2N capability vetoes. Exact command mappings, immutable adapters, original AvailablePorts bounds, native peer/fixture/config hashes and strict process drain are recorded. Same-pin native CLI is distinct from the previous1.7.11 peer.
- Actual original SSLv2-format ClientHello case passes with65,519-byte binary echo and original assertions; `artifacts/managed-contract/python-frozen-identity-214654.json`. Focused receipts are not accepted as a whole family run.
- Test-only GNU getopt174scenarios/518observations and real binary stdfd/poll/FIONREAD/EINTR probes pass (`artifacts/managed-contract/{getopt,stdfd}.json`). Fragmented readiness markers are matched over actual adjacent pipe bytes without altering upstream assertions.
- Host GnuTLS configuration disabledTLS1.0/1.1 before networking. Explicit isolated test-only policy preserves original argv; native priority probes and actual original TLS1.0 payload replay pass. SSLv3Provider also requires a truthful1.0.2 installation prefix; exact focused native replay passes after setting it. Historical failures/interruption receipts remain. Reproduce isolated prerequisites with `python3 s2n-tls/scripts/test-prerequisites.py --fetch never` only when peer execution is drained (package extraction owns those binaries).
- Historical native/translated Python trials exposed549 authored provenance errors treating long command arguments as paths (ENAMETOOLONG), plus351 original OpenSSL/SHA384 unconditional skips. Those failed/skipped receipts remain unchanged. The repaired adapter preserves argv; the measured app-only PSK peer enables actual execution of all351 original cases. The complete strict native18 comparator now passes22,186 applicable cases; complete translated all4 remains open.
- A genuine translated original CLI buffered-send stack overflow is preserved in `artifacts/managed-upstream/python-final-214654-legacy-policy/sslv3-buffered-replay/`. Fixed automatic arrays inside loops were emitted as cumulative CLR stackalloc. `d0f0146de` reuses one lazy fixed allocation per lexical declaration/invocation while re-running every initializer; no recursion or provider substitute. Reduced old compiler reproduces stack overflow; new JIT/AOT and native produce identical output: `artifacts/compiler-discovery/automatic-array-runtime-v2/receipt.json`. Full175 retry223542 and the actual original replay in all four modes pass; broad original-family qualification continues.
- EVP_DecodeBlock repair08796c070 passes1,659 native vectors per runtime and the full47-group JIT/AOT suite. The original failure remains preserved.
- Original certificate test historically failed assertion482 versus native488 because production ASN1_STRING_to_UTF8 accepts invalid universal tag29. The test-only X509_NAME mutation native comparator is exact; the failure is in production conversion. All four failed original executions are retained in `artifacts/provider/original16-expanded/20261023T205925Z-71055145/`. The production repair now passes the native differential suite above. The unchanged original all4 refresh now passes488 assertions per variant; its policy entry was added only afterward.

## Shared changes and remaining gates

Validated local milestones: `f5b36fc6a` GNU cleanup/deprecated/TLS literals; `72011ea54` callback arrays and unused-header-inline pruning; `7aa3f97de` callback overrides/pointer initialization/real memory residency; `27c3194dc` cursor zero initialization; `d9ffe1a3c` mixed inferred arrays and real socketpair; `d75255530` duplicate real-object definition diagnostics; `70d61f9b7` SHAKE/critical certificate/lock policy; `d0f0146de` repeated automatic-array storage.

Focused shared suite prior to the latest array repair passes241unit+12functional, zero skips: `artifacts/campaign/20261023-221258-5f860c83/receipt.json`. Array repair additional tests above pass. A mistakenly selected verify action began regeneration and was cancelled before linkage/publication (`221218-bde7bba7/cancellation-reason.json`); current publication remained214654.

Reproduction passes in `artifacts/campaign/20261023-233001-6f4afff7/reproduction.json`: two complete175-unit warn/off fetch-disabled regenerations from an unrelated cwd, expected invalid-C failure preserving prior publication, and real authored ProjectReference edit pickup without regeneration. A private identical authored host and destination-only profile preserve qualified production files. Original authored sources and delivered current pointer remain unchanged.

Still required: finish all257 original C cases and18 Python families across raw/processed×JIT/AOT; matching native Python families without required skips or retry events; finish application/upstream requalification of the repaired final ASN.1 provider snapshot; and integrated recipe acceptance. The default preparation/immutable translation-closure plumbing has focused contract checks but has not yet completed its full automatic upstream pipeline. These are ongoing authorized work, not unavoidable external blockers, and P6 is not claimed.

Completed native Python strict families include externalPSK1,999/1,999 at
`artifacts/native-python/psk-measured-v3/full-family.json`, version1,525/1,525 at
`artifacts/native-python/retry-investigation-r4/version-full-strict.json`, and
HRR118 applicable original cases plus9 explicitly inapplicable nodes with separate
9 no-HRR controls under `artifacts/native-python/hrr-no-retry-r4/`. Full strict
happy-path8,840 now passes in the retained complete18 comparator; prior failures/reruns stay failed. Measured
same-stderr fragment joins explain all10 old strict happy failures in
`artifacts/managed-contract/native-happy-marker-cause-r4.json`.

Post-repair ABI all4 passes through the shared framework at
`artifacts/campaign/20261024-001718-6f72b210/receipt.json`, including emitted provider
JIT/AOT and canonical raw/processed JIT/AOT probes. Ordinary root solution locked
Release build and its three forced SSLv3/RC4, SSLv3/3DES, TLS1.3 loopbacks pass with
actual runtime/source/fixture hashes at
`artifacts/managed-contract/ordinary-solution-final-asn1/report.json`. This is a
processed JIT check; it does not replace the pending all4 application matrix.

All18 native Python comparator families now pass22,186 applicable original cases
with zero required skips or retries at
`artifacts/native-python/final-eighteen-strict-comparator.json`. The independent
`artifacts/managed-contract/native-eighteen-supervisor-audit.json` rechecks49,656
retained files and exact family/count coverage. Nine impossible-HRR parameter
combinations remain explicitly inapplicable with separately executed nine
no-HRR controls; they are never added to original assertion counts. Final
translated18-family execution in each of the four variants remains in progress.

## Raw RSA finding after the0158 snapshot

Private native differential checks confirm that the delivered raw private RSA
path rejects integer0,1,n−1 that native OpenSSL accepts. The exact negative native
return for out-of-range n also differs. Evidence is
`artifacts/provider/rsa-private/20261023T221932Z-efde53a5/production-private-edges.json`.
Production is held unchanged while the vetted BC blinded engine is retained
behind a narrow native-range input adapter. Passing0158 matrix/ABI/provider
receipts stay tied to their captured bytes; a changed provider requires fresh
qualification. Original raw/JIT cross-compatibility also has5 ticket-out process
failures under independent diagnosis; no common cause is claimed.

Retained recipe inputs are now sealed explicitly against receipt and consumer
source changes (`af8a1145c`). Ten no-protocol selection controls pass, including
rejection of pending/incomplete receipts and changes during acceptance. The old
selector accepted the pending/cache-drift controls; its red result and source
are retained in `artifacts/managed-contract/retained-input-seal-contract/`.
Complete specialist C/Python validation is still required after seal checks.


## Reviewed private RSA candidate and source boundary

The blinded-engine candidate and prospective permanent49th provider regression
each pass1,177 native-matching rows in JIT and NativeAOT:
`artifacts/provider/rsa-private-repair/20261023T223103Z-3879ea83/receipt.json` and
`artifacts/provider/rsa-regression-private/20261023T223521Z-44c72bc9/receipt.json`.
The candidate overrides only input conversion on the pinned BC private engine;
blinding, CRT fault checks and output conversion remain inherited. This is private
proof, not full49 or delivered-provider qualification. Production stays0158d36c
until batch006 naturally drains. The API source-reader window is established at
`artifacts/managed-contract/api-r4-pre-rsa-reader-window.json`.

The pending retained-input bundle is
`artifacts/managed-contract/final-upstream-223542/inputs.json`; actual selection
rejects its pending status before commands. Ten retained-input contract tests
were rerun with `python3 -B -m unittest discover -s s2n-tls/tests/Recipe -p
'test_retained_inputs.py'` and pass. No upstream acceptance was executed.


## Delivered RSA repair and full provider proof

Commit e2876b1a6 applies the exact two-file reviewed blinded-RSA repair and
permanent49th regression after the source-owning batch006 naturally drained.
Freeze `artifacts/provider/freeze-rsa-20261023T224649Z/receipt.json` preserves
all25 delivered provider inputs; ASN.1 repair0158d36c stays unchanged. Full49
groups pass in JIT and NativeAOT at
`artifacts/provider/final49-rsa-224649/receipt.json`; actual emitted183-binding
ABI probes pass both modes at
`artifacts/provider/emitted-probe-rsa-224649/receipt.json`. Independent review
checks312 source/proof/report/output files in
`artifacts/managed-contract/rsa-runtime-milestone-supervisor-review.json`.

The provider runner had passed relative artifact paths to children with another
working directory. Actual reports and fixtures are preserved under the root
`artifacts/provider/final49-rsa-224649/`; campaign-local copies are byte-identical,
with original/copy/output hashes in `report-path-derivation.json` beside the
provider receipt. No tests were rerun for the path correction. Commit659b3498a
resolves future artifact paths before child execution; bounded pre-output path
controls are retained at `artifacts/provider/provider-runner-path-control-2250/`.

New-source original C, CLI, application1076, delivery12 and Python18×4 execution
remain unrun. Old0158 controller results54/2/201 are historical; fresh raw JIT
and all4 assertion runs are mandatory. The captured main/delivery passes above
remain valid only for their captured source bytes.


The repaired-RSA root solution locked Release build passes with0 errors and72
existing compiler/analyzer warnings. Its ordinary processed-JIT consumer passes
three forced SSLv3/RC4, SSLv3/3DES and TLS1.3 loopbacks (six role observations,
1,027 binary bytes per direction, negotiated IDs and clean shutdown). Evidence:
`artifacts/managed-contract/ordinary-solution-final-rsa/report.json` and its
77-file independent audit. All18 actual output files have byte-identical retained
copies before any future canonical rebuild. This check does not close the full
application1076 or delivery12 gates for this provider.


## RSA+stdio scoped execution and new EC finding

CLI host freeze1e4b2c and commit f4465e175 preserve native stdout buffering and
normal exit flushing without changing the original C or shared runtime. Fresh
CLI8 identity audit is `artifacts/managed-contract/final-rsa-stdio/cli-eight-deep-audit.json`.
The original five ticket-output failures now pass, and full raw/JIT cross family
passes468/468 with zero skips/reruns at `cross-full-raw-jit.json` in that directory.
`cross-full-native-comparison.json` checks14,924 retained files and exact native
case coverage. Sole Python matrix PID1648399 adopts only that fresh whole-family
receipt. Remaining72-leaf and application gates remain open; main raw/JIT
bad-padding-3des has an actual BAD_MESSAGE/expected INTEGER_OVERFLOW mismatch in
`artifacts/managed/final-provider-rsa-stdio-main/00501.log`, under native-fixture diagnosis.

Original PSK1021 and client_key_exchange23229 assertions pass EACH in all four
variants against both source freezes. The479-file audit is
`artifacts/provider/final-rsa-original-preflights-2313/receipt.json`.
Same required-C controller PID1606362 now records10/0/247, all fresh raw JIT/all4;
old0158 results remain historical. No duplicate C emission/controller was started.

Shared framework ABI command `bash s2n-tls/scripts/test.sh --suite abi --profile
compat --form all --mode all --tools reuse --fetch never` passes all11 commands
at `artifacts/campaign/20261024-022207-747d11cc/receipt.json`. Root builders drained.
These checks do not cover the subsequently measured EC semantics gap:
`artifacts/provider/ec-public-contract-private/20261023T233124Z-f06a1d30/receipt.json`
retains1080 native/current rows and576 discrepancies. The private rolled-up repair
is in progress; production f80f4238 remains unchanged and P1/P4/P6 remain open.

Authorized old-application compilation-cache maintenance removed only7128 exact
reviewed files (3,982,209,024 allocated bytes), preserving1921 actual outputs and
runtime/product/receipt files. `artifacts/managed-contract/old-application-cache-cleanup-supervisor-postflight.json`
checks all absent candidates, unchanged protected files and14,256 journal records.
The writer reused the sha256 field for record digest; verification explicitly
reconstructs its candidate hash from the immutable reviewed inventory before
checking the original chain. Original journal and receipts remain untouched;
future writers use distinct file_sha256/record_sha256 fields. This is storage
maintenance, not qualification credit.


API r4's continuation ended with an automated content flag. Recovery r5 verifies
that the actual parent/main/matrix/observer PIDs are absent and no parent wait
status explains the exit. The original main progress retains886 published rows:
885passed/1failed,190 required rows unpublished, seven application receipts and
no final report. The original Python matrix retains1passed/1running/70unrun;
orphan pytest1648572 still owns workers/endpoints. Original statuses are untouched.
Exact19 live identities and unchanged25 provider/host inputs are archived at
`artifacts/managed-contract/api-r5-recovery-20261023T2341Z/receipt.json`.
No duplicate controller, restart, signals or source mutation was used for recovery.

Closed root ABI cache maintenance reclaimed111,722,496 bytes from189 exact obj
binary/cache files, under exclusive kernel write leases. All1,239 other own-run
files (including sources, tools, metadata and every bin/publish output) remain
unchanged;378 journal records independently verify at
`artifacts/managed-contract/root-abi-closed-cache-2340/postflight.json`.
