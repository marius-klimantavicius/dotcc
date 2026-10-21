# const_ints batch 1: translated, review pending — HOLD snapshot

Date: 2026-10-21. Authority: `../../PLAN.md`, `../../AGENTS.md`, and explicit
resume in `../coordination/RESUME-20261021.md`. No prior prototype implementation
was used. Original EDG source was opened directly. Source license attribution is
retained. The implementer performed no builds, tests, native preparation, Git
mutations or subdelegation. No routine is parity-qualified.

This is the bounded first batch, not frontend initialization or campaign
acceptance. Its nine source routine names have eleven distinct overload bodies.
There are also two macro counterparts and the required inline representation.
Every listed routine was translated. `incr_integer_value` depends on the disjoint
batch 2 `add_integer_values`; no addition implementation is duplicated here.
The combined batch remains review pending, including this cross-file dependency.

`const_ints-batch1.json` is the byte-precise ledger. It pins whole-file and
inclusive-range SHA-256 values for every routine, branch block, type and macro;
it records managed counterpart ranges and authored test associations. Hashes use
exact file bytes and source lines including their original terminators. The
`.sha256` file identifies the held owned files, excluding itself. Ledger hashes
record snapshots; they do not constitute source review or executed evidence.

## Pinned profile and excluded alternatives

The inspected command is the `cpfe` entry for `upstream/src/const_ints.c` in
`edgcpp/build/compile_commands.json`, not a command combined from other targets.
It uses `/usr/bin/c++`, `-x c++`, `-std=c++14`, `-O3 -DNDEBUG -flto`,
`-fno-exceptions -fno-unwind-tables -fno-rtti`, `-DUSE_CMAKE_DEFINES`, and the
generated `build/src/includes/cmake_defines.h`. The exact full command and hash
are in the JSON ledger. The frontend's eventual strict C++20 runtime profile is
separate. The baseline files and independent `reviews/p0-contract-r1.md` were
read as they arrived; measured ABI and source-to-binary evidence are still open.

| Selection | Active mapping | Excluded alternatives/backlog |
| --- | --- | --- |
| `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0` | All eleven array bodies, array `sign_of`, host-large signed-shift macro | Scalar bodies/header macros, scalar typedefs and scalar comparison branch remain unported; not excluded merely by a runtime option. |
| `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1` | `a_boolean`/TRUE/FALSE map to C# `bool`/true/false | Default `int a_boolean` and byte-boolean branch in basics.h:218-223 are inactive. |
| `__ANSIC__=1`, `EDG_MSDOS=0` | `sizeof_t=size_t`, `a_ptrdiff=ptrdiff_t` | DOS unsigned-long/checked `size_t_arg`, non-ANSI int pointer difference and host-specific branches remain outside this ABI. |
| Linux x86-64 host, `CHAR_BIT=8`, short=2, ptrdiff_t/size_t=8 | ushort / long / ulong, int and uint 32 bits | Other host widths/endianness require a new measured profile; no runtime architecture repair added. |
| `INT128_EXTENSIONS_ALLOWED=1`, `LONG_LONG_ALLOWED=1` | Largest integer 16 target bytes | Scalar host-int128/long-long/long typedef branches are inactive because representation is simulated. Smaller target integer maxima are not this compile configuration. |
| `FIXED_POINT_ALLOWED=1`, maximum fixed point=8 | `8 > 16` replacement predicate false; integer representation remains 16 bytes | Disabled fixed point and larger-fixed-point replacement branch are explicit configuration backlog. Fixed point was not disabled for convenience. |
| `TARG_CHAR_BIT=8` | `INTERNAL_TARG_CHAR_BIT=8` | 16/24/32 branches and invalid-value preprocessing failure are not implemented by this profile. |
| `_lint` scalar conversion alternatives | Both excluded inside scalar representation branch | Neither the lint nor non-lint scalar conversion macro substitutes for the active function. |
| `CHECKING=1`, `DEBUG=0` | No batch 1 routine calls an assertion/diagnostic | CHECK-001 remains required for later callees; `-DNDEBUG` does not authorize removing EDG checking. |

`MAX_SIZEOF_LARGEST_INTEGER=16`, `MAX_SIZEOF_LARGEST_FIXED_POINT=8`, host byte=8,
target byte=8 and ushort=2 produce 8 parts and 128 represented bits. Native
`TYPE_FOR_AN_INTEGER_VALUE=unsigned long long` is in the excluded scalar branch
and does not specify the active struct. `il_def.h` was inspected and hashed;
these raw-value leaves do not use its a_constant/a_type graph. No substitute
graph or partial constant/type object is introduced.

## Representation, identity and lifetime

