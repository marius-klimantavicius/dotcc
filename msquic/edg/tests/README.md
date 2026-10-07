# EDG MsQuic verification

These harnesses link unchanged EDG-generated MsQuic and picotls C# with the
existing BCL host/provider. They preserve source hashes, explicit adapters for
authored C# callers, project files, commands, stdout/stderr and failure reports.
They do not patch generated code or substitute the dotcc-generated core.

```sh
python3 msquic/edg/tests/verify.py \
  --core /path/to/msquic-translation/managed \
  --picotls-core /path/to/picotls-translation/managed \
  --stage /path/to/msquic-translation/source \
  --output-dir /path/to/new-validation-directory --aot
```

The default suite runs:

- Native GCC versus actual generated-type size, alignment, field offset and
  initialized bitfield-byte observations. The native side uses the exact
  staged host profile from the translation manifest.
- Host allocation checks: owner ledger, 16-byte alignment, zero fill, stability
  across compacting GC, size rejection and balanced frees. These exercise the
  real host table and its `NativeMemory` implementation.
- Existing owning API configuration controls and actual IPv4/IPv6 loopback UDP
  transport through EDG MsQuic, EDG picotls and the shared BCL provider. The
  original test sources cover copied ALPN, configuration lifetime, settings,
  leases, streams, flow control, cancellation, shutdown and drained ownership.

`--modes configuration transport packets resumption` selects additional
existing facade groups. `--prepared /path/to/provider-output` reuses an existing
provider preparation; do not build that directory concurrently from another
harness. Without `--aot`, receipts explicitly cover JIT only.

For an independent implementation on the other endpoint:

```sh
python3 msquic/edg/tests/independent.py \
  --prepared /path/to/new-validation-directory/provider \
  --output-dir /path/to/new-independent-directory --aot
```

This installs the repository's checksum-pinned aioquic wheels offline into an
isolated test environment. It reuses the existing authored `ManagedPeer` and
aioquic peer programs, runs both client/server roles, IPv4/IPv6 and AES128/AES256
with an ECDSA certificate, and checks the 65,537-byte exchange, FIN, authenticated
TLS negotiation, native transport statistics and host ownership drain. JIT and
NativeAOT together cover 16 independent exchanges. Pinned oracle wheels require
CPython 3.12 on Linux x64; they are not product dependencies.

A harness is accepted only when its retained `results.json` says
`"status": "passed"`. These focused checks do not claim that all of the older
dotcc recovery, authentication, fault-injection or API campaigns were rerun, or
that other OS/architecture combinations have been exercised.
