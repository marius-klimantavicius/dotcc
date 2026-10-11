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
