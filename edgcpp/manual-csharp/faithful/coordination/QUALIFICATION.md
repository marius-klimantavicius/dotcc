# Qualification checkpoint: original integer source contracts

Independent review: [integer-held-r2](../reviews/integer-held-r2.md).
Exact input/output and source snapshots: ../evidence/integer-first-gate/.

18 routine bodies qualify for their complete source-valid typed-storage and
argument contracts under the exact pinned profile P. This is independent source
review plus finite native/JIT/NativeAOT evidence; not a formal proof, unrestricted
caller acceptance, P0 completion or a frontend API.

Qualified routine records:

- set_integer_value(host-signed)
- set_integer_value(const-reference)
- set_unsigned_integer_value(host-unsigned)
- set_unsigned_integer_value(const-reference)
- conv_integer_value_to_host_large_integer
- cmp_integer_values
- incr_integer_value
- or_integer_values
- and_integer_values
- xor_integer_values
- complement_integer_value
- add_integer_values
- add_mixed_signed_integer_values
- subtract_mixed_signed_integer_values
- shift_left_integer_value
- shift_right_integer_value
- subtract_integer_values
- negate_integer_value

`make_integer_value_mask` and `sign_extend_integer_value` are domain-reviewed
only for widths 1..128 and remain unrestricted-unqualified. They are excluded
from the 18. Three macro counterparts have call-specific contracts, not general
macro acceptance. INIT-001 remains open; no native change/clamp is authorized.

The initial routine files, ledgers and held evidence are committed as this
precise checkpoint. New runtime source and all pending bodies have separate
unresolved review/execution/dependency gates; no acceptance transfers to them.
Full P0–P6 continues.
