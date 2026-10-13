/* Independent EDG semantic backend. SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception */
#include "basic_hdrs.h"
#include "fe_common.h"
#include "lower_eh.h"
#include <sqlite3.h>
#include <cstdlib>
#include <cstdio>
#include <map>
#include <set>
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
static int roslyn_default_role=DO_IL_LOWERING?7001:7003;
static std::map<a_macro_invocation_record_index,Rid> roslyn_macro_invocations;
static std::map<std::string,Rid> roslyn_positions;
static std::map<std::pair<Rid,Rid>,Rid> roslyn_ranges,roslyn_origins;
static std::set<std::pair<Rid,int>> roslyn_availability;
static std::map<std::string,Rid> roslyn_values;
static std::map<std::pair<Rid,int>,Rid> roslyn_name_values;
static std::map<Rid,Rid> roslyn_primary_declarations;
static std::map<const void*,std::set<Rid>> roslyn_parameter_placeholders;
static std::map<a_constant_ptr,Rid> roslyn_constant_values;
static std::map<const void*,a_scope_ptr> roslyn_scope_parents;

#define DB (*roslyn_db)
#define K(f,n) roslyn_tag(f,n)
#define R(v) roslyn_ref(v)
#define Q(v) std::to_string(v)
static void roslyn_fact(Rid owner,const char *fact,const char *state,const char *reason=nullptr) {
 if(!owner)return;int kind=K("fact",fact);if(!roslyn_availability.insert({owner,kind}).second)return;
 DB.put("fact_availability",{{"owner_id",owner},{"fact_id",kind},{"state_id",K("availability",state)},{"reason_id",R(DB.atom(reason))}});
}
#ifndef ROSLYN_CAPTURE_PROFILE_ID
#define ROSLYN_CAPTURE_PROFILE_ID (DO_IL_LOWERING?(IA64_ABI?2001:2003):2002)
#endif
#if DO_IL_LOWERING
#define LF(p,f) static_cast<int>((p)->f)
#define LR(p,f,fn) fn((p)->f)
#else
#define LF(p,f) 0
#define LR(p,f,fn) 0
#endif
#if DO_IL_LOWERING && !DO_FULL_PORTABLE_EH_LOWERING
#define ER(p,f,fn) fn((p)->f)
#else
#define ER(p,f,fn) 0
#endif
#if DO_IL_LOWERING && IA64_ABI
#define ROSLYN_PRIMARY_ENTRY(p) roslyn_routine((p)->primary_ctor_or_dtor)
#else
#define ROSLYN_PRIMARY_ENTRY(p) 0
#endif
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
#define TLS_ALIAS(p) static_cast<int>((p)->is_tls_init_alias)
#else
#define TLS_ALIAS(p) 0
#endif
static void roslyn_collect_local_initializers(a_scope_ptr);
static Rid roslyn_type(a_type_ptr);
static Rid roslyn_scope(a_scope_ptr);
static Rid roslyn_routine(a_routine_ptr);
static Rid roslyn_variable(a_variable_ptr);
#if DECL_MODIFIERS_IN_USE
static void roslyn_decl_modifiers(Rid,a_decl_modifier_set);
#endif
static void roslyn_routine_type(a_type_ptr,Rid);
static Rid roslyn_requires_clause(a_requires_clause_ptr);
static void roslyn_qualifier_fields(std::vector<std::pair<const char*,RoslynSqlValue>> &,a_type_qualifier_set);
static Rid roslyn_field(a_field_ptr,a_type_ptr=nullptr);
static Rid roslyn_label(a_label_ptr);
static Rid roslyn_constant(a_constant_ptr);
static Rid roslyn_expr(an_expr_node_ptr,int=roslyn_default_role);
static Rid roslyn_statement(a_statement_ptr,int=roslyn_default_role);
static Rid roslyn_init(a_dynamic_init_ptr,int=roslyn_default_role);
static Rid roslyn_lifetime(an_object_lifetime_ptr);
static Rid roslyn_namespace(a_namespace_ptr);
static Rid roslyn_handler(a_handler_ptr);
static Rid roslyn_block_descriptor(a_block_ptr);
static Rid roslyn_try_descriptor(a_try_supplement_ptr);
static Rid roslyn_local_static_initialization(a_local_static_variable_init_ptr);
static Rid roslyn_constructor_initializer(a_constructor_init_ptr);
static Rid roslyn_token_sequence(a_token_sequence_ptr);
static Rid roslyn_template(a_template_ptr);
static Rid roslyn_base(a_base_class_ptr);
static Rid roslyn_instance(const void *,Rid,a_template_ptr,a_template_arg_ptr,a_template_arg_ptr);
static Rid roslyn_argument(a_template_arg_ptr);
static Rid roslyn_parameter_type(a_param_type_ptr);
static Rid roslyn_lambda(a_lambda_ptr);
static Rid roslyn_vla_dimension(a_vla_dimension_ptr);
static Rid roslyn_asm(an_asm_entry_ptr);
static void roslyn_source_sequences(a_scope_ptr,Rid);
static void roslyn_record_declarations(a_type_ptr,Rid);
#if EXTRA_SOURCE_POSITIONS_IN_IL
static void roslyn_declaration_positions(Rid,a_decl_position_supplement_ptr,const char *);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
static Rid roslyn_ms_attribute(an_ms_attribute_ptr);
static void roslyn_ms_attributes(Rid,an_ms_attribute_ptr);
#endif
static an_expr_node_ptr roslyn_local_expression(const void *,a_local_expr_node_ref_kind);
static a_vla_dimension_ptr roslyn_type_vla_dimension(a_type_ptr);
static void roslyn_scope_expression_references(a_scope_ptr,Rid);
static Rid roslyn_scoped_expression(a_scoped_expression_ptr);
static Rid roslyn_entity(const a_tagged_pointer &);
static int roslyn_builtin_function_kind(a_builtin_function_kind p) {
 if(!p)return 0;
 if(p<bfk_last)return roslyn_kind_edg_builtin_function(static_cast<a_builtin_function_kind_tag>(p));
 return roslyn_kind_edg_builtin_user_function(static_cast<a_builtin_user_function_kind>(p));
}
static Rid roslyn_source_file(a_source_file_ptr,Rid=0,int=0);
static void roslyn_macros();
static Rid roslyn_module(a_module_ptr);
#include "roslyn_source.h"
static Rid roslyn_name(a_source_correspondence &c) {
 const char *name=c.name;
#if NEED_NAME_MANGLING
 if(c.name_has_been_mangled)name=c.unmangled_name_or_mangled_encoding;
#endif
 Rid spelling=DB.atom(name);int category=K("name_category",name?"named":"unnamed");auto key=std::make_pair(spelling,category);auto known=roslyn_name_values.find(key);if(known!=roslyn_name_values.end())return known->second;Rid id=DB.fresh("name");roslyn_name_values[key]=id;DB.put("name",{{"id",id},{"spelling_id",R(spelling)},{"category_id",category}});return id;
}
#include "roslyn_attributes.h"
#include "roslyn_parameters.h"
#include "roslyn_names.h"
#include "roslyn_type_support.h"
#include "roslyn_modules.h"
static a_scope_ptr roslyn_scope_parent(a_scope_ptr p) {if(!p)return nullptr;if(p->parent)return p->parent;auto found=roslyn_scope_parents.find(p);return found==roslyn_scope_parents.end()?nullptr:found->second;}
static bool roslyn_metadata_routine(a_routine_ptr p) {return p&&(p->is_prototype_instantiation||p->is_consteval||special_kind_is(p,sfk_deduction_guide));}
static int roslyn_routine_role(a_routine_ptr p) {return roslyn_metadata_routine(p)?7002:DO_IL_LOWERING?7001:7003;}
static a_routine_ptr roslyn_scope_routine(a_scope_ptr p) {std::set<a_scope_ptr> seen;for(;p&&seen.insert(p).second;p=roslyn_scope_parent(p))if(p->kind==sck_function)return p->variant.routine.ptr;return nullptr;}
static a_scope_ptr roslyn_source_scope(a_source_correspondence &c) {if(c.parent_scope)return c.parent_scope;auto found=roslyn_scope_parents.find(&c);return found==roslyn_scope_parents.end()?nullptr:found->second;}
static Rid roslyn_owner(a_source_correspondence &c) {
 a_scope_ptr retained_parent=roslyn_source_scope(c);
 if(retained_parent&&(retained_parent->kind==sck_class_struct_union||retained_parent->kind==sck_enum))return roslyn_type(retained_parent->variant.assoc_type);
 if(retained_parent&&retained_parent->kind==sck_namespace)return roslyn_namespace(retained_parent->variant.assoc_namespace);
 if(c.enclosing_routine)return roslyn_routine(c.enclosing_routine);
 if(retained_parent){a_scope_ptr p=retained_parent;if(p->kind==sck_class_struct_union)return roslyn_type(p->variant.assoc_type);if(p->kind==sck_namespace)return roslyn_namespace(p->variant.assoc_namespace);return roslyn_scope(p);}
 return 0;
}
static Rid roslyn_prepare_declaration(Rid symbol,bool declare=true) {
 if(!declare)return 0;auto known=roslyn_primary_declarations.find(symbol);if(known!=roslyn_primary_declarations.end())return known->second;
 Rid declaration=DB.fresh("declaration");roslyn_primary_declarations[symbol]=declaration;return declaration;
}
static void roslyn_register_parameter_placeholder(const void *descriptor,Rid parameter) {
 if(!descriptor)return;
 auto &bindings=roslyn_parameter_placeholders[descriptor];bindings.insert(parameter);
 Rid symbol=bindings.size()==1?*bindings.begin():0;
 Rid type=DB.existing(descriptor,"type");if(type)DB.set_type_symbol(type,symbol);
 Rid templ=DB.existing(descriptor,"template");if(templ)DB.set_template_symbol(templ,symbol);
}
static void roslyn_symbol(Rid id,const char *kind,a_source_correspondence &c,Rid type,int generated=0,int storage=0,bool attrs=true,bool declare=true) {
 Rid declaration=roslyn_prepare_declaration(id,declare);
 DB.put("symbol",{{"id",id},{"kind_id",K("entity",kind)},{"name_id",roslyn_name(c)},{"containing_entity_id",R(roslyn_owner(c))},{"type_id",R(type)},{"source_origin_id",R(roslyn_origin(c.decl_position))},{"compiler_generated",generated},{"linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(c.name_linkage))},{"access_id",roslyn_kind_edg_access(static_cast<an_access_specifier>(c.access))},{"storage_id",R(storage)}});
 if(attrs)roslyn_attributes(id,c.attributes);
 roslyn_fact(id,"source",roslyn_has_position(c.decl_position)?"available":"absent",roslyn_has_position(c.decl_position)?nullptr:"The retained declaration has no source sequence position.");
 if(!declare)return;
 DB.put("declaration",{{"id",declaration},{"kind_id",K("entity",kind)},{"symbol_id",id},{"lexical_scope_id",R(roslyn_scope(roslyn_source_scope(c)))},{"source_origin_id",R(roslyn_origin(c.decl_position))},{"compiler_generated",generated}});
#if EXTRA_SOURCE_POSITIONS_IN_IL
 roslyn_declaration_positions(declaration,c.decl_pos_info,kind);
#endif
}
static void roslyn_child(Rid owner,const char *role,int ordinal,Rid child) {
 if(child){DB.evaluated_child(owner,child);DB.put("structural_child",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"child_id",child}});}
}
static void roslyn_reference(Rid owner,const char *role,int ordinal,Rid target) {
 if(target)DB.put("semantic_reference",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"target_id",target}});
}
#include "roslyn_entities.h"
static Rid roslyn_scope(a_scope_ptr p) {
 bool fresh;Rid id=DB.claim(p,"scope",fresh);if(!fresh)return id;
 int saved_role=roslyn_default_role;a_routine_ptr saved_routine=roslyn_current_routine;
 a_routine_ptr owning_routine=roslyn_scope_routine(p);if(owning_routine){roslyn_current_routine=owning_routine;roslyn_default_role=roslyn_routine_role(owning_routine);}
 Rid assoc=0;if(p->kind==sck_function)assoc=roslyn_routine(p->variant.routine.ptr);else if(p->kind==sck_namespace)assoc=roslyn_namespace(p->variant.assoc_namespace);else if(p->kind==sck_class_struct_union||p->kind==sck_func_prototype||p->kind==sck_enum)assoc=roslyn_type(p->variant.assoc_type);else if(p->kind==sck_condition)assoc=roslyn_statement(p->variant.assoc_statement);else if(p->kind==sck_block)assoc=roslyn_handler(p->variant.assoc_handler);
 DB.put("scope",{{"id",id},{"kind_id",roslyn_kind_edg_scope(p->kind)},{"parent_id",R(roslyn_scope(roslyn_scope_parent(p)))},{"associated_entity_id",R(assoc)},{"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"function_static_lifetime_id",R(p->kind==sck_function?roslyn_lifetime(p->variant.routine.lifetime_of_local_static_vars):0)}});
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
 for(a_using_decl_ptr u=p->using_declarations;u;u=u->next){MEMBER(roslyn_using(u,p));}
 for(a_using_decl_ptr u=p->using_directives;u;u=u->next){MEMBER(roslyn_using(u,p));}
 for(an_asm_entry_ptr a=p->asm_entries;a;a=a->next){MEMBER(roslyn_asm(a));}
 for(a_vla_dimension_ptr d=p->vla_dimensions;d;d=d->next){MEMBER(roslyn_vla_dimension(d));}
#undef MEMBER
 for(a_scope_ptr s=p->scopes;s;s=s->next)roslyn_scope(s);
 int local_static_ordinal=0;for(a_local_static_variable_init_ptr i=p->local_static_variable_inits;i;i=i->next)
  DB.put("scope_local_static_initialization",{{"owner_id",id},{"ordinal",local_static_ordinal++},{"initialization_id",roslyn_local_static_initialization(i)}});
 int constructor_ordinal=0;for(a_constructor_init_ptr i=p->kind==sck_function?p->variant.routine.constructor_inits:nullptr;i;i=i->next){
  DB.put("scope_constructor_initializer",{{"owner_id",id},{"ordinal",constructor_ordinal},{"initializer_id",roslyn_constructor_initializer(i)}});
#if !DO_IL_LOWERING
  if(p->kind==sck_function&&p->assoc_block){Rid body=roslyn_statement(p->assoc_block,roslyn_default_role);roslyn_child(body,"initialization",constructor_ordinal,roslyn_init(i->initializer,roslyn_default_role));roslyn_child(body,"operand",constructor_ordinal,roslyn_expr(i->source_expr,roslyn_default_role));}
#endif
  ++constructor_ordinal;
 }

 int init_ordinal=0;for(a_dynamic_init_ptr i=p->dynamic_inits;i;i=i->next)DB.put("scope_initialization",{{"owner_id",id},{"ordinal",init_ordinal++},{"initialization_id",roslyn_init(i)}});
 if(p->assoc_block)roslyn_statement(p->assoc_block);
 roslyn_scope_expression_references(p,id);
 roslyn_source_sequences(p,id);
#if MICROSOFT_EXTENSIONS_ALLOWED
 roslyn_ms_attributes(id,p->ms_attributes);
#endif
 roslyn_default_role=saved_role;roslyn_current_routine=saved_routine;return id;
}
static Rid roslyn_namespace(a_namespace_ptr p) {
 bool fresh;Rid id=DB.claim(p,"namespace",fresh);if(!fresh)return id;roslyn_symbol(id,"namespace",p->source_corresp,0);
 DB.put("namespace_symbol",{{"id",id},{"scope_id",R(p->is_namespace_alias?0:roslyn_scope(p->variant.assoc_scope))},{"alias_target_id",R(p->is_namespace_alias?roslyn_namespace(p->variant.assoc_namespace):0)},{"is_inline",static_cast<int>(p->is_inline)},{"is_anonymous",static_cast<int>(p->source_corresp.name==nullptr)}});return id;
}
#include "roslyn_routines.h"
#include "roslyn_variables.h"
#include "roslyn_layout.h"
static Rid roslyn_label(a_label_ptr p) {
 bool fresh;Rid id=DB.claim(p,"label",fresh);if(!fresh)return id;roslyn_symbol(id,"label",p->source_corresp,0);
 DB.put("label_symbol",{{"id",id},{"statement_id",R(roslyn_statement(p->exec_stmt))},{"lifetime_id",R(p->exec_stmt&&p->exec_stmt->kind==stmk_label?roslyn_lifetime(p->exec_stmt->variant.label.lifetime):0)},{"is_break",static_cast<int>(p->break_label)},{"is_continue",static_cast<int>(p->continue_label)}});return id;
}
static Rid roslyn_type_declaration(a_type_ptr p,Rid type) {
 if(p->kind==tk_template_param){auto parameter=roslyn_parameter_placeholders.find(p);return parameter==roslyn_parameter_placeholders.end()||parameter->second.size()!=1?0:*parameter->second.begin();}
 bool declared=p->source_corresp.name||p->kind==tk_class||p->kind==tk_struct||p->kind==tk_union||(p->kind==tk_integer&&p->variant.integer.enum_type);
 if(!declared)return 0;
 bool fresh;Rid symbol=DB.claim(p,"type_symbol",fresh);if(!fresh)return symbol;
 bool enumeration=p->kind==tk_integer&&p->variant.integer.enum_type,alias=p->kind==tk_typeref&&typeref_is_typedef(p);
 roslyn_symbol(symbol,enumeration?"enum":alias?"alias":"type_symbol",p->source_corresp,type,(p->kind==tk_class||p->kind==tk_struct||p->kind==tk_union)?LF(p->variant.class_struct_union.extra_info,compiler_generated):0,0,false);
 DB.put("type_symbol",{{"id",symbol},{"type_id",type}});
 if(alias)DB.put("alias_symbol",{{"id",symbol},{"underlying_type_id",roslyn_type(p->variant.typeref.type)}});
 if(enumeration){auto &e=p->variant.integer;auto supplement=e.extra_info;Rid underlying=0;
 // Audited stock intrinsic: complete real enums only, after frontend wrapup.
 // EDG may lazily allocate its canonical builtin descriptor; no source symbol
 // or declaration is manufactured for that unnamed builtin.
 if(!is_incomplete_type(p)&&!roslyn_type_is_dependent(p)&&!e.is_nonreal){a_type_ptr effective=apply_type_transforming_intrinsic(p,trk_is_underlying_type,nullptr,FALSE);if(!effective||effective->kind==tk_error)DB.fail("Stock enum underlying-type query failed");else underlying=roslyn_type(effective);}
 std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",symbol},{"underlying_type_id",R(underlying)},{"explicit_underlying_type_id",R(supplement?roslyn_type(supplement->base_type):0)},{"is_scoped",static_cast<int>(e.is_scoped_enum)},{"has_explicit_base",static_cast<int>(e.has_explicit_enum_base)},{"originally_unnamed",static_cast<int>(e.originally_unnamed)},{"is_template_enum",static_cast<int>(e.is_template_enum)},{"is_prototype_instantiation",static_cast<int>(e.is_prototype_instantiation)},{"is_nonreal",static_cast<int>(e.is_nonreal)},{"is_specialized",static_cast<int>(e.is_specialized)}};
#if GNU_EXTENSIONS_ALLOWED
 values.push_back({"is_packed",static_cast<int>(e.packed)});values.push_back({"underlying_type_should_use_unsigned",static_cast<int>(supplement&&supplement->underlying_type_should_use_unsigned)});
#endif
#if GNU_EXTENSIONS_ALLOWED && GNU_VISIBILITY_ATTRIBUTE_ALLOWED
 values.push_back({"elf_visibility_id",roslyn_kind_edg_elf_visibility(static_cast<an_ELF_visibility_kind>(e.ELF_visibility))});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 values.push_back({"is_ms_instantiated_nonreal_enum",static_cast<int>(e.is_ms_instantiated_nonreal_enum)});values.push_back({"uuid_string_id",R(supplement?DB.atom(supplement->uuid_string):0)});
#if DO_IL_LOWERING
 values.push_back({"uuid_variable_id",R(supplement?roslyn_variable(supplement->uuid_variable):0)});
#endif
#endif
 if(supplement){values.push_back({"enumerator_list_seen",static_cast<int>(supplement->enumerator_list_seen)});values.push_back({"enumerator_list_complete",static_cast<int>(supplement->enumerator_list_complete)});values.push_back({"has_nodiscard",static_cast<int>(supplement->has_nodiscard_attribute)});values.push_back({"base_position_id",R(e.has_explicit_enum_base?roslyn_position(supplement->base_type_position):0)});values.push_back({"template_id",R(roslyn_template(supplement->assoc_template))});}
 DB.put_fields("enum_symbol",values);
 a_constant_ptr first=e.is_scoped_enum?(e.enum_info.assoc_scope?e.enum_info.assoc_scope->constants:nullptr):e.enum_info.constant_list;int n=0;for(a_constant_ptr c=first;c;c=c->next){Rid constant=roslyn_constant(c);bool ef;Rid enumerator=DB.claim(c,"enumerator_symbol",ef);if(ef)roslyn_symbol(enumerator,"enumerator_symbol",c->source_corresp,roslyn_type(c->type));DB.put("enumerator",{{"owner_id",symbol},{"ordinal",n++},{"symbol_id",enumerator},{"constant_id",constant}});}}

 return symbol;
}
#include "roslyn_types.h"
#include "roslyn_values.h"
#include "roslyn_constants.h"
static void roslyn_initialization_children(Rid owner,a_constant_ptr p,int role,int &ordinal,std::set<a_constant_ptr> &seen) {
 if(!p||!seen.insert(p).second)return;
 if(p->kind==ck_dynamic_init)roslyn_child(owner,"initialization",ordinal++,roslyn_init(p->variant.dynamic_init.ptr,role));
 else if(p->kind==ck_aggregate)for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next)roslyn_initialization_children(owner,c,role,ordinal,seen);
 else if(p->kind==ck_init_repeat)roslyn_initialization_children(owner,p->variant.init_repeat.constant,role,ordinal,seen);
}
static Rid roslyn_init(a_dynamic_init_ptr p,int role) {
 if(!DO_IL_LOWERING&&role==7001)role=7003;bool fresh;Rid id=DB.claim(p,"initialization",fresh);if(!fresh){DB.promote_execution(id,role);return id;}role=DB.semantic_role(id,role);int saved_role=roslyn_default_role;roslyn_default_role=role;
 DB.put("semantic_node",{{"id",id},{"kind_id",roslyn_kind_dynamic_init(p->kind)},{"compiler_generated",0},{"representation_role_id",role}});
 Rid expr=0,constant=0,ctor=0,source=0;int frees_exception_object=0;
#if DO_IL_LOWERING && GENERATE_EH_TABLES
 frees_exception_object=static_cast<int>(p->is_freeing_of_exception_object);
#endif
 switch(p->kind){case dik_none:case dik_zero:break;case dik_constant:case dik_nonconstant_aggregate:case dik_lambda:constant=roslyn_constant(p->variant.constant.ptr);break;case dik_expression:case dik_class_result_via_ctor:expr=roslyn_expr(p->variant.expression,role);break;case dik_constructor:ctor=roslyn_routine(p->variant.constructor.ptr);break;case dik_bitwise_copy:source=roslyn_expr(p->variant.bitwise_copy.source,role);break;default:DB.fail("Initialization payload adapter missing");break;}
 DB.put("initialization",{{"id",id},{"kind_id",roslyn_kind_dynamic_init(p->kind)},{"target_id",R(roslyn_variable(p->variable))},{"type_id",R(p->variable?roslyn_type(p->variable->type):0)},{"lambda_id",R(p->kind==dik_lambda?roslyn_lambda(p->variant.constant.lambda):0)},{"expression_id",R(expr)},{"constant_id",R(constant)},{"constructor_id",R(ctor)},{"destructor_id",R(roslyn_routine(p->destructor))},{"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"init_expression_lifetime_id",R(roslyn_lifetime(p->init_expr_lifetime))},{"master_id",R(roslyn_init(p->master_entry,role))},{"next_destruction_id",R(roslyn_init(p->next_in_destruction_list,role))},{"overlapping_lifetime_id",R(roslyn_lifetime(p->lifetime_of_overlapping_temps))},{"source_id",R(source)},{"is_unordered",static_cast<int>(p->unordered)},{"is_conditional",static_cast<int>(p->inside_conditional_expression)},{"is_overlapping",static_cast<int>(p->overlaps_temps_in_inner_lifetime)},{"is_static_temporary",static_cast<int>(p->static_temp)},{"is_partial_aggregate",static_cast<int>(p->destruction_is_for_partially_constructed_aggregate)},{"is_partial_initialization",static_cast<int>(p->is_partially_initialized)},{"frees_storage",static_cast<int>(p->is_freeing_of_storage_on_exception)},{"is_freeing_of_exception_object",frees_exception_object},{"frees_array",static_cast<int>(p->is_array_freeing)},{"is_explicit_cast",static_cast<int>(p->is_explicit_cast)},{"is_braced",static_cast<int>(p->is_braced_initializer)},{"is_compound_literal",static_cast<int>(p->is_compound_literal)},{"follows_executable_statement",static_cast<int>(p->follows_an_exec_statement)},{"has_temporary_lifetime",static_cast<int>(p->has_temporary_lifetime)},{"is_constructor_initialization",static_cast<int>(p->is_constructor_init)},{"is_conditional_guard",LF(p,is_guard_var_for_local_static_var_init)},{"is_vla_deallocation",LF(p,is_vla_deallocation)},{"creates_initializer_list",static_cast<int>(p->is_creation_of_initializer_list_object)},{"is_initializer_list_array",static_cast<int>(p->is_array_for_initializer_list_object)},{"is_reused_value",static_cast<int>(p->is_reused_value)},{"is_question_result",static_cast<int>(p->is_result_for_class_rvalue_question_mark)},{"is_comma_result",static_cast<int>(p->is_result_for_comma_operator)},{"is_class_result_master",static_cast<int>(p->class_rvalue_initialized_through_master_entry)},{"constructor_copy_implied_source",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_copy_constructor_with_implied_source)},{"constructor_implicit_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_implicit_copy_for_copy_initialization)},{"constructor_value_initialization",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.value_initialization)},{"constructor_sequenced_arguments",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.has_sequenced_arguments)},{"constructor_array_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_array_copy)},{"constant_nonconstant",static_cast<int>((p->kind==dik_constant||p->kind==dik_nonconstant_aggregate||p->kind==dik_lambda)&&p->variant.constant.non_constant)}});
 roslyn_child(id,"operand",0,expr);if(source)roslyn_child(id,"operand",expr?1:0,source);
 if(p->kind==dik_constant||p->kind==dik_nonconstant_aggregate||p->kind==dik_lambda){int ordinal=0;std::set<a_constant_ptr> seen;roslyn_initialization_children(id,p->variant.constant.ptr,role,ordinal,seen);}
 if(p->kind==dik_constructor){int n=0;for(an_expr_node_ptr a=p->variant.constructor.args;a;a=a->next){Rid arg=roslyn_expr(a,role);DB.put("initialization_operand",{{"owner_id",id},{"role_id",K("role","argument")},{"ordinal",n},{"expression_id",arg}});roslyn_child(id,"argument",n++,arg);}}
 roslyn_default_role=saved_role;return id;
}
#include "roslyn_lifetimes.h"

#if DO_IL_LOWERING && GENERATE_EH_TABLES && !DO_FULL_PORTABLE_EH_LOWERING
static std::vector<a_constant_ptr> roslyn_elements(a_constant_ptr p) {std::vector<a_constant_ptr> r;if(p&&p->kind==ck_aggregate)for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next)r.push_back(c);return r;}
static std::string roslyn_integer(a_constant_ptr c) {return c?std::string(decimal_str_for_integer_constant(c).as_temp_characters()):"0";}
static long long roslyn_small_integer(a_constant_ptr c) {return std::strtoll(roslyn_integer(c).c_str(),nullptr,10);}
static Rid roslyn_cleanup_location(a_constant_ptr handle,bool indirect=false,bool receiver=false,bool thrown_object=false,bool no_object=false) {
 Rid id=DB.fresh("cleanup_storage");Rid variable=0,base=0;std::string offset="0";const char *kind=indirect?"indirect_variable":"direct_variable";
 if(receiver){kind="receiver_relative";offset=roslyn_integer(handle);}
 else if(handle&&handle->kind==ck_stack_offset){variable=roslyn_variable(handle->variant.stack_offset.variable);offset=Q(handle->variant.stack_offset.offset);}
 else if(handle&&handle->kind==ck_address){kind="static_address";auto &a=handle->variant.address;if(a.kind==abk_variable)variable=roslyn_variable(a.variant.variable);else if(a.kind==abk_constant||a.kind==abk_temporary)base=roslyn_constant(a.variant.constant);else DB.fail("EH static locator unsupported base");offset=Q(a.offset);}
 else if(handle&&handle->kind==ck_integer){offset=roslyn_integer(handle);if(thrown_object)kind="thrown_object_relative";else if(no_object&&offset=="0")kind="no_object";else DB.fail("EH integer handle has no documented object base");}
 else DB.fail("EH symbolic locator unsupported");
 DB.put("cleanup_storage_location",{{"id",id},{"kind_id",K("storage_location_kind",kind)},{"variable_id",R(variable)},{"base_entity_id",R(base)},{"offset",offset},{"is_indirect",static_cast<int>(indirect)},{"uses_this_parameter_offset",static_cast<int>(receiver)}});return id;
}
static Rid roslyn_eh(an_eh_prologue_supplement_ptr p,Rid prologue) {
 auto old=roslyn_function_eh.find(p->routine);if(old!=roslyn_function_eh.end())return old->second;
 Rid eh=DB.fresh("function_eh");roslyn_function_eh[p->routine]=eh;roslyn_current_eh=eh;
 if(p->region_table)roslyn_projected_tables[p->region_table]=eh;if(p->array_table)roslyn_projected_tables[p->array_table]=eh;
 auto initializer=[](a_variable_ptr v)->a_constant_ptr {if(!v)return nullptr;if(v->init_kind==initk_static)return v->initializer.constant;auto f=roslyn_local_constants.find(v);return f==roslyn_local_constants.end()?nullptr:f->second;};
 std::vector<a_constant_ptr> rows=roslyn_elements(initializer(p->region_table));std::vector<a_constant_ptr> arrays=roslyn_elements(initializer(p->array_table));std::vector<Rid> array_ids;std::vector<a_constant_ptr> array_handles;
 for(size_t i=0;i<arrays.size();++i){auto a=roslyn_elements(arrays[i]);if(a.size()!=3){DB.fail("EDG EH array row shape");break;}Rid aid=DB.fresh("cleanup_array");array_ids.push_back(aid);array_handles.push_back(a[0]);DB.put("cleanup_array",{{"id",aid},{"function_eh_id",eh},{"ordinal",static_cast<sqlite3_int64>(i)},{"handle_constant_id",roslyn_constant(a[0])},{"element_size",roslyn_integer(a[1])},{"element_count",roslyn_integer(a[2])},{"is_nonarray_size",static_cast<int>(roslyn_small_integer(a[2])==0)}});}
 for(size_t i=0;i<rows.size();++i)roslyn_regions[std::make_pair(eh,static_cast<unsigned long>(i))]=DB.fresh("cleanup_region");
 std::map<size_t,Rid> supplement_owner;
 for(size_t i=0;i<rows.size();++i){auto row=roslyn_elements(rows[i]);if(row.size()!=4){DB.fail("EDG EH region row shape");break;}Rid region=roslyn_regions[{eh,static_cast<unsigned long>(i)}];std::string next_value=roslyn_integer(row[2]);unsigned long next=next_value==Q(null_eh_region_number)?null_eh_region_number:std::strtoul(next_value.c_str(),nullptr,10);unsigned flags=static_cast<unsigned>(roslyn_small_integer(row[3]));auto next_it=roslyn_regions.find({eh,next});bool supplement=supplement_owner.count(i)!=0;
 a_routine_ptr action_routine=nullptr;if(!supplement&&row[0]->kind==ck_address&&row[0]->variant.address.kind==abk_routine)action_routine=row[0]->variant.address.variant.routine;
 RoslynSqlValue delegated;
#if IA64_ABI
 if(action_routine&&special_kind_is(action_routine,sfk_destructor))delegated=RoslynSqlValue(static_cast<int>(action_routine->ctor_dtor_kind==cdk_delegation));
#endif
 Rid cond=0,abi=0;size_t extra=i+1;
 if(!supplement&&(flags&2)){auto s=extra<rows.size()?roslyn_elements(rows[extra]):std::vector<a_constant_ptr>();if(s.size()!=4){DB.fail("EDG EH condition supplement shape");break;}supplement_owner[extra++]=region;cond=DB.fresh("cleanup_condition");DB.put("cleanup_condition",{{"id",cond},{"storage_id",roslyn_cleanup_location(s[1],(roslyn_small_integer(s[3])&1)!=0,(roslyn_small_integer(s[3])&16)!=0)},{"kind_id",K("condition_kind","constructed")},{"comparison_value","0"}});}
 if(!supplement&&((!(flags&8)&&(flags&64)&&(flags&32))||((flags&8)&&(flags&64)))){auto s=extra<rows.size()?roslyn_elements(rows[extra]):std::vector<a_constant_ptr>();if(s.size()!=4){DB.fail("EDG EH ABI supplement shape");break;}supplement_owner[extra++]=region;abi=DB.fresh("cleanup_abi");unsigned supplement_flags=static_cast<unsigned>(roslyn_small_integer(s[3]));Rid locator=roslyn_cleanup_location(s[1],(supplement_flags&1)!=0,(supplement_flags&16)!=0);DB.put("cleanup_abi_detail",{{"id",abi},{"vla_count_location_id",R((flags&8)?locator:0)},{"vtt_location_id",R(!(flags&8)?locator:0)},{"is_delegating",delegated}});}
 DB.put("cleanup_region",{{"id",region},{"function_eh_id",eh},{"ordinal",static_cast<sqlite3_int64>(i)},{"next_region_id",R(next_it==roslyn_regions.end()?0:next_it->second)},{"condition_id",R(cond)},{"is_supplement",static_cast<int>(supplement)},{"supplement_owner_id",R(supplement?supplement_owner[i]:0)}});
 if(supplement)continue;
 Rid routine=0;if(row[0]->kind==ck_address&&row[0]->variant.address.kind==abk_routine)routine=roslyn_routine(row[0]->variant.address.variant.routine);else if(row[0]->kind!=ck_integer)DB.fail("EDG EH action routine shape");
 // Stock null-variable object cleanup is based on the runtime thrown object.
 // The distinct clear_handle release action is an actual void() routine;
 // cfront destructor wrappers can be ordinary routines taking an object.
 bool thrown_receiver=false,no_object_receiver=false;
 a_type_ptr action_type=action_routine?skip_typerefs(action_routine->type):nullptr;
 if(action_type&&action_type->kind==tk_routine){auto signature=action_type->variant.routine.extra_info;
  thrown_receiver=signature->param_type_list||signature->has_this_param||signature->this_class;
  no_object_receiver=!thrown_receiver&&!signature->has_ellipsis&&skip_typerefs(action_type->variant.routine.return_type)->kind==tk_void&&(flags&(1|8|16))==0;
 }
 Rid arr=0,storage=0;if(flags&8){long long index=roslyn_small_integer(row[1]);if(index<0||static_cast<size_t>(index)>=array_ids.size())DB.fail("EDG EH array index");else {arr=array_ids[static_cast<size_t>(index)];storage=roslyn_cleanup_location(array_handles[static_cast<size_t>(index)],(flags&1)!=0,(flags&16)!=0,thrown_receiver,no_object_receiver);}}else storage=roslyn_cleanup_location(row[1],(flags&1)!=0,(flags&16)!=0,thrown_receiver,no_object_receiver);
 Rid action=DB.fresh("cleanup_action");DB.put("cleanup_action",{{"id",action},{"region_id",region},{"ordinal",0},{"kind_id",K("cleanup_kind",(flags&128)?"guard":(flags&4)?"deallocation":routine?"destructor":"special")},{"routine_id",R(routine)},{"storage_id",R(storage)},{"array_id",R(arr)},{"abi_detail_id",R(abi)},{"is_array",static_cast<int>((flags&8)!=0)},{"is_indirect",static_cast<int>((flags&1)!=0)},{"is_conditional",static_cast<int>((flags&2)!=0)},{"is_base_subobject",static_cast<int>(!(flags&8)&&(flags&64))},{"is_vla",static_cast<int>((flags&8)&&(flags&64))},{"is_deallocation",static_cast<int>((flags&4)!=0)},{"is_local_static_guard",static_cast<int>((flags&128)!=0)}});
 }
 DB.put("function_eh",{{"id",eh},{"routine_id",roslyn_routine(p->routine)},{"prologue_id",prologue},{"region_table_id",R(roslyn_variable(p->region_table))},{"array_table_id",R(roslyn_variable(p->array_table))},{"initial_state",Q(null_eh_region_number)},{"null_state",Q(null_eh_region_number)}});
 if(p->region_table)DB.put("eh_table_projection",{{"owner_id",roslyn_variable(p->region_table)},{"kind_id",K("role","cleanup")},{"function_eh_id",eh}});
 if(p->array_table)DB.put("eh_table_projection",{{"owner_id",roslyn_variable(p->array_table)},{"kind_id",K("role","element")},{"function_eh_id",eh}});return eh;
}
#endif
static void roslyn_expr_operands(Rid owner,an_expr_node_ptr list,int role,bool evaluated=true) {int n=0;for(an_expr_node_ptr a=list;a;a=a->next){Rid child=roslyn_expr(a,role);DB.put("expression_operand",{{"owner_id",owner},{"role_id",K("role","operand")},{"ordinal",n},{"expression_id",child}});if(evaluated)roslyn_child(owner,"operand",n,child);else roslyn_reference(owner,"metadata",n,child);++n;}}
#include "roslyn_expressions_extra.h"
static Rid roslyn_vla_dimension(a_vla_dimension_ptr p) {
 bool fresh;Rid id=DB.claim(p,"vla_dimension",fresh);if(!fresh)return id;
 Rid variable=0;
#if DO_IL_LOWERING && !LOWER_VARIABLE_LENGTH_ARRAYS
 variable=roslyn_variable(p->dimension_variable);
#endif
 DB.put("vla_dimension",{{"id",id},{"type_id",R(roslyn_type(p->type))},{"expression_id",R(roslyn_expr(p->dimension_expr,7002))},{"original_dimension_id",R(roslyn_vla_dimension(p->original_dimension))},{"dimension_variable_id",R(variable)},{"in_prototype_scope",static_cast<int>(p->in_prototype_scope)},{"has_size_statement",static_cast<int>(p->has_size_statement)},{"source_position_id",R(roslyn_position(p->position))}});
 return id;
}
static void roslyn_coroutine(a_coroutine_descr_ptr p,Rid id,int role) {
 if(!p){DB.fail("Coroutine statement has no retained descriptor");return;}
 if(p->error_descr){DB.fail("Invalid coroutine descriptor reached successful export");return;}
 Rid initial=roslyn_expr(p->initial_suspend_call,role),final=roslyn_expr(p->final_suspend_call,role),exception=roslyn_expr(p->unhandled_exception_call,role),result=roslyn_expr(p->get_return_object_call,role),failure=roslyn_expr(p->alloc_failure_gro_call,role);
 DB.put("coroutine",{{"id",id},{"routine_id",roslyn_routine(roslyn_current_routine)},{"traits_type_id",R(roslyn_type(p->traits))},{"handle_id",R(roslyn_variable(p->handle))},{"promise_id",R(roslyn_variable(p->promise))},{"initial_await_resume_id",R(roslyn_variable(p->init_await_resume))},{"this_parameter_copy_id",R(roslyn_variable(p->this_param_copy))},{"final_suspend_label_id",R(roslyn_label(p->final_suspend_label))},{"initial_suspend_id",R(initial)},{"final_suspend_id",R(final)},{"exception_id",R(exception)},{"return_object_id",R(result)},{"allocation_failure_call_id",R(failure)},{"allocator_id",R(roslyn_routine(p->new_routine))},{"deallocator_id",R(roslyn_routine(p->delete_routine))},{"source_position_id",R(roslyn_position(p->position))},{"has_return_void",static_cast<int>(p->has_return_void)},{"body_generated",static_cast<int>(p->body_generated)},{"error_descriptor",static_cast<int>(p->error_descr)}});
 int ordinal=0;for(a_variable_ptr v=p->parameter_copies;v;v=v->next,++ordinal)if(v->is_parameter&&v!=p->this_param_copy)DB.put("coroutine_parameter_copy",{{"owner_id",id},{"ordinal",ordinal},{"copy_id",roslyn_variable(v)},{"initialization_id",R(v->init_kind==initk_dynamic?roslyn_init(v->initializer.dynamic,7002):0)}});
 roslyn_child(id,"initialization",0,initial);roslyn_child(id,"result",0,result);roslyn_child(id,"cleanup",0,final);roslyn_child(id,"else",0,exception);roslyn_child(id,"else",1,failure);
}
static Rid roslyn_asm(an_asm_entry_ptr p) {
 bool fresh;Rid id=DB.claim(p,"asm_entry",fresh);if(!fresh)return id;
 int is_volatile=0,is_goto=0,gnu_form=0,has_volatile=0,has_inline=0;RoslynSqlValue count;
#if GNU_EXTENSIONS_ALLOWED
 is_volatile=p->is_volatile;is_goto=p->is_asm_goto;gnu_form=p->gnu_asm_form;has_volatile=p->has_volatile_keyword;has_inline=p->has_inline_keyword;
#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
 count=Q(p->number_of_constraints);
#endif
#endif
 DB.put("asm_entry",{{"id",id},{"asm_string_id",roslyn_constant(p->asm_string)},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position))},{"gnu_asm_form",gnu_form},{"is_volatile",is_volatile},{"has_volatile_keyword",has_volatile},{"has_inline_keyword",has_inline},{"is_goto",is_goto},{"number_of_constraints",count}});roslyn_attributes(id,p->source_corresp.attributes);
