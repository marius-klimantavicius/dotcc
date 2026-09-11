# Product dependency audit

Run `python3 picotls/scripts/audit-product.py` after translation and Release builds
of both generated variants and the owning provider. The JSON inventory is written
to `picotls/artifacts/dependencies/report.json`; `--output` selects another path.
Exit 0 means the inventory checks passed, 1 means a prohibited dependency or
provenance mismatch was found, and 2 means required product evidence is missing.
`--self-test` checks prohibited-code detection and manifest path validation without
building or loading the product.

The initial source-only run reports zero provider violations and exits 2: no
successful full-core translation, generated products or Release dependency
manifests exist yet. This is not a completed P5 dependency or NativeAOT result.

The audit discovers raw and optimized generated sources through the compiler's
`Dotcc.SourceFiles.txt`. It rejects missing files, duplicate entries, path escapes,
symlinks and additional source files outside the manifest. Source, project and
manifest hashes must match `artifacts/translation/success.json`. Pinned archive
configuration, core source list, defines, authored host source hashes and the three
compiler/postprocessor tool hashes must also match that translation record. The
report independently records hashes of every authored provider source file.

For the actual projects, the report lists framework, assembly, project and package
references. For every discovered Release `.deps.json`, it records dependency
edges, managed assembly assets, native assets and RID-specific runtime assets.
Explicit third-party package/assembly references and unexpected project references
are rejected. Framework implementation assemblies can be supplied by the shared
runtime without appearing in an application dependency manifest. This inventory
does not parse PE AssemblyRef tables or prove binary freshness; rebuild from the
recorded source before publishing and retain the build/publish logs with the report.

Authored provider and translated protocol source are checked for P/Invoke,
`NativeLibrary`, generated native import tables, process creation, reflection,
dynamic dispatch, runtime code generation and unmanaged callback declarations.
Comments and ordinary string/character literals are excluded. This is a
conservative source check, not a C# semantic analyzer or a proof against deliberate
obfuscation. Provider callbacks use statically typed managed function pointers and
GCHandles; there is no authored native crypto import layer.

The compiler currently embeds the generic libc implementation in generated
projects. That shared source includes native filesystem/process operations and
dynamic-library facilities needed by other C programs. The audit inventories all
such markers separately after the compiler's embedded-runtime boundary. It rejects
direct uses of the known native/process wrappers by the protocol or provider. It
does not claim whole-program reachability or that every unused runtime facility
has been removed; NativeAOT publish output and platform binary inspection remain
separate evidence.

The authored algorithm registration evidence and BCL crypto API identifiers are
included with source locations. The configured profile currently registers
TLS_AES_128_GCM_SHA256 and TLS_AES_256_GCM_SHA384, P-256 ECDHE, P-256 ECDSA/SHA-256
and RSA-PSS/SHA-256. AES ECB and CTR are supplementary cipher callbacks; SHA-256
and SHA-384 implement the hash callbacks. Tickets use independent AES-256-GCM
keys. A source inventory proves neither that callbacks work nor that an algorithm
was negotiated; provider vectors and full handshake tests provide that evidence.

“BCL-only” describes the application API/dependency boundary. .NET cryptography
uses platform implementations, including OpenSSL on Linux and CNG on Windows;
certificate validation also follows platform behavior. Those runtime dependencies
are compatible with using only BCL APIs and must still be available on deployment
targets. See Microsoft's [cross-platform cryptography documentation](https://learn.microsoft.com/en-us/dotnet/standard/security/cross-platform-cryptography).
NativeAOT also restricts dynamic assembly loading and runtime code generation;
passing this source audit does not replace an actual warning-reviewed NativeAOT
publish and execution. See the [NativeAOT limitations](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/#limitations-of-native-aot-deployment).

Native OpenSSL/picotls oracle executables and test-only .NET peers have independent
projects and dependency records. They are excluded from this production-provider
inventory and must not be substituted for translated-product test results.
