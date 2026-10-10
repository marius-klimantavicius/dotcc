# Scalar member-function-pointer transport

ICU UnicodeSet::applyPattern accepts `UnicodeSet& (UnicodeSet::*)(int32_t)` by
value. EDG lowers that source scalar into its generated member-function-pointer
record. Our generic class transport guard required original class copy facts
which cannot exist for this representation.

Stock lower_il.c::make_mptr_type defines the LP64 layout: one erased code-address
word and one ptrdiff_t adjustment word. Its exact creation is already observed,
and SourceRecords verifies both field roles, offsets, sizes and alignment.
SourceMemberPointerTypes separately retains the original class and member type.
Stock C copies, returns and passes this value directly; the retained reducer
shows nonzero this adjustment and ordinary containing-record copying.

The patch admits scalar value/copy operations only for that observed ABI key,
not other generated ABI records. It preserves volatile/atomic guards, exact
layout verification, original class operation facts, and all source signatures.
The ABI row stays explicit-only: it is not a fabricated trivially-copyable
source record. No new tables or source syntax escape are introduced.

Frozen121 rejects the retained reducer at its by-value return. Native C++ and
stock C O0/O2 each execute successfully; the integrated122 export passes. The local-initializer verifier recognizes only
a retained source member-function-pointer mapped to this exact physical ABI
record, including the original owner and routine member type. Existing
declaration, initialization kind, zeroing and body target proofs remain mandatory.
Immutable native122/managed142 passes 31 checks including managed execution,
NativeAOT and resealed ABI identity, original initializer type, copy-policy and
adjustment-field type rejection. Receipt:
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v51-member-pointer-122-142-002/results.json`.


# Virtual wrapper result-buffer forwarding

A secondary-base virtual function returning a nontrivial record uses a
caller-owned result pointer. ICU MutablePatternModifier::getSymbol returns
UnicodeString this way. A small nontrivial Result fixture reproduces the exact
wrapper formal gate on frozen121.

Stock lower_il.c::make_wrapper_routine copies the overriding routine parameters,
then lower_init.c::add_body_for_wrapper_routine creates each lowered formal.
For a lowered void return it inserts the selected target call as an expression
statement and leaves a void return. The retained C output forwards the incoming
Result pointer unchanged, adjusts only this by -8, and calls Derived::make.

The existing thunk creation/body provenance plus target source ABI suffices.
The patch requires both source return ABIs to be result-buffer, equal original
return types, equal execution slots, void physical return, and no covariant
return adjustment. Exact formal forwarding remains mandatory. The owned result
binding must occur only once, as the selected forwarding argument: no wrapper
copy, result storage allocation, result-pointer mutation or new lifetime.
Existing direct-return/covariant-pointer wrappers retain their current behavior.

The enhanced reducer deletes its copy constructor, checks one construction and
one destruction after successful return, then throws during construction and
checks no extra destruction. Immutable native122/managed141 passes 27 checks:
native C++ and stock C O0/O2, source-hidden verification, managed execution,
NativeAOT, and resealed result-adjustment, missing-forwarding and extra-result-use
rejection. Receipt: `/mnt/storage/dotcc-edg-campaign-20261003/dotobj-v51-result-wrapper-122-141-001/results.json`.
Real ICU correspondence still requires fresh project generation acceptance.
