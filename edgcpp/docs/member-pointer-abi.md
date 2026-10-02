# Managed member-pointer ABI integration

The next target version will select EDG's existing
`TARG_IA64_ABI_USE_VARIANT_PTR_TO_MEMBER_FUNCTION_REPR=1` configuration.
This is a supported target choice, not a change to EDG lowering. The ordinary
Itanium representation tags a function-address bit, which is not a valid
assumption for CLR managed function pointers.

The variant descriptor occupies 16 bytes with alignment 8:

| Offset | Meaning |
| --- | --- |
| 0 | Untagged managed function address, or virtual-table byte offset |
| 8 | Signed `this` adjustment shifted left one bit, with virtual tag in bit 0 |

EDG remains responsible for null representation, conversions, comparison,
adjustment and call selection. The backend admits only the canonical lowered
member-function-pointer record, identified by EDG's semantic type predicate,
and validates both fields and their offsets. Unrelated compiler-generated
record fields remain subject to their own explicit handling. Data member
pointers retain EDG's offset representation and null sentinel.

Integration must introduce a new explicit target profile and record its
descriptor contract in the manifest. The prior profile can remain available
for its existing feature subset; it must not silently acquire member-function
descriptor support. The original C oracle receives the same variant
configuration for focused comparisons. The managed callable still uses the
existing static adapter and optional explicit owner parameter.

Prepared synthetic acceptance covers 48 cases: data and function members,
virtual/nonvirtual calls, null and virtual slot zero, base/derived adjustment,
arrays/copies/returns, evaluation order and forced GC, under static and
independent-owner JIT/NativeAOT configurations. Configuration evidence must
show the variant flag enabled for both managed and C-oracle frontends.

One defined C++17 sequencing case uses a separate direct C++ source oracle:
assignment evaluates its right operand before its left operand. Original EDG
C output leaves that order to the C compiler; GCC and Clang produce different
traces for that C expression. Both C++17 compilers agree, and the managed
backend must match the C++17 trace. This difference does not justify changing
EDG. The existing typed assignment helper already evaluates RHS before LHS.

This document describes the pending integration. Prepared tests and native
oracle traces do not establish passing managed member-pointer execution.
