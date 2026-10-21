# P0 native contract review, round 1 — HOLD

Date: 2026-10-21. Independent contract review in the current shared worktree.
Authority: `../../PLAN.md`, `../../AGENTS.md`, and
`../coordination/RESUME-20261021.md`; explicit resume supersedes historical STOP.
Ownership is confined to this file and `REVIEW-TEMPLATE.md`. No implementation,
upstream, prototype, build, test, native preparation, or Git mutation is authorized
for this reviewer. No subdelegation occurred.

**Verdict: HOLD; no routine is parity-qualified by this review.** The proposed
fixed eight-ushort representation and bool/64-bit host work types agree with the
inspected source and recorded Linux x86_64 build assumptions. This is a contract
finding, not a measured ABI result or approval of future C# bytes. The supervisor
must hold every shared writer before executing validation.

## Early blockers and supervisor dependencies

1. **Exact native build/ABI receipt is pending.** Existing CMake records identify
   selections, not proof that an existing executable contains current source
   bytes. Pin one target's full compile command, generated headers, compiler,
   executable, runtime options and matching managed snapshot. Measure native and
   managed sizes/alignment/offsets on the held snapshot. Do not reuse an old
   executable hash as source-to-binary proof.
2. **Source-derived initializer width conflict needs investigation before the
   frontend initialization closure.** `targ_def.h:1184–1188` defaults
   `DEFAULT_BITINT_MAXWIDTH_VALUE` to 65535; no override was found in the inspected
   generated header. `target_early_init` (`target.c:1029`) assigns it to
   `bitint_maxwidth_value` (called in `fe_init.c:1770`).
   `fe_init.c:1953–1957` calls `target_init` before `const_ints_init`.
   `const_ints.c:2681–2735` includes both bit-precise kinds in initialization,
   computes size 8192 and storage width 65536 at eight target bits per byte,
   subtracts one for signed kinds, and passes 65535/65536 to
   `make_integer_value_mask` and then sign extension. The representation is 128
   bits. `make_integer_value_mask` subtracts this size_t width from 128 and casts
   to int; on the recorded usual GCC conversion this leads to a negative count
   and negative component shifts. These operations require native
   implementation-defined/undefined-behavior investigation, not an invented
   portable result. This is **not a confirmed runnable native defect**: no probe
   was run here. Do not add a clamp, reduce the enabled maximum, skip kinds, or
   use CLR masked shifts to make initialization pass. Isolated leaf review does
   not qualify this initialization/caller path.
3. **Aliasing is part of the API.** Native reference-to-pointer setter adaptation
   must bind existing storage and copy forward, with no hidden input snapshot.
   Arithmetic must preserve post-write rereads of operand 2. See below. A test
   suite limited to distinct inputs cannot close this dependency.
4. **CHECKING is enabled.** Any leaf that reaches `check_assertion`,
   `unexpected_condition`, a formatter, or an allocator also requires its actual
   failure/runtime closure. `Debug.Assert`, `Unsupported`, a no-op assertion, or
   an arbitrary managed exception is not an implementation of that closure.
   Routines outside the initial leaf set remain explicitly unported; no
   placeholder may be reachable on a qualified path.
5. **P0 inventory is not complete.** The available inventory summary calls itself
   PARTIAL and records 8,707 unresolved regions, without guard evaluation or a
   resolved call graph. Complete file enumeration is useful baseline evidence,
   but does not meet PLAN's complete initial routine/dependency inventory gate.
   Keep P0 open and manually close the selected leaf scope first; do not rename
   heuristic routine candidates as accepted coverage.

## Evidence classification and configuration

The original files were opened directly, including exact routine bodies in
`const_ints.c/.h` and required representation/identity sections of `basics.h`,
`il_def.h`, `targ_def.h`, `il.h`, `checking.h`, `util.h`, `types.h`, `error.c`,
`mem_manage.c`, `target.c` and `fe_init.c`. This is not a whole-file semantic
audit of every declaration in the 19,491-line `il_def.h`.

The existing `build/compile_commands.json` has different commands for `cpfe`,
`cpfe-cp`, `cdisp`, `cpfe-dot`, and `cpfe-dot-oracle`. Do not mix their macros.
The recorded `const_ints.c` commands use `/usr/bin/c++`, `-x c++`, C++14,
`-O3 -DNDEBUG -flto -fno-exceptions -fno-unwind-tables -fno-rtti`, and
`-DUSE_CMAKE_DEFINES` with `build/src/includes`. CMake records GNU 13.3.0,
Release, `EDG_MACRO_CONF=all-extensions`, no 32-bit build, no ASan, eight-byte
pointers, ELF and little endian. These are **recorded build facts**, potentially
stale, not newly measured host facts. C++14 here is the language used to compile
EDG; strict C++20 is a distinct input/runtime qualification profile.

`basics.h` includes `defines.h`; `USE_CMAKE_DEFINES` makes the latter include the
generated `cmake_defines.h`. Recorded selections are:

