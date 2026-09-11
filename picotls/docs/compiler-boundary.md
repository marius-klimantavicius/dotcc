# Core dependency and API inventory

The selected source closure is `lib/hpke.c`, `lib/picotls.c`, and
`lib/pembase64.c`. Each is separately preprocessed with dotcc's supplied headers
and the real upstream `include/` directory. Definitions are read from
`config/core-defines.txt`; no OpenSSL headers or system include directories are
added to managed translation. See [blockers.md](blockers.md) for the results and
initial source-linking choice.

`python3 scripts/inventory.py` reads the native core archive after the oracle
build and records exact symbol sets, lexical includes, and callback macro
declarations in `artifacts/compiler/inventory.json`. The initial Debug oracle
has **85 globally defined core symbols** and **23 external native dependencies**
after resolving references between the three units. These are native inventory
facts, not a claim that all API bodies translate or are supported public features.

| Surface | Findings and intended resolution |
| --- | --- |
| Allocation | `malloc`, `free`, `posix_memalign`; dotcc supplies the first two, but `posix_memalign` is absent from its headers/runtime. It is used by `ptls_buffer_reserve_aligned`. |
| Memory/string | `memchr`, `memcmp`, `memcpy`, `memmove`, `memset`, `strlen`, `strncmp`; supplied runtime exists, but several declarations use `int` lengths. Audit conversions and bound input sizes. |
| PEM files/diagnostics | `fopen`, `fgets`, `fclose`, `stderr`; supplied stdio declarations/runtime. No filesystem-backed credential loading is required for the future owning BCL API, but these upstream APIs remain in the source closure. |
| Time | `gettimeofday` remains in default `ptls_get_time`, even when a custom callback is registered. Existing `PosixLib` provides UTC wall time and a 16-byte LP64 timeval declaration; translated execution still needs validation. |
| IP parsing | `inet_pton` remains in `ptls_server_name_is_ipaddr`, for IPv4/IPv6. Existing `SocketLib` supplies it; no live socket I/O is required by the TLS core. |
| Native tooling | `_GLOBAL_OFFSET_TABLE_`, `__assert_fail`, `__fprintf_chk`, `__memcpy_chk`, `__memset_chk`, `__stack_chk_fail`, `abort` reflect native assertions/toolchain hardening or process termination. Dotcc supplies its own assertion/runtime path; native compiler support symbols are not crypto imports. |
| Headers | Standard C, `sys/types.h`, `unistd.h`, `sys/socket.h`, `arpa/inet.h`, `netinet/in.h`, `sys/time.h` resolve through dotcc. `pthread.h` does not. Native-only conditional `sys/syscall.h`, Apple and Windows headers are not selected by dotcc's macros. |
| Crypto | The core archive has **no unresolved OpenSSL/minicrypto crypto symbol**. Hash/cipher/AEAD/key-exchange/random/sign/verify/ticket operations are function-pointer contracts. HMAC, HKDF and the TLS schedule stay in the translated core. |

The exported surface includes connection/context lifecycle (`ptls_new`,
`ptls_free`, `ptls_get_context`, `ptls_set_context`), handshake and records
(`ptls_handshake`, `ptls_handle_message`, `ptls_send`, `ptls_receive`,
`ptls_send_alert`), SNI/ALPN getters/setters, exporters and key updates,
buffer/PEM helpers, crypto combinators and HPKE setup. The inventory retains the
exact full list and cross-unit dependencies instead of implying tested support
for every exported API. Numerous public helpers are header `static inline`
functions and consequently are not counted as global archive symbols.

Provider structs declare update/final/clone hash callbacks; cipher setup/init/
transform/dispose; AEAD setup, mandatory one-shot/vector/decrypt, IV access and
deprecated incremental callbacks; key-exchange creation/exchange and context
release. The header also defines callback objects for time, ClientHello,
certificate emission/signing/verification, tickets, open-count tracking, traffic
keys, extensions and ECH. `ptls_context_t.random_bytes` is a direct callback.
The verification callback returns a signature-verification callback and retained
state with an explicit empty-input cleanup call. Full storage/lifetime/error
contracts are in [crypto-provider.md](crypto-provider.md).

The initial profile disables logging and dtrace and leaves Brotli undefined.
That removes active mutex/logging/syscall and optional compression behavior;
it does not remove the unconditional pthread include or aligned allocation.
Optional HPKE/ECH/QUIC helpers present in the core are not advertised features.
