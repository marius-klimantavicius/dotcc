# Upstream test provenance and coverage

The pinned MsQuic source contains **590 literal GoogleTest declarations in 21
files** under `src`. This is a source inventory, not a count of executed tests or
expanded parameter combinations. The scan includes conditional compilation
branches, removes comments and string literals, and recognizes `TEST`, `TEST_F`,
`TEST_P`, `TYPED_TEST`, and `TYPED_TEST_P`. It found 217 `TEST`, 201 `TEST_F`, and
172 `TEST_P` declarations at this pin.

`config/upstream-test-inventory.json` records every declaration's file, line,
suite, name, category, and source-file SHA-256, together with the source commit
and archive SHA-256. Reproduce or check it with:

```sh
python3 msquic/scripts/inventory-upstream-tests.py
python3 msquic/scripts/inventory-upstream-tests.py --check
```

| Upstream category | Literal declarations | Scope |
| --- | ---: | --- |
| Core unit tests | 316 | BBR, Cubic, frames, packet numbers, partitions, ranges, receive buffers, settings, sliding windows, tickets, transport parameters, variable integers, and version negotiation |
| Platform unit tests | 102 | Allocation, packet crypto, datapath, platform services, TLS, and Toeplitz hashing |
| Transport/API tests | 172 | Public API validation and parameterized live-connection scenarios registered by `quic_gtest.cpp` |

The transport declarations call helpers in `src/test/lib`; those helpers are not
counted as additional independently registered tests. Conditional branches and
parameter generators require a configured native GoogleTest runner to establish
an executable case count. Fuzz input spaces and generated packets are not
enumerated by this declaration scan.

## Relationship to this campaign

The authored host, crypto, TLS, transport, and managed API tests exercise the
translated core and its contracts. They have their own case counts and receipts.
They are not direct executions of all the C++ GoogleTest declarations above.
Every inventory entry currently retains `direct_port_status: unqualified`.

| Existing campaign evidence | Relationship to upstream |
| --- | --- |
| Public/core/host ABI and contract probes | Compare actual pinned headers and selected C functions with native execution; they do not run the upstream GoogleTest harness. |
| Platform and datapath host controls | Exercise generated worker code and actual host callbacks, including ownership and teardown. They overlap platform categories but are separately authored cases. |
| Packet crypto vectors | Exercise generated `crypt.c` and managed packet callbacks with recorded RFC/native controls. This is not the complete upstream `CryptTest.cpp` suite. |
| Fragmented TLS, credentials, tickets | Exercise the translated provider and MsQuic TLS callback contract. Their cipher/role/error matrices are not equivalent to all native-provider `TlsTest.cpp` cases. |
| Live native/independent interop and fault proxy | Exercise genuine transport traffic under selected settings and faults. Required failures remain failures; packet delivery alone does not qualify migration or all transport tests. |
| Owning API and injected host controls | Qualify their explicitly named behavior when their receipts pass. They do not automatically satisfy an upstream declaration with a similar name. |

For a directly reused corpus subset, record exact upstream case names, relevant
source lines and hashes, adaptation rules, and native/translated observations.
`FrameTest.cpp` (34 declarations) and `TransportParamTest.cpp` (13 declarations)
are candidate sources for malformed-frame and parameter vectors. Until that
subset is executed and recorded, upstream-corpus reuse and its negative decoding
gate remain open in P7. The current inventory deliberately assigns no invented
coverage percentage to overlapping behavioral tests.
