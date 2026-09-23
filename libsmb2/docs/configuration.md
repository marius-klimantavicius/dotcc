# Initial Linux x64 profile

This page describes the async product profile. [Kerberos and DFS](enterprise-client.md)
are implemented with enterprise qualification pending on Windows and Linux.

The source closure is fixed in [sources.json](../config/sources.json), command-line
definitions in [defines.json](../config/defines.json), and feature header in
[managed/config.h](../config/managed/config.h). The header freezes the original
Linux CMake configuration; managed translation does not rerun host feature
discovery. `__linux__=1` selects the Linux-shaped LP64 ABI and endian wrappers.
This is an explicit initial target, not a claim of portability to other ABIs.

| Area | Selected implementation |
| --- | --- |
| C/ABI | C17 plus evidenced GNU extensions; little-endian Linux x64 LP64. |
| Sources | 53 upstream library units, including portable crypto, sync/async/raw APIs and minimal share-enum wrappers. |
| Authentication | Built-in NTLMSSP or authored Kerberos.NET/Windows SSPI bridge. Native GSS/Kerberos libraries are not linked. |
| Dialects | SMB 2.0.2, 2.1, 3.0, 3.0.2, 3.1.1. |
| Signing | Upstream SMB2 HMAC-SHA256 and SMB3 AES-CMAC paths. |
| Encryption | Upstream AES-128-CCM; no AES-GCM profile is advertised. |
| Preauthentication | Upstream SHA-512 transcript/key derivation for SMB 3.1.1. |
| AES | `aes_reference.c`, selected by upstream `aes.c`; Apple accelerator not selected. |
| Entropy | Secure `arc4random_buf`/`getrandom` host functions required; deterministic PRNG fallback is not acceptable for the product. |
| Product transport | Authored completion-driven BCL socket host, callback registration, bounded send/receive buffers and async DNS preparation. |
| Socket ABI | External unmanaged `t_socket`, four-byte size/alignment, explicit conversion to integer ABI, separate registry tokens. |
| Native controls | Same upstream commit and disabled optional dependency profile; native Linux system services. |

[api-coverage.tsv](../config/api-coverage.tsv) inventories every name in the pinned
`lib/libsmb2.syms`. `required` marks the client surface to qualify, while
`translated-unqualified` and `deferred-client-profile` do not advertise managed
support. All managed validation rows start pending. Native oracle checks establish
the selected dialect/protection baseline, not managed coverage or full API parity.

The product profile uses [dotcc-overrides.json](../config/dotcc-overrides.json)
for exact semantic function replacements and external type registration, and
[emit-defines.json](../config/emit-defines.json) for named socket, event and open
constants. Original C signatures are preserved; integer descriptor prototypes
call audited authored adapters. The static `set_nonblocking` helper is replaced
directly, avoiding a variadic `fcntl` substitution. Polling and server primitives
return an explicit unsupported-operation error. The required
`HAVE_ARC4RANDOM_BUF` branch excludes the integer file-descriptor entropy fallback;
translation rejects changing that assumption without a descriptor audit.

Shared Libc networking remains available to other programs and to the explicit
`--profile legacy` upstream-test baseline, whose definitions live in
[legacy-defines.json](../config/legacy-defines.json). It is not a product fallback.
Both profiles retain the exact same 53 original library source files.

No protocol algorithm or generated C# is rewritten. Server APIs present in shared
units may be emitted, but SMB server hosting, full DCE/RPC, leases used
for application caching, and recovery features remain outside the initial client
qualification. See the plan for the required client behavior and later profiles.

The async profile defines `HAVE_LIBKRB5` and `HAVE_GSSAPI_GSSAPI_H`. The small
headers under `config/managed/gssapi` and `krb5` provide declarations/storage only;
they are not a native Kerberos ABI. `krb5-wrapper.c` is not translated: authored
`src/Kerberos/*.cs` implements its client boundary. `Directory.Build.targets`
links those files and the centrally pinned Kerberos.NET package into raw and
processed projects. The legacy profile leaves Kerberos disabled. Source and
package-version hashes participate in generation/qualification receipts.
