# EDG picotls verification

Run the actual pinned picotls core through the native compiler and EDG C#
backend, then compare exact output from native, .NET JIT, and NativeAOT:

```sh
python3 picotls/edg/tests/verify.py \
  --compiler edgcpp/build/bin/cpfe-dot \
  --output-dir /absolute/new/evidence-directory
```

The script uses all three unchanged `lib/picotls.c`, `lib/hpke.c`, and
`lib/pembase64.c` files at the revision in `picotls/config/inputs.json`.
The generated program uses translated musl with the BCL-backed portable POSIX
runtime, rejects unresolved externals, and treats managed/AOT warnings as errors.
It records input hashes, commands, exit statuses, and original output streams.
It retains raw C# and the generated project for inspection. `--native-only`
checks the native fixture without translating or claiming managed coverage.

`core.c` verifies stack-to-heap buffer growth, alignment and preserved data,
ASN.1 integer serialization, QUIC integer boundaries and truncated inputs,
network byte order, RFC 4648 base64 vectors and incremental/error handling,
indirect memory-clear/equality calls, escaping, IP helpers, and multi-object PEM
loading through actual filesystem I/O. These tests do not establish TLS or
cryptographic correctness; the provider and peer suites cover those separately.

The existing comprehensive TLS peer harness also accepts a prebuilt EDG peer,
without selecting or rebuilding the default dotcc-generated project:

```sh
# peer-command.json contains ["dotnet", "/absolute/ManagedConsumer.dll"]
python3 picotls/scripts/test-managed-peer.py --no-prepare \
  --managed-command-json /absolute/peer-command.json \
  --output-dir /absolute/new/peer-evidence-directory
```

For a prebuilt NativeAOT peer, use its executable as the single command element
and pass `--aot`. The runner uses the existing native picotls/OpenSSL and
independent .NET SslStream peers; the selected command is recorded in evidence.
