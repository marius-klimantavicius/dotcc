# Owned C++ record layout proposal

Status: the first complete own-vptr/ABI-record subset is implemented in v15;
base/subobject views and the remaining broader design below are pending. This
work changes neither EDG layout nor its lowering decisions. It separates
original declarations from the physical records that EDG already constructs.

## Actual DuckDB failure and its reduction

The original `src/common/exception.cpp` TU parses successfully. The retained
`dotobj-duckdb-project-010/results.json` receipt rejects `record:std::type_info`
while resolving libc++ 18.1.8 `__functional/function.h:517`, in the static policy
construction path. That policy contains a `const std::type_info*` and, with RTTI
enabled, `&typeid(void)`.

The immediate `std::type_info` blocker has a virtual destructor; it does **not**
itself inherit from another class. Its original source declaration has one
`__type_name` member. Rejecting it as “outside complete plain layout” currently
prevents even retaining a pointer to that otherwise ordinary 16-byte layout.

Evidence lives under `/mnt/storage/dotcc-edg-campaign-20261003`:

- `dotobj-duckdb-project-010`: original DuckDB command, diagnostic and source hash.
- `dotobj-duckdb-layout-research-004/input.cpp`: reduction including the actual
  libc++ `typeinfo` header, returning `&typeid(void)` and reading `name()`.
- `dotobj-duckdb-layout-research-004/input.int.c`: stock EDG C backend output,
  exit 0. It declares `_ZSt9type_info` with `const long *__vptr` followed by the
  original `__type_name` pointer. Exported `info_size` and `info_alignment`
  constants are 16 and 8. `void_info` casts the address of `_ZTIv.base` to
  `std::type_info*`; the EDG ABI metadata record and the original libc++ class
  are distinct record identities with compatible pointer views.
- `dotobj-cpp-layout-research-001/stock.int.c`: base-tail-reuse reduction and
  corresponding legacy C# type metadata, described below.

The stock frontend does not install our portable system-target hook. The
successful reduction passes the same target-fact definitions from
`backend/target/portable-lp64-binary80-v2.def` explicitly; its invocation is
retained. A full stock-C replay of the DuckDB TU was **not** successful: the
stock frontend's immutable host OS macros select system locale headers that
our portable-header invocation does not select. Attempts 001–003 retain that
failure. The reduction establishes these layouts and stock-C lowering choices;
it is not a claim that the whole DuckDB TU was exported by the stock backend
under an identical macro environment.

## Physical evidence that prevents a simple field-list relaxation

| Record/view | Native extent | Native alignment | Relevant layout |
|---|---:|---:|---|
| Complete `VirtualBase` | 16 | 8 | vptr at 0, int at 8 |
| `__SO__11VirtualBase` subobject view | 12 | 8 | same payload, no final four bytes |
| `VirtualDerived` | 16 | 8 | base extent 12, derived int at 12 |
| Complete `PlainBase` | 8 | 4 | int at 0, char at 4 |
| `__SO__9PlainBase` subobject view | 5 | 4 | same payload, no final three bytes |
| `PlainDerived` | 8 | 4 | base extent 5, derived char at 5 |

Stock `c_gen_be.c` flattens a base field marked
`class_subobject_with_tail_padding` into the containing record. Its base view
still has its own semantic identity. A complete `VirtualBase` CLR field would
overlap and potentially overwrite the derived int; rounding the subobject
view's extent to its native alignment would also change its stride. Likewise,
`[[no_unique_address]]` members can reuse tail bytes.

Stock `eok_bassign` emits a byte copy whose count comes from the actual lowered
operand type's size. The owned graph must preserve that extent instead of
substituting a complete-class assignment or `sizeof(complete-class)`.

## Proposed owned tables and existing-table extensions

All IDs below are object-local integer identities, never EDG pointers. Source
positions use existing source-file IDs and line/column columns. Exact spelling
and future macro expansion mappings remain provenance, not linker keys.

New source tables:

| Table | Columns |
|---|---|
| `source_record_types` | `source_type_id PK`, `identity_key`, `source_name`, `record_kind` (class/struct/union), `is_complete`, `native_size`, `native_alignment`, `capture_status`, `unsupported_reason nullable`, source position |
| `source_record_fields` | `id PK`, `record_source_type_id FK`, `ordinal`, `source_type_id FK`, `source_name`, `is_mutable`, `has_no_unique_address`, `bit_width nullable`, source position |
| `source_record_bases` | `id PK`, `derived_source_type_id FK`, `ordinal`, `base_source_type_id FK`, `access_kind`, `is_virtual`, source position |

