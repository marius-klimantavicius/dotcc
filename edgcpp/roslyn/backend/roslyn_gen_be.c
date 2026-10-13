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
static std::map<a_using_decl_ptr,std::set<a_scope_ptr>> roslyn_using_scopes;
static std::map<Rid,a_source_correspondence_ptr> roslyn_correspondence_owners;

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
static Rid roslyn_source_correspondence(a_source_correspondence &);
static void roslyn_attach_correspondence(Rid,a_source_correspondence &);
#if GENERATE_SOURCE_SEQUENCE_LISTS
static Rid roslyn_source_entry(a_source_sequence_entry_ptr);
static Rid roslyn_source_sublist(a_src_seq_sublist_ptr);
#endif
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
static an_expr_node_ptr roslyn_unique_local_expression(const void *,a_local_expr_node_ref_kind);
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
#include "roslyn_attributes.h"
#include "roslyn_parameters.h"
#include "roslyn_names.h"
#include "roslyn_type_support.h"
#include "roslyn_modules.h"
static a_scope_ptr roslyn_scope_parent(a_scope_ptr p) {if(!p)return nullptr;if(p->parent)return p->parent;auto found=roslyn_scope_parents.find(p);return found==roslyn_scope_parents.end()?nullptr:found->second;}
static bool roslyn_metadata_routine(a_routine_ptr p) {return p&&(p->is_prototype_instantiation||p->is_consteval||special_kind_is(p,sfk_deduction_guide));}
static int roslyn_routine_role(a_routine_ptr p) {return roslyn_metadata_routine(p)?7002:DO_IL_LOWERING?7001:7003;}
static a_routine_ptr roslyn_scope_routine(a_scope_ptr p) {std::set<a_scope_ptr> seen;for(;p&&seen.insert(p).second;p=roslyn_scope_parent(p))if(p->kind==sck_function)return p->variant.routine.ptr;return nullptr;}
static void roslyn_child(Rid owner,const char *role,int ordinal,Rid child) {
 if(child){DB.evaluated_child(owner,child);DB.put("structural_child",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"child_id",child}});}
}
static void roslyn_reference(Rid owner,const char *role,int ordinal,Rid target) {
 if(target)DB.put("semantic_reference",{{"owner_id",owner},{"role_id",K("role",role)},{"ordinal",ordinal},{"target_id",target}});
}
#include "roslyn_common.h"
#include "roslyn_entities.h"
#include "roslyn_routines.h"
#include "roslyn_variables.h"
#include "roslyn_layout.h"
static Rid roslyn_type_declaration(a_type_ptr p,Rid type) {
 if(p->kind==tk_template_param){auto parameter=roslyn_parameter_placeholders.find(p);return parameter==roslyn_parameter_placeholders.end()||parameter->second.size()!=1?0:*parameter->second.begin();}
 if((p->kind==tk_class||p->kind==tk_struct||p->kind==tk_union)&&p->variant.class_struct_union.proxy_class)return 0;
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
 // Master and destruction-order links identify associated recipes. Actual
 // evaluated edges promote the same nodes when their execution is reached.
 DB.put("initialization",{{"id",id},{"kind_id",roslyn_kind_dynamic_init(p->kind)},{"target_id",R(roslyn_variable(p->variable))},{"type_id",R(p->variable?roslyn_type(p->variable->type):0)},{"lambda_id",R(p->kind==dik_lambda?roslyn_lambda(p->variant.constant.lambda):0)},{"expression_id",R(expr)},{"constant_id",R(constant)},{"constructor_id",R(ctor)},{"destructor_id",R(roslyn_routine(p->destructor))},{"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"init_expression_lifetime_id",R(roslyn_lifetime(p->init_expr_lifetime))},{"master_id",R(roslyn_init(p->master_entry,7002))},{"next_destruction_id",R(roslyn_init(p->next_in_destruction_list,7002))},{"overlapping_lifetime_id",R(roslyn_lifetime(p->lifetime_of_overlapping_temps))},{"source_id",R(source)},{"is_unordered",static_cast<int>(p->unordered)},{"is_conditional",static_cast<int>(p->inside_conditional_expression)},{"is_overlapping",static_cast<int>(p->overlaps_temps_in_inner_lifetime)},{"is_static_temporary",static_cast<int>(p->static_temp)},{"is_partial_aggregate",static_cast<int>(p->destruction_is_for_partially_constructed_aggregate)},{"is_partial_initialization",static_cast<int>(p->is_partially_initialized)},{"frees_storage",static_cast<int>(p->is_freeing_of_storage_on_exception)},{"is_freeing_of_exception_object",frees_exception_object},{"frees_array",static_cast<int>(p->is_array_freeing)},{"is_explicit_cast",static_cast<int>(p->is_explicit_cast)},{"is_braced",static_cast<int>(p->is_braced_initializer)},{"is_compound_literal",static_cast<int>(p->is_compound_literal)},{"follows_executable_statement",static_cast<int>(p->follows_an_exec_statement)},{"has_temporary_lifetime",static_cast<int>(p->has_temporary_lifetime)},{"is_constructor_initialization",static_cast<int>(p->is_constructor_init)},{"is_conditional_guard",LF(p,is_guard_var_for_local_static_var_init)},{"is_vla_deallocation",LF(p,is_vla_deallocation)},{"creates_initializer_list",static_cast<int>(p->is_creation_of_initializer_list_object)},{"is_initializer_list_array",static_cast<int>(p->is_array_for_initializer_list_object)},{"is_reused_value",static_cast<int>(p->is_reused_value)},{"is_question_result",static_cast<int>(p->is_result_for_class_rvalue_question_mark)},{"is_comma_result",static_cast<int>(p->is_result_for_comma_operator)},{"is_class_result_master",static_cast<int>(p->class_rvalue_initialized_through_master_entry)},{"constructor_copy_implied_source",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_copy_constructor_with_implied_source)},{"constructor_implicit_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_implicit_copy_for_copy_initialization)},{"constructor_value_initialization",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.value_initialization)},{"constructor_sequenced_arguments",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.has_sequenced_arguments)},{"constructor_array_copy",static_cast<int>(p->kind==dik_constructor&&p->variant.constructor.is_array_copy)},{"constant_nonconstant",static_cast<int>((p->kind==dik_constant||p->kind==dik_nonconstant_aggregate||p->kind==dik_lambda)&&p->variant.constant.non_constant)}});
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
#elif DO_IL_LOWERING && !DO_FULL_PORTABLE_EH_LOWERING
static Rid roslyn_eh(an_eh_prologue_supplement_ptr p,Rid prologue) {
 auto old=roslyn_function_eh.find(p->routine);if(old!=roslyn_function_eh.end())return old->second;
 Rid eh=DB.fresh("function_eh");roslyn_function_eh[p->routine]=eh;roslyn_current_eh=eh;
 DB.put("function_eh",{{"id",eh},{"routine_id",roslyn_routine(p->routine)},{"prologue_id",prologue},
  {"initial_state",RoslynSqlValue()},{"null_state",RoslynSqlValue()}});return eh;
}
#endif
#include "roslyn_execution.h"
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
 bool fresh;Rid id=DB.claim(p,"macro_definition",fresh);if(!fresh)return id;roslyn_attach_correspondence(id,p->source_corresp);
 Rid replacement=0;