- `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0`;
- `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1`, `CHECKING=1`, `DEBUG=0`;
- `LONG_LONG_ALLOWED=1`, `INT128_EXTENSIONS_ALLOWED=1`, `FIXED_POINT_ALLOWED=1`;
- GNU and Microsoft extension compilation and IA64 ABI support enabled.

`BUILTIN_FUNCTIONS_ENABLED` defaults to 1 in `defines.h` when not overridden.
`MULTIPLE_THREAD_COMPILATION` defaults to false in `basics.h`; no override was
found in the inspected target commands/header. Thus EXTERN_THREAD does not by
itself authorize new per-session/thread-local state. `-DNDEBUG` does not remove
EDG's CHECKING branches. Runtime strict mode cannot erase compiled dependencies.
The scalar-representation `TYPE_FOR_AN_INTEGER_VALUE=unsigned long long` and
signed counterpart in the generated header are inactive for these routines.

## Source-shaped representation contract

`targ_def.h:1601–1679` defines the active representation:

- `an_int_value_part = unsigned short`, mask `0xffff`, sign bit `0x8000`.
- Host work signed/unsigned are `a_ptrdiff` / `sizeof_t`, which in the selected
  ANSI non-DOS branch are `ptrdiff_t` / `size_t` (`basics.h:530–554`). Under the
  recorded LP64 ABI, C# `long` / `ulong` match their expected widths. This is a
  host qualification, not a portable equivalence on every CLR/native platform.
- `INTEGER_VALUE_REPRESENTATION_SIZE` is the maximum of compile-time largest
  integer and enabled fixed-point sizes. INT128 gives the integer maximum 16;
  inspected fixed-point defaults are at most eight. With host short=2,
  host/target byte=8, the derived limb count is eight and total width 128.
- `an_integer_value` is a struct containing the array inline. `part[0]` is most
  significant; this logical order is independent of little-endian byte order
  within a ushort. Expected LP64 native size/alignment is 16/2, with
  `offsetof(part)=0` and two-byte element stride; measurement remains pending.
- A C# unsafe value struct with `fixed ushort part[8]` is an appropriate
  source-shaped candidate. Ordinary struct assignment must produce an
  independent value copy. A struct holding `ushort[]`, a normalized BigInteger,
  or a reference wrapper would change assignment/alias semantics.
- `a_boolean` and `a_byte_boolean` are native C++ bool under this profile
  (`basics.h:213–217`). C# bool is an appropriate in-process candidate; native
  storage width and pointer/output writes must still be measured. No default
  P/Invoke bool marshaling contract is implied or needed.
- Native int/unsigned-int width is expected 32, size_t/ptrdiff_t 64,
  `a_targ_size_t=a_host_large_unsigned`, `a_targ_alignment=unsigned short`.
  C# `sizeof_t=ulong` is a profile-specific mapping, not a claim that target
  C++ size_t is always ulong. Host containers and target integer kinds differ.
- Native ushort integer promotion to int, explicit work casts, truncation,
  complements, arithmetic right shift construction and unchecked narrowing must
  be audited expression by expression. A project-wide unchecked setting does not
  establish equivalent intermediate types. CLR shift-count masking cannot define
  an out-of-contract native shift. For valid limb paths, preserve widened work
  shifts (including 16), not narrowed ushort operations.

No heap allocation, graph, parser, interpreter, canonicalization, session state,
diagnostic engine, or frontend invocation is needed for pure value leaves. Do
not introduce one to implement them. This observation does not authorize omission
of any enabled dependency from a later type/constant/formatting closure.

## Native const-reference overload to C# pointer contract

`const_ints.c:50–59` and `79–88`, declared in `const_ints.h:119–126`, implement
separate overloads from the host scalar setters. Each source `const
an_integer_value& value` binds an existing object and copies
`intval->part[i] = value.part[i]` for unsigned i ascending from 0 through 7.
The source does not take a value snapshot or allocate. `intval == &value` is
valid and copies each limb onto itself.

An unsafe counterpart `(an_integer_value* intval, an_integer_value* value)` is
an acceptable **candidate language adaptation** only if its ledger states:

1. The second pointer denotes the exact original referent, is non-null on native
   valid calls, remains valid for the call, and is never written through as
   `value`. Native references do not add a nullable input case.
2. Call sites take the address of the original variable/subobject, preserve any
   native temporary's call lifetime, and do not clone before the call. Managed
   storage must remain pinned for every escaped/native-style alias lifetime.
3. The forward per-part copy and source order are retained, including self alias
   and any proven valid shared-storage use; replacing it with whole assignment,
   reverse copy, memcpy, memmove, or a by-value parameter needs a separate
   equivalence argument for the complete call domain.
4. Native `const` prohibits writes through this parameter, but is not no-alias or
   immutability of an object also reached through `intval`. C# pointer mutability
   must not become permission to mutate the source independently.
