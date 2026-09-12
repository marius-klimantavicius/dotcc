# Campaign configuration

P0 was executed on the existing `sqlite` branch on 2026-09-11. Reference files
remain unchanged. Revisions, archive hashes and licenses are in
[source.md](source.md) and [inputs.json](../config/inputs.json).

## Translated source closure

[core-sources.txt](../config/core-sources.txt) selects three unchanged upstream
sources: `lib/hpke.c`, `lib/picotls.c`, and `lib/pembase64.c`.
[core-wrappers.json](../config/core-wrappers.json) explicitly selects the authored
`src/Host/picotls-quic.c` wrapper for `lib/picotls.c`. It includes that exact
reference source in the same translation unit and adds a small post-handshake
ticket encoder for the separate MsQuic adapter. The existing core algorithms and
reference files remain unchanged. `host-sources.txt` records both authored C
inputs, and translation provenance hashes the wrapper selection and sources.
Use the reference root and its `include/` plus dotcc's supplied C/POSIX headers;
`picotls/pembase64.h` is an additional upstream header used by `pembase64.c`.
Do not add the native test dependencies to the translated provider.

[core-defines.txt](../config/core-defines.txt) defines `PTLS_HAVE_LOG=0` and
`PICOTLS_USE_DTRACE=0`. `PICOTLS_USE_BROTLI` stays **undefined** because upstream
uses presence checks. No fusion, AEGIS or MbedTLS define is added. The source
retains HPKE code but HPKE is not an accepted public managed feature. TLS 1.2,
0-RTT and other follow-up capabilities remain unadvertised; the provider's later
runtime registration controls algorithm lists.

The unchanged core includes POSIX/OS headers even with logging disabled:
`pthread.h`, `unistd.h`, socket/address headers, `sys/time.h`, and Linux
`sys/syscall.h`. Do not force another OS identity to hide these dependencies.
P1 inventories which declarations need dotcc support and which symbols actually
remain referenced. The default time implementation still matters when a custom
time callback will eventually be registered.

## Native oracle recipe and source closure

From `picotls/`, run `./scripts/oracle.sh`. It resolves its own paths, sets
`TMPDIR=picotls/artifacts/tmp/p0`, fetches/verifies pinned inputs, configures CMake,
builds serially (`--parallel 1`), then runs the selected upstream tests. Native
builds have a 600-second bound, tests a 180-second bound, and CLI inspection a
10-second bound. Coordinate the repository's single build/test slot before use.

The oracle builds `picotls-core`, `picotls-openssl`, `cli`, `test-openssl.t` and
`test-minicrypto.t` with GCC and system OpenSSL. Debug configuration preserves
C assertions; upstream still adds `-O2 -g -std=c99 -Wall`. Linux CMake adds
`-D_GNU_SOURCE -pthread`. Campaign core feature defines match those above.
`test-openssl.t` additionally has upstream's `PTLS_MEMORY_DEBUG=1`.

`WITH_DTRACE`, `WITH_FUSION`, `WITH_AEGIS`, `WITH_MBEDTLS`, and `BUILD_FUZZER` are
explicitly OFF. Upstream probes Brotli without a disable option;
[oracle-pkg-config.sh](../scripts/oracle-pkg-config.sh) rejects exactly
`libbrotlienc` and `libbrotlidec`, forwarding all other queries to system
pkg-config. The recipe checks generated compile commands for accidental Brotli
activation and checks the disabled CMake switches. No upstream CMake edits are
needed. Fusion prerequisite detection may succeed, but its target is disabled.

The core library contains only the three selected core files. The OpenSSL adapter
is `lib/openssl.c`; CLI is `t/cli.c` plus `lib/pembase64.c`, linked with the two
libraries and host crypto/resolver libraries. Upstream CMake adds include paths
for `deps/cifra/src/ext`, `deps/cifra/src`, `deps/micro-ecc`, `deps/picotest`,
`include`, and the build directory even to the core target. The core does not
compile bundled crypto sources.

Native C tests combine `t/hpke.c`, `t/picotls.c`, `t/quiclb.c` with their backend
driver and pinned picotest. The drivers include portions of upstream core/backend
implementation directly, as upstream intended. Both executables compile bundled
cifra/micro-ecc sources and support files from the parent CMake source lists;
the OpenSSL suite deliberately exercises cross-backend behavior. These test-only
dependencies do not expand the translated source closure. The exact per-target
file list and compiler command are captured in `artifacts/oracle/compile_commands.json`.

