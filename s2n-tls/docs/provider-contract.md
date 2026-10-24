# Managed libcrypto boundary

This records the implemented provider boundary and its independent evidence.
All 183 typed overrides compile in the supervisor's raw and processed 175-unit
product, `20261023-223542-f7e7054e`, including the required SHAKE branch. The
actual emitted provider probe passes independently in JIT and NativeAOT.
Provider tests, emitted ABI checks and product TLS qualification remain separate
gates; this document does not pass the campaign.

The current production runtime freeze is
`artifacts/provider/freeze-ec-20261023T235817Z/receipt.json`. All 50 provider
groups pass in JIT and NativeAOT with its exact sources; the actual emitted
183-binding ABI probe also passes both modes. The 25 production inputs,
including `config/provider.json`, remain byte frozen. Prior application results
under the ASN.1-only and RSA freezes remain historical until fresh source-aware
execution against this EC repair and the owning API's final fixture freeze.

The declaration reference is `openssl-legacy` in `config/inputs.json`: actual
OpenSSL 1.1.1w headers. Its `OPENSSL_VERSION_NUMBER` describes those declarations.
Do not define AWS-LC, BoringSSL, LibreSSL or FIPS identity macros. The product
provider identity is `S2N_DOTCC_MANAGED_PROVIDER`, defined explicitly by the recipe.
`config/provider.json` is authoritative for dependency pins and compiler features.

