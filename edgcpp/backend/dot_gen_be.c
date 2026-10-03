// Experimental EDG -> C# backend. Compiled as C++, like EDG's other .c files.
// Unsupported IL is rejected before publishing any generated project files.
#include <string>
#include <map>
#include <vector>
#include <set>
#include <algorithm>
#include <cstdio>
#include <cstdlib>
#include "fe_common.h"
#include "il.h"
#include "const_ints.h"
#include "float_pt.h"
#include "mem_manage.h"
#include "lower_il.h"
#include "lower_eh.h"
USING_NAMESPACE_EDG

namespace {
using std::string;
std::map<const void *, string> names;
std::map<a_variable_ptr, unsigned long long> global_offsets, tls_offsets;
std::set<a_variable_ptr> exception_metadata;
std::map<a_constant_ptr,unsigned> virtual_type_tokens;
std::map<a_variable_ptr,unsigned> virtual_types;
std::map<a_variable_ptr,a_type_ptr> rtti_source_types,rtti_lowered_types;
std::set<a_type_ptr> rtti_user_prefix_types;
bool rtti_helpers_emitted=false;
bool typeinfo_helpers_emitted=false;
bool runtime_exceptions=false;
a_scope_ptr current_function_scope=nullptr;
std::vector<a_variable_ptr> constant_template_variables;
std::map<a_variable_ptr,a_constant_ptr> constant_templates;
bool emitting_tls_initializer = false;
std::map<string, string> assignment_helpers;
string helper_output, type_output, address_output, literal_bytes;
std::map<a_routine_ptr, string> routine_addresses;
std::map<string, unsigned long long> literal_offsets;
string routine_address(a_routine_ptr r);
string function_pointer_type(a_type_ptr t);
bool va_tag_type(a_type_ptr t);
bool variadic_routine(a_type_ptr t);
std::map<a_type_ptr, string> aggregate_types;
std::map<a_constant_ptr, string> aggregate_constants;
std::map<a_variable_ptr, unsigned long long> local_offsets;
unsigned long long frame_size = 0, frame_alignment = 8;
string type(a_type_ptr t);
string initializer(string dst, a_type_ptr t, a_constant_ptr c);
string typeinfo_constant(a_constant_ptr c);
bool aggregate(a_type_ptr t) {
  t = skip_typerefs(t);
  return t->kind == tk_array || t->kind == tk_struct || t->kind == tk_class || t->kind == tk_union;
}
unsigned serial = 0;
bool instance_mode, nested_types, library_mode;
bool function_returns_void;
string output;
const char *context = "file scope";
a_source_position diagnostic_position{};
struct diagnostic_scope {
  a_source_position previous;
  explicit diagnostic_scope(const a_source_position &position):previous(diagnostic_position) {
    if(position.seq)diagnostic_position=position;
  }
  ~diagnostic_scope(){diagnostic_position=previous;}
};

[[noreturn]] void unsupported(const char *what, int kind = -1) {
  if(diagnostic_position.seq) {
    a_const_char *file=nullptr,*full=nullptr;a_line_number line_number=0;a_boolean at_end=FALSE;
    conv_seq_to_file_and_line(diagnostic_position.seq,&file,&full,&line_number,&at_end);
    if(file)fprintf(stderr,"%s:%llu:%u: ",file,(unsigned long long)line_number,(unsigned)diagnostic_position.column);
  }
  fprintf(stderr, "dot_gen_be: unsupported %s (kind %d) in %s\n", what, kind, context);
  exit(1);
}
// Same entity liveness contract as c_gen_be.c::start_unreferenced_bracket.
bool entity_needed(a_source_correspondence *source) {
#if MAINTAIN_NEEDED_FLAGS
  return needed_flag_is_set(source);
#else
  return source->referenced;
#endif
}
bool variable_needed(a_variable_ptr v) {
  if(ignore_variable_in_back_end(v))return false;
  auto &source=v->source_corresp;
  // The C backend additionally preserves a Sun-C-only varargs marker. Our
  // generated target is managed code, so that target-specific case is absent.
  bool magic=source.name_linkage==nlk_internal && source.name && string(source.name)=="__link";
  return magic || entity_needed(&source);
}
bool routine_needed(a_routine_ptr r) {
  return !ignore_routine_in_back_end(r) && entity_needed(&r->source_corresp);
}
bool routine_body_needed(a_routine_ptr r) {
  return routine_needed(r) && r->function_def_number!=NULL_function_def_number
#if MAINTAIN_NEEDED_FLAGS
    && r->definition_needed
#endif
    ;
}
bool selected_runtime_branch(a_statement_ptr s,a_statement_ptr &branch);
#include "dot_gen_members.h"
string name(const void *p, const char *prefix) {
  auto i = names.find(p);
  if (i != names.end()) return i->second;
  string result = string(prefix) + std::to_string(++serial);
  const char *cls = getenv("EDG_DOT_CLASS");
  if (cls && string(cls) == "@" + result) result += "_";
  return names[p] = result;
}
#include "dot_gen_trace.h"
#include "dot_gen_order.h"
#include "dot_gen_ir_adapter.h"
#include "dot_gen_storage_types.h"
bool owned_ir_enabled=false;

string aggregate_type(a_type_ptr t) {
  const auto layout_size=owned_ir_enabled?owned_ir.types[own_type(t)].size:(unsigned long long)t->size;
  auto found = aggregate_types.find(t);
  if (found != aggregate_types.end()) return found->second;
  string array_key;
  if(t->kind==tk_array) {
    array_key=storage_type_key(t);
    auto prior=array_storage_types.find(array_key);
    if(prior!=array_storage_types.end())return aggregate_types[t]=prior->second;
  }
  if(t->kind==tk_array && !t->size && !t->variant.array.is_vla && !t->variant.array.is_template_dependent_size_array) {
    // An address-only view for C flexible/incomplete array tails. No CLR field
    // is embedded for this marker; the containing record keeps its EDG size.
    string id=name(t,"__edg_array_tail");
    if(!nested_types)id=string(getenv("EDG_DOT_CLASS")+1)+"_"+id;
    aggregate_types[t]=id;array_storage_types[array_key]=id;type_output+="public struct "+id+" {}\n";return id;
  }
  if (!layout_size || layout_size > 64*1024*1024) unsupported(("aggregate size "+std::to_string(layout_size)+" for "+string(t->source_corresp.name?t->source_corresp.name:"<anonymous>")+" (1..64 MiB bytes required)").c_str());
  string id = name(t, "__edg_type");
  if (!nested_types) id = string(getenv("EDG_DOT_CLASS") + 1) + "_" + id;
  aggregate_types[t] = id; // Register before walking self-referential pointers.
  if(!array_key.empty())array_storage_types[array_key]=id;
  string fields;
  if (t->kind == tk_array) {
    if (t->variant.array.is_variable_size_array || t->variant.array.is_vla ||
        t->variant.array.is_template_dependent_size_array) unsupported("variable array bound");
    auto element = skip_typerefs(t->variant.array.element_type);
    if (!element->size || t->size % element->size ||
        t->size / element->size != t->variant.array.variant.number_of_elements)
      unsupported("array size/stride mismatch");
    auto count = t->variant.array.variant.number_of_elements;
    string element_type = type(t->variant.array.element_type);
    // InlineArray informs the CLR that the full extent contains elements,
    // avoiding both padding-only copies and one source field per element.
    // Like dotcc, wrap pointer cells so C# generic-type restrictions do not
    // produce CS9184; translated accesses still use original element pointers.
    if(element->kind==tk_pointer) {
      string cell=id+"_Element";
      type_output+="[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack=1)]\npublic unsafe struct "+cell+" { public "+element_type+" Value; }\n";
      element_type=cell;
    }
    type_output+="[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack=1)]\n[global::System.Runtime.CompilerServices.InlineArray("+std::to_string(count)+")]\npublic unsafe struct "+id+" { public "+element_type+" e0; }\n";
    return id;
  } else {
    std::set<unsigned long long> bit_bytes;
    for (auto f = t->variant.class_struct_union.field_list; f; f = f->next) {
      auto ft=skip_typerefs(f->type);
      if(f->is_bit_field) {
        if(!f->bit_size)continue;
        if(ft->kind!=tk_integer || f->bit_size>64 || f->offset+((unsigned)f->offset_bit_remainder+(unsigned)f->bit_size+7U)/8U>t->size)
          unsupported("bitfield representation");
        unsigned count=((unsigned)f->offset_bit_remainder+(unsigned)f->bit_size+7U)/8U;
        for(unsigned i=0;i<count;++i)if(bit_bytes.insert(f->offset+i).second)
          fields+="[global::System.Runtime.InteropServices.FieldOffset("+std::to_string(f->offset+i)+")] private byte __edg_bits"+std::to_string(f->offset+i)+";\n";
        string read="bits";
        if(int_type_is_signed(ft))read="((long)(bits << "+std::to_string(64-f->bit_size)+") >> "+std::to_string(64-f->bit_size)+")";
        // Byte operations avoid a 64-iteration loop while preserving adjacent
        // packed fields. At most nine bytes cover a 64-bit unaligned field;
        // no widened load/store may reach outside the declared object.
        unsigned remainder=(unsigned)f->offset_bit_remainder;
        if(remainder>7)unsupported("bitfield byte remainder");
        string load="ulong bits=((ulong)p[0] >> "+std::to_string(remainder)+")";
        for(unsigned i=1;i<count;++i)
          load+=" | ((ulong)p["+std::to_string(i)+"] << "+std::to_string(i*8-remainder)+")";
        load+=";";
        if(f->bit_size<64)load+="bits &= "+std::to_string((1ULL<<f->bit_size)-1ULL)+"UL;";
        string store="ulong bits=unchecked((ulong)value);";
        for(unsigned i=0;i<count;++i) {
          int shift=(int)(i*8)-(int)remainder;
          unsigned low=shift<0?(unsigned)-shift:0;
          unsigned high=std::min(8U,remainder+(unsigned)f->bit_size-i*8);
          unsigned mask=((1U<<(high-low))-1U)<<low;
          string fragment=shift<0?"(bits << "+std::to_string(-shift)+")":"(bits >> "+std::to_string(shift)+")";
          store+="p["+std::to_string(i)+"]=(byte)(((ulong)p["+std::to_string(i)+"] & ~"+std::to_string(mask)+"UL) | ("+fragment+" & "+std::to_string(mask)+"UL));";
        }
        fields+="public "+type(f->type)+" "+name(f,"m")+" { get { fixed(byte* p=&__edg_bits"+std::to_string(f->offset)+") { "+load+"return unchecked(("+type(f->type)+")"+read+"); } } set { fixed(byte* p=&__edg_bits"+std::to_string(f->offset)+") { "+store+"} } }\n";
        continue;
      }
      bool vptr=f->compiler_generated && f->source_corresp.name && string(f->source_corresp.name)=="__vptr" &&
          f->offset==class_type_supp(t)->virtual_function_info_offset && ft->kind==tk_pointer &&
          skip_typerefs(ft->variant.pointer.type)->kind==tk_integer && skip_typerefs(ft->variant.pointer.type)->size==8;
      if (f->compiler_generated && !f->is_lowered_base_class && !vptr && !member_pointer_field(t,f) &&
          !(f->is_anonymous_parent_object && (ft->kind==tk_struct || ft->kind==tk_union)))
        unsupported(("bitfield or synthetic record field: "+string(f->source_corresp.name?f->source_corresp.name:"unnamed")).c_str());
      if(ft->kind==tk_array && !ft->size)continue; // Flexible tail: address only, no storage field.
      if (f->offset + skip_typerefs(f->type)->size > t->size) unsupported("field outside record");
      const char* saved_context=context;
      string field_context=string(context)+" -> field "+string(f->source_corresp.name?f->source_corresp.name:"<anonymous>");
      context=field_context.c_str();string field_type=type(f->type);context=saved_context;
      fields += "[global::System.Runtime.InteropServices.FieldOffset(" + std::to_string(f->offset) +
                ")] public " + field_type + " " + name(f, "m") + ";\n";
    }
  }
  type_output += "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = " +
                 std::to_string(layout_size) + ", Pack = 1)]\npublic unsafe struct " + id + " {\n" + fields + "}\n";
  return id;
}
string type(a_type_ptr t) {
  if(owned_ir_enabled)own_type(t);
  // Restrict constrains valid source aliases; omitting that optimization
  // promise in managed code preserves valid-program behavior. EDG itself has
  // SUPPRESS_RESTRICT_IN_GENERATED_CODE in il_to_str.c::form_type_qualifiers.
  if ((get_type_qualifiers(t) & ~(TQ_CONST | TQ_VOLATILE | TQ_RESTRICT | TQ_C11_ATOMIC)) != 0) unsupported("qualified type");
  t = skip_typerefs(t);
  if(va_tag_type(t)) {
    if(t->size!=24 || alignment_of_type(t)!=8)unsupported("builtin va_list tag layout");
    return "global::Edg.Runtime.VaList";
  }
  if(rtti_user_prefix_types.count(t)) {
    if(!type_of_type_info)unsupported("missing public type_info definition");
    return type(type_of_type_info);
  }
  if (aggregate(t)) return aggregate_type(t);
  if (t->kind == tk_void) return "void";
  if (t->kind == tk_pointer) {
    if (t->size != 8 || t->variant.pointer.is_handle || t->variant.pointer.is_interior_ptr)
      unsupported("pointer representation");
    if (skip_typerefs(t->variant.pointer.type)->kind == tk_routine)
      return function_pointer_type(skip_typerefs(t->variant.pointer.type));
    auto pointee=skip_typerefs(t->variant.pointer.type);
    if(aggregate(pointee) && pointee->incomplete && pointee->kind!=tk_array) {
      // C permits opaque handles with no definition. Keep pointer type identity;
      // no value, size or member operation on this placeholder is emitted.
      auto found=aggregate_types.find(pointee);
      if(found==aggregate_types.end()) {
        string id=name(pointee,"__edg_opaque");
        if(!nested_types)id=string(getenv("EDG_DOT_CLASS")+1)+"_"+id;
        aggregate_types[pointee]=id;
        type_output+="public struct "+id+" {}\n";
        return id+"*";
      }
      return found->second+"*";
    }
    return type(t->variant.pointer.type) + "*";
  }
  if(t->kind==tk_float) {
    if((t->variant.float_kind==fk_float || t->variant.float_kind==fk_std_float32) && t->size==4)return "float";
    if((t->variant.float_kind==fk_float16 || t->variant.float_kind==fk_fp16 || t->variant.float_kind==fk_std_float16) && t->size==2)return "global::System.Half";
    if((t->variant.float_kind==fk_float128 || t->variant.float_kind==fk_std_float128) && t->size==16)return "global::Edg.Runtime.Float128";
    if((t->variant.float_kind==fk_double || t->variant.float_kind==fk_float32x || t->variant.float_kind==fk_std_float64) && t->size==8)return "double";
    if((t->variant.float_kind==fk_long_double || t->variant.float_kind==fk_float80 || t->variant.float_kind==fk_float64x) && t->size==16)
      return "global::Edg.Runtime.Float80";
    unsupported("extended floating representation");
  }
  if (t->kind != tk_integer) unsupported("type", t->kind);
  switch (t->variant.integer.int_kind) {
    case ik_char: if (t->size == 1) return targ_has_signed_chars ? "sbyte" : "byte"; break;
    case ik_signed_char: if (t->size == 1) return "sbyte"; break;
    case ik_unsigned_char: if (t->size == 1) return "byte"; break;
    case ik_short: if (t->size == 2) return "short"; break;
    case ik_unsigned_short: if (t->size == 2) return "ushort"; break;
    case ik_int: if (t->size == 4) return "int"; break;
    case ik_unsigned_int: if (t->size == 4) return "uint"; break;
    case ik_long: case ik_long_long: if (t->size == 8) return "long"; break;
    case ik_unsigned_long: case ik_unsigned_long_long: if (t->size == 8) return "ulong"; break;
    case ik_int128: if(t->size==16)return "global::System.Int128";break;
    case ik_unsigned_int128: if(t->size==16)return "global::System.UInt128";break;
    default: break;
  }
  unsupported("integer representation", t->variant.integer.int_kind);
}
string function_pointer_type(a_type_ptr t) {
  auto extra = t->variant.routine.extra_info;
  string result = "delegate* managed<";
  if (instance_mode) result += string(getenv("EDG_DOT_CLASS")) + ", ";
  for (auto p = extra->param_type_list; p; p = p->next) result += type(p->type) + ", ";
  if(extra->has_ellipsis)result+="global::Edg.Runtime.VaList*, ";
  return result + type(t->variant.routine.return_type) + ">";
}
#include "dot_gen_imports.h"
#include "dot_gen_threads.h"
string routine_address(a_routine_ptr r) {
  bool pure=r->compiler_generated && r->source_corresp.name && string(r->source_corresp.name)=="__cxa_pure_virtual";
  if (r->function_def_number == NULL_function_def_number && !pure && !libc_routine(r) && !thread_routine(r)) unsupported(("address of external function: "+string(r->source_corresp.name?r->source_corresp.name:"<unnamed>")).c_str());
  r=libc_address_routine(r);
  auto found = routine_addresses.find(r);
  if (found != routine_addresses.end()) return found->second + "()";
  auto rt = skip_typerefs(r->type);
  string fp = function_pointer_type(rt), id = name(r, "f"), getter = "__edg_address_" + id;
  routine_addresses[r] = getter;
  if(pure) {
    if(skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void || rt->variant.routine.extra_info->param_type_list)
      unsupported("pure virtual runtime ABI");
    helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+"void "+id+
      "() { global::System.Environment.FailFast(\"EDG pure virtual call\"); }\n// dot-gen:member-end\n";
  }
  string target = id;
  {
    target = "__edg_adapter_" + id;
    string signature = "private static " + type(rt->variant.routine.return_type) + " " + target + "(" +
                       string(instance_mode ? string(getenv("EDG_DOT_CLASS")) + " owner" : "");
    string args;
    unsigned index = 0;
    for (auto p = rt->variant.routine.extra_info->param_type_list; p; p = p->next) {
      string arg = "a" + std::to_string(index++);
      signature += string(instance_mode || index>1 ? ", " : "") + type(p->type) + " " + arg;
      if (!args.empty()) args += ", ";
      args += arg;
    }
    if(rt->variant.routine.extra_info->has_ellipsis) {
      signature+=string(instance_mode || index?", ":"")+"global::Edg.Runtime.VaList* __edg_arguments";
      args+=string(args.empty()?"":", ")+"__edg_arguments";
    }
    address_output += "// dot-gen:member-begin\n" + signature + ") { "+string(instance_mode?"owner.__edg_enter();":"/*__EDG_ENTER__*/")+" try { " +
                      (type(rt->variant.routine.return_type) == "void" ? "" : "return ") +
                      string(instance_mode?"owner.":"") + id + "(" + args + "); } finally { "+string(instance_mode?"owner.__edg_leave(); global::System.GC.KeepAlive(owner);":"/*__EDG_LEAVE__*/")+" } }\n// dot-gen:member-end\n";
  }
  // Cache the managed entry point once. Interlocked publication preserves
  // identity across concurrent first use and tiered-JIT address-taking.
  address_output += "private static nint " + getter + "_cache;\n// dot-gen:member-begin\nprivate static " + fp + " " + getter + "() {\n"
    "if (global::System.Threading.Volatile.Read(ref " + getter + "_cache) == 0) "
    "global::System.Threading.Interlocked.CompareExchange(ref " + getter + "_cache, (nint)(" + fp + ")&" + target + ", 0);\n"
    "return (" + fp + ")global::System.Threading.Volatile.Read(ref " + getter + "_cache); }\n// dot-gen:member-end\n";
  return getter + "()";
}
string literal_address(a_constant_ptr c) {
  if (c->kind != ck_string) unsupported("non-string literal address");
  string bytes(c->variant.string.value, c->variant.string.length);
  auto found = literal_offsets.find(bytes);
  unsigned long long offset;
  if (found != literal_offsets.end()) offset = found->second;
  else {
    offset = (literal_bytes.size() + 7) & ~7ULL;
    if (offset + bytes.size() > 4 * 1024 * 1024) unsupported("literal pool exceeds 4 MiB");
    literal_bytes.resize(offset, 0);
    literal_bytes += bytes;
    literal_offsets[bytes] = offset;
  }
  return "(__edg_literal_base + " + std::to_string(offset) + ")";
}
bool function_pointer(a_type_ptr t) {
  t = skip_typerefs(t);
  return t->kind == tk_pointer && skip_typerefs(t->variant.pointer.type)->kind == tk_routine;
}
std::map<a_variable_ptr, string> expression_variables;
string variable(a_variable_ptr v) {
  if(v->storage_class==sc_extern){string imported=libc_variable(v);if(!imported.empty())return imported;unsupported("reference to external variable");}
  if(exception_metadata.count(v))unsupported("native RTTI storage outside exception matching");
  auto captured=expression_variables.find(v);
  if(captured!=expression_variables.end())return captured->second;
  auto tls = tls_offsets.find(v);
  if (tls != tls_offsets.end())
    return "(*(" + type(v->type) + "*)(" + (emitting_tls_initializer ? "p" : "__edg_tls_base") + " + " + std::to_string(tls->second) + "))";
  auto it = global_offsets.find(v);
  if (it == global_offsets.end()) {
    auto local = local_offsets.find(v);
    if (local != local_offsets.end())
      return "(*(" + type(v->type) + "*)(__edg_frame + " + std::to_string(local->second) + "))";
    return name(v, "v");
  }
  return "(*(" + type(v->type) + "*)(__edg_base + " + std::to_string(it->second) + "))";
}
string constant(a_constant_ptr c) {
  auto token=virtual_type_tokens.find(c);
  if(token!=virtual_type_tokens.end())return "(("+type(c->type)+")(__edg_type_token_base + "+std::to_string(8*token->second)+"))";
  if (c->kind == ck_string) return "(*(" + type(c->type) + "*)" + literal_address(c) + ")";
  if (c->kind == ck_aggregate) {
    auto found = aggregate_constants.find(c);
    if (found != aggregate_constants.end()) return found->second + "()";
    string ty = type(c->type), id = name(c, "__edg_constant");
    aggregate_constants[c] = id;
    // Aggregate constants have no runtime local captures; pointer relocations
    // may refer to the current owner's arena, hence an instance helper in that mode.
    string body = initializer("result", c->type, c);
    helper_output += "// dot-gen:member-begin\nprivate " + string(instance_mode ? "" : "static ") +
                     ty + " " + id + "() {\n" + ty + " result = default;\n" + body +
                     "return result; }\n// dot-gen:member-end\n";
    return id + "()";
  }
  if (c->kind == ck_address) {
    string metadata=typeinfo_constant(c);
    if(!metadata.empty())return metadata;
    if (c->variant.address.kind == abk_routine) {
      if (c->variant.address.offset) unsupported("function address offset");
      return "((" + type(c->type) + ")" + routine_address(c->variant.address.variant.routine) + ")";
    }
    if (c->variant.address.kind == abk_constant)
      return "((" + type(c->type) + ")(" + literal_address(c->variant.address.variant.constant) + " + (" +
             std::to_string(c->variant.address.offset) + ")))";
    if (c->variant.address.kind != abk_variable || !c->variant.address.variant.variable)
      unsupported("address constant", c->variant.address.kind);
    auto v = c->variant.address.variant.variable;
    if (!global_offsets.count(v) && !tls_offsets.count(v) && libc_variable(v).empty()) unsupported(("address of unallocated global: "+string(v->source_corresp.name?v->source_corresp.name:"<unnamed>")).c_str());
    return "((" + type(c->type) + ")((byte*)&" + variable(v) + " + (" +
           std::to_string(c->variant.address.offset) + ")))";
  }
  if(c->kind==ck_float) {
    auto t=skip_typerefs(c->type);
    string bits=fp_to_hex_string(t->variant.float_kind,&c->variant.float_value).as_temp_characters();
    string ty=type(t);
    if(ty=="global::Edg.Runtime.Float128") {
      if(bits.size()!=32)unsupported("binary128 constant encoding");
      return "global::Edg.Runtime.Float128.FromBits(((global::System.UInt128)0x"+bits.substr(0,16)+"UL<<64)|0x"+bits.substr(16)+"UL)";
    }
    if(ty=="global::System.Half")return "global::System.BitConverter.UInt16BitsToHalf(0x"+bits+")";
    if(ty=="global::Edg.Runtime.Float80") {
      if(bits.size()<20 || (bits.size()>20 && bits.substr(0,bits.size()-20).find_first_not_of('0')!=string::npos))
        unsupported("binary80 constant encoding");
      bits=bits.substr(bits.size()-20);
      return "new global::Edg.Runtime.Float80(0x"+bits.substr(4)+"UL, 0x"+bits.substr(0,4)+")";
    }
    return ty=="float" ? "global::System.BitConverter.Int32BitsToSingle(unchecked((int)0x"+bits+"U))" :
      "global::System.BitConverter.Int64BitsToDouble(unchecked((long)0x"+bits+"UL))";
  }
  if (c->kind != ck_integer) unsupported("constant", c->kind);
  string ty = type(c->type);
  if(skip_typerefs(c->type)->size==16) {
    auto high=c->variant.integer_value; a_boolean overflow;
    auto low=unsigned_value_of_integer_value(&high,FALSE,&overflow);
    shift_right_integer_value(&high,64,FALSE,FALSE);
    auto hi=unsigned_value_of_integer_value(&high,FALSE,&overflow);
    char text[160];snprintf(text,sizeof(text),"(((global::System.UInt128)0x%llxUL << 64) | (global::System.UInt128)0x%llxUL)",(unsigned long long)hi,(unsigned long long)low);
    return "unchecked(("+ty+")"+text+")";
  }
  a_boolean overflow;
  auto bits = unsigned_value_of_integer_constant(c, &overflow);
  // Signed negatives are represented via their unsigned bits; avoid C#'s
  // special treatment of decimal minimum signed literals.
  if (overflow && !int_constant_is_signed(c)) unsupported("wide integer constant");
  char text[64];
  snprintf(text, sizeof(text), "0x%llxUL", (unsigned long long)bits);
  return "unchecked((" + ty + ")" + text + ")";
}
string initializer(string dst, a_type_ptr t, a_constant_ptr c) {
  t = skip_typerefs(t);
  if(t->kind==tk_array && !t->size) {
    // c_gen_be.c::dump_initializer_part accepts empty zero-bound and flexible
    // array initializers. These members have address-only views here,
    // not CLR storage fields. Consume their empty initializer without naming
    // or assigning a fictitious one-byte managed marker object.
    if(t->variant.array.is_vla || t->variant.array.is_template_dependent_size_array ||
       !((c->kind==ck_aggregate && !c->variant.aggregate.first_constant) ||
         (c->kind==ck_string && !c->variant.string.length)))
      unsupported("nonempty initializer for zero-storage array");
    return "";
  }
  if (c->kind == ck_string) {
    if (t->kind != tk_array || c->variant.string.length > t->size) unsupported("string initializer bounds");
    string result = dst + " = default;\n";
    for (unsigned long long i = 0; i < c->variant.string.length; ++i)
      result += "((byte*)&(" + dst + "))[" + std::to_string(i) + "] = " +
                std::to_string((unsigned char)c->variant.string.value[i]) + ";\n";
    return result;
  }
  if (c->kind != ck_aggregate) return dst + " = " + constant(c) + ";\n";
  if (!aggregate(t)) unsupported("aggregate initializer type");
  string result = dst + " = default;\n";
  auto field = t->kind == tk_array ? nullptr : next_non_empty_initializable_field(t->variant.class_struct_union.field_list);
  unsigned long long index = 0;
  for (auto item = c->variant.aggregate.first_constant; item; item = item->next) {
    if (item->kind == ck_designator) {
      // c_gen_be.c::dump_initializer_part retains lowered designators only
      // to select a union member; the following constant supplies its value.
      if (t->kind != tk_union || item->variant.designator.is_generic ||
          !item->variant.designator.is_field_designator)
        unsupported("lowered aggregate designator");
      auto selected = item->variant.designator.variant.field;
      bool found = false;
      for (auto candidate = t->variant.class_struct_union.field_list;
           candidate; candidate = candidate->next)
        if (candidate == selected) { found = true; break; }
      if (!found || !item->next || item->next->kind == ck_designator)
        unsupported("lowered union designator field or value");
      field = selected;
      item = item->next;
    }
    auto value = item;
    unsigned long long count = 1;
    if (item->kind == ck_init_repeat) {
      count = item->variant.init_repeat.count;
      value = item->variant.init_repeat.constant;
    } else if(t->kind==tk_array) {
      // EDG can expand repeated aggregate designators into adjacent equal
      // constants rather than ck_init_repeat. Its strict representation-aware
      // comparison preserves signed zero, pointer identity and relocations.
      while(item->next && item->next->kind!=ck_init_repeat &&
            compare_constants(value,item->next,CC_STRICTLY_IDENTICAL|CC_EXACT_EQUIVALENCE)) {
        ++count;item=item->next;
      }
    }
    if(t->kind==tk_array && count>1) {
      auto element=t->variant.array.element_type;
      auto elements=t->variant.array.variant.number_of_elements;
      if(index>elements || count>elements-index)unsupported("array initializer repetition bounds");
      if(skip_typerefs(value->type)->size!=skip_typerefs(element)->size)
        unsupported("flattened multidimensional initializer");
      a_boolean overflow=FALSE;
      bool zero=value->kind==ck_integer && unsigned_value_of_integer_constant(value,&overflow)==0 && !overflow;
      if(!zero) {
        string counter="__edg_repeat"+std::to_string(++serial);
        result+="for(ulong "+counter+"=0; "+counter+"<"+std::to_string(count)+"; ++"+counter+") {\n";
        result+=initializer("(("+type(element)+"*)&("+dst+"))["+std::to_string(index)+"+"+counter+"]",element,value);
        result+="}\n";
      }
      index+=count;continue;
    }
    for (unsigned long long repeat = 0; repeat < count; ++repeat) {
      if (t->kind == tk_array) {
        auto element = t->variant.array.element_type;
        if (index >= t->variant.array.variant.number_of_elements) unsupported("array initializer bounds");
        if (skip_typerefs(value->type)->size != skip_typerefs(element)->size)
          unsupported("flattened multidimensional initializer");
        result += initializer("((" + type(element) + "*)&(" + dst + "))[" + std::to_string(index++) + "]", element, value);
      } else {
        if (!field || count != 1) unsupported("record initializer fields");
        result += initializer("(" + dst + ")." + name(field, "m"), field->type, value);
        field = next_non_empty_initializable_field(field->next);
      }
    }
  }
  return result;
}
string expr(an_expr_node_ptr e);
string gnu_statement_expression(an_expr_node_ptr e);
string statement_expression_result(an_expr_node_ptr e);
void ensure_lifecycle();
bool selected_runtime_branch(a_statement_ptr s,a_statement_ptr &branch) {
  // c_gen_be.c emits only the selected branch of constexpr/consteval forms.
  switch(s->kind) {
    case stmk_constexpr_if:
      branch=s->variant.constexpr_if->value?s->variant.constexpr_if->then_statement:s->variant.constexpr_if->else_statement;
      return true;
    case stmk_if_consteval:branch=s->variant.if_stmt.else_statement;return true;
    case stmk_if_not_consteval:branch=s->variant.if_stmt.then_statement;return true;
    default:return false;
  }
}
string source_jump_exclusion(const string &error);
#include "dot_gen_arrays.h"
#include "dot_gen_vla.h"
#include "dot_gen_cleanup.h"
#include "dot_gen_setjmp.h"
#include "dot_gen_terminate.h"
#include "dot_gen_exceptions.h"
#include "dot_gen_lifecycle.h"
#include "dot_gen_virtual.h"
#include "dot_gen_rtti.h"
#include "dot_gen_typeinfo.h"
#include "dot_gen_exception_ptr.h"
#include "dot_gen_unexpected.h"
#include "dot_gen_runtime.h"
string location(an_expr_node_ptr e);
#include "dot_gen_materialize.h"
#include "dot_gen_sequence.h"
#include "dot_gen_varargs.h"
#include "dot_gen_atomics.h"
#include "dot_gen_intrinsics.h"

string condition(an_expr_node_ptr e) {
  return "(" + expr(e) + (skip_typerefs(e->type)->kind == tk_pointer ? " != null)" : " != 0)");
}
string convert(a_type_ptr t, string value) {
  return "unchecked((" + type(t) + ")(" + value + "))";
}
string arithmetic_type(a_type_ptr a,a_type_ptr b) {
  a=skip_typerefs(a);b=skip_typerefs(b);
  if((a->kind==tk_float && type(a)=="global::Edg.Runtime.Float128") || (b->kind==tk_float && type(b)=="global::Edg.Runtime.Float128"))return "global::Edg.Runtime.Float128";
  if((a->kind==tk_float && a->size==16)||(b->kind==tk_float && b->size==16))return "global::Edg.Runtime.Float80";
  if(a->kind==tk_float || b->kind==tk_float)
    return (type(a)=="double" || type(b)=="double")?"double":"float";
  unsigned as=(unsigned)std::max<a_targ_size_t>(4,a->size),bs=(unsigned)std::max<a_targ_size_t>(4,b->size);
  bool au=a->size>=4 && !int_type_is_signed(a),bu=b->size>=4 && !int_type_is_signed(b);
  unsigned size=std::max(as,bs);bool uns=(au&&as>=bs)||(bu&&bs>=as);
  if(size==16)return uns?"global::System.UInt128":"global::System.Int128";
  return size==8?(uns?"ulong":"long"):(uns?"uint":"int");
}
// Atomic qualification is retained by IL and consumed by the access adapters.
#include "dot_gen_c11_atomics.h"
// Bitfields are properties backed by the exact EDG byte/bit positions. Their
// mutation helper takes the containing object's address, evaluating it once.
a_field_ptr bitfield(an_expr_node_ptr e) {
  if(e->kind!=enk_operation)return nullptr;
  auto op=e->variant.operation.kind;
  if(op!=eok_dot_field && op!=eok_points_to_field)return nullptr;
  auto f=node_field(e->variant.operation.operands->next);
  return f->is_bit_field?f:nullptr;
}
string bitfield_mutation(an_expr_node_ptr e,an_expr_node_ptr rhs,const string &op,bool post=false) {
  auto f=bitfield(e);auto base=e->variant.operation.operands;
  bool pointer=e->variant.operation.kind==eok_points_to_field;
  auto obj=pointer?skip_typerefs(base->type)->variant.pointer.type:base->type;
  string ty=type(e->type),rt=rhs?type(rhs->type):ty;
  string id="__edg_bits_mutate"+std::to_string(++serial),field="p->"+name(f,"m");
  string calc=arithmetic_type(f->type,rhs?rhs->type:f->type);
  string value=op=="="?"rhs":"(("+calc+")"+field+") "+op+" "+
      ((op=="<<"||op==">>")?"(int)rhs":"(("+calc+")rhs)");
  helper_output+="// dot-gen:member-begin\nprivate static "+ty+" "+id+"("+rt+" rhs, "+type(obj)+"* p) { "+
      (post?ty+" old="+field+"; ":"")+field+"=unchecked(("+type(f->type)+")("+value+")); return "+(post?"old":field)+"; }\n// dot-gen:member-end\n";
  return id+"("+(rhs?expr(rhs):"1")+", "+(pointer?expr(base):"&("+location(base)+")")+")";
}
string location(an_expr_node_ptr e) {
  if (!e) unsupported("null expression");
  diagnostic_scope diagnostic(e->position);
  string jump=source_jump_expression(e);
  if(!jump.empty())return jump;
  string materialized=materialized_location(e);
  if(!materialized.empty())return materialized;
  string metadata=typeinfo_expression(e);
  if(!metadata.empty())return metadata;
  switch (e->kind) {
    case enk_constant: return constant(node_constant(e));
    case enk_routine: return routine_address(e->variant.routine.ptr);
    case enk_variable: return variable(node_variable(e));
    case enk_statement: return gnu_statement_expression(e);
    case enk_object_lifetime: return sequence_expression(e);
    case enk_throw: return sequence_expression(e);
    case enk_lowered_eh_construct: return exception_address(e);
    case enk_result_of_overriding_function: {
      // c_gen_be.c::dump_result_of_overriding_function forwards the wrapper's
      // parameters after EDG's this/covariant-result adjustments.
      auto r=current_function_scope->variant.routine.ptr->overriding_function_for_wrapper;
      if(!r || r->function_def_number==NULL_function_def_number)unsupported("external virtual thunk target");
      string call=name(r,"f")+"(";
      for(auto p=current_function_scope->variant.routine.parameters;p;p=p->next) {
        if(p!=current_function_scope->variant.routine.parameters)call+=", ";
        call+=variable(p);
      }
      if(variadic_routine(current_function_scope->variant.routine.ptr->type))
        call+=string(current_function_scope->variant.routine.parameters?", ":"")+"__edg_arguments";
      return call+")";
    }
    case enk_operation: break;
    default: unsupported("expression", e->kind);
  }
  auto op = e->variant.operation.kind;
  auto a = e->variant.operation.operands;
  auto b = a ? a->next : nullptr;
  string atomic_mutation=c11_atomic_mutation(e);
  if(!atomic_mutation.empty())return atomic_mutation;
  string vararg_operation=va_operation(e);
  if(!vararg_operation.empty())return vararg_operation;
  if (op == eok_comma) return sequence_expression(e);
  if (op == eok_lvalue_adjust) return "(*(" + type(e->type) + "*)&(" + location(a) + "))";
  if (op == eok_call) {
    string intrinsic=intrinsic_call(e);
    if(!intrinsic.empty())return intrinsic;
    string packed=variadic_call(a,b);
    if(!packed.empty())return packed;
    string call;
    if (a->kind == enk_routine) {
      auto r = a->variant.routine.ptr;
      string source=r->source_corresp.name?r->source_corresp.name:"";
      string vla=vla_call(r,b);
      if(!vla.empty())return vla;
      string array_call=array_allocation_call(r,b);
      if(!array_call.empty())return array_call;
      string lifecycle=lifecycle_call(r,b);
      if(!lifecycle.empty())return lifecycle;
      string primary_exception=exception_ptr_call(r,b);
      if(!primary_exception.empty())return primary_exception;
      string unexpected=unexpected_call(r,b);
      if(!unexpected.empty())return unexpected;
      string provider=runtime_provider_call(r,b);
      if(!provider.empty())return provider;
      string atomic=atomic_call(r,b);
      if(!atomic.empty())return atomic;
      if (r->compiler_generated && r->function_def_number == NULL_function_def_number && source=="__dynamic_cast")
        return dynamic_cast_call(r,b);
      if (r->compiler_generated && r->function_def_number == NULL_function_def_number && (source=="__cxa_vec_ctor" || source=="__cxa_vec_dtor")) {
        auto rt=skip_typerefs(r->type);
        if(skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void) unsupported("array helper return ABI");
        size_t count=0; for(auto arg=b;arg;arg=arg->next)++count;
        if(count!=(source=="__cxa_vec_ctor"?5:4))unsupported("array helper argument ABI");
        ensure_array_helpers();
        call=(source=="__cxa_vec_ctor"?"__edg_vec_ctor(":"__edg_vec_dtor(");
      } else if (r->compiler_generated && r->function_def_number == NULL_function_def_number &&
                 (source=="memcpy" || source=="memset")) {
        // EDG lowers base-subobject assignment to a byte-counted memcpy so
        // reused tail padding is not overwritten. Value initialization may
        // likewise use memset. These are compiler helpers, not libc imports.
        auto rt=skip_typerefs(r->type);
        auto params=rt->variant.routine.extra_info->param_type_list;
        if(type(rt->variant.routine.return_type)!="void*" || !params || type(params->type)!="void*" ||
           !params->next || type(params->next->type)!=(source=="memcpy"?"void*":"int") ||
           !params->next->next || type(params->next->next->type)!="ulong" || params->next->next->next ||
           rt->variant.routine.extra_info->has_ellipsis)unsupported("memory helper ABI");
        string id="__edg_"+source;
        if(!assignment_helpers.count(id)) {
          assignment_helpers[id]=id;
          helper_output+="// dot-gen:member-begin\nprivate static void* "+id+"(void* dst, "+
              (source=="memcpy"?"void* src":"int value")+", ulong count) {\n"+
              (source=="memcpy"?"if(count!=0)global::System.Buffer.MemoryCopy(src,dst,count,count);\n":
                                  "if(count!=0)global::System.Runtime.InteropServices.NativeMemory.Fill(dst,(nuint)count,unchecked((byte)value));\n")+
              "return dst; }\n// dot-gen:member-end\n";
        }
        call=id+"(";
      } else {
        if ((r->function_def_number == NULL_function_def_number || !scope_for_routine_or_null(r)) && !libc_routine(r) && !thread_routine(r))
          unsupported(("external call: "+source).c_str());
        call = name(r, "f") + "(";
      }
    } else {
      call = expr(a) + "(";
      if (instance_mode) call += "this";
    }
    bool separator = a->kind != enk_routine && instance_mode;
    for (auto arg = b; arg; arg = arg->next) {
      if (separator) call += ", ";
      call += expr(arg); separator = true;
    }
    return call + ")";
  }
  if (op == eok_array_to_pointer) return "((" + type(e->type) + ")&(" + location(a) + "))";
  if (op == eok_dot_field || op == eok_points_to_field) {
    auto field = node_field(b);
    auto object=skip_typerefs(a->type);
    if(op==eok_points_to_field)object=skip_typerefs(object->variant.pointer.type);
    if(user_typeinfo(object))
      return typeinfo_field(field,op==eok_dot_field?"&("+location(a)+")":expr(a),a);
    auto ft=skip_typerefs(field->type);
    if(ft->kind==tk_array && !ft->size) {
      string base=op==eok_dot_field?"&("+location(a)+")":expr(a);
      return "(*("+type(field->type)+"*)((byte*)("+base+") + "+std::to_string(field->offset)+"))";
    }
    return "(" + (op==eok_dot_field?location(a):expr(a)) + (op == eok_dot_field ? ")." : ")->") + name(field, "m");
  }
  if (op == eok_bassign) {
    if (!aggregate(a->type) || skip_typerefs(a->type) != skip_typerefs(b->type))
      unsupported("partial or cross-type block copy");
    // EDG uses distinct, truncated subobject types for tail-padding reuse.
    // Pack=1 and the explicit EDG size keep these copies within that subobject.
    auto ty = type(a->type);
    string id = "__edg_copy_" + ty;
    if (!assignment_helpers.count(id)) {
      assignment_helpers[id] = id;
      helper_output += "// dot-gen:member-begin\nprivate static void " + id + "(" + ty +
                       " rhs, " + ty + "* lhs) { *lhs = rhs; }\n// dot-gen:member-end\n";
    }
    return id + "(" + expr(b) + ", &(" + location(a) + "))";
  }
  if (op == eok_address_of && a->kind == enk_routine) return expr(a);
  if (op == eok_address_of || op == eok_reference_to) return "(&" + location(a) + ")";
  if (op == eok_indirect && function_pointer(a->type)) return expr(a);
  if (op == eok_indirect || op == eok_ref_indirect) return "(*(" + expr(a) + "))";
  if (op == eok_subscript) {
    // C++17 sequences the syntactic left operand first, even for index[p].
    return "(*(" + expr(a) + " + " + expr(b) + "))";
  }
  if (op == eok_cast) {
    if (skip_typerefs(e->type)->kind == tk_void) unsupported("retained void cast");
    return convert(e->type, expr(a));
  }
  if (op == eok_parens) return "(" + location(a) + ")";
  if (op == eok_question)
    return "(" + condition(a) + " ? " + expr(b) + " : " + expr(b->next) + ")";
  if (op == eok_not) return "(" + condition(a) + " ? 0 : 1)";
  if (op == eok_land || op == eok_lor)
    return "((" + condition(a) + (op == eok_land ? " && " : " || ") + condition(b) + ") ? 1 : 0)";
  if(op==eok_negate) {
    auto operand_type=skip_typerefs(a->type);
    // C/C++ unsigned negation is modular subtraction. C# has no unary minus
    // for ulong, so preserve EDG's promoted operand width explicitly.
    if(operand_type->kind==tk_integer && !int_type_is_signed(operand_type))
      return convert(e->type,"("+type(operand_type)+")0 - ("+expr(a)+")");
  }
  if (op == eok_negate || op == eok_unary_plus || op == eok_complement)
    return convert(e->type, string(op == eok_negate ? "-" : op == eok_unary_plus ? "+" : "~") + expr(a));
  if (op == eok_pre_incr || op == eok_pre_decr || op == eok_post_incr || op == eok_post_decr) {
    string token = (op == eok_pre_incr || op == eok_post_incr) ? "++" : "--";
    bool pre = op == eok_pre_incr || op == eok_pre_decr;
    if(bitfield(a))return bitfield_mutation(a,nullptr,token=="++"?"+":"-",!pre);
    if(get_type_qualifiers(a->type)&TQ_VOLATILE) {
      string id="__edg_volatile_inc"+std::to_string(++serial),ty=type(a->type);
      helper_output+="// dot-gen:member-begin\nprivate static "+ty+" "+id+"("+ty+"* p) {global::System.Threading.Thread.MemoryBarrier(); "+ty+" old=*p; "+ty+" value=unchecked(("+ty+")(old "+(token=="++"?"+":"-")+" 1)); *p=value;global::System.Threading.Thread.MemoryBarrier();return "+(pre?"value":"old")+";}\n// dot-gen:member-end\n";
      return id+"(&("+location(a)+"))";
    }
    return "unchecked(" + (pre ? token + location(a) : location(a) + token) + ")";
  }
  const char *token = nullptr;
  bool comparison = false, assignment = false;
  switch (op) {
#define BIN(kind, text) case kind: token = text; break
    BIN(eok_padd, "+"); BIN(eok_psubtract, "-"); BIN(eok_pdiff, "-");
    BIN(eok_add, "+"); BIN(eok_subtract, "-"); BIN(eok_multiply, "*");
    BIN(eok_divide, "/"); BIN(eok_remainder, "%");
    BIN(eok_and, "&"); BIN(eok_or, "|"); BIN(eok_xor, "^");
    BIN(eok_shiftl, "<<"); BIN(eok_shiftr, ">>");
#undef BIN
#define CMP(kind, text) case kind: token = text; comparison = true; break
    CMP(eok_eq, "=="); CMP(eok_ne, "!="); CMP(eok_lt, "<");
    CMP(eok_le, "<="); CMP(eok_gt, ">"); CMP(eok_ge, ">=");
#undef CMP
#define ASSIGN(kind, text) case kind: token = text; assignment = true; break
    ASSIGN(eok_padd_assign, "+="); ASSIGN(eok_psubtract_assign, "-=");
    ASSIGN(eok_assign, "="); ASSIGN(eok_add_assign, "+=");
    ASSIGN(eok_subtract_assign, "-="); ASSIGN(eok_multiply_assign, "*=");
    ASSIGN(eok_divide_assign, "/="); ASSIGN(eok_remainder_assign, "%=");
    ASSIGN(eok_and_assign, "&="); ASSIGN(eok_or_assign, "|="); ASSIGN(eok_xor_assign, "^=");
    ASSIGN(eok_shiftl_assign, "<<="); ASSIGN(eok_shiftr_assign, ">>=");
#undef ASSIGN
    default: unsupported("operator", op);
  }
  if (assignment && !a->is_lvalue) unsupported("non-scalar assignment");
  if(assignment && bitfield(a)) {
    string operation=token;
    if(operation!="=")operation.pop_back();
    return bitfield_mutation(a,b,operation);
  }
  string rhs = expr(b);
  if (assignment) {
    // C++17 assignment evaluates RHS before LHS. C# assignment evaluates
    // LHS first; a typed helper's arguments establish the required order.
    string lhs_type = type(a->type), rhs_type = type(b->type);
    bool vol=(get_type_qualifiers(a->type)&TQ_VOLATILE)!=0;
    string key = lhs_type + ":" + token + ":" + rhs_type+(vol?":volatile":"");
    auto it = assignment_helpers.find(key);
    string helper;
    if (it != assignment_helpers.end()) helper = it->second;
    else {
      helper = "__edg_assign" + std::to_string(assignment_helpers.size());
      assignment_helpers[key] = helper;
      string operation=token,value="rhs";
      if(operation!="=") {
        operation.pop_back();
        if(skip_typerefs(a->type)->kind==tk_pointer)value="*lhs "+operation+" rhs";
        else {
          string calc=arithmetic_type(a->type,b->type);
          value="(("+calc+")(*lhs)) "+operation+" "+
              ((operation=="<<"||operation==">>")?"(int)rhs":"(("+calc+")rhs)");
        }
      }
      helper_output += "// dot-gen:member-begin\nprivate static " + lhs_type + " " + helper +
        "(" + rhs_type + " rhs, " + lhs_type + "* lhs) { "+string(vol?"global::System.Threading.Thread.MemoryBarrier(); ":"")+"*lhs = unchecked(("+lhs_type+")("+value+")); "+string(vol?"global::System.Threading.Thread.MemoryBarrier(); ":"")+"return *lhs; }\n// dot-gen:member-end\n";
    }
    return helper + "(" + rhs + ", &(" + location(a) + "))";
  }
  if (op == eok_shiftl || op == eok_shiftr) rhs = "unchecked((int)(" + rhs + "))";
  string lhs = expr(a);
  if (comparison && function_pointer(a->type)) {
    lhs = "((nuint)(" + lhs + "))";
    rhs = "((nuint)(" + rhs + "))";
  }
  string value = "(" + lhs + " " + token + " " + rhs + ")";
  if (comparison) return "(" + value + " ? 1 : 0)";
  return convert(e->type, value);
}
void line(string s) { output += s + "\n"; }
string expr(an_expr_node_ptr e) {
  string value=location(e);
  if(e->is_lvalue && c11_atomic_type(e->type))return c11_atomic_read(e,"&("+value+")");
  if(!(get_type_qualifiers(e->type)&TQ_VOLATILE) || !e->is_lvalue)return value;
  if(bitfield(e))unsupported("volatile bitfield access");
  string ty=type(e->type),key="volatile-read:"+ty,id;
  auto found=assignment_helpers.find(key);
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_volatile_read"+std::to_string(++serial);assignment_helpers[key]=id;
    // Retain a discarded volatile read: its callee must still compute the
    // returned value even when the caller ignores it. Barriers alone do not
    // prevent an inliner from eliminating an ordinary unused load.
    helper_output+="// dot-gen:member-begin\n[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\nprivate static "+ty+" "+id+"("+ty+"* p) {global::System.Threading.Thread.MemoryBarrier(); "+ty+" value=*p;global::System.Threading.Thread.MemoryBarrier();return value;}\n// dot-gen:member-end\n";
  }
  return id+"(&("+value+"))";
}

