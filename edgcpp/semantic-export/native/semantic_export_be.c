// Versioned unlowered EDG semantic graph. No EDG parser/lowering modifications.
#include <cstdio>
#include <cstdlib>
#include <string>
#include <vector>
#include <map>
#include <set>
#include "fe_common.h"
#include "il.h"
#include "il_walk.h"
#include "mem_manage.h"
#include "const_ints.h"
#include "float_pt.h"
USING_NAMESPACE_EDG
namespace {
#include "semantic_enum_names.h"
struct Node {char *ptr; an_il_entry_kind kind;};
std::vector<Node> nodes;
std::map<const void*,unsigned> ids;
std::set<const void*> visited;
std::map<const void*,sizeof_t> strings;
std::vector<std::pair<unsigned,std::string>> diagnostics;
FILE *out; unsigned current;
unsigned id(const void *p) {if(!p)return 0;auto q=ids.find(p);if(q!=ids.end())return q->second;unsigned n=(unsigned)ids.size()+1;ids[p]=n;return n;}
void bytes(const char *s,size_t n) {fputc('"',out);for(size_t i=0;i<n;++i){unsigned char c=(unsigned char)s[i];if(c=='"'||c=='\\'){fputc('\\',out);fputc(c,out);}else if(c<32||c>=127)fprintf(out,"\\u%04x",c);else fputc(c,out);}fputc('"',out);}
void quote(const char *s) {bytes(s?s:"",s?strlen(s):0);}
void key(const char *s){fputc(',',out);quote(s);fputc(':',out);}
void txt(const char*k,const char*v){key(k);quote(v);}
void num(const char*k,unsigned long long v){key(k);fprintf(out,"%llu",v);}
void signednum(const char*k,long long v){key(k);fprintf(out,"%lld",v);}
void boolean(const char*k,bool v){key(k);fputs(v?"true":"false",out);}
const char *storage_name(a_storage_class value){switch(value){case sc_unspecified:return "sc_unspecified";case sc_extern:return "sc_extern";case sc_static:return "sc_static";case sc_auto:return "sc_auto";case sc_typedef:return "sc_typedef";case sc_register:return "sc_register";case sc_asm:return "sc_asm";default:return "unknown";}}
void ref(const char*k,const void*p){key(k);fprintf(out,"%u",id(p));}
void loc_raw(a_seq_number seq,a_column_number column){const char *file=nullptr,*full=nullptr;a_line_number line=0;a_boolean end=FALSE;a_source_file_ptr sf=nullptr;if(seq)sf=conv_seq_to_file_and_line(seq,&file,&full,&line,&end);fprintf(out,"{\"sequence\":%lu,\"column\":%lu,\"line\":%lu,\"source\":%u,\"presumedFile\":",(unsigned long)seq,(unsigned long)column,(unsigned long)line,id(sf));quote(file);txt("physicalFile",full);fputc('}',out);}
void pos(const char*k,const a_source_position&p){key(k);fputs("{\"expansion\":",out);loc_raw(p.seq,p.column);key("spelling");loc_raw(p.orig_seq,p.orig_column);num("macroContext",(unsigned long long)p.macro_context);fputc('}',out);}
void simplepos(const char*k,const a_simple_source_position&p){key(k);loc_raw(p.seq,p.column);}
void range(const char*k,const a_source_range&r){key(k);fputc('{',out);fputs("\"available\":",out);fputs(r.start.seq?"true":"false",out);pos("start",r.start);pos("end",r.end);fputc('}',out);}
void source(const a_source_correspondence&s){txt("name",s.name);ref("attributes",s.attributes);pos("location",s.decl_position);ref("parentScope",s.parent_scope);ref("enclosingRoutine",s.enclosing_routine);ref("declarationPositions",s.decl_pos_info);ref("sourceSequence",s.source_sequence_entry);txt("linkage",name_linkage_kind_names[s.name_linkage]);boolean("classMember",s.is_class_member);boolean("parentViaLocalScopeRef",s.parent_via_local_scope_ref);num("access",s.access);}
void unsupported(const std::string &s){diagnostics.push_back({current,s});txt("coverage","unsupported");}
void visit(char*p,an_il_entry_kind k){id(p);if(visited.insert(p).second)nodes.push_back({p,k});}
void strvisit(char*p,an_il_entry_kind k,sizeof_t n){visit(p,k);strings[p]=n;}
void tagged(const char*k,const a_tagged_pointer&p){ref(k,p.ptr);}
char *discover(char*p,an_il_entry_kind k){if(p&&!visited.count(p)){if(is_string_entry_kind(k))strvisit(p,k,strlen(p)+1);else visit(p,k);}return p;}
bool edge_first;
char *edge(char*p,an_il_entry_kind k){if(p){if(!edge_first)fputc(',',out);edge_first=false;fprintf(out,"{\"target\":%u,\"kind\":",id(p));quote(il_entry_kind_names[k]);fputc('}',out);}return p;}
void emit(Node n){current=id(n.ptr);fprintf(out,"{\"id\":%u,\"kind\":",current);quote(il_entry_kind_names[n.kind]);
 auto st=strings.find(n.ptr);if(st!=strings.end()){key("bytesLatin1");bytes(n.ptr,(size_t)st->second);fputc('}',out);return;}
 switch(n.kind){
 case iek_type:{auto p=(a_type_ptr)n.ptr;source(p->source_corresp);txt("form",name_of(p->kind));ref("next",p->next);num("size",p->size);num("alignment",p->alignment);boolean("explicitAlignment",p->alignment_set_explicitly);boolean("incomplete",p->incomplete);boolean("dependent",p->is_instantiation_dependent);ref("canonicalType",skip_typerefs(p));switch(p->kind){
 case tk_integer:txt("integerKind",name_of(p->variant.integer.int_kind));boolean("signed",int_kind_is_signed[p->variant.integer.int_kind]);boolean("enum",p->variant.integer.enum_type);boolean("bool",p->variant.integer.bool_type);ref("integerInfo",p->variant.integer.extra_info);if(p->variant.integer.enum_type){boolean("scopedEnum",p->variant.integer.is_scoped_enum);if(p->variant.integer.is_scoped_enum)ref("enumScope",p->variant.integer.enum_info.assoc_scope);else ref("enumerators",p->variant.integer.enum_info.constant_list);}break;
 case tk_pointer:ref("elementType",p->variant.pointer.type);boolean("reference",p->variant.pointer.is_reference);boolean("rvalueReference",p->variant.pointer.is_rvalue_reference);break;
 case tk_typeref:ref("underlyingType",p->variant.typeref.type);ref("typerefInfo",p->variant.typeref.extra_info);num("qualifiers",p->variant.typeref.qualifiers);boolean("const",(p->variant.typeref.qualifiers&TQ_CONST)!=0);boolean("volatile",(p->variant.typeref.qualifiers&TQ_VOLATILE)!=0);boolean("restrict",(p->variant.typeref.qualifiers&TQ_RESTRICT)!=0);boolean("atomic",(p->variant.typeref.qualifiers&TQ_C11_ATOMIC)!=0);break;
 case tk_routine:ref("returnType",p->variant.routine.return_type);ref("signature",p->variant.routine.extra_info);break;
 case tk_array:ref("elementType",p->variant.array.element_type);boolean("variableLength",p->variant.array.is_variable_size_array);if(!p->variant.array.is_variable_size_array&&!p->variant.array.is_template_dependent_size_array)num("count",p->variant.array.variant.number_of_elements);ref("bound",p->variant.array.bound_constant);break;
 case tk_class:case tk_struct:case tk_union:ref("fields",p->variant.class_struct_union.field_list);ref("recordInfo",p->variant.class_struct_union.extra_info);boolean("templateInstance",p->variant.class_struct_union.is_template_class);boolean("prototypeInstance",p->variant.class_struct_union.is_prototype_instantiation);boolean("nonreal",p->variant.class_struct_union.is_nonreal_class);num("maxMemberAlignment",p->variant.class_struct_union.max_member_alignment);
#if GNU_EXTENSIONS_ALLOWED
 boolean("packed",p->variant.class_struct_union.is_packed);
#endif
 break;
 case tk_ptr_to_member:ref("elementType",p->variant.ptr_to_member.type);ref("memberClass",p->variant.ptr_to_member.class_of_which_a_member);break;
 case tk_float:num("floatKind",p->variant.float_kind);break;case tk_void:case tk_template_param:case tk_nullptr:break;
 default:unsupported(std::string("Type adapter: ")+name_of(p->kind));break;}break;}
 case iek_routine:{auto p=(a_routine_ptr)n.ptr;source(p->source_corresp);ref("next",p->next);ref("type",p->type);ref("declaredType",p->declared_type);ref("template",p->assoc_template);ref("templateArguments",p->template_arg_list);ref("bodyScope",p->memory_region?il_header.region_scope_entry[p->memory_region]:nullptr);txt("storageClassName",storage_name(p->storage_class));boolean("defined",p->defined);boolean("compilerGenerated",p->compiler_generated);boolean("templateInstance",p->is_template_function);boolean("prototypeInstance",p->is_prototype_instantiation);boolean("virtual",p->is_virtual);boolean("neverThrows",p->never_throws);txt("specialKind",name_of(p->special_kind));boolean("nodiscard",p->has_nodiscard_attribute);
#if GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED
 boolean("hasConstructorPriority",p->has_ctor_priority);boolean("hasDestructorPriority",p->has_dtor_priority);
#endif
#if GNU_EXTENSIONS_ALLOWED || REDEFINE_EXTNAME_PRAGMA_ENABLED
 ref("gnuRoutineInfo",p->gnu_extra_info);
#endif
#if GNU_EXTENSIONS_ALLOWED
 boolean("gnuAlias",p->is_gnu_alias);boolean("implicitAlias",p->implicit_alias);boolean("indirectFunction",p->is_ifunc);boolean("weak",p->is_weak);boolean("weakReference",p->is_weakref);boolean("initializationRoutine",p->is_initialization_routine);boolean("finalizationRoutine",p->is_finalization_routine);boolean("allocatesMemory",p->allocates_memory);
#endif
#if GNU_EXTENSIONS_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED
 boolean("pure",p->is_pure);
#endif
 break;}
 case iek_variable:{auto p=(a_variable_ptr)n.ptr;source(p->source_corresp);ref("next",p->next);ref("type",p->type);ref("declaredType",p->declared_type);boolean("parameter",p->is_parameter);boolean("threadLocal",p->is_thread_local);boolean("compilerGenerated",p->compiler_generated);boolean("extendsLifetime",p->extends_lifetime);num("alignment",p->alignment);
#if GNU_EXTENSIONS_ALLOWED
 ref("cleanupRoutine",p->cleanup_routine);
#endif
 num("storageClass",p->storage_class);txt("storageClassName",storage_name(p->storage_class));txt("initializationKind",name_of(p->init_kind));if(p->init_kind==initk_dynamic)ref("initializer",p->initializer.dynamic);else if(p->init_kind==initk_static)ref("initializer",p->initializer.constant);range("initializerRange",p->initializer_range);break;}
 case iek_field:{auto p=(a_field_ptr)n.ptr;source(p->source_corresp);ref("next",p->next);ref("type",p->type);num("offset",p->offset);num("alignment",p->alignment);
#if GNU_EXTENSIONS_ALLOWED
 boolean("packed",p->is_packed);
#endif
 num("bitOffset",p->offset_bit_remainder);num("bitSize",p->bit_size);boolean("bitfield",p->is_bit_field);ref("initializer",p->initializer);break;}
 case iek_scope:{auto p=(a_scope_ptr)n.ptr;txt("form",name_of(p->kind));ref("parent",p->parent);ref("next",p->next);ref("children",p->scopes);ref("body",p->assoc_block);ref("lifetime",p->lifetime);ref("variables",p->variables);ref("types",p->types);ref("routines",p->routines);ref("templates",p->templates);ref("sourceSequence",p->source_sequence_list);ref("pragmas",p->pragmas);ref("localStaticInitializers",p->local_static_variable_inits);ref("localExpressionReferences",p->expr_node_refs);ref("localScopeReferences",p->scope_refs);if(p->kind==sck_function){ref("routine",p->variant.routine.ptr);ref("parameters",p->variant.routine.parameters);ref("constructorInitializers",p->variant.routine.constructor_inits);}break;}
 case iek_expr_node:{auto p=(an_expr_node_ptr)n.ptr;txt("form",expr_node_kind_names[p->kind]);ref("type",p->type);ref("originalLvalueType",p->orig_lvalue_type);ref("next",p->next);pos("location",p->position);range("range",p->expr_range);txt("valueCategory",p->is_lvalue?"lvalue":p->is_xvalue?"xvalue":"prvalue");boolean("compilerGenerated",p->compiler_generated);boolean("parenthesized",p->is_parenthesized);boolean("volatileFetch",p->volatile_fetch);switch(p->kind){
 case enk_operation:txt("operator",name_of(p->variant.operation.kind));txt("operationTypeKind",name_of(p->variant.operation.type_kind));boolean("pointerOperandIsSecond",p->variant.operation.pointer_operand_is_second);
#if GNU_EXTENSIONS_ALLOWED
 boolean("gnuTwoOperandConditional",p->variant.operation.is_gnu_two_operand_question_mark);
#endif
 ref("operands",p->variant.operation.operands);boolean("virtualCall",p->variant.operation.is_virtual_call);boolean("evaluateLeftToRight",p->variant.operation.eval_left_to_right);boolean("evaluateRightToLeft",p->variant.operation.eval_right_to_left);boolean("implicitInMemberNaming",p->variant.operation.implicit_in_member_naming);break;
 case enk_constant:ref("constant",p->variant.constant.ptr);break;
 case enk_variable:ref("symbol",p->variant.variable.ptr);break;
 case enk_routine:ref("symbol",p->variant.routine.ptr);break;
 case enk_field:ref("symbol",p->variant.field.ptr);break;
 case enk_temp_init:ref("initialization",p->variant.init.dynamic_init);break;
 case enk_object_lifetime:ref("expression",p->variant.object_lifetime.expr);ref("lifetime",p->variant.object_lifetime.ptr);break;
 case enk_condition:ref("condition",p->variant.condition);break;
 case enk_throw:ref("throwInfo",p->variant.throw_info);break;
 case enk_sizeof:case enk_alignof:boolean("typeOperand",p->variant.sizeof_info.is_type);if(p->variant.sizeof_info.is_type)ref("operandType",p->variant.sizeof_info.variant.type);else ref("operand",p->variant.sizeof_info.variant.expr);break;
 case enk_type_operand:ref("operandType",p->variant.type_operand.type);break;
 case enk_builtin_operation:txt("builtinOperator",builtin_operation_names[p->variant.builtin_operation.kind]);ref("operands",p->variant.builtin_operation.operands);break;
 default:unsupported(std::string("Expression adapter: ")+expr_node_kind_names[p->kind]);break;}break;}
 case iek_statement:{auto p=(a_statement_ptr)n.ptr;ref("attributes",p->attributes);txt("form",name_of(p->kind));pos("location",p->position);pos("end",p->end_position);ref("next",p->next);ref("parent",p->parent);ref("expression",p->expr);ref("sourceSequence",p->source_sequence_entry);boolean("compilerGenerated",p->compiler_generated);switch(p->kind){
 case stmk_if:ref("then",p->variant.if_stmt.then_statement);ref("else",p->variant.if_stmt.else_statement);break;
 case stmk_while:case stmk_end_test_while:ref("body",p->variant.loop_statement);break;
 case stmk_for:ref("body",p->variant.for_loop.statement);ref("loopInfo",p->variant.for_loop.extra_info);break;
 case stmk_block:ref("statements",p->variant.block.statements);ref("blockInfo",p->variant.block.extra_info);break;
 case stmk_return:ref("returnInitialization",p->variant.return_dynamic_init);break;
 case stmk_goto:case stmk_label:ref("label",p->variant.label.ptr);ref("commonLifetime",p->variant.label.lifetime);if(p->kind==stmk_goto&&p->variant.label.ptr)txt("sourceForm",p->variant.label.ptr->break_label?"break":p->variant.label.ptr->continue_label?"continue":"goto");break;
 case stmk_init:ref("initialization",p->variant.dynamic_init);break;
 case stmk_try_block:ref("tryInfo",p->variant.try_block);break;
 case stmk_decl:ref("entities",p->variant.decl.entities);break;
 case stmk_switch:ref("body",p->variant.switch_stmt.body_statement);ref("switchInfo",p->variant.switch_stmt.extra_info);break;
 case stmk_switch_case:ref("switch",p->variant.switch_case.switch_statement);ref("caseInfo",p->variant.switch_case.extra_info);break;
 case stmk_expr:case stmk_empty:break;
 default:unsupported(std::string("Statement adapter: ")+name_of(p->kind));break;}break;}
 case iek_for_loop:{auto p=(a_for_loop_ptr)n.ptr;ref("initialization",p->initialization);ref("increment",p->increment);ref("scope",p->for_init_scope);break;}
 case iek_switch_stmt_descr:{auto p=(a_switch_stmt_descr_ptr)n.ptr;ref("cases",p->cases);ref("defaultCase",p->default_case);ref("sortedCases",p->sorted_cases);break;}
 case iek_switch_case_entry:{auto p=(a_switch_case_entry_ptr)n.ptr;ref("statement",p->stmt);ref("caseValue",p->case_value);ref("next",p->next);ref("nextSorted",p->next_on_sorted_list);boolean("reachableByFallThrough",p->reachable_by_fall_through);pos("location",p->position);
#if GNU_EXTENSIONS_ALLOWED
 ref("rangeEnd",p->range_end);
#endif
 break;}
 case iek_block:{auto p=(a_block_ptr)n.ptr;ref("scope",p->assoc_scope);ref("lifetime",p->lifetime);pos("end",p->final_position);break;}
 case iek_label:{auto p=(a_label_ptr)n.ptr;source(p->source_corresp);boolean("break",p->break_label);boolean("continue",p->continue_label);ref("statement",p->exec_stmt);break;}
 case iek_constant:{auto p=(a_constant_ptr)n.ptr;source(p->source_corresp);txt("form",name_of(p->kind));ref("type",p->type);ref("originalType",p->orig_type);ref("backingExpression",p->expr);ref("next",p->next);switch(p->kind){
 case ck_integer:{a_boolean is_signed=int_constant_is_signed(p);a_host_large_integer v=0;a_boolean err=FALSE;conv_integer_value_to_host_large_integer(&p->variant.integer_value,is_signed,&v,&err);if(err)unsupported("Integer constant exceeds 64-bit value adapter");else{key("integerValue");if(is_signed)fprintf(out,"\"%lld\"",(long long)v);else fprintf(out,"\"%llu\"",(unsigned long long)(a_host_large_unsigned)v);}break;}
 case ck_address:ref("subobjectPath",p->variant.address.subobject_path);num("addressKind",p->variant.address.kind);switch(p->variant.address.kind){case abk_routine:ref("routine",p->variant.address.variant.routine);break;case abk_variable:ref("variable",p->variant.address.variant.variable);break;case abk_constant:ref("constant",p->variant.address.variant.constant);break;default:unsupported("Address constant base adapter");break;}key("offset");fprintf(out,"%lld",(long long)p->variant.address.offset);break;
 case ck_string:key("bytesLatin1");bytes(p->variant.string.value,(size_t)p->variant.string.length);break;
 case ck_float:{auto type=skip_typerefs(p->type);if(type->kind!=tk_float){unsupported("Floating constant with nonfloating type");break;}a_boolean positive_infinity=FALSE,negative_infinity=FALSE,nan=FALSE;auto value=fp_to_string(type->variant.float_kind,&p->variant.float_value,&positive_infinity,&negative_infinity,&nan);txt("floatValue",value.as_temp_characters());boolean("positiveInfinity",positive_infinity);boolean("negativeInfinity",negative_infinity);boolean("notANumber",nan);
#if USE_HEX_FP_CONSTANTS_IN_GENERATED_CODE
 auto exact=fp_to_hex_constant_string(type->variant.float_kind,&p->variant.float_value,nullptr,nullptr,nullptr);txt("floatHexValue",exact.as_temp_characters());
#endif
 break;}
 case ck_aggregate:ref("elements",p->variant.aggregate.first_constant);boolean("explicitBraces",p->explicit_braces_on_aggregate);boolean("designatedInitializers",p->uses_designated_initializers);boolean("dynamicComponent",p->variant.aggregate.has_dynamic_init_component);break;
 case ck_designator:boolean("fieldDesignator",p->variant.designator.is_field_designator);boolean("generic",p->variant.designator.is_generic);if(p->variant.designator.is_generic)unsupported("Dependent initializer designator adapter");else if(p->variant.designator.is_field_designator)ref("field",p->variant.designator.variant.field);else num("elementIndex",p->variant.designator.variant.array_element);break;
 case ck_init_repeat:ref("constant",p->variant.init_repeat.constant);num("count",p->variant.init_repeat.count);boolean("multidimensionalTailNotRepeated",p->variant.init_repeat.multidimensional_aggr_tail_not_repeated);break;
 case ck_dynamic_init:ref("initialization",p->variant.dynamic_init.ptr);break;
 case ck_template_param:txt("resolution","dependent");break;
 default:unsupported(std::string("Constant adapter: ")+name_of(p->kind));break;}break;}
 case iek_dynamic_init:{auto p=(a_dynamic_init_ptr)n.ptr;txt("form",name_of(p->kind));ref("next",p->next);ref("variable",p->variable);ref("destructor",p->destructor);ref("lifetime",p->lifetime);ref("nextDestruction",p->next_in_destruction_list);ref("initializationExpressionLifetime",p->init_expr_lifetime);ref("master",p->master_entry);boolean("temporaryLifetime",p->has_temporary_lifetime);boolean("conditional",p->inside_conditional_expression);boolean("partialAggregate",p->destruction_is_for_partially_constructed_aggregate);boolean("constructorInitialization",p->is_constructor_init);boolean("unordered",p->unordered);boolean("freeStorageOnException",p->is_freeing_of_storage_on_exception);boolean("arrayFreeing",p->is_array_freeing);boolean("overlapsInnerTemporaries",p->overlaps_temps_in_inner_lifetime);ref("overlappingTemporariesLifetime",p->lifetime_of_overlapping_temps);switch(p->kind){case dik_nonconstant_aggregate:case dik_constant:ref("constant",p->variant.constant.ptr);break;case dik_class_result_via_ctor:case dik_expression:ref("expression",p->variant.expression);break;case dik_constructor:ref("constructor",p->variant.constructor.ptr);ref("arguments",p->variant.constructor.args);break;case dik_bitwise_copy:ref("source",p->variant.bitwise_copy.source);break;case dik_none:case dik_zero:break;default:unsupported(std::string("Initialization adapter: ")+name_of(p->kind));break;}break;}
 case iek_local_static_variable_init:{auto p=(a_local_static_variable_init_ptr)n.ptr;ref("next",p->next);ref("variable",p->variable);ref("lifetime",p->lifetime);txt("initializationKind",name_of(p->init_kind));if(p->init_kind==initk_static)ref("initializer",p->initializer.constant);else if(p->init_kind==initk_dynamic)ref("initializer",p->initializer.dynamic);else unsupported("Local static initialization adapter");break;}
 case iek_local_expr_node_ref:{auto p=(a_local_expr_node_ref_ptr)n.ptr;ref("next",p->next);ref("expression",p->expr);tagged("entity",p->referrer);num("referenceKind",p->kind);break;}
 case iek_local_scope_ref:{auto p=(a_local_scope_ref_ptr)n.ptr;ref("next",p->next);ref("scope",p->scope);tagged("entity",p->referrer);break;}
#if GENERATE_SOURCE_SEQUENCE_LISTS
 case iek_src_seq_sublist:{auto p=(a_src_seq_sublist_ptr)n.ptr;ref("next",p->next);ref("sourceSequence",p->source_sequence_list);ref("lastSourceSequence",p->last_source_sequence_entry);break;}
#endif
#if SCOPE_ORPHANED_LIST_PROCESSING_NEEDED
 case iek_scope_orphaned_list_header:{auto p=(a_scope_orphaned_list_header_ptr)n.ptr;ref("next",p->next);ref("routine",p->assoc_routine);num("scopeNumber",p->scope_number);ref("types",p->orphaned_types);ref("variables",p->orphaned_variables);ref("namespaces",p->orphaned_namespaces);
#if GENERATE_SOURCE_SEQUENCE_LISTS
 ref("sourceSequenceSublists",p->orphaned_src_seq_sublists);
#endif
 break;}
#endif
 case iek_object_lifetime:{auto p=(an_object_lifetime_ptr)n.ptr;txt("form",name_of(p->kind));ref("parentLifetime",p->parent_lifetime);ref("parentDestructionCursor",p->parent_destruction_sublist);ref("destructions",p->destructions);ref("childLifetime",p->child_lifetime);ref("next",p->next);tagged("entity",p->entity);break;}
 case iek_try_supplement:{auto p=(a_try_supplement_ptr)n.ptr;ref("body",p->statement);ref("handlers",p->handlers);ref("lifetime",p->lifetime);boolean("functionTryBlock",p->is_function_try_block);break;}
 case iek_handler:{auto p=(a_handler_ptr)n.ptr;ref("next",p->next);ref("parameter",p->parameter);ref("body",p->statement);ref("initialization",p->dynamic_init);pos("location",p->catch_position);break;}
 case iek_src_seq_end_of_construct:{auto p=(a_src_seq_end_of_construct_ptr)n.ptr;tagged("entity",p->entity);pos("location",p->position);break;}
 case iek_name_reference:{auto p=(a_name_reference_ptr)n.ptr;ref("next",p->next);ref("qualifier",p->qualifier);ref("writtenTemplateArguments",p->orig_template_arg_list);boolean("globalQualified",p->is_global_qualified_name);boolean("templateId",p->is_template_id);txt("specialKind",name_of(p->special_kind));break;}
 case iek_template_decl:{auto p=(a_template_decl_ptr)n.ptr;ref("parent",p->parent);ref("parameters",p->param_list);ref("scope",p->scope);pos("location",p->template_pos);break;}
 case iek_namespace:{auto p=(a_namespace_ptr)n.ptr;source(p->source_corresp);ref("next",p->next);boolean("alias",p->is_namespace_alias);boolean("inline",p->is_inline);if(p->is_namespace_alias)ref("namespace",p->variant.assoc_namespace);else ref("scope",p->variant.assoc_scope);break;}
 case iek_constructor_init:{auto p=(a_constructor_init_ptr)n.ptr;num("constructorInitKind",p->kind);ref("next",p->next);ref("initialization",p->initializer);ref("sourceExpression",p->source_expr);if(p->kind==cik_field)ref("field",p->variant.field);else if(p->kind==cik_virtual_base_class||p->kind==cik_direct_base_class)ref("base",p->variant.base_class);ref("originalType",p->orig_type);range("range",p->ctor_init_range);boolean("compilerGenerated",p->compiler_generated);break;}
 case iek_based_type_list_member:{auto p=(a_based_type_list_member_ptr)n.ptr;ref("next",p->next);ref("basedType",p->based_type);num("basedTypeKind",p->kind);boolean("frontendOnly",p->front_end_only);break;}
 case iek_integer_type_supplement:{auto p=(an_integer_type_supplement_ptr)n.ptr;ref("baseType",p->base_type);ref("template",p->assoc_template);num("bitWidth",p->bit_width);boolean("enumeratorListSeen",p->enumerator_list_seen);boolean("enumeratorListComplete",p->enumerator_list_complete);boolean("nodiscard",p->has_nodiscard_attribute);pos("baseTypeLocation",p->base_type_position);break;}
 case iek_typeref_type_supplement:{auto p=(a_typeref_type_supplement_ptr)n.ptr;ref("templateArguments",p->template_arg_list);ref("writtenTemplateArguments",p->orig_template_arg_list);ref("template",p->assoc_template);ref("expression",p->expr);ref("proxyClass",p->proxy_class);ref("operatorTypeArgument",p->operator_type_arg);signednum("minimumTemplateArguments",p->min_template_arguments);range("typeIdRange",p->type_id_range);
#if DEFAULT_RECORD_FORM_OF_NAME_REFERENCE
 ref("qualifier",p->name_qualifier);
#endif
 break;}
 case iek_pragma:{auto p=(a_pragma_ptr)n.ptr;ref("next",p->next);txt("pragmaKind",pragma_ids[p->kind]);boolean("ignoreInBackend",p->ignore_in_back_end);tagged("entity",p->entity);pos("location",p->position);ref("sourceSequence",p->source_sequence_entry);txt("text",p->pragma_text);if(p->kind==pk_stdc){num("stdcKind",p->variant.stdc.kind);num("stdcValue",p->variant.stdc.value);}else if(p->kind==pk_pack){num("alignment",p->variant.alignment);}else if(!p->ignore_in_back_end)unsupported(std::string("Pragma semantics adapter: ")+pragma_ids[p->kind]);break;}
 case iek_seq_number_lookup_entry:{auto p=(a_seq_number_lookup_entry_ptr)n.ptr;ref("next",p->next);ref("source",p->source_file);num("firstSequence",p->first);num("lastSequence",p->last);num("line",p->line_number);break;}
 case iek_subobject_path:{auto p=(a_subobject_path_ptr)n.ptr;ref("next",p->next);boolean("offsetPath",p->is_offset);boolean("baseClassPath",p->is_base_class);boolean("arrayToPointer",p->is_converted);if(p->is_offset)signednum("elementOffset",p->variant.ptr_offset);else if(p->is_base_class)ref("base",p->variant.base_class);else ref("field",p->variant.field);break;}
 case iek_element_position:{auto p=(an_element_position_ptr)n.ptr;ref("next",p->next);pos("location",p->position);num("elementKind",p->kind);break;}
 case iek_attribute:{auto p=(an_attribute_ptr)n.ptr;ref("next",p->next);txt("name",p->name);txt("attributeNamespace",p->namespace_name);num("attributeKind",p->kind);num("attributeFamily",p->family);num("syntacticLocation",p->syntactic_location);boolean("primaryDeclaration",p->on_primary_declaration);boolean("transformsType",p->transforms_type_specifier);boolean("packExpansion",p->is_pack_expansion);ref("arguments",p->arguments);ref("attributeGroup",p->group);pos("location",p->position);break;}
 case iek_attribute_group:{auto p=(an_attribute_group_ptr)n.ptr;pos("location",p->position);
#if EXTRA_SOURCE_POSITIONS_IN_IL
 pos("end",p->end_position);
#endif
 break;}
 case iek_attribute_arg:{auto p=(an_attribute_arg_ptr)n.ptr;ref("next",p->next);num("argumentKind",p->kind);boolean("packExpansion",p->is_pack_expansion);boolean("localExpressionReference",p->local_expr_ref);pos("location",p->position);switch(p->kind){case aak_empty:break;case aak_raw_token:case aak_token:txt("token",p->variant.token);num("tokenKind",p->token_kind);break;case aak_constant:ref("constant",p->variant.constant);break;case aak_type:ref("type",p->variant.type);break;case aak_expression:if(p->local_expr_ref){ref("scopedExpression",p->variant.sexpr);unsupported("Attribute local expression resolution adapter");}else ref("expression",p->variant.expr);break;default:unsupported("Attribute argument adapter");break;}break;}
#if GNU_EXTENSIONS_ALLOWED || REDEFINE_EXTNAME_PRAGMA_ENABLED
 case iek_gnu_routine_supplement:{auto p=(a_gnu_routine_supplement_ptr)n.ptr;txt("section",p->section);txt("assemblyName",p->asm_name);ref("aliasedRoutine",p->aliased_routine);ref("inlinePartner",p->inline_partner);
#if GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED
 num("constructorPriority",p->ctor_priority);num("destructorPriority",p->dtor_priority);
#endif
#if GNU_FUNCTION_MULTIVERSIONING
 boolean("multiversionRepresentative",p->is_representative);boolean("targetSpecificVersion",p->is_target_specific_version);if(p->is_representative||p->is_target_specific_version)unsupported("GNU multiversion routine adapter");
#endif
 break;}
#endif
 case iek_macro_invocation_record_block:txt("coverage","expanded-in-macroInvocations-table");break;
 case iek_source_sequence_entry:{auto p=(a_source_sequence_entry_ptr)n.ptr;ref("next",p->next);tagged("entity",p->entity);break;}
 case iek_il_entity_list_entry:{auto p=(an_il_entity_list_entry_ptr)n.ptr;ref("next",p->next);tagged("entity",p->entity);break;}
 case iek_src_seq_secondary_decl:{auto p=(a_src_seq_secondary_decl_ptr)n.ptr;ref("attributes",p->attributes);tagged("entity",p->entity);ref("declaredType",p->declared_type);ref("declarationPositions",p->decl_pos_info);pos("location",p->decl_position);break;}
 case iek_decl_position_supplement:{auto p=(a_decl_position_supplement_ptr)n.ptr;range("identifier",p->identifier_range);range("specifiers",p->specifiers_range);range("declaratorOrEnumOrNamespace",p->variant.declarator_range);break;}
 case iek_template:{auto p=(a_template_ptr)n.ptr;source(p->source_corresp);txt("form",name_of(p->kind));ref("canonicalTemplate",p->canonical_template);ref("definitionTemplate",p->definition_template);ref("prototypeTemplate",p->prototype_template);range("definitionRange",p->definition_range);txt("text",p->text);ref("declaration",p->template_decl);break;}
 case iek_template_arg:{auto p=(a_template_arg_ptr)n.ptr;txt("form",name_of(p->kind));ref("next",p->next);boolean("pack",p->is_pack);boolean("explicit",p->explicitly_specified);switch(p->kind){case tak_type:ref("type",p->variant.type);break;case tak_nontype:ref("constant",p->variant.constant);break;case tak_template:ref("template",p->variant.templ.ptr);break;default:unsupported(std::string("Template argument adapter: ")+name_of(p->kind));break;}break;}
 case iek_class_type_supplement:{auto p=(a_class_type_supplement_ptr)n.ptr;ref("scope",p->assoc_scope);ref("bases",p->direct_base_classes);ref("template",p->assoc_template);ref("templateArguments",p->template_arg_list);ref("partialSpecializationArguments",p->partial_spec_template_arg_list);ref("memberDeclarations",p->member_declarations);break;}
 case iek_param_type:{auto p=(a_param_type_ptr)n.ptr;ref("attributes",p->attributes);ref("next",p->next);ref("type",p->type);ref("declaredType",p->declared_type);txt("name",p->name);ref("defaultArgument",p->default_arg_expr);break;}
 case iek_macro:{auto p=(a_macro_ptr)n.ptr;source(p->source_corresp);ref("next",p->next);txt("text",p->text);boolean("undef",p->is_undef);boolean("commandLine",p->is_command_line_definition);boolean("predefined",p->is_predefined);boolean("objectLike",p->object_like);range("replacementRange",p->replacement_text_range);break;}
 case iek_source_file:{auto p=(a_source_file_ptr)n.ptr;txt("path",p->file_name);txt("fullPath",p->full_name);txt("nameAsWritten",p->name_as_written);ref("next",p->next);ref("firstChild",p->first_child_file);num("firstSequence",p->first_seq_number);num("lastSequence",p->last_seq_number);num("firstLine",p->first_line_number);boolean("include",p->is_include_file);break;}
 case iek_routine_type_supplement:{auto p=(a_routine_type_supplement_ptr)n.ptr;ref("parameters",p->param_type_list);ref("routine",p->assoc_routine);ref("thisClass",p->this_class);ref("exceptionSpecification",p->exception_specification);boolean("prototyped",p->prototyped);boolean("oldStyleParametersScanned",p->old_style_params_scanned);boolean("variadic",p->has_ellipsis);boolean("hasThis",p->has_this_param);num("qualifiers",p->qualifiers);num("thisQualifiers",p->this_qualifiers);num("callingConvention",p->calling_convention);boolean("doesNotReturn",p->does_not_return);
#if GNU_EXTENSIONS_ALLOWED
 boolean("constFunction",p->is_const);
#endif
 break;}
 case iek_condition_supplement:{auto p=(a_condition_supplement_ptr)n.ptr;ref("scope",p->scope);ref("initialization",p->dynamic_init);ref("expression",p->expr);ref("initializationStatement",p->initialization);break;}
 case iek_throw_supplement:{auto p=(a_throw_supplement_ptr)n.ptr;ref("type",p->type);ref("initialization",p->dynamic_init);ref("destructor",p->destructor);break;}
 default:unsupported(std::string("Metadata adapter: ")+il_entry_kind_names[n.kind]);break;
 }
 key("nativeEdges");fputc('[',out);edge_first=true;remap_pointers_in_il_entry(n.ptr,n.kind,edge,edge,FALSE);fputs("]}",out);
}
}
void back_end(void){
 const char*path=getenv("EDG_SEMANTIC_OUTPUT");if(!path){fprintf(stderr,"EDG_SEMANTIC_OUTPUT must specify graph JSON output\n");exit(2);}out=fopen(path,"wb");if(!out){perror(path);exit(2);}nodes.clear();ids.clear();visited.clear();strings.clear();diagnostics.clear();
 walk_file_scope_il(visit,strvisit,nullptr,nullptr,nullptr,FALSE);
 std::set<a_memory_region_number> regions;
 for(size_t i=0;i<nodes.size();++i)if(nodes[i].kind==iek_routine){auto r=(a_routine_ptr)nodes[i].ptr;if(r->memory_region&&il_header.region_scope_entry[r->memory_region]&&regions.insert(r->memory_region).second)walk_routine_scope_il(r->memory_region,visit,strvisit,nullptr,nullptr,nullptr,FALSE);}
 for(size_t i=0;i<nodes.size();++i)if(!is_string_entry_kind(nodes[i].kind))remap_pointers_in_il_entry(nodes[i].ptr,nodes[i].kind,discover,discover,FALSE);
 fprintf(out,"{\"schema\":\"edg-semantic/1\",\"lowered\":false,\"completeForEmission\":false");txt("producerVersion",il_header.compiler_version);txt("sourceLanguage",il_header.source_language==sl_C?"C":"C++");num("standardVersion",il_header.std_version);ref("mainRoutine",il_header.main_routine);ref("rootScope",il_header.primary_scope);ref("primarySource",il_header.primary_source_file);num("pointerBytes",targ_sizeof_pointer);num("intBytes",targ_sizeof_int);num("longBytes",targ_sizeof_long);num("longDoubleBytes",targ_sizeof_long_double);boolean("littleEndian",targ_little_endian);boolean("plainCharSigned",il_header.plain_chars_are_signed);key("configuration");fprintf(out,"{\"DO_IL_LOWERING\":%d,\"GENERATE_SOURCE_SEQUENCE_LISTS\":%d,\"EXTRA_SOURCE_POSITIONS_IN_IL\":%d,\"ALL_TEMPLATE_INFO_IN_IL\":%d,\"RECORD_MACROS_IN_IL\":%d,\"MACRO_INVOCATION_TREE_IN_IL\":%d,\"RECORD_MACRO_ARGS\":%d,\"DOING_SOURCE_ANALYSIS\":%d}",DO_IL_LOWERING,GENERATE_SOURCE_SEQUENCE_LISTS,EXTRA_SOURCE_POSITIONS_IN_IL,ALL_TEMPLATE_INFO_IN_IL,RECORD_MACROS_IN_IL,MACRO_INVOCATION_TREE_IN_IL,RECORD_MACRO_ARGS,DOING_SOURCE_ANALYSIS);key("nodes");fputc('[',out);
 for(size_t i=0;i<nodes.size();++i){if(i)fputc(',',out);emit(nodes[i]);}fputc(']',out);
 key("macroInvocations");fputc('[',out);bool first=true;for(a_macro_invocation_record_index i=1;i<il_header.num_macro_invocation_records;++i){a_macro_invocation_record_ptr p;set_macro_inv_record_ptr_to_index(il_header.root_macro_invocation_record_block,i,p);if(!p)continue;if(!first)fputc(',',out);first=false;fprintf(out,"{\"index\":%ld,\"parentIndex\":%ld",(long)i,(long)p->parent_macro_index);ref("definition",p->assoc_macro);simplepos("start",p->start);simplepos("end",p->end);txt("arguments",p->arguments);fputc('}',out);}fputc(']',out);
 key("diagnostics");fputc('[',out);for(size_t i=0;i<diagnostics.size();++i){if(i)fputc(',',out);fprintf(out,"{\"node\":%u,\"code\":\"SEM001\",\"message\":",diagnostics[i].first);quote(diagnostics[i].second.c_str());fputc('}',out);}fprintf(out,"]}\n");if(fclose(out)){perror(path);exit(2);}fprintf(stderr,"Semantic graph: %zu nodes, %zu explicitly reported adapter gaps\n",nodes.size(),diagnostics.size());
}