#if GNU_EXTENSIONS_ALLOWED
 int ordinal=0;for(an_asm_operand_ptr a=p->operands;a;a=a->next){
 RoslynSqlValue constraint,modifier;const char *kind;
#if RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
 constraint=R(DB.atom(a->constraints_string));kind=a->is_output_operand?"output":"input";
#else
 modifier=roslyn_kind_edg_asm_modifier(a->modifiers);kind=a->modifiers==aom_input?"input":a->modifiers==aom_modify?"inout":"output";
 int n=0;for(an_asm_operand_constraint_ptr c=a->constraints;c;c=c->next){Rid condition=0;
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
 if(c->kind==aoc_cc)condition=DB.atom(c->cond_code);
#endif
 DB.put("asm_operand_constraint",{{"owner_id",id},{"operand_ordinal",ordinal},{"ordinal",n++},{"kind_id",roslyn_kind_edg_asm_constraint(c->kind)},{"condition_code_id",R(condition)}});}
#endif
 Rid expression=roslyn_expr(a->expression,7002);DB.put("asm_operand",{{"owner_id",id},{"ordinal",ordinal},{"kind_id",K("asm_operand_kind",kind)},{"name_id",R(DB.atom(a->name))},{"constraint_id",constraint},{"modifier_kind_id",modifier},{"expression_id",expression},{"source_position_id",R(roslyn_position(a->position))}});++ordinal;
 }
 ordinal=0;for(a_named_register_list_ptr c=p->clobbers;c;c=c->next)DB.put("asm_clobber",{{"owner_id",id},{"ordinal",ordinal++},{"register_kind_id",roslyn_kind_edg_named_register(c->reg)}});
 ordinal=0;for(a_label_list_ptr l=p->labels;l;l=l->next)DB.put("asm_label",{{"owner_id",id},{"ordinal",ordinal++},{"label_id",roslyn_label(l->label)}});
