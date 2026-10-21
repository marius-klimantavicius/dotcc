# const_ints arithmetic closure, batch 2

State: **translated but dependency-blocked; independent produced-code review
pending**. No routine or branch is parity-qualified. Implementation task:
`faithful-p1-integer-arithmetic-20261021-r1`, 2026-10-21. Authority is the current
PLAN.md / AGENTS.md and coordination/RESUME-20261021.md. Explicit resume
supersedes historical STOP. No child build, test, native preparation, Git
mutation or subdelegation occurred.

`const_ints-batch2.json` is the full per-routine, per-block operation and branch
ledger. Each row records inclusive native/C# line ranges and SHA-256 of those
exact bytes, including original line terminators. Whole-file and exact routine
hashes, configuration alternatives and dependency snapshots are recorded there.
Hash calculation/JSON formatting is metadata bookkeeping; all production C# and
test code were handwritten by reading the original source. The independent
reviewer must open the original C and the actual C# at the held snapshot.

Owned paths are `src/EdgFaithful/const_ints.arithmetic.cs`, this batch2 ledger,
and `tests/IntegerArithmetic/Program.cs`. Project manifests, the representation,
leaf helpers, upstream, prototypes, prior evidence and coordination are outside
this worker's write ownership. No additional integer foundation or duplicate
`incr_integer_value` is introduced.

## Native configuration and representation

The original is `edgcpp/upstream/src/const_ints.c`. Read the actual
`edgcpp/build/compile_commands.json`: separate entries exist for cpfe, cpfe-cp,
cdisp, cpfe-dot, cpfe-dot-oracle. The cpfe command is the recorded reference
selection in the JSON ledger; no macros from the other targets are combined.
It compiles as C++14 using `/usr/bin/c++`, `-DUSE_CMAKE_DEFINES`, include
`build/src/includes`, `-O3 -DNDEBUG -flto -fno-exceptions -fno-unwind-tables
-fno-rtti`, plus the exact entry's definitions/warnings. The supervisor must
produce the source-to-binary receipt; an existing artifact does not establish
that receipt. Runtime strict C++20 is separate from these build features.

`fe_common.h` includes `basic_hdrs.h`; `basics.h` includes `defines.h`, which
selects generated `cmake_defines.h` under USE_CMAKE_DEFINES. Inspected values:

- INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0: the simulated-part branch is active.
- USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1: `a_boolean` is native C++ `bool`
  (basics.h:213–217), despite the inactive default-int alternative. C# parameters,
  locals and `bool*` outputs preserve this contract. Expected native size is one
  byte on Linux x86_64, with supervisor measurement pending. No interop
  marshalling is used.
- FIXED_POINT_ALLOWED=1 and INT128_EXTENSIONS_ALLOWED=1 remain enabled.
  MAX_SIZEOF_LARGEST_INTEGER=16, inspected largest fixed-point size=8, so the
  native maximum representation is 16 bytes. Native parts are unsigned short,
  expected two bytes, eight parts, most significant first, 128 bits total.
- `a_host_large_integer=ptrdiff_t` and
  `a_host_large_unsigned=sizeof_t=size_t`; the pinned Linux x86_64 LP64 contract
  maps their arithmetic here to C# long/ulong. Native int is expected 32 bits.
  Source `size_t bits` maps to C# nuint, and only mask/sign-extension use it.
  On this 64-bit profile it equals the width/range of the leaf's ulong sizeof_t.
  Other host ABIs are outside this candidate qualification.
- CHECKING=1, DEBUG=0. These routines contain no native check_assertion branch,
  diagnostic, allocator or recovery path. That does not remove those dependencies
  from future callers. No new diagnostic or invalid-input guard is added here.

The exact source/header/configuration hashes and selected full compile command
are in the JSON. The independent contract report is
`reviews/p0-contract-r1.md`; it is a source-derived candidate contract, with
measured ABI and produced-code review still pending.

## Parameters, identity, state and lifetime

All void returns remain void. `op_1`, `mask`, and `value` are live pointers to the
original inline `an_integer_value` storage and receive in-place mutations.
`op_2` is a live pointer, never an implicit by-value snapshot. Native value-copy
locals `orig_value` and `result` remain inline struct copies. The representation
is supplied by the leaf owner: `public unsafe struct an_integer_value` with
`fixed ushort part[8]` in namespace EdgFaithful. Source units share
`public static unsafe partial class const_ints`; splitting the file follows
exclusive worker ownership and retains original routine grouping/order.

