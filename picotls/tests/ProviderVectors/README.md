# Actual provider vectors

This executable references `src/BclProvider`, which references the generated
picotls product. It does not declare mirror protocol types or replace core
HKDF/allocation functions. Build/run after translation:

```sh
dotnet run --project picotls/tests/ProviderVectors -c Release
dotnet publish picotls/tests/ProviderVectors -c Release -r linux-x64 -p:PublishAot=true
```

Published known-answer inputs: SHA-256/SHA-384 `abc`; FIPS 197 AES-128/AES-256
block examples; [SP800-38A appendix F.5](https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38a.pdf)
AES-128/AES-256 CTR examples; zero-key/zero-IV GCM ciphertext/tag examples; and
[RFC5869 appendix A.1](https://www.rfc-editor.org/rfc/rfc5869.html#appendix-A.1)
HKDF-SHA256 using the actual translated HKDF implementation.

Additional integration assertions cover clone/reset/free including null output,
forced GC, gather and overlapping buffers, IV get/set, TLS sequence nonce XOR,
supplementary encryption consuming freshly produced ciphertext, invalid tags,
AAD and sequences, malformed pointer/length input, setup failure unwinding,
handle-count balance and nested/unscoped callback failures. An internal, thread-local
friend-test failpoint rejects each unmanaged-context/GCHandle ownership boundary
after BCL state creation, checks successful disposal and balanced handles, and
checks that failed setup preserves the caller-owned allocation and canary. Supplementary and
clone mechanics have BCL cross-checks; those checks are not independent primitive
known-answer vectors.

Failed hash cloning returns a process-lifetime poison context after recording the
original callback exception. This is required because the pinned ticket-issuance
path restores an unchecked clone result and later cleanup dereferences it. The
poison context owns no handle, never writes digest output and cannot become valid
in a fresh callback scope. Its free operation permits repeated cleanup without
freeing the algorithm table. Ordinary hash creation still returns null on failure.
Direct allocation-failure vectors exercise both clone ownership boundaries.
`HandshakeAllocationVectors` measures a real authenticated client/server handshake
with ticket issuance, then fails each observed provider allocation boundary using
fresh owning contexts and connections. It checks exception identity, no published
result from a failing facade call, safe poison-clone cleanup, disposed-connection
rejection and balanced provider handles/keys after disposal and finalizer drains.
An internal scoped counter proves ticket cloning was included. The sweep is bounded
to 512 provider context/GCHandle allocation boundaries; it does not inject every
BCL allocation or translated-core malloc. All four Linux x64 raw/optimized ×
JIT/NativeAOT variants pass 2033 checks, including 124 measured handshake allocation
boundaries and two failed ticket-clone cases. Public results match. Evidence:
`picotls/artifacts/tests/run-lqwhjcso`, `picotls/artifacts/tests/PASS.json`, and
`picotls/docs/validation.md`.

The provider requires `CallbackScope.Enter()` around each raw translated call;
call `ThrowIfFailed()` before exposing bytes. It bounds each callback/gather to
16 MiB and a vector list to 1024 entries. Scope objects stay on one thread and
must be disposed in stack order. Algorithm tables deliberately have process
lifetime. Contexts and connections are used serially; independent contexts can
run concurrently. The tests execute against actual emitted types and core code.

Ticket tests exercise the actual encrypt-ticket callback and translated buffer
allocator: authenticated round trips, corrupt header/ciphertext/tag, unknown key,
future issue time, exclusive expiry, empty plaintext, key rotation, bounded old-key
retention, owner/context lease disposal and failed allocation cleanup. Rejected
tickets must preserve the destination pointer, capacity, offset and prefix.

`TicketProtector` uses AES-256-GCM with fresh random keys and identifiers. Its
authenticated envelope contains a version, 128-bit key identifier, issue and
expiry timestamps, and a 96-bit nonce formed from a random prefix and a per-key
counter. Each key permits at most 2^32 encryptions. Lifetime is explicitly set to
1–604800 whole seconds, with at most eight retained previous keys; additional
rotations may invalidate still-unexpired tickets. A protector binds permanently
to one server context. Successful decryption rejects early data, and context
configuration requires fresh DHE for resumption. The test clock is internal,
instance-specific and thread-local; production randomness is never overridden.
Full handshake and resumption tests also pass in the separate TlsTests executable,
including its Linux x64 NativeAOT run. Other operating systems and architectures
remain unverified.