#endif
 return id;
}
static Rid roslyn_expr(an_expr_node_ptr p,int role) {
 if(!DO_IL_LOWERING&&role==7001)role=7003;bool fresh;Rid id=DB.claim(p,"expression",fresh);if(!fresh){DB.promote_execution(id,role);return id;}role=DB.semantic_role(id,role);int saved_role=roslyn_default_role;roslyn_default_role=role;
 if(p->kind==enk_error||p->kind==enk_reclaimed||p->kind==enk_last){DB.fail("Invalid EDG expression reached");roslyn_default_role=saved_role;return id;}
 int kind=roslyn_kind_expression(p->kind);DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(roslyn_origin(p->position,&p->expr_range.end,&p->expr_range.start))},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 DB.put("expression",{{"id",id},{"type_id",roslyn_type(p->type)},{"value_category_id",K("value_category",p->is_lvalue?"lvalue":p->is_xvalue?"xvalue":"prvalue")},{"original_lvalue_type_id",R(roslyn_type(p->orig_lvalue_type))},{"result_unused",static_cast<int>(p->result_is_not_used)},{"initialization_guard",static_cast<int>(p->is_initialization_guard)},{"generated_default_argument",static_cast<int>(p->generated_default_arg)},{"volatile_fetch",static_cast<int>(p->volatile_fetch)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},{"is_parenthesized",static_cast<int>(p->is_parenthesized)},{"is_static_cast",static_cast<int>(p->is_static_cast)},{"is_functional_cast",static_cast<int>(p->is_functional_notation_cast)},{"is_brace_cast",static_cast<int>(p->is_brace_notation_cast)},{"is_objectless_member_reference",static_cast<int>(p->is_objectless_nonstatic_data_mem_ref)},{"is_gnu_extension",static_cast<int>(p->marked_as_gnu_extension)},{"type_definition_needed",static_cast<int>(p->type_definition_needed)},{"boolean_not_normalized",LF(p,is_non_normalized_boolean_controlling_expr)},{"is_safe_cast",static_cast<int>(p->is_safe_cast)}});
 switch(p->kind){
 case enk_operation:{auto &o=p->variant.operation;Rid property_kind=0;
#if MICROSOFT_EXTENSIONS_ALLOWED
 property_kind=roslyn_kind_rewritten_property(static_cast<a_rewritten_property_reference_kind>(o.rewritten_property_reference_kind));
#endif
 DB.put("operator_expression",{{"id",id},{"operator_kind_id",roslyn_kind_operator(o.kind)},{"operation_type_kind_id",roslyn_kind_type(o.type_kind)},{"eval_left_to_right",static_cast<int>(o.eval_left_to_right)},{"eval_right_to_left",static_cast<int>(o.eval_right_to_left)},{"pointer_operand_is_second",static_cast<int>(o.pointer_operand_is_second)},{"is_reinterpret_like_cast",static_cast<int>(o.is_reinterpret_like_cast)},{"requires_runtime_cast_check",static_cast<int>(o.requires_runtime_cast_check)},{"returns_lvalue",static_cast<int>(o.returns_lvalue_instead_of_usual_rvalue)},{"is_reinterpret_cast",static_cast<int>(o.is_reinterpret_cast)},{"is_reference_cast",static_cast<int>(o.is_reference_cast)},{"is_rvalue_reference_cast",static_cast<int>(o.is_rvalue_reference_cast)},{"is_const_cast",static_cast<int>(o.is_const_cast)},{"is_tracking_reference_cast",static_cast<int>(o.is_tracking_reference_cast)},{"is_gnu_two_operand_conditional",static_cast<int>(o.is_gnu_two_operand_question_mark)},{"call_uses_operator_syntax",static_cast<int>(o.call_uses_operator_syntax)},{"implicit_member_naming",static_cast<int>(o.implicit_in_member_naming)},{"implicit_explicit_cast_step",static_cast<int>(o.implicit_step_of_explicit_cast)},{"is_conversion_call",static_cast<int>(o.is_conversion_call)},{"argument_dependent_lookup_suppressed",static_cast<int>(o.arg_dependent_lookup_suppressed_on_call)},{"qualified_function_name",static_cast<int>(o.call_with_qualified_function_name)},{"retained_virtual_call",static_cast<int>(o.is_virtual_call)},{"rewritten_property_kind_id",R(property_kind)}});roslyn_expr_operands(id,o.operands,role);
 if(o.kind==eok_call||o.kind==eok_dot_member_call||o.kind==eok_points_to_member_call||o.kind==eok_dot_pm_call||o.kind==eok_points_to_pm_call){
 an_expr_node_ptr callee=o.operands,target_expression=callee;
 if(callee&&callee->kind==enk_operation&&(callee->variant.operation.kind==eok_dot_static||callee->variant.operation.kind==eok_points_to_static)){auto selector=callee->variant.operation.operands;target_expression=selector?selector->next:nullptr;}
 a_routine_ptr selected=target_expression&&target_expression->kind==enk_routine?target_expression->variant.routine.ptr:nullptr;
 Rid target=roslyn_routine(selected);a_type_ptr signature=selected?skip_typerefs(selected->type):roslyn_call_type(callee);
 a_param_type_ptr formal=signature?signature->variant.routine.extra_info->param_type_list:nullptr;Rid receiver=0,returned=0;
 bool member=o.kind==eok_dot_member_call||o.kind==eok_points_to_member_call;
 bool member_pointer=o.kind==eok_dot_pm_call||o.kind==eok_points_to_pm_call;
 an_expr_node_ptr arguments=callee?callee->next:nullptr;if(member||member_pointer){receiver=roslyn_expr(arguments,role);arguments=arguments?arguments->next:nullptr;}
 int n=0;for(an_expr_node_ptr a=arguments;a;a=a->next){const char *argrole=roslyn_parameter_role(signature,formal,selected);Rid aid=roslyn_expr(a,role);
 if(std::strcmp(argrole,"this_parameter")==0)receiver=aid;if(std::strcmp(argrole,"return_parameter")==0)returned=aid;
 DB.put("call_argument",{{"owner_id",id},{"ordinal",n++},{"expression_id",aid},{"role_id",K("role",argrole)}});if(formal)formal=formal->next;}
 DB.put("call_expression",{{"id",id},{"callee_id",R(roslyn_expr(callee,role))},{"target_routine_id",R(target)},{"function_type_id",R(roslyn_type(signature))},{"receiver_id",R(receiver)},{"return_slot_id",R(returned)},{"source_target_routine_id",R(!DO_IL_LOWERING?target:0)},{"source_dispatch_kind_id",R(!DO_IL_LOWERING?K("source_dispatch_kind",member_pointer?"member_pointer":member&&o.is_virtual_call?"virtual":"non_virtual"):0)},{"call_kind_id",K("call_kind",member_pointer?"member_pointer":member?"member":target?"direct":"indirect")}});
 roslyn_fact(id,"source_target",DO_IL_LOWERING?"lost":target?"available":"not_applicable",DO_IL_LOWERING?"Stock lowering rewrites calls; a current callee is not a retained source-selected target.":target?nullptr:"The source call uses an indirect or member-pointer target.");
 roslyn_fact(id,"source_dispatch",DO_IL_LOWERING?"lost":"available",DO_IL_LOWERING?"Stock lowering rewrites member and virtual dispatch; the current call form is retained separately.":nullptr);
 }
 break;}
 case enk_constant:DB.put("constant_expression",{{"id",id},{"constant_id",roslyn_constant(p->variant.constant.ptr)},{"name_reference_id",R(roslyn_name_reference(p->variant.constant.name_reference))}});break;
 case enk_variable:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_variable(p->variant.variable.ptr)},{"reference_kind_id",K("reference_kind","variable")},{"name_reference_id",R(roslyn_name_reference(p->variant.variable.name_reference))}});break;
 case enk_routine:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_routine(p->variant.routine.ptr))},{"reference_kind_id",K("reference_kind","routine")},{"name_reference_id",R(roslyn_name_reference(p->variant.routine.name_reference))}});break;
 case enk_field:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_field(p->variant.field.ptr)},{"reference_kind_id",K("reference_kind","field")},{"name_reference_id",R(roslyn_name_reference(p->variant.field.name_reference))}});break;
 case enk_object_lifetime:{auto &l=p->variant.object_lifetime;Rid ex=roslyn_expr(l.expr,role);DB.put("lifetime_expression",{{"id",id},{"expression_id",R(ex)},{"lifetime_id",R(roslyn_lifetime(l.ptr))}});roslyn_child(id,"body",0,ex);break;}
 case enk_temp_init:DB.put("temporary_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.init.dynamic_init,role)}});roslyn_child(id,"initialization",0,roslyn_init(p->variant.init.dynamic_init,role));break;
 case enk_statement:{Rid body=roslyn_statement(p->variant.statement,role);DB.put("statement_expression",{{"id",id},{"statement_id",body}});roslyn_child(id,"body",0,body);break;}
 case enk_reuse_value:DB.put("reuse_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.reused_value_init,role)}});break;
 case enk_builtin_operation:DB.put("builtin_expression",{{"id",id},{"kind_id",roslyn_kind_builtin(p->variant.builtin_operation.kind)}});roslyn_expr_operands(id,p->variant.builtin_operation.operands,role);break;
 case enk_type_operand:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_type(p->variant.type_operand.type))},{"reference_kind_id",K("reference_kind","type")},{"name_reference_id",R(roslyn_name_reference(p->variant.type_operand.name_reference))}});break;
