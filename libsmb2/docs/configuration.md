# Initial Linux x64 profile

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
| Authentication | Built-in NTLMSSP; Kerberos/GSSAPI disabled. |
| Dialects | SMB 2.0.2, 2.1, 3.0, 3.0.2, 3.1.1. |
| Signing | Upstream SMB2 HMAC-SHA256 and SMB3 AES-CMAC paths. |
| Encryption | Upstream AES-128-CCM; no AES-GCM profile is advertised. |
| Preauthentication | Upstream SHA-512 transcript/key derivation for SMB 3.1.1. |
| AES | `aes_reference.c`, selected by upstream `aes.c`; Apple accelerator not selected. |
| Entropy | Secure `arc4random_buf`/`getrandom` host functions required; deterministic PRNG fallback is not acceptable for the product. |
| Native controls | Same upstream commit and disabled optional dependency profile; native Linux system services. |

[api-coverage.tsv](../config/api-coverage.tsv) inventories every name in the pinned
`lib/libsmb2.syms`. `required` marks the client surface to qualify, while
`translated-unqualified` and `deferred-client-profile` do not advertise managed
support. All managed validation rows start pending. Native oracle checks establish
the selected dialect/protection baseline, not managed coverage or full API parity.

The initial import audit finds allocation/string/format/time services already in
dotcc libc, but additional work is required for DNS, secure entropy, scatter/gather
I/O, socket readiness, nonblocking operation and IPv6. The existing `fcntl`/`poll`
placeholder behavior cannot satisfy the client contract. Generic shared runtime
implementations are preferred; each needs execution tests before qualification.

No protocol algorithm or generated C# is rewritten. Server APIs present in shared
units may be emitted, but SMB server hosting, Kerberos, full DCE/RPC, leases used
for application caching, and recovery features remain outside the initial client
qualification. See the plan for the required client behavior and later profiles.