void variables(a_variable_ptr v, bool global) {
  for (; v; v = v->next) {
    if(!variable_needed(v))continue;
    if (v->is_thread_local) unsupported("thread-local storage");
    if (v->storage_class == sc_extern) unsupported(("external variable: "+string(v->source_corresp.name?v->source_corresp.name:"unnamed")).c_str());
    if (local_offsets.count(v)) continue;
    if (v->alignment > 8 || alignment_of_type(v->type) > 8)
      unsupported("overaligned local");
    string init = "default";
    if (v->init_kind == initk_static) init = constant(v->initializer.constant);
    else if (v->init_kind != initk_none && v->init_kind != initk_zero &&
             !(v->init_kind == initk_dynamic && !global))
      unsupported("variable initializer", v->init_kind);
    line(string(global ? (instance_mode ? "private " : "private static ") : "") +
         type(v->type) + " " + name(v, "v") + " = " + init + ";");
  }
}
void collect_constant_templates(a_scope_ptr scope) {
  std::map<a_variable_ptr,a_constant_ptr> inits;
  for(auto p=scope->local_static_variable_inits;p;p=p->next)
    if(p->init_kind==initk_static)inits[p->variable]=p->initializer.constant;
  for(auto v=scope->variables;v;v=v->next) {
    if(!variable_needed(v))continue;
    if(!v->lowering_generated || v->source_corresp.name || !aggregate(v->type))continue;
    auto t=skip_typerefs(v->type);
    // Partial EH lowering also leaves synthesized integer arrays (pointer cv
    // flags). Preserve their initialized storage just like constant templates;
    // do not guess which unnamed tables can be discarded.
    bool integer_array=t->kind==tk_array && skip_typerefs(t->variant.array.element_type)->kind==tk_integer;
    // Aggregate base initialization can use an unqualified template of the
    // source record type. Runtime-generated record/table types stay excluded.
    bool record_template=t->kind!=tk_array && !class_type_supp(t)->compiler_generated;
    if(!(get_type_qualifiers(v->type)&TQ_CONST) && !integer_array && !record_template)continue;
    a_constant_ptr c=v->init_kind==initk_static?v->initializer.constant:inits[v];
    if(!c)continue;
    constant_templates[v]=c; constant_template_variables.push_back(v);
  }
  for(auto s=scope->scopes;s;s=s->next)collect_constant_templates(s);
}
void collect_local_static_storage(a_scope_ptr scope) {
  std::map<a_variable_ptr,a_constant_ptr> inits;
  for(auto p=scope->local_static_variable_inits;p;p=p->next)
    if(p->init_kind==initk_static)inits[p->variable]=p->initializer.constant;
  for(auto v=scope->variables;v;v=v->next) {
    if(!variable_needed(v))continue;
    if(v==cleanup_region_var || v==cleanup_array_var || exception_metadata.count(v) || constant_templates.count(v) || v->storage_class==sc_extern)continue;
    if(inits.count(v))constant_templates[v]=inits[v];
    local_static_storage.push_back(v);
  }
  for(auto s=scope->scopes;s;s=s->next)collect_local_static_storage(s);
}
void globals(a_variable_ptr first, const char *cls) {
  std::vector<a_variable_ptr> stored_variables;
  for(auto v=first;v;v=v->next) {
    if(!variable_needed(v))continue;
    // The C backend emits declarations, not storage, for extern variables.
    // Cast-only classes can leave unused vtables/RTTI declarations behind.
    // Actual references still reject in variable() and constant().
    if(v->storage_class==sc_extern)continue;
    stored_variables.push_back(v);
  }
  stored_variables.insert(stored_variables.end(),constant_template_variables.begin(),constant_template_variables.end());
  stored_variables.insert(stored_variables.end(),local_static_storage.begin(),local_static_storage.end());
  unsigned long long size[2] = {0, 0}, alignment[2] = {8, 8};
  bool has_tls = false;
  for (auto v : stored_variables) {
    if(exception_metadata.count(v))continue;
    diagnostic_scope diagnostic(v->source_corresp.decl_position);
    unsigned n = v->is_thread_local ? 1 : 0;
    has_tls |= n != 0;
    if (v->storage_class == sc_extern) unsupported(("external variable: "+string(v->source_corresp.name?v->source_corresp.name:"unnamed")).c_str());
    if(!skip_typerefs(v->type)->size)unsupported("zero-sized standalone global storage");
    const char* saved_context=context;
    string global_context="global "+string(v->source_corresp.name?v->source_corresp.name:"<anonymous>");
    context=global_context.c_str();(void)type(v->type);context=saved_context;
    unsigned long long align = alignment_of_type(v->type);
    if (v->alignment > align) align = v->alignment;
    if (!align || (align & (align - 1)) || align > 4096) unsupported("global/TLS alignment");
    if (align > alignment[n]) alignment[n] = align;
    size[n] = (size[n] + align - 1) & ~(align - 1);
    (n ? tls_offsets : global_offsets)[v] = size[n];
    size[n] += skip_typerefs(v->type)->size;
    if (size[n] > 64 * 1024 * 1024) unsupported("global/TLS arena exceeds 64 MiB");
  }
  string mod = instance_mode ? "" : "static ";
  auto aligned_pointer = [](string storage, unsigned long long align) {
    return "(byte*)(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref "
           "global::System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(" + storage + ")) + " +
           std::to_string(align - 1) + ") & ~(nuint)" + std::to_string(align - 1) + ")";
  };
  line("private " + mod + "byte[] __edg_storage = global::System.GC.AllocateArray<byte>(" +
       std::to_string(size[0] + alignment[0] - 1) + ", pinned: true);");
  line("private " + mod + "byte* __edg_base => " + aligned_pointer("__edg_storage", alignment[0]) + ";");
  if (has_tls) {
    ensure_lifecycle();
    line("private " + mod + "byte* __edg_tls_base => " + aligned_pointer("(__edg_threads.Value!.Storage ?? (__edg_threads.Value!.Storage = __edg_create_tls()))", alignment[1]) + ";");
  }
  line(string(instance_mode ? "public " : "static ") + cls + "() {");
  line("if (sizeof(void*) != 8 || !global::System.BitConverter.IsLittleEndian) "
       "throw new global::System.PlatformNotSupportedException(\"EDG storage ABI requires little-endian 64-bit .NET\");");
  if(getenv("EDG_DOT_TARGET") && string(getenv("EDG_DOT_TARGET"))=="portable-lp64-binary80-v2")
    line("if(global::System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture is not (global::System.Runtime.InteropServices.Architecture.X64 or global::System.Runtime.InteropServices.Architecture.Arm64)) throw new global::System.PlatformNotSupportedException(\"EDG v2 target requires x64 or Arm64 .NET\");");
  for (auto v : stored_variables) {
    if(exception_metadata.count(v))continue;
    if (v->is_thread_local) continue;
    if (constant_templates.count(v)) line(initializer(variable(v),v->type,constant_templates[v]));
    else if (v->init_kind == initk_static) line(initializer(variable(v), v->type, v->initializer.constant));
    else if (v->init_kind != initk_none && v->init_kind != initk_zero && v->init_kind!=initk_dynamic) unsupported("global initializer", v->init_kind);
  }
  if(!startup_routines.empty()) {
    line("try {");for(auto r:startup_routines)line(name(r,"f")+"();");
    line("} catch(global::System.Exception error) {/*__EDG_GLOBAL_TERMINATE__*/}");
  }
  line("}");
  if (has_tls) {
    line("// dot-gen:member-begin");
    line("private " + mod + "byte[] __edg_create_tls() {");
    line("byte[] storage = global::System.GC.AllocateArray<byte>(" + std::to_string(size[1] + alignment[1] - 1) + ", pinned: true);");
    line("byte* p = " + aligned_pointer("storage", alignment[1]) + ";");
    emitting_tls_initializer = true;
    for (auto v : stored_variables) {
      if (!v->is_thread_local) continue;
      if(constant_templates.count(v))line(initializer(variable(v),v->type,constant_templates[v]));
      else if (v->init_kind == initk_static) line(initializer(variable(v), v->type, v->initializer.constant));
      else if (v->init_kind != initk_none && v->init_kind != initk_zero && v->init_kind!=initk_dynamic) unsupported("dynamic TLS initializer", v->init_kind);
    }
    emitting_tls_initializer = false;
    line("return storage; }");
    line("// dot-gen:member-end");
  }
}
void plan_frame(a_scope_ptr s) {
  for (auto v = s->nonstatic_variables; v; v = v->next) {
    if(!variable_needed(v))continue;
    diagnostic_scope diagnostic(v->source_corresp.decl_position);
    unsigned long long alignment = alignment_of_type(v->type);
    if (v->alignment > alignment) alignment = v->alignment;
    if (!aggregate(v->type) && alignment <= 8) continue;
    if(!skip_typerefs(v->type)->size)unsupported("zero-sized standalone local storage");
    (void)type(v->type);
    if (!alignment || (alignment & (alignment - 1)) || alignment > 4096)
      unsupported("local aggregate alignment");
    if (alignment > frame_alignment) frame_alignment = alignment;
    frame_size = (frame_size + alignment - 1) & ~(alignment - 1);
    local_offsets[v] = frame_size;
    frame_size += skip_typerefs(v->type)->size;
    if (frame_size + frame_alignment > 64 * 1024 * 1024) unsupported("local frame exceeds 64 MiB");
  }
  for (auto child = s->scopes; child; child = child->next) plan_frame(child);
}
void locals(a_scope_ptr s) {
  if (!s) return;
  for(auto v=s->variables;v;v=v->next)
    if(variable_needed(v) && v!=cleanup_region_var && v!=cleanup_array_var && !exception_metadata.count(v) && !constant_templates.count(v) && !global_offsets.count(v) && !tls_offsets.count(v))
      unsupported(("function-local static storage: "+string(v->source_corresp.name?v->source_corresp.name:"unnamed")).c_str());
  variables(s->nonstatic_variables, false);
  for (auto child = s->scopes; child; child = child->next) locals(child);
}