`targ_def.h:1611-1677` supplies `an_int_value_part=unsigned short` and
`struct an_integer_value { part[8]; }`; `IntegerRepresentation.cs` preserves the
inline array in an unsafe value struct with sequential layout, pack 2, size 16.
Native size/alignment 16/2, part offset 0 and stride 2 are source-derived profile
expectations, awaiting supervisor measurements (ABI-001). Part zero is most
significant, independently of byte order inside a ushort. Whole struct assignment
copies the inline array, not an array reference. No padding/address bytes are
claimed to be semantic values.

Native a_host_large_integer/a_host_large_unsigned are ptrdiff_t/size_t, mapped
to long/ulong only for this pinned LP64 host. sizeof_t maps to ulong in these
leaves; source `size_t` parameters in the arithmetic batch use nuint under the
same 64-bit host restriction. Constants whose native expressions have size_t
type use C# int constants where their exact values (2, 8, 16, 64, 128) are
representable and all affected loop/conversion arithmetic has the same value;
INT_VALUE_PART_BASE retains unsigned host-large type ulong. Native ushort
promotes to int for bitwise operators/complement; explicit ushort casts record
the source's implicit store narrowing. Signed/unsigned host bit reinterpretation
is unchecked. Unsigned setter's masked ulong-to-long conversion is explicit in
C#; the masked value is always 0..65535. No signed-overflow case is introduced.

Every pointer designates existing live correctly aligned storage. The caller
owns allocation/free. No routine retains a pointer, allocates, frees, pins,
normalizes or canonicalizes. Stack locals remain live for nested calls. Future
managed arrays/objects must be pinned for the entire lifetime of all aliases;
this batch does not create managed storage or lifetime wrappers.

The two C++ `const an_integer_value& value` overloads are represented as a second
pointer. It is non-null and live for native-valid calls; this introduces no
nullable source case. It denotes the original referent, including self alias.
The port never writes through this parameter. Native const does not prohibit
the destination from reaching that same storage. Future callers must take the
address of their original variable/subobject and preserve native temporary call
lifetimes; they must not clone or use a by-value argument first. No frontend
callers are accepted here. Test transport `copy-signed`/`copy-unsigned` passes
the original `other` pointer; scalar transport setters first form a host value,
then select the long/ulong overload. Increment selects the long overload with
literal one. Arithmetic batch callers remain subject to its own exact ledger.

Reference copies traverse ascending indices; numeric setters and bitwise writes
traverse descending indices. Exact alias and distinct operands at both address
orders are supported. Partially overlapping storage is not claimed valid without
a demonstrated native object-lifetime/alias contract. Conversion host output and
bool output must be valid correctly typed output storage; no qualification for
ill-typed overlapping native objects is invented.

## Operations and branch mapping

The JSON gives exact byte hashes for the ranges in this table and finer branch
blocks. All rows have verdict **review pending**, test state **authored, not run**.
Local declarations, initialization, loop updates/exits, returns and ordered
output writes are included. There are no labels, diagnostics, recovery,
global/thread/context/cache reads or writes, allocation or cleanup in batch 1.

