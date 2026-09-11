# picotls source snapshot

Inspected on 2026-09-11. The official repository's default branch is `master`.
GitHub's releases and tags APIs returned empty lists on this date, so “latest”
means the default-branch commit observed below, rather than a numbered release.

| Item | Recorded value |
| --- | --- |
| Repository | https://github.com/h2o/picotls |
| Commit | `3598470df01264da85157025ed10db0f7e103790` |
| Commit time | `2026-09-08T08:59:16Z` |
| Commit subject | Merge pull request #619 from h2o/kazuho/save-ticket-early-data-flag |
| Archive | https://codeload.github.com/h2o/picotls/tar.gz/3598470df01264da85157025ed10db0f7e103790 |
| Archive SHA-256 | `735ac6f0f3823dd5d0210ccf3a367ed39edc6fe889214db0749670437f705d00` |
| Local extraction | `picotls/ref/picotls-3598470df01264da85157025ed10db0f7e103790/` |

The archive was downloaded and inspected for planning. No compiler probe, native
build, crypto implementation, or TLS execution has been performed for this
campaign. Upstream files remain unchanged and are ignored by git.

Verify the SHA-256 before extraction on subsequent fetches. Implement a repeatable
fetch script in P0. If implementation starts against a newer upstream HEAD,
record and checksum that new immutable snapshot first; never silently move the
revision during a repair/validation cycle. Fetch any test submodule at the gitlink
revision recorded by the chosen parent commit, and record its URL and hash.
The current `.gitmodules` declares `deps/picotest`; GitHub source archives do not
populate that submodule. It has not been fetched for this planning step.

## Source findings

The pinned [CMake source list](https://github.com/h2o/picotls/blob/3598470df01264da85157025ed10db0f7e103790/CMakeLists.txt)
builds the core from `lib/picotls.c`, `lib/hpke.c`, and `lib/pembase64.c`.
`lib/certificate_compression.c` is added when Brotli support is selected.
The core test list includes `t/picotls.c`, `t/hpke.c`, and `t/quiclb.c`.

The [public header](https://github.com/h2o/picotls/blob/3598470df01264da85157025ed10db0f7e103790/include/picotls.h)
defines provider tables for hashes, ciphers, AEAD, key exchange, randomness,
signing, verification, time, and tickets. Its AEAD one-shot, vector-encryption,
and decryption callbacks are mandatory; the older init/update/final callbacks
are deprecated. Hashes require independent cloning and three finalization modes.
Certificate verification returns another callback and state; an empty-data/signature
invocation is its documented cleanup path after an aborted handshake.

The [upstream overview](https://github.com/h2o/picotls/blob/3598470df01264da85157025ed10db0f7e103790/README.md)
describes OpenSSL, minicrypto, fusion, and libaegis crypto integrations. Those are
reference implementations for callback behavior, not planned product dependencies.
The core includes OS headers and optional logging code, so replacing crypto alone
does not prove that its preprocessing or platform dependencies are resolved.

## BCL references checked

- [Cross-platform cryptography](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography): BCL crypto delegates to OS providers; availability varies by platform and algorithm.
- [IncrementalHash.Clone](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.incrementalhash.clone?view=net-10.0): independent hash-state cloning. The installed .NET 10 reference pack also exposes this method and `GetCurrentHash`.
- [ECDiffieHellman.DeriveRawSecretAgreement](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.ecdiffiehellman.deriverawsecretagreement?view=net-10.0): obtain the raw agreement for the translated TLS key schedule; do not substitute a BCL method that applies an additional KDF.

These API checks establish a proposed design, not successful provider behavior on
any target platform. P1/P3 must verify the actual APIs and byte encodings used.
