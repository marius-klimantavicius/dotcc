# Shared BCL provider for EDG picotls

The implementation remains in `picotls/src/BclProvider`. `../provider.py` creates
isolated projects from an EDG `Generated.csproj`, linking the generated sources
unchanged. It copies the existing provider and verification sources with recorded
ABI spelling adjustments:

- EDG uses public typedef names such as `ptls_context_t`; dotcc used C tags such
  as `st_ptls_context_t`.
- Plain C `char*` is `sbyte*` in this EDG profile. Byte-oriented managed callers
  need explicit pointer casts at those boundaries.
- Embedded arrays use EDG inline-array structs; pointer consumers take the
  address of element zero.
- One existing TLS test names dotcc's anonymous handshake-properties union. The
  adapter discovers EDG's emitted field name from the generated declaration.

These adaptations affect authored callers only. They do not replace translated
TLS functions or change cryptographic operations. The manifest records original
and adapted source hashes and every replacement.

`ProviderMemory.cs` extends the translated partial owner with allocation methods
that use its existing allocator and ownership registry. This is essential because
the provider and protocol exchange ownership of allocated buffers. Immutable
algorithm names use pinned managed arrays; ordinary allocations use the
translated native heap. No dotcc runtime assembly or native TLS library is linked.

Prepare projects only:

```sh
python3 picotls/edg/provider.py --core /path/to/managed --output /path/to/provider
```

Build and execute ABI, upstream utility vectors, provider vectors, and the shared
TLS tests in JIT and NativeAOT configurations:

```sh
python3 picotls/edg/managed/verify.py \
  --core /path/to/managed \
  --output-dir /path/to/new-verification-directory \
  --native-layout /path/to/native-layout.txt --aot
```

The native layout reference comes from compiling and executing
`picotls/tests/native-layout.c` against the pinned picotls headers. The verifier
also builds the existing managed consumer and writes `consumer-jit-command.json`
and `consumer-aot-command.json` for the independent peer suite:

```sh
python3 picotls/scripts/test-managed-peer.py --no-prepare \
  --managed-command-json /path/to/consumer-jit-command.json \
  --output-dir /path/to/new-peer-results
```

Use the AOT command file and `--aot` to test the published executable. Outputs,
commands, failure logs, and successful verification receipts are preserved in the
specified output directories. The original dotcc projects remain independent.
