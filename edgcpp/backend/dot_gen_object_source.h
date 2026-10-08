#ifndef DOT_GEN_OBJECT_SOURCE_H
#define DOT_GEN_OBJECT_SOURCE_H
// Immutable source-declaration snapshots for the owned .dotobj adapter.
// Hooks observe EDG IL before lowering. They never alter EDG entries and are
// inactive for ordinary dot/C backends. Addresses are private lookup keys only;
// the published object contains IDs, owned strings and copied positions.
#include <cstdlib>
#include <map>
#include <string>
#include <vector>
#include "fe_common.h"
#include "il.h"
#include "types.h"
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
  bool is_const=false,is_volatile=false;
  a_source_position position={};
  // Routine types are private signature snapshots, not scalar sourceTypes.
  unsigned result=0;
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
  std::vector<unsigned> parameters;
};
struct State {
  std::map<a_type_ptr,unsigned> type_ids;
  std::vector<Type> types;
  std::map<a_routine_ptr,Function> functions;
  std::map<a_variable_ptr,Declaration> variables;
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
inline unsigned capture_type(a_type_ptr t) {
  if(!t)return 0;
  auto &s=state();auto prior=s.type_ids.find(t);
  if(prior!=s.type_ids.end())return prior->second;
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
    else if(!t->variant.integer.enum_type)switch(t->variant.integer.int_kind) {
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
      default:break;
    }
  } else if(t->kind==tk_float) {
    if(t->variant.float_kind==fk_float && t->size==4){value.kind="float";value.execution=10;}
    else if(t->variant.float_kind==fk_double && t->size==8){value.kind="double";value.execution=11;}
  } else if(t->kind==tk_pointer && !t->variant.pointer.is_reference) {
    value.kind="pointer";
    value.underlying=capture_type(t->variant.pointer.type);
    // Execution pointer IDs are interned by the object writer, independently
    // of EDG addresses and of the source typedef/qualification structure.
  } else if(t->kind==tk_array && !t->variant.array.is_variable_size_array &&
            !t->variant.array.is_vla && !t->variant.array.is_template_dependent_size_array &&
            !t->variant.array.bound_is_zero) {
    value.kind="array";
    value.underlying=capture_type(t->variant.array.element_type);
    value.element_count=t->variant.array.variant.number_of_elements;
  } else if(t->kind==tk_routine) {
    value.kind="routine";
    value.result=capture_type(t->variant.routine.return_type);
    for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)
      value.parameters.push_back(capture_type(p->declared_type?p->declared_type:p->type));
  }
  // Unknown entries are retained privately as unavailable, never exported as
  // opaque payloads. An unused unsupported declaration cannot block the TU.
  if(t->kind!=tk_typeref)value.name.clear();
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
  Function function;function.declaration.type=signature.result;
  function.declaration.name=spelling(r->source_corresp);
  function.declaration.position=r->source_corresp.decl_position;
  function.parameters=signature.parameters;s.functions[r]=function;
}
inline void observe_routine(a_routine_ptr routine){if(enabled())capture_routine(routine);}
inline void capture_variable(a_variable_ptr v) {
  auto &s=state();if(s.variables.count(v) || v->compiler_generated)return;
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
    for(auto v=scope->variant.routine.parameters;v;v=v->next)capture_variable(v);
  }
  for(auto v=scope->nonstatic_variables;v;v=v->next)capture_variable(v);
  for(auto r=scope->routines;r;r=r->next)capture_routine(r);
  for(auto child=scope->scopes;child;child=child->next)capture_scope(child);
  for(auto ns=scope->namespaces;ns;ns=ns->next)
    if(!ns->is_namespace_alias)capture_scope(ns->variant.assoc_scope);
}
inline void observe_scope(a_scope_ptr scope){if(enabled())capture_scope(scope);}
}
END_EDG_NAMESPACE
#endif