#if EXTRA_SOURCE_POSITIONS_IN_IL
 replacement=roslyn_range(p->replacement_text_range);
#endif
 const char *origin=p->is_predefined?"predefined":p->is_command_line_definition?"command_line":"ordinary";
 DB.put("macro_definition",{{"id",id},{"text_id",DB.atom(p->text)},{"replacement_range_id",R(replacement)},{"kind_id",K("source_origin_kind",origin)},{"is_function_like",static_cast<int>(!p->object_like)},{"is_predefined",static_cast<int>(p->is_predefined)},{"is_undef",static_cast<int>(p->is_undef)}});return id;
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
 RoslynStore store;roslyn_db=&store;roslyn_current_eh=0;roslyn_static_ordinal=0;roslyn_override_ordinals.clear();roslyn_field_records.clear();roslyn_local_constants.clear();roslyn_projected_tables.clear();roslyn_function_eh.clear();roslyn_regions.clear();roslyn_current_routine=nullptr;roslyn_default_role=DO_IL_LOWERING?7001:7003;roslyn_macro_invocations.clear();roslyn_positions.clear();roslyn_ranges.clear();roslyn_origins.clear();roslyn_availability.clear();roslyn_values.clear();roslyn_name_values.clear();roslyn_primary_declarations.clear();roslyn_parameter_placeholders.clear();roslyn_constant_values.clear();roslyn_scope_parents.clear();roslyn_using_scopes.clear();roslyn_correspondence_owners.clear();roslyn_local_expressions.clear();roslyn_vla_contexts.clear();roslyn_collected_scopes.clear();roslyn_collected_types.clear();
 std::string output=std::getenv("ROSLYNOBJ_OUTPUT")?std::getenv("ROSLYNOBJ_OUTPUT"):std::string(il_header.primary_source_file->file_name)+".roslynobj";
 std::string temporary=output+".tmp.XXXXXX";std::vector<char> sibling(temporary.begin(),temporary.end());sibling.push_back(0);int temporary_fd=mkstemp(sibling.data());if(temporary_fd<0){std::perror("roslynobj temporary output");std::exit(EXIT_FAILURE);}if(::close(temporary_fd)!=0){std::perror("roslynobj temporary close");std::remove(sibling.data());std::exit(EXIT_FAILURE);}temporary=sibling.data();
 if(sqlite3_open_v2(temporary.c_str(),&store.connection,SQLITE_OPEN_READWRITE,nullptr)!=SQLITE_OK)store.fail(sqlite3_errmsg(store.connection));
 const char *page_size=std::getenv("ROSLYNOBJ_SQLITE_PAGE_SIZE");if(!page_size)page_size="1024";
 if(std::strcmp(page_size,"1024")&&std::strcmp(page_size,"2048")&&std::strcmp(page_size,"4096")&&std::strcmp(page_size,"8192"))store.fail("ROSLYNOBJ_SQLITE_PAGE_SIZE must be 1024, 2048, 4096, or 8192");else store.exec((std::string("PRAGMA page_size=")+page_size).c_str());
 store.exec("PRAGMA foreign_keys=OFF; PRAGMA journal_mode=DELETE; BEGIN IMMEDIATE;");store.exec(roslynobj_schema_sql);
 int bulk_fk_state=-1;if(sqlite3_db_config(store.connection,SQLITE_DBCONFIG_ENABLE_FKEY,-1,&bulk_fk_state)!=SQLITE_OK||bulk_fk_state!=0)store.fail("Private bulk-load foreign key state is not OFF");
 roslyn_unit=DB.fresh("translation_unit");
 roslyn_fact(roslyn_unit,"macro_tree",MACRO_INVOCATION_TREE_IN_IL?"available":"disabled",MACRO_INVOCATION_TREE_IN_IL?nullptr:"MACRO_INVOCATION_TREE_IN_IL is disabled in this resolved build.");
 roslyn_fact(roslyn_unit,"macro_arguments",RECORD_MACRO_ARGS?"available":"disabled",RECORD_MACRO_ARGS?nullptr:"RECORD_MACRO_ARGS is disabled in this resolved build.");
 roslyn_fact(roslyn_unit,"tokens","not_retained","Stock IL retains specialized token sequences, but no general source token stream.");
 roslyn_fact(roslyn_unit,"trivia","not_retained","No complete source trivia stream is retained by this stock configuration.");
 if(!GENERATE_SOURCE_SEQUENCE_LISTS)roslyn_fact(roslyn_unit,"declaration_sequence","disabled","GENERATE_SOURCE_SEQUENCE_LISTS is disabled in this resolved build.");
 for(a_memory_region_number region=1;region<=highest_used_region_number;++region)for(a_scope_ptr s=il_header.region_scope_entry[region];s;s=s->next)roslyn_collect_local_initializers(s);
 roslyn_source_information();Rid source=roslyn_source_file(il_header.primary_source_file);roslyn_macros();Rid scope=roslyn_scope(il_header.primary_scope);roslyn_header_roots();
 DB.put("translation_unit",{{"id",roslyn_unit},{"global_scope_id",scope},{"entry_point_id",R(roslyn_routine(il_header.main_routine))},{"primary_input_id",source},{"language_mode_id",DB.atom(std::string(il_header.source_language==sl_C?"c":"c++")+Q(il_header.std_version))}});
 DB.put("target_profile",{{"id",1},{"abi_id",K("abi",IA64_ABI?"itanium":targ_microsoft_ptr_to_member_sizing?"microsoft":CFRONT_OBJECT_CODE_COMPATIBILITY?"cfront":"cfront_like")},{"addressable_unit_bits",static_cast<int>(targ_char_bit)},{"little_endian",static_cast<int>(targ_little_endian)},{"pointer_bits",static_cast<int>(targ_sizeof_pointer*targ_char_bit)},{"pointer_alignment",Q(targ_alignof_pointer)}});
