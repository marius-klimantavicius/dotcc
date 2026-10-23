# Managed libcrypto boundary

This is a provider implementation and source contract, not a claim that the
translated s2n product or P1 is qualified. Product ABI binding and the emitted
native/managed comparison require the supervisor's actual output.

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
secret for DH_compute_key; s2n applies its own leading-zero padding. NIST ECDH
uses uncompressed SEC1 points and raw fixed-width shared secrets; X25519 uses
32-byte little-endian encodings and rejects an all-zero secret. ECDSA signatures
are DER sequences, not P1363. DER parsers advance input pointers by consumed
bytes; s2n retains its trailing-certificate-byte policy.

All exported C boundaries must contain exceptions and return the documented
error/null result. Failure is never recorded as success, and unsupported
operations do not fabricate objects. Do not advertise an incomplete X509 or
other capability just because primitive tests pass. Managed primitive tests,+native differential vectors, actual emitted ABI and end-to-end TLS are separate
gates.