#if VLA_DEALLOCATIONS_IN_IL
 case enk_vla_dealloc:DB.put("vla_operation",{{"id",id},{"variable_id",roslyn_variable(p->variant.vla_variable)},{"type_id",roslyn_type(p->variant.vla_variable->type)}});break;
#endif
 case enk_param_ref:DB.put("parameter_reference_expression",{{"id",id},{"parameter_number",static_cast<int>(p->variant.param_ref.param_num)},{"parameter_depth",static_cast<int>(p->variant.param_ref.levels_up)},{"is_this",static_cast<int>(p->variant.param_ref.param_num==0)}});break;
 case enk_throw:{auto t=p->variant.throw_info;Rid ex=t?ER(t,expr,roslyn_expr):0;Rid init=t?roslyn_init(t->dynamic_init,DO_IL_LOWERING?7002:role):0;DB.put("throw_expression",{{"id",id},{"operand_id",R(ex)},{"thrown_type_id",R(t?roslyn_type(t->type):0)},{"initialization_id",R(init)},{"destructor_id",R(t?roslyn_routine(t->destructor):0)},{"is_rethrow",static_cast<int>(!t)}});roslyn_child(id,"operand",1,ex);if(!DO_IL_LOWERING)roslyn_child(id,"initialization",0,init);break;}
 case enk_condition:{auto t=p->variant.condition;Rid ex=roslyn_expr(t->expr,role);DB.put("condition_expression",{{"id",id},{"expression_id",R(ex)},{"initialization_id",R(roslyn_statement(t->initialization,role))},{"dynamic_initialization_id",R(roslyn_init(t->dynamic_init,role))},{"variable_id",R(t->dynamic_init?roslyn_variable(t->dynamic_init->variable):0)},{"scope_id",R(roslyn_scope(t->scope))}});roslyn_child(id,"condition",0,ex);roslyn_child(id,"initialization",0,roslyn_statement(t->initialization,role));if(!DO_IL_LOWERING)roslyn_child(id,"initialization",1,roslyn_init(t->dynamic_init,role));break;}
 case enk_sizeof:case enk_alignof:case enk_datasizeof:{auto &q=p->variant.sizeof_info;Rid ex=q.is_type?0:roslyn_expr(q.variant.expr,7002);DB.put("type_query_expression",{{"id",id},{"query_kind_id",kind},{"operand_type_id",R(q.is_type?roslyn_type(q.variant.type):0)},{"operand_expression_id",R(ex)},{"is_standard_alignof",static_cast<int>(q.is_std_alignof)}});roslyn_reference(id,"metadata",0,ex);break;}
 case enk_address_of_ellipsis:break;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
 case enk_result_of_overriding_function:break;