5. The scalar and aggregate overloads remain identifiable under the original
   names. Distinct pointer and long/ulong argument types select the intended
   implementation; every caller's choice belongs in the dependency ledger.

An `in`/`ref readonly` alternative also requires proof that C# binding introduces
no temporary/defensive copy. This round does not approve unproduced method bytes.

## Leaf branch and dependency obligations

The following is a source review checklist, **not a completed per-method C#
branch ledger**. Each implementation must expand it into every predicate arm,
loop entry/exit/body, short-circuit operand, return, goto/label, macro and
compile-time alternative with exact source and counterpart hashes. No branch
may be marked covered merely because its containing routine has a test.

- `signed_shift_right` / `sign_of` (`const_ints.h:26–52`): record both
  representation branches; active signed shift casts host-large to unsigned,
  shifts, conditionally ORs a sign-fill mask, casts back. Active sign reads
  `part[0] & 0x8000`. It is not a signedness tag on the stored value.
- Signed/unsigned scalar setters (33–47, 62–76): low limb first by descending
  index, mask then narrow/store then signed-fill/logical shift. Preserve all
  eight writes even for zero. Aggregate overloads (50–59, 79–88): forward copy.
- Host conversion (91–142): derive discarded bits, compute raw sign, choose
  required high fill using signedness, traverse high to low, flag each bad
  discarded limb while still collecting retained limbs, then check signed
  result sign. Store truncated result before error. Overflow still returns bits.
- `value_of_integer_value` (202–222) and unsigned wrapper (225–249): first clear
  `*ovflo`, call conversion with local value/error, preserve short-circuit order
  and final overflow write, return the original converted/truncated value.
- `cmp_integer_values` (284–334): short-circuit signed flags before sign reads,
  differing signs first; otherwise high-to-low unsigned limb comparison with
  greater branch before less branch and early breaks. No type/target table is
  needed for this raw-value comparison.
- `incr_integer_value` (634–643): construct local one then call unsigned add;
  discard the local error without replacing the native operation.
- Add (711–764): cache only sign_1, low-to-high carry propagation, then unsigned
  carry or signed overflow. **sign_of(*op_2) is read after op_1 writes.** With
  `op_1==op_2`, taking an early copy changes native overflow behavior.
- Mixed add (767–792): take the selected original value copy first, call add
  with op_1 signedness, then replace the error only when signedness differs.
- Mixed subtract (795–826): snapshot op_1, call subtract, then on differing
  signedness read operand-2 sign and compare original/result. Preserve the
  source `cmp > 0` versus `cmp <= 0` arms, including equality; do not repair
  these conditions using a preferred mathematical interpretation.
- Mask (829–850): zero, complement, logical shift in that order. Sign extension
  (853–865): left shift with ignored local error followed by signed extending
  right shift. No simplified mask or cast replaces this call sequence.
- OR/AND/XOR (868–907): descending indices, in-place first operand; preserve
  second operand identity. Complement (1109–1120): descending index, promoted
  complement then mask before narrowing.
- `get_part` macro (910–917): negative-index check then upper-bound check via
  short circuit; fill is the caller's local value; out-of-range limbs must not
  be dereferenced. Any helper needs explicit bindings, widened return type, and
  source-order argument mapping.
- Left shift (921–977): divide/remainder count into whole-limb and component
  offsets, traverse ascending to preserve unread input, read original work,
  test whole-part loss then boundary-part loss, combine two guarded source
  parts, mask and write; final error write comes last. Keep zero-count path and
  16-bit component shifts as implemented by the active multi-part branch.
- Right shift (980–1029): fill requires `is_signed && sign_extend`; derive fill
  from original sign before mutation; descending traversal, two get_part reads,
  mask and write. No error output exists.
- Subtract (1032–1091): cached first sign, descending borrow propagation then
  unsigned borrow/signed-overflow alternatives. Operand 2 is reread after
  mutation in the signed check, as in add. Negate (1094–1105): local zero,
  subtract with signed TRUE and caller error, then whole-value result copy.

The arithmetic worker's reported 711–1105 ownership also includes macros and
compile-time alternatives around that range. Sharing `sign_of` and setters
requires their exact signatures/types and reviewed snapshot, not duplicate
helpers with different semantics. Keep the partial type named `const_ints` and
source grouping evident; splitting into `const_ints.arithmetic.cs` is a file-only
adaptation with explicit source ranges, not license to fuse algorithms.

## Boundaries beyond this first leaf closure

Source-confirmed dependencies must stay open until ported and reviewed:

- Multiply/divide (1178–1746) require native local pointer redirection, work
  arrays, forward `copy_parts` and `clear_parts`, single-part normalization,
  quotient trial/retry, delayed output writes, sign restoration, `goto exit`,
  discarded/intermediate errors, and final error overwrite. They cannot be
  replaced by BigInteger or host division. The divisor-greater branch stores
  quotient zero before rereading `*orig_op_1` for remainder; do not reorder it
  when outputs alias inputs. Remainder has its own signed -1 shortcut.
