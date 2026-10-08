#ifndef DOT_GEN_OBJECT_SOURCE_H
#define DOT_GEN_OBJECT_SOURCE_H
// Immutable source-declaration snapshots for the owned .dotobj adapter.
// Hooks observe EDG IL before lowering. They never alter EDG entries and are
// inactive for ordinary dot/C backends. Addresses are private lookup keys only;
// the published object contains IDs, owned strings and copied positions.
#include <cstdlib>
#include <map>
#include <set>
#include <string>
#include <vector>
#include "fe_common.h"
#include "il.h"
#include "types.h"
#include "lower_name.h"
BEGIN_EDG_NAMESPACE
namespace dot_object_source {
inline bool enabled() {
  static const bool value=[](){const char *v=std::getenv("EDG_DOT_OBJECT");return v && std::string(v)=="1";}();
  return value;
}
struct Type {
  unsigned id=0,underlying=0,execution=0;
  unsigned long long element_count=0;
  std::string kind,name;
  a_type_ptr record_type=nullptr;
  bool is_const=false,is_volatile=false;
  // Private physical representation fact also retained for 128-bit enums.
  bool is_int128=false,is_unsigned_int128=false;
  a_source_position position={};
  // Routine types are private signature snapshots, not scalar sourceTypes.
  unsigned result=0,object_type=0;
  bool object_is_const=false,object_is_volatile=false;
  std::string object_ref_qualifier="none";
  bool variadic=false;
  std::string exception_policy="unwind",parameter_mode="prototype";
  std::vector<unsigned> parameters;
};
struct Declaration {
  unsigned type=0;
  std::string name;
  a_source_position position={};
  // Private IL lookup keys only. The writer publishes a routine entity ID and
  // an explicit activation statement, never an EDG address or attribute text.
  a_routine_ptr cleanup=nullptr;
  bool has_initializer=false;
};
struct Function {
  Declaration declaration;
  unsigned signature_type=0;
  std::vector<unsigned> parameters;
  bool scope_captured=false;
  a_variable_ptr implicit_this=nullptr;
  std::vector<a_variable_ptr> parameter_variables;
};
struct RecordField {
  a_field_ptr il=nullptr;
  unsigned source_type=0;
  std::string name,original_name;
  unsigned long long offset=0;
  unsigned bit_offset=0,bit_width=0;
  bool bit_signed=false,is_mutable=false,no_unique_address=false,is_bit_field=false,has_initializer=false;
  a_source_position position={};
};
struct Record {
  std::string name,identity,error,layout_kind;
  bool template_identity=false;
  bool has_virtual_functions=false,has_bases=false;
  unsigned source_occurrence=0;
  unsigned long long size=0,alignment=0;
  a_source_position position={};
  std::vector<RecordField> fields;
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
struct PrelowerBase {
  a_base_class_ptr il=nullptr;
  a_type_ptr original_type=nullptr;
  unsigned source_type_snapshot=0;
  a_source_position position={};
  bool is_virtual=false;
  std::string access;
};
struct PrelowerRecord {
  a_source_position position={};
  std::string name,error;
  bool complete=false,trivial_copy_known=false,trivial_copy=false;
  unsigned long long size=0,alignment=0;
  std::vector<PrelowerField> fields;
  std::vector<PrelowerBase> bases;
};
struct State {
  std::map<a_type_ptr,unsigned> type_ids;
  std::vector<Type> types;
  std::map<a_routine_ptr,Function> functions;
  std::map<a_variable_ptr,Declaration> variables;
  std::map<a_type_ptr,Record> records;
  std::set<a_type_ptr> capturing_records;
  std::map<std::pair<a_seq_number,a_column_number>,unsigned> record_occurrence_counts;
  std::map<a_type_ptr,unsigned> record_occurrences;
  std::map<a_type_ptr,PrelowerRecord> prelower_records;
  std::set<a_type_ptr> abi_records;
  std::map<a_type_ptr,std::string> abi_record_keys;
  std::map<a_field_ptr,unsigned> original_field_ids;
  std::map<a_field_ptr,a_field_ptr> copied_fields;
  std::map<a_field_ptr,std::string> generated_field_roles;
  unsigned next_original_field=1;
};
inline State &state(){static State value;return value;}
inline std::string spelling(const a_source_correspondence &source) {
  if(!source.name)return "";
#if NEED_NAME_MANGLING
  if(source.unnamed_entity_given_fabricated_name)return "";
  if(source.name_has_been_mangled && source.unmangled_name_or_mangled_encoding)
    return source.unmangled_name_or_mangled_encoding;
#endif
  return source.name;
}
inline unsigned capture_type(a_type_ptr t);
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
  std::string qualified=value.name;
  bool local=value.name.empty() || c.originally_unnamed || t->source_corresp.is_local_to_function ||
    t->source_corresp.enclosing_routine || t->declared_in_function_prototype;
  for(auto scope=t->source_corresp.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      auto part=ns?spelling(ns->source_corresp):std::string{};
      if(!ns || ns->has_internal_linkage || part.empty())local=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind==sck_class_struct_union) {
      auto owner=scope->variant.assoc_type;
      if(owner && owner->variant.class_struct_union.is_template_class)value.template_identity=true;
      auto part=owner?spelling(owner->source_corresp):std::string{};
      if(part.empty())local=true;
      qualified=part+"::"+qualified;
    } else if(scope->kind!=sck_file)local=true;
  }
  value.identity=(local?"local-record:":"record:")+qualified;
  if(t->incomplete){s.records[t]=value;return;}
  value.has_virtual_functions=c.any_virtual_functions_including_in_base_classes;
  value.has_bases=extra && extra->base_classes;
  if(!value.size || value.size>2147483647ULL || !value.alignment || value.alignment>16)
    value.error="record outside complete plain layout profile";
#if GNU_EXTENSIONS_ALLOWED
  if(c.is_packed)value.error="packed record layout";
#endif
  if(c.max_member_alignment)value.error="packed record alignment";
  // Unsupported execution layout does not erase the original member index.
  // Keep its typed source declarations for the independent semantic index;
  // the execution writer still rejects value.error before publishing layouts.
  for(auto f=c.field_list;f;f=f->next) {
    RecordField field;field.il=f;field.name=spelling(f->source_corresp);field.original_name=field.name;field.offset=f->offset;
    field.has_initializer=f->has_initializer;field.is_mutable=f->is_mutable;field.no_unique_address=f->has_no_unique_address_attribute;field.is_bit_field=f->is_bit_field;
    field.position=f->source_corresp.decl_position;
    if(field.name.empty())field.name="__anonymous_member_"+std::to_string(value.fields.size());
    if(f->has_no_unique_address_attribute || f->is_lowered_base_class || f->class_subobject_with_tail_padding ||
       f->alignment>skip_typerefs(f->type)->alignment)
      value.error="record field outside plain layout profile";
    if(f->is_bit_field) {
      field.bit_offset=f->offset_bit_remainder;field.bit_width=f->bit_size;field.bit_signed=f->bit_field_is_signed;
      if(field.bit_offset>7 || field.bit_width>64 || skip_typerefs(f->type)->kind!=tk_integer ||
         (field.bit_width && skip_typerefs(f->type)->size>8))
        value.error="bitfield outside fixed integer layout";
    }
#if GNU_EXTENSIONS_ALLOWED
    if(f->is_packed)value.error="packed record field";
#endif
    field.source_type=capture_type(f->type);value.fields.push_back(field);
  }
  s.records[t]=value;
}
inline unsigned capture_type(a_type_ptr t) {
  if(!t)return 0;
  auto &s=state();auto prior=s.type_ids.find(t);
  if(prior!=s.type_ids.end()) {
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union)capture_record(t);
    return prior->second;
  }
  unsigned id=(unsigned)s.types.size()+1;s.type_ids[t]=id;s.types.emplace_back();
  Type value;value.id=id;value.name=spelling(t->source_corresp);value.position=t->source_corresp.decl_position;
  if(t->kind==tk_typeref) {
    value.kind=type_is_typedef(t)?"typedef":"qualified";
    value.is_const=(t->variant.typeref.qualifiers&TQ_CONST)!=0;
    value.is_volatile=(t->variant.typeref.qualifiers&TQ_VOLATILE)!=0;
    value.underlying=capture_type(t->variant.typeref.type);
    value.execution=s.types.at(value.underlying-1).execution;
    if(value.kind=="qualified") {
      value.name.clear();
      if(!value.is_const && !value.is_volatile) {
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
    if(t->variant.integer.enum_type && !t->variant.integer.bool_type)value.kind="enum";
  } else if(t->kind==tk_float) {
    if(t->variant.float_kind==fk_float && t->size==4){value.kind="float";value.execution=10;}
    else if(t->variant.float_kind==fk_double && t->size==8){value.kind="double";value.execution=11;}
    else if(t->size==16 && t->alignment==16) {
      if(t->variant.float_kind==fk_long_double)value.kind="long-double";
      else if(t->variant.float_kind==fk_float80)value.kind="float80";
      else if(t->variant.float_kind==fk_float64x)value.kind="float64x";
    }
  } else if(t->kind==tk_pointer) {
    value.kind=t->variant.pointer.is_reference?(t->variant.pointer.is_rvalue_reference?"rvalue-reference":"lvalue-reference"):"pointer";
    value.underlying=capture_type(t->variant.pointer.type);
    // Execution pointer IDs are interned by the object writer, independently
    // of EDG addresses and of the source typedef/qualification structure.
  } else if(t->kind==tk_array && !t->variant.array.is_variable_size_array &&
            !t->variant.array.is_vla && !t->variant.array.is_template_dependent_size_array &&
            true) {
    value.kind="array";
    value.underlying=capture_type(t->variant.array.element_type);
    value.element_count=t->variant.array.variant.number_of_elements;
  } else if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
    if(t->variant.class_struct_union.extra_info && t->variant.class_struct_union.extra_info->is_va_list_tag)
      value.kind="va-list";
    else {value.kind="record";value.record_type=t;capture_record(t);}
  } else if(t->kind==tk_routine) {
    value.kind="routine";
    value.result=capture_type(t->variant.routine.return_type);
    auto extra=t->variant.routine.extra_info;
    if(extra->this_class) {
      value.object_type=capture_type(extra->this_class);
      value.object_is_const=(extra->qualifiers&TQ_CONST)!=0;
      value.object_is_volatile=(extra->qualifiers&TQ_VOLATILE)!=0;
      value.object_ref_qualifier=extra->ref_qualifiers==rqk_lvalue?"lvalue":
        extra->ref_qualifiers==rqk_rvalue?"rvalue":"none";
    }
    value.variadic=t->variant.routine.extra_info->has_ellipsis;
    value.parameter_mode=t->variant.routine.extra_info->prototyped?"prototype":"unspecified";
    auto specification=t->variant.routine.extra_info->exception_specification;
    if(specification && !specification->throw_any)
      value.exception_policy=specification->is_noexcept?"terminate":"unsupported-dynamic";
    for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)
      value.parameters.push_back(capture_type(p->declared_type?p->declared_type:p->type));
  }
  // Unknown entries are retained privately as unavailable, never exported as
  // opaque payloads. An unused unsupported declaration cannot block the TU.
  if(t->kind!=tk_typeref && value.kind!="record" && value.kind!="enum")value.name.clear();
  s.types[id-1]=value;return id;
}
inline void observe_type(a_type_ptr type){if(enabled())capture_type(type);}
inline void capture_routine(a_routine_ptr r) {
  if(!r)return;
  auto &s=state();if(s.functions.count(r))return;
  a_type_ptr type=r->type;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  if(r->declared_type)type=r->declared_type;
#endif
  auto id=capture_type(type);
  while(s.types.at(id-1).underlying)id=s.types.at(id-1).underlying;
  const auto signature=s.types.at(id-1);
  Function function;function.declaration.type=signature.result;function.signature_type=id;
  function.declaration.name=spelling(r->source_corresp);
  function.declaration.position=r->source_corresp.decl_position;
  function.parameters=signature.parameters;s.functions[r]=function;
}
inline void observe_routine(a_routine_ptr routine){if(enabled())capture_routine(routine);}
inline bool abi_storage_type(a_type_ptr type) {
  type=skip_typerefs(type);
  while(type->kind==tk_array || type->kind==tk_pointer) {
    type=skip_typerefs(type->kind==tk_array?type->variant.array.element_type:type->variant.pointer.type);
  }
  return state().abi_records.count(type)!=0;
}
inline void capture_variable(a_variable_ptr v) {
  auto &s=state();if(s.variables.count(v) || v->is_this_parameter || v->compiler_generated || abi_storage_type(v->type))return;
  a_type_ptr type=v->type;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  if(v->declared_type)type=v->declared_type;
#endif
  Declaration value;value.type=capture_type(type);value.name=spelling(v->source_corresp);
  value.has_initializer=v->init_kind!=initk_none;
#if GNU_EXTENSIONS_ALLOWED
  value.cleanup=v->cleanup_routine;
#endif
  value.position=v->source_corresp.decl_position;s.variables[v]=value;
}
inline void capture_scope(a_scope_ptr scope) {
  if(!scope)return;
  if(scope->kind==sck_function) {
    capture_routine(scope->variant.routine.ptr);
    auto &function=state().functions.at(scope->variant.routine.ptr);
    if(!function.scope_captured) {
      function.scope_captured=true;function.implicit_this=scope->variant.routine.this_param_variable;
      for(auto v=scope->variant.routine.parameters;v;v=v->next) {
        if(v!=function.implicit_this)function.parameter_variables.push_back(v);
      }
    }
    for(auto v=scope->variant.routine.parameters;v;v=v->next)capture_variable(v);
  }
  for(auto t=scope->types;t;t=t->next)capture_type(t);
  for(auto v=scope->nonstatic_variables;v;v=v->next)capture_variable(v);
  for(auto v=scope->variables;v;v=v->next)capture_variable(v);
  for(auto r=scope->routines;r;r=r->next)capture_routine(r);
  for(auto child=scope->scopes;child;child=child->next)capture_scope(child);
  for(auto ns=scope->namespaces;ns;ns=ns->next)
    if(!ns->is_namespace_alias)capture_scope(ns->variant.assoc_scope);
}
inline void observe_scope(a_scope_ptr scope){if(enabled())capture_scope(scope);}
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
  s.prelower_records.emplace(type,value);
}
inline void observe_abi_record(a_type_ptr type) {
  if(enabled())state().abi_records.insert(type);
}
inline void observe_typeinfo_record(a_type_ptr type,a_type_info_kind kind,unsigned long long base_count) {
  if(!enabled())return;
  // The kind comes from the actual RTTI construction operation. The ABI's
  // canonical kind name is not guessed from a generated record spelling.
  if((unsigned)kind>=(unsigned)tik_last || !type_info_names[(int)kind])return;
  state().abi_record_keys[type]=std::string("abi:rtti:")+type_info_names[(int)kind]+":bases:"+std::to_string(base_count);
}
inline void observe_field_copy(a_field_ptr original,a_field_ptr copy) {
  if(!enabled())return;
  auto &s=state();s.copied_fields[copy]=original;
  auto source=s.original_field_ids.find(original);
  if(source!=s.original_field_ids.end())s.original_field_ids[copy]=source->second;
  auto generated=s.generated_field_roles.find(original);
  if(generated!=s.generated_field_roles.end())s.generated_field_roles[copy]=generated->second;
}
inline void observe_vptr_field(a_field_ptr field) {
  if(enabled())state().generated_field_roles[field]="vptr";
}
}
END_EDG_NAMESPACE
#endif
