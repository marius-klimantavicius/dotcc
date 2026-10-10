# Original anonymous routine spelling and label lifetime overlap

Profile 50 preserves the captured source spelling of ordinary routines, including
an empty spelling for an implicit constructor of an unnamed member record. The
execution symbol remains unchanged. This fixes the ICU `MicroProps::helpers`
constructor correspondence in `number_formatimpl.cpp` and `numrange_impl.cpp`;
no source-name comparison is relaxed. Generated-only routine origins retain
existing naming rules. Stock `c_gen_be.c::dump_routine_name` uses the generated
execution spelling; that is separate from the original declaration metadata.

Label lifetimes may retain an exact predecessor destruction in their parent
block. `il.c::push_or_repush_object_lifetime` records that parent sublist when
entering a lifetime after a label. `lower_il.c::normalize_cleanup_state_for_outer_lifetimes`
and the normal exit traversal consume it, including their handling of temporary
lifetimes. The verifier now accepts this relationship only for a block or
block-after-label parent, retaining exact parent-owned destruction/allocation
membership, owner identity, ordinal and cycle checks. This fixes the source
lifetime of `quoted:` in DuckDB `QualifiedName::ParseComponents`.

Accepted immutable native121/managed138 evidence, rooted at
`/mnt/storage/dotcc-edg-campaign-20261003`:

- `dotobj-v50-anonymous-name-121-138-001/results.json`: 24 checks, including
  native C++, stock C, source-hidden verification, managed execution, NativeAOT,
  and independently resealed original/execution source-name mismatches.
- `dotobj-v50-label-overlap-121-138-001/results.json`: 23 checks, including
  native C++, stock C, managed and NativeAOT cleanup traces through backward
  goto and exception exit, plus foreign destruction and invalid parent-kind
  metadata rejection.

These focused results establish the two bounded contracts. They do not imply
that the complete DuckDB program has been linked or executed.
