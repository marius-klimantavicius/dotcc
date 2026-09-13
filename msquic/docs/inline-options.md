# Inline functions

The product link uses `--deduplicate-inline` and the `--export-inline` selectors
in `config/inline-exports.txt`. This is a compiler/linker transformation over
metadata derived from typed IR; no generated C# is hand-edited.

The selected managed methods include `CxPlatEwma`, `QuicAddrCompare`,
`QuicAddrCompareIp`, `QuicAddrGetFamily`, `QuicAddrGetPort`, `QuicAddrSetFamily`,
`QuicAddrSetPort` and `QuicAddrIsWildCard`. The host also pins
`QuicAddrSetToLoopback`, `MaxUdpPayloadSizeFromMTU`, `CxPlatTimeAtOrBefore64`,
`CxPlatTimeDiff64`, `CxPlatKeyLength` and `CxPlatHashLength`.
They can be called directly, for example:

```csharp
MsQuic.QUIC_ADDR address = default;
MsQuic.QuicAddrSetFamily(&address, (ushort)MsQuic.QUIC_ADDRESS_FAMILY_INET);
MsQuic.QuicAddrSetPort(&address, 443);
ushort port = MsQuic.QuicAddrGetPort(&address);
bool wildcard = MsQuic.QuicAddrIsWildCard(&address) != 0;
ulong average = MsQuic.CxPlatEwma(100, 200, 4); // 125
```

Unambiguous, proven inline groups also receive their original names without an
export selector. `--export-inline` makes a selected managed name a requirement:
if a future source unit adds a conflicting implementation, compilation diagnoses
it instead of retaining unit-specific names for that API.

Function fingerprints compare the types actually bound in the IR. The linker
validates complete aggregate layouts and resolves forward declarations separately.
An unrelated opaque type becoming complete therefore does not prevent sharing:
`lookup.c` completing `QUIC_PARTITIONED_HASHTABLE` no longer creates extra copies
of helpers such as `QuicConnIsServer`. Incompatible complete layouts still fail.

The final postprocessed output stays in `generated/TranslatedMsQuic`; raw output
stays in `generated/raw/TranslatedMsQuic`. Regenerate both with
`scripts/translate.sh`. Older objects must be rebuilt to add inline metadata.

## Constant-backed helper sharing

The inline proof now includes typed initializers for value reads from `static
const` scalars and pointer-free aggregates. `QuicAddrSetToLoopback` reads the
same `in6addr_loopback` value in every unit, so it can share a body and receive
its original managed name. Mutable, volatile, thread-local and identity-sensitive
static dependencies remain excluded. Rebuild objects for inline metadata version 3.

This changes function sharing, not global storage isolation. The existing
compiler merges same-named static global storage; keeping distinct methods never
fixed that limitation. In particular, the earlier loopback copies below all read
one emitted global, despite having different method names.

## Regeneration with compiler c0ab93d

All 47 product units were rebuilt and postprocessed into
`generated/TranslatedMsQuic` (raw output remains in
`generated/raw/TranslatedMsQuic`).

| Final generated source | Previous | Current |
|---|---:|---:|
| C# files | 10 | 9 |
| UTF-8 bytes | 3,937,152 | 3,916,798 |
| Methods with a `__unit_` suffix | 79 | 32 |
| `QuicAddrSetToLoopback` definitions | 47 | 1 |

`MsQuic.QuicAddrSetToLoopback` now has one normal managed name. The remaining
32 unit methods are non-inline static functions. The 60 host/core and 29 public
ABI observations match native execution under JIT and NativeAOT. Both raw and
postprocessed libraries pass the rooted JIT/NativeAOT consumer, including direct
calls to the loopback helper that check IPv4 `127.0.0.1` and IPv6 `::1` bytes.
Postprocessing rewrites 7,921 `Cond.B` calls and removes 1,057 empty blocks.

Compiler validation passes 2,195 unit tests and 455 functional tests (1,005
optional oracle tests skipped). The NativeAOT compiler also emits and links
constant-backed inline proofs. SQLite was regenerated with the postprocessor and
passes its JIT/NativeAOT SQL, JSONB, FTS5, WAL, UTF-16, callback and integrity
consumer checks. Logs and measurements are saved under
`artifacts/inline-options/static-constants`. These checks do not relabel the
historical transport campaign below as current evidence.

## Previous regeneration with compiler 255ccfa

| Final generated source | Before | After |
|---|---:|---:|
| C# files | 30 | 10 |
| UTF-8 bytes | 15,180,086 | 3,937,152 |
| Methods with a `__unit_` suffix | 8,167 | 79 |
| Function-pointer cache fields | 9,018 | 883 |

The reduction in suffixes includes both removed duplicate bodies and functions
receiving their original names automatically or through export selection. `QuicAddrIsWildCard`, `QuicConnIsServer` and
`QuicPathGetDatagramPayloadSize` each have exactly one cleanly named definition.
That snapshot retained 79 unit methods: 32 non-inline static functions and 47
copies of `QuicAddrSetToLoopback`. The old inline proof rejected its reference
to `static const in6addr_loopback`; this was independent of the global merger.

All 47 product units were regenerated. The 60 host/core and 29 public ABI
observations match native execution under JIT and NativeAOT. Raw and optimized
libraries pass JIT and NativeAOT consumers that root the entire generated
assembly. These consumers also call the exported EWMA helper and exercise the
exported address port and wildcard helpers for IPv4 and IPv6. Postprocessing
rewrites 7,967 `Cond.B` calls and removes 1,057 standalone empty blocks across
nine files.

Compiler validation passed 2,190 unit tests and 455 functional tests (1,005
optional oracle tests skipped). The compiler itself publishes as NativeAOT, and
that native binary passes source-to-object metadata emission and inline linking.
The owning `ManagedApi` project also builds successfully; existing generated
source warnings remain.

All 32 public consumer cases also pass across raw/optimized output, JIT/NativeAOT
and IPv4/IPv6, including stream transfer, FIN, resumption and authentication
failures. SQLite was regenerated with the postprocessor and passed its JIT and
NativeAOT managed SQL/JSONB/FTS5/WAL/UTF-16/callback consumer checks.

Measurements and copied validation logs are in `artifacts/inline-options/final`;
the initial implementation's results remain under `artifacts/inline-options/initial`.
The exact generated-source and tool hashes are recorded in
`config/product-closure.json`. ABI/build qualification is separate from the full
transport and shared SQLite campaigns; those historical campaigns are not
relabelled by regeneration.
