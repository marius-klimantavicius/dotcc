#ifndef DOT_GEN_OBJECT_SOURCE_H
#define DOT_GEN_OBJECT_SOURCE_H
// Immutable source-declaration snapshots for the owned .dotobj adapter.
// Hooks observe EDG IL before lowering. They never alter EDG entries and are
// inactive for ordinary dot/C backends. Addresses are private lookup keys only;
// the published object contains IDs, owned strings and copied positions.
#include <cstdlib>
#include <cstdio>
#include <map>
#include <set>
#include <string>
#include <vector>
#include <tuple>
#include "fe_common.h"
#include "il.h"
#include "il_walk.h"
#include "types.h"
#include "lower_name.h"
#if DO_IL_LOWERING
#include "lower_il.h"
#endif
#include "symbol_tbl.h"
#include "const_ints.h"
#include "exprutil.h"
#include "scope_stk.h"
BEGIN_EDG_NAMESPACE
namespace dot_object_source {
inline bool enabled() {
  static const bool value=[](){const char *v=std::getenv("EDG_DOT_OBJECT");return v && std::string(v)=="1";}();
  return value;
}
struct ExceptionPolicyFlags {
  bool present=false,indeterminate=false,arg_cached=false,copy_from_prototype=false;
  bool is_noexcept=false,throw_any=false;
  std::string policy() const {
    return !present || throw_any?"unwind":is_noexcept?"terminate":
      indeterminate || arg_cached || copy_from_prototype?"unsupported-dynamic":"dynamic";
  }
};
inline ExceptionPolicyFlags exception_policy_flags(a_type_ptr type) {
  ExceptionPolicyFlags result;auto spec=type->variant.routine.extra_info->exception_specification;
  if(spec) {
    result.present=true;result.indeterminate=spec->indeterminate;result.arg_cached=spec->arg_cached;
    result.copy_from_prototype=spec->copy_from_prototype;result.is_noexcept=spec->is_noexcept;
    result.throw_any=spec->throw_any;
  }
  return result;
}
struct DynamicExceptionSpecification {
  std::vector<unsigned> declared_types,match_types;
  std::string error;
};
struct RoutineExceptionResolution {
  unsigned final_type=0;
  a_routine_ptr routine=nullptr;
  ExceptionPolicyFlags final_flags;
  DynamicExceptionSpecification dynamic_specification;
  std::string special_kind,error;
  bool is_defaulted=false,compiler_generated=false,inheriting_constructor=false,implicit_destructor=false;
};
static_assert(int(trk_is_decltype)==1 &&
  int(trk_is_underlying_type)==5 &&
  int(trk_is_typeof_with_expression)==6 &&
  int(trk_is_typeof_with_type_operand)==7 &&
  int(trk_add_lvalue_reference)==14 &&
  int(trk_add_pointer)==15 &&
  int(trk_add_rvalue_reference)==16 &&
  int(trk_decay)==17 &&
  int(trk_make_signed)==18 &&
  int(trk_make_unsigned)==19 &&
  int(trk_remove_all_extents)==20 &&
  int(trk_remove_const)==21 &&
  int(trk_remove_cv)==22 &&
  int(trk_remove_cvref)==23 &&
  int(trk_remove_extent)==24 &&
  int(trk_remove_pointer)==25 &&
  int(trk_remove_reference_t)==26 &&
  int(trk_remove_restrict)==27 &&
  int(trk_remove_volatile)==28 &&
  int(trk_remove_reference)==29, "source type operator profile mismatch");
struct Type {
  unsigned id=0,underlying=0,execution=0,namespace_owner=0;
  unsigned long long element_count=0;
  bool array_bound_known=false;
  unsigned dependent_array_bound=0;
  std::string kind,name,source_form="plain";
  int type_operator=-1;
  unsigned operator_expression=0,operator_type=0;
  bool operator_dependent=false,operator_unparenthesized=false;
  a_type_ptr record_type=nullptr;
  bool is_const=false,is_volatile=false,is_restrict=false,is_atomic=false;
  std::vector<std::string> qualifiers;
  std::string error;
  a_type_ptr original_type=nullptr; // Private correspondence key, never wire.
  unsigned member_owner=0;
  std::string dependent_kind,dependent_identity;
  unsigned dependent_owner=0,dependent_original_nested=0;
  int dependent_depth=0,dependent_position=0;
  bool dependent_pack=false;
  unsigned constraint_expression=0;
  // Private physical representation fact also retained for 128-bit enums.
  bool is_int128=false,is_unsigned_int128=false;
  a_source_position position={};
  // Routine types are private signature snapshots, not scalar sourceTypes.
  unsigned result=0,object_type=0,prototype_origin=0,prototype_origin_binder=0;
  bool object_is_const=false,object_is_volatile=false;
  std::string object_ref_qualifier="none";
  bool variadic=false;
  std::string exception_policy="unwind",parameter_mode="prototype";
  ExceptionPolicyFlags initial_exception_flags;
  DynamicExceptionSpecification dynamic_specification;
  std::vector<unsigned> parameters;
  // Private prelowering parameter-declaration facts, never type identity/wire.
  std::vector<bool> declaration_defaults;
};
struct SourceSemanticConstant {
  unsigned pack_type=0,pack_expression=0,pack_template=0;
  bool pack_is_type=false,pack_is_template=false;
  unsigned type=0,member_owner=0,type_operand=0,source_expression=0;
  unsigned aggregate_flags=0,aggregate_original_type=0,aggregate_expression=0;
  bool aggregate_template_const=false;
  unsigned enum_increment_original_type=0;
  bool enum_increment_forced_type=false;
  int builtin_operation=-1;
  bool standard_alignof=false;
  std::string kind,name,value,operation,error;
  int depth=0,position=0;
  bool pack=false;
  a_source_position location={};
  std::vector<unsigned> operands;
};
struct SourceEnumIncrement {
  a_type_ptr original_type=nullptr;
  a_constant_ptr previous=nullptr,one=nullptr;
  bool forced_type=false;
  std::string error;
};
struct SourceTemplateFormal {
  std::string kind,name;
  unsigned type=0,constant=0,templ=0,default_type=0,default_constant=0,default_template=0;
  bool pack=false,abbreviated=false;
  a_source_position position={};
};
struct SourceTemplateReference {
  unsigned namespace_owner=0;
  std::string name,error,kind="parameter",qualified_name;
  unsigned owner_type=0,prototype_type=0,header_constraint=0;
  int depth=0,position=0;
  bool pack=false;
  a_source_position location={};
  std::vector<SourceTemplateFormal> parameters;
};
struct SourceTemplateArgument {
  unsigned type=0,constant=0,templ=0;
  std::string kind;
  bool pack=false,pack_element=false,ellipsis=false,integer_pack=false,explicit_argument=false,injected_class=false;
  bool auto_parameter=false,decltype_auto_parameter=false;
  unsigned expansion=0,substituted_template=0;
};
inline SourceTemplateArgument capture_constraint_argument(a_template_arg_ptr,std::string&);
struct Enumerator {
  unsigned constant=0;
  std::string name,value;
  a_source_position position={};
};
struct Enum {
  unsigned underlying=0,owner=0;
  bool is_template=false,nonreal=false,prototype=false,specialized=false,originally_unnamed=false;
  std::string identity,error;
  bool scoped=false,complete=false,list_complete=false,explicit_base=false;
  bool needs_final_identity=false;
  unsigned occurrence=0;
  std::vector<Enumerator> enumerators;
};
struct Declaration {
  unsigned type=0;
  std::string name;
  a_source_position position={};
  // Private IL lookup keys only. The writer publishes a routine entity ID and
  // an explicit activation statement, never an EDG address or attribute text.
  a_routine_ptr cleanup=nullptr;
  bool has_initializer=false;
  unsigned initializer_kind=0; // Exact original an_init_kind before lowering.
};
struct Function {
  Declaration declaration;
  unsigned signature_type=0;
  std::string function_kind="ordinary";
  std::vector<unsigned> parameters;
  bool scope_captured=false;
  a_variable_ptr implicit_this=nullptr;
  std::vector<a_variable_ptr> parameter_variables;
  std::vector<bool> parameter_defaults;
};
// Source-only memory operand shape. This does not declare general VLA
// execution/source types. The original leaf and dimension facts are captured
// before stock lowering replaces variable array types with their leaf type.
struct AsmDimension {
  unsigned long long fixed_count=0;
  a_vla_dimension_ptr original=nullptr;
  a_variable_ptr saved_total=nullptr;
  a_source_position position={};
};
struct AsmOperand {
  std::string direction,constraint,name,shape="object",error;
  unsigned source_element_type=0;
  a_type_ptr element_type=nullptr;
  unsigned long long element_size=0,fixed_count=1;
  bool array_address=false;
  a_source_position position={};
  std::vector<AsmDimension> dimensions;
};
struct AsmStatement {
  std::string error;
  bool is_volatile=false,has_volatile_keyword=false,has_inline_keyword=false;
  bool clobbers_memory=false,lowering_observed=false;
  std::vector<an_asm_operand_ptr> order;
  std::map<an_asm_operand_ptr,AsmOperand> operands;
};
struct RecordField {
  a_field_ptr il=nullptr;
  unsigned source_type=0;
  std::string name,original_name;
  unsigned long long offset=0,explicit_alignment=0;
  bool is_packed=false;
  unsigned bit_offset=0,bit_width=0;
  bool bit_signed=false,is_mutable=false,no_unique_address=false,is_bit_field=false,has_initializer=false;
  a_source_position position={};
};
struct PrelowerBase {
  a_base_class_ptr il=nullptr;
  a_type_ptr original_type=nullptr;
  unsigned source_type_snapshot=0;
  a_source_position position={};
  bool is_virtual=false;
  std::string access;
};
struct Record {
  unsigned template_member_witness=0;
  unsigned namespace_owner=0;
  std::string name,identity,error,layout_kind;
  bool template_identity=false,complete=false,dependent=false,prototype=false,layout_known=false;
  unsigned template_prototype=0,member_template_owner=0,template_parameter=0,proxy_type=0;
  bool template_member_of_unknown_base=false,template_qualified_unknown_base=false;
  std::vector<SourceTemplateArgument> template_arguments;
  std::string template_error;
  std::vector<PrelowerBase> bases;
  bool has_virtual_functions=false,has_bases=false;
  unsigned source_occurrence=0;
  // Private original declaration ownership. Unlike the C backend, dot retains
  // function-local types and therefore need not give them promoted C names.
  a_routine_ptr local_owner=nullptr;
  unsigned long long local_discriminator=0;
  bool local_owner_external=false,local_owner_c_linkage=false,local_identity_observed=false;
  unsigned long long size=0,alignment=0,max_member_alignment=0,explicit_alignment=0;
  bool is_packed=false;
  a_source_position position={};
  std::vector<RecordField> fields;
};
struct RecordOwner {
  a_type_ptr owner=nullptr;
  a_field_ptr field=nullptr;
  unsigned ordinal=0;
};
// Independent provenance for the next record-layout profile. These snapshots
// do not participate in v14 emission or change its capture/type numbering.
struct PrelowerField {
  unsigned id=0,source_type_snapshot=0;
  a_field_ptr il=nullptr;
  a_type_ptr original_type=nullptr; // Private correspondence key, never wire.
  std::string name;
  a_source_position position={};
  unsigned long long byte_offset=0;
  unsigned bit_offset=0,bit_width=0;
  bool is_bit_field=false,is_mutable=false,no_unique_address=false,has_initializer=false;
};
struct ExceptionBaseSnapshot {
  unsigned source_type=0;
  unsigned long long offset=0;
  bool is_public=false,is_ambiguous=false,is_virtual=false;
};
struct PrelowerRecord {
  a_source_position position={};
  std::string name,error;
  bool complete=false,trivial_copy_known=false,trivial_copy=false;
  bool operations_known=false,bitwise_construction=false,bitwise_assignment=false,requires_return_storage=false,requires_parameter_storage=false;
  unsigned long long size=0,alignment=0;
  std::vector<PrelowerField> fields;
  std::vector<PrelowerBase> bases,virtual_bases;
  std::vector<ExceptionBaseSnapshot> exception_bases;
};
struct RoutineAbi {
  std::string proof,error;
  bool result_buffer=false,result_follows_this=false;
  a_type_ptr vtt_type=nullptr;
  std::vector<bool> indirect_parameters;
  std::vector<a_param_type_ptr> original_parameters;
};
struct DefaultAdapter {
  a_routine_ptr target=nullptr,primary=nullptr;
  bool implied_arguments=false,copy_constructor=false,has_arguments=false;
};
struct DefaultArgumentExecution {
  a_routine_ptr owner=nullptr,source_owner=nullptr,target=nullptr;
  a_scope_ptr scope=nullptr;
  an_object_lifetime_ptr root=nullptr;
  an_expr_node_ptr forwarding=nullptr;
  std::map<a_dynamic_init_ptr,a_dynamic_init_ptr> obligations,temporaries;
  std::map<an_object_lifetime_ptr,an_object_lifetime_ptr> lifetimes;
  std::map<a_dynamic_init_ptr,unsigned> copied_ordinals;
  std::map<an_object_lifetime_ptr,unsigned> sibling_ordinals;
  bool complete=false;
  std::string error;
};
struct LambdaEntry {
  a_routine_ptr target=nullptr;
  unsigned signature_type=0,closure_type=0;
  a_scope_ptr scope=nullptr;
  an_expr_node_ptr forwarding=nullptr;
  std::vector<a_variable_ptr> parameters;
};
struct ThunkWrapper {
  a_routine_ptr target=nullptr,overridden=nullptr;
  long long delta=0,vcall_index=0,return_delta=0,vbase_index=0;
  a_scope_ptr scope=nullptr;
  an_expr_node_ptr forwarding=nullptr;
  std::vector<a_variable_ptr> parameters;
};
// Private provenance for stock file-scope initialization lowering. An
// insertion interval belongs to one original storage initializer; the writer
// publishes only owned storage/action/statement IDs, never these IL pointers.
struct DynamicInitializer {
  a_dynamic_init_ptr original=nullptr;
  a_variable_ptr variable=nullptr;
  a_routine_ptr routine=nullptr;
  unsigned source_ordinal=0,ordinal=0;
  a_source_position position={};
  a_statement_ptr previous=nullptr;
  bool begun=false,complete=false,guarded=false;
  std::vector<a_statement_ptr> statements;
  std::string error;
};
struct GeneratedInitializer {
  unsigned long priority=0,needed_bit=0;
  bool is_thread_local=false,complete=false;
  std::vector<a_dynamic_init_ptr> initializers;
  std::string error;
};
struct SourceRecordTemporary {
  a_dynamic_init_ptr original=nullptr,initializer=nullptr;
  a_routine_ptr owner=nullptr;
  unsigned source_type=0,ordinal=0,initialization_kind=0;
  a_source_position position={};
};
struct RecordTemporaryBinding {
  a_dynamic_init_ptr original=nullptr,lowered=nullptr;
  a_routine_ptr owner=nullptr;
};
struct FieldInitializerExecution {
  unsigned constructor_ordinal=0;
  bool no_effect=false;
};
struct SourceInitializer {
  a_field_ptr field=nullptr;
  a_type_ptr field_owner=nullptr;
  a_routine_ptr field_destructor=nullptr;
  a_dynamic_init_ptr original=nullptr;
  a_variable_ptr variable=nullptr;
  a_routine_ptr owner=nullptr;
  an_object_lifetime_ptr expression_lifetime=nullptr;
  unsigned source_type=0,ordinal=0;
  a_source_position position={};
  std::string source_name,error;
};
struct StaticLifetimeSnapshot {
  an_object_lifetime_ptr original=nullptr,parent=nullptr;
  a_dynamic_init_ptr initializer=nullptr,parent_obligation=nullptr;
  a_routine_ptr owner=nullptr;
  unsigned sibling_ordinal=0;
  bool has_sibling_ordinal=false;
  std::string kind,error,entity_description;
  a_source_position position={};
};
struct StaticDestructionSnapshot {
  a_dynamic_init_ptr original=nullptr,initializer=nullptr;
  an_object_lifetime_ptr lifetime=nullptr;
  a_routine_ptr owner=nullptr,selected_routine=nullptr;
  a_variable_ptr variable=nullptr;
  unsigned ordinal=0,source_type=0;
  bool persistent=false,partial_aggregate=false,allocation=false,array_allocation=false,thread=false;
  a_source_position position={};
  std::string error;
};
struct StaticRegistrationSnapshot {
  a_dynamic_init_ptr original=nullptr;
  a_routine_ptr owner=nullptr,callback=nullptr;
  an_expr_node_ptr object=nullptr,callback_expression=nullptr,dso=nullptr;
  a_variable_ptr variable=nullptr;
  bool thread=false;
  std::string error;
};
struct GeneratedStaticDestructor {
  a_dynamic_init_ptr original=nullptr;
  a_variable_ptr variable=nullptr,parameter=nullptr;
  a_scope_ptr scope=nullptr;
  a_type_ptr element_type=nullptr;
  unsigned long long element_count=0,element_size=0;
  bool complete=false,array=false;
  std::string error;
};
struct LifetimeSnapshot {
  a_routine_ptr owner=nullptr;
  a_dynamic_init_ptr initializer=nullptr;
  an_object_lifetime_ptr original=nullptr,parent=nullptr;
  a_dynamic_init_ptr parent_obligation=nullptr;
  unsigned sibling_ordinal=0;
  std::string kind,error;
  a_source_position position={};
};
struct UninitializedAllocationSnapshot {
  a_new_delete_supplement_ptr original=nullptr;
  a_type_ptr allocated_type=nullptr;
  a_routine_ptr allocator=nullptr,deallocator=nullptr;
  bool placement=false,has_initializer=false;
  a_source_position position={};
  std::string error;
};
struct DestructionSnapshot {
  a_routine_ptr owner=nullptr,destructor=nullptr;
  a_dynamic_init_ptr initializer=nullptr;
  an_object_lifetime_ptr lifetime=nullptr;
  a_dynamic_init_ptr original=nullptr;
  unsigned ordinal=0,source_type=0;
  bool allocation_release=false,array_allocation=false,partial_aggregate=false;
  a_source_position position={};
  std::string error;
};
struct LocalStaticSnapshot {
  a_routine_ptr owner=nullptr;
  a_variable_ptr variable=nullptr,guard=nullptr;
  an_object_lifetime_ptr lifetime=nullptr;
  unsigned source_type=0;
  a_source_position position={};
  bool probe=false,acquire=false,release=false,thread_publish=false;
};
struct ArrayRuntimeSnapshot {
  a_routine_ptr owner=nullptr,callback=nullptr;
  a_dynamic_init_ptr original=nullptr;
  std::string operation;
};
struct CleanupPathPart {
  a_type_ptr type=nullptr;
  a_field_ptr field=nullptr;
  a_base_class_ptr base=nullptr;
  unsigned long long index=0;
};
struct CleanupSnapshot {
  a_routine_ptr owner=nullptr;
  a_handler_ptr release_handler=nullptr;
  an_expr_node_ptr throw_origin=nullptr;
  a_dynamic_init_ptr original=nullptr;
  a_variable_ptr variable=nullptr,conditional_flag=nullptr,vtt_argument=nullptr;
  a_type_ptr base_type=nullptr;
  bool indirect=false,base_subobject=false,array=false,delegation=false;
  bool allocation_release=false,guard_abort=false;
  a_type_ptr element_type=nullptr;
  unsigned long long element_count=0;
  std::vector<CleanupPathPart> path;
  std::string error;
};
struct VirtualBaseAdjustmentSnapshot {
  a_routine_ptr owner=nullptr;
  an_expr_node_ptr source_object=nullptr,lookup_object=nullptr,offset=nullptr;
  a_type_ptr derived_type=nullptr,base_type=nullptr;
  long long vtable_index=0;
};
struct RecordTransferSnapshot {
  a_routine_ptr owner=nullptr;
  an_expr_node_ptr target=nullptr,value=nullptr;
  std::string kind;
  a_variable_ptr constant_storage=nullptr;
  std::string constant_initialization_kind;
};
struct RecordStorageCopySnapshot {
  a_routine_ptr owner=nullptr;
  an_expr_node_ptr target=nullptr,value=nullptr;
  a_type_ptr type=nullptr;
  unsigned long long bytes=0;
  std::string kind;
};
struct EntityInitializerContext {
  a_routine_ptr owner=nullptr;
  a_dynamic_init_ptr allocation=nullptr;
  a_type_ptr element=nullptr;
};
struct GeneratedEntityInitializer {
  EntityInitializerContext origin;
  a_scope_ptr scope=nullptr;
  a_variable_ptr destination=nullptr,count=nullptr,model=nullptr;
  an_expr_node_ptr copy=nullptr;
  bool complete=false;
  std::string error;
};
struct GeneratedArrayInitializer {
  a_routine_ptr owner=nullptr,constructor=nullptr;
  a_dynamic_init_ptr allocation=nullptr;
  a_type_ptr element=nullptr;
  a_scope_ptr scope=nullptr;
  a_variable_ptr parameter=nullptr,model=nullptr;
  an_expr_node_ptr copy=nullptr,constructor_call=nullptr;
  bool complete=false;
  std::string error;
};
struct ArrayAllocationSnapshot {
  a_routine_ptr owner=nullptr,allocator=nullptr,deallocator=nullptr,constructor=nullptr,destructor=nullptr;
  a_dynamic_init_ptr original=nullptr,original_destruction=nullptr;
  a_type_ptr element_type=nullptr;
  a_constant_ptr padding=nullptr;
  std::string helper,error;
};
struct ArrayDeletionSnapshot {
  bool virtual_callback=false;
  unsigned long long vtable_index=0,cookie_size=0;
  a_routine_ptr owner=nullptr,deallocator=nullptr,destructor=nullptr;
  a_dynamic_init_ptr original=nullptr;
  a_type_ptr element_type=nullptr;
  a_constant_ptr padding=nullptr;
  std::string helper,error;
};
struct ThrowSnapshot {
  an_expr_node_ptr original=nullptr,initializer=nullptr;
  a_routine_ptr owner=nullptr,destructor=nullptr;
  a_type_ptr payload_type=nullptr,leaf_type=nullptr;
  unsigned source_type=0,leaf_source_type=0;
  unsigned long long size=0,alignment=0;
  bool rethrow=false,complete=false;
  std::string error;
};
struct NormalDestruction {
  a_routine_ptr owner=nullptr,callback=nullptr;
  a_dynamic_init_ptr original=nullptr;
};
struct HandlerSnapshot {
  unsigned source_type=0,match_type=0;
  a_variable_ptr parameter=nullptr;
};
struct TrySnapshot {
  a_routine_ptr owner=nullptr;
  a_try_supplement_ptr original=nullptr;
  std::vector<a_handler_ptr> handlers;
  bool function_try=false;
};
struct SourceAliasInstance {
  unsigned templ=0;bool prototype=false,nonreal=false,dependent=false,original_present=false;
  int minimum_arguments=-1;std::string error;
  std::vector<SourceTemplateArgument> arguments,original_arguments;
};
struct State {
  std::map<a_type_ptr,SourceAliasInstance> source_aliases;
  std::vector<SourceTemplateReference> source_templates;
  std::map<a_template_ptr,unsigned> source_template_ids;
  std::vector<SourceSemanticConstant> semantic_constants;
  std::map<a_constant_ptr,unsigned> semantic_constant_ids;
  std::map<an_expr_node_ptr,unsigned> semantic_constant_expr_ids;
  std::map<an_expr_node_ptr,SourceEnumIncrement> source_enum_increments;
  std::map<a_type_ptr,unsigned> type_ids;
  std::vector<Type> types;
  std::map<a_type_ptr,Enum> enums;
  std::set<a_type_ptr> capturing_enums;
  std::map<a_type_ptr,a_type_ptr> lowered_source_types;
  std::set<a_type_ptr> erased_code_pointer_types;
  std::map<a_type_ptr,std::string> finalized_enum_identities;
  std::map<std::pair<a_seq_number,a_column_number>,unsigned> enum_occurrence_counts;
  std::map<a_routine_ptr,Function> functions;
  std::map<a_dynamic_init_ptr,SourceInitializer> source_initializers;
  std::vector<a_dynamic_init_ptr> source_initializer_order;
  std::map<std::pair<a_dynamic_init_ptr,a_routine_ptr>,FieldInitializerExecution> field_initializer_executions;
  std::map<a_constructor_init_ptr,unsigned> field_initializer_ordinals;
  std::map<a_routine_ptr,unsigned> source_initializer_ordinals;
  std::map<an_object_lifetime_ptr,StaticLifetimeSnapshot> static_lifetimes;
  std::vector<an_object_lifetime_ptr> static_lifetime_order;
  std::map<a_dynamic_init_ptr,StaticDestructionSnapshot> static_destructions;
  std::vector<a_dynamic_init_ptr> static_destruction_order;
  std::map<an_expr_node_ptr,StaticRegistrationSnapshot> static_registrations;
  std::set<a_dynamic_init_ptr> static_no_effect,static_partial_terminal;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> field_partial_terminal,field_destructor_no_effect;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> automatic_partial_terminal;
  std::map<std::pair<a_dynamic_init_ptr,a_routine_ptr>,std::set<a_dynamic_init_ptr>> automatic_partial_terminal_copies;
  std::set<a_dynamic_init_ptr> automatic_no_effect_observed;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> automatic_destructor_no_effect;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> empty_allocation_initializations;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> zero_array_allocation_initializations;
  std::map<a_dynamic_init_ptr,UninitializedAllocationSnapshot> uninitialized_allocations;
  std::map<a_routine_ptr,GeneratedStaticDestructor> generated_static_destructors;
  std::map<a_routine_ptr,GeneratedArrayInitializer> generated_array_initializers;
  std::vector<EntityInitializerContext> entity_initializer_contexts;
  std::map<a_routine_ptr,GeneratedEntityInitializer> generated_entity_initializers;
  std::map<an_expr_node_ptr,a_routine_ptr> generated_entity_initializer_calls;
  std::map<a_routine_ptr,std::vector<a_dynamic_init_ptr>> adopted_initializers;
  std::map<std::pair<an_object_lifetime_ptr,a_routine_ptr>,a_dynamic_init_ptr> initializer_lifetime_adoptions;
  a_dynamic_init_ptr active_file_initializer=nullptr;
  std::map<an_object_lifetime_ptr,LifetimeSnapshot> lifetimes;
  std::map<a_routine_ptr,std::vector<an_object_lifetime_ptr>> lifetime_order;
  std::map<a_dynamic_init_ptr,DestructionSnapshot> destructions;
  std::map<a_dynamic_init_ptr,a_dynamic_init_ptr> source_array_allocation_elements;
  std::map<a_routine_ptr,std::vector<a_dynamic_init_ptr>> destruction_order;
  std::map<a_dynamic_init_ptr,a_dynamic_init_ptr> dynamic_init_copies;
  std::map<a_dynamic_init_ptr,SourceRecordTemporary> record_temporaries;
  std::vector<a_dynamic_init_ptr> record_temporary_order;
  std::map<std::pair<a_routine_ptr,a_dynamic_init_ptr>,unsigned> record_temporary_ordinals;
  std::map<a_variable_ptr,std::vector<RecordTemporaryBinding>> record_temporary_bindings;
  std::map<an_object_lifetime_ptr,an_object_lifetime_ptr> lifetime_copies;
  std::map<an_expr_node_ptr,ArrayAllocationSnapshot> array_allocation_calls;
  std::map<an_expr_node_ptr,std::pair<a_routine_ptr,a_routine_ptr>> array_callback_addresses;
  std::vector<ArrayAllocationSnapshot> array_allocation_contexts;
  std::map<an_expr_node_ptr,ArrayDeletionSnapshot> array_deletion_calls;
  std::vector<ArrayDeletionSnapshot> array_deletion_contexts;
  std::map<an_expr_node_ptr,ArrayRuntimeSnapshot> array_runtime_calls;
  std::map<a_routine_ptr,std::vector<an_expr_node_ptr>> array_runtime_order;
  std::map<a_routine_ptr,a_variable_ptr> vtt_parameters;
  std::map<a_constant_ptr,CleanupSnapshot> cleanup_regions;
  std::map<an_expr_node_ptr,std::pair<a_routine_ptr,a_dynamic_init_ptr>> allocation_internal_tries;
  std::map<a_variable_ptr,LocalStaticSnapshot> local_static_initializers;
  std::map<a_variable_ptr,a_variable_ptr> local_static_guard_variables;
  std::map<an_expr_node_ptr,std::pair<a_variable_ptr,std::string>> local_static_guard_operations;
  std::map<a_dynamic_init_ptr,std::pair<a_routine_ptr,a_variable_ptr>> local_static_guard_cleanups;
  std::vector<CleanupSnapshot> payload_subobjects;
  std::map<a_try_supplement_ptr,TrySnapshot> tries;
  std::map<a_handler_ptr,HandlerSnapshot> handlers;
  std::vector<a_handler_ptr> active_handlers;
  std::map<a_dynamic_init_ptr,std::pair<a_routine_ptr,a_handler_ptr>> handler_releases;
  std::map<an_expr_node_ptr,RecordTransferSnapshot> record_transfers;
  std::map<an_expr_node_ptr,RecordStorageCopySnapshot> record_storage_copies;
  std::map<a_variable_ptr,a_constant_ptr> pooled_record_constants;
  std::map<an_expr_node_ptr,VirtualBaseAdjustmentSnapshot> virtual_base_adjustments;
  std::map<an_expr_node_ptr,ThrowSnapshot> throws;
  std::map<an_expr_node_ptr,an_expr_node_ptr> throw_addresses,throw_markers;
  std::vector<an_expr_node_ptr> active_throws;
  std::vector<NormalDestruction> normal_destruction_contexts;
  std::map<an_expr_node_ptr,NormalDestruction> normal_destruction_calls;
  std::map<a_routine_ptr,std::vector<an_expr_node_ptr>> normal_destruction_order;
  bool file_initializers_observed=false,file_initializer_destructions=false;
  std::map<a_dynamic_init_ptr,DynamicInitializer> dynamic_initializers;
  std::map<a_routine_ptr,GeneratedInitializer> generated_initializers;
  std::map<unsigned,RoutineAbi> routine_abis;
  std::map<unsigned,RoutineExceptionResolution> routine_exception_resolutions;
  std::set<a_routine_ptr> requested_defaulted_exception_policies;
  std::map<a_routine_ptr,a_variable_ptr> result_parameters;
  std::set<a_routine_ptr> runtime_declarations;
  std::map<a_routine_ptr,DefaultAdapter> default_adapters;
  std::map<a_routine_ptr,DefaultArgumentExecution> default_argument_executions;
  std::map<a_routine_ptr,unsigned> dynamic_exception_boundaries;
  std::map<a_routine_ptr,std::string> dynamic_exception_boundary_errors;
  std::vector<a_routine_ptr> active_default_argument_copies;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> default_argument_obligation_adoptions;
  std::set<std::pair<an_object_lifetime_ptr,a_routine_ptr>> default_argument_lifetime_adoptions;
  std::set<std::pair<a_dynamic_init_ptr,a_routine_ptr>> default_argument_temporary_adoptions;
  std::map<a_routine_ptr,ThunkWrapper> thunk_wrappers;
  std::map<a_routine_ptr,LambdaEntry> lambda_entries;
  std::map<a_type_ptr,std::pair<a_type_ptr,a_routine_ptr>> lambda_prototype_recipes;
  std::map<an_asm_entry_ptr,AsmStatement> asm_statements;
  std::map<a_variable_ptr,Declaration> variables;
  std::map<a_type_ptr,Record> records;
  std::map<a_type_ptr,unsigned long long> requested_record_alignments;
  std::map<a_field_ptr,unsigned long long> requested_field_alignments;
  std::map<a_field_ptr,unsigned long long> observed_field_alignments;
  std::map<a_type_ptr,std::string> finalized_record_identities;
  std::map<a_type_ptr,RecordOwner> record_owners;
  std::map<a_type_ptr,a_type_ptr> nested_type_declarations;
  std::map<a_type_ptr,std::string> owned_local_record_identities;
  std::set<a_type_ptr> capturing_records;
  std::map<std::pair<a_seq_number,a_column_number>,unsigned> record_occurrence_counts;
  std::map<a_type_ptr,unsigned> record_occurrences;
  std::map<a_type_ptr,PrelowerRecord> prelower_records;
  std::set<a_type_ptr> abi_records;
  std::map<a_type_ptr,std::string> abi_record_keys;
  std::map<a_field_ptr,unsigned> original_field_ids;
  std::map<a_field_ptr,a_field_ptr> copied_fields;
  std::map<a_field_ptr,std::string> generated_field_roles;
  std::map<a_field_ptr,a_base_class_ptr> generated_base_fields;
  std::map<a_type_ptr,a_type_ptr> observed_subobjects;
  unsigned next_original_field=1;
};
inline State &state(){static State value;return value;}
inline void observe_lambda_conversion_prototype(a_type_ptr copied,a_routine_ptr selected) {
  if(!enabled() || !copied || !selected)return;
  auto value=std::make_pair(selected->type,selected);
  auto inserted=state().lambda_prototype_recipes.emplace(copied,value);
  if(!inserted.second && inserted.first->second!=value)inserted.first->second={nullptr,nullptr};
}
inline void observe_lambda_prototype_type_copy(a_type_ptr original,a_type_ptr copied) {
  if(!enabled() || !original || !copied || original==copied)return;
  auto &recipes=state().lambda_prototype_recipes;auto found=recipes.find(original);
  if(found==recipes.end())return;
  auto inserted=recipes.emplace(copied,found->second);
  if(!inserted.second && inserted.first->second!=found->second)inserted.first->second={nullptr,nullptr};
}

inline void observe_class_layout_directive(a_type_ptr type) {
  if(!enabled())return;
  // Snapshot the requested directive before do_class_layout normalizes it.
  // Repeated layout must not replace an ignored request with its final value.
  state().requested_record_alignments.emplace(type,type->alignment_set_explicitly?type->alignment:0);
}
inline void observe_field_layout_directive(a_field_ptr field) {
  if(enabled())state().requested_field_alignments.emplace(field,field->alignment);
}
inline void observe_field_layout_alignment(a_field_ptr field,a_boolean for_alignof,a_targ_alignment alignment) {
  if(enabled() && !for_alignof)state().observed_field_alignments[field]=alignment;
}
inline std::string spelling(const a_source_correspondence &source) {
  if(!source.name)return "";
#if NEED_NAME_MANGLING
  if(source.unnamed_entity_given_fabricated_name)return "";
  if(source.name_has_been_mangled && source.unmangled_name_or_mangled_encoding)
    return source.unmangled_name_or_mangled_encoding;
#endif
  return source.name;
}
#include "dot_gen_object_source_namespaces.h"
inline unsigned capture_type(a_type_ptr t);
inline DynamicExceptionSpecification capture_dynamic_exception_specification(an_exception_specification_ptr specification) {
  DynamicExceptionSpecification value;
  if(!specification || specification->is_noexcept || specification->throw_any || specification->indeterminate ||
     specification->arg_cached || specification->copy_from_prototype) {
    value.error="dynamic exception specification is unresolved or has another kind";return value;
  }
  for(auto item=specification->variant.exception_specification_type_list;item;item=item->next) {
    if(item->is_pack_expansion) {value.error="dynamic exception specification retains an unexpanded type pack";break;}
    value.declared_types.push_back(capture_type(item->type));
    auto plain=skip_typerefs(item->type);
    auto match=plain->kind==tk_pointer && plain->variant.pointer.is_reference?plain->variant.pointer.type:item->type;
    value.match_types.push_back(capture_type(match));
  }
  return value;
}

inline void capture_field_initializer(a_field_ptr field,a_type_ptr owner);
inline void trace_routine_exception_policy(const char *phase,unsigned id,a_type_ptr type,const std::string &cached) {
  static const bool trace=std::getenv("DOTOBJ_TRACE_ROUTINE_EXCEPTION_POLICY")!=nullptr;
  if(!trace)return;
  auto extra=type->variant.routine.extra_info;auto spec=extra->exception_specification;
  std::fprintf(stderr,"DOTOBJ_EXCEPTION_POLICY phase=%s id=%u type=%p receiver=%s receiver_ptr=%p cached=%s present=%d indeterminate=%d arg_cached=%d copy_from_prototype=%d is_noexcept=%d throw_any=%d assoc_routine=%s defaulted=%d\n",
    phase,id,static_cast<void*>(type),extra->this_class?spelling(extra->this_class->source_corresp).c_str():"",
    static_cast<void*>(extra->this_class),cached.c_str(),spec!=nullptr,
    spec?int(spec->indeterminate):0,spec?int(spec->arg_cached):0,spec?int(spec->copy_from_prototype):0,
    spec?int(spec->is_noexcept):0,spec?int(spec->throw_any):0,
    extra->assoc_routine?spelling(extra->assoc_routine->source_corresp).c_str():"",
    extra->assoc_routine?int(extra->assoc_routine->is_defaulted):0);
}

inline unsigned capture_semantic_constant(a_constant_ptr constant);
// Exact all-type operand families from a_builtin_operation_kind.  A negative
// arity means one or more type arguments; zero leaves the family explicitly gated.
inline int source_builtin_trait_arity(a_builtin_operation_kind kind) {
  switch(kind) {
    case bok_has_assign:return 1;
    case bok_has_copy:return 1;
    case bok_has_nothrow_assign:return 1;
    case bok_has_nothrow_constructor:return 1;
    case bok_has_nothrow_copy:return 1;
    case bok_has_trivial_assign:return 1;
    case bok_has_trivial_constructor:return 1;
    case bok_has_trivial_copy:return 1;
    case bok_has_trivial_destructor:return 1;
    case bok_has_user_destructor:return 1;
    case bok_has_virtual_destructor:return 1;
    case bok_is_abstract:return 1;
    case bok_is_base_of:return 2;
    case bok_is_class:return 1;
    case bok_is_convertible_to:return 2;
    case bok_is_empty:return 1;
    case bok_is_enum:return 1;
    case bok_is_pod:return 1;
    case bok_is_polymorphic:return 1;
    case bok_is_union:return 1;
    case bok_types_compatible:return 2;
    case bok_is_trivial:return 1;
    case bok_is_standard_layout:return 1;
    case bok_is_trivially_copyable:return 1;
    case bok_is_literal_type:return 1;
    case bok_has_trivial_move_constructor:return 1;
    case bok_has_trivial_move_assign:return 1;
    case bok_has_nothrow_move_assign:return 1;
    case bok_is_constructible:return -1;
    case bok_is_nothrow_constructible:return -1;
    case bok_has_finalizer:return 1;
    case bok_is_delegate:return 1;
    case bok_is_interface_class:return 1;
    case bok_is_ref_array:return 1;
    case bok_is_ref_class:return 1;
    case bok_is_sealed:return 1;
    case bok_is_simple_value_class:return 1;
    case bok_is_value_class:return 1;
    case bok_is_final:return 1;
    case bok_is_trivially_constructible:return -1;
    case bok_is_destructible:return 1;
    case bok_is_nothrow_destructible:return 1;
    case bok_is_trivially_destructible:return 1;
    case bok_is_nothrow_assignable:return 2;
    case bok_is_trivially_assignable:return 2;
    case bok_is_valid_winrt_type:return 1;
    case bok_is_win_class:return 1;
    case bok_is_win_interface:return 1;
    case bok_is_assignable:return 2;
    case bok_is_trivially_copy_assignable:return 1;
    case bok_is_assignable_no_precondition_check:return 2;
    case bok_has_unique_object_representations:return 1;
    case bok_is_aggregate:return 1;
    case bok_reference_binds_to_temporary:return 2;
    case bok_is_same:return 2;
    case bok_is_same_as:return 2;
    case bok_is_function:return 1;
    case bok_is_layout_compatible:return 2;
    case bok_is_pointer_interconvertible_base_of:return 2;
    case bok_is_array:return 1;
    case bok_array_rank:return 1;
    case bok_is_arithmetic:return 1;
    case bok_is_complete_type:return 1;
    case bok_is_compound:return 1;
    case bok_is_const:return 1;
    case bok_is_floating_point:return 1;
    case bok_is_fundamental:return 1;
    case bok_is_integral:return 1;
    case bok_is_lvalue_reference:return 1;
    case bok_is_member_function_pointer:return 1;
    case bok_is_member_object_pointer:return 1;
    case bok_is_member_pointer:return 1;
    case bok_is_object:return 1;
    case bok_is_pointer:return 1;
    case bok_is_reference:return 1;
    case bok_is_rvalue_reference:return 1;
    case bok_is_scalar:return 1;
    case bok_is_signed:return 1;
    case bok_is_unsigned:return 1;
    case bok_is_void:return 1;
    case bok_is_volatile:return 1;
    case bok_is_bounded_array:return 1;
    case bok_is_unbounded_array:return 1;
    case bok_is_referenceable:return 1;
    case bok_is_nothrow_convertible:return 2;
    case bok_reference_constructs_from_temporary:return 2;
    case bok_reference_converts_from_temporary:return 2;
    case bok_is_convertible:return 2;
    case bok_is_trivially_equality_comparable:return 1;
    case bok_is_scoped_enum:return 1;
    case bok_is_trivially_relocatable:return 1;
    case bok_is_invocable:return -1;
    case bok_is_nothrow_invocable:return -1;
    case bok_is_bitwise_cloneable:return 1;
    case bok_builtin_is_virtual_base_of:return 2;
    case bok_builtin_is_implicit_lifetime:return 1;
    case bok_builtin_lt_synthesizes_from_spaceship:return 2;
    case bok_builtin_gt_synthesizes_from_spaceship:return 2;
    case bok_builtin_le_synthesizes_from_spaceship:return 2;
    case bok_builtin_ge_synthesizes_from_spaceship:return 2;
    case bok_builtin_is_structural:return 1;
    default:return 0;
  }
}
inline unsigned capture_constraint_expression(an_expr_node_ptr,unsigned);
inline unsigned current_source_parameter_binder();
inline bool capture_source_class_specialization(a_type_ptr,a_template_ptr,std::string&);
inline void observe_dependent_enum_increment(an_expr_node_ptr expression,a_type_ptr original_type,bool forced_type,a_type_ptr unknown_type) {
  if(!enabled())return;
  SourceEnumIncrement value;value.original_type=original_type;value.forced_type=forced_type;
  auto operands=expression && expression->kind==enk_operation?expression->variant.operation.operands:nullptr;
  if(!expression || expression->kind!=enk_operation || expression->variant.operation.kind!=eok_add ||
      !expression->compiler_generated || expression->type!=original_type || !operands || !operands->next || operands->next->next ||
      operands->kind!=enk_constant || operands->next->kind!=enk_constant)
    value.error="dependent enum increment lost its exact stock generated addition";
  else {
    value.previous=node_constant(operands);value.one=node_constant(operands->next);
    if(!value.previous || value.previous->kind!=ck_template_param || value.previous->variant.template_param.kind!=tpck_expression ||
        !value.previous->variant.template_param.variant.expr || value.one->kind!=ck_integer ||
        value.previous->variant.template_param.variant.expr->type!=original_type ||
        (forced_type?value.previous->type!=unknown_type:value.previous->type!=original_type))
      value.error="dependent enum increment lost its original previous-value wrapper/type";
  }
  if(!state().source_enum_increments.emplace(expression,value).second)
    state().source_enum_increments.at(expression).error="duplicate stock dependent enum increment observation";
}
inline unsigned capture_source_template(a_template_ptr);
inline unsigned capture_semantic_constant_expr(an_expr_node_ptr expression) {
  auto &s=state();if(!expression)return 0;
  if(expression->kind==enk_constant)return capture_semantic_constant(node_constant(expression));
  auto prior=s.semantic_constant_expr_ids.find(expression);if(prior!=s.semantic_constant_expr_ids.end())return prior->second;
  unsigned id=unsigned(s.semantic_constants.size()+1);s.semantic_constant_expr_ids[expression]=id;s.semantic_constants.emplace_back();
  SourceSemanticConstant value;value.kind="operation";value.type=capture_type(expression->type);value.location=expression->position;
  if(expression->kind==enk_type_operand) {
    value.kind="type-operand";
    if(!expression->variant.type_operand.type)value.error="source type operand has no original type";
    else value.type_operand=capture_type(expression->variant.type_operand.type);
  } else if(expression->kind==enk_builtin_operation) {
    value.kind="builtin-trait";
    auto builtin=expression->variant.builtin_operation.kind;
    value.builtin_operation=int(builtin);value.operation=builtin_operation_names[builtin];
    int arity=source_builtin_trait_arity(builtin);unsigned count=0;
    if(!arity)value.error="dependent builtin operation needs additional owned operand roles";
    for(auto operand=expression->variant.builtin_operation.operands;operand;operand=operand->next) {
      ++count;
      if(operand->kind!=enk_type_operand)value.error="source type trait requires its original ordered type operands";
      value.operands.push_back(capture_semantic_constant_expr(operand));
    }
    if(arity && (arity>0?count!=unsigned(arity):count==0))value.error="source type trait has unexpected original arity";
  } else if(expression->kind==enk_sizeof || expression->kind==enk_alignof || expression->kind==enk_datasizeof) {
    value.kind=expression->kind==enk_sizeof?"sizeof":expression->kind==enk_alignof?"alignof":"datasizeof";
    const auto &query=expression->variant.sizeof_info;
    value.standard_alignof=query.is_std_alignof;
    if(query.is_type)value.type_operand=capture_type(query.variant.type);
    else value.operands.push_back(capture_semantic_constant_expr(query.variant.expr));
  } else if(expression->kind==enk_sizeof_pack) {
    // cp_gen_be::gen_expression preserves these three original operand roles.
    value.kind="sizeof-pack";const auto &query=expression->variant.sizeof_pack;
    value.pack_is_type=query.is_type;value.pack_is_template=query.is_template_template;
    if(query.is_template_template)value.pack_template=capture_source_template(query.variant.templ);
    else if(query.is_type)value.pack_type=capture_type(query.variant.type);
    else value.pack_expression=capture_constraint_expression(query.variant.expr,current_source_parameter_binder());
    if(!value.pack_type && !value.pack_template && !value.pack_expression)value.error="original pack-size query has no operand";
  } else if(expression->kind==enk_variable) {
    value.kind="source-expression";
    value.source_expression=capture_constraint_expression(expression,current_source_parameter_binder());
  } else if(expression->kind!=enk_operation)value.error="dependent constant expression needs an owned source expression family";
  else {
    const auto &operation=expression->variant.operation;
    switch(operation.kind) {
      case eok_add:value.operation="add";break;case eok_subtract:value.operation="sub";break;
      case eok_multiply:value.operation="mul";break;case eok_divide:value.operation="div";break;case eok_remainder:value.operation="rem";break;
      case eok_shiftl:value.operation="shl";break;case eok_shiftr:value.operation="shr";break;
      case eok_and:value.operation="bit-and";break;case eok_or:value.operation="bit-or";break;case eok_xor:value.operation="bit-xor";break;
      case eok_eq:value.operation="eq";break;case eok_ne:value.operation="ne";break;case eok_lt:value.operation="lt";break;
      case eok_le:value.operation="le";break;case eok_gt:value.operation="gt";break;case eok_ge:value.operation="ge";break;
      case eok_land:value.operation="and";break;case eok_lor:value.operation="or";break;
      case eok_negate:value.operation="neg";break;case eok_unary_plus:value.operation="plus";break;
      case eok_not:value.operation="not";break;case eok_complement:value.operation="bit-not";break;
      case eok_cast:value.operation="cast";break;
      case eok_question:value.operation="conditional";break;
      default:value.error="dependent constant operation not yet represented";break;
    }
    for(auto operand=operation.operands;operand;operand=operand->next)value.operands.push_back(capture_semantic_constant_expr(operand));
  }
  if(!value.error.empty()) {
    value.error+=" [expression-kind="+std::to_string(int(expression->kind))+
      ",operator-kind="+(expression->kind==enk_operation?std::to_string(int(expression->variant.operation.kind)):
      expression->kind==enk_builtin_operation?std::to_string(int(expression->variant.builtin_operation.kind)):"none")+
      ",operator-name="+value.operation+"]";
  }
  auto increment=s.source_enum_increments.find(expression);
  if(increment!=s.source_enum_increments.end()) {
    const auto &observed=increment->second;
    if(!observed.error.empty())value.error=observed.error;
    else if(value.kind!="operation" || value.operation!="add" || value.operands.size()!=2 ||
        value.operands[0]!=capture_semantic_constant(observed.previous) || value.operands[1]!=capture_semantic_constant(observed.one))
      value.error="dependent enum increment operands differ from their exact creation observation";
    else {
      value.enum_increment_original_type=capture_type(observed.original_type);
      value.enum_increment_forced_type=observed.forced_type;
    }
  }
  s.semantic_constants[id-1]=value;return id;
}
inline unsigned capture_semantic_constant(a_constant_ptr constant) {
  if(!constant)return 0;
  auto &s=state();auto prior=s.semantic_constant_ids.find(constant);if(prior!=s.semantic_constant_ids.end())return prior->second;
  unsigned id=unsigned(s.semantic_constants.size()+1);s.semantic_constant_ids[constant]=id;s.semantic_constants.emplace_back();
  SourceSemanticConstant value;value.type=capture_type(constant->type);
  value.name=spelling(constant->source_corresp);value.location=constant->source_corresp.decl_position;
  if(constant->kind==ck_integer) {
    value.kind="integer";value.name.clear();value.value=decimal_str_for_integer_constant(constant).as_temp_characters();
  } else if(constant->kind==ck_aggregate) {
    value.kind="aggregate";value.name.clear();
    value.aggregate_original_type=capture_type(constant->orig_type);
    value.aggregate_template_const=constant->variant.aggregate.added_const_for_template_param;
    if(constant->variant.aggregate.has_dynamic_init_component || constant->has_been_prelowered || constant->vptr_has_been_lowered ||
       constant->initializes_empty_object || constant->local_expr_ref || constant->uses_designated_initializers)
      value.error="source aggregate constant requires original dynamic/designated/subobject initializer mapping";
    if(constant->explicit_braces_on_aggregate)value.aggregate_flags|=1U<<0;
    if(constant->explicit_parentheses_on_aggregate)value.aggregate_flags|=1U<<1;
    if(constant->partial_aggr_value)value.aggregate_flags|=1U<<2;
    if(constant->is_partially_initialized)value.aggregate_flags|=1U<<3;
    if(constant->implicit_aggr_element)value.aggregate_flags|=1U<<4;
    if(constant->is_compound_literal)value.aggregate_flags|=1U<<5;
    if(constant->is_result_of_constexpr_call)value.aggregate_flags|=1U<<6;
    if(constant->is_generic_initializer)value.aggregate_flags|=1U<<7;
    if(constant->is_implicit_initialization)value.aggregate_flags|=1U<<8;
    if(constant->constant_for_base_class)value.aggregate_flags|=1U<<9;
    if(constant->constant_for_base_class_from_constexpr_folding)value.aggregate_flags|=1U<<10;
    if(constant->implicit_cast)value.aggregate_flags|=1U<<11;
    if(constant->explicit_cast_applied)value.aggregate_flags|=1U<<12;
    if(constant->is_reinterpret_cast)value.aggregate_flags|=1U<<13;
    if(constant->is_reinterpret_like_cast)value.aggregate_flags|=1U<<14;
    if(constant->uses_designated_initializers)value.aggregate_flags|=1U<<15;
    if(constant->expr)value.aggregate_expression=capture_constraint_expression(constant->expr,current_source_parameter_binder());
    a_constant_ptr last=nullptr;
    for(auto child=constant->variant.aggregate.first_constant;child;child=child->next) {
      value.operands.push_back(capture_semantic_constant(child));last=child;
    }
    if(last!=constant->variant.aggregate.last_constant)value.error="source aggregate constant has inconsistent original element boundaries";
  } else if(constant->kind==ck_template_param) {
    const auto &parameter=constant->variant.template_param;
    if(parameter.kind==tpck_param) {
      value.kind="parameter";value.depth=parameter.variant.coordinates.depth;value.position=parameter.variant.coordinates.position;
      value.pack=parameter.is_pack;
      if(value.depth<0 || value.position<=0 || !value.location.seq)value.error="non-type parameter lacks original declaration coordinates";
    } else if(parameter.kind==tpck_member) {
      value.kind="member";
      auto owner=constant->source_corresp.is_class_member?parent_class_of(constant):nullptr;
      if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
      if(!owner || value.name.empty() || parameter.is_pack)
        value.error="dependent constant member lacks exact named owner";
      else value.member_owner=capture_type(owner);
    } else if(parameter.kind==tpck_sizeof || parameter.kind==tpck_alignof || parameter.kind==tpck_datasizeof) {
      value.kind=parameter.kind==tpck_sizeof?"sizeof":parameter.kind==tpck_alignof?"alignof":"datasizeof";value.name.clear();
      const auto &query=parameter.variant.templ_sizeof;value.standard_alignof=query.is_std_alignof;
#if PROTOTYPE_INSTANTIATIONS_IN_IL
      if(parameter.local_expr_ref)value.error="dependent type query requires owned local expression reference";
      else
#endif
      if(query.expr)value.operands.push_back(capture_semantic_constant_expr(query.expr));
      else if(query.type)value.type_operand=capture_type(query.type);
      else value.error="dependent type query has no original operand";
    } else if(parameter.kind==tpck_expression &&
#if PROTOTYPE_INSTANTIATIONS_IN_IL
              !parameter.local_expr_ref &&
#endif
              parameter.variant.expr) {
      value.kind="expression";value.name.clear();value.operands.push_back(capture_semantic_constant_expr(parameter.variant.expr));
    } else value.error="dependent constant requires additional original semantic representation";
  } else value.error="source template constant family not yet represented";
  if(!value.error.empty()) {
    a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
    conv_seq_to_file_and_line(value.location.seq,&display,&full,&line,&at_end);
    value.error+=" [constant-kind="+std::to_string(int(constant->kind))+",template-kind="+
      (constant->kind==ck_template_param?std::to_string(int(constant->variant.template_param.kind)):"none")+
      ",name="+value.name+",source="+std::string(full?full:display?display:"")+":"+std::to_string(line)+":"+
      std::to_string(value.location.column)+"]";
  }
  s.semantic_constants[id-1]=value;return id;
}

inline unsigned capture_source_template(a_template_ptr templ) {
  if(!templ)return 0;
  auto &s=state();auto previous=s.source_template_ids.find(templ);
  if(previous!=s.source_template_ids.end())return previous->second;
  unsigned id=unsigned(s.source_templates.size()+1);s.source_template_ids[templ]=id;s.source_templates.emplace_back();
  SourceTemplateReference value;value.name=spelling(templ->source_corresp);value.location=templ->source_corresp.decl_position;
  value.depth=templ->coordinates.depth;value.position=templ->coordinates.position;value.pack=templ->is_pack;
  auto declaration=templ->template_decl;
  if(templ->kind==templk_class && templ->prototype_instantiation.type &&
     templ->prototype_instantiation.type->kind==tk_typeref &&
     templ->prototype_instantiation.type->variant.typeref.kind==trk_is_template_alias) {
    value.kind="alias";value.qualified_name=value.name;
    value.namespace_owner=capture_source_namespace_owner(templ->source_corresp);
    if(templ->source_corresp.is_class_member)value.owner_type=capture_type(parent_class_of(templ));
    for(auto scope=templ->source_corresp.parent_scope;scope;scope=scope->parent) {
      if(scope->kind==sck_namespace) {
        auto ns=scope->variant.assoc_namespace;
        if(!ns)value.error="alias template has absent original namespace";
        else if(!spelling(ns->source_corresp).empty())value.qualified_name=spelling(ns->source_corresp)+"::"+value.qualified_name;
      } else if(scope->kind!=sck_file && scope->kind!=sck_class_struct_union)value.error="alias template needs original lexical owner";
    }
    value.prototype_type=capture_type(templ->prototype_instantiation.type);
    if(declaration && declaration->constraint.requires_clause)value.header_constraint=capture_constraint_expression(declaration->constraint.requires_clause->constraint,0);
  } else if(templ->kind!=templk_template_template_param)
    value.error="source template reference kind requires its owned declaration [kind="+std::to_string(int(templ->kind))+",name="+value.name+"]";
  if(!declaration || (value.kind=="parameter" && (value.depth<0 || value.position<=0)) || !value.location.seq)
    value.error="template template parameter lacks its original declaration/header coordinates";
  else if(
#if MICROSOFT_EXTENSIONS_ALLOWED
    declaration->is_generic ||
#endif
    (value.kind=="parameter" && declaration->constraint.requires_clause))
    value.error="source template parameter constraints require an owned semantic graph";
  else for(auto parameter=declaration->param_list;parameter;parameter=parameter->next) {
    SourceTemplateFormal formal;formal.name=spelling(parameter->source_corresp);formal.position=parameter->source_corresp.decl_position;
    formal.pack=parameter->is_pack;formal.abbreviated=parameter->is_abbreviated;
    if(parameter->kind==tpk_type) {
      formal.kind="type";formal.type=capture_type(parameter->variant.type.ptr);
      formal.default_type=capture_type(parameter->variant.type.default_arg_type);
    } else if(parameter->kind==tpk_nontype) {
      formal.kind="constant";formal.constant=capture_semantic_constant(parameter->variant.nontype.constant);
      formal.default_constant=capture_semantic_constant(parameter->variant.nontype.default_arg_constant);
    } else if(parameter->kind==tpk_template) {
      formal.kind="template";formal.templ=capture_source_template(parameter->variant.templ.class_template);
      formal.default_template=capture_source_template(parameter->variant.templ.default_arg_template);
    } else value.error="source template formal parameter has no owned semantic kind";
    value.parameters.push_back(formal);
  }
  s.source_templates[id-1]=value;return id;
}

inline void capture_enum(a_type_ptr t) {
  auto &s=state();auto old=s.enums.find(t);
  if(old!=s.enums.end() && old->second.list_complete)return;
  if(!s.capturing_enums.insert(t).second)return;
  struct EnumCaptureGuard {State &state;a_type_ptr type;~EnumCaptureGuard(){state.capturing_enums.erase(type);}} guard{s,t};
  Enum value;
  if(old==s.enums.end()) {
    auto key=std::make_pair(t->source_corresp.decl_position.seq,t->source_corresp.decl_position.column);
    value.occurrence=s.enum_occurrence_counts[key]++;
  } else value.occurrence=old->second.occurrence;
  // Reserve before capturing the underlying type, whose declaration metadata
  // may refer back to this enum through a typedef.
  s.enums[t]=value;
  auto supplement=integer_type_supp(t);
  value.scoped=t->variant.integer.is_scoped_enum;
  value.complete=!t->incomplete;
  value.list_complete=supplement->enumerator_list_complete;
  value.explicit_base=t->variant.integer.has_explicit_enum_base;
  value.underlying=capture_type(supplement->base_type?supplement->base_type:
                               integer_type(t->variant.integer.int_kind));
  std::string qualified=spelling(t->source_corresp);
  bool local=qualified.empty() || t->source_corresp.is_local_to_function ||
             t->source_corresp.enclosing_routine || t->declared_in_function_prototype;
  value.is_template=t->variant.integer.is_template_enum;
  value.nonreal=t->variant.integer.is_nonreal;
  value.prototype=t->variant.integer.is_prototype_instantiation;
  value.specialized=t->variant.integer.is_specialized;
  value.originally_unnamed=t->variant.integer.originally_unnamed;
  value.needs_final_identity=value.is_template;
  if(t->source_corresp.is_class_member)value.owner=capture_type(parent_class_of(t));
  for(auto scope=t->source_corresp.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;auto part=ns?spelling(ns->source_corresp):std::string{};
      if(!ns || ns->has_internal_linkage || part.empty())local=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind==sck_class_struct_union) {
      auto owner=scope->variant.assoc_type;
      auto part=owner?spelling(owner->source_corresp):std::string{};
      if(part.empty())local=true;
      if(owner && owner->variant.class_struct_union.is_template_class)value.needs_final_identity=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind!=sck_file)local=true;
  }
  value.needs_final_identity=value.needs_final_identity || (!C_mode() && local);
  value.identity=(local?"local-enum:":"enum:")+qualified;
  for(auto con=enum_constants(t);con;con=con->next) {
    if(con->kind!=ck_integer && !(value.is_template && con->kind==ck_template_param)) {
      value.error="enumerator lacks resolved integer value: enum="+qualified+
        " enumerator="+spelling(con->source_corresp)+" constant-kind="+std::to_string(int(con->kind))+
        " template-enum="+std::to_string(int(t->variant.integer.is_template_enum));
      if(getenv("EDG_DOTOBJ_DIAG_SOURCE_ERRORS")) {
        a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
        const auto &position=con->source_corresp.decl_position;
        if(position.seq)conv_seq_to_file_and_line(position.seq,&display,&full,&line,&at_end);
        value.error+=" original="+std::string(full?full:display?display:"")+":"+
          std::to_string(line)+":"+std::to_string(position.column);
      }
      break;
    }
    Enumerator enumerator;enumerator.name=spelling(con->source_corresp);
    enumerator.position=con->source_corresp.decl_position;
    // This is the same arbitrary-precision conversion used by EDG's name
    // mangler. It preserves signed values and all 128 bits without a host cast.
    if(con->kind==ck_integer)enumerator.value=decimal_str_for_integer_constant(con).as_temp_characters();
    else enumerator.constant=capture_semantic_constant(con);
    value.enumerators.push_back(enumerator);
  }
  s.enums[t]=value;
}
inline void capture_record(a_type_ptr t) {
  auto &s=state();auto old=s.records.find(t);
  if(old!=s.records.end() && (old->second.size || t->incomplete))return;
  if(!s.capturing_records.insert(t).second)return;
  struct CaptureGuard {
    State &state; a_type_ptr type;
    ~CaptureGuard(){state.capturing_records.erase(type);}
  } guard{s,t};
  // Reserve first: self pointers and mutually recursive pointer fields are
  // ordinary nominal record references, not infinitely expanded structures.
  s.records[t]=Record{};
  Record value;value.name=spelling(t->source_corresp);value.position=t->source_corresp.decl_position;
  value.namespace_owner=capture_source_namespace_owner(t->source_corresp);
  auto occurrence=s.record_occurrences.find(t);
  if(occurrence==s.record_occurrences.end()) {
    // Several distinct anonymous declarations can come from one top-level
    // macro invocation and share seq/column. Capture occurrence identity
    // before execution reachability changes traversal; never deduplicate by
    // layout or export EDG addresses. Re-capture of this type reuses its ID.
    auto key=std::make_pair(value.position.seq,value.position.column);
    occurrence=s.record_occurrences.emplace(t,s.record_occurrence_counts[key]++).first;
  }
  value.source_occurrence=occurrence->second;
  value.size=t->incomplete?0:t->size;value.alignment=t->incomplete?0:t->alignment;
  value.layout_kind=t->kind==tk_union?"union":t->kind==tk_class?"class":"struct";
  auto &c=t->variant.class_struct_union;auto extra=c.extra_info;
  value.template_identity=c.is_template_class;
  value.complete=!t->incomplete;value.dependent=c.is_nonreal_class;
  value.prototype=c.is_prototype_instantiation;
  value.layout_known=value.complete && !value.dependent && value.alignment;
  // File-scope allocated local type entries cannot point into a function's
  // scope region; EDG retains enclosing_routine precisely for that case.
  value.local_owner=t->source_corresp.enclosing_routine;
  std::string qualified=value.name;
  // In C++, an originally unnamed class may acquire a typedef name for
  // linkage (decls.c). A real, nonfabricated source spelling then gives it
  // the ordinary named-class identity; original scope still controls privacy.
  bool local=value.name.empty() || (C_mode() && c.originally_unnamed) || t->source_corresp.is_local_to_function ||
    t->source_corresp.enclosing_routine || t->declared_in_function_prototype;
  for(auto scope=t->source_corresp.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      auto part=ns?spelling(ns->source_corresp):std::string{};
      if(!ns || ns->has_internal_linkage || part.empty())local=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind==sck_class_struct_union) {
      auto owner=scope->variant.assoc_type;
      if(owner && !value.local_owner)value.local_owner=owner->source_corresp.enclosing_routine;
      if(owner && owner->variant.class_struct_union.is_template_class)value.template_identity=true;
      auto part=owner?spelling(owner->source_corresp):std::string{};
      if(part.empty())local=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind!=sck_file) {
      local=true;
      if(scope->kind==sck_function && !value.local_owner)
        value.local_owner=scope->variant.routine.ptr;
    }
  }
  if(!C_mode() && value.local_owner && t->source_corresp.assoc_info && !is_proxy_class(t)) {
    auto supplement=symbol_supplement_for_class(t);
    // Lambda discriminators are partitioned by call-operator signature, not
    // solely by source spelling. Until that signature is owned in this key,
    // only an already-finalized stock name can identify a closure type.
    if(supplement && !extra->is_lambda_closure_class &&
       (supplement->discriminator || !value.name.empty())) {
      value.local_discriminator=supplement->discriminator;
      auto linkage=value.local_owner->source_corresp.name_linkage;
      value.local_owner_external=linkage==nlk_external || linkage==nlk_cplusplus_external;
      value.local_owner_c_linkage=linkage==nlk_external;
      value.local_identity_observed=true;
    }
  }
  value.identity=(local?"local-record:":"record:")+qualified;
  if(extra && extra->proxy_of_type) {
    // EDG's nonreal lookup class is a proxy for this exact original template
    // parameter or decltype, not an instantiated class template. cp_gen_be
    // follows proxy_of_type when reconstructing the source type/name.
    value.proxy_type=capture_type(extra->proxy_of_type);
    if(!value.dependent || value.prototype || extra->proxy_of_type==t)
      value.template_error="original proxy class has inconsistent nonreal ownership";
  } else if(value.dependent && !value.prototype) {
    auto templ=extra?extra->assoc_template:nullptr;
    bool nonreal_member=templ && templ->kind==templk_class && templ->template_info &&
      templ->template_info->is_nonreal_member && !templ->prototype_instantiation.type;
    if(templ && templ->kind==templk_template_template_param) {
      value.template_parameter=capture_source_template(templ);
    } else if(nonreal_member) {
      // Stock equiv_nonreal_templates identifies T::Member<...> by the
      // original parent type and member name, then compares actual arguments.
      // Such a lookup deliberately has no concrete template prototype.
      auto owner=t->source_corresp.is_class_member?parent_class_of(t):nullptr;
      if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
      if(!owner || value.name.empty() || value.name!=spelling(templ->source_corresp))
        value.template_error="dependent member template lacks exact original owner/name";
      else {
        value.member_template_owner=capture_type(owner);
        value.template_member_of_unknown_base=t->source_corresp.member_of_unknown_base;
        value.template_qualified_unknown_base=t->source_corresp.qualified_unknown_base_member;
      }
    } else if(!templ || (templ->kind!=templk_class && templ->kind!=templk_member_class) ||
       !templ->prototype_instantiation.type || templ->prototype_instantiation.type==t)
      value.template_error="dependent class lacks original class template prototype [name="+value.name+
        ",proxy="+(is_proxy_class(t)?std::string("yes"):std::string("no"))+
        ",proxy-kind="+(extra && extra->proxy_of_type?std::to_string(int(extra->proxy_of_type->kind)):std::string("none"))+
        ",lambda="+(extra && extra->is_lambda_closure_class?std::string("yes"):std::string("no"))+
        ",template-kind="+(templ?std::to_string(int(templ->kind)):std::string("none"))+
        ",prototype-present="+(templ && templ->prototype_instantiation.type?std::string("yes"):std::string("no"))+
        ",prototype-self="+(templ && templ->prototype_instantiation.type==t?std::string("yes"):std::string("no"))+
        ",seq="+std::to_string(value.position.seq)+",column="+std::to_string(value.position.column)+"]";
    else value.template_prototype=capture_type(templ->prototype_instantiation.type);
    if(value.template_prototype)capture_source_class_specialization(t,templ,value.template_error);
    if(value.template_error.empty()) {
      for(auto argument=extra->template_arg_list;argument;argument=argument->next) {
        auto item=capture_constraint_argument(argument,value.template_error);
        if(!value.template_error.empty())value.template_error+=" [argument-kind="+std::to_string(int(argument->kind))+
          ",ordinal="+std::to_string(value.template_arguments.size())+"]";
        value.template_arguments.push_back(item);
      }
    }
  }
  if(t->incomplete){s.records[t]=value;return;}
  value.has_virtual_functions=c.any_virtual_functions_including_in_base_classes;
  value.has_bases=extra && extra->base_classes;
  if(!value.size || value.size>2147483647ULL || !value.alignment || value.alignment>64 || (value.alignment&(value.alignment-1)))
    value.error="record outside complete plain layout profile";
