-- Run with sqlite3 program.dotobj < browse.sql (or in a GUI query editor).
.headers on
.mode column
SELECT s.id,s.name,s.link_name,s.definition_kind,f.body_node_id
FROM symbols s JOIN functions f ON f.symbol_id=s.id;

SELECT n.id,n.kind,n.operator,n.value_category,t.kind AS type_kind,t.name AS type_name,
       s.name AS selected_symbol,n.line,n.column
FROM nodes n LEFT JOIN types t ON t.id=n.type_id
LEFT JOIN symbols s ON s.id=n.symbol_id ORDER BY n.id;

SELECT parent_id,role,ordinal,child_id FROM edges ORDER BY parent_id,role,ordinal;

-- Bound nodes are source navigation, separate from allocation/count execution.
SELECT t.id,t.name,t.element_count,t.is_incomplete,t.array_bound_kind,
       t.array_bound_node_id,n.domain,n.kind,n.operator,n.constant_text
FROM types t LEFT JOIN nodes n ON n.id=t.array_bound_node_id
WHERE t.kind='array' ORDER BY t.id;

SELECT t.name AS enum,storage.builtin_name AS underlying,e.is_scoped,
       e.has_explicit_base,v.ordinal,v.name,v.value_text
FROM enums e JOIN types t ON t.id=e.type_id
JOIN types storage ON storage.id=t.underlying_type_id
LEFT JOIN enum_values v ON v.enum_type_id=e.type_id ORDER BY e.type_id,v.ordinal;

SELECT n.id,t.name AS enum,v.name AS selected_enumerator
FROM nodes n JOIN enum_values v ON v.id=n.enum_value_id
JOIN types t ON t.id=v.enum_type_id ORDER BY n.id;

SELECT t.name AS record,f.name,f.byte_offset,ft.kind AS field_type
FROM fields f JOIN types t ON t.id=f.owner_type_id JOIN types ft ON ft.id=f.type_id
ORDER BY f.owner_type_id,f.ordinal;

SELECT owner.name AS record,base.name AS base,b.ordinal,b.direct_ordinal,
       b.byte_offset,b.is_direct,b.is_virtual,b.is_optimized_empty,b.access
FROM base_subobjects b JOIN types owner ON owner.id=b.owner_type_id
JOIN types base ON base.id=b.base_type_id ORDER BY b.owner_type_id,b.ordinal;

SELECT c.node_id,c.direction,b.id AS selected_base,c.byte_adjustment,
       c.preserves_null,c.source_representation,c.result_representation
FROM class_conversions c JOIN base_subobjects b ON b.id=c.base_subobject_id
ORDER BY c.node_id;

SELECT t.name AS template,s.name AS instance,a.ordinal,a.kind,ty.kind AS argument_type
FROM template_instances i JOIN templates t ON t.id=i.template_id
LEFT JOIN symbols s ON s.id=i.symbol_id JOIN template_arguments a ON a.instance_id=i.id
LEFT JOIN types ty ON ty.id=a.type_id ORDER BY i.id,a.ordinal;

SELECT t.name AS template,f.ordinal,f.kind,f.name,f.parameter_depth,
       f.parameter_position,f.is_pack,f.default_value_text
FROM template_formals f JOIN templates t ON t.id=f.template_id
ORDER BY f.template_id,f.ordinal;

SELECT instance_id,ordinal,parent_ordinal,kind,is_pack_expansion,
       type_id,template_id,constant_text FROM template_arguments
ORDER BY instance_id,ordinal;

SELECT t.name,t.kind,t.is_dependent,p.kind AS parameter_kind,
       p.parameter_depth,p.parameter_position,p.qualifier_type_id
FROM types t LEFT JOIN template_type_parameters p ON p.type_id=t.id
WHERE t.is_dependent=1 ORDER BY t.id;

SELECT s.name AS function,p.ordinal,p.name AS empty_parameter_pack
FROM empty_parameter_packs p JOIN symbols s ON s.id=p.function_id
ORDER BY p.function_id,p.ordinal;

SELECT t.builtin_name,t.size,t.alignment,f.format,f.precision_bits,
       f.min_exponent,f.max_exponent
FROM floating_types f JOIN types t ON t.id=f.type_id ORDER BY t.id;

SELECT v.id,t.builtin_name,f.format,v.bits_hex
FROM floating_values v JOIN types t ON t.id=v.type_id
JOIN floating_types f ON f.type_id=t.id ORDER BY v.id;

-- Destruction ordinal is the order supplied by EDG, not an inferred reversal.
SELECT l.id,l.kind,l.parent_id,l.owner_node_id,d.ordinal,i.id AS initialization,
       v.name AS variable,dt.name AS destructor,i.conditional,i.partial_aggregate
FROM lifetimes l JOIN lifetime_destructions d ON d.lifetime_id=l.id
JOIN initializations i ON i.id=d.initialization_id
LEFT JOIN symbols v ON v.id=i.variable_symbol_id
LEFT JOIN symbols dt ON dt.id=i.destructor_symbol_id ORDER BY l.id,d.ordinal;

SELECT s.name,c.ordinal,c.kind,f.name AS field,i.id AS initialization,
       ctor.name AS constructor,dtor.name AS destructor
FROM constructor_initializers c JOIN symbols s ON s.id=c.function_id
JOIN initializations i ON i.id=c.initialization_id
LEFT JOIN fields f ON f.id=c.field_id
LEFT JOIN symbols ctor ON ctor.id=i.constructor_symbol_id
LEFT JOIN symbols dtor ON dtor.id=i.destructor_symbol_id
ORDER BY c.function_id,c.ordinal;