#endif
 #if DO_IL_LOWERING && GENERATE_EH_TABLES && !DO_FULL_PORTABLE_EH_LOWERING
 case enk_lowered_eh_construct:{auto &e=p->variant.lowered_eh;Rid eh=roslyn_current_eh,region=0;std::string state;Rid routine=0,handler=0,try_id=0,te=0,ce=0;
 switch(e.kind){case leck_function_prologue:eh=roslyn_eh(e.variant.prologue_info,id);break;case leck_cleanup_state:case leck_unreachable_cleanup_state:{auto n=e.variant.cleanup_region_number;state=Q(n);auto found=roslyn_regions.find({eh,static_cast<unsigned long>(n)});if(found!=roslyn_regions.end())region=found->second;DB.put("cleanup_state_operation",{{"id",id},{"region_id",R(region)},{"state_number",state},{"is_unreachable",static_cast<int>(e.kind==leck_unreachable_cleanup_state)}});break;}case leck_function_epilogue:routine=roslyn_routine(e.variant.epilogue_routine);break;case leck_caught_object_address:handler=roslyn_handler(e.variant.caught_object_handler);break;case leck_catch_epilogue:handler=roslyn_handler(e.variant.epilogue_handler);break;case leck_try_epilogue:try_id=roslyn_try_descriptor(e.variant.epilogue_try_block);break;case leck_internal_try:te=roslyn_expr(e.variant.try_and_catch_expr,role);ce=roslyn_expr(e.variant.try_and_catch_expr->next,role);roslyn_child(id,"then",0,te);roslyn_child(id,"else",0,ce);break;default:break;}
 DB.put("lowered_eh_operation",{{"id",id},{"kind_id",roslyn_kind_lowered_eh(e.kind)},{"function_eh_id",R(eh)},{"region_id",R(region)},{"state_number",state.empty()?RoslynSqlValue():RoslynSqlValue(state)},{"routine_id",R(routine)},{"handler_id",R(handler)},{"try_block_id",R(try_id)},{"try_expression_id",R(te)},{"catch_expression_id",R(ce)}});break;}
 #endif
 default:if(!roslyn_extra_expression(p,role,id,kind))DB.fail("Expression payload adapter missing: "+Q(p->kind));break;
 }
 roslyn_default_role=saved_role;return id;
}
static Rid roslyn_statement(a_statement_ptr p,int role) {
 if(!DO_IL_LOWERING&&role==7001)role=7003;bool fresh;Rid id=DB.claim(p,"statement",fresh);if(!fresh){DB.promote_execution(id,role);return id;}role=DB.semantic_role(id,role);int saved_role=roslyn_default_role;roslyn_default_role=role;int kind=roslyn_kind_statement(p->kind);
 DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(roslyn_origin(p->position,&p->end_position))},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 roslyn_attributes(id,p->attributes);
 Rid expression=roslyn_expr(p->expr,role);DB.put("statement",{{"id",id},{"kind_id",kind},{"expression_id",R(expression)},{"parent_id",R(roslyn_statement(p->parent,role))},{"is_guard",static_cast<int>(p->is_initialization_guard)},{"lowering_generated",LF(p,lowering_generated)},{"has_associated_pragma",static_cast<int>(p->has_associated_pragma)},{"is_lowering_boilerplate",LF(p,is_lowering_boilerplate)},{"is_fallthrough_statement",static_cast<int>(p->is_fallthrough_statement)},{"is_likely",static_cast<int>(p->is_likely)},{"is_unlikely",static_cast<int>(p->is_unlikely)}});roslyn_child(id,"operand",0,expression);
 switch(p->kind){
 case stmk_expr:case stmk_empty:case stmk_coroutine_return:break;
#if GNU_EXTENSIONS_ALLOWED
 case stmk_assigned_goto:DB.put("jump_statement",{{"id",id},{"kind_id",K("jump_kind","assigned_goto")},{"value_id",R(expression)}});break;
#endif
 case stmk_coroutine:roslyn_coroutine(p->variant.coroutine.descr,id,role);break;
 case stmk_asm:{auto entry=p->variant.asm_entry;DB.put("asm_statement",{{"id",id},{"entry_id",roslyn_asm(entry)}});
#if GNU_EXTENSIONS_ALLOWED
 int ordinal=0;for(an_asm_operand_ptr a=entry->operands;a;a=a->next)roslyn_child(id,"argument",ordinal++,roslyn_expr(a->expression,role));
#endif
 break;}
#if ASM_FUNCTION_ALLOWED
 case stmk_asm_func_body:DB.put("asm_statement",{{"id",id},{"text_id",DB.atom(p->variant.asm_func_body)}});break;
#endif
 case stmk_constexpr_if:{auto s=p->variant.constexpr_if;Rid then=roslyn_statement(s->then_statement,role),els=roslyn_statement(s->else_statement,role);DB.put("if_statement",{{"id",id},{"then_id",R(then)},{"else_id",R(els)},{"condition_id",R(expression)},{"selection_known",static_cast<int>(s->value_known)},{"selected_then",s->value_known?RoslynSqlValue(static_cast<int>(s->value)):RoslynSqlValue()}});roslyn_child(id,"then",0,then);roslyn_child(id,"else",0,els);break;}
 case stmk_range_based_for:{auto &s=p->variant.range_based_for_loop;auto t=s.extra_info;Rid body=roslyn_statement(s.statement,role),init=roslyn_statement(t->initialization,role),test=roslyn_expr(t->ne_call_expr,role),increment=roslyn_expr(t->incr_call_expr,role);DB.put("range_for",{{"id",id},{"body_id",R(body)},{"initialization_id",R(init)},{"range_variable_id",R(roslyn_variable(t->range))},{"begin_variable_id",R(roslyn_variable(t->begin))},{"end_variable_id",R(roslyn_variable(t->end))},{"iterator_variable_id",R(roslyn_variable(t->iterator))},{"comparison_id",R(test)},{"increment_id",R(increment)},{"scope_id",R(roslyn_scope(t->range_based_for_scope))},{"iterator_scope_id",R(roslyn_scope(t->iterator_scope))},{"use_await",static_cast<int>(t->use_await)}});roslyn_child(id,"initialization",0,init);int ordinal=1;for(a_variable_ptr v:{t->range,t->begin,t->end,t->iterator})if(v&&v->init_kind==initk_dynamic)roslyn_child(id,"initialization",ordinal++,roslyn_init(v->initializer.dynamic,role));roslyn_child(id,"body",0,body);roslyn_child(id,"condition",0,test);roslyn_child(id,"increment",0,increment);break;}
 case stmk_set_vla_size:{auto dimension=p->variant.vla_dimension;DB.put("vla_operation",{{"id",id},{"type_id",R(roslyn_type(dimension->type))},{"dimension_id",roslyn_vla_dimension(dimension)}});roslyn_child(id,"operand",1,roslyn_expr(dimension->dimension_expr,role));break;}
 case stmk_vla_decl:{auto &v=p->variant.vla;DB.put("vla_operation",{{"id",id},{"variable_id",R(v.is_typedef_decl?0:roslyn_variable(v.variant.variable))},{"type_id",R(roslyn_type(v.is_typedef_decl?v.variant.typedef_type:v.variant.variable->type))},{"is_typedef_declaration",static_cast<int>(v.is_typedef_decl)}});break;}
 case stmk_stmt_expr_result:{Rid init=roslyn_init(p->variant.stmt_expr_result.dynamic_init,role);if(init)DB.put("initialization_statement",{{"id",id},{"initialization_id",init}});roslyn_child(id,"initialization",0,init);break;}
 case stmk_block:{auto info=p->variant.block.extra_info;DB.put("block_statement",{{"id",id},{"descriptor_id",R(roslyn_block_descriptor(info))}});int n=0;for(a_statement_ptr c=p->variant.block.statements;c;c=c->next){Rid child=roslyn_statement(c,role);DB.put("block_item",{{"owner_id",id},{"ordinal",n},{"statement_id",child}});roslyn_child(id,"statement",n++,child);}break;}
 case stmk_if:case stmk_if_consteval:case stmk_if_not_consteval:{auto &s=p->variant.if_stmt;Rid then=roslyn_statement(s.then_statement,role),els=roslyn_statement(s.else_statement,role);DB.put("if_statement",{{"id",id},{"then_id",R(then)},{"else_id",R(els)},{"condition_id",R(expression)}});roslyn_child(id,"then",0,then);roslyn_child(id,"else",0,els);break;}
 case stmk_while:case stmk_end_test_while:{Rid body=roslyn_statement(p->variant.loop_statement,role);DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"condition_id",R(expression)}});roslyn_child(id,"body",0,body);break;}
 #if UPC_EXTENSIONS_ALLOWED
 case stmk_upc_notify:case stmk_upc_wait:case stmk_upc_barrier:case stmk_upc_fence:break;
 case stmk_upc_forall:
