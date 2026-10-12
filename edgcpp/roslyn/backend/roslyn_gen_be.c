/* Independent EDG semantic backend. SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception */
#include "basic_hdrs.h"
#include "fe_common.h"
#include "lower_eh.h"
#include <sqlite3.h>
#include <cstdlib>
#include <cstdio>
#include <map>
#include <vector>
#include <string>
#include <initializer_list>
#include <cstring>
#include <utility>
#include <algorithm>
#include <unistd.h>
BEGIN_EDG_NAMESPACE
#include "roslyn_store.h"
#include "roslyn_float.h"
static RoslynStore *roslyn_db;
static sqlite3_int64 roslyn_unit;
typedef sqlite3_int64 Rid;
static std::map<a_variable_ptr,a_constant_ptr> roslyn_local_constants;
static std::map<a_variable_ptr,Rid> roslyn_projected_tables;
static std::map<a_routine_ptr,Rid> roslyn_function_eh;
static std::map<std::pair<Rid,unsigned long>,Rid> roslyn_regions;
static Rid roslyn_current_eh;
static int roslyn_static_ordinal;
static std::map<Rid,int> roslyn_override_ordinals;
static a_routine_ptr roslyn_current_routine;
static std::map<a_try_supplement_ptr,Rid> roslyn_try_blocks;
static std::map<a_macro_invocation_record_index,Rid> roslyn_macro_invocations;
static std::map<std::string,Rid> roslyn_positions;
static std::map<std::string,Rid> roslyn_values;