SELECT * FROM try_handlers ORDER BY try_node_id,ordinal;
SELECT * FROM throws ORDER BY node_id;

SELECT s.name,t.return_storage,n.id AS return_node,i.kind AS initialization,
       ctor.name AS constructor,f.nrvo_candidate_symbol_id
FROM functions f JOIN symbols s ON s.id=f.symbol_id JOIN types t ON t.id=s.type_id
JOIN nodes n ON n.function_id=f.symbol_id AND n.kind='return'
LEFT JOIN initializations i ON i.id=n.initialization_id
LEFT JOIN symbols ctor ON ctor.id=i.constructor_symbol_id ORDER BY s.id,n.id;

SELECT s.name,c.ordinal,field.name,source.name AS selected_copy_source
FROM constructor_initializers c JOIN symbols s ON s.id=c.function_id
LEFT JOIN fields field ON field.id=c.field_id
JOIN symbols source ON source.id=c.copy_source_symbol_id ORDER BY s.id,c.ordinal;

SELECT n.id,n.kind,n.operator,t.kind AS type,n.value_normalization
FROM nodes n LEFT JOIN types t ON t.id=n.type_id
WHERE n.value_normalization IS NOT NULL ORDER BY n.id;

SELECT n.id,n.kind,n.initialization_id,n.lifetime_id,l.owner_node_id
FROM nodes n LEFT JOIN lifetimes l ON l.id=n.lifetime_id
WHERE n.kind IN ('condition','for') ORDER BY n.id;

SELECT t.id,t.link_name,t.address_point_byte_offset,e.slot_index,
       selected.name AS selected,target.name AS target,e.entry_kind,
       e.target_variant,e.this_delta,e.vcall_index,e.return_delta,e.vbase_index
FROM virtual_tables t JOIN virtual_table_entries e ON e.table_id=t.id
JOIN symbols selected ON selected.id=e.selected_symbol_id
LEFT JOIN symbols target ON target.id=e.target_symbol_id
ORDER BY t.id,e.slot_index;

SELECT s.name,w.ordinal,w.phase,w.base_subobject_id,w.vptr_byte_offset,
       w.table_id,w.source_kind,w.construction_index
FROM vptr_writes w JOIN symbols s ON s.id=w.function_id
ORDER BY w.function_id,w.ordinal;

SELECT n.id,n.domain,n.kind,n.operator,n.type_id,r.name,r.parameter_depth,
       r.parameter_position,r.is_pack,r.qualifier_type_id
FROM nodes n LEFT JOIN template_value_references r ON r.node_id=n.id
WHERE n.domain='template-recipe' ORDER BY n.id;

SELECT n.id,n.operator,n.value_category,n.evaluation_order,
       stored.builtin_name AS stored_type,calculation.builtin_name AS calculation_type
FROM nodes n JOIN types stored ON stored.id=n.type_id
JOIN types calculation ON calculation.id=n.operation_type_id ORDER BY n.id;

SELECT id,name,kind,semantic_role,size,alignment
FROM types WHERE semantic_role IS NOT NULL ORDER BY id;

SELECT n.id,n.operator,n.type_id,e.role,e.ordinal,e.child_id
FROM nodes n JOIN edges e ON e.parent_id=n.id
WHERE n.operator LIKE 'va-%' ORDER BY n.id,e.role,e.ordinal;

SELECT a.node_id,a.zero_initialize,e.ordinal,e.array_index,e.repeat_count,
       e.value_node_id,n.kind,n.constant_text
FROM array_initializers a
LEFT JOIN array_initializer_elements e ON e.initializer_node_id=a.node_id
LEFT JOIN nodes n ON n.id=e.value_node_id ORDER BY a.node_id,e.ordinal;

SELECT r.node_id,r.zero_byte_count,e.ordinal,f.name AS selected_field,
       b.base_type_id AS selected_base,e.value_node_id,n.kind
FROM record_initializers r
LEFT JOIN record_initializer_elements e ON e.initializer_node_id=r.node_id
LEFT JOIN fields f ON f.id=e.field_id
LEFT JOIN base_subobjects b ON b.id=e.base_subobject_id
LEFT JOIN nodes n ON n.id=e.value_node_id ORDER BY r.node_id,e.ordinal;

SELECT l.id,t.kind,t.size,l.byte_count,s.link_name,s.linkage
FROM literal_objects l JOIN types t ON t.id=l.type_id
LEFT JOIN symbols s ON s.id=l.selected_symbol_id ORDER BY l.id;
SELECT literal_id,byte_offset,value FROM literal_bytes ORDER BY literal_id,byte_offset;
SELECT n.id,n.kind,n.literal_object_id,n.literal_byte_offset,
       i.copy_byte_count,i.zero_byte_count
FROM nodes n LEFT JOIN literal_initializers i ON i.node_id=n.id
WHERE n.literal_object_id IS NOT NULL ORDER BY n.id;

SELECT a.node_id,t.kind AS allocated_type,s.name AS allocator,a.count_value_node_id,
       count.name AS selected_count,a.size_node_id,a.length_check_node_id,
       failure.link_name AS selected_failure,a.result_byte_offset
FROM allocations a JOIN types t ON t.id=a.allocated_type_id
JOIN symbols s ON s.id=a.allocator_symbol_id
LEFT JOIN symbols count ON count.id=a.count_symbol_id
LEFT JOIN symbols failure ON failure.id=a.length_failure_symbol_id ORDER BY a.node_id;