#define CAP(name) roslyn_capability(#name,Q(name),name!=0)
#ifdef ROSLYN_BUILD_PROFILE_NAME
 roslyn_capability("roslyn_build_profile",ROSLYN_BUILD_PROFILE_NAME,true);
#endif
 CAP(SUN_EXTENSIONS_ALLOWED);CAP(FIXED_POINT_ALLOWED);CAP(USE_DOUBLE_FOR_HOST_FP_VALUE);CAP(USE_LONG_DOUBLE_FOR_HOST_FP_VALUE);CAP(FLOAT128_ENABLING_POSSIBLE);CAP(FLOAT80_ENABLING_POSSIBLE);CAP(FP_HAS_LONG_DOUBLE);CAP(targ_ldbl_mant_dig);
#if SUN_EXTENSIONS_ALLOWED
 CAP(sun_mode);CAP(sun_linker_scope_allowed);
#endif
 CAP(RECORD_BIT_FIELD_CONTAINER_OFFSETS_IN_IL);CAP(TARG_MICROSOFT_PTR_TO_MEMBER_SIZING_win64);CAP(TARG_MICROSOFT_BIT_FIELD_ALLOCATION_win64);CAP(targ_microsoft_bit_field_allocation);
 CAP(il_header.plain_chars_are_signed);CAP(il_header.pcc_compatibility_mode);CAP(il_header.enum_type_is_integral);CAP(il_header.default_max_member_alignment);CAP(il_header.UCN_identifiers_used);CAP(il_header.vla_used);CAP(il_header.any_templates_seen);CAP(il_header.prototype_instantiations_in_il);CAP(il_header.il_has_all_prototype_instantiations);CAP(il_header.il_has_C_semantics);CAP(il_header.target_configuration_index);