- `swap_bytes_in_unsigned_integer` (2612–2645) checks compile-time
  `TARG_SIZEOF_LARGEST_INTEGER`, then CHECKING targ_char_bit==8, clears swapped
  **before** `copy_val = *value`, constructs a mask, and loops while bytes--.
  Source value==swapped therefore matters. A pre-clear input copy changes it.
- `conv_bytes_to_integer_value` (2649–2678) instead checks runtime
  `targ_sizeof_largest_integer`, zeroes result, chooses host byte traversal, and
  continues all shift/OR mutations after result becomes false. Its zero-byte
  little-endian pointer calculation (`bytes += num_bytes-1`) needs a native
  validity/domain review; do not turn questionable pointer behavior into a
  passing invalid-input test or silently add a guard.
- Formatting/float conversions need `a_number_buffer=Small_string<50>` and its
  actual allocator, append/resize/temporary-character/copy/move/destruction
  contracts (`util.h:5200–5216`). `General_allocator` uses alloc_general and
  free_general, and replace_alloc moves elements before freeing. General memory
  lasts through the backend and is reclaimed at mem_manage_wrapup; stack and
  arena/general lifetimes must not be merged. Float retry string conversion is
  enabled in this profile and cannot be removed as a convenience.
- Native checking expands to `assertion_failed` even in Release. The body in
  `error.c:3353–3404` strips filename, respects suppression, builds its message,
  calls internal_error; internal_error updates a recursion guard, emits a
  diagnostic or loop message, and terminates. This is a real diagnostic/state
  dependency. `f_unsigned_to_string_buf` (2360–2387) also has a checking branch.

## Native IL identity contract before expanding to constant/type routines

No replacement frontend architecture was present in the fresh source tree at
the initial inspection; only project scaffolding existed. This is not a blanket
approval of later additions.

`il_def.h` declares a_type/a_constant as pointer-identified structures, with
source correspondence first, linked next pointers and tagged unions. Type
based_types supports reuse but is not guaranteed exhaustive. `an_integer_kind`,
`a_type_kind`, and `a_constant_repr_kind` have explicit a_byte underlying types;
preserve enumerator order and configured alternatives, including the two
bit-precise kinds and ik_none==ik_last. `int_kind_is_signed` is a mutable
EDG_THREAD table: plain-char initially false is updated by target initialization
(`target.c:1076`). A frozen table inferred from selected fixture types differs.

`int_constant_is_signed` (const_ints.c:187–199) follows the actual typeref chain,
then tests tk_integer and indexes its integer kind. Do not replace it with a
new type-class hierarchy, structural-equality lookup, or generic skip routine
whose side effects/availability differ. `_BitInt` width lives in the actual
integer type supplement (`variant.integer.extra_info->bit_width`), not storage
size. `in_range_for_integer_kind` (402–439) and the type-range wrapper distinguish
`min_con == max_con` pointer identity and suppress repeated work accordingly.
Equal-valued separate constants are not the same entity.

Before claiming these routines, map all accessed original types, discriminants,
union members, bitfields, globals and callee chains. Record native offsets where
observed, mask/accessor transformations, allocation, pinning, arena ownership,
canonical identity, sharing, linked order and lifetime. An isolated integer
value leaf does not require fabricating partial alternative a_type/a_constant
objects. Disabled or Unsupported enabled branches block qualification.

## Review and validation gate

Use `REVIEW-TEMPLATE.md` for each routine/macro/type. Its branch ledger is
mandatory, separate from test cases. The supervisor must provide exact held
native/managed input manifests, obtain a fresh independent original/C# review,
then serialize relevant layout, arithmetic, alias, branch, JIT and NativeAOT
checks. For this closure include distinct/equal pointer operands, self-copy,
copy independence, carry/borrow propagation, signed and unsigned boundaries,
overflow outputs/truncated results, zero and limb-boundary valid shifts, and
order-sensitive error writes. Compare raw limbs, booleans and mutation traces
where output alone hides differences. Native addresses/padding are not values
to normalize indiscriminately; preserve logical alias partitions.

Any new source/config/C# byte invalidates the affected snapshot verdict. Tests
cannot approve missing branches or unresolved dependencies. A fresh review round
must include this round's objections and the exact repair response. Future
unproduced bytes are outside this round's acceptance.

## Snapshot record

Final pass and exact hashes follow. Source-range hashes use original file bytes,
inclusive one-based ranges, with existing line terminators retained. No build/test
evidence is claimed.

### Produced C# inspection in this round

The three C# source files became available during the review and were opened in
full alongside the original bodies. The exact inspected file hashes below bind
these observations. Other writers have not supplied a shared HOLD in this round;
this does not accept any later bytes or qualify any routine.