#if GNU_EXTENSIONS_ALLOWED
  value.is_packed=c.is_packed;
#endif
  value.max_member_alignment=c.max_member_alignment;
  auto requested_record=s.requested_record_alignments.find(t);
  if(requested_record!=s.requested_record_alignments.end())value.explicit_alignment=requested_record->second;
  else if(t->alignment_set_explicitly)value.error="record explicit alignment lacks original layout observation";
  // Unsupported execution layout does not erase the original member index.
  // Keep its typed source declarations for the independent semantic index;
  // the execution writer still rejects value.error before publishing layouts.
  for(auto f=c.field_list;f;f=f->next) {
    RecordField field;field.il=f;field.name=spelling(f->source_corresp);field.original_name=field.name;field.offset=f->offset;
    field.has_initializer=f->has_initializer;field.is_mutable=f->is_mutable;field.no_unique_address=f->has_no_unique_address_attribute;field.is_bit_field=f->is_bit_field;
    field.position=f->source_corresp.decl_position;
    if(field.name.empty())field.name="__anonymous_member_"+std::to_string(value.fields.size());
    auto requested_field=s.requested_field_alignments.find(f);
    if(requested_field!=s.requested_field_alignments.end())field.explicit_alignment=requested_field->second;
    else if(f->alignment)value.error="field explicit alignment lacks original layout observation";
    if(f->has_no_unique_address_attribute || f->is_lowered_base_class || f->class_subobject_with_tail_padding)
      value.error="record field outside plain layout profile";
    if(f->is_bit_field) {
      field.bit_offset=f->offset_bit_remainder;field.bit_width=f->bit_size;field.bit_signed=f->bit_field_is_signed;
      if(field.bit_offset>7 || field.bit_width>64 || skip_typerefs(f->type)->kind!=tk_integer ||
         (field.bit_width && skip_typerefs(f->type)->size>8))
        value.error="bitfield outside fixed integer layout";
    }
#if GNU_EXTENSIONS_ALLOWED
    field.is_packed=f->is_packed;
#endif
    if(!C_mode()) {
      // Anonymous C tags are numbered independently in each translation unit.
      // Retain the original member declaration instead of promoting that C
      // spelling to a cross-unit C++ nominal identity. Declarator wrappers do
      // not create new anonymous types (for example `struct {...} *a, *b`).
      auto child=skip_typerefs(f->type);
      while(child->kind==tk_array || child->kind==tk_pointer)
        child=skip_typerefs(child->kind==tk_array?child->variant.array.element_type:child->variant.pointer.type);
      if((child->kind==tk_struct || child->kind==tk_class || child->kind==tk_union) &&
         child->variant.class_struct_union.originally_unnamed && spelling(child->source_corresp).empty() &&
         !child->variant.class_struct_union.extra_info->is_lambda_closure_class &&
         child->source_corresp.is_class_member && parent_class_of(child)==t)
        s.record_owners.emplace(child,RecordOwner{t,f,unsigned(value.fields.size())});
    }
    field.source_type=capture_type(f->type);value.fields.push_back(field);
  }
  if(extra)for(auto base=extra->direct_base_classes;base;base=base->next_direct) {
    PrelowerBase original;original.il=base;original.original_type=base->orig_type?base->orig_type:base->type;
    original.source_type_snapshot=capture_type(original.original_type);
    original.position=base->decl_position;original.is_virtual=base->is_virtual;
    for(auto derivation=base->derivation;derivation;derivation=derivation->next)if(derivation->direct) {
      original.access=derivation->access==as_public?"public":derivation->access==as_protected?"protected":
        derivation->access==as_private?"private":"unknown";break;
    }
    if(original.access.empty())original.access="unknown";
    value.bases.push_back(original);
  }
  s.records[t]=value;
  for(const auto &field:value.fields)capture_field_initializer(field.il,t);
}
#include "dot_gen_object_source_operations.h"
#include "dot_gen_object_source_constraints.h"
// An original type may first be reached through a semantic use before its
// declaration's prototype scope is available. Only that exact original type
// pointer, revisited through a return/parameter type edge, may complete the
// pending operator expression; competing declaration owners stay rejected.
inline bool source_expression_has_parameter(unsigned expression,std::set<unsigned> &seen) {
  if(!expression || !seen.insert(expression).second)return false;
  const auto &value=source_constraint_graph().expressions.at(expression-1);
  if(value.kind=="parameter-reference")return true;
  for(const auto &edge:value.edges)if(source_expression_has_parameter(edge.child,seen))return true;
  return false;
}
inline void capture_pending_type_context(a_type_ptr t,unsigned id) {
  auto &graph=source_constraint_graph();unsigned binder=graph.active_binder;
  if(!binder || !graph.active_type_contexts.insert(t).second)return;
  auto &saved=state().types.at(id-1);
  if(t->kind==tk_typeref) {
    if(saved.source_form=="type-operator" && saved.operator_expression) {
      std::set<unsigned> seen;
      if(source_expression_has_parameter(saved.operator_expression,seen)) {
        auto previous=graph.type_operator_binders.find(id);
        if(previous==graph.type_operator_binders.end() || !previous->second) {
          auto expression=decltype_arg(t);
          if(expression) {
            unsigned resolved=capture_constraint_expression(expression,binder);
            state().types.at(id-1).operator_expression=resolved;graph.type_operator_binders[id]=binder;
          }
        } else if(previous->second!=binder) {
          auto describe=[&](unsigned frame) {
            const auto &entry=graph.binders.at(frame);
            std::string text="binder="+std::to_string(frame)+" owner="+std::to_string(entry.routine)+
              " requires="+std::to_string(entry.expression)+" parent="+std::to_string(entry.parent)+
              " outside="+std::to_string(entry.outside);
            if(entry.routine && std::getenv("EDG_DOTOBJ_DIAG_SOURCE_ERRORS")) {
              const auto &owner=state().types.at(entry.routine-1);
              auto original=owner.original_type;
              if(!original)for(const auto &candidate:state().type_ids)if(candidate.second==owner.id || candidate.second==graph.binders.at(frame).routine){original=candidate.first;break;}
              text+=" original="+std::to_string(reinterpret_cast<size_t>(original));
              text+=" receiver="+std::to_string(owner.object_type);
              if(original && original->kind==tk_routine) {
                auto supplement=original->variant.routine.extra_info;
                text+=" parameters="+std::to_string(reinterpret_cast<size_t>(supplement->param_type_list))+
                  " lambda="+std::to_string(supplement->assoc_routine_is_lambda_body)+
                  " special="+std::to_string(supplement->assoc_routine?int(supplement->assoc_routine->special_kind):-1)+
                  " return="+std::to_string(reinterpret_cast<size_t>(original->variant.routine.return_type))+
                  " routine="+(supplement->assoc_routine?spelling(supplement->assoc_routine->source_corresp):std::string("none"));
                auto diag_associated=supplement->assoc_routine;
                if(diag_associated) {
                  text+=" template-kind="+std::to_string(diag_associated->assoc_template?int(diag_associated->assoc_template->kind):-1)+" arguments=[";
                  for(auto diag_argument=diag_associated->template_arg_list;diag_argument;diag_argument=diag_argument->next) {
                    auto diag_argument_type=diag_argument->kind==tak_type?diag_argument->variant.type:nullptr;
                    text+=std::to_string(int(diag_argument->kind))+":"+std::to_string(reinterpret_cast<size_t>(diag_argument_type))+
                      ":exact-root="+(diag_argument_type==t?"yes":"no")+",";
                  }
                  text+="]";
                }
                text+=" borrowed-root="+std::to_string(reinterpret_cast<size_t>(t))+
                  " type-enclosing-routine="+(t->source_corresp.enclosing_routine?spelling(t->source_corresp.enclosing_routine->source_corresp):std::string("none"))+
                  " type-parent-kind="+std::to_string(t->source_corresp.parent_scope?int(t->source_corresp.parent_scope->kind):-1);
              }
            }
            return text;
          };
          state().types.at(id-1).error="original type operator has competing lexical parameter scopes: type="+
            std::to_string(id)+" old["+describe(previous->second)+"] new["+describe(binder)+"]";
        }
      }
    }
    if(t->variant.typeref.kind==trk_is_template_alias && t->variant.typeref.extra_info) {
      // An alias used in a lexical type operand owns its original argument
      // recipes as well as the substituted underlying type.
      auto extra=t->variant.typeref.extra_info;
      for(auto arg=extra->template_arg_list;arg;arg=arg->next)
        if(arg->kind==tak_type)capture_type(arg->variant.type);
      for(auto arg=extra->orig_template_arg_list;arg;arg=arg->next)
        if(arg->kind==tak_type)capture_type(arg->variant.type);
    }
    capture_type(t->variant.typeref.type);
  } else if(t->kind==tk_pointer)capture_type(t->variant.pointer.type);
  else if(t->kind==tk_array)capture_type(t->variant.array.element_type);
  else if((t->kind==tk_class || t->kind==tk_struct || t->kind==tk_union) && class_type_supp(t)->proxy_of_type)
    capture_type(class_type_supp(t)->proxy_of_type);
  graph.active_type_contexts.erase(t);
}
inline unsigned capture_type(a_type_ptr t) {
  if(!t)return 0;
  auto &s=state();auto prior=s.type_ids.find(t);
  if(prior!=s.type_ids.end()) {
    unsigned original_namespace=capture_parameter_template_namespace(t);
    if(original_namespace) {
      auto &original=s.types.at(prior->second-1);
      if(original.namespace_owner && original.namespace_owner!=original_namespace)
        original.error="source formal namespace conflicts with its exact original enclosing template";
      else original.namespace_owner=original_namespace;
    }
    capture_pending_type_context(t,prior->second);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union){SourceParameterScope scope(class_type_supp(t)->proxy_of_type?current_source_parameter_binder():0);capture_record(t);}
    if(t->kind==tk_integer && t->variant.integer.enum_type && !t->variant.integer.bool_type)capture_enum(t);
    return prior->second;
  }
  unsigned id=(unsigned)s.types.size()+1;s.type_ids[t]=id;s.types.emplace_back();
  Type value;value.id=id;value.original_type=t;value.name=spelling(t->source_corresp);value.position=t->source_corresp.decl_position;
  if(!value.name.empty() || is_immediate_class_type(t) || (t->kind==tk_integer && t->variant.integer.enum_type))
    value.namespace_owner=capture_source_namespace_owner(t->source_corresp);
  unsigned formal_namespace=capture_parameter_template_namespace(t);
  if(formal_namespace) {
    if(value.namespace_owner && value.namespace_owner!=formal_namespace)
      value.error="source formal namespace conflicts with its exact original enclosing template";
    else value.namespace_owner=formal_namespace;
  }
  if(t->kind==tk_typeref) {
    value.kind=type_is_typedef(t)?"typedef":"qualified";
    if(t->variant.typeref.kind==trk_is_template_alias)value.source_form="template-alias";
    else if(t->variant.typeref.kind==trk_is_decltype || t->variant.typeref.kind==trk_is_typeof_with_expression ||
            t->variant.typeref.kind==trk_is_typeof_with_type_operand || t->variant.typeref.kind==trk_is_underlying_type ||
            (t->variant.typeref.kind>=trk_add_lvalue_reference && t->variant.typeref.kind<=trk_remove_reference)) {
      value.source_form="type-operator";value.type_operator=int(t->variant.typeref.kind);
      value.operator_dependent=t->variant.typeref.is_dependent_type_operator;
      value.operator_unparenthesized=t->variant.typeref.decltype_expr_not_parenthesized;
      auto extra=t->variant.typeref.extra_info;
      if(!extra)value.error="original type operator lacks its operand supplement";
      else {
        if(t->variant.typeref.kind==trk_is_decltype || t->variant.typeref.kind==trk_is_typeof_with_expression) {
          auto expression=decltype_arg(t);
          if(!expression)value.error="original expression type operator requires its lexical expression";
          else {
            value.operator_expression=capture_constraint_expression(expression);
            source_constraint_graph().type_operator_binders[id]=current_source_parameter_binder();
          }
        } else if(extra->operator_type_arg)value.operator_type=capture_type(extra->operator_type_arg);
        else value.error="original type operator has no original type operand";
      }
    }
    value.is_const=(t->variant.typeref.qualifiers&TQ_CONST)!=0;
    value.is_volatile=(t->variant.typeref.qualifiers&TQ_VOLATILE)!=0;
    value.is_restrict=(t->variant.typeref.qualifiers&TQ_RESTRICT)!=0;
    value.is_atomic=(t->variant.typeref.qualifiers&TQ_C11_ATOMIC)!=0;
    auto remaining=t->variant.typeref.qualifiers & ~(TQ_CONST|TQ_VOLATILE|TQ_RESTRICT|TQ_C11_ATOMIC);
    auto qualifier=[&](a_type_qualifier_set bit,const char *name) {
      if(remaining&bit) {value.qualifiers.push_back(name);remaining&=~bit;}
    };
    qualifier(TQ_NULLABLE,"nullable");qualifier(TQ_NONNULL,"non-null");qualifier(TQ_NULL_UNSPECIFIED,"null-unspecified");
#if MICROSOFT_EXTENSIONS_ALLOWED
    qualifier(TQ_UNALIGNED,"unaligned");
#endif
#if NEAR_AND_FAR_ALLOWED
    qualifier(TQ_NEAR,"near");qualifier(TQ_FAR,"far");
#endif
#if UPC_EXTENSIONS_ALLOWED
    qualifier(TQ_UPC_SHARED,"upc-shared");qualifier(TQ_UPC_STRICT,"upc-strict");qualifier(TQ_UPC_RELAXED,"upc-relaxed");
#endif
    if(remaining)value.error="source type has unrepresented qualifier bits";
    for(const auto &name:value.qualifiers)
      if(name!="nullable" && name!="non-null" && name!="null-unspecified")
        // UPC qualifiers additionally carry a block size in the typeref
        // supplement. Near/far and unaligned affect storage/access contracts.
        // Keeping only their names would falsely promise complete semantics.
        value.error="source extension qualifier requires an owned auxiliary/storage contract: "+name;
    value.underlying=capture_type(t->variant.typeref.type);
    value.execution=s.types.at(value.underlying-1).execution;
    if(value.kind=="qualified") {
      value.name.clear();
      if(value.source_form=="plain" && !value.is_const && !value.is_volatile && !value.is_restrict && !value.is_atomic && value.qualifiers.empty() && value.error.empty()) {
        // Anonymous typeref bookkeeping with no represented qualifier adds
        // no declaration type distinction in this bounded scalar profile.
        s.type_ids[t]=value.underlying;return value.underlying;
      }
    }
  } else if(t->kind==tk_void){value.kind="void";value.execution=1;}
  else if(t->kind==tk_integer) {
    if(t->variant.integer.bool_type){value.kind="bool";value.execution=3;}
    else switch(t->variant.integer.int_kind) {
      case ik_char:value.kind="char";value.execution=targ_has_signed_chars?4:3;break;
      case ik_signed_char:value.kind="signed-char";value.execution=4;break;
      case ik_unsigned_char:value.kind="unsigned-char";value.execution=3;break;
      case ik_short:value.kind="short";value.execution=5;break;
      case ik_unsigned_short:value.kind="unsigned-short";value.execution=6;break;
      case ik_int:value.kind="int";value.execution=2;break;
      case ik_unsigned_int:value.kind="unsigned-int";value.execution=7;break;
      case ik_long:value.kind="long";value.execution=8;break;
      case ik_unsigned_long:value.kind="unsigned-long";value.execution=9;break;
      case ik_long_long:value.kind="long-long";value.execution=8;break;
      case ik_unsigned_long_long:value.kind="unsigned-long-long";value.execution=9;break;
      case ik_int128:value.kind="int128";value.is_int128=true;break;
      case ik_unsigned_int128:value.kind="unsigned-int128";value.is_int128=true;value.is_unsigned_int128=true;break;
      default:break;
    }
    if(t->variant.integer.wchar_t_type)value.kind="wchar-t";
    else if(t->variant.integer.char8_t_type)value.kind="char8-t";
    else if(t->variant.integer.char16_t_type)value.kind="char16-t";
    else if(t->variant.integer.char32_t_type)value.kind="char32-t";
    if(t->variant.integer.enum_type && !t->variant.integer.bool_type) {value.kind="enum";capture_enum(t);}
  } else if(t->kind==tk_float) {
    if(t->variant.float_kind==fk_float && t->size==4){value.kind="float";value.execution=10;}
    else if(t->variant.float_kind==fk_double && t->size==8){value.kind="double";value.execution=11;}
    else if(t->size==16 && t->alignment==16) {
      if(t->variant.float_kind==fk_long_double)value.kind="long-double";
      else if(t->variant.float_kind==fk_float80)value.kind="float80";
      else if(t->variant.float_kind==fk_float64x)value.kind="float64x";
    }
  } else if(t->kind==tk_nullptr) {
    value.kind="nullptr";
    if(!is_standard_nullptr_type(t))value.error="managed nullptr source type is not represented";
  } else if(t->kind==tk_template_param) {
    value.kind="dependent";
    const auto &parameter=t->variant.template_param;auto extra=parameter.extra_info;
    if(!extra || parameter.is_generic_param || parameter.is_auto_param || parameter.is_decltype_auto ||
       parameter.originally_class_template_param)
      value.error="dependent source type requires additional generic/placeholder/nested-origin contract";
    else if(parameter.kind==tptk_param && extra->coordinates.depth==AUTO_TYPE_NESTING_DEPTH) {
      // Stock make_auto_type owns these placeholder coordinates. They do not
      // denote a template formal and need not have a declaration location.
      value.dependent_kind="auto-placeholder";value.dependent_depth=extra->coordinates.depth;
      value.dependent_position=extra->coordinates.position;
      if(parameter.is_pack || extra->template_symbol || extra->orig_nested_type || !value.name.empty() ||
         (value.dependent_position!=PLAIN_AUTO_TYPE_POS_NUMBER && value.dependent_position!=DECLTYPE_AUTO_POS_NUMBER))
        value.error="source auto placeholder lacks its exact original type-specifier form";
      if(extra->constraint.type_constraint) {
        if(extra->constraint.type_constraint->kind!=enk_concept_id)
          value.error="constrained auto placeholder requires its exact original concept-id";
        else value.constraint_expression=capture_constraint_expression(extra->constraint.type_constraint);
      }
    } else if(parameter.kind==tptk_param) {
      value.dependent_kind="parameter";value.dependent_depth=extra->coordinates.depth;
      value.dependent_position=extra->coordinates.position;value.dependent_pack=parameter.is_pack;
      if(value.dependent_depth<0 || value.dependent_position<=0)
        value.error="dependent source parameter lacks ordinary original coordinates";
      if(extra->constraint.type_constraint) {
        if(extra->constraint.type_constraint->kind!=enk_concept_id)
          value.error="original constrained type requires its exact concept-id";
        else value.constraint_expression=capture_constraint_expression(extra->constraint.type_constraint);
      }
      a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
      conv_seq_to_file_and_line(value.position.seq,&display,&full,&line,&at_end);
      if(!line || !(full?full:display))value.error="dependent source parameter lacks original declaration location [name="+value.name+
        ",seq="+std::to_string(value.position.seq)+",column="+std::to_string(value.position.column)+
        ",depth="+std::to_string(value.dependent_depth)+",position="+std::to_string(value.dependent_position)+
        ",pack="+(parameter.is_pack?"yes":"no")+",template-symbol="+(extra->template_symbol?"yes":"no")+
        ",original-nested-type="+(extra->orig_nested_type?"yes":"no")+"]";
      else value.dependent_identity=std::string(full?full:display)+":"+std::to_string(line)+":"+
        std::to_string(value.position.column)+":"+std::to_string(value.dependent_depth)+":"+std::to_string(value.dependent_position);
    } else if(parameter.kind==tptk_member) {
      value.dependent_kind="member";
      a_type_ptr owner=t->source_corresp.is_class_member?parent_class_of(t):nullptr;
      if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
      if(!owner || owner==t || value.name.empty() || parameter.is_pack)
        value.error="dependent source member lacks exact named owner";
      else {
        value.dependent_owner=capture_type(owner);
        if(extra->orig_nested_type) {
          auto original=extra->orig_nested_type;
          if(!original->source_corresp.is_class_member || parent_class_of(original)!=owner ||
             spelling(original->source_corresp)!=value.name || value.name.empty())
            value.error="nonreal nested type lost its exact original declaration owner/name";
          else {
            auto prior_owner=s.nested_type_declarations.emplace(original,owner);
            if(!prior_owner.second && prior_owner.first->second!=owner)
              value.error="original nested type has conflicting enclosing declarations";
            value.dependent_original_nested=capture_type(original);
          }
        }
      }
    } else if(parameter.kind==tptk_unknown) {
      value.dependent_kind="unknown";value.name.clear();
      if(parameter.is_pack)value.error="unknown dependent pack requires an explicit expansion contract";
    } else value.error="dependent bit-precise source type requires its width expression";
  } else if(t->kind==tk_ptr_to_member) {
    value.kind="member-pointer";
#if MICROSOFT_EXTENSIONS_ALLOWED
    if(t->variant.ptr_to_member.modifiers)value.error="member-pointer modifiers are not represented in the source type contract";
#endif
    value.member_owner=capture_type(t->variant.ptr_to_member.orig_class_of_which_a_member?
      t->variant.ptr_to_member.orig_class_of_which_a_member:t->variant.ptr_to_member.class_of_which_a_member);
    value.underlying=capture_type(t->variant.ptr_to_member.type);
  } else if(t->kind==tk_pointer) {
    value.kind=t->variant.pointer.is_reference?(t->variant.pointer.is_rvalue_reference?"rvalue-reference":"lvalue-reference"):"pointer";
    value.underlying=capture_type(t->variant.pointer.type);
    // Execution pointer IDs are interned by the object writer, independently
    // of EDG addresses and of the source typedef/qualification structure.
  } else if(t->kind==tk_array && t->variant.array.is_template_dependent_size_array) {
    value.kind="array";value.underlying=capture_type(t->variant.array.element_type);
    value.dependent_array_bound=capture_semantic_constant(t->variant.array.variant.element_count_constant);
    if(!value.dependent_array_bound)value.error="dependent array bound lacks its original semantic constant";
  } else if(t->kind==tk_array && !t->variant.array.is_variable_size_array &&
            !t->variant.array.is_vla && !t->variant.array.is_template_dependent_size_array &&
            true) {
    value.kind="array";
    value.underlying=capture_type(t->variant.array.element_type);
    value.element_count=t->variant.array.variant.number_of_elements;
    value.array_bound_known=value.element_count!=0 || t->variant.array.bound_is_zero;
  } else if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
    if(t->variant.class_struct_union.extra_info && t->variant.class_struct_union.extra_info->is_va_list_tag)
      value.kind="va-list";
    else {value.kind="record";value.record_type=t;SourceParameterScope scope(class_type_supp(t)->proxy_of_type?current_source_parameter_binder():0);capture_record(t);}
  } else if(t->kind==tk_routine) {
    value.kind="routine";
    // Capture policy before receiver/result recursion can finalize a defaulted
    // member's exception specification. Only the stock finalization observer
    // below may provide a later semantic policy for this exact source identity.
    value.initial_exception_flags=exception_policy_flags(t);
    value.exception_policy=value.initial_exception_flags.policy();
    if(value.exception_policy=="dynamic")
      value.dynamic_specification=capture_dynamic_exception_specification(t->variant.routine.extra_info->exception_specification);
    auto extra=t->variant.routine.extra_info;
    a_type_ptr recipe=t;
    unsigned recipe_id=id,recipe_parent=current_source_parameter_binder();
    auto copied_prototype=s.lambda_prototype_recipes.find(t);
    if(copied_prototype!=s.lambda_prototype_recipes.end()) {
      // generate_lambda_conversion_function copies the call operator signature,
      // but shares its original return/parameter type recipes. The generated
      // conversion target is a use of that source prototype, not a new lexical
      // declaration of its decltype expressions.
      auto selected=copied_prototype->second.second;
      recipe=copied_prototype->second.first?skip_typerefs(copied_prototype->second.first):nullptr;
      if(extra->assoc_routine && extra->assoc_routine->special_kind==sfk_lambda_entry_point &&
         extra->assoc_routine->variant.lambda_call_operator!=selected)
        value.error="generated lambda prototype entry differs from its observed factory origin";
      if(!recipe || recipe==t || recipe->kind!=tk_routine) {
        value.error="generated lambda prototype lacks its exact original call operator type";
        recipe=t;
      } else {
        auto original=recipe->variant.routine.extra_info;
        if(!original->assoc_routine_is_lambda_body || !original->this_class || !original->has_this_param ||
           extra->assoc_routine_is_lambda_body || extra->this_class || extra->has_this_param ||
           extra->qualifiers!=TQ_NONE || extra->ref_qualifiers!=rqk_default ||
           extra->has_ellipsis!=original->has_ellipsis || extra->prototyped!=original->prototyped ||
           t->variant.routine.return_type!=recipe->variant.routine.return_type ||
           extra->exception_specification!=original->exception_specification)
          value.error="generated lambda prototype differs from stock receiver-cleared signature copy";
#if MICROSOFT_EXTENSIONS_ALLOWED || GNU_X86_ATTRIBUTES_ALLOWED
        if(extra->calling_convention!=original->calling_convention)
          value.error="generated lambda prototype requires its changed calling convention contract";
#endif
        auto copied_parameter=extra->param_type_list,original_parameter=original->param_type_list;
        for(;copied_parameter && original_parameter;copied_parameter=copied_parameter->next,original_parameter=original_parameter->next) {
          if(copied_parameter->type!=original_parameter->type || copied_parameter->declared_type!=original_parameter->declared_type ||
             copied_parameter->qualifiers!=original_parameter->qualifiers || copied_parameter->is_parameter_pack!=original_parameter->is_parameter_pack ||
             copied_parameter->is_requires_expr_param!=original_parameter->is_requires_expr_param || copied_parameter->has_default_arg ||
             std::string(copied_parameter->name?copied_parameter->name:"")!=std::string(original_parameter->name?original_parameter->name:""))
            value.error="generated lambda prototype lost its exact copied parameter declaration";
        }
        if(copied_parameter || original_parameter)value.error="generated lambda prototype parameter copy is incomplete";
        {SourceParameterScope original_scope(0);recipe_id=capture_type(recipe);}
        recipe_parent=0;value.prototype_origin=recipe_id;
      }
    }
    auto frames=capture_parameter_binder_pair(0,recipe_id,recipe_parent,recipe->variant.routine.extra_info->param_type_list);
    if(value.prototype_origin)value.prototype_origin_binder=frames.second;
    {SourceParameterScope prototype(frames.second);value.result=capture_type(t->variant.routine.return_type);}
    SourceParameterScope receiver_scope(0);
    if(extra->this_class) {
      value.object_type=capture_type(extra->this_class);
      value.object_is_const=(extra->qualifiers&TQ_CONST)!=0;
      value.object_is_volatile=(extra->qualifiers&TQ_VOLATILE)!=0;
      value.object_ref_qualifier=extra->ref_qualifiers==rqk_lvalue?"lvalue":
        extra->ref_qualifiers==rqk_rvalue?"rvalue":"none";
    }
    value.variadic=t->variant.routine.extra_info->has_ellipsis;
    value.parameter_mode=t->variant.routine.extra_info->prototyped?"prototype":"unspecified";
    trace_routine_exception_policy("source-capture",id,t,value.exception_policy);
    unsigned parameter_ordinal=0;
    for(auto p=extra->param_type_list;p;p=p->next,++parameter_ordinal) {
      const auto &parameter=source_constraint_graph().binders.at(frames.first).parameters.at(parameter_ordinal);
      value.parameters.push_back(parameter.declared_type?parameter.declared_type:parameter.type);
      value.declaration_defaults.push_back(p->has_default_arg);
    }
    if(C_mode()) {
      RoutineAbi abi;abi.proof="source-c-signature";
      for(auto p=extra->param_type_list;p;p=p->next) {
        abi.original_parameters.push_back(p);abi.indirect_parameters.push_back(false);
      }
      s.routine_abis[id]=abi;
    }
  }
  // Unknown entries are retained privately as unavailable, never exported as
  // opaque payloads. An unused unsupported declaration cannot block the TU.
  if(t->kind!=tk_typeref && value.kind!="record" && value.kind!="enum" && value.kind!="dependent")value.name.clear();
  s.types[id-1]=value;
  if(value.source_form=="template-alias")capture_source_alias(t);
  return id;
}
inline std::string resolved_exception_policy(unsigned id) {
  auto found=state().routine_exception_resolutions.find(id);
  return found==state().routine_exception_resolutions.end()?state().types.at(id-1).exception_policy:
    found->second.final_flags.policy();
}
// The caller is exactly form_exception_specification_for_generated_function:
// retain entry and completion type identity without resolving any EDG state.
struct GeneratedExceptionSpecificationCapture {
  unsigned original_id=0;
  a_type_ptr original_type=nullptr;
  RoutineExceptionResolution resolution;
  GeneratedExceptionSpecificationCapture(a_routine_ptr routine) {
    if(!enabled())return;
    original_type=routine->type;
    auto found=state().type_ids.find(original_type);
    if(found!=state().type_ids.end())original_id=found->second;
    resolution.routine=routine;
    resolution.is_defaulted=routine->is_defaulted;
    resolution.compiler_generated=routine->compiler_generated;
    resolution.inheriting_constructor=routine->is_inheriting_ctor;
    // Stock update_routine_type_exception_specification_if_needed calls this
    // exact factory for user-declared destructors without an explicit spec.
    // Such a source destructor is not a synthesized/defaulted function.
    resolution.implicit_destructor=routine->special_kind==sfk_destructor &&
      !resolution.is_defaulted && !resolution.compiler_generated && !resolution.inheriting_constructor;
    if(routine->special_kind==sfk_constructor)resolution.special_kind="constructor";
    else if(routine->special_kind==sfk_destructor)resolution.special_kind="destructor";
    else if(routine->special_kind==sfk_operator) {
      switch(routine->variant.opname_kind) {
        case onk_assign:resolution.special_kind="assignment";break;
        case onk_eq:resolution.special_kind="equality";break;
        case onk_ne:resolution.special_kind="inequality";break;
        case onk_lt:resolution.special_kind="less";break;
        case onk_gt:resolution.special_kind="greater";break;
        case onk_le:resolution.special_kind="less-equal";break;
        case onk_ge:resolution.special_kind="greater-equal";break;
        case onk_spaceship:resolution.special_kind="three-way-comparison";break;
        default:resolution.error="unexpected generated exception-specification operator";break;
      }
    } else resolution.error="unexpected generated exception-specification function kind";
    if(exception_policy_flags(original_type).present)
      resolution.error="generated exception specification entry was not empty";
  }
  void complete() {
    if(!original_type)return;
    if(!original_id) {
      auto found=state().type_ids.find(original_type);
      if(found==state().type_ids.end())return;
      original_id=found->second;
    }
    resolution.final_type=resolution.routine->type==original_type?original_id:capture_type(resolution.routine->type);
    resolution.final_flags=exception_policy_flags(resolution.routine->type);
    if(resolution.final_flags.policy()=="dynamic")
      resolution.dynamic_specification=capture_dynamic_exception_specification(resolution.routine->type->variant.routine.extra_info->exception_specification);
    if(resolution.final_flags.indeterminate || resolution.final_flags.arg_cached || resolution.final_flags.copy_from_prototype)
      resolution.error="generated exception specification remained unresolved";
    auto found=state().routine_exception_resolutions.find(original_id);
    if(found!=state().routine_exception_resolutions.end() &&
       (found->second.routine!=resolution.routine ||
        found->second.final_type!=resolution.final_type ||
        found->second.final_flags.policy()!=resolution.final_flags.policy() ||
        found->second.special_kind!=resolution.special_kind))
      resolution.error="conflicting generated exception-specification finalizations";
    state().routine_exception_resolutions[original_id]=resolution;
  }
};
inline bool request_referenced_defaulted_exception_policy(a_routine_ptr routine,a_boolean elided) {
  if(!enabled() || C_mode() || !exceptions_enabled || !in_front_end || elided ||
     il_lowering_underway || !routine->is_defaulted || routine->is_deleted ||
     routine->is_prototype_instantiation || routine->defined_outside_of_parent ||
     !routine->source_corresp.is_class_member)return false;
  auto type=skip_typerefs(routine->type);
  if(type->kind!=tk_routine || type->variant.routine.extra_info->exception_specification)return false;
  auto owner=parent_class_of(routine);
  if(owner->incomplete || !owner->source_corresp.assoc_info ||
     type->variant.routine.extra_info->this_class!=owner)return false;
  // Other members may already have prelowered this receiver. Stock
  // update_generated_exception_spec_for_fields explicitly handles the
  // generated base-class fields in that case. The selected routine itself
  // must still have its original signature, before its calling-method/ABI
  // observation; otherwise its emitted body could already use the old policy.
  auto observed_type=state().type_ids.find(type);
  if(observed_type!=state().type_ids.end() &&
     state().routine_abis.count(observed_type->second))return false;
  auto supplement=class_symbol_supp(symbol_for(owner));
  if(supplement->scanning_field_initializer || supplement->has_initializer_fixups ||
     supplement->default_ctor_body_delayed)return false;
  // The set prevents a dependency recursion from querying the same selected
  // defaulted member twice. Capture before the query to retain its initial
  // absent policy; the existing stock-completion observer records the result.
  if(!state().requested_defaulted_exception_policies.insert(routine).second)return false;
  capture_type(type);
  return true;
}
inline void observe_type(a_type_ptr type){if(enabled())capture_type(type);}
inline void observe_lowered_type(a_type_ptr type) {
  if(!enabled())return;
  auto &s=state();auto original=s.type_ids.find(type);if(original==s.type_ids.end())return;
  const auto &source=s.types.at(original->second-1);
  if(source.kind=="nullptr" || source.kind=="member-pointer")s.lowered_source_types[type]=skip_typerefs(type);
}
inline void observe_routine_abi(a_type_ptr type) {
  if(!enabled())return;
  auto &s=state();unsigned id=capture_type(type);
  const auto &source=s.types.at(id-1);
  RoutineAbi abi;abi.proof="observed-routine-lowering";
  auto extra=type->variant.routine.extra_info;
  trace_routine_exception_policy("abi-observation",id,type,source.exception_policy);
  abi.result_buffer=extra->value_returned_as_parameter;
  abi.result_follows_this=extra->return_value_parameter_follows_this;
  for(auto parameter=extra->param_type_list;parameter;parameter=parameter->next) {
    abi.original_parameters.push_back(parameter);
    abi.indirect_parameters.push_back(parameter->passed_via_copy_constructor);
  }
  if(source.kind!="routine" || source.parameters.size()!=abi.original_parameters.size() ||
     (source.object_type!=0)!=(extra->this_class!=nullptr))
    abi.error="routine ABI observation does not match original source signature";
  s.routine_abis[id]=abi;
}
inline void observe_vtt_type(a_type_ptr type) {
  if(!enabled())return;
  auto found=state().type_ids.find(type);
  if(found==state().type_ids.end())return;
  auto extra=type->variant.routine.extra_info;
  if(extra->this_class && skip_typerefs(extra->this_class)->variant.class_struct_union.any_virtual_base_classes) {
    auto &abi=state().routine_abis.at(found->second);
    if(!extra->param_type_list || !extra->param_type_list->next)abi.error="missing observed VTT ABI slot";
    else abi.vtt_type=extra->param_type_list->next->type;
  }
}
inline void observe_vtt_parameter(a_routine_ptr routine,a_variable_ptr variable) {
  if(enabled())state().vtt_parameters[routine]=variable;
}
inline void observe_runtime_signature(a_type_ptr type) {
  if(!enabled())return;
  type=skip_typerefs(type);
  if(type->kind==tk_pointer)type=skip_typerefs(type->variant.pointer.type);
  auto &s=state();unsigned id=capture_type(type);
  RoutineAbi abi;abi.proof="observed-runtime-signature";
  if(type->kind!=tk_routine){abi.error="runtime signature creation is not routine";s.routine_abis[id]=abi;return;}
  auto extra=type->variant.routine.extra_info;
  if(extra->this_class || extra->value_returned_as_parameter || extra->value_returned_by_cctor)
    abi.error="runtime signature outside direct explicit ABI";
  for(auto parameter=extra->param_type_list;parameter;parameter=parameter->next) {
    abi.original_parameters.push_back(parameter);abi.indirect_parameters.push_back(false);
    if(parameter->passed_via_copy_constructor)abi.error="runtime signature has indirect original argument";
  }
  if(s.types.at(id-1).parameters.size()!=abi.original_parameters.size())
    abi.error="runtime signature was captured before final formal assembly";
  s.routine_abis[id]=abi;
}
inline void capture_routine(a_routine_ptr routine);
inline void observe_runtime_declaration(a_routine_ptr routine) {
  if(!enabled())return;
  state().runtime_declarations.insert(routine);
  observe_runtime_signature(routine->type);
  // File-scope generated destructor bodies may create this declaration after
  // the original file scope has been captured. Its exact creation is the
  // provenance boundary; it is never a fabricated source routine.
  capture_routine(routine);
}
inline void observe_result_parameter(a_routine_ptr routine,a_variable_ptr parameter) {
  if(enabled())state().result_parameters[routine]=parameter;
}
inline a_routine_ptr abi_primary(a_routine_ptr r) {
  auto adapter=state().default_adapters.find(r);
  if(adapter!=state().default_adapters.end())return adapter->second.primary;
#if DO_IL_LOWERING && IA64_ABI
  return r && r->compiler_generated ? r->primary_ctor_or_dtor : nullptr;
#else
  (void)r;return nullptr;
#endif
}
inline void observe_asm_before_lowering(an_asm_entry_ptr entry) {
  if(!enabled() || state().asm_statements.count(entry))return;
  auto &record=state().asm_statements[entry];
#if GNU_EXTENSIONS_ALLOWED
  record.is_volatile=entry->is_volatile;
  record.has_volatile_keyword=entry->has_volatile_keyword;
  record.has_inline_keyword=entry->has_inline_keyword;
  auto text=entry->asm_string;
  if(!entry->gnu_asm_form || entry->is_asm_goto || entry->labels ||
     !text || text->kind!=ck_string || text->variant.string.length!=1 ||
     !text->variant.string.value || text->variant.string.value[0]!=0 ||
     skip_typerefs(text->type)->kind!=tk_array ||
     skip_typerefs(skip_typerefs(text->type)->variant.array.element_type)->size!=1) {
    record.error="asm outside exact empty GNU memory barrier";return;
  }
  for(auto clobber=entry->clobbers;clobber;clobber=clobber->next) {
    if(clobber->reg!=anr_memory || record.clobbers_memory) {
      record.error="empty asm clobber outside single memory clobber";return;
    }
    record.clobbers_memory=true;
  }
  bool seen_input=false;
  for(auto operand=entry->operands;operand;operand=operand->next) {
    AsmOperand value;value.position=operand->position;
    value.name=operand->name?operand->name:"";
#if RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
    value.constraint=operand->constraints_string?operand->constraints_string:"";
    if((value.constraint=="m" || value.constraint=="rm" || value.constraint=="mr") &&
       !operand->is_output_operand)value.direction="input";
    else if(value.constraint=="=m" && operand->is_output_operand)value.direction="output";
    else if(value.constraint=="+m" && operand->is_output_operand)value.direction="inout";
    else value.error="empty asm operand outside exact memory constraint";
#else
    auto constraint=operand->constraints;
    bool memory_only=constraint && constraint->kind==aoc_mem_any && !constraint->next;
    bool register_memory=constraint && constraint->next && !constraint->next->next &&
      ((constraint->kind==aoc_reg_integer && constraint->next->kind==aoc_mem_any) ||
       (constraint->kind==aoc_mem_any && constraint->next->kind==aoc_reg_integer));
    if(entry->number_of_constraints!=1 || (!memory_only && !register_memory))
      value.error="empty asm operand outside exact memory constraint";
    if(operand->modifiers==aom_input) {
      value.direction="input";
      value.constraint=register_memory ? (constraint->kind==aoc_reg_integer?"rm":"mr") : "m";
    } else if(operand->modifiers==aom_output && memory_only){value.direction="output";value.constraint="=m";}
    else if(operand->modifiers==aom_modify && memory_only){value.direction="inout";value.constraint="+m";}
    else value.error="empty asm operand direction";
#endif
    if(value.direction=="input")seen_input=true;
    else if(seen_input)value.error="empty asm output follows input";
    auto expression=operand->expression;
    if(!expression){record.error="empty asm missing operand expression";return;}
    auto original_type=expression->orig_lvalue_type?expression->orig_lvalue_type:expression->type;
    if(expression->kind==enk_operation && expression->variant.operation.kind==eok_array_to_pointer) {
      auto original=expression->variant.operation.operands;
      if(!original || original->next || !original->is_lvalue || skip_typerefs(original->type)->kind!=tk_array) {
        record.error="empty asm array operand lacks original place";return;
      }
      value.array_address=true;original_type=original->type;value.shape="fixed-array";
    } else if(!expression->is_lvalue && !expression->orig_lvalue_type) {
      value.error="empty asm scalar memory operand lacks original lvalue";
    }
    auto element_source=original_type;
    auto element=skip_typerefs(element_source);
    if(element->kind==tk_array)value.shape="fixed-array";
    while(element->kind==tk_array) {
      AsmDimension dimension;dimension.position=element->source_corresp.decl_position;
      if(element->variant.array.is_vla) {
        dimension.original=find_vla_dimension_in_current_function(element);
        if(!dimension.original){value.error="empty asm VLA lacks original dimension";break;}
        dimension.position=dimension.original->position;value.shape="vla-array";
      } else if(element->variant.array.is_variable_size_array || element->variant.array.is_template_dependent_size_array) {
        value.error="empty asm dependent array extent";break;
      } else {
        dimension.fixed_count=element->variant.array.variant.number_of_elements;
        if(!dimension.fixed_count && !element->variant.array.bound_is_zero) {
          value.error="empty asm operand has an unspecified array bound";break;
        }
        if(dimension.fixed_count && value.fixed_count>(~0ULL)/dimension.fixed_count) {
          value.error="empty asm fixed extent overflow";break;
        }
        value.fixed_count*=dimension.fixed_count;
      }
      value.dimensions.push_back(dimension);
      element_source=element->variant.array.element_type;
      element=skip_typerefs(element_source);
    }
    // For ICU's input "rm" barrier choose its legal memory alternative.
    // Preserve the original alternatives, and keep the existing address-only
    // evaluation contract. Rvalue materialization/array decay needs a separate
    // model; never silently replace an input expression by a different object.
    if((value.constraint=="rm" || value.constraint=="mr") && value.shape!="object")
      value.error="register-or-memory empty asm requires an original object place";
    if(value.shape=="vla-array" && (value.dimensions.size()!=1 || !value.dimensions[0].original))
      value.error="empty asm higher-rank variable array extent";
    if(element->kind==tk_array || element->kind==tk_void || element->kind==tk_routine || !element->size)
      value.error="empty asm incomplete/unsupported operand element";
    if(value.error.empty()) {
      value.element_type=element;value.element_size=element->size;
      value.source_element_type=capture_type(element_source);
      if(value.shape!="vla-array" && value.fixed_count>(~0ULL)/value.element_size)
        value.error="empty asm byte extent overflow";
    }
    record.order.push_back(operand);record.operands.emplace(operand,value);
  }
#else
  (void)entry;record.error="GNU asm support is not configured";
#endif
}
inline void observe_asm_after_lowering(an_asm_entry_ptr entry) {
  if(!enabled())return;
  auto found=state().asm_statements.find(entry);
  if(found==state().asm_statements.end())return;
  auto &record=found->second;record.lowering_observed=true;
  for(auto &pair:record.operands)for(auto &dimension:pair.second.dimensions) {
    if(!dimension.original)continue;
#if DO_IL_LOWERING && LOWER_VARIABLE_LENGTH_ARRAYS
    // Read the frontend's chosen variable. Never synthesize a new dimension
    // variable or re-evaluate its original expression. The stock operand's
    // lowered comma expression already owns bound evaluation/assignment.
    dimension.saved_total=dimension.original->total_number_of_elements;
    if(!dimension.saved_total)pair.second.error="empty asm VLA has no observed saved total";
#else
    pair.second.error="empty asm VLA requires observed lowering total";
#endif
  }
}
inline bool is_observed_tls_routine(a_routine_ptr routine);
inline void capture_routine(a_routine_ptr r) {
  if(!r || abi_primary(r) || is_observed_tls_routine(r))return;
  if(state().generated_static_destructors.count(r))return;
  if((state().generated_array_initializers.count(r) || state().generated_entity_initializers.count(r)))return;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
  if(r->overriding_function_for_wrapper)return;
#endif
  auto &s=state();if(s.functions.count(r))return;
  if(r->special_kind==sfk_lambda_entry_point) {
    if(s.lambda_entries.count(r))return;
    LambdaEntry entry;entry.target=r->variant.lambda_call_operator;
    entry.signature_type=capture_type(r->type);
    s.lambda_entries.emplace(r,entry);
    if(entry.target) {
      capture_routine(entry.target);
      const auto &original=s.functions.at(entry.target);
      s.lambda_entries.at(r).closure_type=s.types.at(original.signature_type-1).object_type;
    }
    return;
  }
  a_type_ptr type=r->type;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  if(r->declared_type)type=r->declared_type;
#endif
  auto id=capture_type(type);
  while(s.types.at(id-1).underlying)id=s.types.at(id-1).underlying;
  const auto signature=s.types.at(id-1);
  Function function;function.function_kind=s.runtime_declarations.count(r)?"runtime":r->special_kind==sfk_constructor?"constructor":r->special_kind==sfk_destructor?"destructor":"ordinary";function.declaration.type=signature.result;function.signature_type=id;
  function.declaration.name=spelling(r->source_corresp);
  function.declaration.position=r->source_corresp.decl_position;
  function.parameters=signature.parameters;
  // Use the immutable list: the physical type may already have an inserted
  // receiver by the time another declaration sharing it is observed.
  function.parameter_defaults=signature.declaration_defaults;
  s.functions[r]=function;
}
inline void observe_routine(a_routine_ptr routine){if(enabled())capture_routine(routine);}
inline const DynamicExceptionSpecification &resolved_dynamic_exception_specification(unsigned id) {
  auto found=state().routine_exception_resolutions.find(id);
  return found==state().routine_exception_resolutions.end()?state().types.at(id-1).dynamic_specification:found->second.dynamic_specification;
}
inline void observe_dynamic_exception_boundary(a_routine_ptr routine) {
  if(!enabled() || !routine || !exceptions_enabled || building_runtime)return;
  auto type=skip_typerefs(routine->type);
  if(type->kind!=tk_routine || exception_policy_flags(type).policy()!="dynamic")return;
  auto primary=abi_primary(routine);if(!primary)primary=routine;
  auto source=state().functions.find(primary);
  if(source==state().functions.end()) {
    state().dynamic_exception_boundary_errors[routine]="dynamic boundary has no original source routine";return;
  }
  auto id=source->second.signature_type;
  const auto original=resolved_dynamic_exception_specification(id);
  auto actual=capture_dynamic_exception_specification(type->variant.routine.extra_info->exception_specification);
  if(resolved_exception_policy(id)!="dynamic" || !original.error.empty() || !actual.error.empty() ||
     original.declared_types.size()!=actual.declared_types.size()) {
    state().dynamic_exception_boundary_errors[routine]="dynamic boundary differs from original selected throw specification";return;
  }
  for(size_t i=0;i<actual.declared_types.size();++i)
    if(!il_identical_types(state().types.at(original.declared_types[i]-1).original_type,
                           state().types.at(actual.declared_types[i]-1).original_type))
      state().dynamic_exception_boundary_errors[routine]="dynamic boundary changed an original allowed exception type";
  state().dynamic_exception_boundaries.emplace(routine,id);
}