`source_record_bases` records direct bases in declaration order. Preserve the
original typedef spelling through `orig_type` when provided. Physical indirect
or shared virtual-base placements belong to the execution layout rather than
invented extra source base declarations.

Execution table extensions:

| Table | Added/changed columns |
|---|---|
| `record_types` | `representation_kind` (complete/subobject/abi), `source_record_type_id nullable`, `complete_type_id nullable`, `physical_extent`, `native_alignment`, `value_copy_kind` (ordinary/explicit-only); existing nominal `identity_key` and layout kind retained |
| `record_fields` | `field_role` (source/base/vptr/abi), `source_field_id nullable`, `source_base_id nullable`, `storage_extent`, `storage_kind` (physical/view); existing execution `type_id`, offset and bit metadata retained; `source_type_id` nullable for true hidden fields |
| `source_types` | execution correspondence nullable for source-only unsupported/uninstantiated descriptions; add lvalue-reference/rvalue-reference kinds when their lowering is implemented |

A complete record has `physical_extent == size`; a subobject has the exact EDG
subobject extent and a `complete_type_id` link. Its native alignment can exceed
an alignment that divides its extent. Validate extents and alignments
independently. An ABI record without an original declaration explicitly lacks
a source-record identity.

A physical field owns meaningful storage payload. A view names a logical base
or tail-reusing member whose address can be formed, but does not allocate a
complete CLR field. Zero-extent optimized-empty bases remain source bases and
execution views; they are not discarded or assigned a fabricated byte extent.
An execution field can map to at most one original source field; one source
field can map to several execution copies, including a subobject representation.

Function ABI mappings are a necessary following tranche, not solved by these
record columns. v13 now retains each original routine source type. C++ hidden
`this`/result-buffer parameters need explicit execution-parameter roles and
source ordinals; generated routines need explicit synthetic provenance. Source
and execution parameter lists must not be silently forced equal.

## Native extraction and read-only observation hooks

Use the original record snapshot for source fields, direct bases and source
types. Use the **lowered actual** `field_list` for execution layout; do not
relabel a post-lowering field list as a before-lowering capture.

Authoritative EDG facts:

- `a_field::is_lowered_base_class` identifies base storage inserted by lowering.
- `class_subobject_with_tail_padding` identifies a base/member whose tail bytes
  can be reused; `has_no_unique_address_attribute` retains the source feature.
- `class_type_supp(type)->has_subobject_type` and `subobject_partner` connect
  complete and subobject representations. `make_subobject_class_type` sets the
  subobject size/alignment and copies fields.
- `a_base_class::next_direct` is declaration order; `orig_type`, `type`,
  `decl_position`, direct/access/virtual flags are source facts. Lowered base
  placement offsets and primary/virtual sharing are execution facts.
- `prelower_class_type` inserts the vptr through `add_dummy_field` only when
  this record needs its own vptr. A primary base can own the shared vptr.
- Generated subobject fields are created by `copy_field`; it copies the whole
  field entry and changes the owner. There is no durable source-origin pointer
  in the copied field itself.

Add an observation hook `observe_field_copy(original, copy)` immediately after
that EDG copy. Resolve the ultimate captured source-field ID through this
private map. For generated base/vptr fields, observe creation at the lowering
site and record the explicit role and original base identity. Such hooks only
copy facts into backend-owned structures. Do not identify vptrs or bases by
`__vptr`, `__b_` or other spelling heuristics, and do not invoke lowering helpers
from the exporter to manufacture missing metadata.

In the current EDG source, `add_dummy_field` has exactly one caller: the own-vptr
branch of `prelower_class_type`. Its returned `add_field` identity can therefore
be observed as a vptr at that explicit creation site. Base creation has four
call sites, including the primary-base, direct-base, non-IA64 virtual-pointer,
and virtual-base paths. Each caller has the actual `a_base_class_ptr bcp`.
Carry that exact identity into the observation rather than matching only the
base type and offset: those facts alone do not identify a path through repeated
or shared bases. Observation code must preserve the helper's existing return
behavior and all layout decisions.