Native objects for symbol/ABI inventory are
`build/oracle/CMakeFiles/picotls-core.dir/lib/{hpke,picotls,pembase64}.c.o` and
`build/oracle/libpicotls-core.a`.

## Native validation and algorithms

The assertion-enabled run passed `test-openssl.t` and `test-minicrypto.t`:
2 files, 24 top-level TAP groups, 37,025 `ok` lines including nested groups,
zero `not ok` lines, about 7 seconds. This is not 37,025 independent test cases.
OpenSSL's 17 groups cover Blowfish/QUIC-LB, key exchange, hashes, RSA/ECDSA/Ed25519
signatures, certificate verification, several TLS configurations, both
cross-backend directions, HPKE, and repeated synchronous/asynchronous handshakes.
Minicrypto's 7 groups cover P-256, X25519, signatures, TLS, ASN.1 bounds/recursion,
and HelloRetryRequest.

Three internal diagnostics skip a TLS test requiring both SHA-384 and SHA-256
TLS 1.3 suites when a minicrypto configuration lacks both. These native exclusions
do not excuse future managed AES-256/SHA-384 coverage. No C test failure was hidden.

The CLI reports P-256/P-384/P-521/X25519 groups, RSA and ECDSA with all three NIST
curves plus Ed25519 signatures, and `TLS_AES_256_GCM_SHA384`,
`TLS_AES_128_GCM_SHA256`, `TLS_CHACHA20_POLY1305_SHA256` suites. The native default
key-exchange array is P-256. Broader native backend capabilities are oracle
coverage only; the initial managed profile remains P-256, the two AES suites,
ECDSA P-256/SHA-256 and RSA-PSS/SHA-256. CLI help lists a Brotli option regardless
of whether its code is compiled; compile-command inspection confirms it is off.

`t/e2e.t` is not selected: P0 uses upstream in-process C TLS tests; the Perl socket
harness and its extra CPAN/faketime dependencies have not been established here.
`t/ech-live.t` is not selected because it targets public endpoints and ECH is a
follow-up feature. Fusion, MbedTLS, AEGIS and fuzz targets are excluded. P4 must
add the planned independent local-peer and negative authentication evidence.

Evidence is ignored under `artifacts/oracle/`: `configure.log`, `build.log`,
`tests.log`, `environment.txt`, `compile_commands.json`, `cli-help.txt`, and
`native-dependencies.txt`. The recipe exits on configure/build/test failure.

## ABI and platform matrix

The native baseline is Linux x86-64, little-endian System V LP64: 8-byte pointers,
`long`, `size_t`, `ssize_t`, and `time_t`; 4-byte `int`; 2-byte `short`; 8-bit
bytes. These assumptions are to be checked by P1's native/managed ABI probe;
do not reuse LP64 `long` assumptions for Windows LLP64. Function and object
pointers have the same size on this baseline. Appended provider context handles
must respect each prefix's actual size/alignment, not assumed field packing.

| Component/target | P0 evidence |
| --- | --- |
| Host | Zorin 18 / Ubuntu 24.04, Linux x64, kernel 6.8.0-138-generic |
| Native C compiler | GCC 13.3.0, Ubuntu `13.3.0-6ubuntu2~24.04.1` |
| CMake | 3.28.3 (`3.28.3-1build7`) |
| Crypto oracle | OpenSSL 3.0.13, Ubuntu `3.0.13-0ubuntu3.15` |
| Installed .NET | SDK 10.0.111; Microsoft.NETCore.App 10.0.11 |
| Managed target | .NET 10 / C# 14; P1 feasibility probes record execution separately |
| Linux arm64 | Not run |
| Windows x64/arm64 | Not run; LLP64 requires separate ABI validation |
| macOS x64/arm64 | Not run |
| NativeAOT | Not established by P0 |

System OpenSSL and compiler versions are recorded host dependencies, not
checksum-pinned vendored downloads. Preserve new environment evidence whenever
the host toolchain changes. No managed TLS or additional platform support is
claimed by the native oracle results.
