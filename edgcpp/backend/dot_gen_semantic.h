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
  std::set<a_variable_ptr> incomplete_globals;
  std::map<a_field_ptr,Id> fields;
  std::map<a_base_class_ptr,Id> base_subobjects;
  std::map<a_constant_ptr,Id> enum_values;
  std::map<std::pair<Id,std::string>,Id> floating_values;
  std::map<a_template_ptr,Id> templates;
  std::map<std::string,Id> files;
  std::map<an_expr_node_ptr,Id> expressions;
  std::map<an_expr_node_ptr,Id> recipe_expressions;
  std::map<a_constant_ptr,Id> recipe_constants;
  bool template_recipe=false;
  unsigned recipe_capture_depth=0;
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
    missing("dynamic exception specification projection");
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
        }break;
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
      db.update("types","id",id,{{"exception_policy",exception_policy(t)},
        {"return_storage",t->variant.routine.extra_info->value_returned_by_cctor?"caller-provided":"value"}});
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
        db.insert("enum_values",{{"id",value_id},{"enum_type_id",id},{"ordinal",ordinal},
          {"name",spelling(value->source_corresp)},{"value_text",integer(value)},
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
  Id recipe_value(a_constant_ptr c,const a_source_position &pos) {
    auto previous_function=function;bool previous_recipe=template_recipe;
    if(recipe_capture_depth++==0){recipe_expressions.clear();recipe_constants.clear();}
    function=0;template_recipe=true;
    Id id=constant(c,pos);
    template_recipe=previous_recipe;function=previous_function;
    if(--recipe_capture_depth==0){recipe_expressions.clear();recipe_constants.clear();}
    return id;
  }
  const char *dependent_value_kind(a_constant_ptr c) {
    switch(c->variant.template_param.kind) {
      case tpck_param:return "template-parameter-reference";
      case tpck_member:return "dependent-member-reference";
      case tpck_expression:return "dependent-expression";
      case tpck_dependent_constant:return "dependent-constant";
      case tpck_address:return "dependent-address";
      default:missing("dependent value "+spelling(c->source_corresp),int(c->variant.template_param.kind));
    }
  }
  void dependent_value(Id id,a_constant_ptr c) {
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
        auto operand=expr_node_from_tpck_expression(c);
        if(!operand)missing("dependent source expression");
        edge(id,"operand",0,expression(operand));break;
      }
      case tpck_dependent_constant:case tpck_address:
        edge(id,"operand",0,constant(value.variant.constant,c->source_corresp.decl_position));break;
      default:missing("dependent value",int(value.kind));
    }
  }
  void array_initializer(Id id,a_constant_ptr c,const a_source_position &p) {
    auto type=skip_typerefs(c->type);
    if(type->kind!=tk_array)missing("non-array aggregate initializer");
    if(c->variant.aggregate.has_dynamic_init_component)
      missing("array initializer dynamic component");
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
        if(!count)missing("runtime array initializer repetition");
        if(item->variant.init_repeat.multidimensional_aggr_tail_not_repeated)
          missing("designated array initializer repetition tail");
        // EDG can compact repeated leaf values across several nested rows.
        // That selected form needs an explicit multidimensional destination;
        // this immediate-element relation deliberately does not flatten it.
        if(!identical_types_ignoring_qualifiers(value->type,type->variant.array.element_type))
          missing("flattened multidimensional array initializer repetition");
      }
      if(value->kind==ck_dynamic_init)missing("array initializer dynamic component");
      const auto &position=value->source_corresp.decl_position.seq?value->source_corresp.decl_position:p;
      db.insert("array_initializer_elements",{{"initializer_node_id",id},{"ordinal",ordinal++},
        {"array_index",index},{"repeat_count",count},{"value_node_id",constant(value,position)}});
      index+=count;
    }
  }
  Id constant(a_constant_ptr c,const a_source_position &p) {
    if(template_recipe) {
      auto found=recipe_constants.find(c);if(found!=recipe_constants.end())return found->second;
    }
    if(c->kind==ck_template_param) {
      Id id=node(dependent_value_kind(c),p,type_id(c->type),"prvalue");
      if(template_recipe)recipe_constants[c]=id;
      dependent_value(id,c);return id;
    }
    if(c->kind==ck_aggregate) {
      Id id=node("array-initializer",p,type_id(c->type),"prvalue");
      if(template_recipe)recipe_constants[c]=id;
      array_initializer(id,c,p);return id;
    }
    Id id=node("constant",p,type_id(c->type),"prvalue",nullptr,literal_text(c,"initializer",&p));
    if(template_recipe)recipe_constants[c]=id;
    if(c->kind==ck_float)db.update("nodes","id",id,{{"floating_value_id",floating_value(c)}});
    attach_enum_value(id,c);return id;
  }
  void attach_enum_value(Id id,a_constant_ptr c) {
    if(!is_enum_constant(c))return;
    // C enumerators carry an affiliated integral type; C++ enumerators
    // carry the enum type itself. Retain only an actual EDG entity identity,
    // never guess a declaration from a coincidentally equal value or name.
    auto type=skip_lexical_typerefs(c->type);
    type_id(underlying_enum_type(type));
    auto found=enum_values.find(c);
    if(found!=enum_values.end())db.update("nodes","id",id,{{"enum_value_id",found->second}});
  }
  bool empty_pack_parameter(a_variable_ptr v) {
    // Exact lower_il::is_empty_pack_parameter predicate. func_def creates
    // this dummy for an empty expansion; it never denotes runtime storage.
    return v->compiler_generated && type_is(v->type,tk_template_param);
  }
  bool has_static_storage(a_scope_ptr scope) {
    if(scope->local_static_variable_inits)return true;
    for(auto v=scope->variables;v;v=v->next)
      if(!empty_pack_parameter(v))return true;
    return false;
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
    if(template_recipe)missing("template recipe initialization ownership");
    if(init->master_entry || init->class_rvalue_initialized_through_master_entry)
      missing("shared class-result initialization destination");
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
      case dik_class_result_via_ctor:kind="class-result";value=expression(init->variant.expression);break;
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
      DOT_SEMANTIC_OP(eok_class_rvalue_adjust,"class-rvalue-adjust");
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
  Id expression(an_expr_node_ptr e) {
    if(!e)return 0;
    auto &expression_ids=template_recipe?recipe_expressions:expressions;
    auto found=expression_ids.find(e);if(found!=expression_ids.end())return found->second;
    const char *kind=nullptr,*op=nullptr;std::string text;Id symbol=0,field=0;
    switch(e->kind) {
      case enk_constant:
        if(node_constant(e)->kind==ck_template_param)kind=dependent_value_kind(node_constant(e));
        else if(node_constant(e)->kind==ck_aggregate)kind="array-initializer";
        else {kind="constant";text=literal_text(node_constant(e),"expression",&e->position);}break;
      case enk_variable:kind="variable";symbol=variable(node_variable(e));break;
      case enk_routine:kind="function";symbol=routine(e->variant.routine.ptr);break;
      case enk_field:kind="field";field=field_id(node_field(e));break;
      case enk_temp_init:kind="temporary";break;
      case enk_condition:kind="condition";break;
      case enk_object_lifetime:kind="lifetime-scope";break;
      case enk_throw:kind=e->variant.throw_info?"throw":"rethrow";break;
      case enk_operation: {
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
    expression_ids[e]=id;
    // Source-form predicate for lower_il::rewrite_nullptr_expr_if_necessary.
    // lower_expr first changes xvalues to lvalues, so both glvalue forms are
    // excluded here. Retain the original children for evaluation/side effects;
    // the explicit result rule replaces the value, not the storage location.
    if(!e->is_lvalue && !e->is_xvalue && is_standard_nullptr_type(e->type) &&
       e->kind!=enk_constant && e->kind!=enk_field && !e->result_is_not_used)
      db.update("nodes","id",id,{{"value_normalization","nullptr-zero"}});
    if(e->kind==enk_constant) {
      if(node_constant(e)->kind==ck_template_param)dependent_value(id,node_constant(e));
      if(node_constant(e)->kind==ck_aggregate)array_initializer(id,node_constant(e),e->position);
      attach_enum_value(id,node_constant(e));
      if(node_constant(e)->kind==ck_float)
        db.update("nodes","id",id,{{"floating_value_id",floating_value(node_constant(e))}});
    }
    if(e->kind==enk_operation) {
      if(is_compound_assignment_operator(e->variant.operation.kind))
        // The stock helper handles promotions (especially the promoted left
        // operand of a shift). This is distinct from the stored/result type
        // and does not ask the consumer to re-run arithmetic conversions.
        db.update("nodes","id",id,{{"operation_type_id",type_id(compound_assignment_operation_type(e))}});
      if(e->variant.operation.is_virtual_call)virtual_call(id,e);
      if(e->variant.operation.kind==eok_base_class_cast || e->variant.operation.kind==eok_derived_class_cast)
        class_conversion(id,e);
      db.update("nodes","id",id,{{"evaluation_order",e->variant.operation.eval_left_to_right?"left-to-right":
        e->variant.operation.eval_right_to_left?"right-to-left":"unspecified"}});
      unsigned ordinal=0;for(auto child=e->variant.operation.operands;child;child=child->next,++ordinal) {
        // scan_va_start_operator retains the source last-parameter anchor.
        // It supplies no argument value to the portable pack; GNU explicitly
        // ignores it (il::lvalue/rvalue operand checks). Keep it navigable
        // without asking generic operand expansion to evaluate it.
        if(e->variant.operation.kind==eok_va_start && ordinal==1)
          edge(id,"last-parameter",0,expression(child));
        else edge(id,"operand",ordinal,expression(child));
      }
    } else if(e->kind==enk_temp_init) {
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
      edge(id,"value",0,expression(condition->expr));
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
  Id statement(a_statement_ptr s,a_scope_ptr root_scope=nullptr,bool condition_initializer=false) {
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
        if(scope && has_static_storage(scope))missing("block static storage projection");
        attach_lifetime(id,scope?scope->lifetime:s->variant.block.extra_info->lifetime);
        if(scope)for(auto v=scope->nonstatic_variables;v;v=v->next)variable(v,true);
        unsigned ordinal=0;for(auto child=s->variant.block.statements;child;child=child->next)
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
    if(has_static_storage(scope))missing("local static storage projection");
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
      Id copy_source=0;
      auto init=entry->initializer;
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
      db.insert("constructor_initializers",{{"function_id",function},{"ordinal",ordinal},{"kind",kind},
        {"field_id",optional(field)},{"base_subobject_id",optional(base)},
        {"copy_source_symbol_id",optional(copy_source)},
        {"target_type_id",optional(type_id(target))},{"is_virtual",is_virtual},
        {"initialization_id",initialization_id(entry->initializer,target)}});
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
      return_type=skip_typerefs(r->type)->variant.routine.return_type;
      function=routine(r);expressions.clear();
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
      local_bindings(scope);
      constructor_initializers(scope);
      lifetime_id(scope->lifetime);
      Id body=statement(scope->assoc_block,scope);
      const char *policy=exception_policy(r->type);
      if(std::string(policy)=="dependent")missing("finalized function exception policy");
      db.update("types","id",routine_source_types[r],{{"exception_policy",policy},
        {"return_storage",skip_typerefs(r->type)->variant.routine.extra_info->value_returned_by_cctor?"caller-provided":"value"}});
      const char *kind=r->special_kind==sfk_constructor?"constructor":r->special_kind==sfk_destructor?"destructor":"ordinary";
      db.insert("functions",{{"symbol_id",function},{"body_node_id",body},{"receiver_symbol_id",optional(receiver)},
        {"kind",kind},{"exception_policy",policy},
        {"nrvo_candidate_symbol_id",optional(scope->variant.routine.return_value_variable?
          variable(scope->variant.routine.return_value_variable,true):0)}});
      for(auto variable:local_variables)variables.erase(variable);
      local_variables.clear();expressions.clear();declaration_initializers.clear();
      lifetimes.clear();initializations.clear();initialization_values.clear();initialization_types.clear();initializing.clear();
      function=previous;return_type=previous_return_type;return;
    }
    if(!declaration_scopes.insert(scope).second)return;
    // File and namespace initializers have an explicit EDG execution list.
    // Until startup actions are represented, fail even when the initialized
    // variable is never referenced by an executable body.
    if(scope->dynamic_inits)missing("file/namespace dynamic initialization schedule");
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
      if(v->is_nonreal)missing("dependent global declaration");
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
  void finish() {
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
      {"primary_source",source?(source->full_name?source->full_name:source->file_name):""},{"target_abi",target}});
    db.finish();
  }
};
inline State &state() { static State value;return value; }
inline void capture_scope(a_scope_ptr scope) {
  if(!enabled())return;
  state().capture(scope);
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
inline void emit() { state().finish(); }
}
END_EDG_NAMESPACE
#endif