#if GNU_EXTENSIONS_ALLOWED
 CAP(il_header.gcc_mode);CAP(il_header.gpp_mode);CAP(il_header.clang_mode);CAP(il_header.gnu_version);CAP(il_header.clang_version);CAP(il_header.short_enums);CAP(il_header.default_nocommon);CAP(il_header.gnu_c89_inlining);
#endif
#if NEAR_AND_FAR_ALLOWED
 CAP(il_header.near_and_far_are_enabled);CAP(il_header.far_data_pointers);CAP(il_header.far_code_pointers);
#endif
#if UPC_EXTENSIONS_ALLOWED
 CAP(il_header.default_upc_strict_access);CAP(upc_num_threads);CAP(upc_dynamic_threads());
#endif
 CAP(AUTOMATIC_TEMPLATE_INSTANTIATION);CAP(INSTANTIATE_INLINE_VARIABLES);CAP(INSTANTIATE_EXTERN_INLINE);CAP(LOWER_EXTERN_INLINE);CAP(instantiate_extern_inline);CAP(instantiate_inline_variables);CAP(instantiation_mode);CAP(DEFAULT_RECORD_FORM_OF_NAME_REFERENCE);CAP(CREATE_LEXICAL_TYPEREFS);CAP(record_form_of_name_reference);CAP(cfront_2_1_mode);CAP(cfront_3_0_mode);
 CAP(SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS);CAP(USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES);CAP(IMPLEMENTATION_SUPPORTS_MULTIPLE_THREADS);CAP(CHECKING);