void statement(a_statement_ptr s) {
  if (!s) return;
  diagnostic_scope diagnostic(s->position);
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){statement(selected);return;}
  switch (s->kind) {
    case stmk_stmt_expr_result:line(statement_expression_result(s->expr));break;
    case stmk_decl: break; // Declarations are hoisted; stmk_init retains execution order.
    case stmk_empty: line(";"); break;
    case stmk_expr:
      output+=source_jump_prefix(s->expr);
      if (s->expr && s->expr->kind == enk_lowered_eh_construct) {
        auto &eh = s->expr->variant.lowered_eh;
        switch (eh.kind) {
          case leck_cleanup_state:
            line(cleanup_transition(eh.variant.cleanup_region_number,"__edg_cleanup_state")); break;
          // These are annotations, not runtime state changes.
          case leck_unreachable_cleanup_state: case leck_function_prologue: case leck_function_epilogue: break;
          case leck_exception_caught: line("__edg_current_catch!.MarkCaught();"); break;
          case leck_try_epilogue: {
            auto found=try_frames.find(eh.variant.epilogue_try_block);
            if(found==try_frames.end())unsupported("try epilogue without try");
            line(found->second+" = false;"); break;
          }
          case leck_catch_epilogue: {
            auto found=catch_frames.find(eh.variant.epilogue_handler);
            if(found==catch_frames.end())unsupported("catch epilogue outside handler");
            line(found->second+".Leave();"); break;
          }
          default: unsupported("lowered EH statement", eh.kind);
        }
      } else line(expression_effect(s->expr));
      break;
    case stmk_return:
      if (s->variant.return_dynamic_init) unsupported("return initialization");
      // EDG's C backend emits a bare return for the unreachable/fall-through
      // path of a value-returning function. C# requires a typed return or throw.
      line(s->expr ? "return " + expr(s->expr) + ";" : function_returns_void ? "return;" :
           "throw new global::System.InvalidProgramException(\"C++ function returned without a value\");"); break;
    case stmk_block:
      // C++ permits jumping into ordinary nested blocks. Locals are already
      // hoisted; retain EDG's initialization/cleanup statements, not C# braces.
      for (auto child = s->variant.block.statements; child; child = child->next) statement(child);
      break;
    case stmk_try_block: {
      ensure_exception_helpers();
      cleanup_cursor_used=true;
      auto t=s->variant.try_block;
      string id="__edg_try"+std::to_string(++serial);
      try_frames[t]=id+"_active";
      exception_locals+="bool "+id+"_active = false;\n";
      line(id+"_active = true; { int "+id+"_boundary = __edg_cleanup_state; try {");
      statement(t->statement);
      string matches;
      bool catches_all=false;
      for(auto h=t->handlers;h;h=h->next) {
        if(!h->parameter) { catches_all=true; break; }
        if(!matches.empty())matches+=" || ";
        matches+=exception_match("((__edg_exception)"+id+"_error).Type",catch_type(h));
      }
      matches=catches_all?"true":"("+id+"_error is __edg_exception && ("+matches+"))";
      line("} catch (global::System.Exception "+id+"_error) when ("+id+"_active && __edg_begin_exception("+id+"_error) && "+matches+") {");
      line(cleanup_unwind(id+"_boundary"));
      bool first=true,ellipsis=false;
      for(auto h=t->handlers;h;h=h->next) {
        string test=h->parameter?id+"_error is __edg_exception "+id+"_typed"+name(h,"h")+
            " && "+exception_match(id+"_typed"+name(h,"h")+".Type",catch_type(h)):"true";
        line(string(first?"if (":"else if (")+test+") {");
        first=false; ellipsis|=!h->parameter;
        string frame=id+"_frame"+name(h,"h");
        catch_frames[h]=frame;
        line("var "+frame+" = "+exception_catch_frame(id+"_error",h->parameter?catch_type(h):nullptr)+"; try {");
        statement(h->statement);
        line("} catch (global::System.Exception handler_error) when (__edg_catch_filter("+frame+", handler_error)) { throw; } finally { if ("+frame+".Active) { "+cleanup_unwind(id+"_boundary")+" } "+frame+".Leave(); }");
        catch_frames.erase(h);
        line("}");
      }
      if(!ellipsis)line("else { throw; }");
      line("} }");
      break;
    }
    case stmk_if: {
      string id=name(s,"__edg_if");
      output+=source_jump_prefix(s->expr);
      line("if (!"+condition(s->expr)+") goto "+id+"_else;");
      statement(s->variant.if_stmt.then_statement);line("goto "+id+"_end;");
      line(id+"_else: ;");statement(s->variant.if_stmt.else_statement);line(id+"_end: ;");break;
    }
    case stmk_while: case stmk_end_test_while: {
      string id=name(s,"__edg_loop");line(id+"_head: ;");
      if(s->kind==stmk_while)output+=source_jump_prefix(s->expr);
      if(s->kind==stmk_while)line("if (!"+condition(s->expr)+") goto "+id+"_end;");
      statement(s->variant.loop_statement);
      if(s->kind==stmk_end_test_while)output+=source_jump_prefix(s->expr);
      line(s->kind==stmk_while?"goto "+id+"_head;":"if "+condition(s->expr)+" goto "+id+"_head;");
      if(s->kind==stmk_while)line(id+"_end: ;");
      break;
    }
    case stmk_for: {
      auto f = s->variant.for_loop.extra_info;
      statement(f->initialization);
      string id=name(s,"__edg_for");line(id+"_head: ;");
      output+=source_jump_prefix(s->expr);
      if(s->expr)line("if (!"+condition(s->expr)+") goto "+id+"_end;");
      statement(s->variant.for_loop.statement);line(expression_effect(f->increment));
      line("goto "+id+"_head;");if(s->expr)line(id+"_end: ;");break;
    }
    case stmk_switch: {
      string id=name(s,"__edg_switch");
      output+=source_jump_prefix(s->expr);
      line(type(s->expr->type)+" "+id+"_value = "+expr(s->expr)+";");
      auto descr=s->variant.switch_stmt.extra_info;
      for(auto c=descr->cases;c;c=c->next) {
        if(!c->case_value)continue;
        string test=id+"_value == "+constant(c->case_value);
        if(c->range_end)test=id+"_value >= "+constant(c->case_value)+" && "+id+"_value <= "+constant(c->range_end);
        line("if ("+test+") goto "+name(c,"__edg_case")+";");
      }
      line("goto "+(descr->default_case?name(descr->default_case,"__edg_case"):id+"_end")+";");
      statement(s->variant.switch_stmt.body_statement);
      if(!descr->default_case)line(id+"_end: ;");
      break;
    }
    case stmk_switch_case:line(name(s->variant.switch_case.extra_info,"__edg_case")+": ;");break;
    case stmk_label: line(name(s->variant.label.ptr, "label") + ": ;"); break;
    case stmk_goto: line("goto " + name(s->variant.label.ptr, "label") + ";"); break;
    case stmk_init: {
      auto d = s->variant.dynamic_init;
      // Like c_gen_be's dump_whole_variable_dynamic_init, dik_none has
      // no initialization to emit. Lowering already emitted construction
      // and registered any destruction through the cleanup region tables.
      if (d->kind == dik_none) break;
      // A constant aggregate initializer may retain its destructor metadata.
      // Destruction is emitted separately by EDG and the region-table walker,
      // as in c_gen_be; its presence does not change the initialization.
      if (!d->variable) unsupported("initialization without a variable");
      string value;
      if (d->kind == dik_expression) value = expr(d->variant.expression);
      else if (d->kind == dik_constant) {
        line(initializer(variable(d->variable), d->variable->type, d->variant.constant.ptr));
        break;
      }
      else if (d->kind == dik_zero) value = "default";
      else unsupported("dynamic initialization", d->kind);
      line(variable(d->variable) + " = " + value + ";"); break;
    }
    default: unsupported("statement", s->kind);
  }
}
#include "dot_gen_statement_expression.h"
#include "dot_gen_main.h"
#include "dot_gen_coroutine.h"
}

