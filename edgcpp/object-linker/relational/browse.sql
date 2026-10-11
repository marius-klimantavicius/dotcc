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
