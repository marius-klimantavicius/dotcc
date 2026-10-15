# T2 representation direction and evidence gates

Reviewed 2026-10-15 within the approved plan. These are implementation contracts
to prove with genuine fixtures; no T2 execution capability is accepted yet.
The T1 milestone is `b5a096226e0e521a85f01e2f262bcf4b192c4b22`.

## Program assembly

Keep captured TU/declaration identity, selected definition identity and C# emitted
identity distinct. Internal/no-linkage entities remain TU-local. Use actual GNU
assembly names where retained, otherwise the captured current external name.
`NameHasBeenMangled=true` identifies an already mangled current name;
`MangledEncoding` with a false flag is not a function/data linker key. Never
remangle or resolve source overloads. Genuine C++ cross-unit fixtures must prove
this naming contract before C++ external binding is accepted.

Initial selection supports one strong definition or an explicit external
provider. Duplicate strong, common/weak, GNU alias, ifunc/multiversion and
COMDAT/inline selection remain explicit unsupported policies until separately
implemented. A primary/alternate-entry relationship is not alias-address proof.

Compatibility checks are limited to selected C# representations and native ABI
boundaries. Read public selected signatures, parameter roles, hidden return/
receiver conventions, qualifiers/address spaces and exact storage/layout facts.
Do not compare TU-local display strings or revalidate C++ ODR/type semantics.
Two live fixture units may use an explicit open-unit bound; this is not scale
acceptance. Later indexing retains the approved bounded/reopen design.

## Storage and constants

Allocate selected process storage with stable pinned owners and required
alignment before writing nonaddress constants and applying address fixups.
Then execute actual captured ordered initialization roots. Keep equal values
separate from storage identity and preserve exact string units/terminators and
their actual sharing relationships. Cyclic and element/one-past pointer fixups
need genuine native/JIT/NativeAOT comparison.

Prefer named unmanaged fields when CLR layout implements captured alignment;
otherwise use named accessors over aligned storage. Aggregate copies use the
captured source/destination size. Arrays retain captured bounds and stride;
flexible/incomplete/zero bounds remain distinct.

Bitfield byte spans/masks come from captured offsets, bit remainders, widths,
signedness and endian facts, interpreted against pristine EDG documentation/
stock C generation. Do not invent absent allocation containers. Volatile/atomic
access needs its actual memory-order implementation, not ordinary byte RMW.

## Function addresses and native callbacks

Application bodies stay ordinary named static methods. Only address-exposed
functions gain concrete static native-callable wrappers. Their actual native
wrapper address is canonical and stable. A per-selected-signature internal
indirect helper invokes known generated ordinary bodies; an unknown/provider
address uses an explicitly tested native calling-convention branch. This keeps
future generated C++ exceptional execution on the managed path when internal.

Native callback reentry uses the wrapper. C++ unwinding across that boundary is
unsupported. Aliases share addresses only when actual selected alias facts
establish that identity. Tests must prove repeat equality, supported casts,
storage in records/globals, native echo/reentry, unknown-provider calls and
matching selected signatures across units in JIT and NativeAOT.

## Variadic calls

The initial contract is explicitly SysV AMD64. Validate the retained builtin
`va_list` type/layout before using it. An owned translated argument pack contains
the target register-save/overflow areas; native entry can borrow a synchronous
native `va_list` view. `va_start`/`va_copy` cursors stay independent. Consume
already promoted captured argument types and recorded `va_arg` result types;
never redo C promotions or use boxed `params` storage.

Primitive integer/pointer/IEEE classes are an initial partial implementation.
Aggregate/vector/extended classes and unimplemented boundary shapes diagnose
explicitly. Fixed-signature v-form adapters require an equivalent provider
contract. Addressed variadic entry may use generated concrete fixed-prefix C
wrappers that only capture `va_list`, synchronously forward to a fixed managed
bridge, clean up the list and return the recorded result. The host C compiler
implements the incoming ABI; no application algorithm or SQL body belongs in a
shim. Borrowed storage cannot escape the callback. Canonical wrapper addresses,
native reentry, cursor copies, forwarding and GP/SSE overflow require fixtures.

Actual full SQLite inventory determines which primitive, aggregate and addressed
variadic cases are required. Any unresolved required form blocks T3; API tables
and otherwise unreferenced retained definitions must not disappear.