Capture proof requires an observation at the start of the
`!class_has_been_prelowered(class_type)` branch of `prelower_class_type`, before
`compute_vbase_and_vcall_offset_indices` and every synthetic field insertion.
The existing `lower_top_level_scope` snapshot is useful, but the existing
`lower_type` entry hook alone cannot establish this boundary: class prelowering
can already have happened. Reuse an earlier snapshot only after checking its
original field identities against this boundary, then seal that origin map.

Observe `make_lowered_class_type` as a separate creation boundary. In
`lower_eh.c`, both the `__EDG_type_info` record (near line 350) and RTTI-derived
ABI records (near line 561) come through it. They have real execution fields but
no invented original class declaration. Subobject records also pass through
this helper; their later explicit complete/subobject partner relationship
refines the role to subobject. Generated ABI identities need a stable purpose
and representation key, rather than traversal-dependent emitted names such as
`__C1`. The first type-info reduction therefore exercises original own-vptr
layout **and** separately classified RTTI ABI records.

For the later function ABI tranche, `a_variable::is_this_parameter` identifies
the implicit object parameter. Routine-extra flags
`value_returned_as_parameter` and `return_value_parameter_follows_this` identify
the lowered result-buffer convention and its ordering relative to `this`.
Snapshot original parameter identities before lowering, then associate retained
parameters by identity and generated parameters by these explicit ABI facts.
Do not recover their roles from names or compare source and execution arities
as though hidden parameters were source declarations.

Source identity must distinguish template arguments and local declarations.
Use EDG's ABI discriminator for ABI-identifiable C++ classes. v13's owned
occurrence discriminator fixes distinct anonymous C records arising from one
macro callsite; it does not by itself establish cross-TU equivalence for local
classes in different template instantiations.

## Managed representation and checks

Ordinary physical C/POD fields retain source names and normal member access.
Complete records retain their complete-object stride. Hidden vptr fields are
explicit unmanaged fields with their real lowered pointer type. Existing
integer-slot relocations already accommodate vtable address bits.

For a tail-reusing base, flatten its physical leaves into the containing CLR
storage, while keeping a logical typed address view in the owned graph. Name
flattened backing fields from stable IDs and source names; generated spelling
is not semantic identity. Member access through the logical base view can use
an address calculation where C# cannot express it directly. This follows the
stock C backend and confines incompatible syntax to C++ representation needs.

Subobject-only storage must never accidentally acquire a complete-object
stride. If a separate CLR view type is emitted, validate its actual `sizeof`
and use a pack compatible with the exact extent; native allocation/local
address alignment is enforced independently. A view type is not a license to
allocate under-aligned objects or copy the complete parent. Packed layouts and
unrepresented ABI modes remain explicit rejections until separately verified.

Represent lowered byte-copy operations with explicit byte count and owned
source/destination expressions. Evaluate each address once. Complete ordinary
POD assignment can continue using the already-tested InlineArray-backed CLR
value copies. Nontrivial constructors/destructors and exception exits still
require their lifetime protocol; admitting their layouts does not admit
unrepresented cleanup semantics.

Both current native `copyable_value` and managed `CanCopyRecord` recursively
inspect fields. That predicate was bounded by the earlier plain-layout
rejection and must not become permission to copy a class merely because its
newly admitted vptr and data fields are individually copyable. Preserve an
explicit `value_copy_kind`: ordinary complete, trivially copyable source
records may use existing value operations; nontrivial records and subobject
views require explicit lowered operations. Capture the frontend's completed
copyability fact before lowering (the existing `is_trivially_copyable_type`
classification is authoritative), and preserve the separate restriction
against volatile/flexible-array aggregate copies. Merely adding a vptr must
not silently enable source assignment, a by-value ABI, or object destruction.

## Concrete first patch boundary

The first enabled subset is complete, non-packed records with their own vptr
and no bases or overlapping fields. It exports the general source/execution
correspondence rather than a `std::type_info` special case. Do not enable base
or subobject layouts until their physical-view emitter is implemented.

1. In `dot_gen_object_source.h`, give original captured fields stable owned
   IDs and keep source-field declarations separate from physical layout data.
   Add backend-private maps for copied-field origins and generated field roles.
   Snapshot completed source copyability. Preserve source bases for the broader
   semantic index even while their execution layout remains unsupported.
