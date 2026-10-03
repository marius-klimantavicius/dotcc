// Statement-boundary expression plans. Prefixes execute exactly where the
// source expression executes; callers must not hoist them out of a branch/loop.
struct source_expression_plan { string prefix, value; };
struct source_effect_plan { string statements, header; };
source_expression_plan plan_source_value(an_expr_node_ptr);
source_expression_plan plan_source_condition(an_expr_node_ptr);
source_effect_plan plan_source_effect(an_expr_node_ptr);
string source_expression_override(an_expr_node_ptr,bool);
string source_bitfield_members(a_type_ptr);
string address(an_expr_node_ptr);
string source_expression_address_override(an_expr_node_ptr);