| Routine/macro and source range | Ordered operations and all enabled branches | Managed counterpart/test association |
| --- | --- | --- |
| `set_integer_value(long)`, c:33-47 | int i, long this_part; initialize i=7; predicate i>=0 true/false; mask signed host with 65535, narrow/store part[i], invoke signed-shift(value,16), decrement, exit after i=-1. Always eight iterations, including zero input. | const_ints.cs:20-32; SetHostValues, SignedShiftMacro, stream set-signed. |
| `set_integer_value(reference)`, c:50-59 | unsigned i=0; predicate i<8 true/false; read live reference part then write destination at same i; increment, exit at 8. Preserve exact self alias and forward order. | const_ints.cs:35-42; CopyValues, stream copy-signed. |
| `set_unsigned_integer_value(ulong)`, c:62-76 | int i and signed host this_part; descending loop; ulong mask then safe signed assignment; ushort store; logical ulong shift by 16; decrement and false exit. | const_ints.cs:45-57; SetHostValues, stream set-unsigned. |
| `set_unsigned_integer_value(reference)`, c:79-88 | Same forward live-reference copy as signed overload, separate routine identity. | const_ints.cs:60-67; CopyValues, stream copy-unsigned. |
| `conv_integer_value_to_host_large_integer`, c:91-142 | bits_so_far=0, bits_discarded=128-64, unsigned result=0, overflow=false; raw sign_of input. Evaluate is_signed then is_negative: negative signed chooses 65535 fill, else 0. Ascending loop i<8: read part; if bits_so_far<64 compare against fill, mark overflow on mismatch (continue even on error), otherwise shift result then add part. Advance bits by16. Final is_signed short circuit, then compare original raw sign with signed low-result sign; set overflow if unequal. Store *value truncated bits first, then *err. | const_ints.cs:70-117; ConvertValues, stream extract. |
| `cmp_integer_values`, c:284-334 | sign_1=flag1 && sign_of(op1), then sign_2=flag2 && sign_of(op2); result=0. Unequal signs: negative-first ternary -1/1, no part loop. Equal signs: ascending i<8; greater comparison first yields1+break; otherwise less yields-1+break; equality continues; equal-all exits with0. Return result. | const_ints.cs:120-151; CompareValues including alias with differing signedness, stream compare. |
| `incr_integer_value`, c:634-643 | Local one and err; set_integer_value(&one,1); add_integer_values(intval,&one,FALSE,&err). Discard local err after unsigned add; no shortcut increment. | const_ints.cs:154-160; IncrementValues; stream increment. Requires batch2 add. |
| `or_integer_values`, c:868-879 | i=7; loop predicate i>=0; int-promoted two live part reads, OR, narrow/store op1[i], --i; false exit. Same-pointer operands remain live aliases. | const_ints.cs:163-171; BitwiseValues; stream or. |
| `and_integer_values`, c:882-893 | Identical descending source order with int-promoted AND and in-place store. | const_ints.cs:174-182; BitwiseValues; stream and. |
| `xor_integer_values`, c:896-907 | Identical descending source order with int-promoted XOR and in-place store, including self-XOR zero. | const_ints.cs:185-193; BitwiseValues; stream xor. |
| `complement_integer_value`, c:1109-1120 | Descending loop; promote ushort to int, complement int, AND 65535, narrow/store; update/false exit. | const_ints.cs:196-203; BitwiseValues; stream complement. |
| `signed_shift_right`, h:37-40 | Cast host value to ulong then logical shift; evaluate signed sign test; negative arm constructs complement of shifted ulong-all-ones mask, nonnegative arm uses zero; OR and unchecked final signed cast. | IntegerRepresentation.cs:47-51; SignedShiftMacro and scalar signed setter. |
| `sign_of`, h:47-48 | Read live part[0], promote/AND 0x8000, compare nonzero; no signedness argument, no mutation. | IntegerRepresentation.cs:55-58; representation/compare/conversion/arith dependencies. |

The signed-shift macro evaluates source expressions more than once. Its pending
equivalence argument is restricted to the inspected enabled call in c:45, whose operands
are side-effect-free local value and constant 16. Other const_ints.c occurrence
c:993 is in the excluded host-integer branch. The authored direct macro tests
use side-effect-free arguments and only native-defined shifts 0..63; counts
outside this range are not assigned C# masked-count semantics. New macro callers
must receive their own evaluation-order audit.

Native sign_of uses an lvalue expression; its pointer helper reads at the same
call point without copying. Short-circuit flag evaluation remains at callers.
In particular, the arithmetic implementation must retain sign_of(op_2) AFTER
all op_1 writes when source does so. Shared sign_of introduces no cached sign
or operand snapshot. This batch does not silently fix native alias behavior.

## Dependency closure and remaining gates

All routines use the representation, constants and primitive mappings above.
Setters call only the mapped signed-shift macro where applicable; comparison and
conversion call sign_of. Bitwise/complement need no other semantic routine.
Increment additionally needs the source c:711-764 add routine, owned by batch2,
which uses the same representation, sign_of and INT_VALUE_PART_BASE. No diagnostic
or allocator is reachable from these valid leaf bodies. Batch2 itself depends on
the shared setters, complement and compare; the combined source closure is
explicit. Both batches and their shared helpers require independent exact-byte
review before any qualification.

The test runner's optional stream arithmetic adapters call existing batch2
methods only. This is test instrumentation, not additional implementation owned
by batch1. The runner's UInt128 parsing/formatting and test expectations are not
called from the production library. Authored tests are documented under
`tests/IntegerLeaves/README.md`; they include every ordinary exact-alias partition
for these signatures, all comparison signedness pairs, every first-differing
part, high-fill and retained-sign conversion failures, and width boundaries.
An impossible zero-iteration fixed-eight loop is not claimed dynamically
covered; its predicate and exit mapping are nevertheless recorded.

Open gates: ABI-001 exact native/managed widths, layout, compiler/config pin and
source-to-binary receipt; REVIEW-001 independent original/C# method and shared
dependency review; supervisor-only held JIT/native stream/NativeAOT validation.
CHECK-001 applies before enabled assertion dependencies are brought into another
closure. INIT-001 (bitint initialization width 65535/65536 against 128-bit storage)
is a separate unresolved platform-sensitive caller issue. No initialization is
ported or invoked here, no clamp/feature reduction/native-defect exception was
added, and finite valid leaf tests cannot qualify that frontend caller path.

This owned snapshot is ready for supervisor review and serialized validation.
**HOLD: no further edits without a supervisor repair assignment.**