2. Coordinator observation hooks retain the exact field returned by the
   own-vptr `add_field` call and report its role. The `copy_field` hook reports
   original and copy identities after linking the new field. No guessed names
   or frontend layout changes are required.
3. In `Writer::record_type`, register the record identity before visiting its
   recursive field types, emit the original source declarations once, then
   traverse the actual lowered `field_list`. Original fields resolve to their
   source IDs; observed generated vptrs have no invented source declaration.
   Reject any remaining unclassified hidden field. Check actual offsets,
   native extent/alignment, and source-member correspondence independently.
4. Managed `Records.cs` reads roles and origins, and `SourceProvenance.cs`
   validates original source fields separately from hidden ABI fields. Existing
   CLR explicit-layout fields suffice for this first subset. `Linker.cs`
   compares the full physical layout and original-source correspondence across
   TUs, including roles and copy policy. No semantic behavior follows from the
   generated field spelling.
5. Verify the actual libc++ reduction, an independent virtual class with named
   data members, cross-TU pointers to that class, and rejection of a forged
   ordinary-value copy of the nontrivial class. Re-probe original DuckDB and
   record its next genuine ABI/lifetime requirement.

## Implementation split and acceptance gate

Native owner: snapshot original fields/bases, define stable source identities,
install observation-hook interface, export physical records/views and origins,
retain explicit copy extents. Coordinator: versioned schema, hook installation,
packaging, documentation and serialized builds. Managed owner: read/validate
source and execution layouts independently, canonicalize cross-TU identities,
emit physical fields/views, validate CLR sizes/addresses and byte-copy extents.

First gate: the actual libc++ `std::type_info` reduction retains the original
`__type_name` declaration and the hidden vptr with size16/alignment8; `_ZTIv`
metadata and its typed view remain separate. Next gates: nonvirtual inheritance,
multiple inheritance, tail reuse, virtual inheritance and no-unique-address,
including callbacks through vtables and cross-TU definitions. Compare native,
JIT and NativeAOT results, plus pointer deltas, complete/subobject sizes and
copies that must preserve derived tail members.

Re-run the original DuckDB `exception.cpp` after each coherent gate. It must
progress because represented semantics expanded, not because constructors,
RTTI, unsupported source metadata or destruction obligations were discarded.

## Alternative: exact-sized CLR subobject records

The stock C backend flattens tail-reusing base fields because an ordinary C
struct's extent is rounded to its alignment. The owned representation need not
inherit that emission constraint. A separate managed subobject type can retain
its exact physical extent with explicit field offsets and an appropriate CLR
pack, while its complete nominal type retains the complete-object extent.
For the measured examples, a five-byte subobject can use pack 1 and a twelve-byte
subobject can use pack 4. Both continue to carry native alignment 4 and 8,
respectively, in the owned object. A containing derived record can embed the
exact-sized subobject field without overwriting its own tail-reusing member.

This alternative can preserve named `base.member` access and avoids flattening
all nested field paths. It must not silently equate CLR alignment with native
alignment: subobject locals, temporaries, arena addresses and native allocations
must satisfy the recorded native alignment independently. A pointer converted
to the complete base retains that complete type's pointer stride, as in C++.
Subobject copies remain explicit-only and use the actual lowered copy extent;
an ordinary complete-record assignment is not interchangeable with that copy.
The existing source field-copy observer supplies the original field identities
for both physical representations.

A zero-extent optimized empty base cannot be an ordinary CLR field, since an
empty CLR value type still has a nonzero extent. It remains an addressable
logical view with its recorded offset, with no extra physical storage. Overlap,
virtual base placement and `no_unique_address` still require explicit owned
relationships; a choice of packing does not establish those relationships.
This option is under assessment, not an implemented layout guarantee.

The detached managed experiment at
`/mnt/storage/dotcc-edg-campaign-20261003/dotobj-subobject-view-managed-research-001/results.json`
passes JIT and NativeAOT checks for exact five-byte/pack-1 and twelve-byte/pack-4
subobject structs. This establishes the bounded CLR layout technique; the
native exporter and owned layout verifier still reject base/subobject layouts.
It does not establish empty-base views, virtual inheritance, copy lifetimes,
or the full cross-platform execution contract.
