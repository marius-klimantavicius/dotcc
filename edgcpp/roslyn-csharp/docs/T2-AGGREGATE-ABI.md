# Concrete native aggregate gateways

[The immutable receipt](../acceptance/aggregate-abi/report.json) passes 97 expected
outcomes: 94 successful phases and three expected unsupported-admission
rejections. SHA256:
`9ed9392f61c17f1b779c922c27ea351900d5cd9e70d5e26e1a0de7c5f70e7f1b`.
The authoritative artifact directory is
`/mnt/storage/roslynobj-csharp-20261015/t2/aggregate-reproduction`.
This proves the bounded aggregate boundary contract which, together with the
earlier accepted slices, closes T2's fixture gate. It establishes no SQLite
execution or general aggregate ABI support.

| Genuine input | Exact captured representation | Native/JIT/NativeAOT | Observations |
| --- | --- | ---: | ---: |
| integer | struct: signed 32 at 0/4; size 8, alignment 4 | 36 | 20 |
| sse | struct: binary64 at 0/8; size 16, alignment 8 | 94 | 20 |
| mixed | struct: signed 32 at 0, binary64 at 8; size 16, alignment 8 | 76 | 20 |
| memory | struct: signed 64 at 0/8/16; size 24, alignment 8 | 161 | 29 |
| packed | packed struct: byte at 0, unaligned binary64 at 1; size 9, alignment 1 | 364 | 20 |
| union | union: unsigned 64/binary64 both at 0; size 8, alignment 8 | 11 | 13 |

Packed and union admission have their own fixture and physical proof. The
ordinary struct cases do not admit other packed or union representations.
Admission compares retained kind, packing, size, alignment, field representations
and exact offsets. It does not infer compatibility from a record name or replay
C/C++ type semantics.

For each record `R`, the exercised native signatures are `R(R)` and
`R(R (*)(R), R)`. They require a fixed current prototype, ordinary Cdecl roles,
one same-record argument and, optionally, its preceding ordinary one-record
callback pointer. The selected public facts have no `this`, returned-value
parameter or copy-constructor return; all parameters have `ordinary_parameter`
roles and `PassedViaCopyConstructor=false`. Calls have no retained receiver or
return slot, and no TemporaryOperation nodes occur. Aggregate-valued calls feed
explicit local `dik_expression` initializations. The native whitelist is applied
to imports, unknown native addresses and address-exposed entries; direct selected
managed aggregate calls retain their ordinary managed value contract.

Host hidden return storage is a separate ABI matter. In particular, the 24-byte
record still has an ordinary selected EDG return and parameter. The emitter does
not insert or remove execution parameters because the host compiler uses a
hidden return pointer.

Application transforms remain ordinary C# methods with readable record and field
names, local values and returns. For example, the generated memory transform is:

```csharp
public static MemoryTriple transform(MemoryTriple value)
{
    (value).first += 3L;
    (value).second -= 2L;
    (value).third += 5L;
    return value;
}
```

Direct calls invoke that method. Known canonical addresses dispatch to ordinary
managed methods; unknown addresses use concrete forwarding helpers. The generated
C file contains captured-layout mirrors, size/alignment/offsetof static
assertions, callback registration, and typed call/return forwarding. The native
compiler places each concrete aggregate in host registers or stack. There is no
generic aggregate ABI classifier and no application algorithm in the native glue.

The CLR/native bridge itself uses only ordinary scalar/pointer signatures.
Incoming native entries take native record values, pass their addresses and
result storage to a fixed managed bridge, and return the resulting record.
Outgoing fixed wrappers receive the retained function address plus pointers to
materialized record values/result storage, then invoke the concrete native
signature. Native record-byvalue P/Invoke or UnmanagedCallersOnly signatures are
not used. Generated helper names expose the ABI boundary; application control
flow and values remain ordinary C#. Explicit storage casts and hoisted local
initializations retain some verbosity, without an operation interpreter.

Each checker executes the application twice and checks every observation. The
paths include direct and indirect managed calls, native identity echo, native
callback reentry, an unknown native identity callback, stable provider addresses
and canonical callback-address echo. Additional checks use actual
`Unsafe.SizeOf`, `Marshal.OffsetOf`, enclosing-record alignment and stack
addresses. ABI-only native observers read generated storage, including unaligned
packed fields and full-width union bits. EDG-folded layout constants alone are
not the physical evidence.

All six inputs repeat emission identically. Each emits four application
functions and one record, plus its observation-array carrier. The receipt checks
exactly five explicit captured native bindings per input. The separate provider
contains only identity, pointer echo, callback invocation and physical field
observation adapters; its 30 exports are checked against the explicit allowlist.
Generated ABI libraries expose only one canonical entry, its registration and the
three recorded outbound forwarding shapes. Their C source, binaries and identical
build/publish copies are hashed. The generated ABI library loads relative to the
managed assembly; the complete campaign runs without LD_LIBRARY_PATH.

The producer is the frozen Roslyn CPFE binary with SHA256
`cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4`.
The receipt records .NET SDK `10.0.112`, GCC/cc `13.3.0`, and compiler executable
SHA256 `1b99826121ae6682a634e5efe09bd3e3df58ce58e0b28f849114ab5b89139c26`.
Native mirrors compile as C17 with `-O2 -fPIC -fvisibility=hidden -shared`.
Independent native reference executables alone link the original application
sources. They are never generated-workload dependencies.

The receipt freezes 60 implementation inputs and records exact fixture, header,
include, object, provider, generated-source and build hashes. The built
RoslynObj.CSharp.dll SHA256 is
`5b1f919f84144c3bb9b90d55d2bd043206c849f5db611ad814ef5c5c97632d26`;
the runtime DLL SHA256 is
`1d40f58826e27d9024cb2703cb8832f32900261fdf8278b4ca21868d484f252f`.
Managed checker dependencies are only the checker, Generated and Runtime, with
no reader or SQLite dependency. ELF dependency receipts record libc/loader for
the ABI library and libc/libm/loader for NativeAOT executables.

The genuine alignment32, byvalue-bitfield and atomic-field candidates execute
natively as 7, -2 and 11, respectively. Each is rejected with an explicit
unsupported diagnostic before any generated directory is published. Aggregate
and field qualifier checks inspect every retained wrapper and final raw atomic
descriptor before scalar unwrapping. Nonzero address spaces, active UPC/shared
qualifiers, volatile/atomic, near/far and unaligned qualifiers are unsupported;
ordinary address-space zero and mere SharedBlockSize column presence do not
activate UPC semantics.

Other aggregate layouts or signature combinations, nested/array fields,
incomplete records, inheritance/virtual storage, nontrivial/hidden execution
roles, multi-unit aggregate boundary views, aggregate variadics and native C++
exception propagation remain unsupported or unaccepted. This evidence does not
claim general return-value temporary/lifetime lowering or alter the separately
recorded aggregate sequencing differential.

Reproduce under a complete source freeze with a fresh artifact directory:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t2-aggregate-abi.py \
  --objects /mnt/storage/roslynobj-csharp-20261015/t2/aggregate-abi/export \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-AGGREGATE-ATTEMPT
```

Omit `--objects` to perform genuine exports through the frozen producer and the
current Roslyn driver before executing the same checks. All build, export,
native reference, provider, JIT and NativeAOT commands and measured resource
results are preserved in the phase receipts.