Valid pointers must designate native-valid, aligned, initialized objects for
their required reads, valid output storage, and remain alive for the call.
For a pure output mask the old value need not be initialized. Exact self-alias
and disjoint integer operands are preserved; no native-valid partially overlapping
object contract has been established, so none is claimed. Managed addresses
must remain pinned for their source alias lifetime when obtained from movable
managed storage. These methods allocate nothing, retain no pointers, create no
new owner, and leave caller storage lifetime intact. Tests use call-lived stack
objects and inspect alias partitions explicitly.

The `err` pointer designates native bool output storage with its native lifetime.
It receives a final bool store after arithmetic mutation; mixed routines first
permit the base operation's store and can overwrite it later. `sign_extend`
discards its local shift error, exactly as native. No globals, caches, target
tables, thread state, ordered semantic graphs, allocation or cleanup are read or
written. The only constants are the source-defined representation constants.

## Complete routine inventory and operation mapping

Detailed mappings and individual branch/block hashes are in the JSON. All rows
are review-pending and unexecuted. The ranges below include inactive source
alternatives, which are separately inventoried rather than represented as enabled
managed alternatives.

| Source routine/macro | Native const_ints.c | Managed arithmetic.cs | Dependencies in call order |
| --- | --- | --- | --- |
| add_integer_values | 711–764 | 14–47 | sign_of; per-part arithmetic; final short-circuit sign_of reads |
| add_mixed_signed_integer_values | 767–792 | 49–63 | selected struct copy; add_integer_values; conditional cmp_integer_values |
| subtract_mixed_signed_integer_values | 795–826 | 65–89 | struct copy; subtract_integer_values; conditional sign_of then cmp_integer_values |
| make_integer_value_mask | 829–850 | 91–98 | set_integer_value(long zero), complement_integer_value, shift_right_integer_value(false,false) |
| sign_extend_integer_value | 853–865 | 100–106 | shift_left_integer_value, shift_right_integer_value(true,true) |
| get_part macro | 913–917 | 111–117 | negative then upper-bound short circuit; conditional part read; widen to ulong |
| shift_left_integer_value | 921–977 | 119–150 | get_part twice before each write; final err store |
| shift_right_integer_value | 980–1029 | 152–181 | conditional sign_of before mutation; get_part twice per descending write |
| subtract_integer_values | 1032–1091 | 183–216 | sign_of; per-part arithmetic; final short-circuit sign_of reads |
| negate_integer_value | 1094–1105 | 218–224 | set_integer_value(long zero), subtract_integer_values(true), struct copy |

For add/subtract the first sign is captured before mutation. The loop starts
at part 7, reads both current parts, computes using signed 64-bit intermediates,
adjusts carry/borrow, writes the current part, decrements the index, and exits
after part 0. Work before add adjustment is 0..131071; subtraction work is
-65536..65535. Carry/borrow adjustment leaves 0..65535 and keeps native long
arithmetic defined. Narrow stores are explicitly unchecked ushort casts.
Unsigned output tests the final carry/borrow. The signed predicate preserves
short-circuit order and **rereads operand 2 after every operand-1 write**.
Consequently self-add across the sign boundary can report no signed overflow,
and self-subtraction of a negative value can report overflow. Those behaviors
are preserved, not repaired. No early operand-2 sign copy is present.

Mixed add copies the selected signed value before the base call, even when
signedness matches, then replaces err only if flags differ, using the exact
comparison predicate >0. Mixed subtract always copies operand 1 first; after
the base call and only if flags differ, it computes operand-2 negativity using
`op_2_signed && sign_of(*op_2)` at that late point, then compares original and
result using operand-1 signedness for both. Negative-second uses cmp>0;
the other arm uses cmp<=0, including equality. Subtracting differently signed
zero can therefore set err. Aliased operand 2 sees the mutated result here too.

The source `get_part` captures the caller's local fill_value. C# uses one private
helper with the same source name, a pointer to the existing value, explicit
int part_gp, and explicit ushort fill_value. The pointer conversion replaces
the macro lvalue argument without copying storage. The helper checks part_gp<0
first, then part_gp>=8, dereferencing only in-range; it widens the selected
ushort to ulong. Source casts of part_gp are identities because every call uses
an int expression. Original macro argument repetition versus one evaluation is
equivalent for these side-effect-free, bounded index expressions. fill_value is
a nonvolatile local, unchanged during either expression. Both combined get_part
reads precede their destination store; relative native operand evaluation order
has no observable difference because neither read mutates state.

