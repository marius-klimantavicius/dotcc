# Next semantic contract batch

This contract was frozen after verified checkpoint `b7f52f778`: 243 tables and
39,067 append-only stable kinds. Both workers implement it with disjoint ownership.
No new approval is required: these are retained semantic identities and execution
facts in the already-authorized phases. Source coordinates are optional under
the user's 2026-10-13 steering; preserve direct cheap metadata without guessing.

## Attribute identities and exact storage associations

- Keep `attribute` as the canonical actual a_attribute pointer identity and
  payload. Remove sole-owner/ordinal columns. Add ordered
  `owner_attribute(owner_id entity, ordinal, attribute_id attribute)`.
- Make `attribute_argument` an entity keyed by actual a_attribute_arg pointer,
  retaining its present kind/scalar/constant/type/expression/token/source/pack
  payload. Add ordered `attribute_argument_link(owner_id attribute, ordinal,
  argument_id attribute_argument)`; remove sole-owner/ordinal payload columns.
- Add the entity catalog tag only if not already present; append stable IDs.
  Standalone reflected attributes/arguments can exist before any owner-list
  visit. Owner relations always come from actual retained lists, not copied
  positions, names or text. Distinct copied attribute entries remain distinct.
- Reader: `SemanticEntity.Attributes` follows the ordered relation; `Attribute`
  exposes paged `Owners` and optional unique `Owner`. `Attribute.Arguments`
  returns canonical objects; `AttributeArgument.OwningAttributes` exposes the
  reverse associations. Type symbols project descriptor attributes as before.
- Add `constant.associated_storage_variable_id -> variable_symbol`, optional
  occurrence-level association under resident separate-backend lowering gates.
  Do not attach it to interned values, infer by names, or call an allocating
  EDG helper. Reader `Constant.AssociatedStorageVariable` is ordinary navigation.

## Canonical lifetime bindings and initializer roots

- Add `block_descriptor` for actual a_block, containing the current
  block_statement scope/lifetime/final-position/reachability/statement-expression/
  implicit-scope/UPC-access fields. `block_statement` contains only its optional
  descriptor FK; null extra_info is retained without a fabricated wrapper. The block's statement chain belongs to the statement union,
  not a_block: existing `block_item` and execution edges stay statement-owned.
- Add `try_descriptor` for actual a_try_supplement: dependent body, function-try
  flag and lifetime. Existing managed-dialect-only finally information remains
  optional/excluded by user scope. `try_statement` contains descriptor FK only.
- Move handler ownership/order into ordered `try_handler(owner_id try_descriptor,
  ordinal, handler_id handler)`; handler keeps its own retained payload. This
  supports wrapper identity independent of statement visitation. Lowered-EH
  try-supplement links target `try_descriptor`, not a guessed statement.
- Add `local_static_initialization` for actual a_local_static_variable_init:
  variable, exact init-kind, active constant or dynamic initialization and its
  retry/rollback lifetime. Ordered `scope_local_static_initialization` records
  storage traversal, explicitly not execution order. Replace the incorrect
  projection of this list as TU static-initialization actions. Variable initializer
  conveniences project this association without duplicating semantic authority.
- Lifetime: `bound_entity_id -> entity` is nullable actual lifetime.entity.
  Preserve actual parent, parent checkpoint, has-implicit-child and
  has-block-after-label-child flags. Drop unwritten synthetic scope/begin/end
  columns; no fabricated lifetime transition data. Any containing-function
  convenience must derive from proven retained binding/context, never the routine
  that happens to visit a descriptor. Plural referenced-by-function navigation may
  be useful but is not semantic ownership.
- Add ordered `lifetime_child(owner_id, ordinal, child_id)` from child_lifetime/
  next; this is reverse creation order. Parent of block_after_label is a
  continuation predecessor, not lexical nesting. Null/unbound lifetime.entity
  after stock lowering is legitimate. Children may be incomplete when implicit
  region-local children exist; preserve the recorded flag.
- Add scope's separate function-static-lifetime FK for the actual
  variant.routine.lifetime_of_local_static_vars root. Keep scope.lifetime separate.
- Normalize `lifetime_cleanup` to owner/ordinal/initialization only. Destructor,
  unordered and conditional convenience properties project Initialization.
  This list includes exceptional-only recipes, not unconditional exit calls.
- Rename jump's target-lifetime field to common-lifetime: stock goto field is
  the source/target common boundary. Capture label statement's separate retained
  lifetime. Do not synthesize source lifetime or transition tables.
- Add `constructor_initializer` for actual a_constructor_init with kind,
  field/base active union, initialization/source-expression, generated/pack/
  braced/use-field-initializer flags, safe orig_type and optional retained range.
  Ordered `scope_constructor_initializer` is actual construction/destruction
  order, not source textual order. Present source initialization execution can
  link to the actual function graph; a null recipe with use-field-initializer
  navigates the field's retained recipe, without a fabricated call or copy.
  Primary lowering's cleared list is documented loss; actual lowered body wins.

