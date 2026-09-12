# Explicit ticket key configuration

`BclCryptoProvider.TicketProtector.ImportKeys(ReadOnlySpan<TicketKeyImport>)`
atomically replaces an explicitly configured ring. `TicketKeyImport` is a
borrowed pair of `ReadOnlyMemory<byte> Id` and `Material`: exactly 16 and 64 bytes,
respectively. The caller must not mutate them during import and remains
responsible for clearing its copies afterward. The provider copies both values
before publication; later caller mutations cannot change installed keys.

The ring contains 1–16 entries, matching the pinned MsQuic maximum. Entry zero
encrypts; every entry decrypts. Empty rings, incorrect lengths, duplicate IDs
(even with identical material), and an existing ID paired with different
material are rejected without changing the active ring. Preparation failures
dispose every unpublished key. Successful replacement immediately removes keys
omitted from the new ring; it does not keep a hidden retired-key cache. A removed
ID may subsequently be installed with new material, which cannot decrypt its
old tickets. Callers should assign new IDs to new material.

Imported decryption keys survive expiry pruning regardless of local issuance
history: a different server may have minted their tickets. Individual tickets
still have authenticated issuance and exclusive expiry times, and their lifetime
must match the receiving protector's configured lifetime. Importing `[new, old]`
rotates issuance while retaining old-ticket decryption; importing `[new]` removes
it. These explicit managed ring semantics extend the pinned native OpenSSL
provider, which currently consumes only the first configured key.

The existing constructor and `Rotate()` retain the original random-key mode and
version-one envelope. The first successful import permanently selects imported
mode for that protector and replaces its random keys. `Rotate()` then throws
`InvalidOperationException`; applications rotate through `ImportKeys` instead.
The constructor's `retainedPreviousKeys` controls only random mode. Every import,
including one with the same keys in the same order, creates fresh random
derivation domains and resets only those new domains' counters. Tickets from
previous domains still decrypt while their master remains configured.

## Version-two envelope

Integers are big endian. Every header byte is AES-GCM associated data.

| Offset | Length | Value |
| --- | --- | --- |
| 0 | 4 | ASCII `DPTK` |
| 4 | 1 | Version `2` |
| 5 | 16 | Configured key ID |
| 21 | 8 | Nonnegative issue time, Unix milliseconds |
| 29 | 8 | Exclusive expiry, Unix milliseconds |
| 37 | 4 | Zero nonce prefix |
| 41 | 8 | Encryption counter, starting at zero |
| 49 | 32 | Random HKDF derivation salt for this import domain |
| 81 | variable | Encrypted opaque picotls ticket |
| final 16 | 16 | AES-GCM authentication tag |

The AES key is the 32-byte output of BCL HKDF-SHA256 with the imported 64-byte
master as IKM, header salt as salt, and UTF-8
`Managed.Security.TicketProtector/v2/AES-256-GCM` followed directly by the 16-byte
ID as info. Decryption derives from the received authenticated-header salt;
it does not require the receiving instance to retain that encryption domain.
No attacker-controlled derived-key cache is retained. Derived key bytes are
cleared immediately after constructing `AesGcm`; temporary plaintext is cleared
on success and rejection. Owned masters are cleared on replacement or final
lease release.

The 96-bit nonce is bytes 37–48. There are at most 2^32 encryption attempts per
derived key, serialized under the state lock; consumed counters are never rolled
back after a later failure. A new instance/import samples a 256-bit salt from
`RandomNumberGenerator`, so reuse of a master and counter does not deliberately
reuse an AES key/nonce pair. Domain collision probability follows the birthday
bound for random 256-bit salts. This uses HKDF's random-salt and application-info
separation from [RFC 5869 sections 3.1–3.2](https://www.rfc-editor.org/rfc/rfc5869.html#section-3.1)
and satisfies the per-key nonce requirement of
[BCL AesGcm.Encrypt](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.aesgcm.encrypt?view=net-10.0).

The protector still binds to exactly one immutable server context. Explicit
master distribution allows tickets across contexts/instances, so applications
must distribute keys only among equivalent authentication policies. Native
OpenSSL/Schannel ticket ciphertext is not interoperable with this private
envelope. Invalid, unknown-version/key, tampered, future or expired tickets leave
the destination unchanged. Successful decryption returns picotls's explicit
early-data rejection result; resumption requires fresh DHE and never permits
0-RTT. Owner disposal rejects imports while active context leases keep the
installed state alive until final release.

`tests/ProviderVectors/ImportedTicketVectors.cs` covers independent instances,
reimport, rotation/removal, caller-buffer ownership, 16-key decryption without
local issuance, duplicate/conflicting IDs, preparation allocation failures,
counter exhaustion, authenticated fields, expiry, lifetime mismatch and leases.
Its independent BCL decode checks the documented format. The complete existing
campaign runs these vectors and the existing handshake/native-peer suites over
raw/optimized translations in JIT and NativeAOT modes; validation receipts live
under `artifacts/tests/`.