Both shifts compute part_offset, first_part_shift, second_part_shift in that
order. Left walks ascending (0..7) so higher input indices are unread until
needed; right walks descending (7..0) so lower input indices remain intact.
Each pair of reads is widened to native unsigned 64-bit work before shifting,
ORed, masked to 0xffff, then narrowed/stored. No temporary array or whole-value
snapshot replaces this traversal. Left tests the current original work for
whole-part loss first, then boundary-part loss via else-if, retaining sticky
overflow and storing it after the loop. Zero-count calls still traverse all
eight parts. Right's fill branch short-circuits `is_signed && sign_extend`,
then converts native bool to int 0/1, negates, masks, and narrows before the
loop; the C# conditional is the required bool-to-int language adaptation.

Mask calls zero/complement/logical right shift in the native order. Sign
extension computes shift_bits, invokes left shift with a call-lived local bool
error, ignores that output, then invokes signed extending right shift. Negate
creates a local zero, passes it as subtract's first operand with the caller's
original pointer as second, then copies the whole result back after err was
written. No expression replaces those source calls.

## Bounded shift contract and INIT-001

The candidate valid shift domain is **0..127**; the original callers must
provide legal counts. It gives first_part_shift 0..15 and second_part_shift
1..16, so every 64-bit work shift is within its native width, including the
16-bit shift used at zero/whole-limb counts. int offsets remain bounded and
each get_part read is guarded. Mask/sign-extension test and qualification
domain is **1..128 bits**: native size_t subtraction yields 0..127 before
the int cast. These are qualification/caller obligations, not new production
checks or claimed semantics for invalid inputs.

**INIT-001 remains open.** The P0 reviewer found native initialization widths
65535/65536 derived from enabled bit-precise kinds, beyond 128-bit storage.
This can underflow source size_t subtraction and produce negative shifts after
the int conversion on the recorded compiler. No native execution here establishes
the behavior. The port does not clamp widths, disable kinds, rewrite the
initializer, add Unsupported/exception guards or claim CLR count masking as
portable native behavior. `unchecked` keeps the source conversion stages
explicit; it is no equivalence argument outside the bounded domain. The
frontend initialization path remains unqualified and requires supervisor
investigation/user decision if a divergence is proposed.

## Dependencies and acceptance gates

All dependencies remain review-pending, including their observed snapshots.
The JSON records exact observed hashes; leaf/config files can still change
before their own HOLD. The supervisor must reconcile the held full closure
before independent review or execution.

- Representation/constants and sign_of: leaf-owned IntegerRepresentation.cs;
  pointers and copy layout require native/managed size/alignment/stride/bool
  measurement and independent produced-code review.
- set_integer_value(long), complement_integer_value and cmp_integer_values:
  leaf-owned const_ints.cs and batch1 ledger. The signed setter's transitive
  signed_shift_right contract also remains pending. None is reimplemented here.
- In-batch add/subtract/shifts/get_part form the explicit call closure in the
  JSON; qualifying a caller requires qualifying its callees first.
- Caller incr_integer_value remains leaf-owned and calls this unsigned add.
  Other callers, initialization, multiply/divide, range/type/constant/formatting
  and diagnostic closures are outside this batch; no qualification propagates
  to them automatically.
- No actual independent reviewer has yet opened these produced arithmetic
  bytes. Self-inspection/hash generation does not supply a reviewer verdict.

## Authored validation, unexecuted

`tests/IntegerArithmetic/Program.cs` contains independent test-only UInt128 and
Int128 expectations. Production never uses those types. Tests cover distinct
signed/unsigned pairwise arithmetic over zero, limb carries/borrows, host/128-bit
boundaries and patterns; native alias diagnostic quirks; all mixed signedness
branches and equality; error overwrite from both initial bool values; output
storage and bool guards; exact alias partitions; shift counts 0,1 and both sides
of every limb boundary through 127; all four right-shift flag pairs; every mask
and sign-extension width 1..128 with positive/negative/truncated fields; signed
minimum negation and zero. Struct copies and disjoint operand storage are
checked for independent mutation. These are authored cases, not coverage proof.

The leaf-owned runner's `--stream` adapter implements the supervisor's test-only
native-oracle protocol, including these arithmetic operations and post-call raw
limbs/error/alias partitions. It avoids a duplicate adapter here. Supervisor
owns both native and managed execution and JIT/NativeAOT validation on the exact
held reviewed input set. No passing result, executed count or ABI measurement
is claimed by this worker. All tests/traces are pending. Any changed source,
helper, configuration or C# bytes invalidates affected review/validation.

Final owned hashes and explicit HOLD are recorded in `const_ints-batch2.sha256`
and the task's final handoff. No follow-on writes occur after that HOLD until
the supervisor requests a concrete repair.
