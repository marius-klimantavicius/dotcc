#ifndef DOT_GEN_SEMANTIC_H
#define DOT_GEN_SEMANTIC_H
// A source-shaped, resolved program projection. Only owned program entities
// are recorded; this is not a serializer for arbitrary EDG internal entries.
#include <cstdlib>
#include <cstdio>
#include <map>
#include <set>
#include <string>
#include <vector>
#include "fe_common.h"
#include "il.h"
#include "types.h"
#include "const_ints.h"
#include "exprutil.h"
#include "expr.h"
#include "float_pt.h"
#include "target.h"
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
  Id next_lifetime=1,next_initialization=1,next_base=1,next_enum_value=1,next_formal=1,next_floating_value=1;
  Id function=0;
  a_type_ptr return_type=nullptr;
  std::map<a_type_ptr,Id> types;
  std::set<a_type_ptr> capturing_types;
  std::set<a_type_ptr> incomplete_records,record_template_instances;
  std::map<a_routine_ptr,Id> routines;
  std::map<a_routine_ptr,Id> routine_source_types;
  std::map<a_routine_ptr,Id> routine_receiver_types;
  Id next_virtual_table=1;
  std::map<std::pair<a_variable_ptr,long long>,Id> virtual_tables;
  std::vector<Id> virtual_table_stack;
  struct VirtualEntry {
    Id selected=0,signature=0,receiver=0,target=0;
    a_routine_ptr abi_routine=nullptr;
    const char *kind="ordinary",*variant="ordinary";
    long long delta=0,vcall=0,return_delta=0,vbase=0;
  };
  std::map<std::pair<Id,long long>,VirtualEntry> virtual_entries;
  std::vector<std::pair<Id,std::string>> vptr_phases;
  std::map<Id,unsigned> vptr_ordinals;
  std::map<a_variable_ptr,Id> variables;
  std::map<a_variable_ptr,Id> static_local_symbols;
  std::set<a_variable_ptr> promoted_static_locals;
  std::set<a_variable_ptr> incomplete_globals;
  std::map<a_field_ptr,Id> fields;
  std::map<a_base_class_ptr,Id> base_subobjects;
  std::map<a_constant_ptr,Id> enum_values;
  std::map<std::pair<Id,std::string>,Id> floating_values;
  Id next_literal=1;
  // Keep source constant identities until their owning function finishes
  // lowering. Already-promoted constexpr literals use stable assoc_var
  // identities, including when reached from namespace initializers.
  a_memory_region_number literal_source_region=NULL_region_number;
  std::map<a_memory_region_number,std::map<a_constant_ptr,Id>> literal_constants;
  struct SelectedLiteral {
    Id object=0,symbol=0;
    a_type_ptr type=nullptr;
    a_const_char *bytes=nullptr;
    a_targ_size_t byte_count=0;
    a_source_position position{};
  };
  std::map<a_variable_ptr,SelectedLiteral> selected_literals;
  // Static typeid constants retain their selected C++ type before lowering.
  // Stock RTTI selection supplies the persistent ABI variable independently
  // of copied constants and reclaimed function memory regions.
  struct SelectedTypeInfo {
    Id symbol=0,reference=0;
    a_variable_ptr variable=nullptr;
  };
  struct TypeInfoVariable { Id symbol=0;bool row_exists=false,emitted=false; };
  std::map<a_type_ptr,SelectedTypeInfo> selected_typeinfos;
  std::map<a_variable_ptr,TypeInfoVariable> typeinfo_variables;
  struct Allocation {
    Id node=0,owner=0,allocated_type=0,allocator=0,count_value=0,count_symbol=0,size=0,check=0,failure=0,zero_initialization=0,effective_count_value=0;
    a_memory_region_number region=NULL_region_number;
    an_expr_node_ptr source_count=nullptr;
    a_variable_ptr count_temporary=nullptr;
    a_variable_ptr effective_count_temporary=nullptr;
    a_source_position position{};
    std::map<an_expr_node_ptr,Id> projected;
    bool placement=false,zeroing_observed=false,construction_null_check=false,construction_null_check_observed=false,complete=false;
  };
  std::map<a_new_delete_supplement_ptr,Allocation> allocations;
  std::vector<a_new_delete_supplement_ptr> allocation_stack;
  struct Deallocation {
    Id node=0,owner=0,pointer_value=0,execution=0,destruction=0;
    a_memory_region_number region=NULL_region_number;
    an_expr_node_ptr source_pointer=nullptr;
    a_dynamic_init_ptr source_destruction=nullptr;
    a_source_position position{};
    std::map<an_expr_node_ptr,Id> projected;
    bool destruction_observed=false;
  };
  std::map<a_new_delete_supplement_ptr,Deallocation> deallocations;
  std::vector<a_new_delete_supplement_ptr> deallocation_stack;
  std::map<a_template_ptr,Id> templates;
  std::map<std::string,Id> files;
  std::map<an_expr_node_ptr,Id> expressions;
  std::map<a_statement_ptr,Id> source_statement_nodes;
  std::map<an_expr_node_ptr,Id> recipe_expressions;
  std::map<a_constant_ptr,Id> recipe_constants;
  bool template_recipe=false;
  unsigned recipe_capture_depth=0;
  std::set<a_routine_ptr> bodies;
  std::set<a_variable_ptr> local_variables;
  std::set<a_dynamic_init_ptr> declaration_initializers;
  std::set<a_dynamic_init_ptr> subobject_initializations;
  std::set<a_scope_ptr> declaration_scopes;
  std::map<an_object_lifetime_ptr,Id> lifetimes,static_lifetimes;
  std::map<a_dynamic_init_ptr,Id> initializations,initialization_values;
  std::map<a_dynamic_init_ptr,Id> initialization_types;
  std::set<a_dynamic_init_ptr> initializing;
  struct DefaultMemberInitializers {
    Id owner=0,body=0;
    an_object_lifetime_ptr original_root=nullptr;
    std::map<a_constructor_init_ptr,Id> selected;
    std::map<a_dynamic_init_ptr,Id> init_ids;
    std::map<an_object_lifetime_ptr,Id> lifetime_ids;
  };
  std::map<a_scope_ptr,DefaultMemberInitializers> default_member_initializers;
  std::set<an_object_lifetime_ptr> deferred_destructions;
  State():db(std::getenv("EDG_DOT_SEMANTIC")) {}
  [[noreturn]] void missing(const std::string &what,int kind=-1) {
    std::string detail=what;
    unsigned array_contexts=0;
    for(auto type:capturing_types)if(type->kind==tk_array &&
      (type->variant.array.is_template_dependent_size_array || type->variant.array.is_variable_size_array ||
       type->variant.array.constant_bound_expr_in_local_expr_node_ref ||
       type->variant.array.dep_constant_bound_expr_in_local_expr_node_ref) && array_contexts++<3)
      detail+="\n  "+array_bound_description(type);
    std::fprintf(stderr,"semantic exporter missing %s%s\n",detail.c_str(),
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
    // EDG canonicalizes template declaration identities this same way in
    // il::canonicalize_tagged_ptr; do not allocate one entity per redeclaration.
    if(t->canonical_template)t=t->canonical_template;
    auto found=templates.find(t);if(found!=templates.end())return found->second;
    Id id=next_template++;templates[t]=id;auto pos=location(t->source_corresp.decl_position);
    const char *kind=nullptr;
    switch(t->kind) {
      case templk_class:kind="class";break;
      case templk_function:kind="function";break;
      case templk_variable:kind="variable";break;
      case templk_member_function:kind="member-function";break;
      case templk_static_data_member:kind="static-data-member";break;
      case templk_member_class:kind="member-class";break;
      case templk_member_enum:kind="member-enum";break;
      case templk_template_template_param:kind="template-parameter";break;
      case templk_concept:kind="concept";break;
      default:missing("template declaration kind",int(t->kind));
    }
    bool parameter=t->kind==templk_template_template_param;
    db.insert("templates",{{"id",id},{"kind",kind},{"name",spelling(t->source_corresp)},
      {"parameter_depth",parameter?Value(t->coordinates.depth):Value(nullptr)},
      {"parameter_position",parameter?Value(t->coordinates.position):Value(nullptr)},
      {"is_parameter_pack",bool(t->is_pack)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    unsigned ordinal=0;
    for(auto formal=t->template_decl?t->template_decl->param_list:nullptr;formal;formal=formal->next,++ordinal) {
      const char *formal_kind=nullptr;Id formal_type=0,formal_template=0,default_type=0,default_template=0,default_value_type=0,default_float=0,default_expression=0;
      Value depth=nullptr,position=nullptr,default_value=nullptr;
      switch(formal->kind) {
        case tpk_type: {
          formal_kind="type";auto parameter_type=formal->variant.type.ptr;
          formal_type=type_id(parameter_type);default_type=type_id(formal->variant.type.default_arg_type);
          auto plain=skip_typerefs(parameter_type);
          if(plain->kind==tk_template_param) {
            depth=plain->variant.template_param.extra_info->coordinates.depth;
            position=plain->variant.template_param.extra_info->coordinates.position;
          }
          break;
        }
        case tpk_nontype: {
          formal_kind="constant";auto parameter_value=formal->variant.nontype.constant;
          formal_type=type_id(parameter_value->type);
          if(parameter_value->kind!=ck_template_param || parameter_value->variant.template_param.kind!=tpck_param)
            missing("nontype template formal reference");
          depth=parameter_value->variant.template_param.variant.coordinates.depth;
          position=parameter_value->variant.template_param.variant.coordinates.position;
          if(auto initial=formal->variant.nontype.default_arg_constant) {
            default_value_type=type_id(initial->type);
            if(initial->kind==ck_template_param)
              default_expression=recipe_value(initial,formal->source_corresp.decl_position);
            else default_value=literal_text(initial,"template default "+spelling(t->source_corresp)+
              "::"+spelling(formal->source_corresp),&formal->source_corresp.decl_position);
            if(initial->kind==ck_float)default_float=floating_value(initial);
          }
          break;
        }
        case tpk_template: {
          formal_kind="template";auto parameter_template=formal->variant.templ.class_template;
          formal_template=template_id(parameter_template);
          depth=parameter_template->coordinates.depth;position=parameter_template->coordinates.position;
          default_template=template_id(formal->variant.templ.default_arg_template);break;
        }
        default:missing("template formal",int(formal->kind));
      }
      auto formal_pos=location(formal->source_corresp.decl_position);
      db.insert("template_formals",{{"id",next_formal++},{"template_id",id},{"ordinal",ordinal},
        {"kind",formal_kind},{"name",spelling(formal->source_corresp)},
        {"type_id",optional(formal_type)},{"parameter_template_id",optional(formal_template)},
        {"parameter_depth",depth},{"parameter_position",position},{"is_pack",bool(formal->is_pack)},
        {"default_type_id",optional(default_type)},{"default_template_id",optional(default_template)},
        {"default_value_type_id",optional(default_value_type)},{"default_value_text",default_value},
        {"default_floating_value_id",optional(default_float)},
        {"default_expression_id",optional(default_expression)},
        {"file_id",optional(formal_pos.file)},{"line",formal_pos.line},{"column",formal_pos.column}});
    }
    return id;
  }
  std::string integer(a_constant_ptr c) {
    if(!c || c->kind!=ck_integer)missing("noninteger constant value",c?int(c->kind):-1);
    // EDG formats the selected signedness and target width directly, without
    // narrowing through a host integer (including 128-bit literal values).
    return decimal_str_for_integer_constant(c).as_temp_characters();
  }
  std::string literal_text(a_constant_ptr c,const std::string &context="literal",
                           const a_source_position *position=nullptr) {
    if(c && c->kind==ck_float) {
      auto t=skip_typerefs(c->type);
      // Display only; the exact value is the separate literal bit pattern.
      return fp_to_string(t->variant.float_kind,&c->variant.float_value,nullptr,nullptr,nullptr).as_temp_characters();
    }
    if(c && c->kind!=ck_integer) {
      std::string detail="noninteger constant in "+context+" [name="+spelling(c->source_corresp);
      if(c->kind==ck_template_param)
        detail+=", template-kind="+std::to_string(int(c->variant.template_param.kind));
      if(c->type)detail+=", type="+spelling(c->type->source_corresp)+
        ", type-kind="+std::to_string(int(c->type->kind));
      auto pos=location(position?*position:c->source_corresp.decl_position);
      std::string path;for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
      detail+=", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
      missing(detail,int(c->kind));
    }
    return integer(c);
  }
  Id floating_value(a_constant_ptr c) {
#if IA64_ABI
    auto t=skip_typerefs(c->type);Id value_type=type_id(c->type);
    // Stock float_pt::fp_to_hex_string emits high-order bytes first, retaining
    // negative zero and NaN payloads. No host-number conversion is involved.
    std::string bits=fp_to_hex_string(t->variant.float_kind,&c->variant.float_value).as_temp_characters();
    auto precision=num_mantissa_bits[int(t->variant.float_kind)];
    int minimum=min_exponent[int(t->variant.float_kind)];
    int maximum=max_exponent[int(t->variant.float_kind)];
    unsigned digits=0;
    if(precision==11 && minimum==-13 && maximum==16)digits=4;
    else if(precision==8 && minimum==-125 && maximum==128)digits=4;
    else if(precision==24 && minimum==-125 && maximum==128)digits=8;
    else if(precision==53 && minimum==-1021 && maximum==1024)digits=16;
    else if(precision==64 && minimum==-16381 && maximum==16384) {
      digits=20;
      // The stock formatter prefixes zero long-double storage padding.
      // Strip only that checked padding, never any significand/exponent bits.
      if(bits.size()>digits && bits.substr(0,bits.size()-digits).find_first_not_of('0')==std::string::npos)
        bits=bits.substr(bits.size()-digits);
    } else if(precision==113 && minimum==-16381 && maximum==16384)digits=32;
    if(!digits || bits.size()!=digits || bits.find_first_not_of("0123456789abcdef")!=std::string::npos)
      missing("exact floating literal encoding for "+std::string(floating_name(t->variant.float_kind)));
    auto key=std::make_pair(value_type,bits);auto found=floating_values.find(key);
    if(found!=floating_values.end())return found->second;
    Id id=next_floating_value++;floating_values[key]=id;
    db.insert("floating_values",{{"id",id},{"type_id",value_type},{"bits_hex",bits}});
    return id;
#else
    (void)c;
    missing("exact floating literal encoding for target ABI");
#endif
  }
  void instance(a_template_ptr t,a_template_arg_ptr args,Id symbol,Id type,bool dependent) {
    if(!t)return;
    Id id=next_instance++;
    db.insert("template_instances",{{"id",id},{"template_id",template_id(t)},
      {"symbol_id",optional(symbol)},{"type_id",optional(type)},{"is_dependent",dependent}});
    unsigned ordinal=0;int pack_ordinal=-1;
    for(auto arg=args;arg;arg=arg->next,++ordinal) {
      if(arg->kind==tak_start_of_pack_expansion) {
        // As in lower_name::mangled_template_arguments_or_parameter_pack,
        // the following is_pack_element entries belong to this pack. An
        // empty pack still has its own semantic argument container.
        db.insert("template_arguments",{{"instance_id",id},{"ordinal",ordinal},{"kind","pack"}});
        pack_ordinal=int(ordinal);continue;
      }
      if(!arg->is_pack_element && !arg->is_pack)pack_ordinal=-1;
      const char *kind=nullptr;Id argument_type=0,templ=0,floating=0,argument_expression=0;Value value=nullptr;
      switch(arg->kind) {
        case tak_type:
          kind="type";argument_type=type_id(arg->variant.type);break;
        case tak_nontype:
          kind="constant";
          if(arg->is_array_bound_of_unknown_type || !arg->variant.constant)
            missing("unresolved nontype template argument");
          argument_type=type_id(arg->variant.constant->type);
          if(arg->variant.constant->kind==ck_template_param)
            argument_expression=recipe_value(arg->variant.constant,arg->variant.constant->source_corresp.decl_position);
          else value=literal_text(arg->variant.constant,"template argument "+spelling(t->source_corresp)+
            " ordinal "+std::to_string(ordinal));
          if(arg->variant.constant->kind==ck_float)floating=floating_value(arg->variant.constant);break;
        case tak_template:
          kind="template";templ=template_id(arg->variant.templ.ptr);
          if(!templ)missing("selected template template argument");break;
        default:missing("template argument",int(arg->kind));
      }
      db.insert("template_arguments",{{"instance_id",id},{"ordinal",ordinal},{"kind",kind},
        {"parent_ordinal",pack_ordinal<0?Value(nullptr):Value(pack_ordinal)},
        {"is_pack_expansion",bool(arg->pack_expansion_descr || arg->is_pack || arg->has_pack_ellipsis)},
        {"type_id",optional(argument_type)},{"template_id",optional(templ)},{"constant_text",value},
        {"floating_value_id",optional(floating)},{"expression_id",optional(argument_expression)}});
    }
  }
  const char *exception_policy(a_type_ptr type) {
    auto t=skip_typerefs(type);
    if(t->kind!=tk_routine)return "unwind";
    auto spec=t->variant.routine.extra_info->exception_specification;
    if(!spec || spec->throw_any)return "unwind";
    if(spec->indeterminate || spec->arg_cached || spec->copy_from_prototype)return "dependent";
    if(spec->is_noexcept)return "terminate";
    return "dynamic";
  }
  void exception_specification_types(Id signature,a_type_ptr type) {
    auto spec=skip_typerefs(type)->variant.routine.extra_info->exception_specification;
    unsigned ordinal=0;
    for(auto selected=spec->variant.exception_specification_type_list;selected;selected=selected->next)
      db.insert("exception_specification_types",{{"function_type_id",signature},{"ordinal",ordinal++},
        {"allowed_type_id",type_id(selected->type)}});
  }
  const char *floating_name(a_float_kind kind) {
    // Preserve language type identity even when two kinds share a format.
    switch(kind) {
      case fk_float16:return "_Float16";
      case fk_fp16:return "__fp16";
      case fk_float:return "float";
      case fk_float32x:return "_Float32x";
      case fk_double:return "double";
      case fk_float64x:return "_Float64x";
      case fk_long_double:return "long double";
      case fk_float80:return "__float80";
      case fk_float128:return "__float128";
      case fk_std_bfloat16:return "std::bfloat16_t";
      case fk_std_float16:return "std::float16_t";
      case fk_std_float32:return "std::float32_t";
      case fk_std_float64:return "std::float64_t";
      case fk_std_float128:return "std::float128_t";
      default:missing("floating language type",int(kind));
    }
  }
  std::string array_bound_description(a_type_ptr type) {
    auto &array=type->variant.array;
    auto selected=array.is_template_dependent_size_array?array.variant.element_count_constant:nullptr;
    auto expr=array.is_variable_size_array && !array.is_vla?array.variant.element_count_expr:nullptr;
    auto position=type->source_corresp.decl_position;
    if(!position.seq && selected)position=selected->source_corresp.decl_position;
    if(!position.seq && expr)position=expr->position;
    auto pos=location(position);std::string path,owner;
    for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
    if(type->source_corresp.enclosing_routine)owner=spelling(type->source_corresp.enclosing_routine->source_corresp);
    else for(const auto &entry:routines)if(entry.second==function){owner=spelling(entry.first->source_corresp);break;}
    return "array bound [type="+spelling(type->source_corresp)+", function="+owner+
      ", recipe="+std::to_string(template_recipe?1:0)+
      ", dependent="+std::to_string(int(array.is_template_dependent_size_array))+
      ", runtime="+std::to_string(int(array.is_variable_size_array))+
      ", vla="+std::to_string(int(array.is_vla))+
      ", vla-dimension="+std::to_string(int(array.has_assoc_vla_dimension))+
      ", local-constant="+std::to_string(int(array.constant_bound_expr_in_local_expr_node_ref))+
      ", local-dependent="+std::to_string(int(array.dep_constant_bound_expr_in_local_expr_node_ref))+
      ", constant-kind="+std::to_string(selected?int(selected->kind):-1)+
      ", dependent-kind="+std::to_string(selected && selected->kind==ck_template_param?int(selected->variant.template_param.kind):-1)+
      ", expression-kind="+std::to_string(expr?int(expr->kind):-1)+
      ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
  }
  Id type_id(a_type_ptr t) {
    if(!t)return 0;
    auto found=types.find(t);
    if(found!=types.end()) {
      // A declaration can first be reached through a pointer before EDG
      // sees its definition. Complete that same source entity once, before
      // lowering can add ABI fields; never reinterpret an already-captured
      // complete record from a later lowered field list.
      if(incomplete_records.count(t) && !is_incomplete_type(t) && !capturing_types.count(t)) {
        incomplete_records.erase(t);capturing_types.insert(t);
        db.update("types","id",found->second,{{"size",t->size},{"alignment",t->alignment},
          {"is_incomplete",false},{"is_dependent",bool(is_template_dependent_type(t))},
          {"name",spelling(t->source_corresp)},
          {"record_kind",t->kind==tk_struct?"struct":t->kind==tk_class?"class":"union"}});
        record_definition(t,found->second);
        capturing_types.erase(t);
      }
      return found->second;
    }
    Id id=next_type++;types[t]=id;capturing_types.insert(t);
    std::string kind,name=spelling(t->source_corresp),record_kind,builtin,float_format;
    Id underlying=0,pointee=0,element=0,result=0,object=0;
    Value count=nullptr;bool variadic=false,incomplete=is_incomplete_type(t);
    unsigned qualifiers=get_type_qualifiers(t);
    if(qualifiers & ~(TQ_CONST|TQ_VOLATILE|TQ_RESTRICT))missing("extended type qualifiers");
    switch(t->kind) {
      case tk_void:kind="void";builtin="void";break;
      case tk_nullptr:
        if(!is_standard_nullptr_type(t))missing("C++/CLI nullptr type");
        // Keep the distinct source type. lower_il::lower_type uses void*
        // storage, while nullptr prvalues denote zero even when their source
        // storage is uninitialized (rewrite_nullptr_expr_if_necessary).
        kind="nullptr";builtin="std::nullptr_t";break;
      case tk_template_param:kind="template-parameter";break;
      case tk_typeref:
        kind=type_is_typedef(t)?"typedef":"qualified";underlying=type_id(t->variant.typeref.type);break;
      case tk_integer:
        if(t->variant.integer.enum_type) {
          kind="enum";
          auto extra=integer_type_supp(t);
          // Use the same selected underlying type as EDG's
          // types::apply_type_transforming_intrinsic(trk_is_underlying_type).
          // GNU's underlying kind can differ from the promotion kind.
          if(t->variant.integer.has_explicit_enum_base)underlying=type_id(extra->base_type);
#if GNU_EXTENSIONS_ALLOWED
          else if(extra->underlying_type_should_use_unsigned)
            underlying=type_id(integer_type(unsigned_int_kind_of[t->variant.integer.int_kind]));
#endif
          else underlying=type_id(integer_type(t->variant.integer.int_kind));
          break;
        }
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
#if INT128_EXTENSIONS_ALLOWED
          case ik_int128:kind="i128";builtin="__int128";break;
          case ik_unsigned_int128:kind="u128";builtin="unsigned __int128";break;
#endif
          default:missing("integer type",int(t->variant.integer.int_kind));
        }
        if(t->variant.integer.wchar_t_type)builtin="wchar_t";
        else if(t->variant.integer.char8_t_type)builtin="char8_t";
        else if(t->variant.integer.char16_t_type)builtin="char16_t";
        else if(t->variant.integer.char32_t_type)builtin="char32_t";
        break;
      case tk_float: {
        builtin=floating_name(t->variant.float_kind);
        // These target properties are initialized for every EDG float kind
        // by exprutil_init. Storage size alone cannot distinguish binary128
        // from a padded x87 value, or long double from double on all targets.
        auto precision=num_mantissa_bits[int(t->variant.float_kind)];
        int minimum=min_exponent[int(t->variant.float_kind)];
        int maximum=max_exponent[int(t->variant.float_kind)];
        if(precision==11 && minimum==-13 && maximum==16){kind="f16";float_format="binary16";}
        else if(precision==8 && minimum==-125 && maximum==128){kind="bf16";float_format="bfloat16";}
        else if(precision==24 && minimum==-125 && maximum==128){kind="f32";float_format="binary32";}
        else if(precision==53 && minimum==-1021 && maximum==1024){kind="f64";float_format="binary64";}
        else if(precision==64 && minimum==-16381 && maximum==16384){kind="f80";float_format="x87-binary80";}
        else if(precision==113 && minimum==-16381 && maximum==16384){kind="f128";float_format="binary128";}
        else {kind="floating";float_format="target-binary";}
        break;
      }
      case tk_pointer:
        kind=t->variant.pointer.is_reference?(t->variant.pointer.is_rvalue_reference?"rvalue-reference":"lvalue-reference"):"pointer";
        pointee=type_id(t->variant.pointer.type);break;
      case tk_ptr_to_member:
#if MICROSOFT_EXTENSIONS_ALLOWED
        if(t->variant.ptr_to_member.modifiers)missing("member-pointer extended pointer modifiers");
#endif
        kind="member-pointer";
        object=type_id(t->variant.ptr_to_member.orig_class_of_which_a_member?
          t->variant.ptr_to_member.orig_class_of_which_a_member:t->variant.ptr_to_member.class_of_which_a_member);
        pointee=type_id(t->variant.ptr_to_member.type);break;
      case tk_array:
        if(t->variant.array.is_vla)missing("VLA type requires selected dimension correspondence");
        kind="array";element=type_id(t->variant.array.element_type);
        if(!t->variant.array.is_variable_size_array && !t->variant.array.is_template_dependent_size_array &&
           (t->variant.array.variant.number_of_elements || t->variant.array.bound_is_zero))
          count=Value(t->variant.array.variant.number_of_elements);
        break;
      case tk_routine:
        kind="function";result=type_id(t->variant.routine.return_type);
        variadic=t->variant.routine.extra_info->has_ellipsis;
        object=type_id(t->variant.routine.extra_info->this_class);break;
      case tk_struct:case tk_class:case tk_union:
        kind="record";record_kind=t->kind==tk_struct?"struct":t->kind==tk_class?"class":"union";
        break;
      default: {
        auto pos=location(t->source_corresp.decl_position);std::string path,owner;
        for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        for(const auto &entry:routines)if(entry.second==function){owner=spelling(entry.first->source_corresp);break;}
        std::string detail="type [name="+name+", function="+owner+
          ", recipe="+std::to_string(template_recipe?1:0)+", file="+path+
          ", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
        unsigned container_count=0;
        for(auto container:capturing_types)if(container!=t && container_count++<3) {
          auto source=location(container->source_corresp.decl_position);
          detail+=" [capturing-type="+spelling(container->source_corresp)+
            ", kind="+std::to_string(int(container->kind))+
            ", file-id="+std::to_string(source.file)+", line="+std::to_string(source.line)+
            ", column="+std::to_string(source.column)+"]";
        }
        missing(detail,int(t->kind));
      }
    }
    // These are EDG's builtin identities, not names or inferred layouts.
    // Preserve the source typedef/record and its layout; the role selects
    // the portable cursor representation used by the managed calling ABI.
    const char *semantic_role=nullptr;
    if(t->is_builtin_va_list)semantic_role="va-list";
    else if((t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) &&
            class_type_supp(t)->is_va_list_tag)semantic_role="va-list-storage";
    db.insert("types",{{"id",id},{"kind",kind},{"name",name},{"link_name",t->source_corresp.name?t->source_corresp.name:""},
      {"linkage",linkage(t->source_corresp)},{"size",t->size},{"alignment",t->alignment},
      {"underlying_type_id",optional(underlying)},{"pointee_type_id",optional(pointee)},
      {"element_type_id",optional(element)},{"element_count",count},{"return_type_id",optional(result)},
      {"object_type_id",optional(object)},{"is_const",bool(qualifiers&TQ_CONST)},
      {"is_volatile",bool(qualifiers&TQ_VOLATILE)},{"is_restrict",bool(qualifiers&TQ_RESTRICT)},
      {"is_variadic",variadic},{"is_incomplete",incomplete},{"record_kind",record_kind.empty()?Value(nullptr):Value(record_kind)},
      {"is_dependent",bool(is_template_dependent_type(t))},
      {"builtin_name",builtin.empty()?Value(nullptr):Value(builtin)},
      {"semantic_role",semantic_role?Value(semantic_role):Value(nullptr)}});
    if(t->kind==tk_array) {
      auto &array=t->variant.array;const char *bound_kind=nullptr;Id bound=0;
      if(array.is_template_dependent_size_array) {
        bound_kind="dependent";
        auto selected=array.variant.element_count_constant;
        an_expr_node_ptr local_expr=nullptr;
        if(array.dep_constant_bound_expr_in_local_expr_node_ref) {
          local_expr=find_local_expr_node((char*)t,lerk_dep_array_bound);
          if(!local_expr)missing("dependent array bound local expression correspondence");
          if(!selected || selected->kind!=ck_template_param ||
             (selected->variant.template_param.kind!=tpck_expression && selected->variant.template_param.kind!=tpck_sizeof &&
              selected->variant.template_param.kind!=tpck_datasizeof && selected->variant.template_param.kind!=tpck_alignof &&
              selected->variant.template_param.kind!=tpck_noexcept && selected->variant.template_param.kind!=tpck_typeid &&
              selected->variant.template_param.kind!=tpck_uuidof))
            missing("dependent array bound local constant family");
        }
        if(selected)bound=recipe_value(selected,t->source_corresp.decl_position,local_expr);
      } else if(array.is_variable_size_array) {
        bound_kind="runtime";
        auto selected=array.variant.element_count_expr;
        if(!selected)missing("runtime array bound expression correspondence");
        bound=recipe_expression(selected,selected->position,0);
      } else if(array.constant_bound_expr_in_local_expr_node_ref) {
        auto selected=find_local_expr_node((char*)t,lerk_array_bound);
        if(!selected)missing("constant array bound local expression correspondence");
        bound=recipe_expression(selected,selected->position,0);
      }
      if(bound_kind || bound)db.update("types","id",id,{{"array_bound_kind",bound_kind},{"array_bound_node_id",optional(bound)}});
    }
    if(t->kind==tk_template_param) {
      auto extra=t->variant.template_param.extra_info;
      const char *parameter_kind=nullptr;Id qualifier=0;
      Value depth=nullptr,position=nullptr;
      switch(t->variant.template_param.kind) {
        case tptk_param:
          parameter_kind="parameter";depth=extra->coordinates.depth;position=extra->coordinates.position;break;
        case tptk_member: {
          parameter_kind="member";auto owner=parent_class_or_null(t);
          // A proxy is EDG lookup machinery; its proxy_of_type identifies
          // the actual source qualifier, e.g. T in typename T::member.
          if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
          qualifier=type_id(owner);break;
        }
        case tptk_unknown:parameter_kind="unknown";break;
        default:missing("dependent type form",int(t->variant.template_param.kind));
      }
      db.insert("template_type_parameters",{{"type_id",id},{"kind",parameter_kind},
        {"parameter_depth",depth},{"parameter_position",position},
        {"is_pack",bool(t->variant.template_param.is_pack)},{"qualifier_type_id",optional(qualifier)}});
    }
    if(t->kind==tk_float)db.insert("floating_types",{{"type_id",id},{"format",float_format},
      {"precision_bits",num_mantissa_bits[int(t->variant.float_kind)]},
      {"min_exponent",min_exponent[int(t->variant.float_kind)]},
      {"max_exponent",max_exponent[int(t->variant.float_kind)]}});
    if(t->kind==tk_routine) {
      const char *policy=exception_policy(t);
      db.update("types","id",id,{{"exception_policy",policy},
        {"return_storage",t->variant.routine.extra_info->value_returned_by_cctor?"caller-provided":"value"}});
      if(std::string(policy)=="dynamic")exception_specification_types(id,t);
      unsigned ordinal=0;
      for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)
        db.insert("type_parameters",{{"type_id",id},{"ordinal",ordinal++},{"parameter_type_id",type_id(p->type)}});
    }
    if(kind=="enum") {
      auto extra=integer_type_supp(t);auto pos=location(t->source_corresp.decl_position);
      db.insert("enums",{{"type_id",id},{"is_scoped",bool(t->variant.integer.is_scoped_enum)},
        {"has_explicit_base",bool(t->variant.integer.has_explicit_enum_base)},
        {"declared_base_type_id",optional(type_id(extra->base_type))},
        {"is_definition",bool(extra->enumerator_list_complete)},
        {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
      // enum_constants also handles scoped enum declarations. This is the
      // same ordered list consumed by c_gen_be::dump_enum_definition.
      unsigned ordinal=0;
      for(auto value=enum_constants(t);value;value=value->next,++ordinal) {
        Id value_id=next_enum_value++;enum_values[value]=value_id;
        auto value_pos=location(value->source_corresp.decl_position);
        Value selected_integer=nullptr;Id selected_expression=0;
        if(value->kind==ck_template_param)
          selected_expression=recipe_value(value,value->source_corresp.decl_position);
        else selected_integer=integer(value);
        db.insert("enum_values",{{"id",value_id},{"enum_type_id",id},{"ordinal",ordinal},
          {"name",spelling(value->source_corresp)},{"value_text",selected_integer},
          {"expression_node_id",optional(selected_expression)},
          {"value_type_id",type_id(value->type)},{"file_id",optional(value_pos.file)},
          {"line",value_pos.line},{"column",value_pos.column}});
      }
    }
    if(kind=="record") {
      if(incomplete)incomplete_records.insert(t);
      else record_definition(t,id);
      record_instance(t,id);
    }
    capturing_types.erase(t);return id;
  }
  void record_instance(a_type_ptr t,Id id) {
    auto extra=class_type_supp(t);
    if(extra->assoc_template && record_template_instances.insert(t).second)
      instance(extra->assoc_template,extra->template_arg_list,0,id,is_template_dependent_type(t));
  }
  void record_definition(a_type_ptr t,Id id) {
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
      // Virtual table lowering needs these exact source signatures after
      // routine types have acquired ABI parameters. Capture only the named
      // virtual declarations of this already-selected record.
      if(extra->assoc_scope && !is_template_dependent_type(t))
        for(auto r=extra->assoc_scope->routines;r;r=r->next)
          if(r->is_virtual && !r->is_consteval)routine(r);
      record_instance(t,id);
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
    if(!owner) {
      auto owner_type=parent_class_of(f);
      bool cached=types.count(owner_type)!=0;
      type_id(owner_type);found=fields.find(f);if(found!=fields.end())return found->second;
      int list_ordinal=-1;unsigned list_size=0;bool proxy=false;
      if(is_immediate_class_type(owner_type)) {
        for(auto entry=owner_type->variant.class_struct_union.field_list;entry;entry=entry->next,++list_size)
          if(entry==f)list_ordinal=int(list_size);
        proxy=class_type_supp(owner_type)->proxy_of_type!=nullptr;
      }
      auto pos=location(f->source_corresp.decl_position);std::string path;
      for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
      missing("field owner [field="+spelling(f->source_corresp)+", owner="+spelling(owner_type->source_corresp)+
        ", owner-kind="+std::to_string(int(owner_type->kind))+", cached="+std::to_string(cached)+
        ", capturing="+std::to_string(capturing_types.count(owner_type))+
        ", owner-list-ordinal="+std::to_string(list_ordinal)+", owner-list-size="+std::to_string(list_size)+
        ", proxy="+std::to_string(proxy)+", generated="+std::to_string(bool(f->compiler_generated))+
        ", field-type="+spelling(f->type->source_corresp)+", field-type-kind="+std::to_string(int(f->type->kind))+
        ", recipe="+std::to_string(template_recipe)+", file="+path+
        ", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]");
    }
    Id id=next_field++;fields[f]=id;auto pos=location(f->source_corresp.decl_position);
    db.insert("fields",{{"id",id},{"owner_type_id",owner},{"name",spelling(f->source_corresp)},
      {"type_id",type_id(f->type)},{"ordinal",ordinal},{"byte_offset",f->offset},
      {"bit_offset",f->is_bit_field?Value(f->offset_bit_remainder):Value(nullptr)},
      {"bit_width",f->is_bit_field?Value(f->bit_size):Value(nullptr)},
      {"bit_is_signed",f->is_bit_field?Value(bool(f->bit_field_is_signed)):Value(nullptr)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});return id;
  }
  Id routine(a_routine_ptr r) {
    auto found=routines.find(r);if(found!=routines.end())return found->second;
    Id id=next_symbol++;routines[r]=id;auto pos=location(r->source_corresp.decl_position);
    Id signature=type_id(r->type);routine_source_types[r]=signature;
    auto signature_type=skip_typerefs(r->type);
    routine_receiver_types[r]=type_id(signature_type->variant.routine.extra_info->this_class);
    db.insert("symbols",{{"id",id},{"kind","function"},{"name",spelling(r->source_corresp)},
      {"link_name",r->source_corresp.name?r->source_corresp.name:""},{"linkage",linkage(r->source_corresp)},
      {"type_id",signature},{"is_definition",r->function_def_number!=NULL_function_def_number},
      {"definition_kind",r->is_inline || (r->is_template_function && !r->is_specialized)?"weak":"strong"},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    instance(r->assoc_template,r->template_arg_list,id,0,is_template_dependent_type(r->type));return id;
  }
  void global_initializer(a_variable_ptr v,Id id) {
    if(v->init_kind!=initk_none && v->init_kind!=initk_zero && v->init_kind!=initk_static)
      missing("global dynamic initializer");
    // Declaration processing canonicalizes storage duration (il.h). Its
    // default-initialization path explicitly treats static-duration objects
    // as zero-initialized (decls.c); preserve that action for array storage.
    bool zero_array=v->storage_class!=sc_extern && var_has_static_or_thread_storage_duration(v) &&
      (v->init_kind==initk_none || v->init_kind==initk_zero) && is_array_type(v->type);
    if(v->init_kind==initk_static || zero_array) {
      Id previous=function;bool previous_recipe=template_recipe;function=0;template_recipe=false;
      Id initializer;
      if(zero_array) {
        initializer=node("array-initializer",v->source_corresp.decl_position,type_id(v->type),"prvalue");
        db.insert("array_initializers",{{"node_id",initializer},{"zero_initialize",true}});
      } else initializer=constant(v->initializer.constant,v->source_corresp.decl_position);
      db.insert("global_initializers",{{"symbol_id",id},{"initializer_node_id",initializer}});
      function=previous;template_recipe=previous_recipe;
    }
  }
  Id variable(a_variable_ptr v,bool local=false,bool parameter=false,bool source_definition=false) {
    if(v->is_nonreal) {
      if(!template_recipe)missing_source_variable(v,"dependent variable in executable expression");
      return source_variable(v);
    }
    auto found=variables.find(v);if(found!=variables.end()) {
      // A function can refer to an extern declaration before its definition
      // is seen. Final source-scope capture completes that same EDG entity;
      // already-defined globals are never refreshed from lowered storage.
      if(source_definition && incomplete_globals.count(v) && v->storage_class!=sc_extern) {
        incomplete_globals.erase(v);
        auto pos=location(v->source_corresp.decl_position);
        db.update("symbols","id",found->second,{{"name",spelling(v->source_corresp)},
          {"link_name",v->source_corresp.name?v->source_corresp.name:""},{"linkage",linkage(v->source_corresp)},
          {"type_id",type_id(v->type)},{"is_definition",true},{"file_id",optional(pos.file)},
          {"line",pos.line},{"column",pos.column}});
        global_initializer(v,found->second);
      }
      return found->second;
    }
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
    if(!local) {
      if(v->storage_class==sc_extern)incomplete_globals.insert(v);
      global_initializer(v,id);
    }
    return id;
  }
  Id node(const char *kind,const a_source_position &p,Id type=0,const char *category=nullptr,
          const char *op=nullptr,const std::string &constant="",Id symbol=0,Id field=0,Id original=0,bool synthetic=false) {
    Id id=next_node++;auto pos=location(p);
    db.insert("nodes",{{"id",id},{"function_id",optional(function)},{"kind",kind},{"type_id",optional(type)},
      {"domain",template_recipe?"template-recipe":"program"},
      {"value_category",category},{"operator",op},{"constant_text",constant.empty()?Value(nullptr):Value(constant)},
      {"symbol_id",optional(symbol)},{"field_id",optional(field)},{"original_lvalue_type_id",optional(original)},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column},{"is_synthetic",synthetic}});
    return id;
  }
  void edge(Id parent,const char *role,unsigned ordinal,Id child) {
    if(child)db.insert("edges",{{"parent_id",parent},{"role",role},{"ordinal",ordinal},{"child_id",child}});
  }
  Id recipe_value(a_constant_ptr c,const a_source_position &pos,an_expr_node_ptr local_expression=nullptr) {
    auto previous_function=function;bool previous_recipe=template_recipe;
    if(recipe_capture_depth++==0){recipe_expressions.clear();recipe_constants.clear();}
    function=0;template_recipe=true;
    Id id=constant(c,pos,true,local_expression);
    template_recipe=previous_recipe;function=previous_function;
    if(--recipe_capture_depth==0){recipe_expressions.clear();recipe_constants.clear();}
    return id;
  }
  Id recipe_expression(an_expr_node_ptr e,const a_source_position &pos,Id source_node) {
    auto previous_function=function;bool previous_recipe=template_recipe;
    if(recipe_capture_depth++==0){recipe_expressions.clear();recipe_constants.clear();}
    function=0;template_recipe=true;
    Id id=expression(e,&pos,source_node);
    template_recipe=previous_recipe;function=previous_function;
    if(--recipe_capture_depth==0){recipe_expressions.clear();recipe_constants.clear();}
    return id;
  }
  [[noreturn]] void missing_source_variable(a_variable_ptr v,const char *reason) {
    auto source=location(v->source_corresp.decl_position);std::string path;
    for(const auto &file:files)if(file.second==source.file){path=file.first;break;}
    auto init=v->init_kind==initk_dynamic?v->initializer.dynamic:nullptr;
    missing(std::string(reason)+" [variable="+spelling(v->source_corresp)+
      ", type="+spelling(v->type->source_corresp)+", type-kind="+std::to_string(int(v->type->kind))+
      ", nonreal="+std::to_string(int(v->is_nonreal))+
      ", prototype="+std::to_string(int(v->is_prototype_instantiation))+
      ", init-kind="+std::to_string(int(v->init_kind))+
      ", action-kind="+std::to_string(init?int(init->kind):-1)+
      ", destructor="+std::to_string(init && init->destructor?1:0)+
      ", lifetime="+std::to_string(init && init->lifetime?1:0)+
      ", expression-lifetime="+std::to_string(init && init->init_expr_lifetime?1:0)+
      ", file="+path+", line="+std::to_string(source.line)+
      ", column="+std::to_string(source.column)+"]");
  }
  Id source_variable(a_variable_ptr v) {
    auto found=variables.find(v);if(found!=variables.end())return found->second;
    Id symbol=next_symbol++;variables[v]=symbol;
    auto previous_function=function;bool previous_recipe=template_recipe;
    if(recipe_capture_depth++==0){recipe_expressions.clear();recipe_constants.clear();}
    function=0;template_recipe=true;
    auto pos=location(v->source_corresp.decl_position);Id type=type_id(v->type);
    db.insert("symbols",{{"id",symbol},{"kind","source-variable"},{"name",spelling(v->source_corresp)},
      {"link_name",v->source_corresp.name?v->source_corresp.name:""},{"linkage",linkage(v->source_corresp)},
      {"type_id",type},{"is_definition",v->storage_class!=sc_extern},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    Id declaration=node("variable-declaration",v->source_corresp.decl_position,type,nullptr,nullptr,"",symbol);
    const char *kind=nullptr;Id initializer=0;
    switch(v->init_kind) {
      case initk_none:kind="none";break;
      case initk_zero:kind="zero";break;
      case initk_static:
        kind="constant";initializer=constant(v->initializer.constant,v->source_corresp.decl_position);break;
      case initk_dynamic: {
        auto init=v->initializer.dynamic;
        if(!init || init->destructor || init->lifetime || init->init_expr_lifetime ||
           init->lifetime_of_overlapping_temps || init->master_entry ||
           init->class_rvalue_initialized_through_master_entry || init->unordered ||
           init->overlaps_temps_in_inner_lifetime || init->destruction_is_for_partially_constructed_aggregate ||
           init->is_freeing_of_storage_on_exception)
          missing_source_variable(v,"source variable initializer ownership");
        switch(init->kind) {
          case dik_none:kind="none";break;
          case dik_zero:kind="zero";break;
          case dik_constant:
            kind="constant";initializer=constant(init->variant.constant.ptr,v->source_corresp.decl_position);break;
          case dik_expression:
            kind="expression";initializer=expression(init->variant.expression,&v->source_corresp.decl_position,declaration);break;
          default:missing_source_variable(v,"source variable selected initializer");
        }
        break;
      }
      default:missing_source_variable(v,"source variable initialization");
    }
    db.update("nodes","id",declaration,{{"operator",kind}});
    edge(declaration,"initializer",0,initializer);
    if(v->template_info)instance(v->template_info->assoc_template,v->template_info->template_arg_list,symbol,0,true);
    template_recipe=previous_recipe;function=previous_function;
    if(--recipe_capture_depth==0){recipe_expressions.clear();recipe_constants.clear();}
    return symbol;
  }
  void query_operand(Id id,a_type_ptr selected_type,an_expr_node_ptr selected_expression,const a_source_position &p) {
    Id operand=0;
    if(selected_expression)operand=expression(selected_expression,&p,id);
    else {
      if(!selected_type)missing("source query selected operand correspondence");
      operand=node("type-reference",p,type_id(selected_type));
      db.update("nodes","id",operand,{{"source_node_id",id}});
    }
    edge(id,"operand",0,operand);
  }
  Id recipe_operand(an_expr_node_ptr selected,const a_source_position &pos,Id owner) {
    if(selected->kind!=enk_type_operand &&
       !(selected->kind==enk_routine && !selected->variant.routine.ptr))
      return expression(selected,&pos,owner);
    auto found=recipe_expressions.find(selected);if(found!=recipe_expressions.end())return found->second;
    Id id=0;
    if(selected->kind==enk_type_operand) {
      if(!selected->variant.type_operand.type)missing("source recipe selected type operand");
      id=node("type-reference",selected->position.seq?selected->position:pos,
        type_id(selected->variant.type_operand.type));
      db.update("nodes","id",id,{{"source_node_id",owner}});
    } else id=vacuous_destructor_name(selected,pos,owner);
    recipe_expressions[selected]=id;return id;
  }
  [[noreturn]] void missing_dependent_value(a_constant_ptr c,const a_source_position &p,const char *reason) {
    auto source=location(p);std::string path;
    for(const auto &file:files)if(file.second==source.file){path=file.first;break;}
    auto reference=c->variant.template_param.kind==tpck_template_ref?
      c->variant.template_param.variant.template_ref.con:c;
    if(!reference)reference=c;
    std::string detail=std::string(reason)+" [name="+spelling(reference->source_corresp)+
      ", recipe="+std::to_string(template_recipe?1:0)+", reference-kind="+
      std::to_string(reference->kind==ck_template_param?int(reference->variant.template_param.kind):-1);
    if(c->variant.template_param.kind==tpck_template_ref) {
      unsigned ordinal=0;
      for(auto arg=c->variant.template_param.variant.template_ref.arg_list;arg;arg=arg->next,++ordinal)
        detail+=" [argument="+std::to_string(ordinal)+", kind="+std::to_string(int(arg->kind))+
          ", pack-element="+std::to_string(int(arg->is_pack_element))+
          ", pack="+std::to_string(int(arg->is_pack))+"]";
    }
    missing(detail+", file="+path+", line="+std::to_string(source.line)+
      ", column="+std::to_string(source.column)+"]",int(c->variant.template_param.kind));
  }
  const char *dependent_value_kind(a_constant_ptr c,const a_source_position &p) {
    switch(c->variant.template_param.kind) {
      case tpck_param:return "template-parameter-reference";
      case tpck_member:return "dependent-member-reference";
      case tpck_expression:return "dependent-expression";
      case tpck_dependent_constant:return "dependent-constant";
      case tpck_address:return "dependent-address";
      case tpck_template_ref:return "dependent-template-reference";
      case tpck_destructor:return "dependent-destructor-reference";
      case tpck_sizeof:case tpck_datasizeof:case tpck_alignof:
      case tpck_noexcept:case tpck_typeid:case tpck_uuidof:return "operation";
      case tpck_unknown_function:return c->variant.template_param.is_qualified_name?
        "dependent-qualified-function-reference":"dependent-function-reference";
      default:missing_dependent_value(c,p,"dependent value");
    }
  }
  std::string namespace_reference_name(a_constant_ptr c) {
    std::string name=spelling(c->source_corresp);
    // The source correspondence retains the namespace selected for this
    // dependent name. The front-end overload-set symbol is never inspected.
    for(auto ns=parent_namespace_or_null(c);ns;ns=parent_namespace_or_null(ns)) {
      auto prefix=spelling(ns->source_corresp);
      if(!prefix.empty())name=prefix+"::"+name;
    }
    return name;
  }
  void dependent_template_arguments(Id id,a_constant_ptr c,const a_source_position &p) {
    Id pack=0;unsigned ordinal=0,element=0;
    for(auto arg=c->variant.template_param.variant.template_ref.arg_list;arg;arg=arg->next) {
      if(arg->kind==tak_start_of_pack_expansion) {
        pack=node("template-argument-pack",p);
        edge(id,"template-argument",ordinal++,pack);element=0;continue;
      }
      if(!arg->is_pack_element && !arg->is_pack)pack=0;
      Id value=0;
      switch(arg->kind) {
        case tak_type:value=node("type-reference",p,type_id(arg->variant.type));break;
        case tak_nontype:
          if(arg->is_array_bound_of_unknown_type || !arg->variant.constant)
            missing_dependent_value(c,p,"dependent explicit template argument value");
          value=constant(arg->variant.constant,p);break;
        default:missing_dependent_value(c,p,"dependent explicit template argument kind");
      }
      if(arg->pack_expansion_descr || arg->is_pack || arg->has_pack_ellipsis) {
        Id expansion=node("template-pack-expansion",p);
        edge(expansion,"operand",0,value);value=expansion;
      }
      if(pack)edge(pack,"element",element++,value);
      else edge(id,"template-argument",ordinal++,value);
    }
  }
  void dependent_value(Id id,a_constant_ptr c,const a_source_position &p,an_expr_node_ptr local_expression=nullptr) {
    auto &value=c->variant.template_param;
    switch(value.kind) {
      case tpck_param:case tpck_member: {
        Value depth=nullptr,position=nullptr;Id qualifier=0;
        if(value.kind==tpck_param) {
          depth=value.variant.coordinates.depth;position=value.variant.coordinates.position;
        } else {
          auto owner=parent_class_or_null(c);
          if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
          qualifier=type_id(owner);
        }
        db.insert("template_value_references",{{"node_id",id},{"name",spelling(c->source_corresp)},
          {"parameter_depth",depth},{"parameter_position",position},{"is_pack",bool(value.is_pack)},
          {"qualifier_type_id",optional(qualifier)}});
        break;
      }
      case tpck_expression: {
        // This stock accessor also resolves EDG's file/function-region
        // correspondence. Only the source expression is projected.
        auto operand=local_expression?local_expression:expr_node_from_tpck_expression(c);
        if(!operand)missing("dependent source expression");
        edge(id,"operand",0,expression(operand));break;
      }
      case tpck_sizeof:case tpck_datasizeof:case tpck_alignof:
      case tpck_noexcept:case tpck_typeid:case tpck_uuidof: {
        if(!template_recipe)missing_dependent_value(c,p,"source query constant outside source recipe");
        auto &selected=value.variant.templ_sizeof;
        auto operand=local_expression?local_expression:generic_sizeof_arg_expr(c);
#if PROTOTYPE_INSTANTIATIONS_IN_IL
        if(value.local_expr_ref && !operand)
          missing_dependent_value(c,p,"source query local expression correspondence");
#endif
        const char *op=value.kind==tpck_sizeof?"sizeof":value.kind==tpck_datasizeof?"datasizeof":
          value.kind==tpck_noexcept?"noexcept":value.kind==tpck_typeid?"typeid-address":
          value.kind==tpck_uuidof?"uuidof-address":
          selected.is_std_alignof?"alignof":"gnu-alignof";
        if(value.kind==tpck_noexcept && !operand)
          missing_dependent_value(c,p,"noexcept selected expression correspondence");
        if(!operand && !selected.type)
          missing_dependent_value(c,p,"source query selected operand correspondence");
        db.update("nodes","id",id,{{"operator",op}});
        query_operand(id,selected.type,operand,p);break;
      }
      case tpck_dependent_constant:case tpck_address:
        edge(id,"operand",0,constant(value.variant.constant,c->source_corresp.decl_position));break;
      case tpck_unknown_function: {
        if(!template_recipe)missing_dependent_value(c,p,"unresolved function reference outside source recipe");
        auto owner=parent_class_or_null(c);
        if(owner && class_type_supp(owner)->proxy_of_type)owner=class_type_supp(owner)->proxy_of_type;
        db.insert("template_value_references",{{"node_id",id},{"name",namespace_reference_name(c)},
          {"is_pack",bool(value.is_pack)},{"qualifier_type_id",optional(type_id(owner))}});
        if(value.has_address_of)db.update("nodes","id",id,{{"operator","address-of"}});
        if(value.variant.unknown_function.conversion_type)
          edge(id,"conversion-type",0,node("type-reference",p,type_id(value.variant.unknown_function.conversion_type)));
#if MICROSOFT_EXTENSIONS_ALLOWED
        if(value.variant.unknown_function.property_or_event_descr)
          missing_dependent_value(c,p,"C++/CLI dependent accessor reference");
#endif
        break;
      }
      case tpck_template_ref:
        if(!template_recipe)missing_dependent_value(c,p,"unresolved template reference outside source recipe");
        edge(id,"template",0,constant(value.variant.template_ref.con,p));
        dependent_template_arguments(id,c,p);break;
      case tpck_destructor:
        if(!template_recipe)missing_dependent_value(c,p,"dependent destructor outside source recipe");
        db.update("nodes","id",id,{{"operator",value.variant.destructor.unqualified?"unqualified":"qualified"}});
        db.insert("template_value_references",{{"node_id",id},{"name",spelling(c->source_corresp)},
          {"is_pack",bool(value.is_pack)},{"qualifier_type_id",optional(type_id(value.variant.destructor.type))}});
        break;
      default:missing_dependent_value(c,p,"dependent value");
    }
  }
  Id aggregate_element(a_constant_ptr c,const a_source_position &p,bool complete_object=true) {
    if(c->kind!=ck_dynamic_init)return constant(c,p,complete_object);
    auto init=c->variant.dynamic_init.ptr;
    // lower_init::lower_ck_dynamic_init applies this exact selected action
    // to the current aggregate destination. add_init_assignment uses the
    // selected expression/constant/zero action for complete record values
    // without introducing a constructor. Base subobject copies still need
    // an explicit destination extent and remain outside this bound.
    bool value_component=is_scalar_type(c->type) || is_reference_type(c->type) ||
      (complete_object && is_immediate_class_type(skip_typerefs(c->type)));
    if(!init || !value_component ||
       (init->kind!=dik_expression && init->kind!=dik_constant && init->kind!=dik_zero) ||
       init->variable || init->destructor || init->lifetime || init->init_expr_lifetime ||
       init->lifetime_of_overlapping_temps || init->master_entry ||
       init->class_rvalue_initialized_through_master_entry || init->unordered ||
       init->overlaps_temps_in_inner_lifetime ||
       init->destruction_is_for_partially_constructed_aggregate ||
       init->is_freeing_of_storage_on_exception)
      missing_initialization(init,c->type,p,
        "aggregate component requires scalar, reference or complete record initialization without cleanup/remapping",c);
    Id id=node("initialize",p,type_id(c->type));
    db.update("nodes","id",id,{{"initialization_id",initialization_id(init,c->type)}});
    return id;
  }
  void record_initializer(Id id,a_constant_ptr c,const a_source_position &p,bool complete_object) {
    auto type=skip_typerefs(c->type);
    auto &record=type->variant.class_struct_union;
    if(record.any_virtual_base_classes || record.any_virtual_functions_including_in_base_classes)
      missing("record initializer virtual layout actions");
    if(c->uses_designated_initializers && !c->explicit_braces_on_aggregate &&
       !c->explicit_parentheses_on_aggregate)
      missing("unbraced designated record initializer merge");
#if DO_IL_LOWERING
    if(c->has_been_prelowered)missing("record initializer already lowered");
#endif
    a_targ_size_t zero_bytes=0;
    if(c->is_partially_initialized && !record.is_empty_class) {
#if IA64_ABI
      if(contains_ptr_to_data_member(c->type))missing("record initializer pointer-member zero model");
#endif
      // Exact extent selected by lower_init::insert_call_to_initialize_entity:
      // empty classes need no writes; base subobjects exclude reusable tail
      // padding and virtual bases. The consumer receives only this action.
      zero_bytes=complete_object?type->size:class_type_supp(type)->size_without_virtual_base_classes;
    }
    db.insert("record_initializers",{{"node_id",id},{"zero_byte_count",zero_bytes}});
    auto field=next_proper_initializable_field(record.field_list);
    auto base=direct_base_classes_of(type);
    unsigned ordinal=0;
    for(auto item=c->variant.aggregate.first_constant;item;item=item->next) {
      if(item->is_pack_expansion)missing("record initializer dependent pack destination");
      if(item->kind==ck_designator) {
        if(item->variant.designator.is_generic || !item->variant.designator.is_field_designator)
          missing("record initializer non-field designator");
        field=item->variant.designator.variant.field;
        item=item->next;
        if(!item || item->kind==ck_designator)missing("record initializer designator value");
      }
      if(item->kind==ck_init_repeat)missing("record initializer repetition");
      Id selected_field=0,selected_base=0;
      bool child_complete=true;
      if(item->constant_for_base_class) {
        if(!base || base->is_virtual)missing("record initializer selected direct base");
        selected_base=base_id(base);child_complete=false;base=base->next_direct;
      } else {
        if(!field)missing("record initializer selected field");
        if(field->has_no_unique_address_attribute && is_immediate_class_type(skip_typerefs(field->type)) &&
           !skip_typerefs(field->type)->variant.class_struct_union.is_empty_class)
          missing("record initializer overlapping nonempty field");
        selected_field=field_id(field);
        field=type->kind==tk_union?nullptr:next_proper_initializable_field(field->next);
      }
      const auto &position=item->source_corresp.decl_position.seq?item->source_corresp.decl_position:p;
      db.insert("record_initializer_elements",{{"initializer_node_id",id},{"ordinal",ordinal++},
        {"field_id",optional(selected_field)},{"base_subobject_id",optional(selected_base)},
        {"value_node_id",aggregate_element(item,position,child_complete)}});
    }
  }
  void aggregate_initializer(Id id,a_constant_ptr c,const a_source_position &p,bool complete_object=true) {
    if(template_recipe) {
      db.update("nodes","id",id,{{"kind","aggregate-initializer"},
        {"operator",c->explicit_braces_on_aggregate?"braces":c->explicit_parentheses_on_aggregate?"parentheses":"implicit"}});
      unsigned ordinal=0;
      for(auto item=c->variant.aggregate.first_constant;item;item=item->next,++ordinal) {
        const auto &position=item->source_corresp.decl_position.seq?item->source_corresp.decl_position:p;
        if(item->kind==ck_designator || item->kind==ck_init_repeat || item->kind==ck_dynamic_init) {
          auto source=location(position);std::string path;
          for(const auto &file:files)if(file.second==source.file){path=file.first;break;}
          missing("source aggregate selected component [type="+spelling(c->type->source_corresp)+
            ", ordinal="+std::to_string(ordinal)+", file="+path+
            ", line="+std::to_string(source.line)+", column="+std::to_string(source.column)+"]",int(item->kind));
        }
        edge(id,"element",ordinal,constant(item,position));
      }
      return;
    }
    auto type=skip_typerefs(c->type);
    if(is_immediate_class_type(type)) {
      record_initializer(id,c,p,complete_object);return;
    }
    if(type->kind!=tk_array) {
      auto pos=location(c->source_corresp.decl_position.seq?c->source_corresp.decl_position:p);
      std::string path;
      for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
      missing("aggregate initializer type [type="+spelling(type->source_corresp)+
        ", type-kind="+std::to_string(int(type->kind))+
        ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]");
    }
    if(c->uses_designated_initializers && !c->explicit_braces_on_aggregate &&
       !c->explicit_parentheses_on_aggregate)
      missing("unbraced designated array initializer merge");
    // decl_inits propagates this selected flag to dynamic initialization;
    // c_gen_be::start_initializer_assignments uses it to zero the object
    // before element writes. No element-count inference is needed here.
    db.insert("array_initializers",{{"node_id",id},{"zero_initialize",bool(c->is_partially_initialized)}});
    a_targ_size_t index=0;unsigned ordinal=0;
    for(auto item=c->variant.aggregate.first_constant;item;item=item->next) {
      // Same selected position updates as c_gen_be::dump_initializer_part.
      if(item->kind==ck_designator) {
        if(item->variant.designator.is_generic || item->variant.designator.is_field_designator)
          missing("non-array initializer designator");
        index=item->variant.designator.variant.array_element;
        item=item->next;
        if(!item || item->kind==ck_designator)missing("array initializer designator value");
      }
      auto value=item;a_targ_size_t count=1;
      if(item->kind==ck_init_repeat) {
        value=item->variant.init_repeat.constant;count=item->variant.init_repeat.count;
        if(value->kind==ck_dynamic_init ||
           (value->kind==ck_aggregate && value->variant.aggregate.has_dynamic_init_component))
          missing("repeated dynamic aggregate component evaluation");
        if(!count)missing("runtime array initializer repetition");
        if(item->variant.init_repeat.multidimensional_aggr_tail_not_repeated)
          missing("designated array initializer repetition tail");
        // EDG can compact repeated leaf values across several nested rows.
        // That selected form needs an explicit multidimensional destination;
        // this immediate-element relation deliberately does not flatten it.
        if(!identical_types_ignoring_qualifiers(value->type,type->variant.array.element_type))
          missing("flattened multidimensional array initializer repetition");
      }
      const auto &position=value->source_corresp.decl_position.seq?value->source_corresp.decl_position:p;
      db.insert("array_initializer_elements",{{"initializer_node_id",id},{"ordinal",ordinal++},
        {"array_index",index},{"repeat_count",count},{"value_node_id",aggregate_element(value,position)}});
      index+=count;
    }
  }
  a_memory_region_number literal_region(a_constant_ptr c) {
    if(in_file_scope(c))return NULL_region_number;
#if EXPENSIVE_CHECKING
    return il_entry_prefix_of(c).region_number;
#else
    if(literal_source_region==NULL_region_number)
      missing("unassociated function literal outside source owner");
    return literal_source_region;
#endif
  }
  Id emit_literal(a_type_ptr type,a_const_char *bytes,a_targ_size_t count) {
    Id id=next_literal++;
    db.insert("literal_objects",{{"id",id},{"type_id",type_id(type)},{"byte_count",count}});
    // EDG already decoded escapes and target encoding. In particular, length
    // includes embedded NULs and may omit a terminal NUL; never use strlen.
    for(a_targ_size_t offset=0;offset<count;++offset)
      db.insert("literal_bytes",{{"literal_id",id},{"byte_offset",offset},
        {"value",static_cast<unsigned char>(bytes[offset])}});
    return id;
  }
  void bind_literal_storage(a_variable_ptr v,SelectedLiteral &selected,Id object) {
    if(!selected.symbol) {
      selected.symbol=next_symbol++;
      auto pos=location(selected.position);
      // lower_il::promote_static_variable_out_of_function selects external
      // relaxed definitions (and IA64 COMDAT) for this narrow string path in
      // multiply emitted functions. Keep that exact link entity
      // separate from ordinary global initialization/storage emission.
      db.insert("symbols",{{"id",selected.symbol},{"kind","literal"},
        {"name",spelling(v->source_corresp)},
        {"link_name",v->source_corresp.name?v->source_corresp.name:""},
        {"linkage",linkage(v->source_corresp)},{"type_id",type_id(selected.type)},
        {"is_definition",true},{"definition_kind",std::string(linkage(v->source_corresp))=="external"?"weak":"strong"},
        {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    }
    db.update("literal_objects","id",object,{{"selected_symbol_id",selected.symbol}});
    if(!selected.object)selected.object=object;
  }
  void literal_storage_selected(a_constant_ptr c,a_variable_ptr v) {
    // Called at get_variable_for_string_constant's common return, after
    // promotion/linkage selection. The constant may precede source capture;
    // keep only correspondence and file-scope type/text, emitting no row yet.
    if(c->kind!=ck_string)missing("selected literal storage constant",int(c->kind));
    auto &selected=selected_literals[v];
    if(!selected.type) {
      selected.type=c->type;selected.bytes=c->variant.string.value;
      selected.byte_count=c->variant.string.length;
      selected.position=c->source_corresp.decl_position;
    }
    // The common-return hook may run outside source capture. Look up this
    // exact constant identity in the still-live owner caches, never derive
    // an owner from the currently lowering function.
    for(const auto &region:literal_constants) {
      auto found=region.second.find(c);
      if(found!=region.second.end())bind_literal_storage(v,selected,found->second);
    }
  }
  void literal_function_complete(a_routine_ptr r) {
    // lower_function_scope has finished all string-to-variable rewrites.
    // No constant pointer from this region can survive its reclamation.
    if(mem_region_for_routine(r)!=NULL_region_number)
      literal_constants.erase(mem_region_for_routine(r));
    for(const auto &entry:default_member_initializers)
      if(entry.first->variant.routine.ptr==r)
        missing("default member initializers without finalized constructor cleanup");
    for(auto entry=allocations.begin();entry!=allocations.end();) {
      if(entry->second.region==mem_region_for_routine(r)) {
        if(!entry->second.complete)missing("new expression without selected allocation size/result");
        entry=allocations.erase(entry);
      } else ++entry;
    }
    for(auto entry=deallocations.begin();entry!=deallocations.end();) {
      if(entry->second.region==mem_region_for_routine(r)) {
        if(!entry->second.execution)missing("delete expression without selected deallocator call");
        entry=deallocations.erase(entry);
      } else ++entry;
    }
  }
  Id selected_literal_object(a_variable_ptr v) {
    auto found=selected_literals.find(v);
    if(found==selected_literals.end())missing("nonliteral variable constant address");
    auto &selected=found->second;
    if(!selected.object) {
      Id id=emit_literal(selected.type,selected.bytes,selected.byte_count);
      bind_literal_storage(v,selected,id);
    }
    return selected.object;
  }
  Id literal_object(a_constant_ptr c) {
    if(c->kind!=ck_string)missing("nonstring literal address target",int(c->kind));
    if(c->variant.string.embed_expansion)missing("embed literal expansion");
    if(c->assoc_var) {
      literal_storage_selected(c,c->assoc_var);
      return selected_literal_object(c->assoc_var);
    }
    auto &ids=literal_constants[literal_region(c)];
    auto found=ids.find(c);if(found!=ids.end())return found->second;
    Id id=emit_literal(c->type,c->variant.string.value,c->variant.string.length);
    ids[c]=id;return id;
  }
  void attach_variable_address(Id id,a_constant_ptr c,const a_source_position &p) {
    auto v=c->variant.address.variant.variable;
    // EDG records a selected base entity, a byte offset and the converted
    // pointer type. This slice handles only a complete persistent object at
    // offset zero; subobject/routine/member-pointer relocations remain separate.
    if(c->variant.address.offset || c->variant.address.subobject_path ||
       c->variant.address.one_past_the_end || c->variant.address.is_object_reflection)
      missing("variable address constant requires complete object at offset zero");
    if(!is_pointer_type(c->type) || !var_has_static_or_thread_storage_duration(v) ||
       is_effective_thread_local(v))
      missing("variable address constant requires non-TLS persistent pointer target");
    if(v->source_corresp.is_local_to_function && !static_local_symbols.count(v))
      missing("variable address constant missing selected local static storage");
    Id symbol=variable(v);
    Id target=node("variable",p,type_id(v->type),"lvalue",nullptr,"",symbol,0,0,true);
    Id address=node("operation",p,type_id(make_pointer_type(v->type)),"prvalue","address-of","",0,0,0,true);
    db.update("nodes","id",target,{{"source_node_id",id}});
    db.update("nodes","id",address,{{"source_node_id",id}});
    edge(address,"operand",0,target);
    // set_variable_address_constant uses the same stock pointer type. Any
    // conversion retained on the final constant is explicit on this root.
    db.update("nodes","id",id,{{"kind","operation"},{"operator","cast"}});
    edge(id,"operand",0,address);
  }
  void attach_routine_address(Id id,a_constant_ptr c) {
    auto selected=c->variant.address.variant.routine;
    if(!selected || !is_pointer_type(c->type) || c->variant.address.offset ||
       c->variant.address.subobject_path || c->variant.address.one_past_the_end ||
       c->variant.address.is_object_reflection)
      missing("routine address constant requires selected function pointer at offset zero");
    // The selected routine and converted pointer type completely describe
    // this value. Retain the source constant without synthetic operators.
    db.update("nodes","id",id,{{"kind","routine-address"},{"symbol_id",routine(selected)}});
  }
  void typeinfo_storage_selected(a_type_ptr target,a_variable_ptr v) {
    auto found=selected_typeinfos.find(target);
    if(found==selected_typeinfos.end())return;
    auto &selected=found->second;
    if(selected.variable && selected.variable!=v)
      missing("static typeid selected storage correspondence");
    auto variable_entry=typeinfo_variables.find(v);
    if(variable_entry==typeinfo_variables.end()) {
      auto existing=variables.find(v);
      TypeInfoVariable storage;
      storage.symbol=existing==variables.end()?selected.symbol:existing->second;
      storage.row_exists=existing!=variables.end();
      typeinfo_variables[v]=storage;variables[v]=storage.symbol;
      variable_entry=typeinfo_variables.find(v);
    }
    if(selected.symbol!=variable_entry->second.symbol) {
      db.update("nodes","symbol_id",selected.symbol,{{"symbol_id",variable_entry->second.symbol}});
      selected.symbol=variable_entry->second.symbol;
    }
    selected.variable=v;
  }
  void attach_typeinfo_target(Id id,a_type_ptr target,const a_source_position &p) {
    auto &selected=selected_typeinfos[target];
    if(!selected.reference) {
      selected.symbol=next_symbol++;
      auto previous_function=function;bool previous_recipe=template_recipe;
      function=0;template_recipe=false;
      selected.reference=node("type-reference",p,type_id(target));
      template_recipe=previous_recipe;function=previous_function;
    }
    db.update("nodes","id",id,{{"kind","type-info-address"},{"symbol_id",selected.symbol}});
    edge(id,"selected-type",0,selected.reference);
    if(target->typeinfo_var)typeinfo_storage_selected(target,target->typeinfo_var);
  }
  void attach_typeinfo_address(Id id,a_constant_ptr c,const a_source_position &p) {
    if(c->variant.address.offset || c->variant.address.subobject_path ||
       c->variant.address.one_past_the_end || c->variant.address.is_object_reflection ||
       !is_pointer_type(c->type))
      missing("static typeid address requires selected complete object pointer");
    attach_typeinfo_target(id,c->variant.address.variant.type,p);
  }
  void finish_typeinfo_symbols() {
    for(const auto &entry:selected_typeinfos)
      if(!entry.second.variable)missing("static typeid without EDG-selected persistent RTTI variable");
    // A definition can select other RTTI objects in its initializer. Emit each
    // final ABI entity once, using its actual stock initializer when present.
    bool pending=true;
    while(pending) {
      pending=false;
      for(auto &entry:typeinfo_variables) {
        auto &selected=entry.second;if(selected.emitted)continue;
        selected.emitted=true;pending=true;
        auto v=entry.first;auto pos=location(v->source_corresp.decl_position);
        bool weak=false;
#if DO_IL_LOWERING
        weak=v->comdat_group!=nullptr;
#endif
#if GNU_EXTENSIONS_ALLOWED
        weak=weak || v->is_weak;
#endif
        std::initializer_list<std::pair<const char*,Value>> values={
          {"id",selected.symbol},{"kind","rtti-object"},{"name",spelling(v->source_corresp)},
          {"link_name",v->source_corresp.name?v->source_corresp.name:""},
          {"linkage",linkage(v->source_corresp)},{"type_id",type_id(v->type)},
          {"parent_symbol_id",nullptr},{"is_definition",v->storage_class!=sc_extern},
          {"definition_kind",weak?"weak":"strong"},{"file_id",optional(pos.file)},
          {"line",pos.line},{"column",pos.column}};
        if(selected.row_exists)db.update("symbols","id",selected.symbol,values);
        else db.insert("symbols",values);
        if(!selected.row_exists)global_initializer(v,selected.symbol);
      }
    }
  }
  void attach_literal(Id id,a_constant_ptr c,const a_source_position &p,bool initializer=false) {
    if(c->kind==ck_string) {
      db.update("nodes","id",id,{{"literal_object_id",literal_object(c)}});
      // check_string_constant_type retains the destination extent and marks
      // partial initialization. c_gen_be::dump_init_assignment copies exactly
      // length bytes, after the selected partial aggregate zero action.
      if(initializer && !template_recipe)
        db.insert("literal_initializers",{{"node_id",id},{"copy_byte_count",c->variant.string.length},
          {"zero_byte_count",c->is_partially_initialized?c->type->size:0}});
    } else {
      Id object=0;
      if(c->variant.address.kind==abk_constant)
        object=literal_object(c->variant.address.variant.constant);
      else if(c->variant.address.kind==abk_variable) {
        auto v=c->variant.address.variant.variable;
        if(!selected_literals.count(v)){attach_variable_address(id,c,p);return;}
        object=selected_literal_object(v);
      }
      else if(c->variant.address.kind==abk_routine) {attach_routine_address(id,c);return;}
      else if(c->variant.address.kind==abk_typeid) {attach_typeinfo_address(id,c,p);return;}
      else {
        auto pos=location(p);std::string path,owner;
        for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        for(const auto &routine:routines)if(routine.second==function){owner=spelling(routine.first->source_corresp);break;}
        std::string detail="nonliteral constant address [function="+owner+", file="+path+
          ", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column);
        if(c->variant.address.kind==abk_typeid) {
          auto target=c->variant.address.variant.type;
          detail+=", typeid-target="+spelling(target->source_corresp)+", typeid-kind="+std::to_string(int(target->kind));
        }
        detail+="]";
        missing(detail,int(c->variant.address.kind));
      }
      db.update("nodes","id",id,{{"literal_object_id",object},{"literal_byte_offset",c->variant.address.offset}});
    }
  }
  Id constant(a_constant_ptr c,const a_source_position &p,bool complete_object=true,an_expr_node_ptr local_expression=nullptr) {
    if(template_recipe) {
      auto found=recipe_constants.find(c);if(found!=recipe_constants.end())return found->second;
    }
    if(c->kind==ck_template_param) {
      Id id=node(dependent_value_kind(c,p),p,type_id(c->type),"prvalue");
      if(template_recipe)recipe_constants[c]=id;
      dependent_value(id,c,p,local_expression);attach_enum_value(id,c);return id;
    }
    if(c->kind==ck_aggregate) {
      Id id=node(template_recipe?"aggregate-initializer":is_immediate_class_type(skip_typerefs(c->type))?"record-initializer":"array-initializer",
        p,type_id(c->type),"prvalue");
      if(template_recipe)recipe_constants[c]=id;
      aggregate_initializer(id,c,p,complete_object);return id;
    }
    if(c->kind==ck_string || c->kind==ck_address) {
      Id id=node(c->kind==ck_string?"string-literal":"address-constant",p,type_id(c->type),"prvalue");
      if(template_recipe)recipe_constants[c]=id;
      attach_literal(id,c,p,true);return id;
    }
    Id id=node("constant",p,type_id(c->type),"prvalue",nullptr,literal_text(c,"initializer",&p));
    if(template_recipe)recipe_constants[c]=id;
    if(c->kind==ck_float)db.update("nodes","id",id,{{"floating_value_id",floating_value(c)}});
    attach_enum_value(id,c);return id;
  }
  void attach_enum_value(Id id,a_constant_ptr c) {
    auto found=enum_values.find(c);
    if(found==enum_values.end()) {
      if(!is_enum_constant(c))return;
      auto type=skip_lexical_typerefs(c->type);
      type_id(underlying_enum_type(type));
      found=enum_values.find(c);
    }
    // C enumerators carry an affiliated integral type; C++ enumerators
    // carry the enum type itself. Retain only an actual EDG entity identity,
    // never guess a declaration from a coincidentally equal value or name.
    if(found!=enum_values.end())db.update("nodes","id",id,{{"enum_value_id",found->second}});
  }
  bool empty_pack_parameter(a_variable_ptr v) {
    // Exact lower_il::is_empty_pack_parameter predicate. func_def creates
    // this dummy for an empty expansion; it never denotes runtime storage.
    return v->compiler_generated && type_is(v->type,tk_template_param);
  }
  a_local_static_variable_init_ptr static_local_initializer(a_scope_ptr scope,a_variable_ptr v) {
    for(auto init=scope->local_static_variable_inits;init;init=init->next)
      if(init->variable==v)return init;
    return nullptr;
  }
  bool supported_static_local(a_scope_ptr scope,a_variable_ptr v) {
    if(v->storage_class!=sc_static || is_effective_thread_local(v))return false;
    if(v->init_kind==initk_none || v->init_kind==initk_zero || v->init_kind==initk_static)return true;
    if(v->init_kind!=initk_function_local)return false;
    auto init=static_local_initializer(scope,v);
    return init && init->init_kind==initk_static && !init->lifetime;
  }
  bool has_static_storage(a_scope_ptr scope) {
    for(auto init=scope->local_static_variable_inits;init;init=init->next)
      if(init->init_kind!=initk_static || init->lifetime)return true;
    for(auto v=scope->variables;v;v=v->next)
      if(!empty_pack_parameter(v) && !supported_static_local(scope,v))return true;
    return false;
  }
  void static_local(a_scope_ptr scope,a_variable_ptr v) {
    if(static_local_symbols.count(v))return;
    if(!supported_static_local(scope,v))missing("local static requires constant/zero storage without guard or destruction");
    if(variables.count(v))missing("local static captured without lexical storage owner");
    Id id=next_symbol++;static_local_symbols[v]=id;variables[v]=id;
    auto pos=location(v->source_corresp.decl_position);
    db.insert("symbols",{{"id",id},{"kind","static-local"},{"name",spelling(v->source_corresp)},
      {"link_name",v->source_corresp.name?v->source_corresp.name:""},{"linkage",linkage(v->source_corresp)},
      {"type_id",type_id(v->type)},{"parent_symbol_id",function},{"is_definition",true},
      {"definition_kind","strong"},{"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
    auto init_kind=v->init_kind;
    a_constant_ptr value=nullptr;
    if(init_kind==initk_function_local) {
      auto selected=static_local_initializer(scope,v);
      init_kind=selected->init_kind;value=selected->initializer.constant;
    } else if(init_kind==initk_static)value=v->initializer.constant;
    Id previous=function;bool previous_recipe=template_recipe;function=0;template_recipe=false;
    Id action;
    if(init_kind==initk_static)action=constant(value,v->source_corresp.decl_position);
    else {
      auto type=skip_typerefs(v->type);
#if IA64_ABI
      if(contains_ptr_to_data_member(type))missing("local static pointer-member zero model");
#endif
      if(type->kind==tk_array) {
        action=node("array-initializer",v->source_corresp.decl_position,type_id(v->type),"prvalue");
        db.insert("array_initializers",{{"node_id",action},{"zero_initialize",true}});
      } else if(is_scalar_type(type)) {
        action=node("constant",v->source_corresp.decl_position,type_id(v->type),"prvalue",nullptr,"0");
      } else missing("local static zero storage type",int(type->kind));
    }
    db.insert("global_initializers",{{"symbol_id",id},{"initializer_node_id",action}});
    function=previous;template_recipe=previous_recipe;
  }
  void static_local_storage_selected(a_variable_ptr v,a_routine_ptr owner) {
    auto found=static_local_symbols.find(v);if(found==static_local_symbols.end())return;
    if(!routines.count(owner))missing("promoted local static without source function");
    if(v->initialization_rewritten_as_assignment)
      missing("local static constant rewritten as guarded initialization");
    promoted_static_locals.insert(v);
    // promote_static_variable_out_of_function selects external linkage only
    // for the relaxed ref/def model of a multiple-copy routine (and adds an
    // IA64 COMDAT). No consumer name-based pooling or guard is inferred.
    db.update("symbols","id",found->second,{{"link_name",v->source_corresp.name?v->source_corresp.name:""},
      {"linkage",linkage(v->source_corresp)},{"is_definition",v->storage_class!=sc_extern},
      {"definition_kind",v->source_corresp.name_linkage==nlk_external?"weak":"strong"}});
  }
  Id lifetime_id(an_object_lifetime_ptr value) {
    if(!value)return 0;
    if(template_recipe)missing("template recipe lifetime ownership");
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
    if(!deferred_destructions.count(value))
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
  [[noreturn]] void missing_initialization(a_dynamic_init_ptr init,a_type_ptr target,
      a_source_position pos,const char *reason,a_constant_ptr component=nullptr) {
    if(!init)missing(std::string(reason)+" without selected initialization");
    a_constant_ptr aggregate=nullptr;
    if(init->kind==dik_nonconstant_aggregate) {
      aggregate=init->variant.constant.ptr;
      if(!component && aggregate->kind==ck_aggregate)
        for(auto item=aggregate->variant.aggregate.first_constant;item;item=item->next)
          if(item->kind==ck_dynamic_init){component=item;break;}
      if(!pos.seq)pos=aggregate->source_corresp.decl_position;
    }
    auto selected=component?component->variant.dynamic_init.ptr:nullptr;
    if(!pos.seq && component)pos=component->source_corresp.decl_position;
    if(!pos.seq && selected && selected->kind==dik_expression)
      pos=selected->variant.expression->position;
    if(!pos.seq && init->variable)pos=init->variable->source_corresp.decl_position;
    if(!pos.seq && (init->kind==dik_expression || init->kind==dik_class_result_via_ctor))
      pos=init->variant.expression->position;
    if(!pos.seq && init->kind==dik_constructor && init->variant.constructor.args)
      pos=init->variant.constructor.args->position;
    auto init_expression=(init->kind==dik_expression || init->kind==dik_class_result_via_ctor)?
      init->variant.expression:nullptr;
    if(!pos.seq && init_expression && init_expression->kind==enk_operation)
      for(auto child=init_expression->variant.operation.operands;child;child=child->next)
        if(child->position.seq){pos=child->position;break;}
    std::string name;
    for(const auto &entry:routines)if(entry.second==function) {
      name=spelling(entry.first->source_corresp);
      if(!pos.seq)pos=entry.first->source_corresp.decl_position;
      break;
    }
    auto source=location(pos);std::string path;
    for(const auto &file:files)if(file.second==source.file){path=file.first;break;}
    auto entity=target?target:init->variable?init->variable->type:aggregate?aggregate->type:nullptr;
    std::string detail=std::string(reason)+" [function="+name+
      ", target="+(entity?spelling(entity->source_corresp):"")+
      ", target-kind="+std::to_string(entity?int(skip_typerefs(entity)->kind):-1)+
      ", variable="+(init->variable?spelling(init->variable->source_corresp):"")+
      ", aggregate-kind="+std::to_string(aggregate?int(aggregate->kind):-1)+
      ", partial="+std::to_string(int(init->is_partially_initialized))+
      ", destructor="+std::to_string(init->destructor?1:0)+
      ", lifetime="+std::to_string(init->lifetime?1:0)+
      ", expression-lifetime="+std::to_string(init->init_expr_lifetime?1:0)+
      ", overlap="+std::to_string(init->lifetime_of_overlapping_temps?1:0)+
      ", shared-result="+std::to_string(int(init->class_rvalue_initialized_through_master_entry))+
      ", master-kind="+std::to_string(init->master_entry?int(init->master_entry->kind):-1)+
      ", master-variable="+(init->master_entry && init->master_entry->variable?
        spelling(init->master_entry->variable->source_corresp):"")+
      ", partial-cleanup="+std::to_string(int(init->destruction_is_for_partially_constructed_aggregate));
    if(init_expression) {
      detail+=", expression-kind="+std::to_string(int(init_expression->kind))+
        ", expression-operator="+std::to_string(init_expression->kind==enk_operation?
          int(init_expression->variant.operation.kind):-1);
      if(init_expression->kind==enk_operation) {
        unsigned ordinal=0;
        for(auto child=init_expression->variant.operation.operands;child && ordinal<3;child=child->next,++ordinal) {
          auto child_source=location(child->position);std::string child_path;
          for(const auto &file:files)if(file.second==child_source.file){child_path=file.first;break;}
          auto child_init=child->kind==enk_temp_init?child->variant.init.dynamic_init:nullptr;
          detail+=" [operand="+std::to_string(ordinal)+", kind="+std::to_string(int(child->kind))+
            ", operator="+std::to_string(child->kind==enk_operation?int(child->variant.operation.kind):-1)+
            ", initialization-kind="+std::to_string(child_init?int(child_init->kind):-1)+
            ", shares-selected-destination="+std::to_string(child_init && child_init->master_entry==init?1:0)+
            ", variable="+(child_init && child_init->variable?spelling(child_init->variable->source_corresp):"")+
            ", file="+child_path+", line="+std::to_string(child_source.line)+
            ", column="+std::to_string(child_source.column)+"]";
        }
      }
    }
    if(selected)detail+=", component-kind="+std::to_string(int(selected->kind))+
      ", component-type="+(component->type?spelling(component->type->source_corresp):"")+
      ", component-type-kind="+std::to_string(component->type?int(skip_typerefs(component->type)->kind):-1)+
      ", component-destructor="+std::to_string(selected->destructor?1:0)+
      ", component-lifetime="+std::to_string(selected->lifetime?1:0)+
      ", component-expression-lifetime="+std::to_string(selected->init_expr_lifetime?1:0)+
      ", component-overlap="+std::to_string(selected->lifetime_of_overlapping_temps?1:0)+
      ", component-partial="+std::to_string(int(selected->destruction_is_for_partially_constructed_aggregate));
    missing(detail+", file="+path+", line="+std::to_string(source.line)+
      ", column="+std::to_string(source.column)+"]",int(init->kind));
  }
  Id initialization_id(a_dynamic_init_ptr init,a_type_ptr target=nullptr) {
    if(!init)return 0;
    if(template_recipe)missing("template recipe initialization ownership");
    auto selected_target=target?target:init->variable?init->variable->type:nullptr;
    Id target_type=type_id(selected_target);
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
    if(init->class_rvalue_initialized_through_master_entry && init->kind!=dik_expression)
      missing_initialization(init,selected_target,pos,"shared-result selected effect action kind");
    // The selected destination is a semantic storage-sharing relation.
    // Reserving this action first handles either master-first or dependent-
    // first capture, including a nested master using an outer destination.
    Id destination=init->master_entry?initialization_id(init->master_entry,selected_target):0;
    switch(init->kind) {
      case dik_none:kind="none";break;
      case dik_zero:kind="zero";break;
      case dik_expression:
        // Stock lower_class_rvalue_returning_operation evaluates this
        // conditional/comma graph for initialization effects; its class
        // result is not copied into the destination afterward.
        kind=init->class_rvalue_initialized_through_master_entry?"shared-result":"expression";
        value=expression(init->variant.expression);break;
      case dik_class_result_via_ctor:kind="class-result";value=expression(init->variant.expression);break;
      case dik_constant:kind="constant";value=constant(init->variant.constant.ptr,pos,!subobject_initializations.count(init));break;
      case dik_nonconstant_aggregate:
      case dik_lambda:
        if(init->destructor || init->lifetime || init->init_expr_lifetime ||
           init->lifetime_of_overlapping_temps || init->unordered ||
           init->overlaps_temps_in_inner_lifetime ||
           init->destruction_is_for_partially_constructed_aggregate ||
           init->is_freeing_of_storage_on_exception)
          missing_initialization(init,target,pos,"nonconstant aggregate requires richer cleanup projection");
        kind=init->kind==dik_lambda && !init->variant.constant.non_constant?"constant":"aggregate";
        if(init->kind==dik_lambda) {
          auto selected=init->variant.constant.lambda;
          if(!selected || !selected->closure_class || !init->variant.constant.ptr)
            missing_initialization(init,target,pos,"selected lambda aggregate initializer");
          // A direct closure declaration can retain only dik_lambda, without
          // an enk_lambda wrapper. Preserve its source identity on the same
          // selected aggregate value, without another initialization action.
          value=constant(init->variant.constant.ptr,selected->start_position,!subobject_initializations.count(init));
          db.update("nodes","id",value,{{"operator","lambda"},
            {"symbol_id",optional(selected->lambda_routine?routine(selected->lambda_routine):0)}});
        } else value=constant(init->variant.constant.ptr,pos,!subobject_initializations.count(init));
        break;
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
      default:missing_initialization(init,target,pos,"dynamic initialization");
    }
    Id life=lifetime_id(init->lifetime),expr_life=lifetime_id(init->init_expr_lifetime);
    Id overlap=lifetime_id(init->lifetime_of_overlapping_temps);
    db.insert("initializations",{{"id",id},{"function_id",optional(function)},{"kind",kind},
      {"type_id",optional(initialization_types[init])},
      {"destination_initialization_id",optional(destination)},
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
  const char *operation(an_expr_operator_kind kind,an_expr_node_ptr source=nullptr) {
    switch(kind) {
#define DOT_SEMANTIC_OP(edg,text) case edg:return text
      DOT_SEMANTIC_OP(eok_address_of,"address-of");DOT_SEMANTIC_OP(eok_reference_to,"reference-to");
      DOT_SEMANTIC_OP(eok_indirect,"indirect");DOT_SEMANTIC_OP(eok_ref_indirect,"ref-indirect");
      DOT_SEMANTIC_OP(eok_cast,"cast");DOT_SEMANTIC_OP(eok_ref_cast,"ref-cast");
      DOT_SEMANTIC_OP(eok_lvalue_cast,"lvalue-cast");DOT_SEMANTIC_OP(eok_lvalue_adjust,"lvalue-adjust");
      DOT_SEMANTIC_OP(eok_class_rvalue_adjust,"class-rvalue-adjust");
      DOT_SEMANTIC_OP(eok_base_class_cast,"base-class-cast");DOT_SEMANTIC_OP(eok_derived_class_cast,"derived-class-cast");
      DOT_SEMANTIC_OP(eok_bool_cast,"bool-cast");DOT_SEMANTIC_OP(eok_array_to_pointer,"array-to-pointer");
      DOT_SEMANTIC_OP(eok_dot_vacuous_destructor_call,"dot-vacuous-destructor-call");
      DOT_SEMANTIC_OP(eok_points_to_vacuous_destructor_call,"points-to-vacuous-destructor-call");
      DOT_SEMANTIC_OP(eok_add,"add");DOT_SEMANTIC_OP(eok_subtract,"subtract");
      DOT_SEMANTIC_OP(eok_multiply,"multiply");DOT_SEMANTIC_OP(eok_divide,"divide");DOT_SEMANTIC_OP(eok_remainder,"remainder");
      DOT_SEMANTIC_OP(eok_padd,"pointer-add");DOT_SEMANTIC_OP(eok_psubtract,"pointer-subtract");
      DOT_SEMANTIC_OP(eok_pdiff,"pointer-difference");
      DOT_SEMANTIC_OP(eok_eq,"eq");DOT_SEMANTIC_OP(eok_ne,"ne");DOT_SEMANTIC_OP(eok_lt,"lt");
      DOT_SEMANTIC_OP(eok_le,"le");DOT_SEMANTIC_OP(eok_gt,"gt");DOT_SEMANTIC_OP(eok_ge,"ge");
      DOT_SEMANTIC_OP(eok_assign,"assign");DOT_SEMANTIC_OP(eok_add_assign,"add-assign");
      DOT_SEMANTIC_OP(eok_padd_assign,"pointer-add-assign");
      DOT_SEMANTIC_OP(eok_psubtract_assign,"pointer-subtract-assign");
      DOT_SEMANTIC_OP(eok_subtract_assign,"subtract-assign");DOT_SEMANTIC_OP(eok_multiply_assign,"multiply-assign");
      DOT_SEMANTIC_OP(eok_divide_assign,"divide-assign");DOT_SEMANTIC_OP(eok_remainder_assign,"remainder-assign");
      DOT_SEMANTIC_OP(eok_and_assign,"bit-and-assign");DOT_SEMANTIC_OP(eok_or_assign,"bit-or-assign");
      DOT_SEMANTIC_OP(eok_xor_assign,"bit-xor-assign");
      DOT_SEMANTIC_OP(eok_shiftl_assign,"shift-left-assign");DOT_SEMANTIC_OP(eok_shiftr_assign,"shift-right-assign");
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
      DOT_SEMANTIC_OP(eok_va_start,"va-start");DOT_SEMANTIC_OP(eok_va_start_single_operand,"va-start-single");
      DOT_SEMANTIC_OP(eok_va_arg,"va-arg");DOT_SEMANTIC_OP(eok_va_copy,"va-copy");DOT_SEMANTIC_OP(eok_va_end,"va-end");
#undef DOT_SEMANTIC_OP
      default: {
        if(template_recipe)return recipe_operation(kind,source);
        std::string detail="expression operation";
        if(source) {
          auto pos=location(source->position);std::string path,owner;
          for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
          for(const auto &routine:routines)if(routine.second==function){owner=spelling(routine.first->source_corresp);break;}
          detail+=" [function="+owner+", file="+path+", line="+std::to_string(pos.line)+
            ", column="+std::to_string(pos.column)+"]";
        }
        missing(detail,int(kind));
      }
    }
  }
  const char *recipe_operation(an_expr_operator_kind kind,an_expr_node_ptr source) {
    // These names describe the selected source operation. Its ordered typed
    // operands remain recipes; no runtime conversion, dispatch or evaluation
    // is required to browse the expression.
    switch(kind) {
#define DOT_RECIPE_OP(edg,text) case edg:return text
      DOT_RECIPE_OP(eok_handle_to,"handle-to");DOT_RECIPE_OP(eok_box,"box");
      DOT_RECIPE_OP(eok_handle_to_box,"handle-to-box");DOT_RECIPE_OP(eok_unbox,"unbox");
      DOT_RECIPE_OP(eok_unbox_lvalue,"unbox-lvalue");
      DOT_RECIPE_OP(eok_pm_base_class_cast,"member-pointer-to-base");
      DOT_RECIPE_OP(eok_pm_derived_class_cast,"member-pointer-to-derived");
      DOT_RECIPE_OP(eok_dynamic_cast,"dynamic-cast");DOT_RECIPE_OP(eok_ref_dynamic_cast,"reference-dynamic-cast");
#if MICROSOFT_EXTENSIONS_ALLOWED
      DOT_RECIPE_OP(eok_assume,"assume");
#endif
      DOT_RECIPE_OP(eok_noexcept,"noexcept");DOT_RECIPE_OP(eok_parens,"parentheses");
      DOT_RECIPE_OP(eok_unary_plus,"unary-plus");DOT_RECIPE_OP(eok_vector_not,"vector-not");
      DOT_RECIPE_OP(eok_vector_fill,"vector-fill");
#if C99_IL_EXTENSIONS_SUPPORTED
      DOT_RECIPE_OP(eok_xconj,"complex-conjugate");DOT_RECIPE_OP(eok_real_part,"real-part");
      DOT_RECIPE_OP(eok_imag_part,"imaginary-part");DOT_RECIPE_OP(eok_jmultiply,"imaginary-multiply");
      DOT_RECIPE_OP(eok_jdivide,"imaginary-divide");DOT_RECIPE_OP(eok_fjadd,"real-imaginary-add");
      DOT_RECIPE_OP(eok_jfadd,"imaginary-real-add");DOT_RECIPE_OP(eok_fjsubtract,"real-imaginary-subtract");
      DOT_RECIPE_OP(eok_jfsubtract,"imaginary-real-subtract");
#endif
      DOT_RECIPE_OP(eok_spaceship,"three-way-comparison");
      DOT_RECIPE_OP(eok_vector_eq,"vector-eq");DOT_RECIPE_OP(eok_vector_ne,"vector-ne");
      DOT_RECIPE_OP(eok_vector_gt,"vector-gt");DOT_RECIPE_OP(eok_vector_lt,"vector-lt");
      DOT_RECIPE_OP(eok_vector_ge,"vector-ge");DOT_RECIPE_OP(eok_vector_le,"vector-le");
      DOT_RECIPE_OP(eok_gnu_min,"minimum");DOT_RECIPE_OP(eok_gnu_max,"maximum");
      DOT_RECIPE_OP(eok_bassign,"block-assignment");DOT_RECIPE_OP(eok_vector_land,"vector-logical-and");
      DOT_RECIPE_OP(eok_vector_lor,"vector-logical-or");DOT_RECIPE_OP(eok_vector_subscript,"vector-subscript");
      DOT_RECIPE_OP(eok_pm_field,"member-pointer-field");
      DOT_RECIPE_OP(eok_pm_points_to_field,"member-pointer-points-to-field");
      DOT_RECIPE_OP(eok_dot_pm_func_ptr,"member-function-pointer");
      DOT_RECIPE_OP(eok_points_to_pm_func_ptr,"points-to-member-function-pointer");
      DOT_RECIPE_OP(eok_dot_static,"dot-static");DOT_RECIPE_OP(eok_points_to_static,"points-to-static");
      DOT_RECIPE_OP(eok_virtual_function_ptr,"virtual-function-pointer");
      DOT_RECIPE_OP(eok_vector_question,"vector-conditional");
      DOT_RECIPE_OP(eok_dot_pm_call,"member-pointer-call");
      DOT_RECIPE_OP(eok_points_to_pm_call,"points-to-member-pointer-call");
      DOT_RECIPE_OP(eok_cli_subscript,"cli-subscript");DOT_RECIPE_OP(eok_lvalue,"dependent-lvalue");
      DOT_RECIPE_OP(eok_await,"co-await");DOT_RECIPE_OP(eok_yield,"co-yield");
      DOT_RECIPE_OP(eok_splice,"expression-splice");
#undef DOT_RECIPE_OP
      default:missing("source recipe operation discriminator",int(source?source->variant.operation.kind:kind));
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
  a_routine_ptr virtual_source(a_routine_ptr r) {
    if(!r)return nullptr;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
    if(r->overriding_function_for_wrapper)r=r->overriding_function_for_wrapper;
#endif
#if DO_IL_LOWERING && IA64_ABI
    if(r->primary_ctor_or_dtor)r=r->primary_ctor_or_dtor;
#endif
    return r;
  }
  void virtual_source_routine(a_routine_ptr r) {
    if(!r || virtual_source(r)!=r || !r->is_virtual || r->is_consteval)return;
    auto owner=parent_class_or_null(r);
    if(owner && types.count(owner))routine(r);
  }
  void virtual_source_class(a_type_ptr t) {
    if(!types.count(t) || is_template_dependent_type(t))return;
    // The existing stock prelower boundary precedes synthetic vptr/base
    // fields. Finalize a source declaration that became complete meanwhile.
    type_id(t);
    auto scope=class_type_supp(t)->assoc_scope;
    if(scope)for(auto r=scope->routines;r;r=r->next)virtual_source_routine(r);
  }
  Id virtual_table(a_type_ptr owner,a_base_class_ptr base,a_base_class_ptr construction,
                   a_variable_ptr table,long long address_point,long long entry_size,bool definition) {
    auto key=std::make_pair(table,address_point);
    auto found=virtual_tables.find(key);
    if(found!=virtual_tables.end()) {
      if(definition)db.update("virtual_tables","id",found->second,{{"is_definition",true}});
      return found->second;
    }
    Id id=next_virtual_table++;virtual_tables[key]=id;
    db.insert("virtual_tables",{{"id",id},{"owner_type_id",type_id(owner)},
      {"base_subobject_id",optional(base_id(base))},{"construction_base_subobject_id",optional(base_id(construction))},
      {"link_name",table->source_corresp.name?table->source_corresp.name:""},
      {"linkage",linkage(table->source_corresp)},{"address_point_byte_offset",address_point},
      {"entry_size",entry_size},{"is_definition",definition}});
    return id;
  }
  void virtual_table_begin(a_type_ptr owner,a_base_class_ptr base,a_base_class_ptr construction,
                          a_variable_ptr table,long long address_point,long long entry_size) {
    // Tables belonging solely to lowering/runtime internals are not source
    // entities. A zero context also shields a nested internal table build.
    virtual_table_stack.push_back(types.count(owner)?
      virtual_table(owner,base,construction,table,address_point,entry_size,true):0);
  }
  void virtual_table_end() {
    if(virtual_table_stack.empty())missing("virtual table capture context");
    virtual_table_stack.pop_back();
  }
  void virtual_table_entry(long long slot,a_routine_ptr selected,a_routine_ptr target,
                           long long delta,bool null_entry) {
    if(virtual_table_stack.empty() || !virtual_table_stack.back())return;
    auto original_selected=virtual_source(selected),original_target=virtual_source(target);
    auto source_signature=routine_source_types.find(original_selected);
    if(source_signature==routine_source_types.end())
      missing("virtual slot source signature for "+spelling(original_selected->source_corresp));
    VirtualEntry entry;
    entry.selected=routines.at(original_selected);entry.signature=source_signature->second;
    entry.receiver=routine_receiver_types.at(original_selected);
    if(!entry.receiver)missing("virtual slot source receiver");
    if(original_target) {
      auto source_target=routines.find(original_target);
      if(source_target==routines.end())
        missing("virtual target source signature for "+spelling(original_target->source_corresp));
      entry.target=source_target->second;
    }
    entry.abi_routine=target;entry.delta=delta;
    entry.kind=null_entry || !target?"null":original_target->pure_virtual?"pure":
      original_target->is_deleted?"deleted":"ordinary";
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN && IA64_ABI
    if(target && target->overriding_function_for_wrapper) {
      entry.delta+=target->delta;entry.vcall=target->vcall_index;
      entry.return_delta=target->return_delta;entry.vbase=target->vbase_index;
    }
#endif
#if DO_IL_LOWERING && IA64_ABI
    auto variant_target=target;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
    if(variant_target && variant_target->overriding_function_for_wrapper)
      variant_target=variant_target->overriding_function_for_wrapper;
#endif
    if(variant_target) {
      switch(variant_target->ctor_dtor_kind) {
        case cdk_none:break;
        case cdk_subobject:entry.variant="base";break;
        case cdk_complete:entry.variant="complete";break;
        case cdk_deleting:entry.variant="deleting";break;
        default:missing("virtual table constructor/destructor entry variant",int(variant_target->ctor_dtor_kind));
      }
    }
#endif
    // One final semantic row per physical slot, even if EDG revisits a table
    // after deciding its definition is required later in the translation.
    virtual_entries[{virtual_table_stack.back(),slot}]=entry;
  }
  void virtual_vptr_begin(a_routine_ptr r,const char *phase) {
    auto source=routines.find(virtual_source(r));
    vptr_phases.push_back({source==routines.end()?0:source->second,phase});
  }
  void virtual_vptr_end() {
    if(vptr_phases.empty())missing("virtual pointer action capture context");
    vptr_phases.pop_back();
  }
  void virtual_vptr_write(a_type_ptr owner,a_base_class_ptr base,an_expr_node_ptr address,
                          bool construction,long long construction_index) {
    if(vptr_phases.empty() || !vptr_phases.back().first)return;
    Id table=0;Value offset=nullptr;
    auto object_type=base?base->type:owner;
    if(!base || !any_virtual_steps_in_derivation(base)) {
#if IA64_ABI
      offset=base?(long long)base->offset:0LL;
#else
      offset=(base?(long long)base->offset:0LL)+
        (long long)class_type_supp(object_type)->virtual_function_info_offset;
#endif
    }
    (void)object_type;
    if(!construction) {
      if(!address || address->kind!=enk_constant)missing("selected virtual table address constant");
      auto c=node_constant(address);
      if(c->kind!=ck_address || c->variant.address.kind!=abk_variable)
        missing("selected virtual table variable address");
      auto variable=c->variant.address.variant.variable;
      table=virtual_table(owner,base,nullptr,variable,(long long)c->variant.address.offset,
        (long long)skip_typerefs(type_pointed_to(c->type))->size,false);
    }
    auto current=vptr_phases.back();
    db.insert("vptr_writes",{{"function_id",current.first},{"ordinal",vptr_ordinals[current.first]++},
      {"phase",current.second},{"base_subobject_id",optional(base_id(base))},{"vptr_byte_offset",offset},
      {"table_id",optional(table)},{"source_kind",construction?"construction-table":"table"},
      {"construction_index",construction?Value(construction_index):Value(nullptr)}});
  }
  void virtual_call(Id id,an_expr_node_ptr e) {
    auto callee=e->variant.operation.operands;
    auto receiver=callee?callee->next:nullptr;
    if(!callee || callee->kind!=enk_routine || !receiver) {
      auto pos=location(e->position);
      missing("selected virtual callee/receiver at line "+std::to_string(pos.line));
    }
    auto selected=callee->variant.routine.ptr;
    auto receiver_type=skip_typerefs(receiver->type);
    if(e->variant.operation.kind==eok_points_to_member_call)
      receiver_type=skip_typerefs(type_pointed_to(receiver_type));
    long long offset=0;
#if !IA64_ABI
    offset=(long long)class_type_supp(receiver_type)->virtual_function_info_offset;
#endif
    db.insert("virtual_calls",{{"node_id",id},{"selected_symbol_id",routine(selected)},
      {"receiver_node_id",expression(receiver)},{"receiver_type_id",type_id(receiver_type)},
      {"callable_type_id",routine_source_types.at(selected)},{"vptr_byte_offset",offset},
      {"slot_index",selected->number.virtual_function}});
  }
  bool plain_allocation(a_new_delete_supplement_ptr value) {
    if(!value->is_new || !value->routine || value->placement_new || value->aligned_version ||
       value->arg || value->dynamic_init || value->freeing_of_storage_on_exception || value->has_new_initializer)
      return false;
    auto allocated_type=skip_typerefs(value->type);
    auto element=allocated_type->kind==tk_array?skip_typerefs(allocated_type->variant.array.element_type):allocated_type;
    // Scalar elements use lower_new's plain allocation branch. Class cookies,
    // multidimensional counts, and initialized storage need further selected
    // actions and are deliberately outside this projection.
    return is_scalar_type(element);
  }
  bool placement_construction(a_new_delete_supplement_ptr value) {
    if(!value->is_new || !value->routine || !value->placement_new || value->aligned_version ||
       !value->arg || value->arg->next || !is_scalar_type(value->arg->type) || value->number_of_elements)
      return false;
    auto allocated_type=skip_typerefs(value->type);
    auto init=value->dynamic_init;auto cleanup=value->freeing_of_storage_on_exception;
    bool selected_init=init &&
      ((init->kind==dik_constructor && init->variant.constructor.ptr) ||
       (init->kind==dik_expression && init->variant.expression));
    return is_immediate_class_type(allocated_type) && !is_incomplete_type(allocated_type) &&
      selected_init &&
      !init->variable && !init->destructor && !init->lifetime && !init->master_entry &&
      !init->class_rvalue_initialized_through_master_entry &&
      cleanup && cleanup->kind==dik_none && cleanup->destructor &&
      cleanup->is_freeing_of_storage_on_exception && !cleanup->is_array_freeing;
  }
  bool zero_array_allocation(a_new_delete_supplement_ptr value) {
    if(!value->is_new || !value->routine || value->placement_new || value->aligned_version ||
       value->arg || !value->number_of_elements || value->freeing_of_storage_on_exception)
      return false;
    auto type=skip_typerefs(value->type);auto init=value->dynamic_init;
    return type->kind==tk_array && is_incomplete_array_type(type) &&
      is_scalar_type(skip_typerefs(type->variant.array.element_type)) &&
      init && init->kind==dik_zero && !init->variable && !init->destructor &&
      !init->lifetime && !init->init_expr_lifetime && !init->master_entry &&
      !init->class_rvalue_initialized_through_master_entry;
  }
  void source_allocation(Id id,an_expr_node_ptr e) {
    if(template_recipe || !function)missing("allocation outside executable function");
    auto value=e->variant.new_delete;
    auto &selected=allocations[value];
    selected.node=id;selected.owner=function;selected.region=literal_source_region;
    selected.position=e->position;selected.allocated_type=type_id(value->type);
    selected.allocator=routine(value->routine);selected.source_count=value->number_of_elements;
    selected.count_value=expression(value->number_of_elements);
    edge(id,"element-count",0,selected.count_value);
    if(value->placement_new) {
      selected.placement=true;
      unsigned ordinal=0;
      for(auto argument=value->arg;argument;argument=argument->next,++ordinal) {
        Id source=expression(argument,&e->position,id);
        edge(id,"placement-argument",ordinal,source);
        // Stock extra_args_for_operator_delete reuses these evaluated
        // placement argument values; failure cleanup must not reevaluate them.
        edge(id,"exception-argument",ordinal,source);
      }
      Id initialize=node("initialize",e->position,selected.allocated_type,nullptr,
        nullptr,"",0,0,0,true);
      db.update("nodes","id",initialize,{{"source_node_id",id},
        {"initialization_id",initialization_id(value->dynamic_init,value->type)}});
      edge(id,"initialize",0,initialize);
      // In stock lower_new's accepted non-array initialization path,
      // turn_off_freeing_of_storage_on_exception protects this initialization
      // expression. Placement arguments and the allocator result assignment
      // remain outside that internal try; the selected null guard surrounds it.
      edge(id,"exception-protected",0,initialize);
      Id cleanup=node("allocation-cleanup",e->position,selected.allocated_type,nullptr,
        nullptr,"",0,0,0,true);
      db.update("nodes","id",cleanup,{{"source_node_id",id},
        {"initialization_id",initialization_id(value->freeing_of_storage_on_exception,value->type)}});
      edge(id,"allocation-cleanup",0,cleanup);
    } else if(value->dynamic_init) {
      selected.zero_initialization=node("allocation-initialize",e->position,selected.allocated_type,
        nullptr,nullptr,"",0,0,0,true);
      db.update("nodes","id",selected.zero_initialization,{{"source_node_id",id},
        {"initialization_id",initialization_id(value->dynamic_init,value->type)}});
      edge(id,"initialize",0,selected.zero_initialization);
    }
  }
  Id allocation_expression(Allocation &selected,an_expr_node_ptr e) {
    if(!e)missing("selected allocation expression");
    if(e==selected.source_count)return selected.count_value;
    if(e->kind==enk_variable && node_variable(e)==selected.effective_count_temporary)
      return selected.effective_count_value;
    auto found=selected.projected.find(e);if(found!=selected.projected.end())return found->second;
    Id previous=function;bool previous_recipe=template_recipe;
    function=selected.owner;template_recipe=false;
    Id id=0;
    if(e->kind==enk_variable && node_variable(e)==selected.count_temporary) {
      id=node("variable",selected.position,type_id(e->type),e->is_lvalue?"lvalue":"prvalue",
        nullptr,"",selected.count_symbol,0,0,true);
    } else if(e->kind==enk_constant && node_constant(e)->kind==ck_integer) {
      id=constant(node_constant(e),selected.position);
    } else if(e->kind==enk_operation) {
      switch(e->variant.operation.kind) {
        case eok_cast:case eok_lvalue_cast:case eok_lvalue_adjust:
        case eok_multiply:case eok_subtract:case eok_divide:
        case eok_gt:case eok_lt:case eok_lor:break;
        default:missing("selected allocation size/check operation",int(e->variant.operation.kind));
      }
      id=node("operation",selected.position,type_id(e->type),"prvalue",operation(e->variant.operation.kind));
      unsigned ordinal=0;
      for(auto child=e->variant.operation.operands;child;child=child->next)
        edge(id,"operand",ordinal++,allocation_expression(selected,child));
    } else missing("selected allocation size/check expression",int(e->kind));
    selected.projected[e]=id;
    db.update("nodes","id",id,{{"source_node_id",selected.node},{"is_synthetic",true}});
    function=previous;template_recipe=previous_recipe;return id;
  }
  void allocation_size_begin(a_new_delete_supplement_ptr value) {
    allocation_stack.push_back(allocations.count(value)?value:nullptr);
  }
  void allocation_count_binding(a_variable_ptr temporary) {
    if(allocation_stack.empty() || !allocation_stack.back())return;
    auto &selected=allocations.at(allocation_stack.back());
    if(selected.count_temporary)missing("multiple allocation count bindings");
    selected.count_temporary=temporary;selected.count_symbol=next_symbol++;
    auto pos=location(selected.position);
    db.insert("symbols",{{"id",selected.count_symbol},{"kind","local"},
      {"name",spelling(temporary->source_corresp)},{"link_name",""},{"linkage","none"},
      {"type_id",type_id(temporary->type)},{"parent_symbol_id",selected.owner},{"is_definition",true},
      {"file_id",optional(pos.file)},{"line",pos.line},{"column",pos.column}});
  }
  void allocation_effective_count_binding(a_variable_ptr temporary,an_expr_node_ptr value) {
    if(allocation_stack.empty() || !allocation_stack.back())return;
    auto &selected=allocations.at(allocation_stack.back());
    if(!temporary || !value || selected.effective_count_temporary)
      missing("selected allocation effective count binding");
    // size_arg_for_new saves the already-checked/cast count a second time
    // when initialization needs a reusable element count. Its reads alias
    // that exact selected RHS; they do not reevaluate the source count.
    selected.effective_count_value=allocation_expression(selected,value);
    selected.effective_count_temporary=temporary;
  }
  void allocation_length_check(an_expr_node_ptr predicate,a_routine_ptr failure) {
    if(allocation_stack.empty() || !allocation_stack.back())return;
    auto &selected=allocations.at(allocation_stack.back());
    selected.check=allocation_expression(selected,predicate);selected.failure=routine(failure);
  }
  void allocation_size_end(a_new_delete_supplement_ptr value,an_expr_node_ptr size) {
    if(allocation_stack.empty())missing("allocation size hook nesting");
    if(allocation_stack.back()) {
      if(allocation_stack.back()!=value)missing("allocation size hook owner");
      auto &selected=allocations.at(value);selected.size=allocation_expression(selected,size);
    }
    allocation_stack.pop_back();
  }
  void allocation_construction_null_check(a_new_delete_supplement_ptr value,bool required) {
    auto found=allocations.find(value);
    if(found==allocations.end() || (!found->second.placement && !found->second.zero_initialization))return;
    auto &selected=found->second;
    if(selected.construction_null_check_observed && selected.construction_null_check!=required)
      missing("conflicting selected construction null check");
    selected.construction_null_check=required;selected.construction_null_check_observed=true;
  }
  void allocation_zeroing(a_new_delete_supplement_ptr value,a_variable_ptr byte_count) {
    auto found=allocations.find(value);if(found==allocations.end() || !found->second.zero_initialization)return;
    auto &selected=found->second;
    if(!byte_count || !selected.size || selected.zeroing_observed)
      missing("selected allocation byte-zero extent");
    // Stock lower_new passes var_rvalue_expr(num_bytes_temp) to its
    // runtime zeroing call. That temporary was assigned this captured size.
    edge(selected.zero_initialization,"byte-count",0,selected.size);
    selected.zeroing_observed=true;
  }
  void allocation_simple_result(a_new_delete_supplement_ptr value) {
    auto found=allocations.find(value);if(found==allocations.end())return;
    auto &selected=found->second;
    if(!selected.size || selected.complete)missing("selected allocation completion");
    if((selected.placement || selected.zero_initialization) && !selected.construction_null_check_observed)
      missing("allocation initialization without selected null-check decision (initialization may have been removed)");
    if(selected.zero_initialization && !selected.zeroing_observed)
      missing("scalar runtime-array initialization without observed stock byte-zero action");
    // This callback is at lower_new's final non-array change_to_cast. The
    // result is the allocation call's pointer, with no cookie adjustment.
    db.insert("allocations",{{"node_id",selected.node},{"allocated_type_id",selected.allocated_type},
      {"allocator_symbol_id",selected.allocator},{"count_value_node_id",optional(selected.count_symbol?selected.count_value:0)},
      {"count_symbol_id",optional(selected.count_symbol)},{"size_node_id",selected.size},
      {"length_check_node_id",optional(selected.check)},{"length_failure_symbol_id",optional(selected.failure)},
      {"result_byte_offset",0},{"construction_null_check",selected.placement || selected.zero_initialization?
        Value(selected.construction_null_check):Value(nullptr)}});
    selected.complete=true;
  }
  bool plain_deallocation(a_new_delete_supplement_ptr value) {
    return !value->is_new && value->routine && value->arg && !value->arg->next &&
      !value->dynamic_init && !value->freeing_of_storage_on_exception &&
      is_scalar_type(skip_typerefs(value->type));
  }
  bool record_deallocation(a_new_delete_supplement_ptr value) {
#if IA64_ABI
    auto type=skip_typerefs(value->type);auto init=value->dynamic_init;
    return !value->is_new && value->routine && value->arg && !value->arg->next &&
      !value->array_delete && !value->aligned_version && !value->freeing_of_storage_on_exception &&
      is_immediate_class_type(type) && !is_incomplete_type(type) &&
      init && init->kind==dik_none && init->destructor && !init->destructor->is_virtual &&
      is_non_throwing_routine(init->destructor) && !init->variable && !init->lifetime &&
      !init->init_expr_lifetime && !init->master_entry && !init->class_rvalue_initialized_through_master_entry;
#else
    return false;
#endif
  }
  void source_deallocation(Id id,an_expr_node_ptr e) {
    if(template_recipe || !function)missing("deallocation outside executable function");
    auto value=e->variant.new_delete;
    auto &selected=deallocations[value];
    selected.node=id;selected.owner=function;selected.region=literal_source_region;
    selected.position=e->position;selected.source_pointer=value->arg;
    selected.pointer_value=expression(value->arg);
    edge(id,"operand",0,selected.pointer_value);
    if(value->dynamic_init) {
      selected.source_destruction=value->dynamic_init;
      selected.destruction=node("destroy",e->position,type_id(value->type),nullptr,nullptr,"",0,0,0,true);
      db.update("nodes","id",selected.destruction,{{"source_node_id",id},
        {"initialization_id",initialization_id(value->dynamic_init,value->type)}});
      edge(id,"destroy",0,selected.destruction);
    }
  }
  Id deallocation_expression(Deallocation &selected,an_expr_node_ptr e) {
    if(!e)missing("selected deallocation argument");
    if(e==selected.source_pointer)return selected.pointer_value;
    auto found=selected.projected.find(e);if(found!=selected.projected.end())return found->second;
    Id id=0;
    if(e->kind==enk_constant && node_constant(e)->kind==ck_integer) {
      id=constant(node_constant(e),selected.position);
    } else if(e->kind==enk_operation && (e->variant.operation.kind==eok_cast ||
              e->variant.operation.kind==eok_lvalue_cast || e->variant.operation.kind==eok_lvalue_adjust)) {
      id=node("operation",selected.position,type_id(e->type),"prvalue",operation(e->variant.operation.kind));
      unsigned ordinal=0;
      for(auto child=e->variant.operation.operands;child;child=child->next)
        edge(id,"operand",ordinal++,deallocation_expression(selected,child));
    } else missing("selected deallocation argument expression",int(e->kind));
    selected.projected[e]=id;
    db.update("nodes","id",id,{{"source_node_id",selected.node},{"is_synthetic",true}});
    return id;
  }
  void deallocation_begin(a_new_delete_supplement_ptr value) {
    deallocation_stack.push_back(deallocations.count(value)?value:nullptr);
  }
  void deallocation_destruction(a_dynamic_init_ptr init,a_routine_ptr selected_routine,
      an_expr_node_ptr receiver,an_expr_node_ptr reused_pointer,bool null_check) {
    if(deallocation_stack.empty() || !deallocation_stack.back())return;
    auto &selected=deallocations.at(deallocation_stack.back());
    if(!selected.destruction || selected.destruction_observed || init!=selected.source_destruction ||
       !receiver || !reused_pointer || !selected_routine)
      missing("selected record delete destruction correspondence");
#if IA64_ABI
    if(selected_routine->ctor_dtor_kind!=cdk_complete || virtual_source(selected_routine)!=init->destructor)
      missing("selected record delete complete destructor entry");
#endif
    // make_reusable_copy supplies the same saved pointer for the destructor
    // and deallocator. Keep that alias to the captured source operand rather
    // than exporting lowering temporaries or reevaluating the source.
    selected.projected[receiver]=selected.pointer_value;
    selected.projected[reused_pointer]=selected.pointer_value;
    Id previous=function;bool previous_recipe=template_recipe;
    function=selected.owner;template_recipe=false;
    Id guard=node("constant",selected.position,type_id(bool_type()),"prvalue",nullptr,
      null_check?"true":"false",0,0,0,true);
    db.update("nodes","id",guard,{{"source_node_id",selected.node}});
    edge(selected.node,"null-check",0,guard);
    function=0;template_recipe=true;
    Id symbol=routine(selected_routine);
    Id abi=node("function",selected.position,routine_source_types.at(selected_routine),nullptr,
      "complete-destructor","",symbol);
    db.update("nodes","id",abi,{{"source_node_id",selected.destruction}});
    edge(selected.destruction,"selected-abi-entry",0,abi);
    function=previous;template_recipe=previous_recipe;
    selected.destruction_observed=true;
  }
  void deallocation_arguments(a_routine_ptr callee,an_expr_node_ptr arguments) {
    if(deallocation_stack.empty() || !deallocation_stack.back())return;
    auto &selected=deallocations.at(deallocation_stack.back());
    if(selected.execution)missing("multiple selected deallocator calls");
    Id previous=function;bool previous_recipe=template_recipe;
    function=selected.owner;template_recipe=false;
    Id symbol=routine(callee);
    Id call=node("operation",selected.position,type_id(skip_typerefs(callee->type)->variant.routine.return_type),
      "prvalue","call","",symbol,0,0,true);
    Id target=node("function",selected.position,routine_source_types.at(callee),"prvalue",nullptr,"",symbol,0,0,true);
    db.update("nodes","id",call,{{"source_node_id",selected.node}});
    db.update("nodes","id",target,{{"source_node_id",selected.node}});
    edge(call,"operand",0,target);
    unsigned ordinal=1;
    for(auto argument=arguments;argument;argument=argument->next)
      edge(call,"operand",ordinal++,deallocation_expression(selected,argument));
    // modify_delete_call_args supplies the exact pointer cast and any
    // sized/aligned arguments. A record delete's separately observed guard
    // surrounds its selected destruction followed by this deallocation.
    edge(selected.node,"execution",0,call);selected.execution=call;
    function=previous;template_recipe=previous_recipe;
  }
  void deallocation_end(a_new_delete_supplement_ptr value) {
    if(deallocation_stack.empty())missing("deallocation hook nesting");
    if(deallocation_stack.back() && (deallocation_stack.back()!=value || !deallocations.at(value).execution))
      missing("delete expression without selected deallocator call");
    if(deallocation_stack.back()) {
      auto &selected=deallocations.at(value);
      if(selected.destruction && !selected.destruction_observed)
        missing("record delete destruction was removed before selected guard/complete-entry observation");
    }
    deallocation_stack.pop_back();
  }
  Id vacuous_destructor_name(an_expr_node_ptr value,const a_source_position &pos,Id owner) {
    if(value->kind!=enk_routine || value->variant.routine.ptr || value->next)
      missing("vacuous destructor source-name operand");
    auto reference=value->variant.routine.name_reference;
    if(!reference || reference->special_kind!=sfk_none || reference->is_template_id ||
       reference->orig_template_arg_list || reference->is_super_qualified || reference->is_decltype_qualified)
      missing("vacuous destructor source-name form");
    Id previous=function;bool previous_recipe=template_recipe;
    function=0;template_recipe=true;
    auto named_type=reference->variant.destructor_type;
    const auto &position=value->position.seq?value->position:pos;
    Id id=node("reference-name",position,type_id(named_type),nullptr,
      reference->is_global_qualified_name?"global-destructor-name":"destructor-name",
      named_type?spelling(named_type->source_corresp):"");
    db.update("nodes","id",id,{{"source_node_id",owner}});
    std::vector<a_name_qualifier_ptr> qualifiers;
    for(auto qualifier=reference->qualifier;qualifier;qualifier=qualifier->previous_qualifier)
      qualifiers.push_back(qualifier);
    unsigned ordinal=0;
    for(auto qualifier=qualifiers.rbegin();qualifier!=qualifiers.rend();++qualifier) {
      auto selected=*qualifier;
      Id part=node("reference-name",position,selected->is_class?type_id(selected->qualifier.class_type):0,
        nullptr,selected->is_class?"class-qualifier":"namespace-qualifier",selected->name?selected->name:"");
      db.update("nodes","id",part,{{"source_node_id",owner}});
      edge(id,"qualifier",ordinal++,part);
    }
    function=previous;template_recipe=previous_recipe;
    return id;
  }
  Id expression(an_expr_node_ptr e,const a_source_position *inherited_position=nullptr,Id source_node=0,
                bool source_condition=false) {
    if(!e)return 0;
    auto &expression_ids=template_recipe?recipe_expressions:expressions;
    auto found=expression_ids.find(e);if(found!=expression_ids.end()) {
      if(source_condition)missing("constexpr condition wrapper already captured for execution");
      return found->second;
    }
    // Some stock folded/synthetic operands have no position of their own.
    // Retain their nearest enclosing expression's navigation and explicit
    // source link without changing EDG entries or reconstructing syntax.
    const auto &source_position=e->position.seq || !inherited_position?e->position:*inherited_position;
#if BUILTIN_FUNCTIONS_ENABLED
    if(e->kind==enk_operation && e->variant.operation.kind==eok_call) {
      auto callee=e->variant.operation.operands;
      auto selected=callee && callee->kind==enk_routine?callee->variant.routine.ptr:nullptr;
      if(selected && selected->special_kind==sfk_none &&
         selected->variant.builtin_function_kind==bfk_is_constant_evaluated) {
        if(callee->next)missing("constant-evaluation intrinsic arguments");
        // c_gen_be::dump_intrinsic_call emits typed zero for this residual
        // runtime query. There is no callable ABI or exception boundary.
        // A dependent source recipe records the selected query, not its
        // eventual value; EDG owns any later constant evaluation.
        Id id=node("operation",source_position,type_id(e->type),
          e->is_lvalue?"lvalue":e->is_xvalue?"xvalue":"prvalue",
          "is-constant-evaluated",template_recipe?"":"0",0,0,type_id(e->orig_lvalue_type),e->compiler_generated);
        expression_ids[e]=id;
        if(!e->position.seq && source_position.seq && source_node)
          db.update("nodes","id",id,{{"source_node_id",source_node}});
        Id previous=function;bool previous_recipe=template_recipe;
        function=0;template_recipe=true;
        Id name=node("reference-name",callee->position.seq?callee->position:source_position,
          0,nullptr,nullptr,spelling(selected->source_corresp));
        db.update("nodes","id",name,{{"source_node_id",id}});
        function=previous;template_recipe=previous_recipe;
        edge(id,"source-name",0,name);
        return id;
      }
    }
#endif
    const char *kind=nullptr,*op=nullptr;std::string text;Id symbol=0,field=0;
    switch(e->kind) {
      case enk_constant:
        if(node_constant(e)->kind==ck_template_param)kind=dependent_value_kind(node_constant(e),source_position);
        else if(node_constant(e)->kind==ck_string)kind="string-literal";
        else if(node_constant(e)->kind==ck_address)kind="address-constant";
        else if(node_constant(e)->kind==ck_aggregate)
          kind=template_recipe?"aggregate-initializer":is_immediate_class_type(skip_typerefs(e->type))?"record-initializer":"array-initializer";
        else {kind="constant";text=literal_text(node_constant(e),"expression",&e->position);}break;
      case enk_variable:
        if(selected_literals.count(node_variable(e)))kind="string-literal";
        else {kind="variable";symbol=variable(node_variable(e));}break;
      case enk_routine:kind="function";symbol=routine(e->variant.routine.ptr);break;
      case enk_field:kind="field";field=field_id(node_field(e));break;
      case enk_temp_init:kind="temporary";break;
      case enk_lambda: {
        auto selected=e->variant.init.source.lambda;
        if(!selected || !selected->closure_class || !e->variant.init.dynamic_init)
          missing("selected lambda closure/call initialization");
        // The closure type owns EDG's selected capture fields. Its existing
        // aggregate action initializes them; lower_lambda materializes this
        // same closure without interpreting the source capture list again.
        kind="lambda";
        if(selected->lambda_routine)symbol=routine(selected->lambda_routine);
        break;
      }
      case enk_condition:kind="condition";break;
      case enk_object_lifetime:kind="lifetime-scope";break;
      case enk_throw:kind=e->variant.throw_info?"throw":"rethrow";break;
      case enk_sizeof:case enk_alignof:case enk_datasizeof: {
        if(!template_recipe) {
          auto pos=location(source_position);std::string path;
          for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
          missing("runtime size/alignment expression [file="+path+", line="+std::to_string(pos.line)+
            ", column="+std::to_string(pos.column)+"]",int(e->kind));
        }
        kind="operation";
        op=e->kind==enk_sizeof?"sizeof":e->kind==enk_datasizeof?"datasizeof":
          e->variant.sizeof_info.is_std_alignof?"alignof":"gnu-alignof";
        break;
      }
      case enk_sizeof_pack: {
        auto &selected=e->variant.sizeof_pack;
        if(!template_recipe || selected.is_template_template) {
          auto pos=location(source_position);std::string path;
          for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
          missing("sizeof-pack selected reference [category="+
            std::string(selected.is_template_template?"template-template":selected.is_type?"type":"expression")+
            ", domain="+std::string(template_recipe?"template-recipe":"program")+
            ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]");
        }
        kind="sizeof-pack";op=selected.is_type?"type":"expression";break;
      }
      case enk_typeid:
#if MICROSOFT_EXTENSIONS_ALLOWED
        if(e->is_cli_typeid)missing("C++/CLI source typeid expression");
#endif
        if(template_recipe) {
          kind="operation";op=e->variant.typeid_info.is_dynamic?"dynamic-typeid":"typeid";break;
        }
        if(e->variant.typeid_info.is_dynamic)missing("dynamic source typeid expression");
        if(e->variant.typeid_info.type_with_opt_expr &&
           e->variant.typeid_info.type_with_opt_expr->next)
          missing("static source typeid expression operand correspondence");
        // lower_eh::lower_typeid selects the same static RTTI object and
        // views its public type_info prefix as the recorded source lvalue.
        kind="operation";op="indirect";break;
      case enk_new_delete: {
        auto allocation=e->variant.new_delete;
        if(plain_allocation(allocation)){kind="new";symbol=routine(allocation->routine);break;}
        if(placement_construction(allocation)) {
          kind="new";op="placement";symbol=routine(allocation->routine);break;
        }
        if(zero_array_allocation(allocation)) {
          kind="new";op="zero-initialized";symbol=routine(allocation->routine);break;
        }
        if(record_deallocation(allocation)) {
          kind="delete";op="destroy";symbol=routine(allocation->routine);break;
        }
        if(plain_deallocation(allocation)) {
          kind="delete";op=allocation->array_delete?"array":"scalar";symbol=routine(allocation->routine);break;
        }
        auto allocated_type=allocation->type?skip_typerefs(allocation->type):nullptr;
        auto element_type=allocated_type && allocated_type->kind==tk_array?
          skip_typerefs(allocated_type->variant.array.element_type):nullptr;
        auto init=allocation->dynamic_init;
        auto cleanup=allocation->freeing_of_storage_on_exception;
        auto pos=location(e->position);
        std::string path;for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        unsigned arguments=0;for(auto argument=allocation->arg;argument;argument=argument->next)++arguments;
        std::string detail="new/delete expression [operation="+std::string(allocation->is_new?"new":"delete")+
          ", array="+std::to_string(allocation->is_new?
            int(allocated_type && allocated_type->kind==tk_array):int(allocation->array_delete))+
          ", placement="+std::to_string(int(allocation->placement_new))+
          ", aligned="+std::to_string(int(allocation->aligned_version))+
          ", routine="+(allocation->routine?spelling(allocation->routine->source_corresp):"<none>")+
          ", type="+(allocated_type?spelling(allocated_type->source_corresp):"")+
          ", type-kind="+std::to_string(allocated_type?int(allocated_type->kind):-1)+
          ", type-size="+std::to_string(allocated_type?(unsigned long long)allocated_type->size:0)+
          ", element-type="+(element_type?spelling(element_type->source_corresp):"")+
          ", element-kind="+std::to_string(element_type?int(element_type->kind):-1)+
          ", arguments="+std::to_string(arguments)+
          ", runtime-count="+std::to_string(allocation->number_of_elements?1:0)+
          ", explicit-init="+std::to_string(int(allocation->has_new_initializer))+
          ", init-kind="+std::to_string(init?int(init->kind):-1)+
          ", constructor="+(init && init->kind==dik_constructor && init->variant.constructor.ptr?
            spelling(init->variant.constructor.ptr->source_corresp):"<none>")+
          ", destructor="+(init && init->destructor?spelling(init->destructor->source_corresp):"<none>")+
          ", destructor-virtual="+std::to_string(init && init->destructor?int(init->destructor->is_virtual):0)+
          ", destructor-nonthrowing="+std::to_string(init && init->destructor?int(is_non_throwing_routine(init->destructor)):0)+
          ", exception-delete="+(cleanup && cleanup->destructor?spelling(cleanup->destructor->source_corresp):"<none>")+
          ", exception-cleanup="+std::to_string(cleanup?1:0)+
          ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
        missing(detail,int(e->kind));
      }
      case enk_builtin_operation: {
        auto selected=e->variant.builtin_operation.kind;
        if(template_recipe) {
          // cp_gen_be prints this selected name and operand list directly.
          // Recipes retain the operation without realizing a trait value,
          // callable ABI, or runtime lowering decision.
          kind="operation";op=builtin_operation_names[selected];break;
        }
        if(selected==bok_builtin_addressof) {
          auto operand=e->variant.builtin_operation.operands;
          if(!operand || operand->next || !operand->is_lvalue)
            missing("builtin addressof selected lvalue operand");
          // Stock lower_builtin_addressof preserves this source result
          // type/category and applies ordinary address-of to this lvalue.
          kind="operation";op="address-of";break;
        }
        auto pos=location(source_position);std::string path,owner;
        for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        for(const auto &entry:routines)if(entry.second==function){owner=spelling(entry.first->source_corresp);break;}
        unsigned count=0;for(auto child=e->variant.builtin_operation.operands;child;child=child->next)++count;
        std::string detail="builtin expression [operation="+std::string(builtin_operation_names[selected])+
          ", opcode="+std::to_string(int(selected))+
          ", domain="+std::string(template_recipe?"template-recipe":"program")+
          ", function="+owner+", result-type="+(e->type?spelling(e->type->source_corresp):"")+
          ", result-kind="+std::to_string(e->type?int(skip_typerefs(e->type)->kind):-1)+
          ", operands="+std::to_string(count)+", file="+path+
          ", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
        unsigned ordinal=0;
        for(auto child=e->variant.builtin_operation.operands;child && ordinal<4;child=child->next,++ordinal) {
          auto selected_type=child->kind==enk_type_operand?child->variant.type_operand.type:child->type;
          auto child_pos=location(child->position);std::string child_path;
          for(const auto &file:files)if(file.second==child_pos.file){child_path=file.first;break;}
          detail+=" [operand="+std::to_string(ordinal)+", kind="+std::to_string(int(child->kind))+
            ", operator="+std::to_string(child->kind==enk_operation?int(child->variant.operation.kind):-1)+
            ", type="+(selected_type?spelling(selected_type->source_corresp):"")+
            ", type-kind="+std::to_string(selected_type?int(skip_typerefs(selected_type)->kind):-1)+
            ", file="+child_path+", line="+std::to_string(child_pos.line)+
            ", column="+std::to_string(child_pos.column)+"]";
        }
        missing(detail,int(e->kind));
      }
      case enk_operation: {
        if(!template_recipe && e->variant.operation.kind==eok_subscript && e->variant.operation.pointer_operand_is_second)
          missing("reversed subscript operand projection");
        kind="operation";op=operation(e->variant.operation.kind,e);
        auto first=e->variant.operation.operands;
        if(first && first->kind==enk_routine && first->variant.routine.ptr && (e->variant.operation.kind==eok_call ||
          e->variant.operation.kind==eok_dot_member_call || e->variant.operation.kind==eok_points_to_member_call))
          symbol=routine(first->variant.routine.ptr);
        break;
      }
      default: {
        auto pos=location(source_position);std::string path,owner;
        for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        for(const auto &entry:routines)if(entry.second==function){owner=spelling(entry.first->source_corresp);break;}
        missing("expression [domain="+std::string(template_recipe?"template-recipe":"program")+
          ", function="+owner+", file="+path+", line="+std::to_string(pos.line)+
          ", column="+std::to_string(pos.column)+"]",int(e->kind));
      }
    }
    Id id=node(kind,source_position,type_id(e->type),e->is_lvalue?"lvalue":e->is_xvalue?"xvalue":"prvalue",
      op,text,symbol,field,type_id(e->orig_lvalue_type),e->compiler_generated);
    expression_ids[e]=id;
    if(!e->position.seq && source_position.seq && source_node)
      db.update("nodes","id",id,{{"source_node_id",source_node}});
    if(e->kind==enk_typeid) {
      auto operand=e->variant.typeid_info.type_with_opt_expr;
      if(!operand || operand->kind!=enk_type_operand)missing("static typeid selected type operand");
      if(template_recipe) {
        unsigned ordinal=0;
        for(auto selected=operand;selected;selected=selected->next,++ordinal)
          edge(id,"operand",ordinal,recipe_operand(selected,source_position,id));
      } else {
        Id address=node("type-info-address",source_position,type_id(make_pointer_type(e->type)),"prvalue",
          nullptr,"",0,0,0,true);
        db.update("nodes","id",address,{{"source_node_id",id}});
        attach_typeinfo_target(address,operand->variant.type_operand.type,source_position);
        edge(id,"operand",0,address);
      }
    }
    if(e->kind==enk_new_delete) {
      if(e->variant.new_delete->is_new)source_allocation(id,e);
      else source_deallocation(id,e);
    }
    if(e->kind==enk_variable && selected_literals.count(node_variable(e))) {
      db.update("nodes","id",id,{{"literal_object_id",selected_literal_object(node_variable(e))}});
    }
    // Source-form predicate for lower_il::rewrite_nullptr_expr_if_necessary.
    // lower_expr first changes xvalues to lvalues, so both glvalue forms are
    // excluded here. Retain the original children for evaluation/side effects;
    // the explicit result rule replaces the value, not the storage location.
    if(!e->is_lvalue && !e->is_xvalue && is_standard_nullptr_type(e->type) &&
       e->kind!=enk_constant && e->kind!=enk_field && !e->result_is_not_used)
      db.update("nodes","id",id,{{"value_normalization","nullptr-zero"}});
    if(e->kind==enk_constant) {
      if(node_constant(e)->kind==ck_string || node_constant(e)->kind==ck_address)attach_literal(id,node_constant(e),source_position);
      if(node_constant(e)->kind==ck_template_param)dependent_value(id,node_constant(e),source_position);
      if(node_constant(e)->kind==ck_aggregate)aggregate_initializer(id,node_constant(e),e->position);
      attach_enum_value(id,node_constant(e));
      if(node_constant(e)->kind==ck_float)
        db.update("nodes","id",id,{{"floating_value_id",floating_value(node_constant(e))}});
    }
    if(e->kind==enk_sizeof || e->kind==enk_alignof || e->kind==enk_datasizeof) {
      auto &selected=e->variant.sizeof_info;
      query_operand(id,selected.is_type?selected.variant.type:nullptr,
        selected.is_type?nullptr:selected.variant.expr,source_position);
    }
    if(e->kind==enk_sizeof_pack) {
      // cp_gen_be retains exactly this selected pack operand. It supplies
      // source navigation only; EDG owns any eventual pack-size evaluation.
      auto &selected=e->variant.sizeof_pack;
      query_operand(id,selected.is_type?selected.variant.type:nullptr,
        selected.is_type?nullptr:selected.variant.expr,source_position);
    }
    if(e->kind==enk_operation) {
      bool vacuous=e->variant.operation.kind==eok_dot_vacuous_destructor_call ||
        e->variant.operation.kind==eok_points_to_vacuous_destructor_call;
      if(vacuous && !e->variant.operation.operands)missing("vacuous destructor object operand");
      if(!template_recipe && is_compound_assignment_operator(e->variant.operation.kind))
        // The stock helper handles promotions (especially the promoted left
        // operand of a shift). This is distinct from the stored/result type
        // and does not ask the consumer to re-run arithmetic conversions.
        db.update("nodes","id",id,{{"operation_type_id",type_id(compound_assignment_operation_type(e))}});
      if(!template_recipe && e->variant.operation.is_virtual_call)virtual_call(id,e);
      if(!template_recipe && (e->variant.operation.kind==eok_base_class_cast || e->variant.operation.kind==eok_derived_class_cast))
        class_conversion(id,e);
      db.update("nodes","id",id,{{"evaluation_order",e->variant.operation.eval_left_to_right?"left-to-right":
        e->variant.operation.eval_right_to_left?"right-to-left":"unspecified"}});
      unsigned ordinal=0;for(auto child=e->variant.operation.operands;child;child=child->next,++ordinal) {
        // scan_va_start_operator retains the source last-parameter anchor.
        // It supplies no argument value to the portable pack; GNU explicitly
        // ignores it (il::lvalue/rvalue operand checks). Keep it navigable
        // without asking generic operand expansion to evaluate it.
        if(template_recipe)
          edge(id,"operand",ordinal,recipe_operand(child,source_position,id));
        else if(vacuous && ordinal==1)
          // Stock lower_il evaluates only the object/pointer. A trailing
          // NULL-routine operand describes the source destructor name.
          edge(id,"source-name",0,vacuous_destructor_name(child,source_position,id));
        else if(e->variant.operation.kind==eok_va_start && ordinal==1)
          edge(id,"last-parameter",0,expression(child,&source_position,id));
        else edge(id,"operand",ordinal,expression(child,&source_position,id));
      }
    } else if(e->kind==enk_builtin_operation) {
      if(template_recipe) {
        unsigned ordinal=0;
        for(auto child=e->variant.builtin_operation.operands;child;child=child->next,++ordinal) {
          Id operand=0;
          if(child->kind==enk_type_operand) {
            if(!child->variant.type_operand.type)missing("builtin recipe selected type operand");
            operand=node("type-reference",child->position.seq?child->position:source_position,
              type_id(child->variant.type_operand.type));
            db.update("nodes","id",operand,{{"source_node_id",id}});
          } else operand=expression(child,&source_position,id);
          edge(id,"operand",ordinal,operand);
        }
      } else {
        edge(id,"operand",0,expression(e->variant.builtin_operation.operands,&source_position,id));
        Id previous=function;bool previous_recipe=template_recipe;
        function=0;template_recipe=true;
        Id name=node("reference-name",source_position,0,nullptr,nullptr,
          builtin_operation_names[e->variant.builtin_operation.kind]);
        db.update("nodes","id",name,{{"source_node_id",id}});
        function=previous;template_recipe=previous_recipe;
        edge(id,"source-name",0,name);
      }
    } else if(e->kind==enk_temp_init || e->kind==enk_lambda) {
      db.update("nodes","id",id,{{"initialization_id",initialization_id(e->variant.init.dynamic_init,e->type)}});
    } else if(e->kind==enk_condition) {
      auto condition=e->variant.condition;
      // lower_il::lower_condition keeps this scope around the complete
      // selection, or around each loop iteration including the for step.
      if(condition->scope) {
        if(has_static_storage(condition->scope))missing("condition static storage projection");
        attach_lifetime(id,condition->scope->lifetime);
        for(auto v=condition->scope->nonstatic_variables;v;v=v->next)variable(v,true);
      }
      if(condition->dynamic_init)
        db.update("nodes","id",id,{{"initialization_id",initialization_id(condition->dynamic_init)}});
      // A C++17 selection initializer is a freestanding statement: EDG
      // deliberately supplies no adjacent stmk_init for its declarations.
      edge(id,"initializer",0,statement(condition->initialization,nullptr,true));
      if(source_condition)
        edge(id,"source-condition",0,recipe_expression(condition->expr,source_position,id));
      else edge(id,"value",0,expression(condition->expr));
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
    auto persistent=static_local_symbols.find(v);
    if(persistent!=static_local_symbols.end())
      return node("static-declaration",pos,type_id(v->type),nullptr,nullptr,"",persistent->second);
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
  Id statement(a_statement_ptr s,a_scope_ptr root_scope=nullptr,bool condition_initializer=false) {
    if(!s)return 0;
    if(s->kind==stmk_switch_case || s->kind==stmk_label) {
      auto found=source_statement_nodes.find(s);
      if(found!=source_statement_nodes.end())return found->second;
      if(s->kind==stmk_switch_case)missing("switch case without selected enclosing switch");
    }
    if(s->kind==stmk_init && declaration_initializers.count(s->variant.dynamic_init))return 0;
    auto missing_label=[&](const char *reason) {
      auto label=s->variant.label.ptr;
      auto pos=location(s->position);
      auto target_pos=location(label->exec_stmt?label->exec_stmt->position:label->source_corresp.decl_position);
      std::string path,target_path,name;
      for(const auto &file:files) {
        if(file.second==pos.file)path=file.first;
        if(file.second==target_pos.file)target_path=file.first;
      }
      for(const auto &entry:routines)if(entry.second==function){name=spelling(entry.first->source_corresp);break;}
      missing(std::string(reason)+" [function="+name+", label="+spelling(label->source_corresp)+
        ", common-lifetime-kind="+std::to_string(s->variant.label.lifetime?int(s->variant.label.lifetime->kind):-1)+
        ", target-statement-kind="+std::to_string(label->exec_stmt?int(label->exec_stmt->kind):-1)+
        ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+
        ", target-file="+target_path+", target-line="+std::to_string(target_pos.line)+
        ", target-column="+std::to_string(target_pos.column)+"]",int(s->kind));
    };
    const char *kind=nullptr;
    switch(s->kind) {
      case stmk_block:kind="block";break;case stmk_return:kind="return";break;
      case stmk_expr:kind="expr-stmt";break;case stmk_if:kind="if";break;
      case stmk_constexpr_if:kind="constexpr-if";break;
      case stmk_while:kind="while";break;case stmk_end_test_while:kind="do-while";break;
      case stmk_for:kind="for";break;case stmk_decl:kind="decl";break;case stmk_init:kind="decl";break;
      case stmk_range_based_for:kind="range-for";break;
      case stmk_switch:kind="switch";break;
      case stmk_try_block:kind="try";break;
      case stmk_goto:
        if(s->variant.label.ptr->continue_label)kind="continue";
        else if(s->variant.label.ptr->break_label)kind="break";
        else kind="goto";break;
      case stmk_label:
        if(s->variant.label.ptr->continue_label || s->variant.label.ptr->break_label)kind="empty";
        else kind="label";break;
      case stmk_empty:kind="empty";break;
      default: {
        auto pos=location(s->position);std::string path,name;
        for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        for(const auto &entry:routines)if(entry.second==function){name=spelling(entry.first->source_corresp);break;}
        missing("statement [function="+name+", file="+path+", line="+std::to_string(pos.line)+
          ", column="+std::to_string(pos.column)+"]",int(s->kind));
      }
    }
    Id id=node(kind,s->position);
    if(s->kind==stmk_label)source_statement_nodes[s]=id;
    switch(s->kind) {
      case stmk_goto:
        if(std::string(kind)=="goto") {
          auto label=s->variant.label.ptr;
          if(!label->exec_stmt || label->exec_stmt->kind!=stmk_label)
            missing_label("named goto without selected label definition");
          edge(id,"target",0,statement(label->exec_stmt));
          // The exact common lifetime is the boundary for this transfer,
          // distinct from a lifetime region owned by the statement.
          db.update("nodes","id",id,{{"common_lifetime_id",optional(lifetime_id(s->variant.label.lifetime))}});
        }
        break;
      case stmk_label:
        if(std::string(kind)=="label")
          db.update("nodes","id",id,{{"constant_text",spelling(s->variant.label.ptr->source_corresp)},
            {"containing_lifetime_id",optional(lifetime_id(s->variant.label.lifetime))}});
        break;
      case stmk_switch: {
        // Reserve the selected case statements before traversing the body:
        // the dispatch list and the lexical body reference the same source
        // nodes. EDG's numeric sorted list is not part of the program graph.
        unsigned ordinal=0;
        for(auto entry=s->variant.switch_stmt.extra_info->cases;entry;entry=entry->next) {
          Id selected=node(entry->case_value?"case":"default",entry->stmt->position);
          source_statement_nodes[entry->stmt]=selected;
          edge(id,"case",ordinal++,selected);
          if(entry->case_value)edge(selected,"value",0,constant(entry->case_value,entry->position));
#if GNU_EXTENSIONS_ALLOWED
          if(entry->range_end)edge(selected,"range-end",0,constant(entry->range_end,entry->position));
#endif
        }
        edge(id,"condition",0,expression(s->expr));
        edge(id,"body",0,statement(s->variant.switch_stmt.body_statement));
        break;
      }
      case stmk_block: {
        auto scope=root_scope?root_scope:s->variant.block.extra_info->assoc_scope;
        if(scope && has_static_storage(scope))missing("block static storage projection");
        attach_lifetime(id,scope?scope->lifetime:s->variant.block.extra_info->lifetime);
        if(scope)for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
        unsigned ordinal=0;
        if(root_scope && scope->kind==sck_function &&
           scope->variant.routine.ptr->special_kind==sfk_lambda_entry_point) {
          // The stock generated helper owns these storage variables even
          // though its body has assignments/returns without stmk_decl nodes.
          // Preserve the selected storage, without adding initialization.
          for(auto v=scope->nonstatic_variables;v;v=v->next)if(!v->is_parameter) {
            if(!v->lowering_generated || v->init_kind!=initk_none)
              missing("lambda entry generated storage declaration");
            const auto &pos=v->source_corresp.decl_position.seq?v->source_corresp.decl_position:
              scope->variant.routine.ptr->source_corresp.decl_position;
            edge(id,"statement",ordinal++,declaration(v,pos,false));
          }
        }
        for(auto child=s->variant.block.statements;child;child=child->next)
          edge(id,"statement",ordinal++,statement(child));break;
      }
      case stmk_return:
        if(s->variant.return_dynamic_init) {
          // lower_il::lower_return_statement applies this exact selected
          // initialization to the returned object's destination. EDG has
          // already performed required elision; retain the selected action.
          db.update("nodes","id",id,{{"type_id",type_id(return_type)},
            {"initialization_id",initialization_id(s->variant.return_dynamic_init,return_type)}});
        } else edge(id,"value",0,expression(s->expr));break;
      case stmk_expr:edge(id,"value",0,expression(s->expr));break;
      case stmk_if:
        edge(id,"condition",0,expression(s->expr));edge(id,"then",0,statement(s->variant.if_stmt.then_statement));
        edge(id,"else",0,statement(s->variant.if_stmt.else_statement));break;
      case stmk_constexpr_if: {
        auto selected=s->variant.constexpr_if;
        if(!selected->value_known)missing("dependent constexpr-if outside source recipe");
        auto discarded=selected->value?selected->else_statement:selected->then_statement;
        if(discarded && discarded->kind!=stmk_empty)
          missing("constexpr-if nonempty discarded source branch navigation");
        db.update("nodes","id",id,{{"constant_text",selected->value?"true":"false"}});
        // The stock constexpr-if result selects execution. Its source
        // predicate is navigation-only even when it remains a constexpr call.
        // A C++17 initializer still owns ordinary runtime actions and cleanup.
        if(s->expr && s->expr->kind==enk_condition)
          edge(id,"condition",0,expression(s->expr,&s->position,id,true));
        else edge(id,"source-condition",0,recipe_expression(s->expr,s->position,id));
        Id then_node=statement(selected->then_statement),else_node=statement(selected->else_statement);
        edge(id,"then",0,then_node);edge(id,"else",0,else_node);
        edge(id,"selected-body",0,selected->value?then_node:else_node);
        break;
      }
      case stmk_while:case stmk_end_test_while:
        edge(id,"condition",0,expression(s->expr));edge(id,"body",0,statement(s->variant.loop_statement));break;
      case stmk_for: {
        auto scope=s->variant.for_loop.extra_info->for_init_scope;
        if(scope) {
          if(has_static_storage(scope))missing("for initializer static storage projection");
          // This lifetime surrounds the whole loop; the condition node owns
          // the separate lifetime entered and left on every iteration.
          attach_lifetime(id,scope->lifetime);
          for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
        }
        edge(id,"initializer",0,statement(s->variant.for_loop.extra_info->initialization));
        edge(id,"condition",0,expression(s->expr));edge(id,"step",0,expression(s->variant.for_loop.extra_info->increment));
        edge(id,"body",0,statement(s->variant.for_loop.statement));break;
      }
      case stmk_range_based_for: {
        auto range=s->variant.range_based_for_loop.extra_info;
        if(range->use_await)missing("await range-for projection");
        auto outer=range->range_based_for_scope,iteration=range->iterator_scope;
        if(!outer || !iteration || !range->range || !range->begin || !range->end || !range->iterator ||
           !range->ne_call_expr || !range->incr_call_expr)missing("selected range-for source facts");
        if(has_static_storage(outer) || has_static_storage(iteration))missing("range-for static storage projection");
        // These are the exact scopes and ordered actions consumed by stock
        // lower_il::lower_range_based_for_statement. The selected variables
        // deliberately have no stmk_init statements in source IL.
        attach_lifetime(id,outer->lifetime);
        for(auto v=outer->nonstatic_variables;v;v=v->next)variable(v,true);
        edge(id,"initializer",0,statement(range->initialization,nullptr,true));
        auto binding=[&](a_variable_ptr v,const char *role) {
          const auto &pos=v->source_corresp.decl_position.seq?v->source_corresp.decl_position:s->position;
          Id action=declaration(v,pos);
          db.update("nodes","id",action,{{"source_node_id",id},{"is_synthetic",true}});
          edge(id,role,0,action);
        };
        binding(range->range,"range");binding(range->begin,"begin");binding(range->end,"end");
        edge(id,"condition",0,expression(range->ne_call_expr));
        edge(id,"step",0,expression(range->incr_call_expr));
        Id body=node("block",s->position,0,nullptr,nullptr,"",0,0,0,true);
        db.update("nodes","id",body,{{"source_node_id",id}});
        attach_lifetime(body,iteration->lifetime);
        for(auto v=iteration->nonstatic_variables;v;v=v->next)variable(v,true);
        const auto &pos=range->iterator->source_corresp.decl_position.seq?
          range->iterator->source_corresp.decl_position:s->position;
        edge(body,"statement",0,declaration(range->iterator,pos));
        edge(body,"statement",1,statement(s->variant.range_based_for_loop.statement));
        // Leaving this block (also by continue) performs iteration cleanup.
        // The selected increment is outside it; the outer lifetime surrounds
        // the entire loop and handles break, return, and exceptional exits.
        edge(id,"body",0,body);break;
      }
      case stmk_decl: {
        unsigned ordinal=0;
        for(auto item=s->variant.decl.entities;item;item=item->next) {
          if(item->entity.kind==iek_variable) {
            auto v=(a_variable_ptr)item->entity.ptr;
            // EDG retains the declaration and its immediately following
            // executable initialization separately. Project that exact pair
            // as the original source declaration with one initializer.
            // Standalone C++17 selection initializers instead use the
            // variable's recorded initializer directly, like stock
            // lower_il::make_init_statements_for_entity_list.
            if(v->init_kind==initk_dynamic) {
              if(!condition_initializer && (!s->next || s->next->kind!=stmk_init ||
                 s->next->variant.dynamic_init!=v->initializer.dynamic))
                missing("nonadjacent declaration/initialization projection");
              if(!condition_initializer)declaration_initializers.insert(v->initializer.dynamic);
            }
            edge(id,"declaration",ordinal++,declaration(v,s->position));
          }
          else if(item->entity.kind==iek_type)type_id((a_type_ptr)item->entity.ptr);
          else if(item->entity.kind==iek_static_assertion) {
            auto assertion=(a_static_assertion_ptr)item->entity.ptr;
            const auto &position=assertion->position.seq?assertion->position:s->position;
            Id selected=node("static-assertion",position);
            // EDG has resolved this source declaration. Its selected condition
            // and optional message are navigation recipes, never executable
            // operands or runtime literal storage requirements.
            if(!assertion->condition)missing("selected static assertion condition");
            edge(selected,"condition",0,recipe_value(assertion->condition,position));
            if(assertion->string_literal)
              edge(selected,"message",0,recipe_value(assertion->string_literal,position));
            edge(id,"declaration",ordinal++,selected);
          }
          else {
            a_source_position position=s->position;
            auto source=location(position);std::string path,name;
            for(const auto &file:files)if(file.second==source.file){path=file.first;break;}
            for(const auto &entry:routines)if(entry.second==function){name=spelling(entry.first->source_corresp);break;}
            std::string detail="local declaration [function="+name+
              ", file="+path+", line="+std::to_string(source.line)+
              ", column="+std::to_string(source.column);
            missing(detail+"]",int(item->entity.kind));
          }
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
    if(has_static_storage(scope))missing("local static storage projection");
    for(auto v=scope->variables;v;v=v->next)
      if(!empty_pack_parameter(v))static_local(scope,v);
    for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
    for(auto child=scope->scopes;child;child=child->next)local_bindings(child);
  }
  bool bounded_default_constant(a_field_ptr member) {
    auto init=member->initializer;
    if(member->init_is_ctor_dependent || !init || init->kind!=dik_constant ||
       init->variant.constant.non_constant ||
       init->variable || init->destructor || init->lifetime || init->init_expr_lifetime ||
       init->lifetime_of_overlapping_temps || init->master_entry ||
       init->class_rvalue_initialized_through_master_entry || init->unordered ||
       init->overlaps_temps_in_inner_lifetime ||
       init->destruction_is_for_partially_constructed_aggregate ||
       init->is_freeing_of_storage_on_exception)return false;
    auto value=init->variant.constant.ptr;
    return value && ((is_scalar_type(member->type) &&
      (value->kind==ck_integer || value->kind==ck_float)) ||
      (is_immediate_class_type(skip_typerefs(member->type)) && value->kind==ck_aggregate &&
       !value->variant.aggregate.has_dynamic_init_component));
  }
  bool bounded_default_constructor(a_field_ptr member) {
    auto init=member->initializer;
    if(member->init_is_ctor_dependent || !is_immediate_class_type(skip_typerefs(member->type)) ||
       !init || init->kind!=dik_constructor ||
       !init->variant.constructor.ptr || init->variant.constructor.is_array_copy ||
       init->variant.constructor.is_copy_constructor_with_implied_source ||
       init->variable || init->lifetime || init->init_expr_lifetime ||
       init->lifetime_of_overlapping_temps || init->master_entry ||
       init->class_rvalue_initialized_through_master_entry || init->unordered ||
       init->overlaps_temps_in_inner_lifetime ||
       init->destruction_is_for_partially_constructed_aggregate ||
       init->is_freeing_of_storage_on_exception)return false;
    for(auto arg=init->variant.constructor.args;arg;arg=arg->next) {
      if(arg->kind!=enk_constant || !is_scalar_type(arg->type))return false;
      auto value=arg->variant.constant.ptr;
      if(value->kind!=ck_integer && value->kind!=ck_float)return false;
    }
    return true;
  }
  void default_member_initializers_complete(a_scope_ptr scope) {
    auto found=default_member_initializers.find(scope);
    if(found==default_member_initializers.end())return;
    auto &pending=found->second;
    // copy_non_static_data_member_initializers_if_necessary has installed
    // each exact per-constructor copy and finished interleaving destructions.
    // Only identity correspondence survives the source capture; no expression
    // cloning or cleanup ordering is performed by the exporter.
    for(const auto &entry:pending.selected) {
      auto copied=entry.first->initializer;
      if(!copied)missing("eliminated default member constructor initialization");
      if(copied->kind!=dik_constructor || !copied->is_constructor_init ||
         copied->variant.constructor.is_array_copy ||
         copied->variant.constructor.is_copy_constructor_with_implied_source ||
         copied->variable || copied->init_expr_lifetime ||
         copied->lifetime_of_overlapping_temps || copied->master_entry ||
         copied->class_rvalue_initialized_through_master_entry || copied->unordered ||
         copied->overlaps_temps_in_inner_lifetime ||
         copied->destruction_is_for_partially_constructed_aggregate ||
         copied->is_freeing_of_storage_on_exception ||
         (copied->lifetime && copied->lifetime!=scope->lifetime))
        missing("copied default member constructor requires richer cleanup projection");
      pending.init_ids[copied]=entry.second;
    }
    Id previous=function;function=pending.owner;
    initializations.swap(pending.init_ids);lifetimes.swap(pending.lifetime_ids);
    if(scope->lifetime)deferred_destructions.insert(scope->lifetime);
    Id life=lifetime_id(scope->lifetime);
    for(const auto &entry:pending.selected) {
      auto copied=entry.first->initializer;
      db.update("initializations","id",entry.second,{
        {"is_constructor_init",bool(copied->is_constructor_init)},
        {"lifetime_id",copied->lifetime?Value(life):Value(nullptr)},
        {"conditional",bool(copied->inside_conditional_expression)},
        {"unordered",bool(copied->unordered)},
        {"overlaps_inner",bool(copied->overlaps_temps_in_inner_lifetime)},
        {"partial_aggregate",bool(copied->destruction_is_for_partially_constructed_aggregate)},
        {"free_on_exception",bool(copied->is_freeing_of_storage_on_exception)},
        {"is_partially_initialized",bool(copied->is_partially_initialized)},
        {"static_storage",bool(copied->static_temp)},
        {"implied_source",bool(copied->variant.constructor.is_copy_constructor_with_implied_source)},
        {"sequenced_arguments",bool(copied->variant.constructor.has_sequenced_arguments)},
        {"value_initialization",bool(copied->variant.constructor.value_initialization)},
        {"follows_exec",bool(copied->follows_an_exec_statement)},
        {"temporary_lifetime",bool(copied->has_temporary_lifetime)}});
    }
    if(scope->lifetime) {
      attach_lifetime(pending.body,scope->lifetime);
      unsigned ordinal=0;
      for(auto action=scope->lifetime->destructions;action;action=action->next_in_destruction_list) {
        auto selected=initializations.find(action);
        if(selected==initializations.end())
          missing("copied default member constructor introduced uncaptured destruction");
        db.insert("lifetime_destructions",{{"lifetime_id",life},{"ordinal",ordinal++},
          {"initialization_id",selected->second}});
      }
      deferred_destructions.erase(scope->lifetime);
    }
    // Stock can unbind a newly useless lifetime. Never retain either its
    // earlier identity or its replacement beyond the owning function region.
    deferred_destructions.erase(pending.original_root);
    initializations.swap(pending.init_ids);lifetimes.swap(pending.lifetime_ids);
    function=previous;default_member_initializers.erase(found);
  }
  void constructor_initializers(a_scope_ptr scope) {
    // Seed selected base destinations before any lifetime traversal can reach
    // another initializer in this constructor's list.
    for(auto entry=scope->variant.routine.constructor_inits;entry;entry=entry->next)
      if(entry->kind==cik_direct_base_class || entry->kind==cik_virtual_base_class)
        subobject_initializations.insert(entry->initializer);
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
      auto init=entry->initializer;
      if(!init && entry->kind==cik_field && entry->use_field_initializer) {
        auto member=entry->variant.field;
        auto selected=member->initializer;
        // Exact lower_init::lower_ctor_init selection. Scalar constants need
        // no remapping. Class construction additionally waits for the stock
        // per-constructor copy to supply its cleanup lifetime and ordering.
        // EDG-selected immutable record aggregates use the same direct
        // constant action; their field/base/zero writes are already recorded.
        if(bounded_default_constant(member))init=selected;
        else if(bounded_default_constructor(member))init=selected;
      }
      if(!init) {
        auto member=entry->kind==cik_field?entry->variant.field:nullptr;
        auto selected=member?member->initializer:nullptr;
        auto owner=member?parent_class_of(member):nullptr;
        auto pos=location(member?member->source_corresp.decl_position:
          scope->variant.routine.ptr->source_corresp.decl_position);
        std::string path;for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        std::string detail="default field initializer selection requires scalar/record constant or constructor with scalar constant arguments [constructor="+
          spelling(scope->variant.routine.ptr->source_corresp)+", owner="+
          (owner?spelling(owner->source_corresp):"")+", field="+
          (member?spelling(member->source_corresp):"")+", type="+
          (target?spelling(target->source_corresp):"")+", type-kind="+
          std::to_string(target?int(target->kind):-1)+", ordinal="+std::to_string(ordinal)+
          ", use-field="+std::to_string(int(entry->use_field_initializer))+
          ", ctor-dependent="+std::to_string(member?int(member->init_is_ctor_dependent):0)+
          ", init-kind="+std::to_string(selected?int(selected->kind):-1);
        if(selected && selected->kind==dik_constant)
          detail+=", constant-kind="+std::to_string(int(selected->variant.constant.ptr->kind));
        if(selected && selected->kind==dik_expression)
          detail+=", expression-kind="+std::to_string(int(selected->variant.expression->kind));
        detail+=", destructor="+std::to_string(selected && selected->destructor?1:0)+
          ", lifetime="+std::to_string(selected && selected->lifetime?1:0)+
          ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
        missing(detail);
      }
      if(entry->source_expr)missing("explicit array constructor initializer source");
      Id copy_source=0;
      bool implied=(init->kind==dik_bitwise_copy && !init->variant.bitwise_copy.source) ||
        (init->kind==dik_constructor && init->variant.constructor.is_copy_constructor_with_implied_source);
      if(implied) {
        // Source-form equivalent of lower_init::var_for_copy_constructor_source:
        // before lowering the receiver is separate and there are no added
        // VTT/virtual-base parameters. The first explicit parameter supplies
        // the object; this same selected ctor-initializer selects its member.
        auto source=scope->variant.routine.parameters;
        if(source && source==scope->variant.routine.this_param_variable)source=source->next;
        if(!source)missing("selected constructor copy source parameter");
        copy_source=variable(source,true,true);
      }
      Id selected_init=initialization_id(init,target);
      if(!entry->initializer && init->kind==dik_constructor)
        default_member_initializers.at(scope).selected[entry]=selected_init;
      db.insert("constructor_initializers",{{"function_id",function},{"ordinal",ordinal},{"kind",kind},
        {"field_id",optional(field)},{"base_subobject_id",optional(base)},
        {"copy_source_symbol_id",optional(copy_source)},
        {"target_type_id",optional(type_id(target))},{"is_virtual",is_virtual},
        {"initialization_id",selected_init}});
    }
  }
  void capture(a_scope_ptr scope) {
    if(!scope)return;
    if(scope->kind==sck_function) {
      auto r=scope->variant.routine.ptr;
      if(r->is_prototype_instantiation || !scope->assoc_block || !bodies.insert(r).second)return;
      if(has_static_storage(scope)) {
        std::string detail="function static storage projection in "+spelling(r->source_corresp);
        for(auto v=scope->variables;v;v=v->next)
          detail+=" ["+spelling(v->source_corresp)+", storage="+std::to_string(int(v->storage_class))+
            ", init="+std::to_string(int(v->init_kind))+", pack="+std::to_string(int(v->is_pack))+
            ", generated="+std::to_string(int(v->compiler_generated))+"]";
        missing(detail);
      }
      Id previous=function;auto previous_return_type=return_type;
      auto previous_literal_region=literal_source_region;
      literal_source_region=mem_region_for_routine(r);
      return_type=skip_typerefs(r->type)->variant.routine.return_type;
      function=routine(r);expressions.clear();source_statement_nodes.clear();
      db.update("symbols","id",function,{{"is_definition",true}});
      unsigned empty_ordinal=0;
      for(auto v=scope->variables;v;v=v->next)if(empty_pack_parameter(v)) {
        auto pos=location(v->source_corresp.decl_position);
        db.insert("empty_parameter_packs",{{"function_id",function},{"ordinal",empty_ordinal++},
          {"name",spelling(v->source_corresp)},{"file_id",optional(pos.file)},
          {"line",pos.line},{"column",pos.column}});
      }
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
      // Defer before any initializer/body traversal can reach the root
      // lifetime. EDG will insert the copied default members among its
      // existing explicit member and local destruction entries.
      for(auto entry=scope->variant.routine.constructor_inits;entry;entry=entry->next)
        if(!entry->initializer && entry->kind==cik_field && entry->use_field_initializer &&
           bounded_default_constructor(entry->variant.field)) {
          default_member_initializers[scope].owner=function;
          default_member_initializers[scope].original_root=scope->lifetime;
          if(scope->lifetime)deferred_destructions.insert(scope->lifetime);
          break;
        }
      local_bindings(scope);
      constructor_initializers(scope);
      lifetime_id(scope->lifetime);
      Id body=statement(scope->assoc_block,scope);
      const char *policy=exception_policy(r->type);
      if(std::string(policy)=="dependent")missing("finalized function exception policy");
      db.update("types","id",routine_source_types[r],{{"exception_policy",policy},
        {"return_storage",skip_typerefs(r->type)->variant.routine.extra_info->value_returned_by_cctor?"caller-provided":"value"}});
      // A previously cached declaration can have a deferred specification.
      // Keep its same source signature and replace only the finalized list.
      db.exec("DELETE FROM exception_specification_types WHERE function_type_id="+std::to_string(routine_source_types[r]));
      if(std::string(policy)=="dynamic")exception_specification_types(routine_source_types[r],r->type);
      const char *kind=r->special_kind==sfk_constructor?"constructor":r->special_kind==sfk_destructor?"destructor":"ordinary";
      db.insert("functions",{{"symbol_id",function},{"body_node_id",body},{"receiver_symbol_id",optional(receiver)},
        {"kind",kind},{"exception_policy",policy},
        {"nrvo_candidate_symbol_id",optional(scope->variant.routine.return_value_variable?
          variable(scope->variant.routine.return_value_variable,true):0)}});
      auto pending=default_member_initializers.find(scope);
      if(pending!=default_member_initializers.end()) {
        pending->second.body=body;
        pending->second.init_ids=initializations;
        pending->second.lifetime_ids=lifetimes;
      }
      for(auto variable:local_variables)variables.erase(variable);
      local_variables.clear();expressions.clear();source_statement_nodes.clear();declaration_initializers.clear();subobject_initializations.clear();
      lifetimes.clear();initializations.clear();initialization_values.clear();initialization_types.clear();initializing.clear();
      function=previous;return_type=previous_return_type;literal_source_region=previous_literal_region;return;
    }
    if(!declaration_scopes.insert(scope).second)return;
    // File and namespace initializers have an explicit EDG execution list.
    // Until startup actions are represented, fail even when the initialized
    // variable is never referenced by an executable body.
    if(scope->dynamic_inits) {
      unsigned count=0,effective=0;
      for(auto init=scope->dynamic_inits;init;init=init->next) {
        ++count;auto var=init->variable;
        if(!var || (!ignore_variable_in_back_end(var) && !var->suppress_inline_definition))++effective;
      }
      std::string detail="file/namespace dynamic initialization schedule [entries="+std::to_string(count)+
        ", effective="+std::to_string(effective)+
        ", scope-kind="+std::to_string(int(scope->kind))+
        ", scope-lifetime="+std::to_string(scope->lifetime?1:0)+"]";
      unsigned ordinal=0;
      for(auto init=scope->dynamic_inits;init && ordinal<3;init=init->next) {
        auto var=init->variable;
        if(var && (ignore_variable_in_back_end(var) || var->suppress_inline_definition))continue;
        auto type=var?skip_typerefs(var->type):nullptr;
        auto pos=location(var?var->source_corresp.decl_position:null_source_position);
        std::string path;for(const auto &file:files)if(file.second==pos.file){path=file.first;break;}
        unsigned priority=0;
#if GNU_EXTENSIONS_ALLOWED && GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED
        if(var)priority=var->init_priority;
#endif
        detail+=" [entry="+std::to_string(ordinal)+", variable="+(var?spelling(var->source_corresp):"<none>")+
          ", type="+(type?spelling(type->source_corresp):"")+
          ", type-kind="+std::to_string(type?int(type->kind):-1)+
          ", action-kind="+std::to_string(int(init->kind))+
          ", inline="+std::to_string(var?int(var->is_inline):0)+
          ", template="+std::to_string(var?int(var->is_template_variable):0)+
          ", thread-local="+std::to_string(var?int(is_effective_thread_local(var)):0)+
          ", priority="+std::to_string(priority)+
          ", suppressed="+std::to_string(var?int(var->suppress_inline_definition):0)+
          ", ignored="+std::to_string(var?int(ignore_variable_in_back_end(var)):0)+
          ", destructor="+std::to_string(init->destructor?1:0)+
          ", lifetime="+std::to_string(init->lifetime?1:0)+
          ", expression-lifetime="+std::to_string(init->init_expr_lifetime?1:0)+
          ", file="+path+", line="+std::to_string(pos.line)+", column="+std::to_string(pos.column)+"]";
        ++ordinal;
      }
      if(effective)missing(detail);
    }
    if(scope->asm_entries)missing("file-scope assembly declaration");
    for(auto v=scope->variables;v;v=v->next) {
      // Lowering later promotes the same dummy to file scope. Its source
      // empty-pack declaration was captured with the function, not as data.
      if(empty_pack_parameter(v))continue;
#if DO_IL_LOWERING
      // Earlier function lowering can append ABI-only RTTI/vtable storage to
      // this later file-scope list. Such storage is not a source declaration;
      // selected source type/throw operations already own the relevant facts.
      if(v->lowering_generated)continue;
#endif
      if(v->is_nonreal){source_variable(v);continue;}
      variable(v,false,false,true);
    }
    for(auto r=scope->routines;r;r=r->next)
      if(!r->is_prototype_instantiation && r->source_corresp.decl_position.seq)routine(r);
    for(auto t=scope->types;t;t=t->next) {
      if(!t->source_corresp.decl_position.seq)continue;
      if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
        type_id(t);capture(class_type_supp(t)->assoc_scope);
      } else type_id(t);
    }
    for(auto child=scope->scopes;child;child=child->next)capture(child);
    for(auto ns=scope->namespaces;ns;ns=ns->next)if(!ns->is_namespace_alias)capture(ns->variant.assoc_scope);
  }
  void lambda_entry_complete(a_routine_ptr entry,a_scope_ptr scope) {
    if(!entry || entry->special_kind!=sfk_lambda_entry_point || !routines.count(entry))return;
    auto selected=entry->variant.lambda_call_operator;
    if(!selected || !scope || scope->variant.routine.ptr!=entry || !scope->assoc_block)
      missing("completed selected lambda entry body");
    auto signature=skip_typerefs(entry->type)->variant.routine.extra_info;
    if(signature->value_returned_as_parameter)
      missing("lambda entry caller-provided class result body");
    // The stock factory has attached its actual NULL receiver, ordered
    // forwarded parameters and return/call statements. Capture that exact
    // helper, preserving the previously recorded source signature identity.
    Id previous=function;bool previous_recipe=template_recipe;
    function=0;template_recipe=true;
    Id source=routine(selected);
    Id reference=node("function",entry->source_corresp.decl_position,
      routine_source_types.at(selected),nullptr,"lambda-call-operator","",source);
    function=previous;template_recipe=previous_recipe;
    capture(scope);
    Id symbol=routines.at(entry);
    db.update("symbols","id",symbol,{{"is_definition",true},
      {"link_name",entry->source_corresp.name?entry->source_corresp.name:""},
      {"linkage",linkage(entry->source_corresp)},
      {"definition_kind",entry->use_comdat || entry->is_inline ||
        (entry->is_template_function && !entry->is_specialized)?"weak":"strong"}});
    db.update("nodes","function_id",symbol,{{"is_synthetic",true},{"source_node_id",reference}});
  }
  void finish() {
    finish_typeinfo_symbols();
    if(!default_member_initializers.empty())
      missing("default member initializers without finalized constructor cleanup");
    for(const auto &entry:allocations)
      if(!entry.second.complete)missing("new expression without selected allocation size/result");
    for(const auto &entry:deallocations)
      if(!entry.second.execution)missing("delete expression without selected deallocator call");
    for(const auto &entry:static_local_symbols) {
      auto v=entry.first;
      if(v->initialization_rewritten_as_assignment)
        missing("local static constant rewritten as guarded initialization");
      if(v->promoted_local_static && !promoted_static_locals.count(v))
        missing("promoted local static without selected link identity");
      db.update("symbols","id",entry.second,{{"link_name",v->source_corresp.name?v->source_corresp.name:""},
        {"linkage",linkage(v->source_corresp)}});
    }
    for(const auto &entry:selected_literals)if(entry.second.symbol) {
      auto v=entry.first;
      db.update("symbols","id",entry.second.symbol,{{"link_name",v->source_corresp.name?v->source_corresp.name:""},
        {"linkage",linkage(v->source_corresp)}});
    }
    for(const auto &item:virtual_tables) {
      auto table=item.first.first;
      db.update("virtual_tables","id",item.second,{{"link_name",table->source_corresp.name?table->source_corresp.name:""},
        {"linkage",linkage(table->source_corresp)}});
    }
    for(const auto &item:virtual_entries) {
      const auto &entry=item.second;
      db.insert("virtual_table_entries",{{"table_id",item.first.first},{"slot_index",item.first.second},
        {"selected_symbol_id",entry.selected},{"callable_type_id",entry.signature},{"receiver_type_id",entry.receiver},
        {"target_symbol_id",optional(entry.target)},{"entry_kind",entry.kind},{"target_variant",entry.variant},
        {"abi_entry_name",entry.abi_routine && entry.abi_routine->source_corresp.name?entry.abi_routine->source_corresp.name:""},
        {"this_delta",entry.delta},{"vcall_index",entry.vcall},{"return_delta",entry.return_delta},{"vbase_index",entry.vbase}});
    }
    for(const auto &entry:routines)db.update("symbols","id",entry.second,{{"link_name",entry.first->source_corresp.name?entry.first->source_corresp.name:""}});
    for(const auto &entry:types) {
      auto t=entry.first;if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union ||
                           (t->kind==tk_integer && t->variant.integer.enum_type))
        db.update("types","id",entry.second,{{"link_name",t->source_corresp.name?t->source_corresp.name:""}});
    }
    const auto source=il_header.primary_source_file;
    const char *target=std::getenv("EDG_DOT_TARGET");
    if(!target || !*target)missing("explicit target ABI profile");
    db.insert("object_info",{{"singleton",1},{"format","relational-program-v1"},
      {"primary_source",source?(source->full_name?source->full_name:source->file_name):""},{"target_abi",target},
      {"char_bit",targ_char_bit},{"little_endian",bool(targ_little_endian)}});
    db.finish();
  }
};
inline State &state() { static State value;return value; }
inline void capture_scope(a_scope_ptr scope) {
  if(!enabled())return;
  state().capture(scope);
}
inline void literal_storage_selected(a_constant_ptr constant,a_variable_ptr variable) {
  if(enabled())state().literal_storage_selected(constant,variable);
}
inline void literal_function_complete(a_routine_ptr routine) {
  if(enabled())state().literal_function_complete(routine);
}
inline void default_member_initializers_complete(a_scope_ptr scope) {
  if(enabled())state().default_member_initializers_complete(scope);
}
inline void static_local_storage_selected(a_variable_ptr variable,a_routine_ptr routine) {
  if(enabled())state().static_local_storage_selected(variable,routine);
}
inline void typeinfo_storage_selected(a_type_ptr type,a_variable_ptr variable) {
  if(enabled())state().typeinfo_storage_selected(type,variable);
}
inline void allocation_size_begin(a_new_delete_supplement_ptr allocation) {
  if(enabled())state().allocation_size_begin(allocation);
}
inline void allocation_count_binding(a_variable_ptr temporary) {
  if(enabled())state().allocation_count_binding(temporary);
}
inline void allocation_effective_count_binding(a_variable_ptr temporary,an_expr_node_ptr value) {
  if(enabled())state().allocation_effective_count_binding(temporary,value);
}
inline void allocation_length_check(an_expr_node_ptr predicate,a_routine_ptr failure) {
  if(enabled())state().allocation_length_check(predicate,failure);
}
inline void allocation_size_end(a_new_delete_supplement_ptr allocation,an_expr_node_ptr size) {
  if(enabled())state().allocation_size_end(allocation,size);
}
inline void allocation_simple_result(a_new_delete_supplement_ptr allocation) {
  if(enabled())state().allocation_simple_result(allocation);
}
inline void allocation_construction_null_check(a_new_delete_supplement_ptr allocation,bool required) {
  if(enabled())state().allocation_construction_null_check(allocation,required);
}
inline void allocation_zeroing(a_new_delete_supplement_ptr allocation,a_variable_ptr byte_count) {
  if(enabled())state().allocation_zeroing(allocation,byte_count);
}
inline void deallocation_begin(a_new_delete_supplement_ptr allocation) {
  if(enabled())state().deallocation_begin(allocation);
}
inline void deallocation_destruction(a_dynamic_init_ptr init,a_routine_ptr routine,
    an_expr_node_ptr receiver,an_expr_node_ptr reused_pointer,bool null_check) {
  if(enabled())state().deallocation_destruction(init,routine,receiver,reused_pointer,null_check);
}
inline void deallocation_arguments(a_routine_ptr callee,an_expr_node_ptr arguments) {
  if(enabled())state().deallocation_arguments(callee,arguments);
}
inline void deallocation_end(a_new_delete_supplement_ptr allocation) {
  if(enabled())state().deallocation_end(allocation);
}
inline void virtual_source_class(a_type_ptr type) {
  if(enabled())state().virtual_source_class(type);
}
inline void virtual_source_routine(a_routine_ptr routine) {
  if(enabled())state().virtual_source_routine(routine);
}
inline void virtual_table_begin(a_type_ptr owner,a_base_class_ptr base,a_base_class_ptr construction,
                                a_variable_ptr table,long long address_point,long long entry_size) {
  if(enabled())state().virtual_table_begin(owner,base,construction,table,address_point,entry_size);
}
inline void virtual_table_end() {if(enabled())state().virtual_table_end();}
inline void virtual_table_entry(long long slot,a_routine_ptr selected,a_routine_ptr target,long long delta,bool null_entry) {
  if(enabled())state().virtual_table_entry(slot,selected,target,delta,null_entry);
}
inline void virtual_vptr_begin(a_routine_ptr routine,const char *phase) {
  if(enabled())state().virtual_vptr_begin(routine,phase);
}
inline void virtual_vptr_end() {if(enabled())state().virtual_vptr_end();}
inline void virtual_vptr_write(a_type_ptr owner,a_base_class_ptr base,an_expr_node_ptr address,
                               bool construction,long long construction_index) {
  if(enabled())state().virtual_vptr_write(owner,base,address,construction,construction_index);
}
inline void lambda_entry_complete(a_routine_ptr entry,a_scope_ptr scope) {
  if(enabled())state().lambda_entry_complete(entry,scope);
}
inline void emit() { state().finish(); }
}
END_EDG_NAMESPACE
#endif
