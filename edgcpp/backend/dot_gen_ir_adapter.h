// EDG pointers exist only in this ingestion adapter, never in dot_ir::Graph.
#include "dot_gen_ir.h"
dot_ir::Graph owned_ir;
std::map<std::pair<a_type_ptr,unsigned>,dot_ir::TypeId> owned_type_ids;
void reset_owned_ir() {owned_ir.clear();owned_type_ids.clear();}
dot_ir::TypeId own_type(a_type_ptr source) {
  unsigned q=get_type_qualifiers(source), qualifiers=0;
  if(q&TQ_CONST)qualifiers|=1;
  if(q&TQ_VOLATILE)qualifiers|=2;
  if(q&TQ_RESTRICT)qualifiers|=4;
  if(q&TQ_C11_ATOMIC)qualifiers|=8;
  auto t=skip_typerefs(source);auto key=std::make_pair(t,qualifiers);
  auto found=owned_type_ids.find(key);if(found!=owned_type_ids.end())return found->second;
  auto id=owned_ir.reserve();owned_type_ids[key]=id;
  dot_ir::Type result;result.size=t->size;result.qualifiers=qualifiers;
  result.name=t->source_corresp.name?t->source_corresp.name:"";
  result.complete=t->size!=0;
  if(t->kind!=tk_void && t->kind!=tk_routine && t->size)result.alignment=alignment_of_type(t);
  switch(t->kind) {
    case tk_void:result.kind="void";break;
    case tk_integer:result.kind=int_type_is_signed(t)?"signed-integer":"unsigned-integer";break;
    case tk_float:
      result.kind=(t->variant.float_kind==fk_float128 || t->variant.float_kind==fk_std_float128)?"binary128":
          (t->variant.float_kind==fk_float16 || t->variant.float_kind==fk_fp16 || t->variant.float_kind==fk_std_float16)?"binary16":
          t->variant.float_kind==fk_float?"binary32":t->variant.float_kind==fk_double?"binary64":
          (t->variant.float_kind==fk_long_double || t->variant.float_kind==fk_float80)?"binary80":"extended-float";
      break;
    case tk_pointer:result.kind="pointer";result.element=own_type(t->variant.pointer.type);break;
    case tk_array:
      result.kind="array";result.element=own_type(t->variant.array.element_type);
      if(t->variant.array.is_variable_size_array || t->variant.array.is_vla || t->variant.array.is_template_dependent_size_array)
        result.complete=false;
      else result.count=t->variant.array.variant.number_of_elements;
      break;
    case tk_struct:case tk_class:case tk_union:
      result.kind=t->kind==tk_union?"union":"record";
      for(auto f=t->variant.class_struct_union.field_list;f;f=f->next) {
        if(f->is_bit_field && !f->bit_size)continue;
        dot_ir::Field field;field.name=f->source_corresp.name?f->source_corresp.name:"";
        field.type=own_type(f->type);field.offset=f->offset;field.bitfield=f->is_bit_field;
        if(field.bitfield){field.bit_offset=f->offset_bit_remainder;field.bit_width=f->bit_size;}
        result.fields.push_back(field);
      }
      break;
    case tk_routine:
      result.kind="function";result.result=own_type(t->variant.routine.return_type);
      result.variadic=t->variant.routine.extra_info->has_ellipsis;
      for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)result.parameters.push_back(own_type(p->type));
      break;
    default:result.kind="unemitted-kind-"+std::to_string((unsigned)t->kind);break;
  }
  owned_ir.types[id]=std::move(result);return id;
}
