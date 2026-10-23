# Managed libcrypto boundary

This records the implemented provider boundary and its independent evidence.
The complete 181 typed overrides compile in the supervisor's raw and processed
175-unit product (210200). Provider tests, emitted ABI checks and product TLS
qualification remain separate gates; this document does not pass the campaign.

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
staged TLS <=1.2 repair retains the actual secret length; this provider does not
pad that result. NIST ECDH
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
undefined symbols against the pinned native archive. For 205255 this measures
176 libcrypto imports, all covered by the 181 typed overrides; there are no
missing crypto bindings. The additional declarations are the three managed
identity functions, `OBJ_obj2nid` used by header paths, and the unselected
`EVP_MD_CTX_set_pkey_ctx` variant. The inventory is declaration/call closure,
not evidence that every argument or error branch has executed.

Reproduce the inventory without rebuilding shared tools:

```sh
python3 s2n-tls/tests/Abi/audit-provider-closure.py --campaign s2n-tls/build/campaign/20261023-205255-fc99db78
python3 s2n-tls/tests/Provider/run.py --fetch never --mode all
python3 s2n-tls/tests/Abi/run-provider-probe.py --compiler s2n-tls/build/campaign/20261023-204707-1f228c70/tools/DotCC/dotcc.dll --mode all
```

Private output is under `build/provider/`; receipts and differential vectors
are under `artifacts/provider/`. `closure/20261023-205255-fc99db78/receipt.json`
includes commands and per-import source units. `provider-binding-contract.json`
in `tests/Abi/` records the reviewed signatures and selected native inventory.
The full provider runner passes 42 groups in JIT and NativeAOT:

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

Remaining product gates are the supervisor's actual s2n init and owning API
execution, required 36-suite/version matrix including SSLv3 legacy suites,
upstream parser/validator rejection tests, resumption/nonblocking/lifecycle and
delivery qualification. The provider's independent receipts do not substitute
for those runs. No native libcrypto is imported by this provider assembly.