#endif
 case stmk_for:{auto &s=p->variant.for_loop;Rid body=roslyn_statement(s.statement,role),init=s.extra_info?roslyn_statement(s.extra_info->initialization,role):0,inc=s.extra_info?roslyn_expr(s.extra_info->increment,role):0;DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"initialization_id",R(init)},{"condition_id",R(expression)},{"increment_id",R(inc)},{"init_scope_id",R(s.extra_info?roslyn_scope(s.extra_info->for_init_scope):0)}});roslyn_child(id,"initialization",0,init);roslyn_child(id,"body",0,body);roslyn_child(id,"increment",0,inc);break;}
 case stmk_return:case stmk_goto:{Rid init=p->kind==stmk_return?roslyn_init(p->variant.return_dynamic_init,role):0;Rid label=p->kind==stmk_goto?roslyn_label(p->variant.label.ptr):0;DB.put("jump_statement",{{"id",id},{"kind_id",K("jump_kind",p->kind==stmk_return?"return":p->variant.label.ptr->break_label?"break":p->variant.label.ptr->continue_label?"continue":"goto")},{"value_id",R(expression)},{"label_id",R(label)},{"initialization_id",R(init)},{"common_lifetime_id",R(p->kind==stmk_goto?roslyn_lifetime(p->variant.label.lifetime):0)}});roslyn_child(id,"initialization",0,init);break;}
 case stmk_label:DB.put("label_statement",{{"id",id},{"label_id",roslyn_label(p->variant.label.ptr)}});break;
 case stmk_switch:{auto &s=p->variant.switch_stmt;Rid body=roslyn_statement(s.body_statement,role);DB.put("switch_statement",{{"id",id},{"body_id",R(body)},{"selector_id",R(expression)}});roslyn_child(id,"body",0,body);int n=0;for(a_switch_case_entry_ptr c=s.extra_info->cases;c;c=c->next)DB.put("switch_case",{{"owner_id",id},{"ordinal",n++},{"statement_id",roslyn_statement(c->stmt,role)},{"low_constant_id",R(roslyn_constant(c->case_value))},{"high_constant_id",R(roslyn_constant(c->range_end))},{"is_default",static_cast<int>(!c->case_value)}});break;}
 #if MICROSOFT_EXTENSIONS_ALLOWED
 case stmk_for_each:{auto &s=p->variant.for_each_loop;auto t=s.extra_info;if(t->kind!=sfepk_stl_pattern&&t->kind!=sfepk_array_pattern){DB.fail("For-each managed dialect pattern is excluded");break;}
 a_variable_ptr iterator=t->uses_prev_decl_iterator?t->iterator.prev_decl.variable:t->iterator.variable;
 a_field_ptr field=t->uses_prev_decl_iterator?t->iterator.prev_decl.field:nullptr;
 Rid body=roslyn_statement(s.statement,role),assignment=roslyn_expr(t->uses_prev_decl_iterator?t->iterator.prev_decl.assign_expr:nullptr,role),test=roslyn_expr(t->variant.stl_array_pattern.ne_call_expr,role),increment=roslyn_expr(t->variant.stl_array_pattern.incr_call_expr,role);
 DB.put("for_each_statement",{{"id",id},{"body_id",R(body)},{"collection_id",R(expression)},{"pattern_kind_id",roslyn_kind_for_each_pattern(t->kind)},{"iterator_id",R(roslyn_variable(iterator))},{"uses_previous_declaration_iterator",static_cast<int>(t->uses_prev_decl_iterator)},{"previous_field_id",R(roslyn_field(field))},{"assignment_id",R(assignment)},{"collection_reference_id",R(roslyn_variable(t->collection_expr_ref))},{"temporary_variable_id",R(roslyn_variable(t->temporary_variable))},{"end_variable_id",R(roslyn_variable(t->variant.stl_array_pattern.end_variable))},{"test_id",R(test)},{"increment_id",R(increment)},{"scope_id",R(roslyn_scope(t->for_each_scope))},{"iterator_scope_id",R(roslyn_scope(t->iterator_scope))}});
 int ordinal=0;for(a_variable_ptr v:{t->collection_expr_ref,t->temporary_variable,t->variant.stl_array_pattern.end_variable,t->uses_prev_decl_iterator?nullptr:iterator})if(v&&v->init_kind==initk_dynamic)roslyn_child(id,"initialization",ordinal++,roslyn_init(v->initializer.dynamic,role));roslyn_child(id,"body",0,body);roslyn_child(id,"condition",0,test);roslyn_child(id,"increment",0,increment);roslyn_child(id,"operand",1,assignment);break;}