#ifdef USE_PATCH_INIT_STARTUP
 CAP(USE_PATCH_INIT_STARTUP);
#else
 roslyn_capability("USE_PATCH_INIT_STARTUP","not_defined",false);
#endif
 CAP(INCLUDE_EDG_TEST_NAMED_REGISTERS);CAP(NAMED_REGISTERS_ALLOWED);
#if AUTOMATIC_TEMPLATE_INSTANTIATION
 CAP(DEFAULT_AUTOMATIC_INSTANTIATION_MODE);CAP(automatic_instantiation_mode);CAP(INSTANTIATE_TEMPLATES_EVERYWHERE_USED);CAP(USE_TEMPLATE_INFO_FILE);CAP(INSTANTIATION_FLAGS_IN_TEMPLATE_INFO_FILE);
#endif
#if MINIMAL_INLINING
 CAP(inlining_enabled);CAP(DEFAULT_INLINE_STATEMENT_LIMIT);CAP(inline_statement_limit);
#endif
 CAP(NEED_NAME_MANGLING);CAP(DO_IL_LOWERING);CAP(DO_FULL_PORTABLE_EH_LOWERING);CAP(GENERATE_EH_TABLES);CAP(DO_UNORDERED_EH_PROCESSING);CAP(KEEP_OBJECT_LIFETIME_INFO_IN_LOWERED_IL_WHEN_EH_ENABLED);CAP(BACK_END_IS_C_GEN_BE);CAP(BACK_END_IS_CP_GEN_BE);CAP(BACK_END_SHOULD_BE_CALLED);CAP(DOING_SOURCE_ANALYSIS);CAP(IL_SHOULD_BE_WRITTEN_TO_FILE);CAP(IA64_ABI);CAP(CFRONT_OBJECT_CODE_COMPATIBILITY);CAP(CFRONT_2_1_OBJECT_CODE_COMPATIBILITY);CAP(CFRONT_3_0_OBJECT_CODE_COMPATIBILITY);CAP(ASSIGNMENT_TO_THIS_ALLOWED);CAP(NEW_CAN_BE_FOLDED_INTO_CTOR);CAP(HANDLE_VIRTUAL_BASES_IN_COMPLETE_CTOR_DTORS);CAP(TIE_DEFAULT_GNU_ABI_VERSION_TO_GNU_VERSION);CAP(target_configuration_index);CAP(MINIMAL_INLINING);CAP(MAINTAIN_NEEDED_FLAGS);CAP(DEFAULT_REMOVE_UNNEEDED_ENTITIES);CAP(PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE);CAP(LOWER_VARIABLE_LENGTH_ARRAYS);CAP(LOWER_CLASS_RVALUE_ADJUST);CAP(LOWER_COMPLEX);CAP(LOWER_FIXED_POINT);CAP(LOWER_IFUNC);CAP(LOWER_LVALUE_RETURNING_OPERATIONS);CAP(LOWER_MICROSOFT_NONCONSTANT_AGGREGATE);CAP(LOWER_DESIGNATED_INITIALIZERS);CAP(GENERATE_SOURCE_SEQUENCE_LISTS);CAP(MAINTAIN_CLASS_MEMBER_LIST);CAP(RECORD_BACKING_EXPRS_WITH_IL_LOWERING);CAP(ONE_INSTANTIATION_PER_OBJECT);CAP(EXTRA_SOURCE_POSITIONS_IN_IL);CAP(RECORD_MACROS_IN_IL);CAP(FULLY_RESOLVED_MACRO_POSITIONS);CAP(RECORD_MACRO_INVOCATIONS);CAP(MACRO_INVOCATION_TREE_IN_IL);CAP(RECORD_MACRO_ARGS);CAP(COMPILE_MULTIPLE_TRANSLATION_UNITS);CAP(COROUTINE_ENABLING_POSSIBLE);CAP(REFLECTION_ENABLING_POSSIBLE);CAP(reflection_enabled);CAP(PROTOTYPE_INSTANTIATIONS_IN_IL);CAP(ALL_TEMPLATE_INFO_IN_IL);CAP(prototype_instantiations_in_il);CAP(all_template_info_in_il);CAP(HOST_FP_VALUE_IS_128BIT);CAP(USE_SOFTFLOAT);CAP(USE_FLOAT128_FOR_HOST_FP_VALUE);CAP(HOST_HAS_FLOAT16_TYPE);CAP(exceptions_enabled);CAP(gpp_mode);CAP(gcc_mode);CAP(clang_mode);CAP(gnu_version);CAP(microsoft_mode);CAP(microsoft_version);CAP(RECORD_UNRECOGNIZED_ATTRIBUTES);CAP(record_unrecognized_attributes);CAP(RECOGNIZE_MICROSOFT_ATTRIBUTES);CAP(SUPPRESS_MICROSOFT_ATTRIBUTE_PROCESSING);CAP(cppcli_enabled);CAP(cppcx_enabled);CAP(targ_microsoft_ptr_to_member_sizing);CAP(CPPCLI_ENABLING_POSSIBLE);CAP(CPPCX_ENABLING_POSSIBLE);CAP(READ_CPPCLI_PORTABLE_ASSEMBLIES);CAP(EXPORT_ENABLING_POSSIBLE);CAP(export_template_allowed);CAP(DEFAULT_EXPORT_TEMPLATE_ALLOWED);