// EDG's custom-backend entry point intentionally has global namespace linkage.
void back_end(void) {
  reset_owned_ir();
  source_jumps_used=false;owned_ir_enabled=getenv("EDG_DOT_TYPE_IR") && string(getenv("EDG_DOT_TYPE_IR"))=="1";
  storage_record_ids.clear();array_storage_types.clear();
  diagnostic_position={};
  names.clear(); global_offsets.clear(); tls_offsets.clear(); assignment_helpers.clear();
  emitting_tls_initializer = false;
  helper_output.clear(); type_output.clear(); aggregate_types.clear(); aggregate_constants.clear();
  address_output.clear(); routine_addresses.clear(); literal_bytes.clear(); literal_offsets.clear();
  local_offsets.clear(); serial = 0; output.clear();
  expression_variables.clear(); expression_serial=0; cleanup_slot="__edg_cleanup_state"; array_helpers_emitted=false; array_allocation_helpers.clear();
  context = "file scope";
  exception_helpers_emitted=false; pending_exception.clear(); exception_metadata.clear(); catch_frames.clear();
  terminate_helpers_emitted=false;
  unexpected_helpers_emitted=false;bad_exception_factory=nullptr;bad_exception_type=nullptr;
  exception_classes.clear(); exception_destructors.clear();
  exception_types.clear(); exception_matchers.clear();
  constant_templates.clear(); constant_template_variables.clear();
  virtual_type_tokens.clear(); virtual_types.clear(); rtti_helpers_emitted=false; current_function_scope=nullptr;
  rtti_source_types.clear();rtti_lowered_types.clear(); rtti_user_prefix_types.clear(); typeinfo_helpers_emitted=false;
  lifecycle_emitted=false;local_static_storage.clear();startup_routines.clear();
  libc_enabled=getenv("EDG_DOT_LIBC") && string(getenv("EDG_DOT_LIBC"))=="dotcc";
  reset_libc_imports();
  reset_thread_imports();
  reset_materialization();
  exception_ptr_helpers_emitted=false;
  runtime_exceptions=getenv("EDG_DOT_RUNTIME") && string(getenv("EDG_DOT_RUNTIME"))=="edg-exceptions";
  if (targ_sizeof_pointer != 8) unsupported("non-64-bit target");
  instance_mode = getenv("EDG_DOT_INSTANCE") && string(getenv("EDG_DOT_INSTANCE")) == "1";
  const char *ns = getenv("EDG_DOT_NAMESPACE");
  const char *cls = getenv("EDG_DOT_CLASS");
  if (!ns || !cls) unsupported("invocation: use dotnet.py driver");
  auto scope = il_header.primary_scope;
  // Exception metadata visits aggregate types before ordinary emission.
  nested_types = !getenv("EDG_DOT_NEST") || string(getenv("EDG_DOT_NEST")) == "1";
  if (getenv("EDG_DOT_TRACE")) {trace_program(scope);trace_coroutines(scope);}
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r) || r->function_def_number==NULL_function_def_number)continue;
    auto body=scope_for_routine_or_null(r);if(body)collect_source_jump_statements(body->assoc_block,false);
  }
  collect_virtual_metadata(scope);
  collect_primary_exception_types();
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r))continue;
    collect_dynamic_specification(r);
    if(r->function_def_number==NULL_function_def_number)continue;
    auto s=scope_for_routine_or_null(r);
    if(s) { collect_exception_metadata(s->assoc_block); collect_constant_templates(s); }
  }
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r))continue;
    if(r->function_def_number==NULL_function_def_number)continue;
    auto s=scope_for_routine_or_null(r);if(s){prepare_cleanup(s);collect_local_static_storage(s);}
    if(r->is_initialization_routine || (r->source_corresp.name && string(r->source_corresp.name).find(IL_LOWERING_INIT_ROUTINE_PREFIX)==0))
      startup_routines.push_back(r);
  }
  std::stable_sort(startup_routines.begin(),startup_routines.end(),[](a_routine_ptr a,a_routine_ptr b){return (a->has_ctor_priority?gnu_routine_supp(a)->ctor_priority:a->init_priority?a->init_priority:65535)<(b->has_ctor_priority?gnu_routine_supp(b)->ctor_priority:b->init_priority?b->init_priority:65535);});
  // lower_file_scope_dynamic_inits already produced __sti routines. GCC
  // priority processing can leave the source list attached; the C backend
  // consumes the generated routines rather than executing that list again.
  if(!startup_routines.empty() || !local_static_storage.empty())ensure_lifecycle();
  // Structural lowering moves namespace entities to the primary scope.
  // Namespace shells (including the predefined std namespace) remain.
  if (scope->asm_entries) unsupported("assembly declaration");
  line("// Generated by dot_gen_be.c; experimental runtime ABI v3; EDG region-table cleanup.");
  library_mode = getenv("EDG_DOT_EMIT") && string(getenv("EDG_DOT_EMIT")) == "managedlib";
  if (*ns) line("namespace " + string(ns) + ";");
  line("public " + string(instance_mode ? "sealed" : "static") + " unsafe partial class " + cls + string(instance_mode?" : global::System.IDisposable":"")+" {");
  if(instance_mode)ensure_lifecycle();
  globals(scope->variables, cls);
  a_routine_ptr main_routine = nullptr;
  for (auto r = scope->routines; r; r = r->next) {
    if(!routine_body_needed(r))continue;
    if(r->is_finalization_routine)unsupported("non-atexit finalization routine");
    if (r->function_def_number == NULL_function_def_number) continue;
    auto s = scope_for_routine_or_null(r);
    if (!s) continue;
    context = r->source_corresp.name ? r->source_corresp.name : "generated function";
    diagnostic_scope diagnostic(r->source_corresp.decl_position);
    current_function_scope=s;
    function_vla_used=false;
    try_frames.clear(); exception_locals.clear();
    prepare_cleanup(s);
    prepare_source_jumps(s->assoc_block);
    check_operand_order(s->assoc_block);
    if (r->source_corresp.name && string(r->source_corresp.name) == "main") { main_routine = r; if(main_arguments(r))ensure_lifecycle(); }
    auto rt = skip_typerefs(r->type);
    function_returns_void=skip_typerefs(rt->variant.routine.return_type)->kind==tk_void;
    string signature = "private " + string(instance_mode ? "" : "static ") +
                       type(rt->variant.routine.return_type) + " " + name(r, "f") + "(";
    local_offsets.clear(); frame_size = 0; frame_alignment = 8;
    plan_frame(s);
    prepare_materialization(s->assoc_block);
    auto params = s->variant.routine.parameters;
    for (auto p = params; p; p = p->next) {
      unsigned long long align=std::max<a_targ_alignment>(alignment_of_type(p->type),p->alignment);
      if(align>8 && !local_offsets.count(p)) {
        if(align>4096 || (align&(align-1)))unsupported("parameter alignment");
        frame_alignment=std::max(frame_alignment,align);frame_size=(frame_size+align-1)&~(align-1);
        local_offsets[p]=frame_size;frame_size+=skip_typerefs(p->type)->size;
        if(frame_size+frame_alignment>64*1024*1024)unsupported("local frame exceeds 64 MiB");
      }
      if (p != params) signature += ", ";
      signature += type(p->type) + " " + name(p, "v");
    }
    if(rt->variant.routine.extra_info->has_ellipsis)
      signature+=string(params?", ":"")+"global::Edg.Runtime.VaList* __edg_arguments";
    line("// dot-gen:member-begin");
    line(signature + ") {");
    auto spec = rt->variant.routine.extra_info->exception_specification;
    bool no_throw = spec && !spec->throw_any && spec->is_noexcept;
    bool dynamic_spec = dynamic_exception_spec(r);
    if (spec && spec->indeterminate) unsupported("indeterminate exception specification");
    if (no_throw) {ensure_terminate_helpers();line("try {");}
    else if(dynamic_spec) {ensure_unexpected_helpers();line("try {");}
    bool heap_frame = frame_size + frame_alignment > 65536;
    if (frame_size) {
      if (heap_frame) {
        line("byte* __edg_frame_raw = (byte*)global::System.Runtime.InteropServices.NativeMemory.Alloc(" + std::to_string(frame_size + frame_alignment - 1) + ");");
        line("if (__edg_frame_raw == null) throw new global::System.OutOfMemoryException();");
        line("try {");
      } else line("byte* __edg_frame_raw = stackalloc byte[" + std::to_string(frame_size + frame_alignment - 1) + "];");
      line("byte* __edg_frame = (byte*)(((nuint)__edg_frame_raw + " + std::to_string(frame_alignment - 1) +
           ") & ~(nuint)" + std::to_string(frame_alignment - 1) + ");");
    }
    for(auto p=params;p;p=p->next)if(local_offsets.count(p))line(variable(p)+" = "+name(p,"v")+";");
    locals(s);
    size_t body_start = output.size();
    output+=source_jump_dispatch_prefix();
    statement(s->assoc_block);
    output+=source_jump_dispatch_suffix();
    if (cleanup_cursor_used) {
      emit_cleanup_helper();
      output.insert(body_start, exception_locals+"int __edg_cleanup_state = 0;\nbool __edg_unwinding = false;\ntry {\n");
      line("} catch (global::System.Exception __edg_unwind_error) when (__edg_begin_unwind(__edg_unwind_error, ref __edg_unwinding)) { throw; } finally {");
      line("if (__edg_unwinding) { "+cleanup_unwind("0")+" }");
      line("}");
    }
    if(function_vla_used) {
      output.insert(body_start,"nint __edg_vla_mark = global::Edg.Runtime.Vla.Mark();\ntry {\n");
      line("} finally { global::Edg.Runtime.Vla.Restore(__edg_vla_mark); }");
    }
    if (heap_frame) line("} finally { global::System.Runtime.InteropServices.NativeMemory.Free(__edg_frame_raw); }");
    if (no_throw) line("} catch (global::System.Exception error) when ("+source_jump_exclusion("error")+") { __edg_terminate_runtime(error, \"Exception escaped noexcept function\"); throw; }");
    else if(dynamic_spec)line(dynamic_exception_boundary(r));
    line("}");
    line("// dot-gen:member-end");
    if (library_mode) {
      string exported = "__edg_export_" + name(r, "f"), args;
      for (auto p = params; p; p = p->next) {
        if (p != params) args += ", ";
        args += name(p, "v");
      }
      if(rt->variant.routine.extra_info->has_ellipsis)args+=string(args.empty()?"":", ")+"__edg_arguments";
      // The driver consumes these protocol records into the symbol manifest.
      string source = r->source_corresp.name ? r->source_corresp.name : "";
      string hex;
      const char *digits = "0123456789abcdef";
      for (auto c : source) { hex += digits[(unsigned char)c >> 4]; hex += digits[(unsigned char)c & 15]; }
      line("// dot-gen:export " + hex + " " + exported);
      string api = signature;
      api.replace(0, 8, "public ");
      auto pos = api.find(" " + name(r, "f") + "(");
      api.replace(pos + 1, name(r, "f").size(), exported);
      line("// dot-gen:member-begin");
      line(api + ") { /*__EDG_ENTER__*/ try { " + (type(rt->variant.routine.return_type) == "void" ? "" : "return ") +
           name(r, "f") + "(" + args + "); } /*__EDG_EXCEPTION_BOUNDARY__*/ finally { /*__EDG_LEAVE__*/ global::System.GC.KeepAlive(" +
           string(instance_mode ? "this" : "__edg_storage") + "); } }");
      line("// dot-gen:member-end");
    }
  }
  output += (nested_types ? type_output : "") + helper_output + address_output;
  for(const auto &item:std::vector<std::pair<string,string>>{
      {"SHUTDOWN","Destructor threw during shutdown"},{"GLOBAL","Exception during global initialization"},{"THREAD","Exception escaped translated thread"}}) {
    string marker="/*__EDG_"+item.first+"_TERMINATE__*/";
    string prefix=item.first=="THREAD" && instance_mode?"owner.":"";
    string code=terminate_helpers_emitted?prefix+"__edg_terminate_runtime(error,"+string("\"")+item.second+"\");":
        "global::System.Environment.FailFast(\""+item.second+"\",error);";
    for(size_t pos=output.find(marker);pos!=string::npos;pos=output.find(marker,pos+code.size()))output.replace(pos,marker.size(),code);
  }
  const string exception_boundary="/*__EDG_EXCEPTION_BOUNDARY__*/";
  const string exception_boundary_code=exception_helpers_emitted?
      "catch (global::System.Exception __edg_boundary_error) { __edg_end_exception(__edg_boundary_error); throw; }":"";
  for(size_t pos=output.find(exception_boundary);pos!=string::npos;pos=output.find(exception_boundary,pos+exception_boundary_code.size()))
    output.replace(pos,exception_boundary.size(),exception_boundary_code);
  for(auto replacement:std::vector<std::pair<string,string>>{{"/*__EDG_LIBC_THREAD_KEYS_RELEASE__*/",thread_keys_release_code()},{"/*__EDG_LIBC_THREAD_RELEASE__*/",libc_thread_release_code()},{"/*__EDG_LIBC_DISPOSE__*/",thread_disposal_code()+libc_disposal_code()}})
    for(size_t pos=output.find(replacement.first);pos!=string::npos;pos=output.find(replacement.first,pos+replacement.second.size()))output.replace(pos,replacement.first.size(),replacement.second);
  for(auto replacement:std::vector<std::pair<string,string>>{{"/*__EDG_ENTER__*/",lifecycle_emitted?"__edg_enter();":""},{"/*__EDG_LEAVE__*/",lifecycle_emitted?"__edg_leave();":""}})
    for(size_t pos=output.find(replacement.first);pos!=string::npos;pos=output.find(replacement.first,pos+replacement.second.size()))output.replace(pos,replacement.first.size(),replacement.second);
  if (!literal_bytes.empty()) {
    line("private static readonly byte[] __edg_literals = __edg_make_literals();");
    line("private static byte* __edg_literal_base => (byte*)(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref __edg_literals[0]) + 7) & ~(nuint)7);");
    line("// dot-gen:member-begin");
    line("private static byte[] __edg_make_literals() {");
    line("byte[] pool = global::System.GC.AllocateArray<byte>(" + std::to_string(literal_bytes.size() + 7) + ", pinned: true);");
    line("byte* p = (byte*)(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref pool[0]) + 7) & ~(nuint)7);");
    string data = "global::System.ReadOnlySpan<byte> data = [";
    for (auto byte : literal_bytes) data += std::to_string((unsigned char)byte) + ",";
    line(data + "]; data.CopyTo(new global::System.Span<byte>(p, data.Length)); return pool; }");
    line("// dot-gen:member-end");
  }
  if (!main_routine && !library_mode) unsupported("program without main()");
  if (main_routine)emit_main(main_routine);
  line("}");
  if (!nested_types) output += type_output;
  if(owned_ir_enabled) {
    auto problem=owned_ir.verify();if(!problem.empty())unsupported(("owned physical type graph: "+problem).c_str());
    line("// dot-gen:type-ir "+dot_ir::serialize(owned_ir,getenv("EDG_DOT_TARGET")?getenv("EDG_DOT_TARGET"):"legacy-linux-x64"));
  }
  // Commit output only after the complete IL passed the subset checks.
  if (fwrite(output.data(), 1, output.size(), stdout) != output.size() || fflush(stdout)) {
    perror("dot_gen_be output"); exit(1);
  }
}
