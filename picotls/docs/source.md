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

P0 rechecked the [default-branch commit API](https://api.github.com/repos/h2o/picotls/commits/master)
on 2026-09-11 at implementation start: this remains the latest commit. The recorded
snapshot is retained. The exact API responses are saved in ignored
`artifacts/provenance/latest.json` and `tree.json`.

The [recursive parent tree](https://api.github.com/repos/h2o/picotls/git/trees/3598470df01264da85157025ed10db0f7e103790?recursive=1)
contains one gitlink, `deps/picotest`, from
[`h2o/picotest`](https://github.com/h2o/picotest):

| Item | Recorded value |
| --- | --- |
| picotest commit | `a99858e0c33b97b24cd09ceae729f2be33ec01e1` |
| Archive | https://codeload.github.com/h2o/picotest/tar.gz/a99858e0c33b97b24cd09ceae729f2be33ec01e1 |
| Archive SHA-256 | `f3c42d988c8cd1af24dee2e66ce832f3e257bb7766e34167499954c1604c82c0` |
| Extraction | The parent source directory's `deps/picotest/` |

Run `./scripts/fetch.sh` from `picotls/`, or `picotls/scripts/fetch.sh` from the
repository root. [inputs.json](../config/inputs.json) is the machine-readable pin.
The script retains both original archives, verifies SHA-256 before extraction,
checks each existing upstream file against its archive contents, and refuses to
overwrite modified inputs. A rerun is offline when both verified archives exist.
The picotest contents populate the archive's empty submodule directory; all parent
and submodule files remain unchanged and ignored by git.

Upgrades require deliberately changing revisions, URLs and hashes in the manifest,
checking the new parent's gitlinks, preserving prior archives, and repeating P0/P1.
The fetch recipe never follows a moving branch.

## Licenses and native dependencies

| Input | License evidence in immutable input | Use |
| --- | --- | --- |
| picotls core | MIT notices in `lib/picotls.c`, `lib/hpke.c`, `lib/pembase64.c`, `include/picotls.h` | Translation input and native oracle |
| picotest | MIT notices in `picotest.c` and `picotest.h` | Native tests only |
| Bundled cifra | CC0-1.0, `deps/cifra/COPYING` | Upstream native cross-backend tests only |
| Bundled micro-ecc | BSD-2-Clause, `deps/micro-ecc/LICENSE.txt` | Upstream native cross-backend tests only |
| OpenSSL 3.0.13 | Apache-2.0, host package `/usr/share/doc/libssl-dev/copyright` | Separate native oracle only |

Cifra and micro-ecc are ordinary files in the pinned parent archive, so their
exact contents are covered by its hash; they are not moving fetch dependencies.
OpenSSL is a recorded system dependency, not a vendored or immutable downloaded
input: the tested Ubuntu packages `libssl-dev:amd64`, `libssl3t64:amd64` and
`openssl` are `3.0.13-0ubuntu3.15`. A different host library requires another
oracle run and an updated environment record. The managed product does not link
these native test backends.

The assertion-enabled native oracle now passes the two selected upstream C test
executables. See [configuration.md](configuration.md) for the exact source closure,
options, algorithms, platform evidence and excluded tests. This is native evidence;
it does not establish translated TLS or the BCL provider's correctness.

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