#undef CAP
 const char *count_text=std::getenv("ROSLYNOBJ_ARGUMENT_COUNT");if(count_text){long count=std::strtol(count_text,nullptr,10);for(long i=0;i<count;++i){std::string key="ROSLYNOBJ_ARGUMENT_"+Q(i);const char *argument=std::getenv(key.c_str());if(argument)DB.put("invocation_argument",{{"ordinal",static_cast<sqlite3_int64>(i)},{"text_id",DB.atom(argument)}});}}
 if(!DO_IL_LOWERING)roslyn_capability("source_semantic_graph_v1","1",true,true);
 roslyn_capability("normalized_semantic_graph_v1","1",true,true);roslyn_capability("normalized_semantic_schema_v1",roslynobj_schema_contract_sha256,true,true);roslyn_capability("exact_target_values_v1","1",true,true);if(!roslyn_function_eh.empty())roslyn_capability("edg_partial_eh_v1","1",true,true);
 roslyn_reconcile_using_scopes();store.check_foreign_keys();
 unsigned char random[16];sqlite3_randomness(16,random);std::string artifact;static const char hex[]="0123456789abcdef";for(unsigned char b:random){artifact+=hex[b>>4];artifact+=hex[b&15];}
 DB.put("object_header",{{"id",1},{"format_major",1},{"format_minor",0},{"artifact_id",artifact},{"producer_id",DB.atom("roslyn_gen_be")},{"revision_id",DB.atom("89e67e07c7f6d0fc38622f9778d8e79f40deaa0f")},{"edg_version_id",DB.atom(il_header.compiler_version)},{"capture_profile_id",ROSLYN_CAPTURE_PROFILE_ID},{"target_profile_id",1},{"main_source_file_id",source},{"translation_unit_id",roslyn_unit},{"complete",1}});
 store.check_foreign_keys();
 if(!store.failed)store.exec("COMMIT;");else store.exec("ROLLBACK;");
 if(!store.failed){store.exec("PRAGMA foreign_keys=ON;");int published_fk_state=-1;if(sqlite3_db_config(store.connection,SQLITE_DBCONFIG_ENABLE_FKEY,-1,&published_fk_state)!=SQLITE_OK||published_fk_state!=1)store.fail("Publication foreign key state is not ON");}
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