#endif
 case stmk_switch_case:break;
 case stmk_init:{Rid init=roslyn_init(p->variant.dynamic_init,role);DB.put("initialization_statement",{{"id",id},{"initialization_id",init}});roslyn_reference(id,"initialization",0,init);roslyn_child(id,"initialization",0,init);break;}
 case stmk_try_block:{auto supplement=p->variant.try_block;Rid descriptor=roslyn_try_descriptor(supplement),body=roslyn_statement(supplement->statement,role);DB.put("try_statement",{{"id",id},{"descriptor_id",descriptor}});roslyn_child(id,"body",0,body);int ordinal=0;for(a_handler_ptr h=supplement->handlers;h;h=h->next,++ordinal){Rid handler=roslyn_handler(h);if(!DO_IL_LOWERING)roslyn_child(id,"initialization",ordinal,roslyn_init(h->dynamic_init,role));roslyn_reference(id,"member",ordinal,handler);roslyn_child(id,"else",ordinal,roslyn_statement(h->statement,role));}break;}
 case stmk_microsoft_try:{auto s=p->variant.microsoft_try;Rid body=roslyn_statement(s->guarded_statement,role),filter=roslyn_expr(s->except_expr,role),cleanup=roslyn_statement(s->cleanup_statement,role);DB.put("microsoft_try_statement",{{"id",id},{"body_id",body},{"filter_id",R(filter)},{"handler_id",R(filter?cleanup:0)},{"finally_id",R(!filter?cleanup:0)},{"is_finally",static_cast<int>(!filter)},{"except_or_finally_position_id",R(roslyn_position(s->except_or_finally_position))}});roslyn_child(id,"body",0,body);roslyn_child(id,"else",0,cleanup);roslyn_child(id,"condition",0,filter);break;}
 case stmk_decl:{DB.put("declaration_statement",{{"id",id},{"has_static_or_thread_variable",static_cast<int>(p->variant.decl.has_static_or_thread_variable)}});int n=0;for(an_il_entity_list_entry_ptr e=p->variant.decl.entities;e;e=e->next){Rid entity=roslyn_entity(e->entity);DB.put("declaration_statement_entity",{{"owner_id",id},{"ordinal",n},{"entity_id",entity}});roslyn_reference(id,"member",n++,entity);
#if !DO_IL_LOWERING
 if(e->entity.kind==iek_variable){auto variable=reinterpret_cast<a_variable_ptr>(e->entity.ptr);if(variable->init_kind==initk_dynamic)roslyn_child(id,"initialization",n-1,roslyn_init(variable->initializer.dynamic,role));}
#endif
 }break;}
 default:DB.fail("Statement payload adapter missing: "+Q(p->kind));break;
 }
 roslyn_default_role=saved_role;return id;
}
#include "roslyn_templates.h"
static Rid roslyn_primary_declaration(Rid symbol) {
 auto known=roslyn_primary_declarations.find(symbol);return known==roslyn_primary_declarations.end()?0:known->second;
}
static Rid roslyn_declared_symbol(const a_tagged_pointer &p) {
 if(!p.ptr)return 0;
 auto parameter=roslyn_parameter_placeholders.find(p.ptr);if(parameter!=roslyn_parameter_placeholders.end())return parameter->second.size()==1?*parameter->second.begin():0;
 switch(p.kind){
 case iek_type:{auto type=reinterpret_cast<a_type_ptr>(p.ptr);return roslyn_type_declaration(type,roslyn_type(type));}
 case iek_constant:roslyn_constant(reinterpret_cast<a_constant_ptr>(p.ptr));return DB.existing(p.ptr,"enumerator_symbol");
 case iek_template:return roslyn_template_declared_symbol(reinterpret_cast<a_template_ptr>(p.ptr));
 case iek_routine:case iek_variable:case iek_field:case iek_namespace:return roslyn_entity(p);
 default:return 0;
 }
}
static Rid roslyn_macro(a_macro_ptr);
#include "roslyn_source_sequences.h"
#include "roslyn_ms_attributes.h"
#include "roslyn_local_expressions.h"
static Rid roslyn_macro(a_macro_ptr p) {
 bool fresh;Rid id=DB.claim(p,"macro_definition",fresh);if(!fresh)return id;
 Rid replacement=0;
#if EXTRA_SOURCE_POSITIONS_IN_IL
 replacement=roslyn_range(p->replacement_text_range);
#endif
 const char *origin=p->is_predefined?"predefined":p->is_command_line_definition?"command_line":"ordinary";
 DB.put("macro_definition",{{"id",id},{"name_id",DB.atom(p->source_corresp.name)},{"text_id",DB.atom(p->text)},{"position_id",R(roslyn_position(p->source_corresp.decl_position))},{"replacement_range_id",R(replacement)},{"kind_id",K("source_origin_kind",origin)},{"is_function_like",static_cast<int>(!p->object_like)},{"is_predefined",static_cast<int>(p->is_predefined)},{"is_undef",static_cast<int>(p->is_undef)}});return id;
}
static void roslyn_macros() {
#if MACRO_INVOCATION_TREE_IN_IL
 for(a_macro_invocation_record_index n=1;n<il_header.num_macro_invocation_records;++n){a_macro_invocation_record_ptr p;set_macro_inv_record_ptr_to_index(il_header.root_macro_invocation_record_block,n,p);if(p&&p->parent_macro_index>=NO_PARENT_MACRO_INVOCATION&&p->assoc_macro)roslyn_macro_invocations[n]=DB.fresh("macro_invocation");}
 #endif
 for(a_macro_ptr p=il_header.macros;p;p=p->next)roslyn_macro(p);
#if MACRO_INVOCATION_TREE_IN_IL
 int ordinal=0;for(const auto &entry:roslyn_macro_invocations){a_macro_invocation_record_ptr p;set_macro_inv_record_ptr_to_index(il_header.root_macro_invocation_record_block,entry.first,p);a_source_position start{},end{};start.seq=start.orig_seq=p->start.seq;start.column=start.orig_column=p->start.column;end.seq=end.orig_seq=p->end.seq;end.column=end.orig_column=p->end.column;
 Rid argument_text=0;
#if RECORD_MACRO_ARGS
 argument_text=DB.atom(p->arguments);
#endif
 auto parent=roslyn_macro_invocations.find(p->parent_macro_index);DB.put("macro_invocation",{{"id",entry.second},{"definition_id",roslyn_macro(p->assoc_macro)},{"parent_id",R(parent==roslyn_macro_invocations.end()?0:parent->second)},{"position_id",R(roslyn_position(start))},{"end_position_id",R(roslyn_position(end))},{"argument_text_id",R(argument_text)},{"ordinal",ordinal++}});}
#endif
}
static void roslyn_capability(const char *name,const std::string &value,bool enabled,bool required=false){DB.put("build_capability",{{"name_id",DB.atom(name)},{"state_id",K("availability","available")},{"value_id",DB.atom(value)},{"enabled",static_cast<int>(enabled)},{"required",static_cast<int>(required)}});}
static void roslyn_back_end() {
 RoslynStore store;roslyn_db=&store;roslyn_current_eh=0;roslyn_static_ordinal=0;roslyn_override_ordinals.clear();roslyn_field_records.clear();roslyn_local_constants.clear();roslyn_projected_tables.clear();roslyn_function_eh.clear();roslyn_regions.clear();roslyn_current_routine=nullptr;roslyn_default_role=DO_IL_LOWERING?7001:7003;roslyn_macro_invocations.clear();roslyn_positions.clear();roslyn_ranges.clear();roslyn_origins.clear();roslyn_availability.clear();roslyn_values.clear();roslyn_name_values.clear();roslyn_primary_declarations.clear();roslyn_parameter_placeholders.clear();roslyn_constant_values.clear();roslyn_scope_parents.clear();roslyn_local_expressions.clear();roslyn_vla_contexts.clear();roslyn_collected_scopes.clear();
 std::string output=std::getenv("ROSLYNOBJ_OUTPUT")?std::getenv("ROSLYNOBJ_OUTPUT"):std::string(il_header.primary_source_file->file_name)+".roslynobj";
 std::string temporary=output+".tmp.XXXXXX";std::vector<char> sibling(temporary.begin(),temporary.end());sibling.push_back(0);int temporary_fd=mkstemp(sibling.data());if(temporary_fd<0){std::perror("roslynobj temporary output");std::exit(EXIT_FAILURE);}if(::close(temporary_fd)!=0){std::perror("roslynobj temporary close");std::remove(sibling.data());std::exit(EXIT_FAILURE);}temporary=sibling.data();
 if(sqlite3_open_v2(temporary.c_str(),&store.connection,SQLITE_OPEN_READWRITE,nullptr)!=SQLITE_OK)store.fail(sqlite3_errmsg(store.connection));
 const char *page_size=std::getenv("ROSLYNOBJ_SQLITE_PAGE_SIZE");if(!page_size)page_size="1024";
 if(std::strcmp(page_size,"1024")&&std::strcmp(page_size,"2048")&&std::strcmp(page_size,"4096")&&std::strcmp(page_size,"8192"))store.fail("ROSLYNOBJ_SQLITE_PAGE_SIZE must be 1024, 2048, 4096, or 8192");else store.exec((std::string("PRAGMA page_size=")+page_size).c_str());
 store.exec("PRAGMA foreign_keys=ON; PRAGMA journal_mode=DELETE; BEGIN IMMEDIATE;");store.exec(roslynobj_schema_sql);
 roslyn_unit=DB.fresh("translation_unit");
 roslyn_fact(roslyn_unit,"macro_tree",MACRO_INVOCATION_TREE_IN_IL?"available":"disabled",MACRO_INVOCATION_TREE_IN_IL?nullptr:"MACRO_INVOCATION_TREE_IN_IL is disabled in this resolved build.");
 roslyn_fact(roslyn_unit,"macro_arguments",RECORD_MACRO_ARGS?"available":"disabled",RECORD_MACRO_ARGS?nullptr:"RECORD_MACRO_ARGS is disabled in this resolved build.");
 roslyn_fact(roslyn_unit,"tokens","not_retained","Stock IL retains specialized token sequences, but no general source token stream.");
 roslyn_fact(roslyn_unit,"trivia","not_retained","No complete source trivia stream is retained by this stock configuration.");
 if(!GENERATE_SOURCE_SEQUENCE_LISTS)roslyn_fact(roslyn_unit,"declaration_sequence","disabled","GENERATE_SOURCE_SEQUENCE_LISTS is disabled in this resolved build.");
 roslyn_source_information();Rid source=roslyn_source_file(il_header.primary_source_file);roslyn_macros();for(a_memory_region_number region=1;region<=highest_used_region_number;++region)for(a_scope_ptr s=il_header.region_scope_entry[region];s;s=s->next)roslyn_collect_local_initializers(s);Rid scope=roslyn_scope(il_header.primary_scope);roslyn_header_roots();
 DB.put("translation_unit",{{"id",roslyn_unit},{"global_scope_id",scope},{"entry_point_id",R(roslyn_routine(il_header.main_routine))},{"primary_input_id",source},{"language_mode_id",DB.atom(std::string(il_header.source_language==sl_C?"c":"c++")+Q(il_header.std_version))}});
 DB.put("target_profile",{{"id",1},{"abi_id",K("abi",IA64_ABI?"itanium":targ_microsoft_ptr_to_member_sizing?"microsoft":CFRONT_OBJECT_CODE_COMPATIBILITY?"cfront":"cfront_like")},{"addressable_unit_bits",static_cast<int>(targ_char_bit)},{"little_endian",static_cast<int>(targ_little_endian)},{"pointer_bits",static_cast<int>(targ_sizeof_pointer*targ_char_bit)},{"pointer_alignment",Q(targ_alignof_pointer)}});
#define CAP(name) roslyn_capability(#name,Q(name),name!=0)
 CAP(RECORD_BIT_FIELD_CONTAINER_OFFSETS_IN_IL);CAP(TARG_MICROSOFT_PTR_TO_MEMBER_SIZING_win64);CAP(TARG_MICROSOFT_BIT_FIELD_ALLOCATION_win64);CAP(targ_microsoft_bit_field_allocation);
 CAP(il_header.plain_chars_are_signed);CAP(il_header.pcc_compatibility_mode);CAP(il_header.enum_type_is_integral);CAP(il_header.default_max_member_alignment);CAP(il_header.UCN_identifiers_used);CAP(il_header.vla_used);CAP(il_header.any_templates_seen);CAP(il_header.prototype_instantiations_in_il);CAP(il_header.il_has_all_prototype_instantiations);CAP(il_header.il_has_C_semantics);CAP(il_header.target_configuration_index);
#if GNU_EXTENSIONS_ALLOWED
 CAP(il_header.gcc_mode);CAP(il_header.gpp_mode);CAP(il_header.clang_mode);CAP(il_header.gnu_version);CAP(il_header.clang_version);CAP(il_header.short_enums);CAP(il_header.default_nocommon);CAP(il_header.gnu_c89_inlining);
#endif
#if NEAR_AND_FAR_ALLOWED
 CAP(il_header.near_and_far_are_enabled);CAP(il_header.far_data_pointers);CAP(il_header.far_code_pointers);
#endif
#if UPC_EXTENSIONS_ALLOWED
 CAP(il_header.default_upc_strict_access);
#endif
 CAP(DO_IL_LOWERING);CAP(DO_FULL_PORTABLE_EH_LOWERING);CAP(GENERATE_EH_TABLES);CAP(DO_UNORDERED_EH_PROCESSING);CAP(KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED);CAP(BACK_END_IS_C_GEN_BE);CAP(BACK_END_IS_CP_GEN_BE);CAP(BACK_END_SHOULD_BE_CALLED);CAP(DOING_SOURCE_ANALYSIS);CAP(IL_SHOULD_BE_WRITTEN_TO_FILE);CAP(IA64_ABI);CAP(CFRONT_OBJECT_CODE_COMPATIBILITY);CAP(CFRONT_2_1_OBJECT_CODE_COMPATIBILITY);CAP(CFRONT_3_0_OBJECT_CODE_COMPATIBILITY);CAP(ASSIGNMENT_TO_THIS_ALLOWED);CAP(NEW_CAN_BE_FOLDED_INTO_CTOR);CAP(HANDLE_VIRTUAL_BASES_IN_COMPLETE_CTOR_DTORS);CAP(TIE_DEFAULT_GNU_ABI_VERSION_TO_GNU_VERSION);CAP(target_configuration_index);CAP(MINIMAL_INLINING);CAP(MAINTAIN_NEEDED_FLAGS);CAP(DEFAULT_REMOVE_UNNEEDED_ENTITIES);CAP(PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE);CAP(LOWER_VARIABLE_LENGTH_ARRAYS);CAP(LOWER_CLASS_RVALUE_ADJUST);CAP(LOWER_COMPLEX);CAP(LOWER_FIXED_POINT);CAP(LOWER_IFUNC);CAP(LOWER_LVALUE_RETURNING_OPERATIONS);CAP(LOWER_MICROSOFT_NONCONSTANT_AGGREGATE);CAP(LOWER_DESIGNATED_INITIALIZERS);CAP(GENERATE_SOURCE_SEQUENCE_LISTS);CAP(MAINTAIN_CLASS_MEMBER_LIST);CAP(RECORD_BACKING_EXPRS_WITH_IL_LOWERING);CAP(ONE_INSTANTIATION_PER_OBJECT);CAP(EXTRA_SOURCE_POSITIONS_IN_IL);CAP(RECORD_MACROS_IN_IL);CAP(FULLY_RESOLVED_MACRO_POSITIONS);CAP(RECORD_MACRO_INVOCATIONS);CAP(MACRO_INVOCATION_TREE_IN_IL);CAP(RECORD_MACRO_ARGS);CAP(COMPILE_MULTIPLE_TRANSLATION_UNITS);CAP(COROUTINE_ENABLING_POSSIBLE);CAP(REFLECTION_ENABLING_POSSIBLE);CAP(reflection_enabled);CAP(PROTOTYPE_INSTANTIATIONS_IN_IL);CAP(ALL_TEMPLATE_INFO_IN_IL);CAP(prototype_instantiations_in_il);CAP(all_template_info_in_il);CAP(HOST_FP_VALUE_IS_128BIT);CAP(USE_SOFTFLOAT);CAP(USE_FLOAT128_FOR_HOST_FP_VALUE);CAP(HOST_HAS_FLOAT16_TYPE);CAP(exceptions_enabled);CAP(gpp_mode);CAP(gcc_mode);CAP(clang_mode);CAP(gnu_version);CAP(microsoft_mode);CAP(microsoft_version);CAP(RECORD_UNRECOGNIZED_ATTRIBUTES);CAP(record_unrecognized_attributes);CAP(RECOGNIZE_MICROSOFT_ATTRIBUTES);CAP(SUPPRESS_MICROSOFT_ATTRIBUTE_PROCESSING);CAP(cppcli_enabled);CAP(cppcx_enabled);CAP(targ_microsoft_ptr_to_member_sizing);CAP(CPPCLI_ENABLING_POSSIBLE);CAP(CPPCX_ENABLING_POSSIBLE);CAP(READ_CPPCLI_PORTABLE_ASSEMBLIES);CAP(EXPORT_ENABLING_POSSIBLE);CAP(export_template_allowed);CAP(DEFAULT_EXPORT_TEMPLATE_ALLOWED);
#undef CAP
 const char *count_text=std::getenv("ROSLYNOBJ_ARGUMENT_COUNT");if(count_text){long count=std::strtol(count_text,nullptr,10);for(long i=0;i<count;++i){std::string key="ROSLYNOBJ_ARGUMENT_"+Q(i);const char *argument=std::getenv(key.c_str());if(argument)DB.put("invocation_argument",{{"ordinal",static_cast<sqlite3_int64>(i)},{"text_id",DB.atom(argument)}});}}
 if(!DO_IL_LOWERING)roslyn_capability("source_semantic_graph_v1","1",true,true);
 roslyn_capability("normalized_semantic_graph_v1","1",true,true);roslyn_capability("exact_target_values_v1","1",true,true);if(!roslyn_function_eh.empty())roslyn_capability("edg_partial_eh_v1","1",true,true);
 unsigned char random[16];sqlite3_randomness(16,random);std::string artifact;static const char hex[]="0123456789abcdef";for(unsigned char b:random){artifact+=hex[b>>4];artifact+=hex[b&15];}
 DB.put("object_header",{{"id",1},{"format_major",1},{"format_minor",0},{"artifact_id",artifact},{"producer_id",DB.atom("roslyn_gen_be")},{"revision_id",DB.atom("89e67e07c7f6d0fc38622f9778d8e79f40deaa0f")},{"edg_version_id",DB.atom(il_header.compiler_version)},{"capture_profile_id",ROSLYN_CAPTURE_PROFILE_ID},{"target_profile_id",1},{"main_source_file_id",source},{"translation_unit_id",roslyn_unit},{"complete",1}});
 store.check_foreign_keys();
 if(!store.failed)store.exec("COMMIT;");else store.exec("ROLLBACK;");
 store.close();roslyn_db=nullptr;
 if(store.failed){std::remove(temporary.c_str());std::fprintf(stderr,"roslynobj export error: %s\n",store.failure.c_str());std::exit(EXIT_FAILURE);}
 /* Validate the wrapper acknowledgement before the irreversible publication.
    The wrapper accepts it only when the process also exits successfully. */
 const char *status_path=std::getenv("ROSLYNOBJ_STATUS_PATH");if(status_path){FILE *status=std::fopen(status_path,"w");if(!status){std::remove(temporary.c_str());std::perror("roslynobj status");std::exit(EXIT_FAILURE);}bool ok=std::fputs("published\n",status)>=0;if(std::fclose(status)!=0)ok=false;if(!ok){std::remove(temporary.c_str());std::perror("roslynobj status write");std::exit(EXIT_FAILURE);}}
 if(std::rename(temporary.c_str(),output.c_str())!=0){std::remove(temporary.c_str());std::perror("roslynobj publish");std::exit(EXIT_FAILURE);}
 std::fprintf(stderr,"roslynobj published %s: entities=%lld text_atoms=%lld\n",output.c_str(),static_cast<long long>(store.sequence),static_cast<long long>(store.atom_sequence));
}
END_EDG_NAMESPACE
USING_NAMESPACE_EDG
void back_end(void) { roslyn_back_end(); }
