#ifndef DOT_GEN_SEMANTIC_H
#define DOT_GEN_SEMANTIC_H
// A source-shaped, resolved program projection. Only owned program entities
// are recorded; this is not a serializer for arbitrary EDG internal entries.
#include <cstdlib>
#include <cstdio>
#include <map>
#include <set>
#include <string>
#include "fe_common.h"
#include "il.h"
#include "types.h"
#include "const_ints.h"
#include "exprutil.h"
#include "dot_gen_semantic_sqlite.h"
BEGIN_EDG_NAMESPACE
namespace dot_semantic {
using Id=unsigned long long;
using dot_semantic_sqlite::Value;
inline bool enabled() { const char *p=std::getenv("EDG_DOT_SEMANTIC");return p && *p; }
inline Value optional(Id id) { return id?Value(id):Value(nullptr); }
inline std::string spelling(const a_source_correspondence &s) {
  if(!s.name)return "";
#if NEED_NAME_MANGLING
  if(s.name_has_been_mangled && s.unmangled_name_or_mangled_encoding)
    return s.unmangled_name_or_mangled_encoding;
#endif
  return s.name;
}
inline const char *linkage(const a_source_correspondence &s) {
  return s.name_linkage==nlk_external || s.name_linkage==nlk_cplusplus_external?"external":"internal";
}
struct Location { Id file=0;unsigned long long line=0,column=0; };
struct State {
  dot_semantic_sqlite::Database db;
  Id next_type=1,next_symbol=1,next_node=1,next_field=1,next_file=1,next_template=1,next_instance=1;
  Id next_lifetime=1,next_initialization=1,next_base=1;
  Id function=0;
  std::map<a_type_ptr,Id> types;
  std::map<a_routine_ptr,Id> routines;
  std::map<a_routine_ptr,Id> routine_source_types;
  std::map<a_variable_ptr,Id> variables;
  std::map<a_field_ptr,Id> fields;
  std::map<a_base_class_ptr,Id> base_subobjects;
  std::map<a_template_ptr,Id> templates;
  std::map<std::string,Id> files;
  std::map<an_expr_node_ptr,Id> expressions;
  std::set<a_routine_ptr> bodies;
  std::set<a_variable_ptr> local_variables;
  std::set<a_dynamic_init_ptr> declaration_initializers;
  std::set<a_scope_ptr> declaration_scopes;
  std::map<an_object_lifetime_ptr,Id> lifetimes,static_lifetimes;
  std::map<a_dynamic_init_ptr,Id> initializations,initialization_values;
  std::map<a_dynamic_init_ptr,Id> initialization_types;
  std::set<a_dynamic_init_ptr> initializing;
  State():db(std::getenv("EDG_DOT_SEMANTIC")) {}
  [[noreturn]] void missing(const std::string &what,int kind=-1) {
    std::fprintf(stderr,"semantic exporter missing %s%s\n",what.c_str(),
      (kind<0?std::string():" (EDG kind "+std::to_string(kind)+")").c_str());
    std::exit(1);
  }
  Location location(const a_source_position &p) {
    if(!p.seq)return {};
    a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean end=FALSE;
    conv_seq_to_file_and_line(p.seq,&display,&full,&line,&end);
    std::string path=full?full:display?display:"";
    auto found=files.find(path);
    Id id=found==files.end()?next_file++:found->second;
    if(found==files.end()){files[path]=id;db.insert("files",{{"id",id},{"path",path}});}
    return {id,(unsigned long long)line,(unsigned long long)p.column};
  }
  Id template_id(a_template_ptr t) {
    if(!t)return 0;
    auto found=templates.find(t);if(found!=templates.end())return found->second;
    Id id=next_template++;templates[t]=id;auto pos=location(t->source_corresp.decl_position);
    db.insert("templates",{{"id",id},{"kind","template"},{"name",spelling(t->source_corresp)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    return id;
  }
  std::string integer(a_constant_ptr c) {
    if(!c || c->kind!=ck_integer)missing("noninteger constant value",c?int(c->kind):-1);
    a_boolean overflow=FALSE;
    std::string value=int_constant_is_signed(c)?std::to_string((long long)value_of_integer_constant(c,&overflow)):
      std::to_string((unsigned long long)unsigned_value_of_integer_constant(c,&overflow));
    if(overflow)missing("integer constant wider than 64 bits");
    return value;
  }
  void instance(a_template_ptr t,a_template_arg_ptr args,Id symbol,Id type) {
    if(!t)return;
    Id id=next_instance++;
    db.insert("template_instances",{{"id",id},{"template_id",template_id(t)},
      {"symbol_id",optional(symbol)},{"type_id",optional(type)}});
    unsigned ordinal=0;
    for(auto arg=args;arg;arg=arg->next,++ordinal) {
      if(arg->kind==tak_type)
        db.insert("template_arguments",{{"instance_id",id},{"ordinal",ordinal},{"kind","type"},{"type_id",type_id(arg->variant.type)}});
      else if(arg->kind==tak_nontype)
        db.insert("template_arguments",{{"instance_id",id},{"ordinal",ordinal},{"kind","constant"},
          {"type_id",type_id(arg->variant.constant->type)},{"constant_text",integer(arg->variant.constant)}});
      else missing("template argument",int(arg->kind));
    }
  }
  const char *exception_policy(a_type_ptr type) {
    auto t=skip_typerefs(type);
    if(t->kind!=tk_routine)return "unwind";
    auto spec=t->variant.routine.extra_info->exception_specification;
    if(!spec || spec->throw_any)return "unwind";
    if(spec->indeterminate || spec->arg_cached || spec->copy_from_prototype)return "dependent";
    if(spec->is_noexcept)return "terminate";
    missing("dynamic exception specification projection");
  }
  Id type_id(a_type_ptr t) {
    if(!t)return 0;
    auto found=types.find(t);if(found!=types.end())return found->second;
    Id id=next_type++;types[t]=id;
    std::string kind,name=spelling(t->source_corresp),record_kind,builtin;
    Id underlying=0,pointee=0,element=0,result=0,object=0;
    Value count=nullptr;bool variadic=false,incomplete=is_incomplete_type(t);
    unsigned qualifiers=get_type_qualifiers(t);
    if(qualifiers & ~(TQ_CONST|TQ_VOLATILE|TQ_RESTRICT))missing("extended type qualifiers");
    switch(t->kind) {
      case tk_void:kind="void";builtin="void";break;
      case tk_typeref:
        kind=type_is_typedef(t)?"typedef":"qualified";underlying=type_id(t->variant.typeref.type);break;
      case tk_integer:
        if(t->variant.integer.enum_type && !t->variant.integer.bool_type)missing("enum metadata");
        if(t->variant.integer.bool_type){kind="bool";builtin="bool";}
        else switch(t->variant.integer.int_kind) {
          case ik_char:kind=targ_has_signed_chars?"i8":"u8";builtin="char";break;
          case ik_signed_char:kind="i8";builtin="signed char";break;
          case ik_unsigned_char:kind="u8";builtin="unsigned char";break;
          case ik_short:kind="i16";builtin="short";break;
          case ik_unsigned_short:kind="u16";builtin="unsigned short";break;
          case ik_int:kind="i32";builtin="int";break;
          case ik_unsigned_int:kind="u32";builtin="unsigned int";break;
          case ik_long:kind=t->size==8?"i64":"i32";builtin="long";break;
          case ik_long_long:kind="i64";builtin="long long";break;
          case ik_unsigned_long:kind=t->size==8?"u64":"u32";builtin="unsigned long";break;
          case ik_unsigned_long_long:kind="u64";builtin="unsigned long long";break;
          default:missing("integer type",int(t->variant.integer.int_kind));
        }break;
      case tk_float:
        if(t->variant.float_kind==fk_float){kind="f32";builtin="float";}
        else if(t->variant.float_kind==fk_double){kind="f64";builtin="double";}
        else missing("floating type",int(t->variant.float_kind));break;
      case tk_pointer:
        kind=t->variant.pointer.is_reference?(t->variant.pointer.is_rvalue_reference?"rvalue-reference":"lvalue-reference"):"pointer";
        pointee=type_id(t->variant.pointer.type);break;
      case tk_array:
        if(t->variant.array.is_variable_size_array || t->variant.array.is_template_dependent_size_array)missing("dependent/runtime array bound");
        kind="array";element=type_id(t->variant.array.element_type);
        if(t->variant.array.variant.number_of_elements || t->variant.array.bound_is_zero)count=Value(t->variant.array.variant.number_of_elements);
        break;
      case tk_routine:
        kind="function";result=type_id(t->variant.routine.return_type);
        variadic=t->variant.routine.extra_info->has_ellipsis;
        object=type_id(t->variant.routine.extra_info->this_class);break;
      case tk_struct:case tk_class:case tk_union:
        kind="record";record_kind=t->kind==tk_struct?"struct":t->kind==tk_class?"class":"union";
        break;
      default:missing("type",int(t->kind));
    }
    db.insert("types",{{"id",id},{"kind",kind},{"name",name},{"link_name",t->source_corresp.name?t->source_corresp.name:""},
      {"linkage",linkage(t->source_corresp)},{"size",t->size},{"alignment",t->alignment},
      {"underlying_type_id",optional(underlying)},{"pointee_type_id",optional(pointee)},
      {"element_type_id",optional(element)},{"element_count",count},{"return_type_id",optional(result)},
      {"object_type_id",optional(object)},{"is_const",bool(qualifiers&TQ_CONST)},
      {"is_volatile",bool(qualifiers&TQ_VOLATILE)},{"is_restrict",bool(qualifiers&TQ_RESTRICT)},
      {"is_variadic",variadic},{"is_incomplete",incomplete},{"record_kind",record_kind.empty()?Value(nullptr):Value(record_kind)},
      {"builtin_name",builtin.empty()?Value(nullptr):Value(builtin)}});
    if(t->kind==tk_routine) {
      db.update("types","id",id,{{"exception_policy",exception_policy(t)}});
      unsigned ordinal=0;
      for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)
        db.insert("type_parameters",{{"type_id",id},{"ordinal",ordinal++},{"parameter_type_id",type_id(p->type)}});
    }
    if(kind=="record") {
      auto extra=class_type_supp(t);
      // EDG's list identifies subobjects, including repeated indirect bases;
      // the direct base number separately retains source declaration order.
      for(auto base=extra->base_classes;base;base=base->next)
        if(!base_subobjects.count(base))base_subobjects[base]=next_base++;
      unsigned base_ordinal=0;
      for(auto base=extra->base_classes;base;base=base->next,++base_ordinal) {
        const char *access=nullptr;
        if(base->direct)for(auto path=base->derivation;path;path=path->next)if(path->direct) {
          switch(path->access) {
            case as_public:access="public";break;
            case as_protected:access="protected";break;
            case as_private:access="private";break;
            default:missing("base access specifier",int(path->access));
          }
          break;
        }
        auto pos=location(base->decl_position);
        db.insert("base_subobjects",{{"id",base_subobjects[base]},{"owner_type_id",id},
          {"base_type_id",type_id(base->type)},{"declared_type_id",optional(type_id(base->orig_type))},
          {"ordinal",base_ordinal},{"direct_ordinal",base->direct?Value(base->direct_base_number-1):Value(nullptr)},
          {"byte_offset",base->offset},{"is_direct",bool(base->direct)},{"is_virtual",bool(base->is_virtual)},
          {"is_optimized_empty",bool(base->is_optimized_empty_base)},{"access",access},
          {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
      }
      unsigned ordinal=0;
      for(auto f=t->variant.class_struct_union.field_list;f;f=f->next,++ordinal)field_id(f,id,ordinal);
      instance(extra->assoc_template,extra->template_arg_list,0,id);
    }
    return id;
  }
  Id base_id(a_base_class_ptr base) {
    if(!base)return 0;
    type_id(base->derived_class);
    auto found=base_subobjects.find(base);
    if(found==base_subobjects.end())missing("selected base subobject");
    return found->second;
  }
  Id field_id(a_field_ptr f,Id owner=0,unsigned ordinal=0) {
    auto found=fields.find(f);if(found!=fields.end())return found->second;
    if(!owner){type_id(parent_class_of(f));found=fields.find(f);if(found!=fields.end())return found->second;missing("field owner");}
    Id id=next_field++;fields[f]=id;auto pos=location(f->source_corresp.decl_position);
    db.insert("fields",{{"id",id},{"owner_type_id",owner},{"name",spelling(f->source_corresp)},
      {"type_id",type_id(f->type)},{"ordinal",ordinal},{"byte_offset",f->offset},
      {"bit_offset",f->is_bit_field?Value(f->offset_bit_remainder):Value(nullptr)},
      {"bit_width",f->is_bit_field?Value(f->bit_size):Value(nullptr)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});return id;
  }
  Id routine(a_routine_ptr r) {
    auto found=routines.find(r);if(found!=routines.end())return found->second;
    Id id=next_symbol++;routines[r]=id;auto pos=location(r->source_corresp.decl_position);
    Id signature=type_id(r->type);routine_source_types[r]=signature;
    db.insert("symbols",{{"id",id},{"kind","function"},{"name",spelling(r->source_corresp)},
      {"link_name",r->source_corresp.name?r->source_corresp.name:""},{"linkage",linkage(r->source_corresp)},
      {"type_id",signature},{"is_definition",r->function_def_number!=NULL_function_def_number},
      {"definition_kind",r->is_inline || (r->is_template_function && !r->is_specialized)?"weak":"strong"},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    instance(r->assoc_template,r->template_arg_list,id,0);return id;
  }
  Id variable(a_variable_ptr v,bool local=false,bool parameter=false) {
    auto found=variables.find(v);if(found!=variables.end())return found->second;
    Id id=next_symbol++;variables[v]=id;auto pos=location(v->source_corresp.decl_position);
    if(local)local_variables.insert(v);
    if(is_effective_thread_local(v))missing("thread-local storage");
    if(local && !parameter && v->storage_class==sc_static)missing("local static storage initialization");
    if(!local && v->init_kind!=initk_none && v->init_kind!=initk_zero && v->init_kind!=initk_static)
      missing("global dynamic initializer");
    db.insert("symbols",{{"id",id},{"kind",parameter?"parameter":local?"local":"global"},
      {"name",spelling(v->source_corresp)},{"link_name",v->source_corresp.name?v->source_corresp.name:""},
      {"linkage",local?"none":linkage(v->source_corresp)},{"type_id",type_id(v->type)},
      {"parent_symbol_id",local?optional(function):Value(nullptr)},
      {"is_definition",v->storage_class!=sc_extern},{"file_id",optional(pos.file)},
      {"line",pos.line},{"column",pos.column}});
    if(!local && v->init_kind==initk_static) {
      Id previous=function;function=0;
      Id initializer=constant(v->initializer.constant,v->source_corresp.decl_position);
      db.insert("global_initializers",{{"symbol_id",id},{"initializer_node_id",initializer}});
      function=previous;
    }
    return id;
  }
  Id node(const char *kind,const a_source_position &p,Id type=0,const char *category=nullptr,
          const char *op=nullptr,const std::string &constant="",Id symbol=0,Id field=0,Id original=0,bool synthetic=false) {
    Id id=next_node++;auto pos=location(p);
    db.insert("nodes",{{"id",id},{"function_id",optional(function)},{"kind",kind},{"type_id",optional(type)},
      {"value_category",category},{"operator",op},{"constant_text",constant.empty()?Value(nullptr):Value(constant)},
      {"symbol_id",optional(symbol)},{"field_id",optional(field)},{"original_lvalue_type_id",optional(original)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column},{"is_synthetic",synthetic}});
    return id;
  }
  void edge(Id parent,const char *role,unsigned ordinal,Id child) {
    if(child)db.insert("edges",{{"parent_id",parent},{"role",role},{"ordinal",ordinal},{"child_id",child}});
  }
  Id constant(a_constant_ptr c,const a_source_position &p) {
    if(c->kind!=ck_integer)missing("constant",int(c->kind));
    return node("constant",p,type_id(c->type),"prvalue",nullptr,integer(c));
  }
  Id lifetime_id(an_object_lifetime_ptr value) {
    if(!value)return 0;
    bool persistent=value->kind==olk_global_static || value->kind==olk_function_static;
    auto &ids=persistent?static_lifetimes:lifetimes;
    auto found=ids.find(value);if(found!=ids.end())return found->second;
    Id id=next_lifetime++;ids[value]=id;
    const char *kind=nullptr;
    switch(value->kind) {
      case olk_global_static:kind="global-static";break;
      case olk_function_static:kind="function-static";break;
      case olk_block:kind="block";break;
      case olk_block_after_label:kind="block-after-label";break;
      case olk_expr_temporary:kind="expression-temporary";break;
      case olk_try_block:kind="try-block";break;
      default:missing("object lifetime kind",int(value->kind));
    }
    db.insert("lifetimes",{{"id",id},{"function_id",persistent?Value(nullptr):optional(function)},
      {"kind",kind},{"parent_id",optional(lifetime_id(value->parent_lifetime))},
      {"parent_initialization_id",optional(initialization_id(value->parent_destruction_sublist))}});
    if(persistent) {
      if(value->destructions)missing("persistent object destruction schedule");
      return id;
    }
    unsigned ordinal=0;
    for(auto action=value->destructions;action;action=action->next_in_destruction_list)
      db.insert("lifetime_destructions",{{"lifetime_id",id},{"ordinal",ordinal++},{"initialization_id",initialization_id(action)}});
    ordinal=0;
    for(auto child=value->child_lifetime;child;child=child->next)
      db.update("lifetimes","id",lifetime_id(child),{{"sibling_ordinal",ordinal++}});
    return id;
  }
  void attach_lifetime(Id owner,an_object_lifetime_ptr value) {
    if(!value)return;
    Id id=lifetime_id(value);
    db.update("nodes","id",owner,{{"lifetime_id",id}});
    db.update("lifetimes","id",id,{{"owner_node_id",owner}});
  }
  Id initialization_id(a_dynamic_init_ptr init,a_type_ptr target=nullptr) {
    if(!init)return 0;
    Id target_type=type_id(target?target:init->variable?init->variable->type:nullptr);
    auto found=initializations.find(init);
    if(found!=initializations.end()) {
      if(target_type && !initialization_types[init]) {
        initialization_types[init]=target_type;
        if(!initializing.count(init))db.update("initializations","id",found->second,{{"type_id",target_type}});
      }
      return found->second;
    }
    Id id=next_initialization++;initializations[init]=id;initialization_types[init]=target_type;initializing.insert(init);
    const char *kind=nullptr;Id value=0,constructor=0;
    bool implied_source=false,sequenced=false,value_init=false;
    a_source_position pos={};if(init->variable)pos=init->variable->source_corresp.decl_position;
    switch(init->kind) {
      case dik_none:kind="none";break;
      case dik_zero:kind="zero";break;
      case dik_expression:kind="expression";value=expression(init->variant.expression);break;
      case dik_constant:kind="constant";value=constant(init->variant.constant.ptr,pos);break;
      case dik_constructor:
        kind="constructor";
        if(!init->variant.constructor.ptr)missing("selected constructor");
        if(init->variant.constructor.is_array_copy)missing("array-copy constructor initialization");
        constructor=routine(init->variant.constructor.ptr);
        implied_source=init->variant.constructor.is_copy_constructor_with_implied_source;
        sequenced=init->variant.constructor.has_sequenced_arguments;
        value_init=init->variant.constructor.value_initialization;
        break;
      case dik_bitwise_copy:
        kind="bitwise-copy";value=expression(init->variant.bitwise_copy.source);
        implied_source=init->variant.bitwise_copy.source==nullptr;break;
      default:missing("dynamic initialization",int(init->kind));
    }
    Id life=lifetime_id(init->lifetime),expr_life=lifetime_id(init->init_expr_lifetime);
    Id overlap=lifetime_id(init->lifetime_of_overlapping_temps);
    db.insert("initializations",{{"id",id},{"function_id",optional(function)},{"kind",kind},
      {"type_id",optional(initialization_types[init])},
      {"variable_symbol_id",optional(init->variable?variable(init->variable,true):0)},
      {"constructor_symbol_id",optional(constructor)},
      {"destructor_symbol_id",optional(init->destructor?routine(init->destructor):0)},
      {"lifetime_id",optional(life)},{"init_expr_lifetime_id",optional(expr_life)},{"value_node_id",optional(value)},
      {"conditional",bool(init->inside_conditional_expression)},{"unordered",bool(init->unordered)},
      {"overlaps_inner",bool(init->overlaps_temps_in_inner_lifetime)},
      {"partial_aggregate",bool(init->destruction_is_for_partially_constructed_aggregate)},
      {"free_on_exception",bool(init->is_freeing_of_storage_on_exception)},
      {"follows_exec",bool(init->follows_an_exec_statement)},{"temporary_lifetime",bool(init->has_temporary_lifetime)},
      {"is_constructor_init",bool(init->is_constructor_init)},{"is_partially_initialized",bool(init->is_partially_initialized)},
      {"static_storage",bool(init->static_temp)},{"implied_source",implied_source},
      {"sequenced_arguments",sequenced},{"value_initialization",value_init},{"overlap_lifetime_id",optional(overlap)}});
    initialization_values[init]=value;initializing.erase(init);
    if(init->kind==dik_constructor) {
      unsigned ordinal=0;for(auto argument=init->variant.constructor.args;argument;argument=argument->next)
        db.insert("initialization_arguments",{{"initialization_id",id},{"ordinal",ordinal++},{"node_id",expression(argument)}});
    }
    return id;
  }
  const char *operation(an_expr_operator_kind kind) {
    switch(kind) {
#define DOT_SEMANTIC_OP(edg,text) case edg:return text
      DOT_SEMANTIC_OP(eok_address_of,"address-of");DOT_SEMANTIC_OP(eok_reference_to,"reference-to");
      DOT_SEMANTIC_OP(eok_indirect,"indirect");DOT_SEMANTIC_OP(eok_ref_indirect,"ref-indirect");
      DOT_SEMANTIC_OP(eok_cast,"cast");DOT_SEMANTIC_OP(eok_ref_cast,"ref-cast");
      DOT_SEMANTIC_OP(eok_lvalue_cast,"lvalue-cast");DOT_SEMANTIC_OP(eok_lvalue_adjust,"lvalue-adjust");
      DOT_SEMANTIC_OP(eok_base_class_cast,"base-class-cast");DOT_SEMANTIC_OP(eok_derived_class_cast,"derived-class-cast");
      DOT_SEMANTIC_OP(eok_bool_cast,"bool-cast");DOT_SEMANTIC_OP(eok_array_to_pointer,"array-to-pointer");
      DOT_SEMANTIC_OP(eok_add,"add");DOT_SEMANTIC_OP(eok_subtract,"subtract");
      DOT_SEMANTIC_OP(eok_multiply,"multiply");DOT_SEMANTIC_OP(eok_divide,"divide");DOT_SEMANTIC_OP(eok_remainder,"remainder");
      DOT_SEMANTIC_OP(eok_padd,"pointer-add");DOT_SEMANTIC_OP(eok_psubtract,"pointer-subtract");
      DOT_SEMANTIC_OP(eok_eq,"eq");DOT_SEMANTIC_OP(eok_ne,"ne");DOT_SEMANTIC_OP(eok_lt,"lt");
      DOT_SEMANTIC_OP(eok_le,"le");DOT_SEMANTIC_OP(eok_gt,"gt");DOT_SEMANTIC_OP(eok_ge,"ge");
      DOT_SEMANTIC_OP(eok_assign,"assign");DOT_SEMANTIC_OP(eok_add_assign,"add-assign");
      DOT_SEMANTIC_OP(eok_subtract_assign,"subtract-assign");DOT_SEMANTIC_OP(eok_multiply_assign,"multiply-assign");
      DOT_SEMANTIC_OP(eok_divide_assign,"divide-assign");DOT_SEMANTIC_OP(eok_remainder_assign,"remainder-assign");
      DOT_SEMANTIC_OP(eok_land,"logical-and");DOT_SEMANTIC_OP(eok_lor,"logical-or");
      DOT_SEMANTIC_OP(eok_not,"not");DOT_SEMANTIC_OP(eok_negate,"negate");
      DOT_SEMANTIC_OP(eok_complement,"complement");DOT_SEMANTIC_OP(eok_and,"bit-and");
      DOT_SEMANTIC_OP(eok_or,"bit-or");DOT_SEMANTIC_OP(eok_xor,"bit-xor");
      DOT_SEMANTIC_OP(eok_shiftl,"shift-left");DOT_SEMANTIC_OP(eok_shiftr,"shift-right");
      DOT_SEMANTIC_OP(eok_pre_incr,"pre-increment");DOT_SEMANTIC_OP(eok_post_incr,"post-increment");
      DOT_SEMANTIC_OP(eok_pre_decr,"pre-decrement");DOT_SEMANTIC_OP(eok_post_decr,"post-decrement");
      DOT_SEMANTIC_OP(eok_comma,"comma");DOT_SEMANTIC_OP(eok_question,"conditional");
      DOT_SEMANTIC_OP(eok_subscript,"subscript");DOT_SEMANTIC_OP(eok_dot_field,"dot-field");
      DOT_SEMANTIC_OP(eok_points_to_field,"points-to-field");DOT_SEMANTIC_OP(eok_call,"call");
      DOT_SEMANTIC_OP(eok_dot_member_call,"dot-member-call");DOT_SEMANTIC_OP(eok_points_to_member_call,"points-to-member-call");
#undef DOT_SEMANTIC_OP
      default:missing("expression operation",int(kind));
    }
  }
  void class_conversion(Id id,an_expr_node_ptr e) {
    auto operand=e->variant.operation.operands;
    bool derived=e->variant.operation.kind==eok_derived_class_cast;
    a_type_ptr source=operand->type,target=e->type;
    // Mirror lower_il::related_class_cast_step: EDG represents a resolved
    // conversion as direct/virtual steps, so only that exact step is selected.
    // This is not a search through possible source-language derivation paths.
    bool pointer_form=!e->is_lvalue && is_ptr_or_ref_type(e->type);
    if(pointer_form){source=type_pointed_to(source);target=type_pointed_to(target);}
    source=skip_typerefs(source);target=skip_typerefs(target);
    auto owner=derived?target:source;auto base_type=derived?source:target;
    auto base=class_type_supp(owner)->base_classes;
    for(;base;base=base->next)
      if((base->direct || base->is_virtual) && base->type==base_type)break;
    if(!base)missing("EDG selected class conversion step");
    auto representation=[](a_type_ptr type)->const char * {
      auto t=skip_typerefs(type);
      return t->kind!=tk_pointer?"object":t->variant.pointer.is_reference?"reference":"pointer";
    };
    const char *source_rep=representation(operand->type),*result_rep=representation(e->type);
    db.insert("class_conversions",{{"node_id",id},{"base_subobject_id",base_id(base)},
      {"direction",derived?"to-derived":"to-base"},
      {"preserves_null",pointer_form && std::string(source_rep)=="pointer"},
      {"byte_adjustment",base->is_virtual?Value(nullptr):Value(derived?-(long long)base->offset:(long long)base->offset)},
      {"source_representation",source_rep},{"result_representation",result_rep}});
  }
  Id expression(an_expr_node_ptr e) {
    if(!e)return 0;
    auto found=expressions.find(e);if(found!=expressions.end())return found->second;
    const char *kind=nullptr,*op=nullptr;std::string text;Id symbol=0,field=0;
    switch(e->kind) {
      case enk_constant:kind="constant";text=integer(node_constant(e));break;
      case enk_variable:kind="variable";symbol=variable(node_variable(e));break;
      case enk_routine:kind="function";symbol=routine(e->variant.routine.ptr);break;
      case enk_field:kind="field";field=field_id(node_field(e));break;
      case enk_temp_init:kind="temporary";break;
      case enk_object_lifetime:kind="lifetime-scope";break;
      case enk_throw:kind=e->variant.throw_info?"throw":"rethrow";break;
      case enk_operation: {
        if(e->variant.operation.is_virtual_call)missing("virtual call dispatch projection");
        if(e->variant.operation.kind==eok_subscript && e->variant.operation.pointer_operand_is_second)
          missing("reversed subscript operand projection");
        kind="operation";op=operation(e->variant.operation.kind);
        auto first=e->variant.operation.operands;
        if(first && first->kind==enk_routine && (e->variant.operation.kind==eok_call ||
          e->variant.operation.kind==eok_dot_member_call || e->variant.operation.kind==eok_points_to_member_call))
          symbol=routine(first->variant.routine.ptr);
        break;
      }
      default:missing("expression",int(e->kind));
    }
    Id id=node(kind,e->position,type_id(e->type),e->is_lvalue?"lvalue":e->is_xvalue?"xvalue":"prvalue",
      op,text,symbol,field,type_id(e->orig_lvalue_type),e->compiler_generated);
    expressions[e]=id;
    if(e->kind==enk_operation) {
      if(e->variant.operation.kind==eok_base_class_cast || e->variant.operation.kind==eok_derived_class_cast)
        class_conversion(id,e);
      db.update("nodes","id",id,{{"evaluation_order",e->variant.operation.eval_left_to_right?"left-to-right":
        e->variant.operation.eval_right_to_left?"right-to-left":"unspecified"}});
      unsigned ordinal=0;for(auto child=e->variant.operation.operands;child;child=child->next)
        edge(id,"operand",ordinal++,expression(child));
    } else if(e->kind==enk_temp_init) {
      db.update("nodes","id",id,{{"initialization_id",initialization_id(e->variant.init.dynamic_init,e->type)}});
    } else if(e->kind==enk_object_lifetime) {
      attach_lifetime(id,e->variant.object_lifetime.ptr);
      edge(id,"operand",0,expression(e->variant.object_lifetime.expr));
    } else if(e->kind==enk_throw) {
      auto info=e->variant.throw_info;
      db.insert("throws",{{"node_id",id},{"type_id",optional(info?type_id(info->type):0)},
        {"initialization_id",optional(info?initialization_id(info->dynamic_init,info->type):0)},
        {"destructor_symbol_id",optional(info && info->destructor?routine(info->destructor):0)},
        {"is_rethrow",info==nullptr}});
    }
    return id;
  }
  Id dynamic(a_dynamic_init_ptr init,const a_source_position &pos) {
    initialization_id(init);
    if(init->kind==dik_zero && init->variable)
      return node("constant",pos,type_id(init->variable->type),"prvalue",nullptr,"0");
    return initialization_values[init];
  }
  Id declaration(a_variable_ptr v,const a_source_position &pos,bool value=true) {
    Id symbol=variable(v,true),id=node("initialize",pos,type_id(v->type),nullptr,nullptr,"",symbol);
    if(value) {
      if(v->init_kind==initk_dynamic) {
        db.update("nodes","id",id,{{"initialization_id",initialization_id(v->initializer.dynamic,v->type)}});
        edge(id,"value",0,dynamic(v->initializer.dynamic,pos));
      }
      else if(v->init_kind==initk_static)edge(id,"value",0,constant(v->initializer.constant,pos));
      else if(v->init_kind==initk_zero)edge(id,"value",0,node("constant",pos,type_id(v->type),"prvalue",nullptr,"0"));
      else if(v->init_kind!=initk_none)missing("variable initializer",int(v->init_kind));
    }
    return id;
  }
  Id statement(a_statement_ptr s,a_scope_ptr root_scope=nullptr) {
    if(!s)return 0;
    if(s->kind==stmk_init && declaration_initializers.count(s->variant.dynamic_init))return 0;
    const char *kind=nullptr;
    switch(s->kind) {
      case stmk_block:kind="block";break;case stmk_return:kind="return";break;
      case stmk_expr:kind="expr-stmt";break;case stmk_if:kind="if";break;
      case stmk_while:kind="while";break;case stmk_end_test_while:kind="do-while";break;
      case stmk_for:kind="for";break;case stmk_decl:kind="decl";break;case stmk_init:kind="decl";break;
      case stmk_try_block:kind="try";break;
      case stmk_goto:
        if(s->variant.label.ptr->continue_label)kind="continue";
        else if(s->variant.label.ptr->break_label)kind="break";
        else missing("named goto and label projection");break;
      case stmk_label:
        if(s->variant.label.ptr->continue_label || s->variant.label.ptr->break_label)kind="empty";
        else missing("named label projection");break;
      case stmk_empty:kind="empty";break;
      default:missing("statement",int(s->kind));
    }
    Id id=node(kind,s->position);
    switch(s->kind) {
      case stmk_block: {
        auto scope=root_scope?root_scope:s->variant.block.extra_info->assoc_scope;
        if(scope && (scope->variables || scope->local_static_variable_inits))missing("block static storage projection");
        attach_lifetime(id,scope?scope->lifetime:s->variant.block.extra_info->lifetime);
        if(scope)for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
        unsigned ordinal=0;for(auto child=s->variant.block.statements;child;child=child->next)
          edge(id,"statement",ordinal++,statement(child));break;
      }
      case stmk_return:
        if(s->variant.return_dynamic_init)missing("constructor return initialization");
        edge(id,"value",0,expression(s->expr));break;
      case stmk_expr:edge(id,"value",0,expression(s->expr));break;
      case stmk_if:
        edge(id,"condition",0,expression(s->expr));edge(id,"then",0,statement(s->variant.if_stmt.then_statement));
        edge(id,"else",0,statement(s->variant.if_stmt.else_statement));break;
      case stmk_while:case stmk_end_test_while:
        edge(id,"condition",0,expression(s->expr));edge(id,"body",0,statement(s->variant.loop_statement));break;
      case stmk_for:
        edge(id,"initializer",0,statement(s->variant.for_loop.extra_info->initialization));
        edge(id,"condition",0,expression(s->expr));edge(id,"step",0,expression(s->variant.for_loop.extra_info->increment));
        edge(id,"body",0,statement(s->variant.for_loop.statement));break;
      case stmk_decl: {
        unsigned ordinal=0;
        for(auto item=s->variant.decl.entities;item;item=item->next) {
          if(item->entity.kind==iek_variable) {
            auto v=(a_variable_ptr)item->entity.ptr;
            // EDG retains the declaration and its immediately following
            // executable initialization separately. Project that exact pair
            // as the original source declaration with one initializer.
            if(v->init_kind==initk_dynamic) {
              if(!s->next || s->next->kind!=stmk_init || s->next->variant.dynamic_init!=v->initializer.dynamic)
                missing("nonadjacent declaration/initialization projection");
              declaration_initializers.insert(v->initializer.dynamic);
            }
            edge(id,"declaration",ordinal++,declaration(v,s->position));
          }
          else if(item->entity.kind==iek_type)type_id((a_type_ptr)item->entity.ptr);
          else missing("local declaration",int(item->entity.kind));
        }break;
      }
      case stmk_init: {
        auto init=s->variant.dynamic_init;
        if(!init->variable)missing("initializer without destination variable");
        Id d=declaration(init->variable,s->position,false);
        db.update("nodes","id",d,{{"initialization_id",initialization_id(init,init->variable->type)}});
        edge(d,"value",0,dynamic(init,s->position));edge(id,"declaration",0,d);break;
      }
      case stmk_try_block: {
        auto info=s->variant.try_block;
        if(info->is_function_try_block)db.update("nodes","id",id,{{"operator","function-try"}});
        attach_lifetime(id,info->lifetime);
        edge(id,"body",0,statement(info->statement));
        unsigned ordinal=0;
        for(auto handler=info->handlers;handler;handler=handler->next,++ordinal) {
          Id caught=node("catch",handler->catch_position);
          Id parameter=handler->parameter?variable(handler->parameter,true):0;
          Id type=handler->parameter?type_id(handler->parameter->type):0;
          Id init=initialization_id(handler->dynamic_init,handler->parameter?handler->parameter->type:nullptr);
          db.insert("try_handlers",{{"try_node_id",id},{"ordinal",ordinal},{"catch_node_id",caught},
            {"parameter_symbol_id",optional(parameter)},{"parameter_type_id",optional(type)},{"initialization_id",optional(init)}});
          edge(caught,"body",0,statement(handler->statement));edge(id,"handler",ordinal,caught);
        }
        break;
      }
      default:break;
    }
    return id;
  }
  void local_bindings(a_scope_ptr scope) {
    if(!scope)return;
    if(scope->variables || scope->local_static_variable_inits)missing("local static storage projection");
    for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
    for(auto child=scope->scopes;child;child=child->next)local_bindings(child);
  }
  void constructor_initializers(a_scope_ptr scope) {
    unsigned ordinal=0;
    for(auto entry=scope->variant.routine.constructor_inits;entry;entry=entry->next,++ordinal) {
      const char *kind=nullptr;Id field=0,base=0;bool is_virtual=false;a_type_ptr target=nullptr;
      switch(entry->kind) {
        case cik_field:
          kind="field";field=field_id(entry->variant.field);target=entry->variant.field->type;break;
        case cik_direct_base_class:case cik_virtual_base_class:
          kind=entry->kind==cik_direct_base_class?"direct-base":"virtual-base";
          is_virtual=entry->kind==cik_virtual_base_class;target=entry->variant.base_class->type;
          base=base_id(entry->variant.base_class);break;
        case cik_delegation:
          kind="delegation";target=skip_typerefs(scope->variant.routine.ptr->type)->variant.routine.extra_info->this_class;break;
        default:missing("constructor initializer target",int(entry->kind));
      }
      if(!entry->initializer)missing("default field initializer selection");
      if(entry->source_expr)missing("explicit array constructor initializer source");
      db.insert("constructor_initializers",{{"function_id",function},{"ordinal",ordinal},{"kind",kind},
        {"field_id",optional(field)},{"base_subobject_id",optional(base)},
        {"target_type_id",optional(type_id(target))},{"is_virtual",is_virtual},
        {"initialization_id",initialization_id(entry->initializer,target)}});
    }
  }
  void capture(a_scope_ptr scope) {
    if(!scope)return;
    if(scope->kind==sck_function) {
      auto r=scope->variant.routine.ptr;
      if(r->is_prototype_instantiation || !scope->assoc_block || !bodies.insert(r).second)return;
      if(scope->variables || scope->local_static_variable_inits)missing("function static storage projection");
      Id previous=function;function=routine(r);expressions.clear();
      db.update("symbols","id",function,{{"is_definition",true}});
      unsigned ordinal=0;Id receiver=0;
      // Before lowering, EDG keeps the implicit receiver separate from the
      // explicit parameters. lower_il::lower_routine_scope prepends this same
      // variable later; record the selected receiver directly here.
      if(auto p=scope->variant.routine.this_param_variable) {
        receiver=variable(p,true,true);
        db.update("symbols","id",receiver,{{"name","this"}});
        db.insert("parameters",{{"function_id",function},{"ordinal",ordinal++},{"symbol_id",receiver}});
      }
      for(auto p=scope->variant.routine.parameters;p;p=p->next) {
        if(p==scope->variant.routine.this_param_variable)continue;
        Id symbol=variable(p,true,true);
        db.insert("parameters",{{"function_id",function},{"ordinal",ordinal++},{"symbol_id",symbol}});
      }
      local_bindings(scope);
      constructor_initializers(scope);
      lifetime_id(scope->lifetime);
      Id body=statement(scope->assoc_block,scope);
      const char *policy=exception_policy(r->type);
      if(std::string(policy)=="dependent")missing("finalized function exception policy");
      db.update("types","id",routine_source_types[r],{{"exception_policy",policy}});
      const char *kind=r->special_kind==sfk_constructor?"constructor":r->special_kind==sfk_destructor?"destructor":"ordinary";
      db.insert("functions",{{"symbol_id",function},{"body_node_id",body},{"receiver_symbol_id",optional(receiver)},
        {"kind",kind},{"exception_policy",policy}});
      for(auto variable:local_variables)variables.erase(variable);
      local_variables.clear();expressions.clear();declaration_initializers.clear();
      lifetimes.clear();initializations.clear();initialization_values.clear();initialization_types.clear();initializing.clear();
      function=previous;return;
    }
    if(!declaration_scopes.insert(scope).second)return;
    // File and namespace initializers have an explicit EDG execution list.
    // Until startup actions are represented, fail even when the initialized
    // variable is never referenced by an executable body.
    if(scope->dynamic_inits)missing("file/namespace dynamic initialization schedule");
    if(scope->asm_entries)missing("file-scope assembly declaration");
    for(auto v=scope->variables;v;v=v->next) {
#if DO_IL_LOWERING
      // Earlier function lowering can append ABI-only RTTI/vtable storage to
      // this later file-scope list. Such storage is not a source declaration;
      // selected source type/throw operations already own the relevant facts.
      if(v->lowering_generated)continue;
#endif
      if(v->is_nonreal)missing("dependent global declaration");
      variable(v);
    }
    for(auto r=scope->routines;r;r=r->next)
      if(!r->is_prototype_instantiation && r->source_corresp.decl_position.seq)routine(r);
    for(auto t=scope->types;t;t=t->next) {
      if(!t->source_corresp.decl_position.seq)continue;
      if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
        if(t->variant.class_struct_union.is_nonreal_class)missing("dependent record declaration");
        type_id(t);capture(class_type_supp(t)->assoc_scope);
      } else type_id(t);
    }
    for(auto child=scope->scopes;child;child=child->next)capture(child);
    for(auto ns=scope->namespaces;ns;ns=ns->next)if(!ns->is_namespace_alias)capture(ns->variant.assoc_scope);
  }
  void finish() {
    for(const auto &entry:routines)db.update("symbols","id",entry.second,{{"link_name",entry.first->source_corresp.name?entry.first->source_corresp.name:""}});
    for(const auto &entry:types) {
      auto t=entry.first;if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union)
        db.update("types","id",entry.second,{{"link_name",t->source_corresp.name?t->source_corresp.name:""}});
    }
    const auto source=il_header.primary_source_file;
    const char *target=std::getenv("EDG_DOT_TARGET");
    if(!target || !*target)missing("explicit target ABI profile");
    db.insert("object_info",{{"singleton",1},{"format","relational-program-v1"},
      {"primary_source",source?(source->full_name?source->full_name:source->file_name):""},{"target_abi",target}});
    db.finish();
  }
};
inline State &state() { static State value;return value; }
inline void capture_scope(a_scope_ptr scope) {
  if(!enabled())return;
  state().capture(scope);
}
inline void emit() { state().finish(); }
}
END_EDG_NAMESPACE
#endif