- `IntegerRepresentation.cs:13–17` has inline fixed ushort storage with
  sequential layout, Pack=2 and Size=16. Constants at 22–43 agree with the
  source-derived selected profile. Choosing the integer maximum directly at 32
  is equivalent to the original max calculation only for this pinned 16/8 size
  pair; retain the configuration ledger and do not advertise it as configurable.
- `signed_shift_right` at 47–51 reproduces unsigned cast/shift, negative-value
  conditional fill, and unchecked signed cast. The original macro can evaluate
  arguments more than once; the available caller passes only value and constant
  locals, with count 16. That caller-specific argument fact is necessary to
  review the method adaptation. `sign_of` at 55–58 reads live part[0].
- In `const_ints.cs`, both aggregate setters (35–42 and 60–67) use live pointers,
  unsigned forward loops and per-limb writes; no source snapshot, null guard,
  heap allocation or source mutation through the second parameter was added.
  This matches the **body-level** reference binding contract. Pin/temporary
  lifetime and overload selection at every future caller still require review.
- Scalar setters (20–32 and 45–57) preserve descending writes, mask/narrowing,
  and signed versus unsigned right-shift stages. Conversion (70–117) preserves
  fill selection, full high-to-low loop, both high-limb check arms, low-limb
  accumulation, final signedness check, and result-before-error output order.
  Its compile-time 64-bit discarded-width expression matches the pinned source
  result; this does not generalize to another host width.
- Comparison (120–151) keeps signed short circuits, differing-sign selection,
  unsigned high-limb scan, greater-before-less predicates and each early break.
  Increment (154–160) calls the shared unsigned add and discards the local
  error. OR/AND/XOR (163–193) and complement (196–203) retain descending loops,
  live operand access, promoted operations and explicit narrowing/masking.
- `const_ints.arithmetic.cs` add (14–47), mixed add (49–63), mixed subtract
  (65–89), subtract (183–216) and negate (218–224) retain their original local
  copies, carry/borrow branches, selected signedness, short circuits, post-write
  reads, and error write order. In particular, the two alias-sensitive sign
  rereads and mixed subtraction's `cmp <= 0` arm remain intact. No arithmetic
  correction or input snapshot was introduced.
- Mask (91–98) and sign extension (100–106) retain calls and ordering, with
  unchecked size_t subtraction/narrowing. Their `nuint` parameter is an explicit
  adaptation of native size_t; it matches the proposed ulong sizeof_t only on
  the qualified 64-bit host. Validate/record that boundary before acceptance.
- `get_part` (111–117) preserves negative-index-before-upper-bound short
  circuit, fill-before-dereference selection and ulong work representation.
  Passing fill explicitly replaces the source macro's captured local; the
  current arguments have no side effects. Left shift (119–150) retains ascending
  traversal, both ordered loss tests, two guarded source-part reads, mask/write
  and final error. Right shift (152–181) retains pre-write sign fill and
  descending traversal. These observations cover defined source shift domains;
  they do not resolve the oversized initializer counts above.

No body-level semantic mismatch was found in these inspected leaves under the
stated representation/valid-input assumptions. No replacement parser, graph,
interpreter, native fallback or Unsupported exit occurs in these three files.
This is provisional produced-code evidence, with ABI, caller/domain, complete
ledgers, held-snapshot review and supervisor validation still pending. The batch
ledger files referenced by source comments were not present at this inspection;
later ledger bytes need their own review. The unproduced host-value wrappers,
constant/type routines, initialization, formatting, multiply/divide and shim
closures receive no acceptance credit from this body inspection.

### Next review scope

The next independent round should open the held three-file snapshot, all actual
batch-1/batch-2 ledgers, the named native target/header manifest, and supervisor
ABI/arithmetic/alias/JIT/NativeAOT receipts. It must close every active branch,
source hash and dependency record using `REVIEW-TEMPLATE.md`, with prior findings
and responses included. Probe the initializer conflict separately before adding
`const_ints_init` or a frontend entry point to any qualified closure. Keep source
checking/formatter/allocator failures open until their full bodies are ported.

**Reviewer HOLD:** only the two owned review Markdown files were written. No
further writes remain after the snapshot append. No native preparation, build,
test execution, Git command, implementation edit or subdelegation was performed.

### Exact inspected source/configuration manifest

Paths are relative to `/home/marius/p/dotcc`. SHA-256 of this manifest
(exact sorted lines inside the fence, LF terminated):
`1cc9a17deaf179848f52c5162cdfa854534170e664ecd60669e2739d2509efd8`.