#define DB (*roslyn_db)
#define K(f,n) roslyn_tag(f,n)
#define R(v) roslyn_ref(v)
#define Q(v) std::to_string(v)
static void roslyn_collect_local_initializers(a_scope_ptr);
static Rid roslyn_type(a_type_ptr);
static Rid roslyn_scope(a_scope_ptr);
static Rid roslyn_routine(a_routine_ptr);
static Rid roslyn_variable(a_variable_ptr);
static Rid roslyn_field(a_field_ptr,a_type_ptr=nullptr);
static Rid roslyn_label(a_label_ptr);
static Rid roslyn_constant(a_constant_ptr);
static Rid roslyn_expr(an_expr_node_ptr,int=7001);
static Rid roslyn_statement(a_statement_ptr,int=7001);
static Rid roslyn_init(a_dynamic_init_ptr,int=7001);
static Rid roslyn_lifetime(an_object_lifetime_ptr);
static Rid roslyn_namespace(a_namespace_ptr);
static Rid roslyn_handler(a_handler_ptr,Rid=0,int=0);
static Rid roslyn_template(a_template_ptr);
static Rid roslyn_base(a_base_class_ptr);
static Rid roslyn_instance(const void *,Rid,a_template_ptr,a_template_arg_ptr,a_template_arg_ptr);
static Rid roslyn_argument(a_template_arg_ptr);
static Rid roslyn_source_file(a_source_file_ptr,Rid=0,int=0);
static void roslyn_macros();
static Rid roslyn_position(const a_source_position &p) {
 if(!p.seq&&p.column==0)return 0;
 std::string key=Q(p.seq)+":"+Q(p.column)+":"+Q(p.orig_seq)+":"+Q(p.orig_column)+":"+Q(p.macro_context);auto found=roslyn_positions.find(key);if(found!=roslyn_positions.end())return found->second;
 Rid id=DB.fresh("source_position");roslyn_positions[key]=id;
 Rid macro=0;auto mf=roslyn_macro_invocations.find(p.macro_context);if(mf!=roslyn_macro_invocations.end())macro=mf->second;
 a_line_number line=0;a_boolean end=false;a_source_file_ptr file=p.seq?source_file_for_seq(p.seq,&line,&end,FALSE):nullptr;
 DB.put("source_position",{{"id",id},{"sequence",Q(p.seq)},{"column",static_cast<int>(p.column)},{"original_sequence",Q(p.orig_seq)},{"original_column",static_cast<int>(p.orig_column)},{"macro_invocation_id",R(macro)},{"file_id",R(roslyn_source_file(file))},{"logical_line",file?RoslynSqlValue(Q(line)):RoslynSqlValue()},{"origin_kind_id",K("source_origin_kind",macro?"macro":p.seq?"ordinary":"unknown")}});return id;
}
static Rid roslyn_origin(const a_source_position &p,const a_source_position *end=nullptr,const a_source_position *start=nullptr) {
 Rid pos=roslyn_position(p);if(!pos&&(!start||!start->seq))return 0;Rid range=0;
 if(end&&end->seq){range=DB.fresh("source_range");DB.put("source_range",{{"id",range},{"start_id",start?roslyn_position(*start):pos},{"end_id",roslyn_position(*end)},{"endpoint_kind_id",K("range_endpoint","inclusive")}});}
 Rid n=DB.fresh("source_origin");DB.put("source_origin",{{"id",n},{"position_id",R(pos)},{"range_id",R(range)},{"kind_id",K("source_origin_kind","ordinary")}});return n;
}
static Rid roslyn_name(a_source_correspondence &c) {
 Rid id=DB.fresh("name");const char *name=c.name;
#if NEED_NAME_MANGLING
 if(c.name_has_been_mangled)name=c.unmangled_name_or_mangled_encoding;
#endif
 DB.put("name",{{"id",id},{"spelling_id",R(DB.atom(name))},{"category_id",K("name_category",name?"named":"unnamed")}});return id;
}
static Rid roslyn_owner(a_source_correspondence &c) {
 if(c.enclosing_routine)return roslyn_routine(c.enclosing_routine);
 if(c.parent_scope){a_scope_ptr p=c.parent_scope;if(p->kind==sck_class_struct_union)return roslyn_type(p->variant.assoc_type);if(p->kind==sck_namespace)return roslyn_namespace(p->variant.assoc_namespace);return roslyn_scope(p);}
 return 0;
}
static void roslyn_symbol(Rid id,const char *kind,a_source_correspondence &c,Rid type,int generated=0) {
 DB.put("symbol",{{"id",id},{"kind_id",K("entity",kind)},{"name_id",roslyn_name(c)},{"containing_entity_id",R(roslyn_owner(c))},{"type_id",R(type)},{"source_origin_id",R(roslyn_origin(c.decl_position))},{"compiler_generated",generated}});
 Rid declaration=DB.fresh("declaration");DB.put("declaration",{{"id",declaration},{"kind_id",K("entity",kind)},{"symbol_id",id},{"lexical_scope_id",R(roslyn_scope(c.parent_scope))},{"source_origin_id",R(roslyn_origin(c.decl_position))},{"compiler_generated",generated}});
}
static void roslyn_child(Rid owner,const char *role,int ordinal,Rid child) {
 if(child)DB.put("structural_child",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"child_id",child}});
}
static void roslyn_reference(Rid owner,const char *role,int ordinal,Rid target) {
 if(target)DB.put("semantic_reference",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"target_id",target}});
}
static Rid roslyn_source_file(a_source_file_ptr p,Rid parent,int ordinal) {
 bool fresh;Rid id=DB.claim(p,"source_file",fresh);if(!fresh)return id;
 DB.put("source_file",{{"id",id},{"path_id",R(DB.atom(p->full_name))},{"name_id",R(DB.atom(p->file_name))},{"origin_kind_id",K("source_origin_kind","ordinary")}});
 Rid inc=DB.fresh("include_occurrence");DB.put("include_occurrence",{{"id",inc},{"file_id",id},{"parent_id",R(parent)},{"ordinal",ordinal},{"first_sequence",Q(p->first_seq_number)},{"last_sequence",Q(p->last_seq_number)},{"is_system",static_cast<int>(p->included_by_system_include)}});
 Rid seg=DB.fresh("source_mapping");DB.put("source_mapping_segment",{{"id",seg},{"first_sequence",Q(p->first_seq_number)},{"last_sequence",Q(p->last_seq_number)},{"file_id",id},{"include_id",inc},{"logical_line",Q(p->first_line_number)},{"presumed_name_id",R(DB.atom(p->file_name))}});
 int n=0;for(a_source_file_ptr c=p->first_child_file;c;c=c->next)roslyn_source_file(c,inc,n++);return id;
}
static Rid roslyn_scope(a_scope_ptr p) {
 bool fresh;Rid id=DB.claim(p,"scope",fresh);if(!fresh)return id;
 const char *kind="block";switch(p->kind){case sck_file:kind="file";break;case sck_namespace:kind="namespace";break;case sck_function:kind="function";break;case sck_class_struct_union:kind="class";break;case sck_func_prototype:kind="prototype";break;default:break;}
 Rid assoc=0;if(p->kind==sck_function)assoc=roslyn_routine(p->variant.routine.ptr);else if(p->kind==sck_namespace)assoc=roslyn_namespace(p->variant.assoc_namespace);else if(p->kind==sck_class_struct_union)assoc=roslyn_type(p->variant.assoc_type);
 DB.put("scope",{{"id",id},{"kind_id",K("scope_kind",kind)},{"parent_id",R(roslyn_scope(p->parent))},{"associated_entity_id",R(assoc)},{"lifetime_id",R(roslyn_lifetime(p->lifetime))}});
 int n=0;
#define MEMBER(target) DB.put("scope_member",{{"owner_id",id},{"ordinal",n++},{"member_id",target}})
 for(a_type_ptr t=p->types;t;t=t->next){MEMBER(roslyn_type(t));}
 for(a_variable_ptr v=p->variables;v;v=v->next){MEMBER(roslyn_variable(v));}
 for(a_variable_ptr v=p->nonstatic_variables;v;v=v->next){MEMBER(roslyn_variable(v));}
 for(a_routine_ptr r=p->routines;r;r=r->next){MEMBER(roslyn_routine(r));}
 for(a_constant_ptr c=p->constants;c;c=c->next){MEMBER(roslyn_constant(c));}
 for(a_namespace_ptr ns=p->namespaces;ns;ns=ns->next){MEMBER(roslyn_namespace(ns));}
 for(a_label_ptr l=p->labels;l;l=l->next){MEMBER(roslyn_label(l));}
 for(a_template_ptr t=p->templates;t;t=t->next){MEMBER(roslyn_template(t));}
#undef MEMBER
 for(a_scope_ptr s=p->scopes;s;s=s->next)roslyn_scope(s);
 for(a_local_static_variable_init_ptr i=p->local_static_variable_inits;i;i=i->next){
 if(roslyn_projected_tables.count(i->variable))continue;
 Rid v=roslyn_variable(i->variable);Rid c=i->init_kind==initk_static?roslyn_constant(i->initializer.constant):0;Rid d=i->init_kind==initk_dynamic?roslyn_init(i->initializer.dynamic):0;
 DB.put("static_initialization",{{"owner_id",roslyn_unit},{"ordinal",roslyn_static_ordinal++},{"variable_id",v},{"initialization_id",R(d)},{"constant_id",R(c)}});
 }
 for(a_dynamic_init_ptr i=p->dynamic_inits;i;i=i->next)roslyn_init(i);
 if(p->assoc_block)roslyn_statement(p->assoc_block);
 return id;
}
static Rid roslyn_namespace(a_namespace_ptr p) {
 bool fresh;Rid id=DB.claim(p,"namespace",fresh);if(!fresh)return id;roslyn_symbol(id,"namespace",p->source_corresp,0);
 DB.put("namespace_symbol",{{"id",id},{"scope_id",R(p->is_namespace_alias?0:roslyn_scope(p->variant.assoc_scope))},{"alias_target_id",R(p->is_namespace_alias?roslyn_namespace(p->variant.assoc_namespace):0)},{"is_inline",static_cast<int>(p->is_inline)},{"is_anonymous",static_cast<int>(p->source_corresp.name==nullptr)}});return id;
}
static Rid roslyn_routine(a_routine_ptr p) {
 bool fresh;Rid id=DB.claim(p,"routine",fresh);if(!fresh)return id;
 Rid type=roslyn_type(p->type);roslyn_symbol(id,"routine",p->source_corresp,type,p->compiler_generated);
 a_routine_ptr saved_routine=roslyn_current_routine;roslyn_current_routine=p;
 a_scope_ptr scope=p->function_def_number!=NULL_function_def_number?scope_for_routine_or_null(p):nullptr;roslyn_collect_local_initializers(scope);Rid saved_eh=roslyn_current_eh;roslyn_current_eh=0;Rid root=scope?roslyn_statement(scope->assoc_block,ignore_routine_in_back_end(p)?7002:7001):0;
 Rid containing=0;if(p->source_corresp.is_class_member&&p->source_corresp.parent_scope)containing=roslyn_type(p->source_corresp.parent_scope->variant.assoc_type);
 DB.put("routine_symbol",{{"id",id},{"routine_type_id",type},{"containing_type_id",R(containing)},{"body_root_id",R(root)},{"body_scope_id",R(roslyn_scope(scope))},{"body_state_id",K("body_state",ignore_routine_in_back_end(p)?"dependent":scope?"definition":"declaration_only")},{"is_virtual",static_cast<int>(p->is_virtual)},{"is_pure",static_cast<int>(p->pure_virtual)},{"is_inline",static_cast<int>(p->is_inline)},{"is_constexpr",static_cast<int>(p->is_constexpr)},{"is_deleted",static_cast<int>(p->is_deleted)},{"primary_entry_id",R(roslyn_routine(p->primary_ctor_or_dtor))},{"template_id",R(roslyn_template(p->assoc_template))},{"instance_id",R(p->assoc_template?roslyn_instance(p,id,p->assoc_template,p->template_arg_list,nullptr):0)}});
 if(scope){int ordinal=0;for(a_variable_ptr v=scope->variant.routine.parameters;v;v=v->next)DB.put("routine_parameter",{{"owner_id",id},{"ordinal",ordinal++},{"parameter_id",roslyn_variable(v)},{"role_id",K("role",v->is_this_parameter?"this_parameter":v->variant.assoc_param_type&&v->variant.assoc_param_type->param_num?"ordinary_parameter":"abi_parameter")},{"is_source_parameter",static_cast<int>(!v->is_this_parameter&&v->variant.assoc_param_type&&v->variant.assoc_param_type->param_num!=0)}});}
 roslyn_current_eh=saved_eh;roslyn_current_routine=saved_routine;return id;
}
static Rid roslyn_variable(a_variable_ptr p) {
 bool fresh;Rid id=DB.claim(p,"variable",fresh);if(!fresh)return id;Rid type=roslyn_type(p->type);roslyn_symbol(id,"variable",p->source_corresp,type,p->compiler_generated);
 DB.put("variable_symbol",{{"id",id},{"type_id",type},{"initializer_id",R(p->init_kind==initk_dynamic?roslyn_init(p->initializer.dynamic):0)},{"constant_initializer_id",R(p->init_kind==initk_static&&!roslyn_projected_tables.count(p)?roslyn_constant(p->initializer.constant):0)},{"is_thread_local",static_cast<int>(p->is_thread_local)},{"is_parameter",static_cast<int>(p->is_parameter)},{"storage_duration_id",K("storage_duration",p->is_parameter?"parameter":p->is_thread_local?"thread":p->storage_class==sc_auto||p->storage_class==sc_register?"automatic":"static")}});return id;
}
static Rid roslyn_field(a_field_ptr p,a_type_ptr record) {
 bool fresh;Rid id=DB.claim(p,"field",fresh);if(!fresh)return id;Rid type=roslyn_type(p->type);roslyn_symbol(id,"field",p->source_corresp,type,p->compiler_generated);
 if(!record&&p->source_corresp.parent_scope)record=p->source_corresp.parent_scope->variant.assoc_type;
 DB.put("field_symbol",{{"id",id},{"record_type_id",roslyn_type(record)},{"type_id",type},{"is_bit_field",static_cast<int>(p->is_bit_field)},{"is_mutable",static_cast<int>(p->is_mutable)},{"default_initialization_id",R(roslyn_init(p->initializer,7002))}});return id;
}
static Rid roslyn_label(a_label_ptr p) {
 bool fresh;Rid id=DB.claim(p,"label",fresh);if(!fresh)return id;roslyn_symbol(id,"label",p->source_corresp,0);
 DB.put("label_symbol",{{"id",id},{"statement_id",R(roslyn_statement(p->exec_stmt))},{"is_break",static_cast<int>(p->break_label)},{"is_continue",static_cast<int>(p->continue_label)}});return id;
}
static Rid roslyn_type(a_type_ptr p) {
 bool fresh;Rid id=DB.claim(p,"type",fresh);if(!fresh)return id;
 int kind=roslyn_kind_type(p->kind);if(!kind)DB.fail("Unmapped EDG type kind");
 DB.put("type",{{"id",id},{"kind_id",kind},{"name_id",roslyn_name(p->source_corresp)},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position))},{"size",Q(p->size)},{"alignment",Q(p->alignment)},{"incomplete",static_cast<int>(p->incomplete)},{"is_dependent",static_cast<int>(p->is_instantiation_dependent)}});
 switch(p->kind){
 case tk_void:case tk_nullptr:case tk_reflection:break;
 case tk_integer: DB.put("arithmetic_type",{{"id",id},{"arithmetic_kind_id",K("arithmetic_kind",p->variant.integer.bool_type?"boolean":is_signed_integral_type(p)?"signed_integer":"unsigned_integer")},{"bits",static_cast<int>(p->size*targ_char_bit)},{"signed",static_cast<int>(is_signed_integral_type(p))},{"edg_integer_kind_id",roslyn_kind_edg_integer_kind(p->variant.integer.int_kind)}});break;
 case tk_float:case tk_imaginary:case tk_complex:DB.put("arithmetic_type",{{"id",id},{"arithmetic_kind_id",K("arithmetic_kind",p->kind==tk_float?"floating":p->kind==tk_complex?"complex":"imaginary")},{"edg_float_kind_id",roslyn_kind_edg_float_kind(p->variant.float_kind)},{"bits",static_cast<int>(p->size*targ_char_bit)}});break;
 case tk_pointer:DB.put("pointer_type",{{"id",id},{"pointee_type_id",roslyn_type(p->variant.pointer.type)},{"is_reference",static_cast<int>(p->variant.pointer.is_reference)},{"is_rvalue_reference",static_cast<int>(p->variant.pointer.is_rvalue_reference)}});break;
 case tk_array:DB.put("array_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.array.element_type)},{"bound_kind_id",K("bound_kind",p->variant.array.is_variable_size_array?"variable":p->variant.array.bound_is_zero?"zero":p->incomplete?"incomplete":"constant")},{"bound",p->variant.array.is_variable_size_array||p->variant.array.is_template_dependent_size_array?RoslynSqlValue():RoslynSqlValue(Q(p->variant.array.variant.number_of_elements))},{"bound_is_zero",static_cast<int>(p->variant.array.bound_is_zero)}});break;
 case tk_routine:{auto x=p->variant.routine.extra_info;DB.put("routine_type",{{"id",id},{"result_type_id",roslyn_type(p->variant.routine.return_type)},{"this_type_id",R(roslyn_type(x->this_class))},{"is_variadic",static_cast<int>(x->has_ellipsis)},{"has_prototype",static_cast<int>(x->prototyped)}});int n=0;for(a_param_type_ptr a=x->param_type_list;a;a=a->next)DB.put("routine_type_parameter",{{"owner_id",id},{"ordinal",n++},{"type_id",roslyn_type(a->type)},{"role_id",K("role","ordinary_parameter")},{"default_expression_id",R(roslyn_expr(a->default_arg_expr,7002))}});break;}
 case tk_class:case tk_struct:case tk_union:{auto &r=p->variant.class_struct_union;DB.put("record_type",{{"id",id},{"scope_id",R(roslyn_scope(r.extra_info->assoc_scope))},{"definition_state_id",K("definition_state",p->incomplete?"incomplete":"complete")},{"is_abstract",static_cast<int>(r.abstract)},{"is_empty",static_cast<int>(r.is_empty_class)},{"subobject_partner_id",R(roslyn_type(r.extra_info->subobject_partner))},{"instance_id",R(r.extra_info->assoc_template?roslyn_instance(p,id,r.extra_info->assoc_template,r.extra_info->template_arg_list,r.extra_info->partial_spec_template_arg_list):0)}});Rid layout=DB.fresh("record_layout");DB.put("record_layout",{{"id",layout},{"type_id",id},{"is_complete",static_cast<int>(!p->incomplete)},{"nonvirtual_size",Q(r.extra_info->size_without_virtual_base_classes)},{"nonvirtual_alignment",Q(r.extra_info->alignment_without_virtual_base_classes)}});int n=0;for(a_field_ptr f=r.field_list;f;f=f->next)DB.put("field_layout",{{"owner_id",layout},{"ordinal",n++},{"field_id",roslyn_field(f,p)},{"offset",Q(f->offset)},{"bit_offset",Q(f->offset_bit_remainder)},{"bit_width",Q(f->bit_size)}});for(a_base_class_ptr b=r.extra_info->base_classes;b;b=b->next)roslyn_base(b);n=0;for(a_base_class_ptr b=r.extra_info->direct_base_classes;b;b=b->next_direct)DB.put("direct_base",{{"owner_id",id},{"ordinal",n++},{"base_type_id",roslyn_type(b->type)},{"is_virtual",static_cast<int>(b->is_virtual)},{"subobject_id",roslyn_base(b)}});break;}
 case tk_typeref:DB.put("type_reference",{{"id",id},{"underlying_type_id",roslyn_type(p->variant.typeref.type)},{"reference_kind_id",K("type_reference_kind","typedef")}});DB.put("qualified_type",{{"id",id},{"underlying_type_id",roslyn_type(p->variant.typeref.type)},{"is_const",static_cast<int>((p->variant.typeref.qualifiers&TQ_CONST)!=0)},{"is_volatile",static_cast<int>((p->variant.typeref.qualifiers&TQ_VOLATILE)!=0)}});break;
 case tk_ptr_to_member:DB.put("member_pointer_type",{{"id",id},{"declaring_type_id",roslyn_type(p->variant.ptr_to_member.class_of_which_a_member)},{"member_type_id",roslyn_type(p->variant.ptr_to_member.type)}});break;
 case tk_template_param:{auto &d=p->variant.template_param;auto x=d.extra_info;DB.put("dependent_type",{{"id",id},{"parameter_kind_id",roslyn_kind_template_type(d.kind)},{"parameter_number",x?static_cast<int>(x->coordinates.position):0},{"parameter_depth",x?static_cast<int>(x->coordinates.depth):0},{"parent_type_id",R(x?roslyn_type(x->orig_nested_type):0)},{"constraint_id",R(x&&x->coordinates.depth!=CLASS_TEMPLATE_PLACEHOLDER_NESTING_DEPTH&&x->coordinates.depth!=BIT_PRECISE_INT_NESTING_DEPTH?roslyn_expr(x->constraint.type_constraint,7002):0)},{"bit_width_constant_id",R(x&&x->coordinates.depth==BIT_PRECISE_INT_NESTING_DEPTH?roslyn_constant(x->constraint.bit_width_constant):0)}});break;}
 case tk_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.vector.element_type)},{"vector_kind_id",K("vector_kind","fixed")},{"size_constant_id",R(roslyn_constant(p->variant.vector.size_constant))},{"is_boolean",static_cast<int>(p->variant.vector.is_boolean_vector)}});break;
 case tk_scalable_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.scalable_vector.element_type)},{"vector_kind_id",K("vector_kind","scalable")},{"tuple_elements",static_cast<int>(p->variant.scalable_vector.tuple_elements)}});break;
 case tk_riscv_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.riscv_vector.element_type)},{"vector_kind_id",K("vector_kind","riscv")},{"tuple_elements",static_cast<int>(p->variant.riscv_vector.tuple_elements)},{"length_multiplier",static_cast<int>(p->variant.riscv_vector.length_multiplier)}});break;
 case tk_scalable_vector_count:case tk_mfp8:case tk_float8e4m3:case tk_float8e5m2:break;
 default:DB.fail("Type payload adapter missing: "+Q(p->kind));break;
 }
 return id;
}
static Rid roslyn_constant(a_constant_ptr p) {
 bool fresh;Rid id=DB.claim(p,"constant",fresh);if(!fresh)return id;
 std::string value_key;bool intern=false;
 if(p->kind==ck_integer){value_key="integer:"+std::string(decimal_str_for_integer_constant(p).as_temp_characters());intern=true;}
 else if(p->kind==ck_string){value_key="string:"+Q(p->variant.string.length)+":"+std::string(p->variant.string.value,static_cast<size_t>(p->variant.string.length));intern=true;}
 else if(p->kind==ck_float||p->kind==ck_imaginary){auto f=roslyn_float_parts(skip_typerefs(p->type)->variant.float_kind,p->variant.float_value);value_key="float:"+f.format+":"+f.classification+":"+Q(f.negative)+":"+f.significand+":"+Q(f.exponent)+":"+Q(f.quiet)+":"+f.payload;intern=true;}
 Rid val=0;bool value_fresh=true;if(intern){auto found=roslyn_values.find(value_key);if(found!=roslyn_values.end()){val=found->second;value_fresh=false;}}
 if(value_fresh){val=DB.fresh("constant_value");if(intern)roslyn_values[value_key]=val;DB.put("constant_value",{{"id",val},{"kind_id",roslyn_kind_constant(p->kind)}});}
 DB.put("constant",{{"id",id},{"type_id",R(roslyn_type(p->type))},{"value_id",val},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position,&p->end_position))},{"backing_expression_id",R(roslyn_expr(p->expr,7002))},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)}});
 if(!value_fresh)return id;
 switch(p->kind){
 case ck_void:break;
 case ck_integer:{std::string value=decimal_str_for_integer_constant(p).as_temp_characters();DB.put("integer_value",{{"id",val},{"value_id",DB.atom(value)}});break;}
 case ck_float:case ck_imaginary:{auto f=roslyn_float_parts(skip_typerefs(p->type)->variant.float_kind,p->variant.float_value);DB.put("floating_value",{{"id",val},{"class_id",K("float_class",f.classification.c_str())},{"negative",static_cast<int>(f.negative)},{"radix",2},{"significand_id",DB.atom(f.significand)},{"exponent",static_cast<sqlite3_int64>(f.exponent)},{"nan_payload_id",R(f.payload.empty()?0:DB.atom(f.payload))},{"nan_quiet",static_cast<int>(f.quiet)},{"representation_kind_id",K("float_representation",f.format.c_str())}});break;}
 case ck_string:{const auto &s=p->variant.string;DB.put("string_value",{{"id",val},{"element_bits",8},{"unit_kind_id",K("string_unit_kind","target_bytes")},{"length",Q(s.length)},{"encoding_id",K("string_encoding","target_code_units")}});for(a_targ_size_t i=0;i<s.length;++i)DB.put("string_unit",{{"owner_id",val},{"ordinal",static_cast<sqlite3_int64>(i)},{"unit",static_cast<int>(static_cast<unsigned char>(s.value[i]))}});break;}
 case ck_address:{auto &a=p->variant.address;Rid base=0;switch(a.kind){case abk_routine:base=roslyn_routine(a.variant.routine);break;case abk_variable:base=roslyn_variable(a.variant.variable);break;case abk_constant:case abk_temporary:base=roslyn_constant(a.variant.constant);break;case abk_typeid:case abk_uuidof:case abk_cli_typeid:base=roslyn_type(a.variant.type);break;case abk_label:base=roslyn_label(a.variant.label);break;default:DB.fail("Address base payload adapter missing");break;}DB.put("address_value",{{"id",val},{"base_kind_id",roslyn_kind_address_base(a.kind)},{"base_entity_id",R(base)},{"offset",Q(a.offset)},{"one_past_end",static_cast<int>(a.one_past_the_end)}});break;}
 case ck_stack_offset:DB.put("stack_offset_value",{{"id",val},{"variable_id",roslyn_variable(p->variant.stack_offset.variable)},{"offset",Q(p->variant.stack_offset.offset)}});break;
 case ck_aggregate:{DB.put("aggregate_value",{{"id",val},{"type_id",R(roslyn_type(p->type))}});int n=0;for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next)DB.put("aggregate_element",{{"owner_id",val},{"ordinal",n++},{"constant_id",roslyn_constant(c)}});break;}
 case ck_init_repeat:DB.put("repeat_value",{{"id",val},{"constant_id",roslyn_constant(p->variant.init_repeat.constant)},{"count",Q(p->variant.init_repeat.count)}});break;
 case ck_dynamic_init:DB.put("dynamic_initialization_value",{{"id",val},{"initialization_id",roslyn_init(p->variant.dynamic_init.ptr)}});break;
 default:DB.fail("Constant payload adapter missing");break;
 }
 return id;
}
static Rid roslyn_init(a_dynamic_init_ptr p,int role) {
 bool fresh;Rid id=DB.claim(p,"initialization",fresh);if(!fresh)return id;
 DB.put("semantic_node",{{"id",id},{"kind_id",roslyn_kind_dynamic_init(p->kind)},{"compiler_generated",0},{"representation_role_id",role}});
 Rid expr=0,constant=0,ctor=0,source=0;
 switch(p->kind){case dik_none:case dik_zero:break;case dik_constant:case dik_nonconstant_aggregate:case dik_lambda:constant=roslyn_constant(p->variant.constant.ptr);break;case dik_expression:case dik_class_result_via_ctor:expr=roslyn_expr(p->variant.expression,role);break;case dik_constructor:ctor=roslyn_routine(p->variant.constructor.ptr);break;case dik_bitwise_copy:source=roslyn_expr(p->variant.bitwise_copy.source,role);break;default:DB.fail("Initialization payload adapter missing");break;}
 DB.put("initialization",{{"id",id},{"kind_id",roslyn_kind_dynamic_init(p->kind)},{"target_id",R(roslyn_variable(p->variable))},{"type_id",R(p->variable?roslyn_type(p->variable->type):0)},{"expression_id",R(expr)},{"constant_id",R(constant)},{"constructor_id",R(ctor)},{"destructor_id",R(roslyn_routine(p->destructor))},{"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"init_expression_lifetime_id",R(roslyn_lifetime(p->init_expr_lifetime))},{"master_id",R(roslyn_init(p->master_entry))},{"next_destruction_id",R(roslyn_init(p->next_in_destruction_list))},{"overlapping_lifetime_id",R(roslyn_lifetime(p->lifetime_of_overlapping_temps))},{"source_id",R(source)},{"is_unordered",static_cast<int>(p->unordered)},{"is_conditional",static_cast<int>(p->inside_conditional_expression)},{"is_overlapping",static_cast<int>(p->overlaps_temps_in_inner_lifetime)},{"is_static_temporary",static_cast<int>(p->static_temp)},{"is_partial_aggregate",static_cast<int>(p->destruction_is_for_partially_constructed_aggregate)},{"is_partial_initialization",static_cast<int>(p->is_partially_initialized)},{"frees_storage",static_cast<int>(p->is_freeing_of_storage_on_exception)},{"frees_array",static_cast<int>(p->is_array_freeing)},{"is_explicit_cast",static_cast<int>(p->is_explicit_cast)},{"is_braced",static_cast<int>(p->is_braced_initializer)},{"is_compound_literal",static_cast<int>(p->is_compound_literal)},{"follows_executable_statement",static_cast<int>(p->follows_an_exec_statement)},{"has_temporary_lifetime",static_cast<int>(p->has_temporary_lifetime)},{"is_constructor_initialization",static_cast<int>(p->is_constructor_init)},{"is_conditional_guard",static_cast<int>(p->is_guard_var_for_local_static_var_init)},{"is_vla_deallocation",static_cast<int>(p->is_vla_deallocation)},{"creates_initializer_list",static_cast<int>(p->is_creation_of_initializer_list_object)},{"is_initializer_list_array",static_cast<int>(p->is_array_for_initializer_list_object)},{"is_reused_value",static_cast<int>(p->is_reused_value)},{"is_question_result",static_cast<int>(p->is_result_for_class_rvalue_question_mark)},{"is_comma_result",static_cast<int>(p->is_result_for_comma_operator)},{"is_class_result_master",static_cast<int>(p->class_rvalue_initialized_through_master_entry)},{"constructor_copy_implied_source",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_copy_constructor_with_implied_source)},{"constructor_implicit_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_implicit_copy_for_copy_initialization)},{"constructor_value_initialization",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.value_initialization)},{"constructor_sequenced_arguments",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.has_sequenced_arguments)},{"constructor_array_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_array_copy)},{"constant_nonconstant",static_cast<int>((p->kind==dik_constant||p->kind==dik_nonconstant_aggregate||p->kind==dik_lambda)&&p->variant.constant.non_constant)}});
 roslyn_child(id,"operand",0,expr);
 if(p->kind==dik_constructor){int n=0;for(an_expr_node_ptr a=p->variant.constructor.args;a;a=a->next){Rid arg=roslyn_expr(a,role);DB.put("initialization_operand",{{"owner_id",id},{"role_id",K("role","argument")},{"ordinal",n},{"expression_id",arg}});roslyn_child(id,"argument",n++,arg);}}
 return id;
}
static Rid roslyn_lifetime(an_object_lifetime_ptr p) {
 bool fresh;Rid id=DB.claim(p,"lifetime",fresh);if(!fresh)return id;
 DB.put("lifetime",{{"id",id},{"kind_id",roslyn_kind_lifetime(p->kind)},{"parent_id",R(roslyn_lifetime(p->parent_lifetime))},{"parent_checkpoint_id",R(roslyn_init(p->parent_destruction_sublist))},{"owner_id",R(roslyn_routine(roslyn_current_routine))}});
 int n=0;for(a_dynamic_init_ptr i=p->destructions;i;i=i->next_in_destruction_list)DB.put("lifetime_cleanup",{{"owner_id",id},{"ordinal",n++},{"initialization_id",roslyn_init(i,7002)},{"destructor_id",R(roslyn_routine(i->destructor))},{"is_unordered",static_cast<int>(i->unordered)},{"is_conditional",static_cast<int>(i->inside_conditional_expression)}});
 for(an_object_lifetime_ptr l=p->child_lifetime;l;l=l->next)roslyn_lifetime(l);return id;
}
static void roslyn_collect_local_initializers(a_scope_ptr s) {if(!s)return;for(a_local_static_variable_init_ptr i=s->local_static_variable_inits;i;i=i->next)if(i->init_kind==initk_static)roslyn_local_constants[i->variable]=i->initializer.constant;for(a_scope_ptr c=s->scopes;c;c=c->next)roslyn_collect_local_initializers(c);}
static std::vector<a_constant_ptr> roslyn_elements(a_constant_ptr p) {std::vector<a_constant_ptr> r;if(p&&p->kind==ck_aggregate)for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next)r.push_back(c);return r;}
static std::string roslyn_integer(a_constant_ptr c) {return c?std::string(decimal_str_for_integer_constant(c).as_temp_characters()):"0";}
static long long roslyn_small_integer(a_constant_ptr c) {return std::strtoll(roslyn_integer(c).c_str(),nullptr,10);}
static Rid roslyn_cleanup_location(a_constant_ptr handle,bool indirect=false,bool receiver=false) {
 Rid id=DB.fresh("cleanup_storage");Rid variable=0,base=0;std::string offset="0";const char *kind=indirect?"indirect_variable":"direct_variable";
 if(receiver){kind="receiver_relative";offset=roslyn_integer(handle);}
 else if(handle&&handle->kind==ck_stack_offset){variable=roslyn_variable(handle->variant.stack_offset.variable);offset=Q(handle->variant.stack_offset.offset);}
 else if(handle&&handle->kind==ck_address){kind="static_address";auto &a=handle->variant.address;if(a.kind==abk_variable)variable=roslyn_variable(a.variant.variable);else if(a.kind==abk_constant||a.kind==abk_temporary)base=roslyn_constant(a.variant.constant);else DB.fail("EH static locator unsupported base");offset=Q(a.offset);}
 else if(handle&&handle->kind==ck_integer){kind="receiver_relative";offset=roslyn_integer(handle);}
 else DB.fail("EH symbolic locator unsupported");
 DB.put("cleanup_storage_location",{{"id",id},{"kind_id",K("storage_location_kind",kind)},{"variable_id",R(variable)},{"base_entity_id",R(base)},{"offset",offset}});return id;
}
static Rid roslyn_eh(an_eh_prologue_supplement_ptr p,Rid prologue) {
 auto old=roslyn_function_eh.find(p->routine);if(old!=roslyn_function_eh.end())return old->second;
 Rid eh=DB.fresh("function_eh");roslyn_function_eh[p->routine]=eh;roslyn_current_eh=eh;
 if(p->region_table)roslyn_projected_tables[p->region_table]=eh;if(p->array_table)roslyn_projected_tables[p->array_table]=eh;
 auto initializer=[](a_variable_ptr v)->a_constant_ptr {if(!v)return nullptr;if(v->init_kind==initk_static)return v->initializer.constant;auto f=roslyn_local_constants.find(v);return f==roslyn_local_constants.end()?nullptr:f->second;};
 std::vector<a_constant_ptr> rows=roslyn_elements(initializer(p->region_table));std::vector<a_constant_ptr> arrays=roslyn_elements(initializer(p->array_table));std::vector<Rid> array_ids;
 for(size_t i=0;i<arrays.size();++i){auto a=roslyn_elements(arrays[i]);if(a.size()!=3){DB.fail("EDG EH array row shape");break;}Rid aid=DB.fresh("cleanup_array");array_ids.push_back(aid);DB.put("cleanup_array",{{"id",aid},{"function_eh_id",eh},{"ordinal",static_cast<sqlite3_int64>(i)},{"storage_id",roslyn_cleanup_location(a[0])},{"element_size",roslyn_integer(a[1])},{"element_count",roslyn_integer(a[2])},{"is_nonarray_size",static_cast<int>(roslyn_small_integer(a[2])==0)}});}
 for(size_t i=0;i<rows.size();++i)roslyn_regions[std::make_pair(eh,static_cast<unsigned long>(i))]=DB.fresh("cleanup_region");
 std::map<size_t,Rid> supplement_owner;
 for(size_t i=0;i<rows.size();++i){auto row=roslyn_elements(rows[i]);if(row.size()!=4){DB.fail("EDG EH region row shape");break;}Rid region=roslyn_regions[{eh,static_cast<unsigned long>(i)}];std::string next_value=roslyn_integer(row[2]);unsigned long next=next_value==Q(null_eh_region_number)?null_eh_region_number:std::strtoul(next_value.c_str(),nullptr,10);unsigned flags=static_cast<unsigned>(roslyn_small_integer(row[3]));auto next_it=roslyn_regions.find({eh,next});bool supplement=supplement_owner.count(i)!=0;
 Rid cond=0,abi=0;size_t extra=i+1;
 if(!supplement&&(flags&2)){auto s=extra<rows.size()?roslyn_elements(rows[extra]):std::vector<a_constant_ptr>();if(s.size()!=4){DB.fail("EDG EH condition supplement shape");break;}supplement_owner[extra++]=region;cond=DB.fresh("cleanup_condition");DB.put("cleanup_condition",{{"id",cond},{"storage_id",roslyn_cleanup_location(s[1])},{"kind_id",K("condition_kind","constructed")},{"comparison_value","0"}});}
 if(!supplement&&((!(flags&8)&&(flags&64)&&(flags&32))||((flags&8)&&(flags&64)))){auto s=extra<rows.size()?roslyn_elements(rows[extra]):std::vector<a_constant_ptr>();if(s.size()!=4){DB.fail("EDG EH ABI supplement shape");break;}supplement_owner[extra++]=region;abi=DB.fresh("cleanup_abi");Rid locator=roslyn_cleanup_location(s[1]);DB.put("cleanup_abi_detail",{{"id",abi},{"vla_count_location_id",R((flags&8)?locator:0)},{"vtt_entity_id",R(!(flags&8)?locator:0)}});}
 DB.put("cleanup_region",{{"id",region},{"function_eh_id",eh},{"ordinal",static_cast<sqlite3_int64>(i)},{"next_region_id",R(next_it==roslyn_regions.end()?0:next_it->second)},{"condition_id",R(cond)},{"is_supplement",static_cast<int>(supplement)},{"supplement_owner_id",R(supplement?supplement_owner[i]:0)}});
 if(supplement)continue;
 Rid routine=0;if(row[0]->kind==ck_address&&row[0]->variant.address.kind==abk_routine)routine=roslyn_routine(row[0]->variant.address.variant.routine);else if(row[0]->kind!=ck_integer)DB.fail("EDG EH action routine shape");
 Rid arr=0,storage=0;if(flags&8){long long index=roslyn_small_integer(row[1]);if(index<0||static_cast<size_t>(index)>=array_ids.size())DB.fail("EDG EH array index");else arr=array_ids[static_cast<size_t>(index)];}else storage=roslyn_cleanup_location(row[1],(flags&1)!=0,(flags&16)!=0);
 Rid action=DB.fresh("cleanup_action");DB.put("cleanup_action",{{"id",action},{"region_id",region},{"ordinal",0},{"kind_id",K("cleanup_kind",(flags&128)?"guard":(flags&4)?"deallocation":routine?"destructor":"special")},{"routine_id",R(routine)},{"storage_id",R(storage)},{"array_id",R(arr)},{"abi_detail_id",R(abi)},{"is_array",static_cast<int>((flags&8)!=0)},{"is_indirect",static_cast<int>((flags&1)!=0)},{"is_conditional",static_cast<int>((flags&2)!=0)},{"is_base_subobject",static_cast<int>(!(flags&8)&&(flags&64))},{"is_vla",static_cast<int>((flags&8)&&(flags&64))},{"is_deallocation",static_cast<int>((flags&4)!=0)},{"is_local_static_guard",static_cast<int>((flags&128)!=0)}});
 }
 DB.put("function_eh",{{"id",eh},{"routine_id",roslyn_routine(p->routine)},{"prologue_id",prologue},{"region_table_id",R(roslyn_variable(p->region_table))},{"array_table_id",R(roslyn_variable(p->array_table))},{"initial_state",Q(null_eh_region_number)},{"null_state",Q(null_eh_region_number)}});
 if(p->region_table)DB.put("eh_table_projection",{{"owner_id",roslyn_variable(p->region_table)},{"kind_id",K("role","cleanup")},{"function_eh_id",eh}});
 if(p->array_table)DB.put("eh_table_projection",{{"owner_id",roslyn_variable(p->array_table)},{"kind_id",K("role","element")},{"function_eh_id",eh}});return eh;
}
static void roslyn_expr_operands(Rid owner,an_expr_node_ptr list,int role) {int n=0;for(an_expr_node_ptr a=list;a;a=a->next){Rid child=roslyn_expr(a,role);DB.put("expression_operand",{{"owner_id",owner},{"role_id",K("role","operand")},{"ordinal",n},{"expression_id",child}});roslyn_child(owner,"operand",n++,child);}}
static Rid roslyn_expr(an_expr_node_ptr p,int role) {
 bool fresh;Rid id=DB.claim(p,"expression",fresh);if(!fresh)return id;
 if(p->kind==enk_error||p->kind==enk_reclaimed||p->kind==enk_last){DB.fail("Invalid EDG expression reached");return id;}
 int kind=roslyn_kind_expression(p->kind);DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(roslyn_origin(p->position,&p->expr_range.end,&p->expr_range.start))},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 DB.put("expression",{{"id",id},{"type_id",roslyn_type(p->type)},{"value_category_id",K("value_category",p->is_lvalue?"lvalue":p->is_xvalue?"xvalue":"prvalue")},{"original_lvalue_type_id",R(roslyn_type(p->orig_lvalue_type))},{"result_unused",static_cast<int>(p->result_is_not_used)},{"initialization_guard",static_cast<int>(p->is_initialization_guard)},{"generated_default_argument",static_cast<int>(p->generated_default_arg)},{"volatile_fetch",static_cast<int>(p->volatile_fetch)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},{"is_parenthesized",static_cast<int>(p->is_parenthesized)},{"is_static_cast",static_cast<int>(p->is_static_cast)},{"is_functional_cast",static_cast<int>(p->is_functional_notation_cast)},{"is_brace_cast",static_cast<int>(p->is_brace_notation_cast)},{"is_objectless_member_reference",static_cast<int>(p->is_objectless_nonstatic_data_mem_ref)},{"is_gnu_extension",static_cast<int>(p->marked_as_gnu_extension)},{"type_definition_needed",static_cast<int>(p->type_definition_needed)},{"boolean_not_normalized",static_cast<int>(p->is_non_normalized_boolean_controlling_expr)},{"is_safe_cast",static_cast<int>(p->is_safe_cast)}});
 switch(p->kind){
 case enk_operation:{auto &o=p->variant.operation;DB.put("operator_expression",{{"id",id},{"operator_kind_id",roslyn_kind_operator(o.kind)},{"operation_type_kind_id",roslyn_kind_type(o.type_kind)},{"eval_left_to_right",static_cast<int>(o.eval_left_to_right)},{"eval_right_to_left",static_cast<int>(o.eval_right_to_left)},{"pointer_operand_is_second",static_cast<int>(o.pointer_operand_is_second)},{"is_reinterpret_like_cast",static_cast<int>(o.is_reinterpret_like_cast)},{"requires_runtime_cast_check",static_cast<int>(o.requires_runtime_cast_check)},{"returns_lvalue",static_cast<int>(o.returns_lvalue_instead_of_usual_rvalue)},{"is_reinterpret_cast",static_cast<int>(o.is_reinterpret_cast)},{"is_reference_cast",static_cast<int>(o.is_reference_cast)},{"is_rvalue_reference_cast",static_cast<int>(o.is_rvalue_reference_cast)},{"is_const_cast",static_cast<int>(o.is_const_cast)},{"is_tracking_reference_cast",static_cast<int>(o.is_tracking_reference_cast)},{"is_gnu_two_operand_conditional",static_cast<int>(o.is_gnu_two_operand_question_mark)},{"call_uses_operator_syntax",static_cast<int>(o.call_uses_operator_syntax)},{"implicit_member_naming",static_cast<int>(o.implicit_in_member_naming)},{"implicit_explicit_cast_step",static_cast<int>(o.implicit_step_of_explicit_cast)},{"is_conversion_call",static_cast<int>(o.is_conversion_call)},{"argument_dependent_lookup_suppressed",static_cast<int>(o.arg_dependent_lookup_suppressed_on_call)},{"qualified_function_name",static_cast<int>(o.call_with_qualified_function_name)},{"retained_virtual_call",static_cast<int>(o.is_virtual_call)}});roslyn_expr_operands(id,o.operands,role);
 if(o.kind==eok_call||o.kind==eok_dot_member_call||o.kind==eok_points_to_member_call||o.kind==eok_dot_pm_call||o.kind==eok_points_to_pm_call){an_expr_node_ptr callee=o.operands;Rid target=callee&&callee->kind==enk_routine?roslyn_routine(callee->variant.routine.ptr):0;DB.put("call_expression",{{"id",id},{"callee_id",R(roslyn_expr(callee,role))},{"target_routine_id",R(target)},{"function_type_id",R(callee?roslyn_type(callee->type):0)},{"call_kind_id",K("call_kind",target?"direct":"indirect")}});int n=0;for(an_expr_node_ptr a=callee?callee->next:nullptr;a;a=a->next)DB.put("call_argument",{{"owner_id",id},{"ordinal",n++},{"expression_id",roslyn_expr(a,role)},{"role_id",K("role","argument")}});}
 break;}
 case enk_constant:DB.put("constant_expression",{{"id",id},{"constant_id",roslyn_constant(p->variant.constant.ptr)}});break;
 case enk_variable:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_variable(p->variant.variable.ptr)},{"reference_kind_id",K("reference_kind","variable")}});break;
 case enk_routine:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_routine(p->variant.routine.ptr))},{"reference_kind_id",K("reference_kind","routine")}});break;
 case enk_field:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_field(p->variant.field.ptr)},{"reference_kind_id",K("reference_kind","field")}});break;
 case enk_object_lifetime:{auto &l=p->variant.object_lifetime;Rid ex=roslyn_expr(l.expr,role);DB.put("lifetime_expression",{{"id",id},{"expression_id",R(ex)},{"lifetime_id",R(roslyn_lifetime(l.ptr))}});roslyn_child(id,"body",0,ex);break;}
 case enk_temp_init:case enk_lambda:DB.put("temporary_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.init.dynamic_init,role)}});roslyn_child(id,"initialization",0,roslyn_init(p->variant.init.dynamic_init,role));break;
 case enk_statement:{Rid body=roslyn_statement(p->variant.statement,role);DB.put("statement_expression",{{"id",id},{"statement_id",body}});roslyn_child(id,"body",0,body);break;}
 case enk_reuse_value:DB.put("reuse_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.reused_value_init)}});break;
 case enk_builtin_operation:DB.put("builtin_expression",{{"id",id},{"kind_id",roslyn_kind_builtin(p->variant.builtin_operation.kind)}});roslyn_expr_operands(id,p->variant.builtin_operation.operands,role);break;
 case enk_type_operand:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_type(p->variant.type_operand.type))},{"reference_kind_id",K("reference_kind","type")}});break;
 case enk_param_ref:DB.put("parameter_reference_expression",{{"id",id},{"parameter_number",static_cast<int>(p->variant.param_ref.param_num)},{"parameter_depth",static_cast<int>(p->variant.param_ref.levels_up)},{"is_this",static_cast<int>(p->variant.param_ref.param_num==0)}});break;
 case enk_throw:{auto t=p->variant.throw_info;Rid ex=t?roslyn_expr(t->expr,role):0;DB.put("throw_expression",{{"id",id},{"operand_id",R(ex)},{"thrown_type_id",R(t?roslyn_type(t->type):0)},{"initialization_id",R(t?roslyn_init(t->dynamic_init):0)},{"destructor_id",R(t?roslyn_routine(t->destructor):0)},{"is_rethrow",static_cast<int>(!t)}});roslyn_child(id,"operand",1,ex);break;}
 case enk_condition:{auto t=p->variant.condition;Rid ex=roslyn_expr(t->expr,role);DB.put("condition_expression",{{"id",id},{"expression_id",R(ex)},{"initialization_id",R(roslyn_init(t->dynamic_init))},{"scope_id",R(roslyn_scope(t->scope))}});roslyn_child(id,"condition",0,ex);roslyn_child(id,"initialization",0,roslyn_statement(t->initialization,role));break;}
 case enk_sizeof:case enk_alignof:case enk_datasizeof:{auto &q=p->variant.sizeof_info;Rid ex=q.is_type?0:roslyn_expr(q.variant.expr,7002);DB.put("type_query_expression",{{"id",id},{"query_kind_id",kind},{"operand_type_id",R(q.is_type?roslyn_type(q.variant.type):0)},{"operand_expression_id",R(ex)},{"is_standard_alignof",static_cast<int>(q.is_std_alignof)}});roslyn_reference(id,"metadata",0,ex);break;}
 case enk_braced_init_list:roslyn_expr_operands(id,p->variant.braced_init_list,7002);DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"is_dependent",1}});break;
 case enk_initializer:DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"initialization_id",R(roslyn_init(p->variant.initializer.dyn_init))}});break;
 case enk_const_eval_deferred:{Rid ex=roslyn_expr(p->variant.const_eval_deferred.wrapped,7002);DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"wrapped_expression_id",R(ex)},{"is_dependent",1}});roslyn_reference(id,"metadata",0,ex);break;}
 case enk_address_of_ellipsis:case enk_result_of_overriding_function:break;
 case enk_lowered_eh_construct:{auto &e=p->variant.lowered_eh;Rid eh=roslyn_current_eh,region=0;std::string state;Rid routine=0,handler=0,try_id=0,te=0,ce=0;
 switch(e.kind){case leck_function_prologue:eh=roslyn_eh(e.variant.prologue_info,id);break;case leck_cleanup_state:case leck_unreachable_cleanup_state:{auto n=e.variant.cleanup_region_number;state=Q(n);auto found=roslyn_regions.find({eh,static_cast<unsigned long>(n)});if(found!=roslyn_regions.end())region=found->second;DB.put("cleanup_state_operation",{{"id",id},{"region_id",R(region)},{"state_number",state},{"is_unreachable",static_cast<int>(e.kind==leck_unreachable_cleanup_state)}});break;}case leck_function_epilogue:routine=roslyn_routine(e.variant.epilogue_routine);break;case leck_caught_object_address:handler=roslyn_handler(e.variant.caught_object_handler);break;case leck_catch_epilogue:handler=roslyn_handler(e.variant.epilogue_handler);break;case leck_try_epilogue:{auto t=roslyn_try_blocks.find(e.variant.epilogue_try_block);if(t!=roslyn_try_blocks.end())try_id=t->second;else DB.fail("EH try epilogue missing retained try owner");break;}case leck_internal_try:te=roslyn_expr(e.variant.try_and_catch_expr,role);ce=roslyn_expr(e.variant.try_and_catch_expr->next,role);roslyn_child(id,"then",0,te);roslyn_child(id,"else",0,ce);break;default:break;}
 DB.put("lowered_eh_operation",{{"id",id},{"kind_id",roslyn_kind_lowered_eh(e.kind)},{"function_eh_id",R(eh)},{"region_id",R(region)},{"state_number",state.empty()?RoslynSqlValue():RoslynSqlValue(state)},{"routine_id",R(routine)},{"handler_id",R(handler)},{"try_block_id",R(try_id)},{"try_expression_id",R(te)},{"catch_expression_id",R(ce)}});break;}
 default:DB.fail("Expression payload adapter missing: "+Q(p->kind));break;
 }
 return id;
}
static Rid roslyn_handler(a_handler_ptr p,Rid try_id,int ordinal) {
 bool fresh;Rid id=DB.claim(p,"handler",fresh);if(!fresh)return id;Rid body=roslyn_statement(p->statement);
 DB.put("handler",{{"id",id},{"try_id",R(try_id)},{"ordinal",ordinal},{"body_id",R(body)},{"caught_type_id",R(p->parameter?roslyn_type(p->parameter->type):0)},{"parameter_id",R(roslyn_variable(p->parameter))},{"parameter_initialization_id",R(roslyn_init(p->dynamic_init))},{"typeinfo_id",R(roslyn_variable(p->typeinfo_var))}});return id;
}
static Rid roslyn_statement(a_statement_ptr p,int role) {
 bool fresh;Rid id=DB.claim(p,"statement",fresh);if(!fresh)return id;if(p->kind==stmk_try_block)roslyn_try_blocks[p->variant.try_block]=id;int kind=roslyn_kind_statement(p->kind);
 DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(roslyn_origin(p->position,&p->end_position))},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 Rid expression=roslyn_expr(p->expr,role);DB.put("statement",{{"id",id},{"kind_id",kind},{"expression_id",R(expression)},{"parent_id",R(roslyn_statement(p->parent,role))},{"is_guard",static_cast<int>(p->is_initialization_guard)},{"lowering_generated",static_cast<int>(p->lowering_generated)}});roslyn_child(id,"operand",0,expression);
 switch(p->kind){
 case stmk_expr:case stmk_empty:case stmk_coroutine_return:case stmk_assigned_goto:break;
 case stmk_block:{auto info=p->variant.block.extra_info;DB.put("block_statement",{{"id",id},{"scope_id",R(info?roslyn_scope(info->assoc_scope):0)},{"lifetime_id",R(info?roslyn_lifetime(info->lifetime):0)}});int n=0;for(a_statement_ptr c=p->variant.block.statements;c;c=c->next){Rid child=roslyn_statement(c,role);DB.put("block_item",{{"owner_id",id},{"ordinal",n},{"statement_id",child}});roslyn_child(id,"statement",n++,child);}break;}
 case stmk_if:case stmk_if_consteval:case stmk_if_not_consteval:{auto &s=p->variant.if_stmt;Rid then=roslyn_statement(s.then_statement,role),els=roslyn_statement(s.else_statement,role);DB.put("if_statement",{{"id",id},{"then_id",R(then)},{"else_id",R(els)},{"condition_id",R(expression)}});roslyn_child(id,"then",0,then);roslyn_child(id,"else",0,els);break;}
 case stmk_while:case stmk_end_test_while:{Rid body=roslyn_statement(p->variant.loop_statement,role);DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"condition_id",R(expression)}});roslyn_child(id,"body",0,body);break;}
 case stmk_for:{auto &s=p->variant.for_loop;Rid body=roslyn_statement(s.statement,role),init=s.extra_info?roslyn_statement(s.extra_info->initialization,role):0,inc=s.extra_info?roslyn_expr(s.extra_info->increment,role):0;DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"initialization_id",R(init)},{"condition_id",R(expression)},{"increment_id",R(inc)},{"init_scope_id",R(s.extra_info?roslyn_scope(s.extra_info->for_init_scope):0)}});roslyn_child(id,"initialization",0,init);roslyn_child(id,"body",0,body);roslyn_child(id,"increment",0,inc);break;}
 case stmk_return:case stmk_goto:{Rid label=p->kind==stmk_goto?roslyn_label(p->variant.label.ptr):0;DB.put("jump_statement",{{"id",id},{"kind_id",K("jump_kind",p->kind==stmk_return?"return":p->variant.label.ptr->break_label?"break":p->variant.label.ptr->continue_label?"continue":"goto")},{"value_id",R(expression)},{"label_id",R(label)},{"initialization_id",R(p->kind==stmk_return?roslyn_init(p->variant.return_dynamic_init):0)}});break;}
 case stmk_label:DB.put("label_statement",{{"id",id},{"label_id",roslyn_label(p->variant.label.ptr)}});break;
 case stmk_switch:{auto &s=p->variant.switch_stmt;Rid body=roslyn_statement(s.body_statement,role);DB.put("switch_statement",{{"id",id},{"body_id",R(body)},{"selector_id",R(expression)}});roslyn_child(id,"body",0,body);int n=0;for(a_switch_case_entry_ptr c=s.extra_info->cases;c;c=c->next)DB.put("switch_case",{{"owner_id",id},{"ordinal",n++},{"statement_id",roslyn_statement(c->stmt,role)},{"low_constant_id",R(roslyn_constant(c->case_value))},{"high_constant_id",R(roslyn_constant(c->range_end))},{"is_default",static_cast<int>(!c->case_value)}});break;}
 case stmk_switch_case:break;
 case stmk_init:{Rid init=roslyn_init(p->variant.dynamic_init);DB.put("initialization_statement",{{"id",id},{"initialization_id",init}});roslyn_reference(id,"initialization",0,init);roslyn_child(id,"initialization",0,init);break;}
 case stmk_try_block:{auto s=p->variant.try_block;Rid body=roslyn_statement(s->statement,role);DB.put("try_statement",{{"id",id},{"body_id",body},{"is_function_try",static_cast<int>(s->is_function_try_block)},{"lifetime_id",R(roslyn_lifetime(s->lifetime))}});roslyn_child(id,"body",0,body);int n=0;for(a_handler_ptr h=s->handlers;h;h=h->next){Rid hid=roslyn_handler(h,id,n);roslyn_reference(id,"member",n++,hid);roslyn_child(id,"else",n,roslyn_statement(h->statement,role));}break;}
 case stmk_microsoft_try:{auto s=p->variant.microsoft_try;Rid body=roslyn_statement(s->guarded_statement,role),filter=roslyn_expr(s->except_expr,role),cleanup=roslyn_statement(s->cleanup_statement,role);DB.put("microsoft_try_statement",{{"id",id},{"body_id",body},{"filter_id",R(filter)},{"handler_id",R(filter?cleanup:0)},{"finally_id",R(!filter?cleanup:0)},{"is_finally",static_cast<int>(!filter)}});roslyn_child(id,"body",0,body);roslyn_child(id,"else",0,cleanup);roslyn_child(id,"condition",0,filter);break;}
 case stmk_decl:break;
 default:DB.fail("Statement payload adapter missing: "+Q(p->kind));break;
 }
 return id;
}
static Rid roslyn_base(a_base_class_ptr p) {
 bool fresh;Rid id=DB.claim(p,"base_subobject",fresh);if(!fresh)return id;
 DB.put("base_subobject",{{"id",id},{"most_derived_type_id",roslyn_type(p->derived_class)},{"base_type_id",roslyn_type(p->type)},{"offset",p->offset_is_set?RoslynSqlValue(Q(p->offset)):RoslynSqlValue()},{"is_virtual",static_cast<int>(p->is_virtual)},{"is_direct",static_cast<int>(p->direct)}});
 int path_number=0;for(a_base_class_derivation_ptr d=p->derivation;d;d=d->next){bool df;Rid deriv=DB.claim(d,"base_derivation",df);if(!df)continue;int count=0;for(a_derivation_step_ptr step=d->path;step;step=step->next){DB.put("base_derivation_step",{{"owner_id",deriv},{"ordinal",count++},{"subobject_id",roslyn_base(step->base_class)}});if(step==d->path_tail)break;}DB.put("base_derivation",{{"id",deriv},{"base_subobject_id",id},{"ordinal",path_number++},{"is_direct",static_cast<int>(d->direct)},{"is_preferred",static_cast<int>(d->preferred)},{"access_id",K("access",d->access==as_private?"private":d->access==as_protected?"protected":"public")},{"path_tail_ordinal",count?RoslynSqlValue(count-1):RoslynSqlValue()}});}
 if(!p->is_pack_expansion){for(an_overriding_virtual_function_ptr o=p->variant.overriding_virtual_functions;o;o=o->next)DB.put("override_relation",{{"owner_id",roslyn_routine(o->overriding_function)},{"ordinal",roslyn_override_ordinals[roslyn_routine(o->overriding_function)]++},{"overridden_id",roslyn_routine(o->primary_function)},{"subobject_id",id},{"overriding_subobject_id",R(roslyn_base(o->base_class))},{"return_adjustment_subobject_id",R(roslyn_base(o->return_adjustment_base_class))}});}
 return id;
}
#include "roslyn_templates.h"
static Rid roslyn_macro(a_macro_ptr p) {
 bool fresh;Rid id=DB.claim(p,"macro_definition",fresh);if(!fresh)return id;
 const char *origin=p->is_predefined?"predefined":p->is_command_line_definition?"command_line":"ordinary";
 DB.put("macro_definition",{{"id",id},{"name_id",DB.atom(p->source_corresp.name)},{"text_id",DB.atom(p->text)},{"position_id",R(roslyn_position(p->source_corresp.decl_position))},{"kind_id",K("source_origin_kind",origin)},{"is_function_like",static_cast<int>(!p->object_like)},{"is_predefined",static_cast<int>(p->is_predefined)},{"is_undef",static_cast<int>(p->is_undef)}});return id;
}
static void roslyn_macros() {
 for(a_macro_invocation_record_index n=1;n<il_header.num_macro_invocation_records;++n){a_macro_invocation_record_ptr p;set_macro_inv_record_ptr_to_index(il_header.root_macro_invocation_record_block,n,p);if(p&&p->parent_macro_index>=NO_PARENT_MACRO_INVOCATION&&p->assoc_macro)roslyn_macro_invocations[n]=DB.fresh("macro_invocation");}
 for(a_macro_ptr p=il_header.macros;p;p=p->next)roslyn_macro(p);
 int ordinal=0;for(const auto &entry:roslyn_macro_invocations){a_macro_invocation_record_ptr p;set_macro_inv_record_ptr_to_index(il_header.root_macro_invocation_record_block,entry.first,p);a_source_position start{},end{};start.seq=start.orig_seq=p->start.seq;start.column=start.orig_column=p->start.column;end.seq=end.orig_seq=p->end.seq;end.column=end.orig_column=p->end.column;
 auto parent=roslyn_macro_invocations.find(p->parent_macro_index);DB.put("macro_invocation",{{"id",entry.second},{"definition_id",roslyn_macro(p->assoc_macro)},{"parent_id",R(parent==roslyn_macro_invocations.end()?0:parent->second)},{"position_id",R(roslyn_position(start))},{"end_position_id",R(roslyn_position(end))},{"argument_text_id",R(DB.atom(p->arguments))},{"ordinal",ordinal++}});}
}
static void roslyn_capability(const char *name,const std::string &value,bool enabled,bool required=false){DB.put("build_capability",{{"name_id",DB.atom(name)},{"state_id",K("availability","available")},{"value_id",DB.atom(value)},{"enabled",static_cast<int>(enabled)},{"required",static_cast<int>(required)}});}
static void roslyn_back_end() {
 RoslynStore store;roslyn_db=&store;roslyn_current_eh=0;roslyn_static_ordinal=0;roslyn_override_ordinals.clear();roslyn_local_constants.clear();roslyn_projected_tables.clear();roslyn_function_eh.clear();roslyn_regions.clear();roslyn_try_blocks.clear();roslyn_current_routine=nullptr;roslyn_macro_invocations.clear();roslyn_positions.clear();roslyn_values.clear();
 std::string output=std::getenv("ROSLYNOBJ_OUTPUT")?std::getenv("ROSLYNOBJ_OUTPUT"):std::string(il_header.primary_source_file->file_name)+".roslynobj";
 std::string temporary=output+".tmp.XXXXXX";std::vector<char> sibling(temporary.begin(),temporary.end());sibling.push_back(0);int temporary_fd=mkstemp(sibling.data());if(temporary_fd<0){std::perror("roslynobj temporary output");std::exit(EXIT_FAILURE);}if(::close(temporary_fd)!=0){std::perror("roslynobj temporary close");std::remove(sibling.data());std::exit(EXIT_FAILURE);}temporary=sibling.data();
 if(sqlite3_open_v2(temporary.c_str(),&store.connection,SQLITE_OPEN_READWRITE,nullptr)!=SQLITE_OK)store.fail(sqlite3_errmsg(store.connection));
 store.exec("PRAGMA foreign_keys=ON; PRAGMA journal_mode=DELETE; BEGIN IMMEDIATE;");store.exec(roslynobj_schema_sql);
 roslyn_unit=DB.fresh("translation_unit");Rid source=roslyn_source_file(il_header.primary_source_file);roslyn_macros();Rid scope=roslyn_scope(il_header.primary_scope);
 DB.put("translation_unit",{{"id",roslyn_unit},{"global_scope_id",scope},{"primary_input_id",source},{"language_mode_id",DB.atom(std::string(il_header.source_language==sl_C?"c":"c++")+Q(il_header.std_version))}});
 DB.put("target_profile",{{"id",1},{"abi_id",K("abi","itanium")},{"addressable_unit_bits",static_cast<int>(targ_char_bit)},{"little_endian",static_cast<int>(targ_little_endian)},{"pointer_bits",static_cast<int>(targ_sizeof_pointer*targ_char_bit)},{"pointer_alignment",Q(targ_alignof_pointer)}});
#define CAP(name) roslyn_capability(#name,Q(name),name!=0)
 CAP(DO_IL_LOWERING);CAP(DO_FULL_PORTABLE_EH_LOWERING);CAP(GENERATE_EH_TABLES);CAP(DO_UNORDERED_EH_PROCESSING);CAP(KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED);CAP(BACK_END_IS_C_GEN_BE);CAP(BACK_END_IS_CP_GEN_BE);CAP(BACK_END_SHOULD_BE_CALLED);CAP(DOING_SOURCE_ANALYSIS);CAP(IL_SHOULD_BE_WRITTEN_TO_FILE);CAP(IA64_ABI);CAP(MINIMAL_INLINING);CAP(MAINTAIN_NEEDED_FLAGS);CAP(DEFAULT_REMOVE_UNNEEDED_ENTITIES);CAP(PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE);CAP(LOWER_VARIABLE_LENGTH_ARRAYS);CAP(LOWER_CLASS_RVALUE_ADJUST);CAP(LOWER_COMPLEX);CAP(LOWER_FIXED_POINT);CAP(LOWER_IFUNC);CAP(LOWER_LVALUE_RETURNING_OPERATIONS);CAP(LOWER_MICROSOFT_NONCONSTANT_AGGREGATE);CAP(LOWER_DESIGNATED_INITIALIZERS);CAP(GENERATE_SOURCE_SEQUENCE_LISTS);CAP(RECORD_BACKING_EXPRS_WITH_IL_LOWERING);CAP(ONE_INSTANTIATION_PER_OBJECT);CAP(EXTRA_SOURCE_POSITIONS_IN_IL);CAP(RECORD_MACROS_IN_IL);CAP(FULLY_RESOLVED_MACRO_POSITIONS);CAP(RECORD_MACRO_INVOCATIONS);CAP(MACRO_INVOCATION_TREE_IN_IL);CAP(RECORD_MACRO_ARGS);CAP(COMPILE_MULTIPLE_TRANSLATION_UNITS);CAP(COROUTINE_ENABLING_POSSIBLE);CAP(PROTOTYPE_INSTANTIATIONS_IN_IL);CAP(ALL_TEMPLATE_INFO_IN_IL);CAP(prototype_instantiations_in_il);CAP(all_template_info_in_il);CAP(HOST_FP_VALUE_IS_128BIT);CAP(USE_SOFTFLOAT);CAP(USE_FLOAT128_FOR_HOST_FP_VALUE);CAP(HOST_HAS_FLOAT16_TYPE);CAP(exceptions_enabled);CAP(gpp_mode);CAP(gcc_mode);CAP(clang_mode);CAP(gnu_version);CAP(microsoft_mode);CAP(microsoft_version);CAP(cppcli_enabled);CAP(cppcx_enabled);
#undef CAP
 const char *count_text=std::getenv("ROSLYNOBJ_ARGUMENT_COUNT");if(count_text){long count=std::strtol(count_text,nullptr,10);for(long i=0;i<count;++i){std::string key="ROSLYNOBJ_ARGUMENT_"+Q(i);const char *argument=std::getenv(key.c_str());if(argument)DB.put("invocation_argument",{{"ordinal",static_cast<sqlite3_int64>(i)},{"text_id",DB.atom(argument)}});}}
 roslyn_capability("normalized_semantic_graph_v1","1",true,true);roslyn_capability("exact_target_values_v1","1",true,true);if(!roslyn_function_eh.empty())roslyn_capability("edg_partial_eh_v1","1",true,true);
 unsigned char random[16];sqlite3_randomness(16,random);std::string artifact;static const char hex[]="0123456789abcdef";for(unsigned char b:random){artifact+=hex[b>>4];artifact+=hex[b&15];}
 DB.put("object_header",{{"id",1},{"format_major",1},{"format_minor",0},{"artifact_id",artifact},{"producer_id",DB.atom("roslyn_gen_be")},{"revision_id",DB.atom("89e67e07c7f6d0fc38622f9778d8e79f40deaa0f")},{"edg_version_id",DB.atom(il_header.compiler_version)},{"capture_profile_id",K("capture_profile","native_lowered")},{"target_profile_id",1},{"main_source_file_id",source},{"translation_unit_id",roslyn_unit},{"complete",1}});
 if(!store.failed)store.exec("COMMIT;");else store.exec("ROLLBACK;");
 store.close();roslyn_db=nullptr;
 if(store.failed){std::remove(temporary.c_str());std::fprintf(stderr,"roslynobj export error: %s\n",store.failure.c_str());std::exit(EXIT_FAILURE);}
 if(std::rename(temporary.c_str(),output.c_str())!=0){std::remove(temporary.c_str());std::perror("roslynobj publish");std::exit(EXIT_FAILURE);}
 const char *status_path=std::getenv("ROSLYNOBJ_STATUS_PATH");if(status_path){FILE *status=std::fopen(status_path,"w");if(!status){std::perror("roslynobj status");std::exit(EXIT_FAILURE);}bool ok=std::fputs("published\n",status)>=0;if(std::fclose(status)!=0)ok=false;if(!ok){std::perror("roslynobj status write");std::exit(EXIT_FAILURE);}}
 std::fprintf(stderr,"roslynobj published %s: entities=%lld text_atoms=%lld\n",output.c_str(),static_cast<long long>(store.sequence),static_cast<long long>(store.atom_sequence));
}
END_EDG_NAMESPACE
USING_NAMESPACE_EDG
void back_end(void) { roslyn_back_end(); }
