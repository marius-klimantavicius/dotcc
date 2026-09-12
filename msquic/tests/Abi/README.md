# Linux upstream ABI reference probes

These programs include the unchanged pinned MsQuic headers. Native GCC and dotcc
receive identical Linux/static/preview/insecure feature defines. Native uses the
host glibc headers; dotcc uses its own embedded runtime headers. No diagnostic
source tree, replacement platform declarations, or edited generated C# is used.
The difference in system-header providers is intentional and is measured rather
than assumed equivalent.

Run from the repository root after the compiler Release build:

```sh
python3 msquic/scripts/test-abi.py
```

`--groups public tls core` selects groups; `--native-only` captures native
controls without asserting managed compatibility; `--jit-only` is an iteration
mode that does not establish NativeAOT coverage. Outputs and exact command
arrays are under `msquic/artifacts/abi/`; programs are under `msquic/build/abi/`.
The script verifies every archived reference source file before testing and
returns failure if any selected full comparison fails. Blocked emission/builds
remain failures in the report, never ABI successes.

| Group | Coverage |
| --- | --- |
| public | QUIC_BUFFER, QUIC_ADDR, QUIC_SETTINGS, QUIC_API_TABLE, QUIC_TLS_SECRETS; offsets, zero-initialized bitfield bytes, API table callback invocation |
| tls | CXPLAT_TLS_CONFIG, CXPLAT_TLS_CALLBACKS, CXPLAT_TLS_PROCESS_STATE; ALPN/key/buffer offsets, state bytes, typed TLS callback invocation |
| core | Stream/DATAGRAM frame bitfields, invariant/long/short packet headers, CXPLAT_POOL_HEADER; pragma-packed offsets and initialized wire-header bytes |

This first profile is an **upstream Linux x64 LP64 ABI reference**, not the final
managed host ABI. Callback tests compare source-level invocation in the native
and managed programs independently; they do not test native-to-managed exports.
Only listed fields, layouts and initialized storage bytes are covered. Picotls
boundary structs will be added once the bridge selects its actual public types.