```text
0db60b149065dbdb6f260c2fa843f1f034283d03f2a0d0d944018ab96fe74ac7  edgcpp/build/CMakeCache.txt
12aa7c8bbf1cab72086c6cf13c49e3efb595ce9a89e3a1cbc73bf019b660bd12  edgcpp/build/CMakeFiles/3.28.3/CMakeCXXCompiler.cmake
568c80d00f1887a37a076c35bd8123fd1de1910cbecf73f18151d25bfb86d609  edgcpp/build/compile_commands.json
35c763753d20fe99c59649eb51aafccec7d7d61b6c227824d52d362fcc9e111f  edgcpp/build/src/includes/cmake_defines.h
907fadb2199931f6e190eccee217608f25bcb7d8f49b64a68cfa2257061ffdc4  edgcpp/manual-csharp/AGENTS.md
aa32bdc6f648f41f868fab38dd46546aa7781c3d65a3c6e0ad7d4a30ea60ce2b  edgcpp/manual-csharp/PLAN.md
bf66c25e7769bb1ade9b6f155e0478593c72248d889c10967ada3f14a1f3ea41  edgcpp/manual-csharp/faithful/coordination/RESUME-20261021.md
26464c8aa1b9cc00f1757c9932416cc026a7f70594137c3b36531c86bdc9dc4c  edgcpp/upstream/CMakeLists.txt
e3944295b342c75e2f7527668e689fa90a2e64b39a1ca5478ec63cfe9229cbf9  edgcpp/upstream/cmake/macro-conf/all-extensions/base.cmakedef
39f4990612a8a63f63a29fbf04239adba864d07c203435e51b0a983313de63b0  edgcpp/upstream/cmake/macro-conf/all-extensions/cpfe-dot-oracle.cmakedef
e4ca5b2fdd46f826ebde5e2d10b5cdcbd8938a0e6df998fa43348424332af29b  edgcpp/upstream/cmake/macro-conf/all-extensions/cpfe.cmakedef
65db84f37d6cc58ab159c1d7bf0077807273a83ee52ef6b937250338bee1ca03  edgcpp/upstream/src/CMakeLists.txt
be3354996241679dc89ad58ed66e002c363891ae6132fe205df61d3a2d4df5c2  edgcpp/upstream/src/basic_hdrs.h
65b5752f8f2e0e1eda6dccb59996bf996452df53b8ebd071dab5cb397918b0e9  edgcpp/upstream/src/basics.h
27aa5d201622e328f9b543af1e5b664c76777a19aa51b3364b2502dfd1cb874c  edgcpp/upstream/src/checking.h
952309891bc1662ed750201740ea9c569bef795aa010123d25f6b698d50739d8  edgcpp/upstream/src/cmake/cpfe-dot-oracle/CMakeLists.txt
3899ee8d6bbbd858e3c8ccccc6b8796d238f100305fa4c2b85f2a60854c11efb  edgcpp/upstream/src/cmake/cpfe-dot/CMakeLists.txt
e21c133e928b9a14c0eb34e78ca1399e63cdd38f9f58506a1e701135bd6b4d77  edgcpp/upstream/src/cmake/cpfe/CMakeLists.txt
5a16a2ebd362068f1757cc30f5b4828d67cc1bc16a8e6bfc519e43c0059deb2c  edgcpp/upstream/src/const_ints.c
23280caa487a713f3b1f382056a57c569c9a8d15ff07a218c47ecdea21158d3f  edgcpp/upstream/src/const_ints.h
4112aa9ba2c076dd9b83877bdfae6f107bd12952b6a12776baa6942ad114f18e  edgcpp/upstream/src/defines.h
e9a2fee3c39c8958361236285b57a87a98ceb02592a1de5e1035bd65e7bf3a60  edgcpp/upstream/src/error.c
6611a39baaf1de999aefa446966935878c7786d6fe142240526e830bda32d3e2  edgcpp/upstream/src/error.h
39b80f4d24d5cae46d85230f9bf978d024bc369431336de884f33a54fc62fedb  edgcpp/upstream/src/fe_common.h
7224d6888ef3bf52e37019f8554a609dc4521b3cf3685612d4c5f948a31e827f  edgcpp/upstream/src/fe_init.c
a8c9c5e79dbdd6be8c82f360c58d8681f91015e5e14e61039076b7f8c3b9c2c1  edgcpp/upstream/src/host_envir.h
6a2125d77c052ee8a98c04d5cb3b6b51ba6abc9220314d273af499c83e334a88  edgcpp/upstream/src/il.h
5d2e1ee19def818fc0012c7a862072c2149f3dfc382b032647863a9b0bb43fa6  edgcpp/upstream/src/il_def.h
4127523bae47ab55d89ed8ed99c067f70081bd44ee7aee3dfa5ea9c643b38305  edgcpp/upstream/src/mem_manage.c
e39ed09cdd9d150092231dce1d2b39032ffd442efe57ced0ad7636e12cf1958b  edgcpp/upstream/src/targ_def.h
c9566f5e831009c33946f1a25277dd376606b2a9bc60a21e48011472fa398b67  edgcpp/upstream/src/target.c
7bc5d8c9b472a2d72c724b628f384a49fa726f3c8d7406fedaf9107596375495  edgcpp/upstream/src/types.h
dfdac880faf2620d6098b570c46021b1bc56bdeac6a0d0ba0ba52a69b404306f  edgcpp/upstream/src/util.h
```

