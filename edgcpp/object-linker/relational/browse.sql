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

SELECT t.name AS template,s.name AS instance,a.ordinal,a.kind,ty.kind AS argument_type
FROM template_instances i JOIN templates t ON t.id=i.template_id
LEFT JOIN symbols s ON s.id=i.symbol_id JOIN template_arguments a ON a.instance_id=i.id
LEFT JOIN types ty ON ty.id=a.type_id ORDER BY i.id,a.ordinal;