inline void observe_lambda_entry_body(a_routine_ptr routine,a_scope_ptr scope,an_expr_node_ptr forwarding) {
  if(!enabled() || routine->special_kind!=sfk_lambda_entry_point)return;
  auto found=state().lambda_entries.find(routine);
  if(found==state().lambda_entries.end())return;
  auto &entry=found->second;entry.scope=scope;entry.forwarding=forwarding;
  for(auto p=scope->variant.routine.parameters;p;p=p->next)entry.parameters.push_back(p);
}
inline void observe_thunk_wrapper(a_routine_ptr routine) {
#if DO_IL_LOWERING && IA64_ABI
  if(!enabled() || !routine->overriding_function_for_wrapper)return;
  ThunkWrapper value;value.target=routine->overriding_function_for_wrapper;
  value.overridden=routine->overridden_function_for_wrapper;
  value.delta=routine->delta;value.vcall_index=routine->vcall_index;
  value.return_delta=routine->return_delta;value.vbase_index=routine->vbase_index;
  state().thunk_wrappers.emplace(routine,value);
#else
  (void)routine;
#endif
}
inline void observe_thunk_body(a_routine_ptr routine,a_scope_ptr scope,an_expr_node_ptr forwarding) {
  if(!enabled())return;
  auto found=state().thunk_wrappers.find(routine);
  if(found==state().thunk_wrappers.end())return;
  auto &value=found->second;value.scope=scope;value.forwarding=forwarding;
  for(auto p=scope->variant.routine.parameters;p;p=p->next)value.parameters.push_back(p);
}
inline void observe_default_adapter(a_routine_ptr target,a_routine_ptr adapter,
                                    a_boolean implied,a_boolean copy,a_boolean has_arguments) {
  if(!enabled())return;
  DefaultAdapter origin;origin.target=target;
  // Retain both the exact callable entry and its original declaration owner.
  // A target that is itself an adapter is rejected by the execution writer.
  auto primary=abi_primary(target);origin.primary=primary?primary:target;
  origin.implied_arguments=implied;origin.copy_constructor=copy;origin.has_arguments=has_arguments;
  state().default_adapters.emplace(adapter,origin);
  if(has_arguments) {
    DefaultArgumentExecution execution;execution.target=target;
    execution.owner=innermost_function_scope?innermost_function_scope->variant.routine.ptr:nullptr;
    state().default_argument_executions.emplace(adapter,execution);
  }
}
inline bool abi_storage_type(a_type_ptr type) {
  type=skip_typerefs(type);
  while(type->kind==tk_array || type->kind==tk_pointer) {
    type=skip_typerefs(type->kind==tk_array?type->variant.array.element_type:type->variant.pointer.type);
  }
  return state().abi_records.count(type)!=0;
}
inline void capture_variable(a_variable_ptr v) {
  auto &s=state();if(s.variables.count(v) || v->is_this_parameter || v->compiler_generated || v->lowering_generated || abi_storage_type(v->type))return;
  a_type_ptr type=v->type;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  if(v->declared_type)type=v->declared_type;
#endif
  Declaration value;value.type=capture_type(type);value.name=spelling(v->source_corresp);
  value.has_initializer=v->init_kind!=initk_none;value.initializer_kind=unsigned(v->init_kind);
#if GNU_EXTENSIONS_ALLOWED
  value.cleanup=v->cleanup_routine;
#endif
  value.position=v->source_corresp.decl_position;s.variables[v]=value;
}
inline a_source_position lifetime_position(an_object_lifetime_ptr lifetime) {
  a_source_position result={};
  if(lifetime->entity.kind==iek_scope) {
    auto scope=reinterpret_cast<a_scope_ptr>(lifetime->entity.ptr);
    if(scope && scope->assoc_block)result=scope->assoc_block->position;
  } else if(lifetime->entity.kind==iek_expr_node) {
    auto expr=reinterpret_cast<an_expr_node_ptr>(lifetime->entity.ptr);
    if(expr)result=expr->position;
  } else if(lifetime->entity.kind==iek_statement) {
    auto stmt=reinterpret_cast<a_statement_ptr>(lifetime->entity.ptr);
    if(stmt)result=stmt->position;
  } else if(lifetime->entity.kind==iek_dynamic_init) {
    auto dip=reinterpret_cast<a_dynamic_init_ptr>(lifetime->entity.ptr);
    if(dip && dip->variable)result=dip->variable->source_corresp.decl_position;
    else if(dip && (dip->kind==dik_expression || dip->kind==dik_class_result_via_ctor) && dip->variant.expression)
      result=dip->variant.expression->position;
    else if(dip && dip->kind==dik_constructor && dip->variant.constructor.args)
      result=dip->variant.constructor.args->position;
  }
  return result;
}
// The frontend links deletion-on-throw to the exact new-expression supplement.
// This is the original repeated-element initializer shape built by
// add_array_nonconstant_aggregate_init and consumed by elem_dynamic_init.
inline void capture_array_allocation_element(a_dynamic_init_ptr allocation) {
  if(!allocation->is_freeing_of_storage_on_exception || !allocation->is_array_freeing || !allocation->assoc_new)return;
  auto operation=allocation->assoc_new;
  if(operation->new_initializer_is_brace_enclosed || operation->new_initializer_is_paren_aggr_init)return;
  auto root=operation->dynamic_init;
  if(!root || root->kind!=dik_nonconstant_aggregate)return;
  auto aggregate=root->variant.constant.ptr;
  if(!aggregate || aggregate->kind!=ck_aggregate)return;
  auto repeat=aggregate->variant.aggregate.first_constant;
  if(!repeat || repeat->kind!=ck_init_repeat || repeat->next)return;
  auto element=repeat->variant.init_repeat.constant;
  if(!element || element->kind!=ck_dynamic_init)return;
  auto initialization=element->variant.dynamic_init.ptr;
  if(initialization && initialization->destructor)
    state().source_array_allocation_elements.emplace(allocation,initialization);
}
inline void observe_new_without_initialization(a_new_delete_supplement_ptr original,
    a_dynamic_init_ptr allocation,a_source_position position) {
  if(!enabled() || !allocation)return;
  UninitializedAllocationSnapshot value;value.original=original;value.position=position;
  if(!original || !original->is_new || original->dynamic_init || original->freeing_of_storage_on_exception ||
     allocation->assoc_new || !allocation->is_freeing_of_storage_on_exception ||
     !original->type || !original->routine || !allocation->destructor ||
     bool(allocation->is_array_freeing)!=(skip_typerefs(original->type)->kind==tk_array)) {
    value.error="original uninitialized allocation lacks exact unassociated new recipe and array shape";
  } else {
    value.allocated_type=original->type;value.allocator=original->routine;value.deallocator=allocation->destructor;
    value.placement=original->placement_new;value.has_initializer=original->has_new_initializer;
  }
  auto inserted=state().uninitialized_allocations.emplace(allocation,value);
  if(!inserted.second && inserted.first->second.original!=original)
    inserted.first->second.error="original uninitialized allocation belongs to multiple new expressions";
}
inline void capture_source_lifetime(an_object_lifetime_ptr lifetime,a_routine_ptr owner,
                                    an_object_lifetime_ptr parent=nullptr,unsigned sibling=0,
                                    a_dynamic_init_ptr initializer=nullptr) {
  if(!lifetime || state().lifetimes.count(lifetime))return;
  LifetimeSnapshot saved;saved.owner=owner;saved.initializer=initializer;saved.original=lifetime;saved.parent=parent;
  saved.parent_obligation=lifetime->parent_destruction_sublist;saved.sibling_ordinal=sibling;
  saved.position=lifetime_position(lifetime);
  if(lifetime->entity.kind==iek_local_static_variable_init) {
    auto init=reinterpret_cast<a_local_static_variable_init_ptr>(lifetime->entity.ptr);
    if(init && init->variable) {
      LocalStaticSnapshot value;value.owner=owner;value.variable=init->variable;value.lifetime=lifetime;
      value.source_type=capture_type(init->variable->type);value.position=init->variable->source_corresp.decl_position;
      state().local_static_initializers.emplace(init->variable,value);
      saved.position=value.position;
    }
  }
  // The function cleanup graph ends at the surrounding global-static source
  // context. That parent owns no automatic action in this call frame. Reject
  // any other outer lifetime rather than silently dropping its relation.
  if(!parent && !initializer && (lifetime->kind!=olk_block || lifetime->parent_destruction_sublist ||
      (lifetime->parent_lifetime && lifetime->parent_lifetime->kind!=olk_global_static)))
    saved.error="source function lifetime has unowned outer lifetime";
  switch(lifetime->kind) {
    case olk_block:saved.kind="block";break;
    case olk_block_after_label:saved.kind="block-after-label";break;
    case olk_expr_temporary:saved.kind="expression-temporary";break;
    case olk_try_block:saved.kind="try-block";break;
    default:saved.error="nonautomatic source cleanup lifetime";break;
  }
  state().lifetimes.emplace(lifetime,saved);state().lifetime_order[owner].push_back(lifetime);
  unsigned ordinal=0;
  for(auto dip=lifetime->destructions;dip;dip=dip->next_in_destruction_list,++ordinal) {
    DestructionSnapshot value;value.owner=owner;value.initializer=initializer;value.original=dip;value.lifetime=lifetime;value.ordinal=ordinal;
    value.destructor=dip->destructor;value.position=saved.position;
    value.allocation_release=dip->is_freeing_of_storage_on_exception;
    value.array_allocation=dip->is_array_freeing;
    value.partial_aggregate=dip->destruction_is_for_partially_constructed_aggregate;
    if(!value.destructor || dip->is_freeing_of_exception_object ||
       dip->is_guard_var_for_local_static_var_init || dip->is_vla_deallocation)
      value.error="source cleanup outside ordinary object destruction";
    if(value.destructor) {
      auto primary=abi_primary(value.destructor);if(primary)value.destructor=primary;
      // Do not freeze an out-of-line destructor signature while parsing an
      // earlier constructor. Its own prelower observer captures that signature
      // after the frontend has selected the definition's equivalent type.
    }
    if(value.allocation_release) {
      // The frontend associates the rollback entry with its precise new
      // expression before lowering. Preserve its allocated type, including
      // arrays, independently of the delete routine's erased void* ABI.
      capture_array_allocation_element(dip);
      if(dip->assoc_new)value.source_type=capture_type(dip->assoc_new->type);
      else if(auto found=state().uninitialized_allocations.find(dip);found!=state().uninitialized_allocations.end()) {
        const auto &allocation=found->second;
        if(!allocation.error.empty())value.error=allocation.error;
        else if((abi_primary(allocation.deallocator)?abi_primary(allocation.deallocator):allocation.deallocator)!=value.destructor)
          value.error="uninitialized allocation changed its original selected deallocator";
        else {
          value.source_type=capture_type(allocation.allocated_type);value.position=allocation.position;
          capture_routine(allocation.allocator);
        }
      } else value.error="source allocation release lacks original new expression";
    } else if(dip->variable) {
      capture_variable(dip->variable);value.position=dip->variable->source_corresp.decl_position;
      auto variable=state().variables.find(dip->variable);
      value.source_type=variable==state().variables.end()?capture_type(dip->variable->type):variable->second.type;
    } else if(value.destructor) {
      auto routine=state().functions.find(value.destructor);
      if(routine!=state().functions.end()) {
        auto signature=routine->second.signature_type;
        value.source_type=state().types.at(signature-1).object_type;
      } else {
        auto signature=skip_typerefs(value.destructor->type);
        if(signature->kind==tk_routine && signature->variant.routine.extra_info->this_class)
          value.source_type=capture_type(signature->variant.routine.extra_info->this_class);
      }
    }
    if(!value.source_type) {
      if(value.error.empty())value.error="source destruction lacks original object type";
      value.error+=" [owner="+(owner?spelling(owner->source_corresp):std::string("<initializer>"))+
        ",ordinal="+std::to_string(ordinal)+",allocation="+std::to_string(value.allocation_release)+
        ",array="+std::to_string(value.array_allocation)+",associated-new="+std::to_string(dip->assoc_new?1:0)+"]";
    }
    if(!state().destructions.emplace(dip,value).second)
      state().destructions.at(dip).error="source destruction belongs to multiple lifetimes";
    state().destruction_order[owner].push_back(dip);
  }
  unsigned child_ordinal=0;
  for(auto child=lifetime->child_lifetime;child;child=child->next)
    capture_source_lifetime(child,owner,lifetime,child_ordinal++,initializer);
}
inline void capture_static_lifetime(an_object_lifetime_ptr lifetime,a_routine_ptr owner,
                                   a_dynamic_init_ptr initializer=nullptr,bool listed=false,unsigned ordinal=0) {
  if(!lifetime)return;
  auto captured=state().static_lifetimes.find(lifetime);
  if(captured!=state().static_lifetimes.end()) {
    // A field-default subtree can be observed before the complete global
    // child list. Attach its exact list position when that original list is
    // subsequently traversed, without recapturing IDs or initializer owners.
    if(listed) {
      if(captured->second.parent!=lifetime->parent_lifetime ||
         captured->second.parent_obligation!=lifetime->parent_destruction_sublist ||
         (captured->second.has_sibling_ordinal && captured->second.sibling_ordinal!=ordinal))
        captured->second.error="original static lifetime has conflicting child-list provenance";
      else {
        captured->second.has_sibling_ordinal=true;
        captured->second.sibling_ordinal=ordinal;
      }
    }
    // When the global graph was observed first, attaching the exact
    // initializer later must preserve original graph IDs,
    // parents and sibling ordering rather than recapture or fabricate roots.
    if(initializer) {
      if(captured->second.initializer && captured->second.initializer!=initializer)
        captured->second.error="original static lifetime belongs to multiple initializer subtrees";
      else captured->second.initializer=initializer;
      for(auto dip=lifetime->destructions;dip;dip=dip->next_in_destruction_list) {
        auto destruction=state().static_destructions.find(dip);
        if(destruction==state().static_destructions.end())continue;
        if(destruction->second.initializer && destruction->second.initializer!=initializer)
          destruction->second.error="original static destruction belongs to multiple initializer subtrees";
        else destruction->second.initializer=initializer;
      }
      unsigned sibling=0;
      for(auto child=lifetime->child_lifetime;child;child=child->next)
        capture_static_lifetime(child,owner,initializer,true,sibling++);
    }
    return;
  }
  StaticLifetimeSnapshot value;value.original=lifetime;value.owner=owner;value.initializer=initializer;
  value.parent=lifetime->parent_lifetime;value.parent_obligation=lifetime->parent_destruction_sublist;
  value.has_sibling_ordinal=listed;value.sibling_ordinal=ordinal;value.position=lifetime_position(lifetime);
  if(lifetime->entity.kind==iek_dynamic_init) {
    auto dip=reinterpret_cast<a_dynamic_init_ptr>(lifetime->entity.ptr);
    if(dip)value.entity_description=" dynamic-init-kind="+std::to_string(dip->kind)+
      " dynamic-init-variable="+(dip->variable?spelling(dip->variable->source_corresp):"none")+
      " dynamic-init-lifetime-kind="+(dip->lifetime?std::to_string(dip->lifetime->kind):"none");
  }
  switch(lifetime->kind) {
    case olk_global_static:value.kind="global-static";break;
    case olk_function_static:value.kind="function-static";break;
    case olk_block:value.kind="block";break;
    case olk_block_after_label:value.kind="block-after-label";break;
    case olk_expr_temporary:value.kind="expression-temporary";break;
    case olk_try_block:value.kind="try-block";break;
    default:value.kind="unsupported";break;
  }
  state().static_lifetimes.emplace(lifetime,value);state().static_lifetime_order.push_back(lifetime);
  unsigned destruction_ordinal=0;
  for(auto dip=lifetime->destructions;dip;dip=dip->next_in_destruction_list,++destruction_ordinal) {
    StaticDestructionSnapshot saved;saved.original=dip;saved.initializer=initializer;saved.owner=owner;
    saved.lifetime=lifetime;saved.ordinal=destruction_ordinal;saved.selected_routine=dip->destructor;
    saved.variable=dip->variable;saved.partial_aggregate=dip->destruction_is_for_partially_constructed_aggregate;
    saved.persistent=!saved.partial_aggregate && (lifetime->kind==olk_global_static || lifetime->kind==olk_function_static);
    saved.allocation=dip->is_freeing_of_storage_on_exception;saved.array_allocation=dip->is_array_freeing;
    saved.position=value.position;
    if(saved.selected_routine) {
      auto primary=abi_primary(saved.selected_routine);if(primary)saved.selected_routine=primary;
    } else saved.error="static source graph obligation lacks selected routine";
    if(saved.variable) {
      capture_variable(saved.variable);saved.source_type=capture_type(saved.variable->type);
      saved.position=saved.variable->source_corresp.decl_position;
      saved.thread=var_has_thread_storage_duration(saved.variable);
    } else if(saved.allocation && dip->assoc_new) saved.source_type=capture_type(dip->assoc_new->type);
    else if(saved.selected_routine) {
      auto type=skip_typerefs(saved.selected_routine->type);
      if(type->kind==tk_routine && type->variant.routine.extra_info->this_class)
        saved.source_type=capture_type(type->variant.routine.extra_info->this_class);
    }
    if(saved.allocation && dip->assoc_new)saved.source_type=capture_type(dip->assoc_new->type);
    if(!saved.source_type)saved.error="static source graph obligation lacks original type";
    if(state().static_destructions.emplace(dip,saved).second)state().static_destruction_order.push_back(dip);
  }
  unsigned sibling=0;
  for(auto child=lifetime->child_lifetime;child;child=child->next)
    capture_static_lifetime(child,owner,initializer,true,sibling++);
}
struct SourceInitializerTraversal : an_expr_or_stmt_traversal_block {
  a_dynamic_init_ptr initializer=nullptr;
  a_routine_ptr owner=nullptr;
};
inline void capture_record_temporary(an_expr_node_ptr expr,an_expr_or_stmt_traversal_block_ptr traversal) {
  if(!expr || expr->kind!=enk_temp_init)return;
  auto type=skip_typerefs(expr->type);
  while(type->kind==tk_array)type=skip_typerefs(type->variant.array.element_type);
  if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)return;
  auto dip=expr->variant.init.dynamic_init;
  if(!dip || state().record_temporaries.count(dip))return;
  auto &context=*static_cast<SourceInitializerTraversal*>(traversal);
  SourceRecordTemporary value;value.original=dip;value.owner=context.initializer?nullptr:context.owner;value.initializer=context.initializer;
  value.source_type=capture_type(expr->type);value.initialization_kind=unsigned(dip->kind);value.position=expr->position;
  value.ordinal=state().record_temporary_ordinals[std::make_pair(value.owner,value.initializer)]++;
  state().record_temporaries.emplace(dip,value);state().record_temporary_order.push_back(dip);
}
inline void capture_initializer_nested_destruction(a_dynamic_init_ptr dip,an_expr_or_stmt_traversal_block_ptr traversal) {
  auto &context=*static_cast<SourceInitializerTraversal*>(traversal);
  if(dip->lifetime && (!context.owner || dip->lifetime->kind==olk_global_static || dip->lifetime->kind==olk_function_static)) {
    bool static_root=dip->lifetime->kind==olk_global_static || dip->lifetime->kind==olk_function_static;
    capture_static_lifetime(dip->lifetime,context.owner,static_root?nullptr:context.initializer);
  }
  auto found=state().static_destructions.find(dip);
  if(found!=state().static_destructions.end()) {
    if(found->second.initializer && found->second.initializer!=context.initializer)
      found->second.error="original destruction belongs to multiple source initializer subtrees";
    else found->second.initializer=context.initializer;
  }
  // Nested full expressions can own temporary destruction/allocation lists
  // independently of the aggregate's outer initialization-expression lifetime.
  // Keep those original boundaries; execution adoption is observed separately.
  if(!context.owner && dip->init_expr_lifetime) {
    capture_static_lifetime(dip->init_expr_lifetime,nullptr,context.initializer);
    if(!state().lifetimes.count(dip->init_expr_lifetime))
      capture_source_lifetime(dip->init_expr_lifetime,nullptr,nullptr,0,context.initializer);
  }
}
inline void capture_source_initializer(a_dynamic_init_ptr dip,a_routine_ptr owner,unsigned ordinal) {
  if(!dip || !dip->variable || state().source_initializers.count(dip))return;
  SourceInitializer value;value.original=dip;value.variable=dip->variable;value.owner=owner;value.ordinal=ordinal;
  value.source_name=spelling(dip->variable->source_corresp);
  value.source_type=capture_type(dip->variable->type);value.position=dip->variable->source_corresp.decl_position;
  value.expression_lifetime=dip->init_expr_lifetime;
  capture_variable(dip->variable);
  state().source_initializers.emplace(dip,value);state().source_initializer_order.push_back(dip);
  if(dip->lifetime)capture_static_lifetime(dip->lifetime,owner);
  auto obligation=state().static_destructions.find(dip);
  if(obligation!=state().static_destructions.end())obligation->second.initializer=dip;
  if(dip->init_expr_lifetime && !owner) {
    capture_static_lifetime(dip->init_expr_lifetime,owner,dip);
    capture_source_lifetime(dip->init_expr_lifetime,nullptr,nullptr,0,dip);
  }
  SourceInitializerTraversal traversal;clear_expr_or_stmt_traversal_block(&traversal);
  traversal.initializer=dip;traversal.owner=owner;
  traversal.process_non_dynamic_constants=TRUE;
  traversal.process_expr=capture_record_temporary;
  traversal.process_dynamic_init=capture_initializer_nested_destruction;
  traverse_dynamic_init(dip,&traversal);
}
// A default member initializer is a source template. Stock never lowers this
// original DIP: it copies it into each selected constructor, and the root
// destruction has no original lifetime until that execution copy is attached.
inline void capture_field_initializer(a_field_ptr field,a_type_ptr owner) {
  if(!field || !field->initializer)return;
  auto dip=field->initializer;
  if(state().source_initializers.count(dip))return;
  SourceInitializer value;value.original=dip;value.field=field;value.field_owner=owner;
  value.source_name=spelling(field->source_corresp);value.position=field->source_corresp.decl_position;
  value.source_type=capture_type(field->type);value.field_destructor=dip->destructor;
  if(auto primary=abi_primary(value.field_destructor))value.field_destructor=primary;
  value.expression_lifetime=dip->init_expr_lifetime;
  state().source_initializers.emplace(dip,value);state().source_initializer_order.push_back(dip);
  if(dip->init_expr_lifetime) {
    capture_static_lifetime(dip->init_expr_lifetime,nullptr,dip);
    capture_source_lifetime(dip->init_expr_lifetime,nullptr,nullptr,0,dip);
  }
  SourceInitializerTraversal traversal;clear_expr_or_stmt_traversal_block(&traversal);
  traversal.initializer=dip;traversal.process_non_dynamic_constants=TRUE;
  traversal.process_expr=capture_record_temporary;
  traversal.process_dynamic_init=capture_initializer_nested_destruction;
  traverse_dynamic_init(dip,&traversal);
}
inline a_dynamic_init_ptr original_field_initializer(a_dynamic_init_ptr value,a_routine_ptr owner) {
  auto primary=abi_primary(owner);if(primary)owner=primary;
  std::set<a_dynamic_init_ptr> seen;
  while(value && seen.insert(value).second) {
    auto found=state().source_initializers.find(value);
    if(found!=state().source_initializers.end() && found->second.field &&
       state().field_initializer_executions.count(std::make_pair(value,owner)))return value;
    auto copy=state().dynamic_init_copies.find(value);
    if(copy==state().dynamic_init_copies.end())break;
    value=copy->second;
  }
  return nullptr;
}
inline void observe_field_initializer_selection(a_scope_ptr scope,a_constructor_init_ptr init,bool no_effect) {
  if(!enabled() || !init || init->kind!=cik_field)return;
  auto field=init->variant.field;auto owner=scope->variant.routine.ptr;
  capture_field_initializer(field,parent_class_of(owner));
  auto dip=field->initializer;if(!dip)return;
  auto ordinal=state().field_initializer_ordinals.find(init);
  if(ordinal==state().field_initializer_ordinals.end())return;
  auto primary=abi_primary(owner);if(primary)owner=primary;
  state().field_initializer_executions.emplace(std::make_pair(dip,owner),FieldInitializerExecution{ordinal->second,no_effect});
  if(!no_effect)for(const auto &lifetime:state().lifetimes)if(lifetime.second.initializer==dip)
    state().initializer_lifetime_adoptions.emplace(std::make_pair(lifetime.first,owner),dip);
}
// The original field DIP is completed lazily when its constructor is
// selected. Observe the finalized destructor before stock copies that DIP;
// never infer original selection from a lowered cleanup callback.
inline void finalize_field_initializer_destructor(a_field_ptr field,a_type_ptr owner) {
  if(!field || !field->initializer)return;
  auto found=state().source_initializers.find(field->initializer);
  if(found==state().source_initializers.end())return;
  auto &source=found->second;auto destructor=field->initializer->destructor;
  if(auto primary=abi_primary(destructor))destructor=primary;
  if(source.field!=field || source.field_owner!=owner || source.original!=field->initializer ||
     (source.field_destructor && source.field_destructor!=destructor)) {
    source.error="finalized field initializer destructor conflicts with its original declaration";return;
  }
  source.field_destructor=destructor;
  if(destructor)capture_routine(destructor);
}
struct FieldInitializerCapture {
  a_scope_ptr scope=nullptr;
  FieldInitializerCapture(a_scope_ptr value) {
    if(!enabled())return;scope=value;unsigned ordinal=0;
    for(auto init=scope->variant.routine.constructor_inits;init;init=init->next,++ordinal) {
      if(init->kind!=cik_field || !init->use_field_initializer)continue;
      capture_field_initializer(init->variant.field,parent_class_of(scope->variant.routine.ptr));
      finalize_field_initializer_destructor(init->variant.field,parent_class_of(scope->variant.routine.ptr));
      state().field_initializer_ordinals.emplace(init,ordinal);
    }
  }
};
// Observation scope only: stock lowering still creates/inlines the call. The
// scoped context links that exact call to its originating destruction entry.
struct NormalDestructionCapture {
  bool active=false;
  NormalDestructionCapture(a_dynamic_init_ptr dip,a_routine_ptr owner) {
    active=enabled();if(!active)return;
    NormalDestruction value;value.owner=owner;value.original=dip;
    state().normal_destruction_contexts.push_back(value);
  }
  ~NormalDestructionCapture() {if(active)state().normal_destruction_contexts.pop_back();}
};
inline void observe_normal_destruction_call(a_routine_ptr routine,an_expr_node_ptr call,
                                            an_expr_node_ptr retained,a_routine_ptr owner) {
  if(!enabled() || state().normal_destruction_contexts.empty() || !call || call!=retained ||
     call->kind!=enk_operation || call->variant.operation.kind!=eok_call)return;
  auto value=state().normal_destruction_contexts.back();
  if(value.owner!=owner || !value.original || !value.original->destructor)return;
  auto primary=abi_primary(routine);if(!primary)primary=routine;
  auto original=abi_primary(value.original->destructor);if(!original)original=value.original->destructor;
  if(primary!=original || primary->special_kind!=sfk_destructor)return;
  value.callback=routine;
  if(state().normal_destruction_calls.emplace(call,value).second)
    state().normal_destruction_order[owner].push_back(call);
}
inline void observe_array_runtime(a_dynamic_init_ptr dip,a_routine_ptr callback,an_expr_node_ptr call,a_routine_ptr owner,bool construction) {
  if(!enabled() || !callback || !call)return;
  // IA64 __cxa_vec_ctor returns void; stock lowering wraps its actual call
  // in (call, destination). Attribute ownership to the call, not the comma.
  while(call->kind==enk_operation && call->variant.operation.kind==eok_comma)
    call=call->variant.operation.operands;
  if(!call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call)return;
  if(!dip) {
    if(state().normal_destruction_contexts.empty())return;
    const auto &context=state().normal_destruction_contexts.back();
    if(context.owner!=owner)return;
    dip=context.original;
  }
  if(!dip)return;
  ArrayRuntimeSnapshot value;value.owner=owner;value.original=dip;value.callback=callback;
  value.operation=construction?"construct":"destroy";
  if(state().array_runtime_calls.emplace(call,value).second)state().array_runtime_order[owner].push_back(call);
}
inline void observe_normal_destruction_copy(an_expr_node_ptr original,an_expr_node_ptr copy,a_routine_ptr owner) {
  if(!enabled() || !owner || original==copy || !copy || copy->kind!=enk_operation ||
     copy->variant.operation.kind!=eok_call)return;
  auto array=state().array_runtime_calls.find(original);
  if(array!=state().array_runtime_calls.end()) {
    auto value=array->second;
    auto primary=abi_primary(owner);if(!primary)primary=owner;
    auto original_owner=abi_primary(value.owner);if(!original_owner)original_owner=value.owner;
    if(primary==original_owner) {
      value.owner=owner;
      if(state().array_runtime_calls.emplace(copy,value).second)state().array_runtime_order[owner].push_back(copy);
    }
  }
  auto found=state().normal_destruction_calls.find(original);
  if(found==state().normal_destruction_calls.end())return;
  auto value=found->second;auto old_owner=abi_primary(value.owner);if(!old_owner)old_owner=value.owner;
  auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
  auto target=copy->variant.operation.operands;
  if(old_owner!=new_owner || !target || target->kind!=enk_routine)return;
  auto callback=target->variant.routine.ptr;auto primary=abi_primary(callback);if(!primary)primary=callback;
  auto expected=abi_primary(value.callback);if(!expected)expected=value.callback;
  if(primary!=expected)return;
  value.owner=owner;value.callback=callback;
  if(state().normal_destruction_calls.emplace(copy,value).second)
    state().normal_destruction_order[owner].push_back(copy);
}
inline void observe_virtual_base_adjustment(an_expr_node_ptr result,an_expr_node_ptr source_object,
    an_expr_node_ptr lookup_object,an_expr_node_ptr offset,a_type_ptr derived_type,a_base_class_ptr base,a_routine_ptr owner) {
  if(!enabled() || !owner || !result || !source_object || !offset || !base || !base->is_virtual)return;
  VirtualBaseAdjustmentSnapshot value;value.owner=owner;value.source_object=source_object;
  // field_lvalue_selection_expr cancels a generated (*p).field into
  // p->field. Retain p itself, which survives that exact stock rewrite,
  // rather than the discarded dereference shell.
  if(lookup_object && lookup_object->kind==enk_operation &&
     lookup_object->variant.operation.kind==eok_indirect)
    lookup_object=lookup_object->variant.operation.operands;
  value.lookup_object=lookup_object;value.offset=offset;value.derived_type=derived_type;value.base_type=base->type;
  value.vtable_index=static_cast<long long>(base->vbase_offset_index);
  state().virtual_base_adjustments[result]=value;
}
inline an_expr_node_ptr corresponding_virtual_base_child(an_expr_node_ptr original,an_expr_node_ptr copy,
                                                         an_expr_node_ptr wanted,unsigned depth=0) {
  if(!original || !copy || depth>128)return nullptr;
  if(original==wanted)return copy;
  if(original->kind!=enk_operation || copy->kind!=enk_operation ||
     original->variant.operation.kind!=copy->variant.operation.kind)return nullptr;
  auto a=original->variant.operation.operands,b=copy->variant.operation.operands;
  for(;a && b;a=a->next,b=b->next) {
    auto found=corresponding_virtual_base_child(a,b,wanted,depth+1);if(found)return found;
  }
  return nullptr;
}
inline void observe_virtual_base_adjustment_copy(an_expr_node_ptr original,an_expr_node_ptr copy,a_routine_ptr owner) {
  if(!enabled() || !owner || original==copy)return;
  auto found=state().virtual_base_adjustments.find(original);if(found==state().virtual_base_adjustments.end())return;
  auto old_owner=abi_primary(found->second.owner);if(!old_owner)old_owner=found->second.owner;
  auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
  if(old_owner!=new_owner)return;
  auto value=found->second;value.owner=owner;
  value.source_object=corresponding_virtual_base_child(original,copy,value.source_object);
  value.lookup_object=corresponding_virtual_base_child(original,copy,value.lookup_object);
  value.offset=corresponding_virtual_base_child(original,copy,value.offset);
  state().virtual_base_adjustments[copy]=value;
}
inline void observe_virtual_base_adjustment_overwrite(an_expr_node_ptr destination,an_expr_node_ptr source,a_routine_ptr owner) {
  if(!enabled() || !owner || destination==source)return;
  state().virtual_base_adjustments.erase(destination);
  observe_virtual_base_adjustment_copy(source,destination,owner);
}
inline void observe_virtual_base_null_preservation(an_expr_node_ptr wrapper,an_expr_node_ptr adjusted) {
  if(!enabled())return;
  auto original=state().virtual_base_adjustments.find(wrapper);
  if(original==state().virtual_base_adjustments.end())return;
  auto copied=state().virtual_base_adjustments.find(adjusted);
  // add_null_preservation_code copied the adjusted pointer into its true
  // branch. That branch owns the proof; the new conditional is an ordinary
  // null-preserving pointer expression, not another virtual-base lookup.
  if(copied!=state().virtual_base_adjustments.end() &&
     copied->second.owner==original->second.owner &&
     copied->second.derived_type==original->second.derived_type &&
     copied->second.base_type==original->second.base_type &&
     copied->second.vtable_index==original->second.vtable_index)
    state().virtual_base_adjustments.erase(original);
}
// Stock lowering sometimes saves an already-constructed class value in raw
// execution storage. This is independent of the original C++ copy operations.
inline void observe_record_storage_copy(an_expr_node_ptr expr,a_routine_ptr owner,const char *kind) {
  if(!enabled() || !owner || !expr || expr->kind!=enk_operation ||
     expr->variant.operation.kind!=eok_assign)return;
  auto type=skip_typerefs(expr->type);
  if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)return;
  auto target=expr->variant.operation.operands;auto value=target?target->next:nullptr;
  if(!target || !value || value->next)return;
  RecordStorageCopySnapshot copy;copy.owner=owner;copy.target=target;copy.value=value;
  copy.type=type;copy.bytes=type->size;copy.kind=kind;
  state().record_storage_copies[expr]=copy;
}
inline void observe_record_storage_copy_copy(an_expr_node_ptr original,an_expr_node_ptr copy,a_routine_ptr owner) {
  if(!enabled() || !owner || original==copy || !copy)return;
  auto found=state().record_storage_copies.find(original);
  if(found==state().record_storage_copies.end())return;
  auto old_owner=abi_primary(found->second.owner);if(!old_owner)old_owner=found->second.owner;
  auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
  if(old_owner!=new_owner || original->kind!=enk_operation || original->variant.operation.kind!=eok_assign ||
     original->variant.operation.operands!=found->second.target ||
     found->second.target->next!=found->second.value)return;
  observe_record_storage_copy(copy,owner,found->second.kind.c_str());
  auto copied=state().record_storage_copies.find(copy);
  if(copied!=state().record_storage_copies.end()) {
    copied->second.type=found->second.type;copied->second.bytes=found->second.bytes;
  }
}
inline void observe_record_transfer(an_expr_node_ptr expr,a_routine_ptr owner,const char *kind) {
  if(!enabled() || !owner || !expr || expr->kind!=enk_operation ||
     expr->variant.operation.kind!=eok_assign)return;
  auto type=skip_typerefs(expr->type);
  if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)return;
  auto target=expr->variant.operation.operands;
  auto value=target?target->next:nullptr;
  if(!target || !value || value->next)return;
  RecordTransferSnapshot record;record.owner=owner;record.target=target;record.value=value;record.kind=kind;
  state().record_transfers[expr]=record;
}
inline void observe_pooled_record_constant(a_constant_ptr constant,a_variable_ptr variable) {
  if(!enabled() || !constant || !variable || constant->kind!=ck_aggregate)return;
  auto type=skip_typerefs(constant->type);
  if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)return;
  state().pooled_record_constants[variable]=constant;
}
inline void observe_record_constant_initialization(an_expr_node_ptr expression,a_dynamic_init_ptr initializer,a_constant_ptr constant) {
  if(!enabled())return;
  auto found=state().record_transfers.find(expression);if(found==state().record_transfers.end())return;
  auto kind=initializer?initializer->kind:constant?dik_constant:dik_none;
  if(kind!=dik_zero && kind!=dik_constant)return;
  auto &value=found->second;
  if(value.kind!="construction" || value.value->kind!=enk_variable)return;
  auto variable=node_variable(value.value);
  if(!state().pooled_record_constants.count(variable) ||
     !(variable->compiler_generated || variable->lowering_generated))return;
  value.constant_storage=variable;value.constant_initialization_kind=kind==dik_zero?"zero":"constant";
}
inline void observe_record_transfer_copy(an_expr_node_ptr original,an_expr_node_ptr copy,a_routine_ptr owner) {
  observe_record_storage_copy_copy(original,copy,owner);
  if(!enabled() || !owner || original==copy || !copy)return;
  auto found=state().record_transfers.find(original);
  if(found==state().record_transfers.end())return;
  auto old_owner=abi_primary(found->second.owner);if(!old_owner)old_owner=found->second.owner;
  auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
  if(old_owner!=new_owner || original->kind!=enk_operation || original->variant.operation.kind!=eok_assign ||
     original->variant.operation.operands!=found->second.target ||
     found->second.target->next!=found->second.value)return;
  observe_record_transfer(copy,owner,found->second.kind.c_str());
  auto copied=state().record_transfers.find(copy);
  if(copied!=state().record_transfers.end()) {
    copied->second.constant_storage=found->second.constant_storage;
    copied->second.constant_initialization_kind=found->second.constant_initialization_kind;
  }
}
// overwrite_node promotes a lowered child without calling copy_node. Preserve
// only that child's existing exact assignment proof, clearing stale metadata
// when the new contents do not carry any record initialization observation.
inline void observe_record_transfer_overwrite(an_expr_node_ptr destination,an_expr_node_ptr source,a_routine_ptr owner) {
  if(!enabled() || !owner || destination==source)return;
  state().record_transfers.erase(destination);
  state().record_storage_copies.erase(destination);
  observe_record_transfer_copy(source,destination,owner);
}
// The throw initializer owns payload addresses and start markers. These maps
// retain exact IL correspondence through both shallow and recursive copies.
struct ThrowCapture {
  an_expr_node_ptr expr=nullptr;
  ThrowCapture(an_expr_node_ptr value,a_routine_ptr owner) {
    if(!enabled())return;
    expr=value;ThrowSnapshot saved;saved.original=value;saved.owner=owner;
    if(!owner || !value || value->kind!=enk_throw) {expr=nullptr;return;}
    auto tsp=value->variant.throw_info;saved.rethrow=tsp==nullptr;
    if(tsp) {
      saved.source_type=capture_type(tsp->type);saved.payload_type=tsp->type;
      saved.destructor=tsp->destructor;
      if(saved.destructor) {auto primary=abi_primary(saved.destructor);if(primary)saved.destructor=primary;}
      auto leaf=tsp->type;unsigned depth=0;
      while(true) {
        if(is_or_was_nullptr_type(leaf)) {saved.error="nullptr exception identity not yet owned";break;}
        leaf=skip_typerefs(leaf);
        if(leaf->kind!=tk_pointer)break;
        if(leaf->variant.pointer.is_reference) {saved.error="reference exception payload";break;}
        ++depth;leaf=leaf->variant.pointer.type;
      }
      saved.leaf_type=skip_typerefs(leaf);leaf=saved.leaf_type;
      if(leaf->kind==tk_integer) {
        auto &i=leaf->variant.integer;
        if((i.enum_type && !i.bool_type) || i.wchar_t_type || i.char8_t_type || i.char16_t_type || i.char32_t_type)
          saved.error="exception nominal/special-character identity not yet owned";
      } else if(leaf->kind==tk_struct || leaf->kind==tk_class || leaf->kind==tk_union) {
        if(leaf->incomplete)saved.error="incomplete exception record leaf";
      } else if(leaf->kind!=tk_float && !(leaf->kind==tk_void && depth))
        saved.error="exception type outside builtin/record/data-pointer identity";
      saved.leaf_source_type=capture_type(leaf);
      auto payload=skip_typerefs(tsp->type);saved.size=payload->size;saved.alignment=payload->alignment;
    }
    state().throws[value]=saved;state().active_throws.push_back(value);
  }
  ~ThrowCapture() {
    if(!expr)return;
    auto &saved=state().throws.at(expr);
    if(expr->kind!=enk_throw)saved.error="observed throw was fully lowered";
    else if(saved.rethrow) {
      if(expr->variant.throw_info)saved.error="observed rethrow gained payload";
      else saved.complete=true;
    } else if(!expr->variant.throw_info || expr->variant.throw_info->dynamic_init)
      saved.error="observed throw initializer was not consumed";
    else {saved.initializer=expr->variant.throw_info->expr;saved.complete=true;}
    state().active_throws.pop_back();
  }
};
inline void observe_throw_construct(an_expr_node_ptr expr,bool marker) {
  if(!enabled() || state().active_throws.empty())return;
  auto owner=state().active_throws.back();
  if(marker)state().throw_markers[expr]=owner;
  else state().throw_addresses[expr]=owner;
}
inline void observe_array_protocol_overwrite(an_expr_node_ptr destination,an_expr_node_ptr source,a_routine_ptr owner) {
  if(enabled() && destination!=source) {
    state().generated_entity_initializer_calls.erase(destination);
    auto call=state().generated_entity_initializer_calls.find(source);
    if(call!=state().generated_entity_initializer_calls.end() && state().generated_entity_initializers.at(call->second).origin.owner==owner)
      state().generated_entity_initializer_calls.emplace(destination,call->second);
  }
  if(!enabled() || !owner || destination==source)return;
  state().array_allocation_calls.erase(destination);
  auto allocation=state().array_allocation_calls.find(source);
  if(allocation!=state().array_allocation_calls.end() && allocation->second.owner==owner)
    state().array_allocation_calls.emplace(destination,allocation->second);
  state().array_deletion_calls.erase(destination);
  auto deletion=state().array_deletion_calls.find(source);
  if(deletion!=state().array_deletion_calls.end() && deletion->second.owner==owner)
    state().array_deletion_calls.emplace(destination,deletion->second);
  state().array_callback_addresses.erase(destination);
  auto callback=state().array_callback_addresses.find(source);
  if(callback!=state().array_callback_addresses.end() && callback->second.first==owner)
    state().array_callback_addresses.emplace(destination,callback->second);
}
inline void observe_throw_copy(an_expr_node_ptr original,an_expr_node_ptr copy,a_routine_ptr owner) {
  if(!enabled() || !owner || !original || !copy || original==copy)return;
  auto array_callback=state().array_callback_addresses.find(original);
  if(array_callback!=state().array_callback_addresses.end() && array_callback->second.first==owner)
    state().array_callback_addresses.emplace(copy,array_callback->second);
  auto array_deletion=state().array_deletion_calls.find(original);
  if(array_deletion!=state().array_deletion_calls.end() && array_deletion->second.owner==owner)
    state().array_deletion_calls.emplace(copy,array_deletion->second);
  auto array_allocation=state().array_allocation_calls.find(original);
  if(array_allocation!=state().array_allocation_calls.end() && array_allocation->second.owner==owner)
    state().array_allocation_calls.emplace(copy,array_allocation->second);
  auto registration=state().static_registrations.find(original);
  if(registration!=state().static_registrations.end() && registration->second.owner==owner)
    state().static_registrations.emplace(copy,registration->second);
  auto guard=state().local_static_guard_operations.find(original);
  if(guard!=state().local_static_guard_operations.end()) {
    auto storage=state().local_static_guard_variables.at(guard->second.first);
    auto source_owner=state().local_static_initializers.at(storage).owner;
    auto primary=abi_primary(owner);if(!primary)primary=owner;
    if(source_owner==primary)state().local_static_guard_operations.emplace(copy,guard->second);
  }
  auto found=state().throws.find(original);
  if(found!=state().throws.end() && copy->kind==enk_throw) {
    auto saved=found->second;
    auto old_owner=abi_primary(saved.owner);if(!old_owner)old_owner=saved.owner;
    auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
    if(old_owner==new_owner && saved.complete &&
       saved.rethrow==(copy->variant.throw_info==nullptr)) {
      saved.owner=owner;saved.initializer=copy->variant.throw_info?copy->variant.throw_info->expr:nullptr;
      state().throws[copy]=saved;
    }
  }
  if(copy->kind==enk_lowered_eh_construct) {
    if(copy->variant.lowered_eh.kind==leck_internal_try) {
      // insert_expr shallow-copies an existing root before replacing that
      // root with a comma. Preserve the exact rollback identity on the new
      // internal-try node, just as for copied throw/lifetime constructs.
      auto allocation=state().allocation_internal_tries.find(original);
      if(allocation!=state().allocation_internal_tries.end()) {
        auto old_owner=abi_primary(allocation->second.first);
        if(!old_owner)old_owner=allocation->second.first;
        auto new_owner=abi_primary(owner);if(!new_owner)new_owner=owner;
        if(old_owner==new_owner)
          state().allocation_internal_tries.emplace(copy,std::make_pair(owner,allocation->second.second));
      }
    } else if(copy->variant.lowered_eh.kind==leck_thrown_object_address) {
      auto address=state().throw_addresses.find(original);
      if(address!=state().throw_addresses.end())state().throw_addresses[copy]=address->second;
    } else if(copy->variant.lowered_eh.kind==leck_exception_started) {
      auto marker=state().throw_markers.find(original);
      if(marker!=state().throw_markers.end())state().throw_markers[copy]=marker->second;
    }
  }
}
inline void observe_lifetime_copy(an_object_lifetime_ptr original,an_object_lifetime_ptr copy) {
  if(enabled() && original && copy && original!=copy)state().lifetime_copies.emplace(copy,original);
}
inline an_object_lifetime_ptr source_lifetime(an_object_lifetime_ptr value,a_routine_ptr owner) {
  auto execution_owner=owner;
  auto primary=abi_primary(owner);if(primary)owner=primary;
  std::set<an_object_lifetime_ptr> seen;
  while(value && seen.insert(value).second) {
    auto direct=state().lifetimes.find(value);
    if(direct!=state().lifetimes.end() && (direct->second.owner==owner ||
       state().default_argument_lifetime_adoptions.count(std::make_pair(value,execution_owner)) ||
       (direct->second.initializer && state().initializer_lifetime_adoptions.count(std::make_pair(value,owner)))))return value;
    auto copied=state().lifetime_copies.find(value);
    if(copied==state().lifetime_copies.end())break;
    value=copied->second;
  }
  return nullptr;
}
inline void observe_dynamic_init_copy(a_dynamic_init_ptr original,a_dynamic_init_ptr copy) {
  if(!enabled() || original==copy)return;
  state().dynamic_init_copies.emplace(copy,original);
  if(state().active_default_argument_copies.empty())return;
  auto adapter=state().active_default_argument_copies.back();
  auto &execution=state().default_argument_executions.at(adapter);
  std::set<a_dynamic_init_ptr> seen;
  for(auto candidate=original;candidate && seen.insert(candidate).second;) {
    auto source=state().destructions.find(candidate);
    if(source!=state().destructions.end()) {
      if(!source->second.owner || source->second.initializer || !source->second.lifetime || !copy->lifetime ||
         (execution.source_owner && execution.source_owner!=source->second.owner))
        execution.error="default arguments have unsupported mixed original owners or absent copied lifetime";
      else {
        execution.source_owner=source->second.owner;
        execution.obligations.emplace(copy,candidate);
        auto inserted=execution.lifetimes.emplace(copy->lifetime,source->second.lifetime);
        if(!inserted.second && inserted.first->second!=source->second.lifetime)
          execution.error="generated argument lifetime merges distinct original boundaries";
      }
      break;
    }
    auto prior=state().dynamic_init_copies.find(candidate);
    if(prior==state().dynamic_init_copies.end())break;
    candidate=prior->second;
  }
  seen.clear();
  for(auto candidate=original;candidate && seen.insert(candidate).second;) {
    auto source=state().record_temporaries.find(candidate);
    if(source!=state().record_temporaries.end()) {
      if(!source->second.owner || source->second.initializer ||
         (execution.source_owner && execution.source_owner!=source->second.owner))
        execution.error="default argument temporary has a different original owner";
      else {execution.source_owner=source->second.owner;execution.temporaries.emplace(copy,candidate);}
      break;
    }
    auto prior=state().dynamic_init_copies.find(candidate);
    if(prior==state().dynamic_init_copies.end())break;
    candidate=prior->second;
  }
}
inline void observe_default_argument_copy_begin(a_routine_ptr adapter,a_scope_ptr scope,
                                                an_object_lifetime_ptr lifetime) {
  if(!enabled())return;
  auto found=state().default_argument_executions.find(adapter);
  if(found==state().default_argument_executions.end())return;
  auto &execution=found->second;execution.scope=scope;execution.root=lifetime;
  state().active_default_argument_copies.push_back(adapter);
}
inline void observe_default_argument_copy_end(a_routine_ptr adapter) {
  if(!enabled())return;
  auto found=state().default_argument_executions.find(adapter);
  if(found==state().default_argument_executions.end())return;
  auto &execution=found->second;
  if(state().active_default_argument_copies.empty() || state().active_default_argument_copies.back()!=adapter) {
    execution.error="default argument copy stack mismatch";return;
  }
  state().active_default_argument_copies.pop_back();
  if(execution.obligations.empty() && execution.temporaries.empty())return;
  auto primary=execution.owner?abi_primary(execution.owner):nullptr;
  if(!execution.owner || (primary?primary:execution.owner)!=execution.source_owner)
    execution.error="default argument factory caller differs from the copied source owner";
  if(execution.obligations.empty() || !execution.lifetimes.count(execution.root))
    execution.error="default argument execution lacks exact copied root lifetime";
  for(const auto &entry:execution.lifetimes) {
    auto copied=entry.first,original=entry.second;
    if(copied->kind!=original->kind || copied->kind!=olk_expr_temporary ||
       (copied!=execution.root && !execution.lifetimes.count(copied->parent_lifetime)))
      execution.error="copied default argument lifetime has an unowned parent or kind";
    unsigned ordinal=0;
    for(auto dip=copied->destructions;dip;dip=dip->next_in_destruction_list,++ordinal) {
      if(!execution.obligations.count(dip))execution.error="copied default argument destruction lacks exact original";
      else execution.copied_ordinals.emplace(dip,ordinal);
    }
    unsigned sibling=0;
    if(copied!=execution.root) {
      auto child=copied->parent_lifetime->child_lifetime;
      while(child && child!=copied) {child=child->next;++sibling;}
      if(!child)execution.error="copied default argument child lacks ordered parent membership";
    }
    execution.sibling_ordinals.emplace(copied,sibling);
    auto inserted=state().lifetime_copies.emplace(copied,original);
    if(!inserted.second && inserted.first->second!=original)
      execution.error="copied default argument lifetime has conflicting provenance";
    state().default_argument_lifetime_adoptions.emplace(original,adapter);
  }
  for(const auto &entry:execution.obligations)
    state().default_argument_obligation_adoptions.emplace(entry.second,adapter);
  for(const auto &entry:execution.temporaries)
    state().default_argument_temporary_adoptions.emplace(entry.second,adapter);
  execution.complete=true;
}
inline void observe_default_argument_forwarding(a_routine_ptr adapter,a_scope_ptr scope,an_expr_node_ptr call) {
  if(!enabled())return;
  auto found=state().default_argument_executions.find(adapter);
  if(found==state().default_argument_executions.end())return;
  if(found->second.scope && found->second.scope!=scope)found->second.error="default argument scope changed";
  found->second.forwarding=call;
}
inline a_dynamic_init_ptr original_record_temporary(a_dynamic_init_ptr value) {
  std::set<a_dynamic_init_ptr> seen;
  while(value && seen.insert(value).second) {
    if(state().record_temporaries.count(value))return value;
    auto copy=state().dynamic_init_copies.find(value);
    if(copy==state().dynamic_init_copies.end())break;
    value=copy->second;
  }
  return nullptr;
}
inline void observe_record_temporary_binding(a_dynamic_init_ptr dip,a_variable_ptr variable,a_routine_ptr owner) {
  if(!enabled() || !dip || !variable || !owner || !(variable->compiler_generated || variable->lowering_generated))return;
  auto original=original_record_temporary(dip);if(!original)return;
  RecordTemporaryBinding value;value.original=original;value.lowered=dip;value.owner=owner;
  auto &bindings=state().record_temporary_bindings[variable];
  for(const auto &existing:bindings)
    if(existing.original==original && existing.lowered==dip && existing.owner==owner)return;
  bindings.push_back(value);
}
inline a_dynamic_init_ptr original_static_destruction(a_dynamic_init_ptr value) {
  std::set<a_dynamic_init_ptr> seen;
  while(value && seen.insert(value).second) {
    if(state().static_destructions.count(value))return value;
    auto copied=state().dynamic_init_copies.find(value);
    if(copied==state().dynamic_init_copies.end())break;
    value=copied->second;
  }
  return nullptr;
}
inline a_dynamic_init_ptr source_destruction(a_dynamic_init_ptr value,a_routine_ptr owner);
struct ArrayDeletionCapture {
  bool active=false;
  ArrayDeletionCapture(a_dynamic_init_ptr destruction,a_routine_ptr deallocator,a_type_ptr element,a_routine_ptr owner) {
    if(!(active=enabled()))return;
    ArrayDeletionSnapshot value;value.owner=owner;value.deallocator=deallocator;value.element_type=element;
    value.original=destruction?source_destruction(destruction,owner):nullptr;
    if(!value.original)value.error="array delete lacks original partial element destruction";
    if(!deallocator)value.error="array delete implicit default deallocator requires owned protocol";
    state().array_deletion_contexts.push_back(value);
  }
  ~ArrayDeletionCapture(){if(active)state().array_deletion_contexts.pop_back();}
};
inline void observe_array_deletion_virtual_callback(a_routine_ptr destructor,an_expr_node_ptr callback,a_type_ptr cookie_type) {
  if(!enabled() || state().array_deletion_contexts.empty())return;
  auto &value=state().array_deletion_contexts.back();
  if(!destructor || !destructor->is_virtual || destructor->ctor_dtor_kind!=cdk_complete ||
     !callback || callback->kind!=enk_operation || callback->variant.operation.kind!=eok_question) {
    value.error="array delete virtual callback lacks exact complete-destructor selection";return;
  }
  value.virtual_callback=true;value.destructor=destructor;
  value.vtable_index=destructor->number.virtual_function;
  value.cookie_size=cookie_type?skip_typerefs(cookie_type)->size:0;
}
inline void observe_array_deletion_call(an_expr_node_ptr call) {
  if(!enabled() || state().array_deletion_contexts.empty())return;
  auto value=state().array_deletion_contexts.back();
  if(!call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call ||
     !call->variant.operation.operands || call->variant.operation.operands->kind!=enk_routine) {
    value.error="array delete requires owned direct runtime protocol";
  } else {
    auto target=call->variant.operation.operands;
    value.helper=spelling(target->variant.routine.ptr->source_corresp);
    if(value.helper!="__cxa_vec_delete2" && value.helper!="__cxa_vec_delete3")
      value.error="array delete helper requires additional owned protocol";
    unsigned ordinal=0;
    for(auto argument=target->next;argument;argument=argument->next,++ordinal) {
      if(ordinal==2) {
        if(argument->kind!=enk_constant)value.error="array delete padding lacks exact constant";
        else value.padding=node_constant(argument);
      }
      if(ordinal<3 || ordinal>4)continue;
      if(ordinal==3 && value.virtual_callback)continue;
      auto leaf=argument;
      while(leaf->kind==enk_operation && (leaf->variant.operation.kind==eok_cast ||
            leaf->variant.operation.kind==eok_address_of))leaf=leaf->variant.operation.operands;
      if(leaf->kind!=enk_routine) {
        value.error="array delete virtual callback requires owned dispatch protocol";continue;
      }
      auto callback=leaf->variant.routine.ptr;
      if(ordinal==3)value.destructor=callback;
      else if(callback!=value.deallocator)value.error="array delete callback differs from original selected deallocator";
      state().array_callback_addresses.emplace(leaf,std::make_pair(value.owner,callback));
    }
    if(value.error.empty() && (ordinal!=5 || !value.padding || !value.destructor))value.error="array delete lacks finalized callback arguments";
  }
  if(call)state().array_deletion_calls.emplace(call,value);
}
struct ArrayAllocationCapture {
  bool active=false;
  ArrayAllocationCapture(a_dynamic_init_ptr allocation,a_dynamic_init_ptr destruction,a_type_ptr element,a_routine_ptr owner) {
    if(!(active=enabled()))return;
    ArrayAllocationSnapshot value;value.owner=owner;value.element_type=element;
    value.original=allocation?source_destruction(allocation,owner):nullptr;
    value.original_destruction=destruction?source_destruction(destruction,owner):nullptr;
    if(destruction && !value.original_destruction)value.error="vector allocation element cleanup lacks original source owner";
    state().array_allocation_contexts.push_back(value);
  }
  ~ArrayAllocationCapture(){if(active)state().array_allocation_contexts.pop_back();}
};
struct EntityInitializerCapture {
  bool active=false;
  EntityInitializerCapture(a_dynamic_init_ptr allocation,a_type_ptr element,a_routine_ptr owner) {
    if(!(active=enabled()))return;
    state().entity_initializer_contexts.push_back({owner,allocation?source_destruction(allocation,owner):nullptr,skip_typerefs(element)});
  }
  ~EntityInitializerCapture(){if(active)state().entity_initializer_contexts.pop_back();}
};
inline void observe_generated_entity_initializer_call(a_routine_ptr helper,an_expr_node_ptr call,a_routine_ptr owner) {
  if(!enabled())return;
  auto found=state().generated_entity_initializers.find(helper);
  if(found==state().generated_entity_initializers.end())return;
  if(found->second.origin.owner!=owner || !call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call)
    found->second.error="generated entity initializer lost exact owning direct call";
  else state().generated_entity_initializer_calls.emplace(call,helper);
}
inline void observe_generated_array_initializer_begin(a_routine_ptr helper,a_type_ptr element,
    bool complete_object,bool count_parameter,bool zero_entity,a_routine_ptr constructor) {
  if(!enabled())return;
  if(complete_object && count_parameter && !zero_entity && !constructor && !state().entity_initializer_contexts.empty()) {
    GeneratedEntityInitializer value;value.origin=state().entity_initializer_contexts.back();
    if(!helper || !value.origin.owner || !value.origin.allocation || skip_typerefs(element)!=value.origin.element ||
       (value.origin.element->kind!=tk_struct && value.origin.element->kind!=tk_class))
      value.error="generated counted model initializer lacks exact array-new record origin";
    if(helper) {state().generated_entity_initializers.emplace(helper,value);observe_runtime_signature(helper->type);}
    return;
  }
  if(state().array_allocation_contexts.empty())return;
  const auto &context=state().array_allocation_contexts.back();
  if(!context.original || !complete_object || count_parameter || !zero_entity)return;
  GeneratedArrayInitializer value;value.owner=context.owner;value.allocation=context.original;
  value.element=skip_typerefs(element);value.constructor=constructor;
  if(!helper || !context.owner || !context.error.empty() ||
     value.element!=skip_typerefs(context.element_type) ||
     (value.element->kind!=tk_struct && value.element->kind!=tk_class))
    value.error="generated vector initializer lacks exact complete record/new origin";
  if(helper) {
    state().generated_array_initializers.emplace(helper,value);
    observe_runtime_signature(helper->type);
  }
}
inline void observe_generated_array_initializer_copy(a_routine_ptr helper,a_scope_ptr scope,
    a_variable_ptr parameter,a_variable_ptr model,an_expr_node_ptr copy) {
  if(!enabled())return;
  auto entity=state().generated_entity_initializers.find(helper);
  if(entity!=state().generated_entity_initializers.end()) {
    auto &value=entity->second;value.scope=scope;value.destination=parameter;value.count=parameter?parameter->next:nullptr;value.model=model;value.copy=copy;
    if(!scope || !parameter || !value.count || value.count->next!=model || !model || model->next ||
       !copy || copy->kind!=enk_operation || copy->variant.operation.kind!=eok_assign)
      value.error="generated counted initializer lost exact parameters/model copy";
    observe_record_storage_copy(copy,helper,"entity-model");return;
  }
  auto found=state().generated_array_initializers.find(helper);
  if(found==state().generated_array_initializers.end())return;
  auto &value=found->second;value.scope=scope;value.parameter=parameter;value.model=model;value.copy=copy;
  if(!scope || !parameter || parameter->next || !model || model==parameter ||
     !copy || copy->kind!=enk_operation || copy->variant.operation.kind!=eok_assign)
    value.error="generated array initializer lacks its exact model and destination";
  observe_record_storage_copy(copy,helper,"array-zero-model");
}
inline void observe_generated_array_initializer_constructor(a_routine_ptr helper,an_expr_node_ptr call) {
  if(!enabled())return;
  auto found=state().generated_array_initializers.find(helper);
  if(found!=state().generated_array_initializers.end())found->second.constructor_call=call;
}
inline void observe_generated_array_initializer_complete(a_routine_ptr helper,a_scope_ptr scope) {
  if(!enabled())return;
  auto entity=state().generated_entity_initializers.find(helper);
  if(entity!=state().generated_entity_initializers.end()) {
    auto &value=entity->second;
    if(value.scope!=scope || !value.copy || !value.destination || !value.count || !value.model)
      value.error="generated counted model initializer incomplete at factory completion";
    value.complete=true;return;
  }
  auto found=state().generated_array_initializers.find(helper);
  if(found==state().generated_array_initializers.end())return;
  auto &value=found->second;
  if(value.scope!=scope || !value.copy || !value.model || !value.parameter ||
     bool(value.constructor)!=bool(value.constructor_call))
    value.error="generated array initializer incomplete at stock factory boundary";
  value.complete=true;
}
inline void observe_array_allocation_call(an_expr_node_ptr call,an_expr_node_ptr destination,
    a_routine_ptr constructor,a_routine_ptr destructor,a_routine_ptr allocator,a_routine_ptr deallocator,
    an_expr_node_ptr padding) {
  if(!enabled() || state().array_allocation_contexts.empty())return;
  auto value=state().array_allocation_contexts.back();
  if(!value.original || destination)return;
  value.constructor=constructor;value.destructor=destructor;value.allocator=allocator;value.deallocator=deallocator;
  if(!call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call ||
     !call->variant.operation.operands || call->variant.operation.operands->kind!=enk_routine) {
    value.error="array allocation lacks exact finalized runtime call";
  } else {
    value.helper=spelling(call->variant.operation.operands->variant.routine.ptr->source_corresp);
    if(value.helper!="__cxa_vec_new2" && value.helper!="__cxa_vec_new3")
      value.error="array allocation helper lacks explicit selected callback protocol";
  }
  if(!allocator || !deallocator || !padding || padding->kind!=enk_constant)
    value.error="array allocation helper lacks finalized callbacks or constant padding";
  else value.padding=node_constant(padding);
  if(value.error.empty()) {
    auto argument=call->variant.operation.operands->next;
    a_routine_ptr callbacks[]={constructor,destructor,allocator,deallocator};
    for(unsigned ordinal=0;argument;argument=argument->next,++ordinal)if(ordinal>=3 && ordinal<7) {
      auto callback=callbacks[ordinal-3];if(!callback)continue;
      auto leaf=argument;
      while(leaf->kind==enk_operation && (leaf->variant.operation.kind==eok_cast ||
            leaf->variant.operation.kind==eok_address_of))leaf=leaf->variant.operation.operands;
      if(leaf->kind!=enk_routine || leaf->variant.routine.ptr!=callback) {
        value.error="vector allocation callback lacks exact finalized routine designator";break;
      }
      state().array_callback_addresses.emplace(leaf,std::make_pair(value.owner,callback));
    }
  }
  if(call)state().array_allocation_calls.emplace(call,value);
}
// The stock variable-length dik_zero branch emits only nonthrowing runtime
// zeroing after allocation and deliberately creates no rollback region.
inline void observe_dynamic_array_zero_initialization(a_new_delete_supplement_ptr allocation,
    a_dynamic_init_ptr initialization,a_routine_ptr owner) {
  if(!enabled() || !allocation || !initialization || !owner)return;
  auto release=allocation->freeing_of_storage_on_exception;
  auto original=release?source_destruction(release,owner):nullptr;
  if(!original || allocation->dynamic_init!=initialization ||
     initialization->kind!=dik_zero || !allocation->type ||
     skip_typerefs(allocation->type)->kind!=tk_array || skip_typerefs(allocation->type)->size ||
     release->assoc_new!=allocation || !release->is_array_freeing ||
     !state().destructions.at(original).allocation_release ||
     !state().destructions.at(original).array_allocation)return;
  state().zero_array_allocation_initializations.emplace(original,owner);
}
inline void observe_empty_allocation_initialization(a_dynamic_init_ptr dip,a_routine_ptr owner) {
  if(!enabled() || !dip || !owner)return;
  auto original=source_destruction(dip,owner);
  if(original && state().destructions.at(original).allocation_release)
    state().empty_allocation_initializations.emplace(original,owner);
}
inline void observe_automatic_no_effect(a_dynamic_init_ptr dip) {
  if(!enabled())return;
  auto owner=innermost_function_scope?innermost_function_scope->variant.routine.ptr:nullptr;
  auto original=owner?source_destruction(dip,owner):nullptr;
  if(original) {
    state().automatic_no_effect_observed.insert(original);
    if(!state().destructions.at(original).allocation_release)
      state().automatic_destructor_no_effect.emplace(original,owner);
  }
  auto field=owner?original_field_initializer(dip,owner):nullptr;
  if(field)state().field_destructor_no_effect.emplace(field,owner);
}
inline void observe_static_partial_terminal(a_dynamic_init_ptr dip) {
  if(!enabled())return;
  auto original=original_static_destruction(dip);
  if(original && state().static_destructions.at(original).partial_aggregate)
    state().static_partial_terminal.insert(original);
  auto owner=innermost_function_scope?innermost_function_scope->variant.routine.ptr:nullptr;
  if(original && owner) {
    auto initializer=state().static_destructions.at(original).initializer;
    if(initializer && state().source_initializers.count(initializer) && state().source_initializers.at(initializer).field &&
       state().field_initializer_executions.count(std::make_pair(initializer,owner)))
      state().field_partial_terminal.emplace(original,owner);
  }
  auto automatic=owner?source_destruction(dip,owner):nullptr;
  if(automatic && state().destructions.at(automatic).partial_aggregate) {
    state().automatic_partial_terminal.emplace(automatic,owner);
    state().automatic_partial_terminal_copies[std::make_pair(automatic,owner)].insert(dip);
  }
}
inline void observe_static_no_effect(a_dynamic_init_ptr dip) {
  if(!enabled())return;
  auto original=original_static_destruction(dip);
  if(original)state().static_no_effect.insert(original);
}
inline void observe_static_registration(a_dynamic_init_ptr dip,an_init_pos_descr_ptr position,
                                       an_expr_node_ptr call,a_routine_ptr callback,a_routine_ptr owner) {
  if(!enabled())return;
  StaticRegistrationSnapshot value;value.original=original_static_destruction(dip);
  value.owner=owner;value.callback=callback;value.variable=position->variable;
  value.thread=position->variable && var_has_thread_storage_duration(position->variable);
  if(!value.original || !state().static_destructions.at(value.original).persistent)
    value.error="static registration lacks original persistent source obligation";
  if(position->modifiers || position->indirect_through_variable || position->thrown_object_address ||
     !call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call)
    value.error="static registration lacks exact whole-storage call observation";
  if(call && call->kind==enk_operation && call->variant.operation.kind==eok_call) {
    auto operand=call->variant.operation.operands;
    value.callback_expression=operand?operand->next:nullptr;
    value.object=value.callback_expression?value.callback_expression->next:nullptr;
    value.dso=value.object?value.object->next:nullptr;
    if(!value.callback_expression || !value.object || !value.dso || value.dso->next)
      value.error="static registration runtime call changed arity";
  }
  state().static_registrations.emplace(call,value);
}
inline void observe_static_destructor_begin(a_routine_ptr routine,a_scope_ptr scope,
                                          a_dynamic_init_ptr dip,an_init_pos_descr_ptr position) {
  if(!enabled())return;
  GeneratedStaticDestructor value;value.original=original_static_destruction(dip);
  value.scope=scope;value.variable=position->variable;value.parameter=scope->variant.routine.parameters;
  auto type=skip_typerefs(position->base_type);
  if(!value.original || !value.variable || position->modifiers || position->indirect_through_variable)
    value.error="generated static destructor lacks exact original storage";
  if(type->kind==tk_array) {
    // Array element aliases carry source identity, not storage extent.
    // Keep that identity in the source obligation; observe physical layout
    // from the actual element just as stock destructor generation does.
    value.array=true;value.element_type=skip_typerefs(underlying_array_element_type(type));
    value.element_count=num_array_elements(type);value.element_size=value.element_type->size;
  }
  state().generated_static_destructors.emplace(routine,value);
}
inline void observe_static_destructor_complete(a_routine_ptr routine,a_scope_ptr scope) {
  if(!enabled())return;
  auto found=state().generated_static_destructors.find(routine);
  if(found!=state().generated_static_destructors.end()) {
    found->second.complete=true;found->second.parameter=scope->variant.routine.parameters;
  }
}
inline void observe_initializer_lifetime_copy(an_object_lifetime_ptr original,an_object_lifetime_ptr copy,
                                             a_routine_ptr owner) {
  if(!enabled())return;
  observe_lifetime_copy(original,copy);
  auto initializer=state().active_file_initializer;
  if(initializer && state().lifetimes.count(original) && state().lifetimes.at(original).initializer==initializer)
    state().initializer_lifetime_adoptions.emplace(std::make_pair(original,owner),initializer);
}
inline a_dynamic_init_ptr source_destruction(a_dynamic_init_ptr value,a_routine_ptr owner) {
  auto execution_owner=owner;
  auto primary=abi_primary(owner);if(primary)owner=primary;
  std::set<a_dynamic_init_ptr> seen;
  while(value && seen.insert(value).second) {
    auto direct=state().destructions.find(value);
    if(direct!=state().destructions.end() && (direct->second.owner==owner ||
       state().default_argument_obligation_adoptions.count(std::make_pair(value,execution_owner)) ||
       (direct->second.initializer && state().initializer_lifetime_adoptions.count(std::make_pair(direct->second.lifetime,owner)))))return value;
    auto copied=state().dynamic_init_copies.find(value);
    if(copied==state().dynamic_init_copies.end())break;
    value=copied->second;
  }
  return nullptr;
}
inline void observe_allocation_internal_try(an_expr_node_ptr node,a_dynamic_init_ptr allocation,a_routine_ptr owner) {
  if(!enabled() || !node || !allocation)return;
  // make_placement_array_new_call returns (internal-try, result-temporary).
  // Its direct scalar counterpart returns exactly the internal-try node.
  if(node->kind==enk_operation && node->variant.operation.kind==eok_comma)
    node=node->variant.operation.operands;
  if(node->kind==enk_lowered_eh_construct && node->variant.lowered_eh.kind==leck_internal_try)
    state().allocation_internal_tries.emplace(node,std::make_pair(owner,allocation));
}
inline void observe_local_static_guard_probe(a_variable_ptr variable,a_variable_ptr guard,
                                             an_expr_node_ptr probe,a_routine_ptr owner) {
  if(!enabled())return;
  auto found=state().local_static_initializers.find(variable);
  if(found==state().local_static_initializers.end() || found->second.owner!=owner)return;
  found->second.guard=guard;found->second.probe=true;
  state().local_static_guard_variables.emplace(guard,variable);
  state().local_static_guard_operations.emplace(probe,std::make_pair(guard,"probe"));
}
inline void observe_local_static_guard_calls(a_variable_ptr guard,an_expr_node_ptr acquire,an_expr_node_ptr release) {
  if(!enabled())return;
  auto found=state().local_static_guard_variables.find(guard);
  if(found==state().local_static_guard_variables.end())return;
  auto &source=state().local_static_initializers.at(found->second);
  source.acquire=true;source.release=true;
  state().local_static_guard_operations.emplace(acquire,std::make_pair(guard,"acquire"));
  state().local_static_guard_operations.emplace(release,std::make_pair(guard,"release"));
}
inline void observe_local_thread_guard_publication(a_variable_ptr guard,a_statement_ptr statement) {
  if(!enabled() || !guard || !statement || statement->kind!=stmk_expr ||
     !statement->expr || statement->expr->kind!=enk_operation ||
     statement->expr->variant.operation.kind!=eok_assign || !var_has_thread_storage_duration(guard))return;
  auto found=state().local_static_guard_variables.find(guard);
  if(found==state().local_static_guard_variables.end())return;
  auto &source=state().local_static_initializers.at(found->second);
  if(!var_has_thread_storage_duration(source.variable) || source.acquire || source.release)return;
  source.thread_publish=true;
  state().local_static_guard_operations.emplace(statement->expr,std::make_pair(guard,"thread-publish"));
}
inline void observe_local_static_guard_cleanup(a_dynamic_init_ptr dip,a_variable_ptr guard,a_routine_ptr owner) {
  if(enabled() && state().local_static_guard_variables.count(guard))
    state().local_static_guard_cleanups.emplace(dip,std::make_pair(owner,guard));
}
#if DO_IL_LOWERING && GENERATE_EH_TABLES && !DO_FULL_PORTABLE_EH_LOWERING
inline void observe_try(a_try_supplement_ptr value,a_routine_ptr owner,bool function_try) {
  if(!enabled())return;
  TrySnapshot saved;saved.owner=owner;saved.original=value;saved.function_try=function_try;
  for(auto handler=value->handlers;handler;handler=handler->next) {
    saved.handlers.push_back(handler);HandlerSnapshot caught;caught.parameter=handler->parameter;
    if(caught.parameter) {
      caught.source_type=capture_type(caught.parameter->type);
      auto match=skip_typerefs(caught.parameter->type);
      if(match->kind==tk_pointer && match->variant.pointer.is_reference)match=match->variant.pointer.type;
      caught.match_type=capture_type(match);
    }
    state().handlers.emplace(handler,caught);
  }
  state().tries.emplace(value,saved);
}
struct HandlerCapture {
  bool active=false;
  HandlerCapture(a_handler_ptr handler) {if((active=enabled()))state().active_handlers.push_back(handler);}
  ~HandlerCapture(){if(active)state().active_handlers.pop_back();}
};
// Observe the typed construction destination before aggregate lowering drops
// the final member's unnecessary rollback entry. Complete payload destruction
// still owns that source obligation after successful throw initialization.
inline void observe_payload_subobject(a_dynamic_init_ptr dip,an_init_pos_descr_ptr position,a_routine_ptr owner) {
  if(!enabled() || !position->thrown_object_address || !dip->destructor || !dip->lifetime)return;
  CleanupSnapshot value;value.owner=owner;value.original=dip;value.base_type=position->base_type;
  value.base_subobject=position->base_class_subobject;
  if(state().active_throws.empty() || position->variable || position->indirect_through_variable ||
     position->array_element_sequence || position->num_elem_node || !position->modifiers ||
     !dip->destruction_is_for_partially_constructed_aggregate)
    value.error="payload construction outside scalar partial-aggregate ownership";
  else {
    value.throw_origin=state().active_throws.back();
    const auto &thrown=state().throws.at(value.throw_origin);
    if(thrown.owner!=owner || !thrown.destructor || skip_typerefs(thrown.payload_type)!=skip_typerefs(position->base_type))
      value.error="payload subobject lacks its complete throw destructor";
  }
  for(auto modifier=position->modifiers;modifier;modifier=modifier->next) {
    CleanupPathPart part;part.type=modifier->type;part.field=modifier->curr_field;
    part.base=modifier->curr_base;part.index=modifier->curr_elem;
#if GNU_VECTOR_TYPES_ALLOWED
    if(modifier->is_vector_element)value.error="vector payload construction position";
#endif
    value.path.insert(value.path.begin(),part);
  }
  state().payload_subobjects.push_back(value);
}
inline void observe_cleanup_region(a_dynamic_init_ptr dip,a_routine_ptr owner,a_variable_ptr vtt_argument=nullptr) {
  if(!enabled())return;
  auto descr=dip->destructible_entity_descr;
  if(!descr || !descr->region_table_entry)return;
  CleanupSnapshot value;value.owner=owner;value.original=dip;
  if(dip->is_freeing_of_exception_object) {
    if(state().active_handlers.empty())value.error="exception release lacks its observed handler";
    else {
      value.release_handler=state().active_handlers.back();
      state().handler_releases[dip]={owner,value.release_handler};
    }
    if(!state().cleanup_regions.emplace(descr->region_table_entry,value).second)
      state().cleanup_regions.at(descr->region_table_entry).error="duplicate handler release observation";
    return;
  }
  const auto &position=descr->init_pos_descr;
  value.variable=position.variable;value.base_type=position.base_type;
  value.allocation_release=dip->is_freeing_of_storage_on_exception;
  value.guard_abort=dip->is_guard_var_for_local_static_var_init;
  value.conditional_flag=descr->conditional_flag_var;
  value.indirect=position.indirect_through_variable;value.base_subobject=position.base_class_subobject;
  value.delegation=descr->use_delegation_dtor;value.vtt_argument=vtt_argument;
  auto entity_type=skip_typerefs(position.modifiers?position.modifiers->type:position.base_type);
  value.array=position.array_element_sequence || entity_type->kind==tk_array;
  if(value.array) {
    value.element_type=entity_type->kind==tk_array?underlying_array_element_type(entity_type):entity_type;
    if(position.array_element_sequence) {
      if(position.array_element_count<0)value.error="array cleanup requires owned cookie count";
      else value.element_count=position.array_element_count;
    } else value.element_count=num_array_elements(entity_type);
  }
  if(bool(vtt_argument)!=bool(descr->needs_subobject_construction_vtbl || descr->use_delegation_dtor))
    value.error="cleanup VTT lacks exact argument binding observation";
  if(position.thrown_object_address) {
    if(state().active_throws.empty() || position.variable || position.indirect_through_variable || !position.modifiers)
      value.error="payload subobject cleanup lacks exact active throw position";
    else {
      value.throw_origin=state().active_throws.back();
      const auto &thrown=state().throws.at(value.throw_origin);
      if(thrown.owner!=owner || skip_typerefs(thrown.payload_type)!=skip_typerefs(position.base_type))
        value.error="payload cleanup root differs from observed throw type";
    }
  }
  if(position.num_elem_node || (position.variable && position.variable->is_vla) ||
     (!position.variable && !position.thrown_object_address) ||
     dip->is_freeing_of_exception_object || dip->is_vla_deallocation)
    value.error="cleanup region outside fixed object/array/member/base contract";
  for(auto modifier=position.modifiers;modifier;modifier=modifier->next) {
    CleanupPathPart part;part.type=modifier->type;part.field=modifier->curr_field;
    part.base=modifier->curr_base;part.index=modifier->curr_elem;
#if GNU_VECTOR_TYPES_ALLOWED
    if(modifier->is_vector_element)value.error="vector cleanup position";
#endif
    value.path.insert(value.path.begin(),part);
  }
  if(!state().cleanup_regions.emplace(descr->region_table_entry,value).second)
    state().cleanup_regions.at(descr->region_table_entry).error="duplicate cleanup region observation";
}
inline void observe_cleanup_region_copy(a_constant_ptr original,a_dynamic_init_ptr dip,a_routine_ptr owner) {
  if(!enabled())return;
  auto found=state().cleanup_regions.find(original);
  if(found==state().cleanup_regions.end())return;
  auto value=found->second;
  if(value.owner!=owner)value.error="cloned cleanup region changed function owner";
  if(!state().cleanup_regions.emplace(dip->destructible_entity_descr->region_table_entry,value).second)
    state().cleanup_regions.at(dip->destructible_entity_descr->region_table_entry).error="duplicate cleanup clone observation";
}