Descriptor-first visits through lifetime metadata must not freeze real body
operations as metadata. Later statement execution edges promote evaluated child
subtrees through the existing evaluated-edge mechanism, while cleanup/backing
recipes remain metadata. In source-semantic profiles, actual constructor-init
operations and GNU array-copy `source_expr` are executable semantic operands and
must be reachable through the function execution model exactly once. The latter
is not a source-location field. A referenced field default recipe remains metadata;
`use_field_initializer` is preserved without a fabricated copy or call.

## Execution and family closure

Cleanup storage gains independent `is_indirect` and `uses_this_parameter_offset`
facts so both can coexist. ABI `vtt_location_id` points to cleanup storage; VLA
count location stays separate, and supplement addressing uses its own recorded
flags. Array rows retain the actual handle Constant; decoded location belongs to
each using CleanupAction.Storage with that region's flags. Array.StorageLocations
exposes reverse contexts and optional unique Storage; no global-context guess.
Recorded `IsDelegating` is nullable: the current IA64 lowered destructor kind
provides an exact answer, while cfront wrapper purpose is not inferred. The same
recorded supplemental VttLocation is the delegation argument locator when this
fact is true; the unused synthetic scalar argument column has been removed.
Handler lifetime navigation follows its actual body descriptor or parameter
initialization relationships; a_handler has no independent lifetime field, so
no such synthetic column remains.
Stock currently appends one fresh array row per using region, but the model does
not require undocumented uniqueness.

The next native batch closes no-object/array/VTT cleanup addressing using exact
region/supplement flags and activates the existing real exception-object actions.
It also captures GNU variable cleanup obligations, ordered structured bindings,
raw/effective/extension TLS facts and actual asm-name/register union branches.
Routine defined/discarded-body and recorded eligibility states remain required.
Retained reflection dispatch reuses argument/parameter/token/scoped adapters and
adds data-member specification payload without an invented source correspondence.
The accompanying schema fields must be enumerated from actual retained structures
before freezing; neither schema defaults nor exporter omissions establish facts.

Native owns canonical lifetime/initializer adapters and integration. Managed owns
attribute/reflection extensions in its three headers plus reader/API/tests. The
coordinator allocates schema/catalog IDs, preserves field-sized evidence and
serializes the short contract handoff. Workers do not edit shared schema or commit.


## Frozen variable and storage details

Variable payload now retains the audited common/lowering/GNU/source semantic
flags plus cleanup routine, associated parameter descriptor, actual binding
container/membership, alignment/priority/section/COMDAT/alias/VLA/typeinfo links.
Raw standard TLS, effective standard behavior and DM_THREAD extension remain
separate. Current initialization-routine union branches on raw standard TLS;
public duration can project effective-or-extension behavior. Named-register
number is integer, GNU register is the existing stable named-register kind,
and asm atom is a third active union branch. No independent alias/cleanup calls
are fabricated. Prototype constant_valued is explicitly recorded, not a proof
of a concrete evaluated value.

Composite multi-column primary-key relations use WITHOUT ROWID after verified
native 198 insertion and 232 reader measurements; identity and single-key tables
retain rowid. Page size remains 4KiB until native insertion measurements justify
a change. This is a representation choice with no altered semantic facts.


The 243-table final validation boundary also enables stock C++26 reflection in
profile configuration and source-only RECORD_UNRECOGNIZED_ATTRIBUTES retention.
These activate genuine reflected descriptors and shared attribute owners without
compiler-private state or fabricated metadata. Reflection uses the stock GNU
mode with `--set_flag reflection`; source attributes preserve the actual vendor
marker lists. Neither change expands optional source-coordinate work.


Final genuine throw-aggregate validation activated integer member offsets 0 and
8 with no THIS flag. The pinned lower_throw → partial aggregate cleanup →
make_handle_for_entity producer binds these to the runtime thrown object. New
`thrown_object_relative` preserves that base and exact offset. No-object release
is classified from the actual primary zero handle with no addressing/ARRAY flags
and retained effective void() target prototype without parameters/ellipsis/
implicit object parameter; neither routine names nor compiler-generated status
are required. Object-taking cfront destruction wrappers can have sfk_none and
must not be mistaken for the release action.

CleanupAction.IsUnordered is unavailable/null for decoded region tables: the
format has no such bit, while Initialization.IsUnordered remains retained. Actual
region links and conditional actions encode executable cleanup ordering. Array
count locations come from the using actions' recorded ABI VLA-count locations,
with plural contexts and only an unambiguous optional convenience. Removed the
unwritten singleton array-count and ABI-subobject columns rather than fabricate
associations. All three changes stay within this checkpoint's cleanup semantics.