The provider project reads the NuGet version from that JSON. Its pinned
BouncyCastle.Cryptography dependency is MIT licensed. Its package repository
commit is recorded separately from the s2n and header pins. Only cryptographic
primitives and ASN.1/PKIX facilities are used; no Bouncy Castle TLS engine is used.
The [upstream release announcement](https://www.bouncycastle.org/resources/new-release-bouncy-castle-c-2-7-0/)
identifies the selected release. Framework acquisition of the source/license is
performed by `tests/Provider/run.py`. Existing usable references are preserved.

## Headers, layout and imports

`s2n_hash_state` uses `s2n_evp_digest { const EVP_MD *md; EVP_MD_CTX *ctx; }`.
MD5/SHA headers supply digest sizes; this pin's selected hash implementation does
not access the fields of `MD5_CTX`, `SHA_CTX`, `SHA256_CTX` or `SHA512_CTX`.
Do not translate a different hash engine to avoid the EVP boundary.

With the selected headers, EVP_MD_CTX, EVP_CIPHER_CTX, EVP_PKEY/CTX, BIGNUM, DH,
EC_KEY/GROUP/POINT, X509, X509_STORE and X509_STORE_CTX are opaque handles.
The provider owns their state and reference counts. A borrowed child handle
cannot outlive its parent unless the API explicitly returns an owned reference.
Native storage is required where s2n reads fields: notably `ASN1_STRING` and
`GENERAL_NAME` (`type` and `d` union), plus any exposed OCSP structures in the
selected source branches. STACK_OF accessors are header macros/static inlines
which lower to the generic `OPENSSL_sk_*` import family. Compare actual emitted
layouts with `tests/Abi/provider-layout.c`; handwritten mirrors alone do not pass.

`tests/Abi/inventory.py` records source call candidates and header includes.
These candidates include macros and inactive branches: they are explicitly not
the final unresolved-symbol inventory. The native archive's undefined symbols
and emitted import manifest must be reconciled after the matching builds.
Required families include EVP digest/cipher/PKEY, BN/DH, EC, RAND, ASN.1,
DER/PEM, X509 store/chain/CRL, OBJ, ERR and generic stacks. Entropy is at actual
RAND entry points; the retained no-op s2n random callback API is not the boundary.

External declarations retain their exact C signatures. Typed function overrides
call authored wrappers in the translated class when a return pointer requires
the actual emitted struct type. Those wrappers cast opaque pointers and call
`Managed.S2nTls.Crypto.OpenSslAbi`. The independent provider project must not
reference generated C# or guess generated layouts. Wrappers are authored source
referenced by both raw and processed products; do not edit emitted files.

## Required staged adaptations (supervisor owned)

Preserve reference sources. Require each old string below to match exactly once.

In `crypto/s2n_composite_cipher_aes_sha.c`, replace:

```c
#if !defined(LIBRESSL_VERSION_NUMBER) && !defined(OPENSSL_IS_BORINGSSL)
```

with:

```c
#if !defined(LIBRESSL_VERSION_NUMBER) && !defined(OPENSSL_IS_BORINGSSL) && !defined(S2N_DOTCC_MANAGED_PROVIDER)
```

This declares unsupported acceleration through the existing null-accessor path.
The ordinary s2n CBC and HMAC implementations remain active for all required
suites. This is not permission to remove AES-CBC suites or legacy branches.

In `crypto/s2n_libcrypto.c`, `dotcc_provider.h` must be visible (the recipe may
force-include it). Replace the unique function body of
`s2n_libcrypto_get_version_name` with the following guarded body:

```c
{
#if defined(S2N_DOTCC_MANAGED_PROVIDER)
    return dotcc_crypto_provider_name();
#else
    return SSLeay_version(SSLEAY_VERSION);
#endif
}
```

Replace the unique expression `return !is_other_libcrypto_variant;` with:

```c
#if defined(S2N_DOTCC_MANAGED_PROVIDER)
    return false;
#else
    return !is_other_libcrypto_variant;
#endif
```

In the unique `s2n_libcrypto_validate_runtime` body, put a managed branch before
the original native checks, with the original body in `#else`:

```c
#if defined(S2N_DOTCC_MANAGED_PROVIDER)
    RESULT_ENSURE_EQ(dotcc_crypto_provider_abi(), DOTCC_CRYPTO_PROVIDER_ABI);
    RESULT_ENSURE_EQ(dotcc_crypto_provider_capabilities() & DOTCC_CRYPTO_REQUIRED_CAPABILITIES,
                    DOTCC_CRYPTO_REQUIRED_CAPABILITIES);
    RESULT_GUARD(s2n_libcrypto_validate_expected_version_prefix("DotCC managed s2n crypto"));
    return S2N_RESULT_OK;
#else
    /* Complete original body, unchanged. */
#endif
```

This checks a real provider identity/capability contract. Returning the OpenSSL
version number from managed code merely to pass s2n's vendor check is forbidden.
The capability mask remains incomplete until corresponding bindings pass tests.
`s2n_get_openssl_version` remains the reference declaration version; its name and
meaning must be documented to low-level API consumers.

## Operation semantics

Digest contexts clone current state, finalize once, and reset explicitly. Combined
MD5+SHA1 returns MD5(message)||SHA1(message). Legacy RSA signs those 36 raw bytes
with PKCS#1 type-1 padding and no ASN.1 DigestInfo. Standard digest signatures use
their proper DigestInfo. RSA-PSS uses MGF1 with the signing digest and a digest
length salt. RSA encryption uses PKCS#1 type-2 padding. The actual s2n decrypt
wrapper requests RSA_NO_PADDING, then performs its own constant-time PKCS#1
unpadding with random fallback in translated code. That branch must remain.

RC4 continues one keystream across Update calls. CBC never adds/removes padding;
s2n owns TLS padding and record IV selection. In-place CBC decryption preserves
the incoming final ciphertext block as the next IV. AES-GCM and ChaChaPoly reject
bad tags and clear the destination. The EVP adapter must implement the actual
Init/AAD/Update/Final/tag-control sequence used by s2n, including empty records.

DH uses unsigned big-endian integers, validates peers and returns the unpadded
secret and its actual length for DH_compute_key. Pristine pinned s2n incorrectly
left-pads the result to the modulus width (the native leading-zero control
fails with a 256-byte modulus and one-byte secret). The supervisor's shared
staged TLS <=1.2 repair requests computed-size padding at the two TLS callers,
retaining the actual secret length. The generic padding helper's body and
contract stay unchanged; this provider does not pad DH_compute_key's result.
NIST ECDH
uses uncompressed SEC1 points and raw fixed-width shared secrets; X25519 uses
32-byte little-endian encodings and rejects an all-zero secret. ECDSA signatures
are DER sequences, not P1363. DER parsers advance input pointers by consumed
bytes; s2n retains its trailing-certificate-byte policy.

All exported C boundaries must contain exceptions and return the documented
error/null result. Failure is never recorded as success, and unsupported
operations do not fabricate objects. Do not advertise an incomplete X509 or
other capability just because primitive tests pass. Managed primitive tests,
native differential vectors, actual emitted ABI and end-to-end TLS are separate
gates.

## Selected call closure and verified groups

`tests/Abi/audit-provider-closure.py` privately compiles all 175 frozen staged
units with the translation receipt's exact declaration flags, then reconciles
undefined symbols against the pinned native archive. For 214654 this measures
178 libcrypto imports, all covered by the 183 typed overrides; there are no
missing crypto bindings. The additional declarations are the three managed
identity functions, `OBJ_obj2nid` used by header paths, and the unselected
`EVP_MD_CTX_set_pkey_ctx` variant. The inventory is declaration/call closure,
not evidence that every argument or error branch has executed.

Reproduce the inventory without rebuilding shared tools:

```sh
python3 s2n-tls/tests/Abi/audit-provider-closure.py --campaign s2n-tls/build/campaign/20261023-214654-406d9780
python3 s2n-tls/tests/Provider/run.py --fetch never --mode all
python3 s2n-tls/tests/Abi/run-provider-probe.py --compiler s2n-tls/build/campaign/20261023-214654-406d9780/tools/DotCC/dotcc.dll --mode all --work s2n-tls/build/provider/independent-emitted-probe --artifacts s2n-tls/artifacts/provider/independent-emitted-probe
```

Private output is under `build/provider/`; receipts and differential vectors
are under `artifacts/provider/`. `closure/20261023-214654-406d9780/receipt.json`
includes commands and per-import source units. `provider-binding-contract.json`
in `tests/Abi/` records the reviewed signatures and selected native inventory.
The recorded full provider run passed 42 groups in JIT and NativeAOT before
the later SHAKE/intermediate/base64 additions. Focused receipts qualify those
subsequent changes; the current runner declares 47 groups.

| Group | Executed evidence |
| --- | --- |
| Digest | MD5/SHA1/SHA224/SHA256/SHA384/SHA512 and MD5+SHA1 clone, diverging forks, reset, empty input, native bytes; actual emitted copy/final/free. |
| RC4/CBC | RFC6229 and native RC4 continuity; AES128/256 and 3DES CBC chaining, in-place decrypt, weak DES keys, no provider padding. |
| AEAD | AES128/256 GCM and ChaChaPoly native records, empty records, AAD, tag controls, rejection and cleared plaintext. |
| RSA | 512/1024/2048 keys, PKCS1 encryption, raw decrypt block, legacy combined/SHA224 signatures, PSS, retained context keys, restricted RSA-PSS PKCS8/SPKI hash/MGF/minimum-salt enforcement with native cross-verification. |
| DH/EC | Native DH secret/invalid peers and parameter flags/ownership; P256/P384/P521/X25519 derivation and encodings; native ECDSA DER signatures; actual EVP paramgen/keygen/derive sequence. |
| X509 | Native names/hash/NIDs/extensions and actual emitted SAN union reads; trust/time/partial chains, critical anchor and leaf rejection, MD5/SHA1 chains, legacy purpose rules, DER cursor failures, CRL and OCSP below. |

The advertised capability mask is now `0xff`: every declared group has actual
managed/native evidence and all selected crypto imports bind. The independent
emitted probe checks ABI 1, the managed identity prefix, real initialization,
the mask, native layouts, digest calls and retained callbacks through forced GC
and clearing, in both JIT and NativeAOT. This mask is not a general OpenSSL API
compatibility claim or a passed product handshake matrix.

## Verification flags, callbacks and revocation scope

The actual s2n path sets only CRL_CHECK (`4`), CRL_CHECK_ALL (`8`), PARTIAL_CHAIN
(`0x80000`) and NO_CHECK_TIME (`0x200000`). Those are implemented. The void
context setter records unsupported flags as invalid so later verification fails;
the returning parameter setter fails immediately. Unsupported controls cannot
silently turn into successful validation. OCSP_basic_verify supports the exact
s2n flag value `0`; other flags fail. Initialization accepts only the actual
load-strings/add-ciphers/add-digests bits (`0x0e`), with no settings or engines.

s2n's retained verification callbacks override certificate date errors (9/10)
or CRL date errors (11/12/15/16). The tests retain context/CRL ownership after
caller release, force GC, invoke the saved callback, clear it, and prove expiry
is rejected again. A successful callback override preserves the native error
value observable through get_error. Unsupported general callback-based overrides
of arbitrary trust, signature or critical-extension errors are outside the
declared API; the actual s2n callbacks do not request them.

Direct CRLs are a mandatory actual s2n API path. They are implemented rather
than excluded: issuer/authority key identifier, signing key usage/signature,
time, all-chain checks including root, critical CRL and entry extensions,
removeFromCRL, user/CA/attribute scope, and distribution-point matching.
Differential tests execute critical and noncritical user scope, wrong CA and
attribute scope, named-point mismatch, unknown critical extensions, critical
CRL number and entry, root checking and callback-overridden expiry. Unknown
critical extensions fail; they are not stripped or declared supported by a
blanket verification flag.

The legacy Netscape certificate-type BIT STRING is recognized by an explicit
PKIX checker, matching native OpenSSL. Its client/server/CA bits are enforced
by the authored X509_check_purpose calls made by translated s2n after chain
validation. Native comparisons cover critical accepted and denied purpose
bits, so recognizing this extension does not bypass intent validation.
The focused post-integration regression is reproducible with:

```sh
python3 s2n-tls/tests/Provider/run.py --fetch never --mode all --filter '(abi|audit)/x509' --artifacts s2n-tls/artifacts/provider/netscape-critical
```

Its separate receipt passes four X509 groups in each runtime and records the
38 unselected primitive groups explicitly. The earlier full 42-group receipts
remain separate; filtered success cannot be mistaken for a full suite run.

The unchanged original intent test subsequently exposed critical EKU on an
intermediate: BC's PKIX validator recognizes this on the leaf, but leaves it
unresolved on intermediate certificates. The explicit checker now decodes
intermediate EKU and CRL distribution points. Translated s2n still enforces
EKU over every chain certificate, and VerifyCrls processes distribution scope;
these extensions do not acquire a blanket critical-extension bypass. Invalid
CA intermediates report native error 24. Five focused X509 groups, including
all original intent-chain fixtures, pass native comparison in JIT and NativeAOT
under `artifacts/provider/critical-intermediate/`. Recompiling the unchanged
original intent C harness then passes exactly 1783 assertions, matching native;
its preserved earlier failure at assertion 763 remains under
`artifacts/provider/original16-x509-probe/`, and the repaired execution/hash
receipt is `artifacts/provider/original16-x509-repaired/raw-jit.json`.

The selected native flags reject indirect and reason-partitioned CRLs; matching
that rejection is a scoped exclusion, proven against this pin. EXTENDED_CRL_SUPPORT
and USE_DELTAS are never requested by actual s2n and are not supported provider
flags. The pin nevertheless accepts a standalone delta under its default flags;
the provider matches this observed behavior. Base/delta merging is excluded,
not standalone delta acceptance. General RFC3779 resource and proxy-certificate
validation, relative-name distribution-point variants and overlapping multi-CRL
reason coverage have not received differential qualification; these are explicit
remaining X509 compatibility gates if exercised, not an assertion that the
selected native source disables those branches.

OCSP tests compare good/revoked/unknown statuses and dates with native, reject
bad signatures and empty response data, accept directly issued OCSP delegates
with the required EKU and reject missing/wrong EKU. Signature, trusted signer
chain and authorization remain separate checks. The translated s2n code keeps
its own matching CertID/status/freshness policy.

The supervisor's receipts track actual s2n init and owning API execution,
the required 36-suite/version matrix including SSLv3 legacy suites, upstream
parser/validator rejection tests, resumption/nonblocking/lifecycle and delivery
qualification. The provider's independent receipts do not substitute for those
runs or close the outstanding whole-upstream gate. No native libcrypto is
imported by this provider assembly.

## SHAKE branch repair from original C assertions

The original `s2n_hash_all_algs_test.c` initially executed 40 assertions in the
translated product versus native's 45. Its loop silently continues past an
unavailable algorithm, so a successful process exit did not pass this case.
The missing branch was SHAKE256 with 64 output bytes. It is now implemented
with the pinned BC SHAKE XOF, including copy/reset and variable output length;
the truthful feature definition is `S2N_LIBCRYPTO_SUPPORTS_SHAKE=1`.
`EVP_shake256` and `EVP_DigestFinalXOF` extend the override profile to 183.
The OpenSSL descriptor retains native's 32-byte default size; the actual s2n
branch explicitly finalizes 64 bytes. Ordinary RSA signature algorithms do
not acquire a SHAKE DigestInfo.

The focused native differential cases pass in JIT and NativeAOT, including
empty input, diverging copies, 0/1/32/64/137-byte XOF widths, default finalization,
buffer guards and rejection of non-XOF contexts. The actual emitted C probe
also executes SHAKE update/copy/FinalXOF and matches native in both runtimes:

```sh
python3 s2n-tls/tests/Provider/run.py --fetch never --mode all --filter shake --artifacts s2n-tls/artifacts/provider/shake
python3 s2n-tls/tests/Abi/run-provider-probe.py --compiler s2n-tls/build/campaign/20261023-212541-5df78e58/tools/DotCC/dotcc.dll --mode all --work s2n-tls/build/provider/shake-emitted --artifacts s2n-tls/artifacts/provider/shake-emitted
```

The regenerated 175-unit product `20261023-214654-406d9780` includes this C
branch; the matching native inventory now has 178 imports and no missing
bindings. The unchanged original `s2n_hash_all_algs_test.c` also passes all 45
assertions in a raw JIT harness using the complete SHAKE-enabled product
snapshot `20261023-213416-7ad33661`, with native count equality and source hash
recorded in `artifacts/provider/original16-shake-proof/raw-jit.json`. The
214654 selected16 retry and remaining forms/modes have separate receipts.
Historical old-profile results of 40 assertions remain failures and their
frozen objects and receipts are preserved.

The selected16 original C controls subsequently passed all four raw/processed
JIT/NativeAOT modes on 214654: 64 executions with exact native assertion-count
equality, original source hashes and empty test adaptations. This includes 45
assertions in every hash-test mode, CBC's profile-matching 238 assertions and
1783 X509 intent assertions. The aggregate is
`artifacts/provider/original16-expanded/20261023T191422Z-2c6e0696/all4.json`.
These remain bounded discovery results; the new 223542 compiler/product and
all 257 required C cases need their separate qualification.

## Base64 block contract repair

The old BCL string conversion differed from the selected native
`EVP_DecodeBlock`: it accepted interior whitespace, trimmed a leading newline,
and rejected native block padding/EOF forms. The frozen failure is
`artifacts/provider/base64-audit/frozen-mismatch.json`; the authored regression
also failed before repair against a frozen provider DLL, without rebuilding
the provider (`authored-before-fix.log` in the same directory).

The block decoder now matches the pinned native byte mapping, space/tab
prefix handling, line-ending/EOF suffix handling and zero-valued `=` slots.
It preserves complete-block writes before malformed input and does not add an
error-queue entry when native format rejection adds none. The translated s2n
stuffer still owns padding/cursor handling. No override or C feature changed.

The focused encoder and decoder groups pass in JIT and NativeAOT, comparing
all 256 byte values in each input slot and at both ends, valid encodings,
malformed padding/whitespace, output guard bytes, partial writes and error
queues against actual pinned OpenSSL calls. There are 1659 vectors per mode.

```sh
python3 s2n-tls/tests/Provider/run.py --fetch never --mode all --filter base64 --artifacts s2n-tls/artifacts/provider/base64-contract
```

The receipt is `artifacts/provider/base64-contract/receipt.json`. It qualifies
the provider repair; new full-upstream and product execution gates remain
separate.

## Restore locks

Provider restores select separate authored locks for ordinary and PublishAot
builds, further separated when RuntimeIdentifier is propagated. The four
qualified profiles are `packages.lock.json`, `packages.linux-x64.lock.json`,
`packages.aot.lock.json` and `packages.aot.linux-x64.lock.json`. All preserve the
exact BC 2.7.0 version and the same NuGet content hash recorded in provider.json.
The implicit NativeAOT compiler package belongs only to the AOT locks. Ordinary
compilation no longer removes it from an AOT lock or mutates a different mode's
lock. Other RIDs get distinct paths and require their own qualification.

`python3 s2n-tls/tests/Provider/check-locks.py` runs all four locked restores
concurrently with separate assets directories, builds locked JIT and AOT/RID
profiles, publishes and executes focused NativeAOT SHAKE tests with locked
restore, and verifies that every authored lock hash remains unchanged. Its
receipt is `artifacts/provider/lock-policy/receipt.json`. `--update` is an
explicit lock-generation operation; the normal check never requests changes.

## Additional APIs in original C tests

The production provider remains frozen at 183 typed bindings and capability
mask 255. Additional calls in original upstream tests belong to the separate
`tests/Provider/UpstreamSurface/CryptoProvider.Upstream.csproj` assembly. It links
the identical production C# and OID resource, uses the same exact BC dependency
and four byte-identical locks, and adds only authored test APIs. Its canonical
`S2nTls` partial bindings resolve existing emitted imports; neither original C
nor generated C# is edited to introduce these bindings.

`provider_snapshot.snapshot_provider(project, bindings, destination)` preserves
the evaluated relative source/resource/configuration layout in a private tree.
It checks copied bytes and original inputs before and after copying, omits
build caches and test checks, and returns the frozen project, extra bindings,
and original/frozen input hashes. The upstream builder's `--provider-project`
and repeatable `--provider-binding` switches select this project for an explicit
harness. Each actual application receipt records its own provider snapshot.
Later authored test-surface changes cannot change those frozen applications.

The observed first missing test API was `OpenSSL_version`, reached at compile
time through the `SSLeay_version` macro in `s2n_build_test.c`. Selector 0 returns
the existing actual managed provider identity. Other selectors return null and
record a provider error because OpenSSL build selectors have no defined managed
equivalent in this test surface. The header version never identifies the managed
runtime. This original test's default environment exits after one assertion;
its optional vendor-specific CI preset branch is separate from the provider
identity contract. A managed name must not be replaced with an OpenSSL vendor
claim to satisfy that branch.

The private-snapshot identity check passes 35 assertions in JIT and NativeAOT,
including the exact provider-name pointer, ABI/mask, unsupported selectors and
error consumption, and unchanged context count. The command is:

```sh
python3 -B s2n-tls/tests/Provider/UpstreamSurface/Checks/run.py
```

Its receipt is
`artifacts/provider/upstream-surface-check/20261023T204158Z-4ca44d59/receipt.json`.
This focused identity result does not qualify the remaining original C cases.

The unchanged original `s2n_build_test.c` subsequently passes all four variants
with its exact matching native count of one. The retained repair manifest is
`build/provider/required257-223542-repairs/build-version/expanded-executables.json`;
its four-mode receipt is
`artifacts/provider/original16-expanded/20261023T204431Z-67a7c869/all4.json`.
The earlier failed production-reference build remains recorded by the live
controller; this separate successful repair does not rewrite that attempt.
`tests/Provider/UpstreamSurface/policy.json` catalogs only observed and repaired
extra binding selections, independently of production provider metadata.

The next observed compile failure, `s2n_certificate_test.c`, requires
`X509_NAME_new/free/add_entry_by_NID` and `X509_set_subject_name`. The isolated
extension uses real ASN.1 string storage and BCL DER encoding, preserves name
entry ownership and RDN insertion, and copies the subject into the certificate's
owned name cache. The selected native setter retains the original certificate
DER/signature cache; this is independently measured rather than inferred from
the setter name. The deliberately invalid ASN.1 tag 29 remains an actual tag 29
for s2n's CN-type checks. BC rejects that tag in its certificate parser, so it
must not be relabeled as a supported string to make parsing succeed.

Pinned native, JIT and NativeAOT name mutation transcripts agree on exact tags,
bytes, name DER, unchanged certificate DER cache, and copied-name survival after
freeing its source. The snapshot test also verifies context cleanup and the
identity assertions. Run `Checks/run.py --names`; the receipt is
`artifacts/provider/upstream-surface-check/20261023T205844Z-3a12e1e0/receipt.json`.
Those name boundary checks do not qualify production string conversion. The
first original certificate execution exposed a production defect in
`ASN1_STRING_to_UTF8`: it used cached text and accepted an unsupported tag.
The failed executions remain historical evidence.

## ASN.1 string conversion and final-source refresh

`ASN1_STRING_to_UTF8` reads the actual native Type/Data/Length fields. It accepts
the same tags as pinned OpenSSL: UTF8String (12), single-byte strings
(18/19/20/22/23/24/26), UniversalString (28), and BMPString (30). Unsupported
types return -1 without writing the output pointer or adding an error. Invalid
lengths and encodings follow the native output-pointer/error behavior. The
decoder also preserves the pinned implementation's legacy five/six-byte UTF8,
BMP surrogate, and high UniversalString behavior.

The regression compares 3131 vectors against actual native calls in each of
JIT and NativeAOT, including mutated native fields, the full tag range, all
single-byte values, malformed encodings and lengths, output guard state, and
error consumption. The private candidate passed before the sole production
file was replaced at the drained controller boundary. The final source-freeze
receipt is `artifacts/provider/freeze-asn1-20261023T211648Z/receipt.json`;
`artifacts/provider/final48-asn1-211648/receipt.json` records all 48 native
differential groups passing in both modes with that source. Production still
has the same 183 typed bindings, capability mask, dependency pin and locks.

Under that ASN.1-only freeze, the unchanged original certificate test passed all
four forms/modes with exactly 488 native assertions each. Its receipt is
`artifacts/provider/original16-expanded/20261023T211832Z-6c2a4f05/all4.json`.
Only after this actual pass was its Names binding added to the test policy.

`expand-original-controls.py --refresh-provider --provider-freeze RECEIPT`
copies retained linked raw projects, preserves every generated C# byte, selects
the reviewed production source or a new immutable test-provider snapshot, and
builds and executes all four variants anew. Raw JIT identity reuse is disabled
in this mode. It performs no C emission or C-to-managed linking. The controller
uses the same mode for prior batches, archives their exact old progress, and
resets their current qualification before adopting or rerunning final-source
results. `--reuse-linked-manifest` selects an already linked boundary repair;
`--adopt-expanded` accepts its fresh four-mode result only after closure audit.

The bounded refresh control requalified the certificate and two previously
passing cases (12 actual executions), verified 506 retained files, and rejected
an old-provider identity against the final freeze. Its receipt is
`artifacts/provider/final-source-refresh-control-212202.json`. These results
remain a subset of the required 257-case C gate; historical provider results
and partial aggregates cannot close that gate.
Each later source freeze resets the full batch selection and retains each
case's latest raw-manifest provenance, including separately adopted repairs.
Stale prior active-case subsets cannot omit those cases from requalification.

## Native RSA input and EVP decrypt contract

Native raw private RSA accepts every integer in `[0,n)`, including 0, 1 and
`n-1`, and accepts inputs shorter than the modulus width. BC's generic cipher
input conversion deliberately restricts that range. The provider supplies a
narrow `RsaCoreEngine` subclass overriding only `ConvertInput`, and passes it
to BC's existing `RsaBlindedEngine`. BC still performs private-operation
blinding, CRT fault checks and output conversion. No private exponentiation or
blinding algorithm is reimplemented.

The EVP decrypt adapter checks operation support and initialization before
AUTOARGLEN handling. A size query succeeds without reading ciphertext; a buffer
smaller than `RSA_size` returns 0 before any private operation. Wrong operation
and unsupported contexts return -1 and -2 respectively. A private-operation
failure returns -1 and preserves the caller's output length and bytes.

The permanent 49th regression compares 1177 native rows per runtime, covering
lengths 0/1/size-1/size/size+1, values 0/1/n-1/n/n+1, malformed PKCS1 blocks,
queries, undersized buffers, full guard bytes, context ownership and prior-error
preservation. Exact native and synthetic managed error-code queues are retained
separately by the private proof; the provider's existing error-presence contract
is unchanged. Evidence is in
`artifacts/provider/rsa-private-repair/20261023T223103Z-3879ea83/receipt.json`,
`artifacts/provider/final49-rsa-224649/receipt.json`, and
`artifacts/provider/emitted-probe-rsa-224649/receipt.json`.

The full49 runner initially passed a relative artifact argument to a child
whose working directory was the repository parent. Its actual JIT/AOT reports
remain under `/home/marius/p/dotcc/artifacts/provider/final49-rsa-224649`.
`report-path-derivation.json` in the campaign artifact directory records those
original paths, byte-identical report/fixture copies and actual output hashes.
The runner now resolves the artifact directory before starting children.

The original-C controller naturally drained batch006 at 54 passing cases under
the prior provider before this repair was applied under its exclusive lease.
Those results require fresh raw JIT and all four executions against the new
freeze. C builds were held until the owning API host repair also froze; the
retained 257 C emissions and linked projects remain available for reuse.

Under the RSA and host freezes, the unchanged original `s2n_client_psk_extension_test`
passed all four executions with exactly 1021 native assertions each, using the
retained original policy testlib unit selected by the corrected support decoder.
The unchanged original `s2n_client_key_exchange_test` also passed all four with
exactly 23229 assertions each. Its additional public RSA APIs use real BC/BCL
operations in the isolated test assembly; the canonical typed wrappers have a
separate 277-row native differential in both runtimes. Only after original all4
acceptance was its RSA binding added to the policy catalogue.

`artifacts/provider/final-rsa-original-preflights-2313/receipt.json` records
the eight actual executions and a 479-file source/output/runtime audit. The
retained failed raw key-exchange project was copied with every generated C#
byte preserved; only project XML selected a frozen provider/binding snapshot.
`repair-linked-control.py` records the failed command, input manifest and fresh
raw JIT identity before the four-mode expansion. No C emission was repeated.
These two cases close neither the required 257-case C gate nor other P6 gates.
Their RSA-source executions are historical until the EC-source refresh qualifies
them again.

## EC public state and private roles

Generated and decoded EC keys now support the selected signing and derivation
operations with the same private material. Generated keys use the existing BCL
key export/import and signing operations; decoded private keys use the existing
BCL PKCS8 import and ECDH operation. Public verification reads the current public
point. Explicit public mutation retains NIST private material, so signing still
validates with the original public key while verification and peer derivation
observe the replacement. `EC_KEY_check_key` checks private/public consistency.

The selected TLS point setter follows the pinned native controls for compressed,
uncompressed, hybrid and infinity encodings. Prefix, length and field-range
failures retain an existing public point. For an initially absent point, the
native allocation can leave infinity even when parsing fails. An in-range
off-curve affine assignment returns
failure after changing the public point, as native does. Compressed-coordinate
recovery clears the prior error queue; other tested early failures preserve it.
The getter returns the current uncompressed encoding, including the single zero
byte for infinity. Empty-key output pointers and error behavior are checked
separately. Acquired EC views retain the key and have stable borrowed view/group
identities across repeated acquisition and release.

X25519's native setter replaces the key with public-only state. Existing
contexts observe that change; an own-key derivation or size query then fails
without overwriting output or length. Low-order peer derivation copies the
actual zero agreement result produced by BC's existing span operation before
returning failure, preserving length and adding no error. NIST derivation
supports the native truncated output contract, including a zero-capacity
failure without a new error.

The permanent 50th group compares 1223 native rows in each runtime. It covers
the generated/decoded roles, 22 point forms on all three selected NIST curves,
output guards and lengths, two seeded prior errors, empty keys, stable aliases,
retained owners and X25519 replacement/low-order behavior. Complete native and
synthetic managed numeric queues remain separate evidence; comparison uses
their documented error-presence and retention behavior. The private baseline,
private repair and permanent production-project checks retain every attempt.

The final runtime evidence is
`artifacts/provider/final50-ec-235817/receipt.json` and
`artifacts/provider/emitted-probe-ec-235817/receipt.json`. The ABI run rebuilds
JIT and NativeAOT against retained actual emitted C# and its project, verifies
their bytes before and after, and reruns the pinned native comparator. It does
not regenerate C or edit generated sources. The exact five-file milestone is
`artifacts/provider/milestone-ec-runtime-235817/manifest.json`.

The old RSA-source controller naturally drained batch002 at 26 passing cases.
The same exclusive controller then reset all 257 cases for mandatory fresh raw
JIT and all four executions against the EC/fixture epoch. Historical assertions
and linked sources remain preserved; none count as final EC-source passes.

## Qualified dependency storage

After an original case passes all four executions, the expander may share its
two exact pinned `BouncyCastle.Cryptography.dll` JIT output copies through a
private content-addressed store under `build/provider/immutable-dependencies`.
The helper verifies all four provider locks, NuGet's signed-package content
hash and signatures, and the actual DLL archive member. It copies the store
member into a new read-only inode; the global NuGet store is never linked or
modified. Every application path remains a regular file with identical bytes.

`qualified_dependencies.py` verifies the complete case output/runtime hashes
before and after deduplication, inspects process commands/descriptors/maps, and
requires a kernel write lease before replacing an inactive target with an
atomic hardlink. Busy or unsupported targets remain intact. The shared inode
must never be modified. Per-file receipts retain path, SHA, inode, link count,
role and released allocation. The bounded three-case proof preserves all 12
execution closures and relaunches six actual JIT applications:
`artifacts/provider/dependency-dedupe-closed3-2140/receipt.json`. Descriptor,
closed-descriptor mapping, symlink, mismatched-byte and unqualified-case
controls accompany that proof. This operation changes storage only and does
not qualify any additional original assertions.