### Exact inspected managed snapshot

SHA-256 of this manifest (same rule):
`486cc7cd35d764458e7783fbc7a08ec32b3347f003352a6616712d154c92d1e0`.

```text
4379a643ecfb4ac1e71f7ba07b5787b3fdc828a60e6c6ea8e94b4b1967ef5dd5  edgcpp/manual-csharp/faithful/src/EdgFaithful/IntegerRepresentation.cs
414d0709aa1b5964555ff82fa1539488d2894cf44e4a0b6a580191f44cb55128  edgcpp/manual-csharp/faithful/src/EdgFaithful/const_ints.arithmetic.cs
cd5132dba30a65020fc4de262b56a32e48189075a0b6c3a8cb3681777374ac98  edgcpp/manual-csharp/faithful/src/EdgFaithful/const_ints.cs
```

### Exact body/range hashes for produced-code observations

All rows have verdict **provisional source correspondence; HOLD, not qualified**.
Native routine ranges include their inactive scalar alternatives where present;
only the active multi-part bodies were matched to C#. The branch/dependency
obligations above and template still apply. These range hashes identify bodies,
not branch coverage or executed tests. Native paths below are in upstream/src;
managed paths are in faithful/src/EdgFaithful.

| Entry | Native range / SHA-256 | Managed range / SHA-256 |
| --- | --- | --- |
| an_integer_value | targ_def.h:1673–1675<br>`d94abbbc78eface890f4a826b5ace3ba322995ef63eee4bce58f558996154a4c` | IntegerRepresentation.cs:13–17<br>`3fe5e73c1a0e9830a0a697a3702e11659e677c7a74e5bf0ed1cd193291eafd5b` |
| signed_shift_right [active macro] | const_ints.h:37–40<br>`c94dc7482ec5a681bbab56bad9d818dc5e40a58ad65673fb57b71eea8dfc9d80` | IntegerRepresentation.cs:47–51<br>`df571adc245b0536e6e5b13750bfd8f2d5d4be66def3fa7b97489d3ec6746419` |
| sign_of [active macro] | const_ints.h:47–48<br>`f7463fd6665f8739fa851d4a186ccd013487192410f499af53c7e771ab8ad116` | IntegerRepresentation.cs:55–58<br>`b7a80c3934919bb5255936458c17adb9abce18a7f4747db6a8922f335a92966c` |
| set_integer_value [signed scalar] | const_ints.c:33–47<br>`370bfcf85013f262c2f4777249033f0cee70fcef1a319bc93a5157b0cd55b159` | const_ints.cs:20–32<br>`8c659a02021ebee4f901b9e890821e91ac170b325e11d982a4c61395cf06dd58` |
| set_integer_value [const reference] | const_ints.c:50–59<br>`0b202f4319d3178c907bbac7365f756b31c4f6348fe585b6a5f5b8b227bce27d` | const_ints.cs:35–42<br>`8834e8f1a00213c8ff468318ec900484f585a3a7676b22b1236055559fc37887` |
| set_unsigned_integer_value [unsigned scalar] | const_ints.c:62–76<br>`99d9453b3d8d8de7cb01c15b4cae9ff8b04834de0a02a2e98a2a54ff7db8a01c` | const_ints.cs:45–57<br>`e953fef4f8f5ff1f0f6ccea01e3cbfed1797da175b2822adc49ac638b59d8a9f` |
| set_unsigned_integer_value [const reference] | const_ints.c:79–88<br>`6b3738c071746e8ba76c01362968feb10c3a794ca713588954c1d0da2c4415e4` | const_ints.cs:60–67<br>`97aaeed95af553f75847ea32b783b32d7566a2369f3bd5dc5cd09faa7b0d4629` |
| conv_integer_value_to_host_large_integer | const_ints.c:91–142<br>`5d23cfb67f8ebe0100fd600f81fef100f2795b1c6ea9fed95cb05674a419f2bf` | const_ints.cs:70–117<br>`6d4c7b894e9a5e595c688d94c939e9daa11abb4962c96b8e4518e127e5dc59b1` |
| cmp_integer_values | const_ints.c:284–334<br>`2da1f239d25742f50ae658e3b7029c90abf1662dcdd63596f6855f284b83efa4` | const_ints.cs:120–151<br>`25bf5b14f97a78ab17e8dd53413ba5deae8fb76e57fa1107305f50c20caa172f` |
| incr_integer_value | const_ints.c:634–643<br>`53f3229e41345c13294c0d87fe759cfdb052e1ab324bf9526ec5cd71f31bd562` | const_ints.cs:154–160<br>`f0a8a6d41f56e04000aa4ff839e9f5e0239e232ff5f36b3d1ba1d6ef36867567` |
| or_integer_values | const_ints.c:868–879<br>`210ec76bae1e21523d22cf13e679f79e2b6d04fb849bcd4d8c601fc4af8ed9d5` | const_ints.cs:163–171<br>`d6b47d0fa2c6ccd7cde727d540bd28c5ed58269e3d60edad6187a909762de40b` |
| and_integer_values | const_ints.c:882–893<br>`6e45b8bffbe02c0f4c2ec44fe713f1000bb95b3601491353c715542ed8d72654` | const_ints.cs:174–182<br>`5b41bce3fef2e6535613cc90d78eaad12107cab3a8602724a59b7d27b12560d9` |
| xor_integer_values | const_ints.c:896–907<br>`04520816fe893ef48c7c908ae2bd0667116a176e9ee4b99452081cd2bc41db83` | const_ints.cs:185–193<br>`9a0c31ad8fce5a19c79f651fd8e4323f452d1d65a60ebfcdce6f5a5278565e3a` |
| complement_integer_value | const_ints.c:1109–1120<br>`52aab16381a9e99578a643e9006c18354b10967539080c2595ad12a7eaf53f17` | const_ints.cs:196–203<br>`5700d79fbc26d0563e95323cd5fbc7be73d4d0710ff415dfb28152667da179ff` |
| add_integer_values | const_ints.c:711–764<br>`57aef4a9f0bc2bf7c102b6528b2f6088087d0c1ef71d56d9b0e9f0a0d04819d8` | const_ints.arithmetic.cs:14–47<br>`0b90f594556ee2eedd2b9cd01b72a1437cc45eacc90e57e9aad96342789a95cb` |
| add_mixed_signed_integer_values | const_ints.c:767–792<br>`5cfbf3468a035f02519cc3d1143019f0661fa17b299aa7f84de469408ee3e82f` | const_ints.arithmetic.cs:49–63<br>`107cf48ef73548e28512f3809fd1674008039271b0416e073d10662014a41f2d` |
| subtract_mixed_signed_integer_values | const_ints.c:795–826<br>`42726b70e21790a65196794a6b1f812ba370678e23484e13a3dba78a50f37174` | const_ints.arithmetic.cs:65–89<br>`669f2cbb86e8f86645cc48a79bfa0ae5ff631d735498ecc6d243d3f4cac3d76d` |
| make_integer_value_mask | const_ints.c:829–850<br>`11aaa3e8db571d9a89c273ce683afefa2da6b79f2bfcf75efb9190d42e533877` | const_ints.arithmetic.cs:91–98<br>`02a2764181eec5eccb662476d7e82df63e8432d6e3df44c6477c1c064e8710d9` |
| sign_extend_integer_value | const_ints.c:853–865<br>`5c320ebeded1145e8df74252dfb1773da955485d6ba650aed751f230e1fbf7cc` | const_ints.arithmetic.cs:100–106<br>`d28b090eb55aa04c09e30babc305da7ae8871c15e557bb1f5a362ae62d9a6c35` |
| get_part [macro/local capture adaptation] | const_ints.c:913–917<br>`54fc018a43951e51ea26f2ac7835ba45d418f8470759b9f3df24e3532146a2ef` | const_ints.arithmetic.cs:111–117<br>`a7a6dcf22f39a84307f2d64f33147f4e096578ec762d6deeded4f41156fc15db` |
| shift_left_integer_value | const_ints.c:921–977<br>`8b8c950dbf5b7d205c1cf2f3c785555efb41bf967dbf2789cdbde2955eb2d9d6` | const_ints.arithmetic.cs:119–150<br>`e346930e400b85aeb12c18ba84a6590ab023cf4f9dc765a6b22997cde5c2c535` |
| shift_right_integer_value | const_ints.c:980–1029<br>`c87053cc3bef929a3d03d7c42c64731ffdd844b674e37e5b196bcb9ad7b18faf` | const_ints.arithmetic.cs:152–181<br>`817884dc9899123586c0eeb2e753c0a67fd3bbb5b89773ddb1bb924eb6b3c650` |
| subtract_integer_values | const_ints.c:1032–1091<br>`cff6a7430da12cd9f26f1c62cba9c20f0d51fbc6f9dbb86bd4fde71e7bc604ca` | const_ints.arithmetic.cs:183–216<br>`d2b75015b1123daed265a949e63f75f41f25f5bddab7ee9487f013227c967d3c` |
| negate_integer_value | const_ints.c:1094–1105<br>`3987fcc84bdce3ce4a75a9e20ca9adfee85c73ba6987bb79ba88c5102211b8bf` | const_ints.arithmetic.cs:218–224<br>`bf9139709b02e4d55ded865705599db23a56d8a484b5652af1d4bf1509848fb8` |

This reviewer is now **HOLD**. Final owned-file hashes are returned in the
handoff, outside this self-referential document. Qualification remains zero.
