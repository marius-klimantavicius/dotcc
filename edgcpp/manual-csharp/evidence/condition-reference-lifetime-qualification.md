# Header-reference temporary lifetime exception

Root authorizes a narrow intentional exception to the pinned native interpreter,
after independent source review, exact native controls and C++20 draft rules.
Managed reference-variable temporaries keep their actual header activation owner
through body and for increment; per-test scope exit expires them. Valid binding
is never relabeled invalid to imitate premature native expiry. Acceptance still
requires held managed identity/lifetime/failure-cleanup tests and independent
review. This is not a general EDG parity claim.

## Exact observed controls

All source inputs are retained under tools/oracle-inputs/m4-condition-contracts,
with native-condition-contract-NAME-round1.json recording exact bytes/compiler
SHA/profile: strict C++20/LP64, used instantiation, no-codegen, predefined file
disabled, explicit two-argument UTF8 source kind.

The expected-accept reference-increment, if-reference-temp, while-reference-temp
and if-init-reference-temp controls each actually reject2 with an expired-storage
note on a body read. These four matched=false expectation mismatches are retained
and excluded, not language-negative passes. The initial all-eight-matched summary
was immediately corrected against actual receipts.

Ordinary-reference-temp accepts0 with3; for-reference-existing accepts0 with6.
Thus ordinary local reference extension and a condition referencing existing
storage work under this native profile. Nonreference for-increment accepts6,
empty-init accepts3, else-scope accepts5, nested-shadow accepts2 and the combined
lifetimes source accepts21. Genuine redeclaration controls for-redecl/else-redecl
reject2 without false assertions. Reference-dangling separately rejects2 after
returning a reference to an expired condition temporary; managed correct extension
must preserve this negative after scope exit.

## Normative and algorithmic basis

[N4861 class.temporary6](https://timsong-cpp.github.io/cppwp/n4861/class.temporary#6)
extends a materialized temporary bound directly to a reference, with explicit
exceptions that do not include condition declarations or selection initializers.
[stmt.pre4](https://timsong-cpp.github.io/cppwp/n4861/stmt.pre#4) makes a nonexpression
condition a declaration. [stmt.while2](https://timsong-cpp.github.io/cppwp/n4861/stmt.while#2)
recreates the declared variable per test, and
[stmt.for1](https://timsong-cpp.github.io/cppwp/n4861/stmt.for#1) places increment
inside the equivalent while body. Managed storage therefore uses the actual
reference-variable lifetime owner, retaining through increment and expiring
on each iteration or exit, rather than the shorter condition full-expression.

The inspected EDG source marks reference extension without a special condition
exception. interpret.c do_constexpr_init_variable8113–8119 manages nested
extension_state; enk_temp_init32439–32458 allocates the extended object. The
outer do_constexpr_condition8250ff saves its storage state before initialization
and restores that earlier state8316 before the body. This independent source
trace plus expired-storage controls supports a pinned interpreter limitation
in header-extension storage preservation. It is an inference about this actual
branch, not a claim that all configured EDG builds have the same behavior.

Managed assertions must prove actual per-test temporary/cell/alias identity,
reference owner retention through for increment, cleanup on false test/break/
continue/return/failure/resource reuse and dangling after scope exit. The
separate CHR1-01 redeclaration finding is a real implementation repair, not
covered by this exception. Other differences require independent investigation.

Consulted interpret.c SHA256: d3f34d737a6690e482942a52a463e406b767a0d5fc06b3609c8cb9c79b27cb4d.
