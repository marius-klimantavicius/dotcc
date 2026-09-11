# Upstream case ports

`Program.cs` ports eight named cases from the unchanged pinned `t/picotls.c`:
`is_ipaddr`, `base64-decode`, `ptls_escape_json_unsafe_string`, `quic/varint`,
`hmac-sha256`, `tls-block8`, `tls-block16`, and `quic/block`. The QUIC integer case also checks every truncated prefix and
the maximum 62-bit value. These are calls into the translated product and BCL
provider, not independent C# implementations of the codecs or HMAC.

The complete case inventory, distinctions from other harness coverage and
remaining work are in [upstream-tests.md](../../docs/upstream-tests.md).
The upstream test source carries the MIT notice by DeNA and Kazuho Oku; its
immutable revision and license provenance are in [source.md](../../docs/source.md).

After successful translation:

```bash
dotnet run --project picotls/tests/UpstreamVectors -c Release
dotnet publish picotls/tests/UpstreamVectors -c Release -r linux-x64 \
  -p:PublishAot=true -o picotls/build/upstream-vectors-aot
picotls/build/upstream-vectors-aot/UpstreamVectors
```

Current status: all four Linux x64 raw/optimized × JIT/NativeAOT variants pass
all eight cases and 232 checks against the actual product. Evidence:
`artifacts/tests/run-lqwhjcso` and `artifacts/tests/PASS.json`.