#endif
inline void capture_scope(a_scope_ptr scope,a_routine_ptr function_owner=nullptr) {
  if(!scope)return;
  if(scope->kind==sck_function && is_observed_tls_routine(scope->variant.routine.ptr))return;
  if(scope->kind==sck_function && scope->variant.routine.ptr->special_kind==sfk_lambda_entry_point) {
    capture_routine(scope->variant.routine.ptr);return;
  }
  if(scope->kind==sck_function && state().generated_static_destructors.count(scope->variant.routine.ptr))return;
  if(scope->kind==sck_function && (state().generated_array_initializers.count(scope->variant.routine.ptr) || state().generated_entity_initializers.count(scope->variant.routine.ptr)))return;
  // Alternate ABI bodies contain generated forwarding declarations only.
  if(scope->kind==sck_function && abi_primary(scope->variant.routine.ptr))return;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
  if(scope->kind==sck_function && scope->variant.routine.ptr->overriding_function_for_wrapper)return;
#endif
  if(scope->kind==sck_function) {
    function_owner=scope->variant.routine.ptr;
    snapshot_source_local_scopes(scope);
    capture_routine(scope->variant.routine.ptr);
    capture_static_lifetime(scope->variant.routine.lifetime_of_local_static_vars,scope->variant.routine.ptr);
    if(auto lifetime=scope->variant.routine.lifetime_of_local_static_vars)
      for(auto dip=lifetime->destructions;dip;dip=dip->next_in_destruction_list)
        capture_source_initializer(dip,scope->variant.routine.ptr,0);
    auto &function=state().functions.at(scope->variant.routine.ptr);
    if(!function.scope_captured) {
      function.scope_captured=true;function.implicit_this=scope->variant.routine.this_param_variable;
      capture_source_lifetime(scope->lifetime,scope->variant.routine.ptr);
      SourceInitializerTraversal traversal;clear_expr_or_stmt_traversal_block(&traversal);
      traversal.owner=function_owner;traversal.process_expr=capture_record_temporary;
      traversal.process_non_dynamic_constants=TRUE;
      for(auto init=scope->variant.routine.constructor_inits;init;init=init->next)
        if(init->initializer)traverse_dynamic_init(init->initializer,&traversal);
      if(scope->assoc_block)traverse_statement(scope->assoc_block,&traversal);
      for(auto v=scope->variant.routine.parameters;v;v=v->next) {
        if(v!=function.implicit_this)function.parameter_variables.push_back(v);
      }
    }
    for(auto v=scope->variant.routine.parameters;v;v=v->next)capture_variable(v);
  }
  for(auto t=scope->types;t;t=t->next)capture_type(t);
  for(auto v=scope->nonstatic_variables;v;v=v->next)capture_variable(v);
  for(auto v=scope->variables;v;v=v->next)capture_variable(v);
  for(auto init=scope->local_static_variable_inits;init;init=init->next)
    if(init->init_kind==initk_dynamic) {
      if(function_owner)capture_source_initializer(init->initializer.dynamic,function_owner,0);
    }
  for(auto r=scope->routines;r;r=r->next)capture_routine(r);
  for(auto child=scope->scopes;child;child=child->next)capture_scope(child,function_owner);
  for(auto ns=scope->namespaces;ns;ns=ns->next)
    if(!ns->is_namespace_alias)capture_scope(ns->variant.assoc_scope);
}
inline void observe_scope(a_scope_ptr scope){if(enabled())capture_scope(scope);}
inline bool initializer_has_destructions(an_object_lifetime_ptr lifetime) {
  if(!lifetime)return false;
  if(lifetime->destructions)return true;
  for(auto child=lifetime->child_lifetime;child;child=child->next)
    if(initializer_has_destructions(child))return true;
  return false;
}
inline void observe_file_initializers() {
  if(!enabled())return;
  auto &s=state();s.file_initializers_observed=true;
  s.file_initializer_destructions=initializer_has_destructions(il_header.primary_scope->lifetime);
  capture_static_lifetime(il_header.primary_scope->lifetime,nullptr);
  unsigned ordinal=0;
  for(auto dip=il_header.primary_scope->dynamic_inits;dip;dip=dip->next,++ordinal) {
    capture_source_initializer(dip,nullptr,ordinal);
    DynamicInitializer value;value.original=dip;value.variable=dip->variable;
    value.source_ordinal=ordinal;
    if(value.variable) {
      value.position=value.variable->source_corresp.decl_position;
      capture_variable(value.variable);
    } else value.error="file initializer lacks its source variable";
    if(!s.dynamic_initializers.emplace(dip,value).second)
      s.dynamic_initializers.at(dip).error="duplicate original file initializer";
  }
}
inline void observe_generated_initializer(a_scope_ptr scope,unsigned long needed_bit,
                                          unsigned long priority,a_boolean is_thread_local) {
  if(!enabled())return;
  GeneratedInitializer value;value.needed_bit=needed_bit;value.priority=priority;
  value.is_thread_local=is_thread_local;
  if(!state().generated_initializers.emplace(scope->variant.routine.ptr,value).second)
    state().generated_initializers.at(scope->variant.routine.ptr).error="duplicate generated initializer creation";
}
inline void observe_initializer_begin(a_scope_ptr scope,a_dynamic_init_ptr dip,
                                     a_boolean block_start,a_boolean after_statement,
                                     a_statement_ptr anchor) {
  if(!enabled())return;
  auto &s=state();auto found=s.dynamic_initializers.find(dip);
  if(found==s.dynamic_initializers.end()) {
    DynamicInitializer value;value.error="file initializer absent from original list";
    found=s.dynamic_initializers.emplace(dip,value).first;
  }
  auto &value=found->second;auto routine=scope->variant.routine.ptr;
  auto generated=s.generated_initializers.find(routine);
  if(generated==s.generated_initializers.end()) {value.error="initializer routine lacks creation observation";return;}
  if(value.begun || value.variable!=dip->variable) {value.error="duplicate/changed file initializer";return;}
  value.begun=true;value.routine=routine;
  s.active_file_initializer=dip;
  s.adopted_initializers[routine].push_back(dip);
  for(const auto &lifetime:s.lifetimes)if(lifetime.second.initializer==dip)
    s.initializer_lifetime_adoptions.emplace(std::make_pair(lifetime.first,routine),dip);
  value.ordinal=unsigned(generated->second.initializers.size());
  generated->second.initializers.push_back(dip);
  value.guarded=value.variable->is_inline || value.variable->is_template_variable;
  if(block_start) {
    if(anchor!=scope->assoc_block)value.error="initializer insertion is not in its routine root";
  } else if(after_statement)value.previous=anchor;
  else value.error="initializer insertion is not a statement sequence";
}
inline void observe_initializer_end(a_scope_ptr scope,a_dynamic_init_ptr dip,
                                   a_boolean block_start,a_boolean after_statement,
                                   a_statement_ptr anchor) {
  if(!enabled())return;
  auto found=state().dynamic_initializers.find(dip);
  if(found==state().dynamic_initializers.end())return;
  auto &value=found->second;
  if(state().active_file_initializer==dip)state().active_file_initializer=nullptr;
  if(!value.begun || value.complete || value.routine!=scope->variant.routine.ptr) {
    value.error="file initializer completion lacks matching start";return;
  }
  value.complete=true;
  if(!value.error.empty())return;
  if(block_start) {
    if(value.previous || anchor!=scope->assoc_block)value.error="initializer insertion moved backwards";
    return;
  }
  if(!after_statement) {value.error="initializer completion is not a statement sequence";return;}
  if(anchor==value.previous)return; // An observed no-op constructor is valid.
  auto stmt=value.previous?value.previous->next:scope->assoc_block->variant.block.statements;
  for(;stmt;stmt=stmt->next) {
    value.statements.push_back(stmt);
    if(stmt==anchor)return;
  }
  value.error="initializer statement interval lost its completion anchor";
}
inline void observe_generated_initializer_complete(a_scope_ptr scope) {
  if(!enabled())return;
  auto found=state().generated_initializers.find(scope->variant.routine.ptr);
  if(found!=state().generated_initializers.end())found->second.complete=true;
}
inline void observe_frontend_identity_boundary() {
  if(!enabled() || C_mode())return;
  // fe_wrapup invokes this after normal file-scope name finalization. Copy
  // the exact name already emitted by EDG's mangling pass, like c_gen_be's
  // dump_type_name/dump_name. Never re-enter the mangler here: template
  // arguments may already be lowered and namespace ancestry may be erased.
  // Pointer-derived names in the optional fast-mangling configuration are
  // deliberately not acceptable persistent identities.
  auto &s=state();
  for(const auto &entry:s.enums) {
    const auto &source=entry.first->source_corresp;
#if !ONLY_MANGLE_TYPES_NEEDED_FOR_EXTERNAL_NAMES
    if(source.name && source.name_has_been_mangled && !source.final_name_mangling_pending)
      s.finalized_enum_identities[entry.first]=source.name;
#endif
  }
  for(const auto &entry:s.records) {
    auto type=entry.first;
    if(type->variant.class_struct_union.is_nonreal_class ||
       type->variant.class_struct_union.is_prototype_instantiation)continue;
#if !ONLY_MANGLE_TYPES_NEEDED_FOR_EXTERNAL_NAMES
    const auto &source=type->source_corresp;
    if(source.name && source.name_has_been_mangled && !source.final_name_mangling_pending &&
       !(source.unnamed_entity_given_fabricated_name && s.record_owners.count(type))) {
      s.finalized_record_identities[type]=source.name;
    } else if(entry.second.local_identity_observed) {
      const auto &record=entry.second;
      const auto &owner=record.local_owner->source_corresp;
      const char *unit=std::getenv("EDG_DOT_OBJECT_UNIT_ID");
      if(owner.name && !owner.final_name_mangling_pending &&
         (owner.name_has_been_mangled || record.local_owner_c_linkage) &&
         (record.local_owner_external || (unit && *unit))) {
        // Length-delimit arbitrary unit IDs and original source spellings.
        auto part=[](const std::string &text){return std::to_string(text.size())+":"+text;};
        s.owned_local_record_identities[type]=(record.local_owner_external?"external:":"private:"+part(unit))+
          part(owner.name)+part(record.identity)+std::to_string(record.local_discriminator);
      }
    }
#endif
  }
}
inline void observe_record_before_prelowering(a_type_ptr type) {
  if(!enabled())return;
  // Forward declarations have an ordinary source snapshot, but cannot prove
  // the boundary at which a complete object's ABI fields are introduced.
  // Do not cache them here and block a later completed-class observation.
  if(type->incomplete)return;
  auto &s=state();
  if(s.abi_records.count(type) || s.prelower_records.count(type))return;
  capture_record(type);
  PrelowerRecord value;value.name=spelling(type->source_corresp);
  value.position=type->source_corresp.decl_position;value.complete=!type->incomplete;
  value.size=type->size;value.alignment=type->alignment;
  // Observe only an already-completed semantic fact. Incomplete and generated
  // types never gain permission to copy merely because their fields fit.
  if(value.complete && type->source_corresp.assoc_info) {
    value.trivial_copy_known=true;value.trivial_copy=is_trivially_copyable_type(type);
    if(!C_mode()) {
      // These are distinct completed frontend facts. Whole-type trivial
      // copyability additionally constrains assignment, unlike ABI transport.
      auto cssp=class_symbol_supp(symbol_for(type));
      value.bitwise_construction=cssp->construction_by_bitwise_copy_allowed;
      value.bitwise_assignment=cssp->assignment_by_bitwise_copy_allowed;
      // il.c::set_arg_transfer_method_flag differs from the return rule:
      // a deleted copy constructor alone does not make a trivial move
      // parameter indirect. Observe that distinct completed frontend fact.
      value.requires_parameter_storage=!cssp->construction_by_bitwise_copy_allowed ||
        (!any_cfront_mode() && has_deleted_or_nontrivial_destructor(cssp));
      a_boolean incomplete=FALSE;
      value.requires_return_storage=type_returned_by_cctor(type,&incomplete);
      value.operations_known=!incomplete;
      if(incomplete)value.error="complete class has unresolved return storage policy";
    }
  }
  for(auto field=type->variant.class_struct_union.field_list;field;field=field->next) {
    PrelowerField item;item.il=field;item.original_type=field->type;
    item.id=s.next_original_field++;s.original_field_ids[field]=item.id;
    item.name=spelling(field->source_corresp);item.position=field->source_corresp.decl_position;
    item.byte_offset=field->offset;item.is_bit_field=field->is_bit_field;
    item.has_initializer=field->has_initializer;item.is_mutable=field->is_mutable;item.no_unique_address=field->has_no_unique_address_attribute;
    if(field->is_bit_field){item.bit_offset=field->offset_bit_remainder;item.bit_width=field->bit_size;}
    auto captured=s.type_ids.find(field->type);
    if(captured!=s.type_ids.end())item.source_type_snapshot=captured->second;
    else item.source_type_snapshot=capture_type(field->type);
    if(field->is_lowered_base_class || s.generated_field_roles.count(field))
      value.error="original record observed after ABI field insertion";
    value.fields.push_back(item);
  }
  auto supplement=type->variant.class_struct_union.extra_info;
  if(supplement)for(auto base=supplement->direct_base_classes;base;base=base->next_direct) {
    PrelowerBase item;item.il=base;item.original_type=base->orig_type?base->orig_type:base->type;
    item.position=base->decl_position;item.is_virtual=base->is_virtual;
    auto captured=s.type_ids.find(item.original_type);
    if(captured!=s.type_ids.end())item.source_type_snapshot=captured->second;
    else item.source_type_snapshot=capture_type(item.original_type);
    for(auto derivation=base->derivation;derivation;derivation=derivation->next)if(derivation->direct) {
      item.access=derivation->access==as_public?"public":derivation->access==as_protected?"protected":
        derivation->access==as_private?"private":"unknown";break;
    }
    if(item.access.empty())item.access="unknown";
    value.bases.push_back(item);
  }
  if(supplement)for(auto base=supplement->base_classes;base;base=base->next) {
    if(base->is_virtual) {
      PrelowerBase physical;physical.il=base;physical.original_type=base->orig_type?base->orig_type:base->type;
      physical.is_virtual=true;physical.source_type_snapshot=capture_type(physical.original_type);
      value.virtual_bases.push_back(physical);
    }
    ExceptionBaseSnapshot item;
    item.source_type=capture_type(base->orig_type?base->orig_type:base->type);
    item.offset=base->offset;item.is_public=base->has_public_derivation;
    item.is_ambiguous=base->ambiguous;item.is_virtual=base->is_virtual;
    value.exception_bases.push_back(item);
  }
  s.prelower_records.emplace(type,value);
}
inline void observe_abi_record(a_type_ptr type) {
  if(enabled())state().abi_records.insert(type);
}
inline void observe_member_pointer_record(a_type_ptr type) {
  if(enabled())state().abi_record_keys[type]="abi:member-function-pointer";
}
inline void observe_erased_code_pointer(a_type_ptr type) {
  if(enabled())state().erased_code_pointer_types.insert(type);
}
inline void observe_typeinfo_base_record(a_type_ptr type) {
  if(enabled())state().abi_record_keys[type]="abi:rtti:base-class-spec";
}
inline void observe_typeinfo_record(a_type_ptr type,a_type_info_kind kind,unsigned long long base_count) {
  if(!enabled())return;
  // The kind comes from the actual RTTI construction operation. The ABI's
  // canonical kind name is not guessed from a generated record spelling.
  if((unsigned)kind>=(unsigned)tik_last || !type_info_names[(int)kind])return;
  state().abi_record_keys[type]=std::string("abi:rtti:")+type_info_names[(int)kind]+":bases:"+std::to_string(base_count);
}
inline void observe_base_field(a_field_ptr field,a_base_class_ptr base) {
  if(enabled())state().generated_base_fields[field]=base;
}
inline void observe_subobject_type(a_type_ptr complete,a_type_ptr view) {
  if(enabled() && complete!=view)state().observed_subobjects[view]=complete;
}
inline void observe_field_copy(a_field_ptr original,a_field_ptr copy) {
  if(!enabled())return;
  auto &s=state();s.copied_fields[copy]=original;
  auto source=s.original_field_ids.find(original);
  if(source!=s.original_field_ids.end())s.original_field_ids[copy]=source->second;
  auto base=s.generated_base_fields.find(original);
  if(base!=s.generated_base_fields.end())s.generated_base_fields[copy]=base->second;
  auto generated=s.generated_field_roles.find(original);
  if(generated!=s.generated_field_roles.end())s.generated_field_roles[copy]=generated->second;
}
inline void observe_vptr_field(a_field_ptr field) {
  if(enabled())state().generated_field_roles[field]="vptr";
}
#include "dot_gen_object_source_tls.h"
}
END_EDG_NAMESPACE
#endif
