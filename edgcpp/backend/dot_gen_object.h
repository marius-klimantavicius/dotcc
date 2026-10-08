// Owned semantic-object adapter. Included inside dot_gen_be's namespace.
// This initial profile deliberately rejects forms whose execution or linkage
// contracts are not represented. No EDG pointer is written to the protocol.
namespace dot_object {
string quote(const string &value) {
  string result="\"";
  const char *hex="0123456789abcdef";
  for(char character:value) {
    unsigned char c=static_cast<unsigned char>(character);
    if(c=='"' || c=='\\'){result+='\\';result+=char(c);}
    else if(c<32){result+="\\u00";result+=hex[c>>4];result+=hex[c&15];}
    else result+=char(c);
  }
  return result+'"';
}
string array(const std::vector<string> &values) {
  string result="[";bool first=true;
  for(const auto &value:values){if(!first)result+=',';first=false;result+=value;}
  return result+"]";
}
[[noreturn]] void reject(const char *what,int kind=-1) {
  unsupported((string("dotobj scalar-v16: ")+what).c_str(),kind);
}
void check_lifetime(an_object_lifetime_ptr lifetime) {
  if(!lifetime)return;
  if(lifetime->destructions)reject("destruction obligations");
  for(auto child=lifetime->child_lifetime;child;child=child->next)check_lifetime(child);
}
}
namespace dot_object {
struct Writer {
  unsigned next_entity=1,next_binding=1,next_node=1,next_file=1,next_source_type=1,next_source_declaration=1;
  unsigned function=0,next_execution_type=12,va_list_id=0,f80_id=0,i128_id=0,u128_id=0;
  a_statement_ptr function_block=nullptr;
  std::map<a_routine_ptr,unsigned> routine_ids;
  std::map<a_variable_ptr,unsigned> binding_ids;
  std::map<a_variable_ptr,unsigned> binding_scopes;
  std::set<a_variable_ptr> cleanup_variables,registered_cleanups;
  std::map<a_source_file_ptr,unsigned> file_ids;
  std::vector<a_routine_ptr> routines;
  std::vector<string> files,entities,parameters,functions,bindings,nodes,edges;
  std::vector<string> source_types,source_declarations,source_functions,source_parameters;
  std::vector<string> macro_definitions,macro_parameters,macro_events;
  // Action order is independent of executable reachability/entity numbering.
  std::vector<string> initialization_actions;
  std::map<unsigned,unsigned> source_type_ids;
  std::map<unsigned,unsigned> pointer_types;
  std::map<std::pair<unsigned,unsigned long long>,unsigned> array_types;
  std::vector<string> execution_types,record_types,record_fields;
  std::vector<string> source_records,source_fields;
  std::map<a_type_ptr,unsigned> source_record_ids;
  std::map<a_field_ptr,unsigned> source_field_ids;
  unsigned next_source_record=1,next_source_field=1;
  std::map<a_type_ptr,unsigned> record_ids;
  std::map<a_field_ptr,unsigned> field_ids;
  unsigned next_field=1,next_storage=1;
  std::map<a_variable_ptr,unsigned> storage_ids;
  std::map<a_constant_ptr,unsigned> literal_ids;
  std::map<a_variable_ptr,a_constant_ptr> static_initializers;
  std::vector<string> storages,storage_data,relocations;
  std::map<unsigned,std::map<unsigned long long,string>> storage_chunks;
  std::map<unsigned,std::map<unsigned long long,unsigned char>> bitfield_data;
  std::vector<string> routine_types,routine_parameters,source_routine_types,source_routine_parameters;
  std::map<string,unsigned> signature_ids;
  unsigned signature_type(unsigned result,const std::vector<unsigned> &args,bool variadic,const string &exception_policy) {
    if(exception_policy!="unwind" && exception_policy!="terminate")reject("dynamic typed exception specification");
    string key=number(result)+(variadic?"v:":"f:")+exception_policy+":";for(auto arg:args)key+=number(arg)+",";
    auto prior=signature_ids.find(key);if(prior!=signature_ids.end())return prior->second;
    unsigned id=next_execution_type++;signature_ids[key]=id;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":\"routine\",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    routine_types.push_back("{\"typeId\":"+number(id)+",\"returnTypeId\":"+number(result)+",\"variadic\":"+(variadic?"true":"false")+",\"exceptionPolicy\":"+quote(exception_policy)+"}");
    unsigned ordinal=0;for(auto arg:args)routine_parameters.push_back("{\"typeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+",\"parameterTypeId\":"+number(arg)+"}");
    return id;
  }
  unsigned va_list_type() {
    if(va_list_id)return va_list_id;
    va_list_id=next_execution_type++;
    execution_types.push_back("{\"id\":"+number(va_list_id)+",\"kind\":\"va-list\",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    return va_list_id;
  }
  unsigned f80_type() {
    if(targ_sizeof_long_double!=16 || targ_alignof_long_double!=16 ||
       targ_ldbl_mant_dig!=64 || targ_ldbl_min_exp!=-16381 || targ_ldbl_max_exp!=16384)
      reject("long double target outside 16-byte x87 binary80 ABI");
    if(f80_id)return f80_id;
    f80_id=next_execution_type++;
    execution_types.push_back("{\"id\":"+number(f80_id)+",\"kind\":\"f80\",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    return f80_id;
  }
  unsigned integer128_type(bool is_unsigned) {
    if(targ_sizeof_int128!=16 || targ_alignof_int128!=16)
      reject("integer128 target outside 16-byte aligned LP64 ABI");
    unsigned &id=is_unsigned?u128_id:i128_id;
    if(id)return id;
    id=next_execution_type++;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(is_unsigned?"u128":"i128")+",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    return id;
  }
  bool integer128(a_type_ptr type) {
    auto t=skip_typerefs(type);
    return t->kind==tk_integer && t->size==16;
  }
  string floating_bits(a_constant_ptr constant) {
    auto type=skip_typerefs(constant->type);unsigned execution=type_id(type);
    string bits=fp_to_hex_string(type->variant.float_kind,&constant->variant.float_value).as_temp_characters();
    if(execution==f80_id && f80_id) {
      // Match the legacy backend: zero storage padding is not value payload.
      if(bits.size()<20 || (bits.size()>20 && bits.substr(0,bits.size()-20).find_first_not_of('0')!=string::npos))
        reject("binary80 constant encoding");
      bits=bits.substr(bits.size()-20);
    }
    size_t digits=execution==10?8:execution==11?16:execution==f80_id && f80_id?20:0;
    if(!digits || bits.size()!=digits || bits.find_first_not_of("0123456789abcdef")!=string::npos)
      reject("floating constant encoding");
    return bits;
  }
  unsigned routine_type(a_type_ptr t,bool known_empty_definition=false) {
    auto extra=t->variant.routine.extra_info;
    if((!extra->prototyped && !known_empty_definition) || extra->explicit_calling_convention)
      reject("unprototyped/foreign-convention function type");
    if(known_empty_definition && (extra->param_type_list || extra->has_ellipsis))reject("nonempty old-style execution signature");
    std::vector<unsigned> args;for(auto p=extra->param_type_list;p;p=p->next)args.push_back(type_id(p->type));
    auto specification=extra->exception_specification;
    string policy=specification && !specification->throw_any?(specification->is_noexcept?"terminate":"unsupported-dynamic"):"unwind";
    return signature_type(type_id(t->variant.routine.return_type),args,extra->has_ellipsis,policy);
  }
  unsigned record_type(a_type_ptr t) {
    auto prior=record_ids.find(t);if(prior!=record_ids.end())return prior->second;
    auto &captured=dot_object_source::state();
    bool abi=captured.abi_records.count(t)!=0;
    auto found=captured.records.find(t);
    if(!abi && found==captured.records.end())reject("incomplete or uncaptured record definition");
    auto pre=captured.prelower_records.find(t);
    const dot_object_source::Record *source=abi?nullptr:&found->second;
    if(source && !source->error.empty())reject((source->identity+": "+source->error).c_str());
    if(source && source->has_bases)reject("base/subobject execution layout is not represented");
    if(source && source->has_virtual_functions && pre==captured.prelower_records.end())
      reject("virtual record lacks an observed prelowering boundary");
    if(pre!=captured.prelower_records.end() && !pre->second.error.empty())reject(pre->second.error.c_str());
    if(source && pre!=captured.prelower_records.end()) {
      if(source->fields.size()!=pre->second.fields.size())reject("source field snapshot disagrees with prelowering boundary");
      for(size_t i=0;i<source->fields.size();++i) {
        const auto &field=source->fields[i];const auto &observed=pre->second.fields[i];
        if(field.il!=observed.il || field.original_name!=observed.name || field.source_type!=observed.source_type_snapshot ||
           field.offset!=observed.byte_offset || field.is_bit_field!=observed.is_bit_field || field.bit_width!=observed.bit_width)
          reject("original field identity/type differs at prelowering boundary");
      }
    }
    if(source && ((!t->incomplete && t->size!=source->size) || (!t->incomplete && t->alignment!=source->alignment)))
      reject("record complete-object extent changed during lowering");
    if(!t->incomplete && (!t->size || t->size>2147483647ULL || !t->alignment || t->alignment>16))
      reject("record native extent/alignment outside execution profile");
    string identity,name,layout=t->kind==tk_union?"union":t->kind==tk_class?"class":"struct";
    a_source_position pos={};
    if(abi) {
      auto key=captured.abi_record_keys.find(t);
      if(key==captured.abi_record_keys.end())reject("unclassified compiler ABI record creation");
      identity=key->second;name=source_spelling(&t->source_corresp);
    } else {
      identity=source->identity;name=source->name;layout=source->layout_kind;pos=source->position;
      if(source->template_identity)identity="record-abi:"+string(mangled_class_name(t));
      if(identity.find("local-record:")==0) {
        a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
        conv_seq_to_file_and_line(source->position.seq,&display,&full,&line,&at_end);
        identity+=":"+string(full?full:display?display:"")+":"+std::to_string((unsigned long long)line)+":"+
          std::to_string(source->position.column)+":occurrence:"+std::to_string(source->source_occurrence);
      }
    }
    unsigned id=next_execution_type++;record_ids[t]=id;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":\"record\",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    bool trivial=!abi && !t->incomplete && C_mode();
    if(!abi && !t->incomplete && !C_mode()) {
      if(pre==captured.prelower_records.end() || !pre->second.trivial_copy_known)
        reject("complete C++ source record lacks original copyability fact");
      trivial=pre->second.trivial_copy;
    }
    unsigned source_record=0;
    std::map<a_field_ptr,const dot_object_source::RecordField*> originals;
    if(source) {
      source_record=next_source_record++;source_record_ids[t]=source_record;
      auto source_type_it=captured.type_ids.find(t);
      if(source_type_it==captured.type_ids.end())reject("record source type was not captured before lowering");
      source_records.push_back("{\"id\":"+number(source_record)+",\"sourceTypeId\":"+number(source_type(source_type_it->second))+
        ",\"identityKey\":"+quote(identity)+",\"sourceName\":"+quote(name)+",\"recordKind\":"+quote(layout)+
        ",\"isComplete\":"+(t->incomplete?"false":"true")+",\"nativeSize\":"+std::to_string(source->size)+
        ",\"nativeAlignment\":"+std::to_string(source->alignment)+",\"isTriviallyCopyable\":"+
        (t->incomplete?"null":trivial?"true":"false")+",\"capturePhase\":"+
        quote(pre==captured.prelower_records.end()?"before-lowering":"before-class-prelowering")+position(pos)+"}");
      unsigned source_ordinal=0;
      for(const auto &field:source->fields) {
        unsigned original_id=next_source_field++;source_field_ids[field.il]=original_id;originals[field.il]=&field;
        source_fields.push_back("{\"id\":"+number(original_id)+",\"sourceRecordId\":"+number(source_record)+
          ",\"ordinal\":"+std::to_string(source_ordinal++)+",\"sourceTypeId\":"+number(source_type(field.source_type))+
          ",\"sourceName\":"+quote(field.original_name)+
          ",\"isMutable\":"+(field.is_mutable?"true":"false")+",\"hasNoUniqueAddress\":"+
          (field.no_unique_address?"true":"false")+",\"bitWidth\":"+
          (field.is_bit_field?std::to_string(field.bit_width):"null")+position(field.position)+"}");
      }
    }
    record_types.push_back("{\"typeId\":"+number(id)+",\"identityKey\":"+quote(identity)+",\"sourceName\":"+quote(name)+
      ",\"size\":"+std::to_string(t->incomplete?0:t->size)+",\"alignment\":"+std::to_string(t->incomplete?0:t->alignment)+
      position(pos)+",\"layoutKind\":"+quote(layout)+",\"sourceRecordId\":"+number(source_record)+
      ",\"representationKind\":"+quote(abi?"abi":"complete")+",\"valueCopyKind\":"+quote(trivial?"ordinary":"explicit-only")+
      ",\"captureProof\":"+quote(abi?"observed-abi-creation":pre==captured.prelower_records.end()?"source-snapshot":"observed-class-lowering")+"}");
    unsigned ordinal=0;std::set<string> names;
    for(auto f=t->variant.class_struct_union.field_list;f;f=f->next) {
      if(f->is_bit_field && !f->bit_size)continue;
      auto child=skip_typerefs(f->type);
      if(f->is_bit_field && integer128(child))reject("integer128 bitfield representation");
      auto original=originals.find(f);
      string role,field_name;unsigned source_type_id=0,source_field=0;a_source_position field_pos={};
      if(original!=originals.end()) {
        const auto &field=*original->second;role="source";field_name=field.name;field_pos=field.position;
        source_type_id=source_type(field.source_type);source_field=source_field_ids.at(f);
        if(f->offset!=field.offset)reject("original field offset changed during lowering");
      } else if(abi) {role="abi";field_name=source_spelling(&f->source_corresp);}
      else {
        auto generated=captured.generated_field_roles.find(f);
        if(generated==captured.generated_field_roles.end() || generated->second!="vptr")reject("unclassified lowered physical field");
        role="vptr";field_name="__edg_vptr";
      }
      if(field_name.empty())field_name="__edg_abi_field_"+std::to_string(ordinal);
      bool flexible=child->kind==tk_array && !child->size && !child->variant.array.variant.number_of_elements;
      if(flexible && (f->next || layout=="union"))reject("flexible array outside final struct/class member");
      if(!names.insert(field_name).second || (!child->size && !flexible) || !child->alignment)
        reject("record physical field identity/extent");
      unsigned long long extent=f->is_bit_field?(f->offset_bit_remainder+f->bit_size+7)/8:child->size;
      if(f->offset>t->size || child->alignment>16 || extent>t->size-f->offset)reject("record field outside containing storage");
      if(layout=="union" && f->offset)reject("union field nonzero storage offset");
      unsigned field_id=next_field++;field_ids[f]=field_id;
      record_fields.push_back("{\"id\":"+number(field_id)+",\"recordTypeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"typeId\":"+number(type_id(f->type))+",\"sourceName\":"+quote(field_name)+",\"byteOffset\":"+std::to_string(f->offset)+
        ",\"sourceTypeId\":"+number(source_type_id)+position(field_pos)+
        ",\"bitOffset\":"+(f->is_bit_field?std::to_string(f->offset_bit_remainder):"null")+
        ",\"bitWidth\":"+(f->is_bit_field?std::to_string(f->bit_size):"null")+
        ",\"bitSigned\":"+(f->is_bit_field?(f->bit_field_is_signed?"true":"false"):"null")+
        ",\"sourceFieldId\":"+number(source_field)+",\"fieldRole\":"+quote(role)+"}");
    }
    return id;
  }
  string bytes_hex(const string &bytes) {
    const char *hex="0123456789abcdef";string result;result.reserve(bytes.size()*2);
    for(unsigned char c:bytes){result+=hex[c>>4];result+=hex[c&15];}return result;
  }
  void data_chunk(unsigned id,unsigned long long offset,const string &bytes) {
    if(bytes.empty())return;
    auto &chunks=storage_chunks[id];
    auto following=chunks.lower_bound(offset);
    if(following!=chunks.end() && bytes.size()>following->first-offset)reject("overlapping static data initializers");
    if(following!=chunks.begin()) {
      auto previous=following;--previous;
      if(previous->second.size()>offset-previous->first)reject("overlapping static data initializers");
      if(previous->second.size()==offset-previous->first) {
        previous->second+=bytes;
        if(following!=chunks.end() && previous->first+previous->second.size()==following->first) {
          previous->second+=following->second;chunks.erase(following);
        }
        return;
      }
    }
    string combined=bytes;
    if(following!=chunks.end() && offset+bytes.size()==following->first) {
      combined+=following->second;chunks.erase(following);
    }
    chunks.emplace(offset,std::move(combined));
  }
  unsigned literal(a_constant_ptr c) {
    auto found=literal_ids.find(c);if(found!=literal_ids.end())return found->second;
    if(c->kind!=ck_string)reject("literal storage outside string constants",c->kind);
    auto t=skip_typerefs(c->type);
    if(t->kind!=tk_array || !t->size || c->variant.string.length>t->size)reject("string literal storage bounds");
    unsigned id=next_storage++;literal_ids[c]=id;
    a_source_position pos={};
    storages.push_back("{\"id\":"+number(id)+",\"sourceName\":\"\",\"linkage\":\"internal\",\"externalKey\":null,\"definition\":\"strong\",\"typeId\":"+number(type_id(t))+
      ",\"sourceTypeId\":null,\"isThreadLocal\":false,\"isConst\":true,\"alignment\":"+std::to_string(t->alignment)+position(pos)+"}");
    data_chunk(id,0,string(c->variant.string.value,c->variant.string.length));return id;
  }
  bool zero_constant_bits(a_constant_ptr value) {
    if(!value)return true;
    if(value->kind==ck_integer)return is_zero_constant(value);
    if(value->kind==ck_float)return floating_bits(value).find_first_not_of('0')==string::npos;
    if(value->kind==ck_string)return string(value->variant.string.value,value->variant.string.length).find_first_not_of('\0')==string::npos;
    if(value->kind==ck_init_repeat)return zero_constant_bits(value->variant.init_repeat.constant);
    if(value->kind!=ck_aggregate || value->variant.aggregate.has_dynamic_init_component)return false;
    for(auto item=value->variant.aggregate.first_constant;item;item=item->next)
      if(item->kind!=ck_designator && !zero_constant_bits(item))return false;
    return true;
  }
  unsigned aggregate_literal(a_constant_ptr value,const a_source_position &pos) {
    auto found=literal_ids.find(value);if(found!=literal_ids.end())return found->second;
    auto type=skip_typerefs(value->type);unsigned id=next_storage++;literal_ids[value]=id;
    storages.push_back("{\"id\":"+number(id)+",\"sourceName\":\"\",\"linkage\":\"internal\",\"externalKey\":null,\"definition\":\"strong\",\"typeId\":"+number(type_id(type))+
      ",\"sourceTypeId\":null,\"isThreadLocal\":false,\"isConst\":true,\"alignment\":"+std::to_string(type->alignment)+position(pos)+"}");
    initialize_storage(id,type,value,0);return id;
  }
  void initialize_storage(unsigned id,a_type_ptr type,a_constant_ptr c,unsigned long long offset) {
    auto t=skip_typerefs(type);if(!c)return;
    if(c->kind==ck_string) {
      if(t->kind!=tk_array || c->variant.string.length>t->size)reject("static string initializer bounds");
      data_chunk(id,offset,string(c->variant.string.value,c->variant.string.length));return;
    }
    if(c->kind==ck_address) {
      if((t->kind!=tk_pointer && t->kind!=tk_integer) || t->size!=8)reject((string("address relocation destination kind=")+std::to_string(t->kind)+" size="+std::to_string(t->size)).c_str());
      unsigned target_storage=0,target_entity=0;
      auto &address=c->variant.address;
      if(address.kind==abk_variable)target_storage=storage(address.variant.variable);
      else if(address.kind==abk_constant)target_storage=literal(address.variant.constant);
      else if(address.kind==abk_routine) {
        if(address.offset)reject("function address relocation addend");
        target_entity=routine(address.variant.routine);
      } else reject("unsupported static address relocation",address.kind);
      relocations.push_back("{\"storageId\":"+number(id)+",\"byteOffset\":"+std::to_string(offset)+
        ",\"targetStorageId\":"+number(target_storage)+",\"targetEntityId\":"+number(target_entity)+",\"addend\":"+std::to_string(address.offset)+"}");return;
    }
    if(c->kind==ck_integer) {
      if(integer128(t)) {
        // Follow the stock integer-value helpers and legacy backend: extract
        // exact two's-complement halves without narrowing through host ints.
        auto high=c->variant.integer_value;a_boolean ignored_overflow=FALSE;
        auto low=unsigned_value_of_integer_value(&high,FALSE,&ignored_overflow);
        shift_right_integer_value(&high,64,FALSE,FALSE);
        auto upper=unsigned_value_of_integer_value(&high,FALSE,&ignored_overflow);
        string bytes;
        for(unsigned i=0;i<8;++i)bytes+=char(((unsigned long long)low>>(8*i))&255);
        for(unsigned i=0;i<8;++i)bytes+=char(((unsigned long long)upper>>(8*i))&255);
        data_chunk(id,offset,bytes);return;
      }
      if(t->size>8 || (t->kind!=tk_integer && t->kind!=tk_pointer))reject("static integer encoding type");
      a_boolean overflow=FALSE;
      unsigned long long bits=int_constant_is_signed(c)?(unsigned long long)value_of_integer_constant(c,&overflow):
        (unsigned long long)unsigned_value_of_integer_constant(c,&overflow);
      if(overflow)reject("static integer out of range");
      string bytes;for(unsigned i=0;i<t->size;++i)bytes+=char((bits>>(8*i))&255);
      data_chunk(id,offset,bytes);return;
    }
    if(c->kind==ck_float) {
      auto ct=skip_typerefs(c->type);if(t->kind!=tk_float || type_id(ct)!=type_id(t))reject("static floating encoding type");
      string hex=floating_bits(c);
      string bytes;auto digit=[](char digit_char){return digit_char<='9'?digit_char-'0':digit_char-'a'+10;};
      for(size_t i=hex.size();i;i-=2)bytes+=char((digit(hex[i-2])<<4)|digit(hex[i-1]));
      bytes.resize((size_t)t->size,'\0');data_chunk(id,offset,bytes);return;
    }
    if(c->kind!=ck_aggregate || (t->kind!=tk_array && t->kind!=tk_struct && t->kind!=tk_union))reject("static initializer constant kind",c->kind);
    auto field=t->kind==tk_array?nullptr:next_non_empty_initializable_field(t->variant.class_struct_union.field_list);
    unsigned long long index=0;
    for(auto item=c->variant.aggregate.first_constant;item;item=item->next) {
      if(item->kind==ck_designator) {
        if(item->variant.designator.is_generic || !item->next || item->next->kind==ck_designator)reject("static aggregate designator");
        if(t->kind==tk_array) {
          if(item->variant.designator.is_field_designator)reject("field designator in static array");
          index=item->variant.designator.variant.array_element;
        } else {
          if(!item->variant.designator.is_field_designator)reject("array designator in static record");
          auto selected=item->variant.designator.variant.field;bool found=false;
          for(auto f=t->variant.class_struct_union.field_list;f;f=f->next)if(f==selected)found=true;
          if(!found)reject("static record designator ownership");field=selected;
        }
        item=item->next;
      }
      auto value=item;unsigned long long count=1;
      if(item->kind==ck_init_repeat){value=item->variant.init_repeat.constant;count=item->variant.init_repeat.count;}
      if(!count)reject("empty static initializer repetition");
      if(t->kind==tk_array) {
        auto element=skip_typerefs(t->variant.array.element_type);unsigned long long extent=t->variant.array.variant.number_of_elements;
        if(index>extent || count>extent-index)reject("static array initializer bounds");
        // The C backend permits flattened scalar repetition across nested rows.
        // Handle that representation at its physical element stride.
        if(item->kind==ck_init_repeat && value->type && skip_typerefs(value->type)->size!=element->size) {
          auto leaf=element;while(leaf->kind==tk_array)leaf=skip_typerefs(leaf->variant.array.element_type);
          if(skip_typerefs(value->type)->size!=leaf->size || !leaf->size)reject("static flattened initializer stride");
          auto begin=index*element->size;
          if(begin>t->size || count>(t->size-begin)/leaf->size)reject("static flattened initializer bounds");
          for(unsigned long long n=0;n<count;++n)initialize_storage(id,leaf,value,offset+begin+n*leaf->size);
          if((begin+count*leaf->size)%element->size)reject("static flattened initializer partial row");
          index=(begin+count*leaf->size)/element->size;
        } else for(unsigned long long n=0;n<count;++n,++index)initialize_storage(id,element,value,offset+index*element->size);
      } else {
        if(count!=1 || !field)reject("static record initializer field");
        if(field->is_bit_field) {
          if(value->kind!=ck_integer || field->bit_size>64 || field->offset_bit_remainder>7)reject("static bitfield initializer");
          a_boolean overflow=FALSE;
          unsigned long long bits=int_constant_is_signed(value)?(unsigned long long)value_of_integer_constant(value,&overflow):
            (unsigned long long)unsigned_value_of_integer_constant(value,&overflow);
          if(overflow)reject("static bitfield integer range");
          auto &bytes=bitfield_data[id];
          for(unsigned bit=0;bit<field->bit_size;++bit) {
            unsigned position=field->offset_bit_remainder+bit;
            auto &byte=bytes[offset+field->offset+position/8];unsigned char mask=(unsigned char)(1U<<(position%8));
            byte=(unsigned char)((byte&~mask)|((bits>>bit&1U)?mask:0));
          }
        } else initialize_storage(id,field->type,value,offset+field->offset);
        field=next_non_empty_initializable_field(field->next);
      }
    }
  }
  a_constant_ptr storage_constant(an_init_kind kind,an_initializer_ptr initializer) {
    // Stock c_gen_be::constant_initializer distinguishes the representation
    // marker initk_dynamic from actual executable initialization. Retain the
    // same constant/non-delayed test, with destruction still fail-closed.
    if(kind==initk_static)return initializer->constant;
    if(kind==initk_dynamic) {
      auto init=initializer->dynamic;
      if(init && init->kind==dik_constant && !init->follows_an_exec_statement && !init->destructor) {
        check_lifetime(init->init_expr_lifetime);
        return init->variant.constant.ptr;
      }
    }
    return nullptr;
  }
  unsigned storage(a_variable_ptr v) {
    if(!v)reject("missing storage variable");
    auto prior=storage_ids.find(v);if(prior!=storage_ids.end())return prior->second;
    diagnostic_scope diagnostic(v->source_corresp.decl_position);
    if(register_bound_variable(v))reject("register-bound storage");
#if GNU_EXTENSIONS_ALLOWED
    if(v->is_weak || v->is_weakref || v->is_gnu_alias)reject("weak/aliased storage linkage");
#endif
    auto t=skip_typerefs(v->type);unsigned alignment=v->alignment>t->alignment?v->alignment:t->alignment;
    bool declaration=v->storage_class==sc_extern || v->suppress_inline_definition;
    if((!t->size && !declaration) || (!alignment && !declaration) || (alignment && (alignment&(alignment-1))))
      reject((string("incomplete storage layout: ")+source_spelling(&v->source_corresp)+" kind="+std::to_string(t->kind)+" storage="+std::to_string(v->storage_class)).c_str());
    bool external=v->source_corresp.name_linkage==nlk_external || v->source_corresp.name_linkage==nlk_cplusplus_external;
    string key=variable_linker_symbol(v);if(external && key.empty())reject("missing external storage identity");
    auto replacement=static_initializers.find(v);
    a_constant_ptr initializer=declaration?nullptr:replacement==static_initializers.end()?storage_constant(v->init_kind,&v->initializer):replacement->second;
    if(!declaration && v->init_kind==initk_dynamic && !initializer)
      reject((string("dynamic static/TLS initialization: ")+source_spelling(&v->source_corresp)+" ["+key+"]").c_str());
    unsigned id=next_storage++;storage_ids[v]=id;
    auto captured=dot_object_source::state().variables.find(v);
    unsigned source=captured==dot_object_source::state().variables.end() || dot_object_source::abi_storage_type(v->type)?0:source_type(captured->second.type);
    storages.push_back("{\"id\":"+number(id)+",\"sourceName\":"+quote(source_spelling(&v->source_corresp))+
      ",\"linkage\":"+quote(external?"external":"internal")+",\"externalKey\":"+(external?quote(key):"null")+
      ",\"definition\":"+quote(declaration?"declaration":"strong")+",\"typeId\":"+number(type_id(t))+
      ",\"sourceTypeId\":"+number(source)+",\"isThreadLocal\":"+(thread_storage(v)?"true":"false")+
      ",\"isConst\":"+((get_type_qualifiers(v->type)&TQ_CONST)?"true":"false")+",\"alignment\":"+std::to_string(alignment)+position(v->source_corresp.decl_position)+"}");
    if(!declaration) {
      if(initializer)initialize_storage(id,t,initializer,0);
      else if(v->init_kind!=initk_none && v->init_kind!=initk_zero)reject("static storage initializer",v->init_kind);
    }
    return id;
  }
  void collect_storage(a_scope_ptr scope) {
    for(auto init=scope->local_static_variable_inits;init;init=init->next) {
      diagnostic_scope diagnostic(init->variable->source_corresp.decl_position);
      if(ignore_variable_in_back_end(init->variable))continue;
      an_init_kind kind;an_initializer_ptr initializer;
      get_variable_initializer(init->variable,scope,&kind,&initializer);
      auto constant=storage_constant(kind,initializer);
      if(constant)static_initializers[init->variable]=constant;
      else if(kind!=initk_none && kind!=initk_zero)
        reject((string("local static initialization schedule: ")+source_spelling(&init->variable->source_corresp)+
          " ["+variable_linker_symbol(init->variable)+"]").c_str());
    }
    // Definitions retain library-visible storage even without a use in this
    // unit. Extern declarations enter the execution graph when referenced;
    // their pre-lowering source snapshots remain available independently.
    for(auto v=scope->variables;v;v=v->next) {
      diagnostic_scope diagnostic(v->source_corresp.decl_position);
      // Match stock C dump_scope_variables: dependent/nonreal declarations
      // and extern declarations do not contribute initialization definitions.
      if(ignore_variable_in_back_end(v) || v->storage_class==sc_extern || v->suppress_inline_definition)continue;
      an_init_kind kind;an_initializer_ptr initializer;
      get_variable_initializer(v,scope,&kind,&initializer);
      auto constant=storage_constant(kind,initializer);
      if(constant)static_initializers[v]=constant;
      if(kind==initk_dynamic && !constant)
        reject((string("dynamic static/TLS initialization schedule: ")+source_spelling(&v->source_corresp)+
          " ["+variable_linker_symbol(v)+"]").c_str());
      bool root=v->source_corresp.name_linkage==nlk_external || v->source_corresp.name_linkage==nlk_cplusplus_external;
      if(v->compiler_generated || v->lowering_generated || (v->is_template_variable && !v->is_specialized))root=false;
      if(v->explicit_instantiation && !v->explicit_do_not_instantiate)root=true;
#if GNU_EXTENSIONS_ALLOWED
      root=root || v->has_gnu_used_attribute;
#endif
      if(root)storage(v);
    }
  }
  std::set<a_scope_ptr> bound_scopes;
  std::set<unsigned> declared_bindings;
  struct Control { unsigned node; source_control_context labels; };
  std::vector<Control> controls;
  std::set<a_label_ptr> control_labels;
  std::map<a_label_ptr,unsigned> source_labels;
  std::set<a_label_ptr> emitted_source_labels;
  unsigned label_id(a_label_ptr label) {
    auto prior=source_labels.find(label);if(prior!=source_labels.end())return prior->second;
    unsigned id=next_node++;source_labels[label]=id;return id;
  }
  std::map<a_statement_ptr,unsigned> switch_cases;
  string number(unsigned value){return value?std::to_string(value):"null";}
  unsigned pointer_type(unsigned pointee) {
    auto prior=pointer_types.find(pointee);if(prior!=pointer_types.end())return prior->second;
    unsigned id=next_execution_type++;pointer_types[pointee]=id;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":\"pointer\",\"pointeeTypeId\":"+number(pointee)+",\"elementTypeId\":null,\"elementCount\":null}");
    return id;
  }
  unsigned array_type(unsigned element,unsigned long long count) {

    auto key=std::make_pair(element,count);
    auto prior=array_types.find(key);if(prior!=array_types.end())return prior->second;
    if(count>2147483647ULL)reject("array count outside bounded fixed storage profile");
    unsigned id=next_execution_type++;array_types[key]=id;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":\"array\",\"pointeeTypeId\":null,\"elementTypeId\":"+
      number(element)+",\"elementCount\":"+std::to_string(count)+"}");
    return id;
  }
  unsigned type_id(a_type_ptr type) {
    unsigned q=get_type_qualifiers(type);
    if(q & TQ_C11_ATOMIC)reject("atomic type");
    auto t=skip_typerefs(type);
    if(t->kind==tk_void)return 1;
    if(t->kind==tk_routine)return routine_type(t);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
      if(t->variant.class_struct_union.extra_info && t->variant.class_struct_union.extra_info->is_va_list_tag) {
        if(t->size!=24 || t->alignment!=8)reject("va_list target layout mismatch");
        return va_list_type();
      }
      return record_type(t);
    }
    if(t->kind==tk_pointer) {
      if(t->variant.pointer.is_reference)reject("C++ reference type");
      return pointer_type(type_id(t->variant.pointer.type));
    }
    if(t->kind==tk_array) {
      if(t->variant.array.is_variable_size_array || t->variant.array.is_vla ||
         t->variant.array.is_template_dependent_size_array || t->size>2147483647ULL)
        reject("array outside bounded fixed storage profile");
      auto element=skip_typerefs(t->variant.array.element_type);

      auto count=t->variant.array.variant.number_of_elements;
      if(!element->size || (count && (t->size/element->size!=count || t->size%element->size)) || (!count && t->size))
        reject("array size/stride mismatch");
      return array_type(type_id(t->variant.array.element_type),count);
    }
    if(t->kind==tk_float) {
      if(t->variant.float_kind==fk_float && t->size==4)return 10;
      if(t->variant.float_kind==fk_double && t->size==8)return 11;
      if((t->variant.float_kind==fk_long_double || t->variant.float_kind==fk_float80 || t->variant.float_kind==fk_float64x) &&
         t->size==16 && t->alignment==16)return f80_type();
      reject("floating type outside IEEE binary32/binary64/x87-binary80 profile",t->variant.float_kind);
    }
    // EDG retains the bool marker alongside enum bookkeeping after lowering;
    // unlike a user enum, its executable representation is the lowered byte.
    if(t->kind==tk_integer && t->variant.integer.bool_type && t->size==1)return 3;
    if(t->kind==tk_integer) {
      // Match the existing dot backend's physical LP64 integer mapping.
      // In particular, plain char follows EDG's configured signedness;
      // long and long long share a representation, not a source type identity.
      switch(t->variant.integer.int_kind) {
        case ik_char:if(t->size==1)return targ_has_signed_chars?4:3;break;
        case ik_signed_char:if(t->size==1)return 4;break;
        case ik_unsigned_char:if(t->size==1)return 3;break;
        case ik_short:if(t->size==2)return 5;break;
        case ik_unsigned_short:if(t->size==2)return 6;break;
        case ik_int:if(t->size==4)return 2;break;
        case ik_unsigned_int:if(t->size==4)return 7;break;
        case ik_long:case ik_long_long:if(t->size==8)return 8;break;
        case ik_unsigned_long:case ik_unsigned_long_long:if(t->size==8)return 9;break;
        case ik_int128:if(t->size==16 && t->alignment==16)return integer128_type(false);break;
        case ik_unsigned_int128:if(t->size==16 && t->alignment==16)return integer128_type(true);break;
        default:break;
      }
    }
    reject("type outside void/fixed-width-integer/data-pointer profile",t->kind);
  }
  void source_files(a_source_file_ptr file,unsigned parent) {
    for(;file;file=file->next) {
      unsigned id=next_file++;file_ids[file]=id;
      const char *path=file->full_name?file->full_name:file->file_name;
      files.push_back("{\"id\":"+number(id)+",\"path\":"+quote(path?path:"")+",\"parentId\":"+number(parent)+"}");
      source_files(file->first_child_file,id);
    }
  }
  a_source_file_ptr source_file(a_source_file_ptr file,a_seq_number seq) {
    for(;file;file=file->next)if(seq>=file->first_seq_number && seq<=file->last_seq_number) {
      auto child=source_file(file->first_child_file,seq);return child?child:file;
    }
    return nullptr;
  }
  string position(const a_source_position &pos) {
    unsigned id=0; a_line_number line=0;
    if(pos.seq) {
      auto file=source_file(il_header.primary_source_file,pos.seq);
      if(file)id=file_ids.at(file);
      a_const_char *display=nullptr,*full=nullptr;a_boolean at_end=FALSE;
      conv_seq_to_file_and_line(pos.seq,&display,&full,&line,&at_end);
    }
    return ",\"sourceFileId\":"+number(id)+",\"line\":"+std::to_string((unsigned long long)line)+",\"column\":"+std::to_string((unsigned)pos.column);
  }
  string macro_position(const a_source_position &pos,const string &origin) {
    if(origin!="source")return ",\"sourceFileId\":null,\"line\":0,\"column\":0";
    if(!pos.seq || !pos.column || !source_file(il_header.primary_source_file,pos.seq))
      reject("macro position outside captured source files");
    return position(pos);
  }
  void capture_macros() {
    const auto &captured=dot_object_macros::state();
    if(!captured.unsupported.empty())reject(captured.unsupported.c_str());
    for(size_t i=0;i<captured.definitions.size();++i) {
      const auto &value=captured.definitions[i];unsigned id=(unsigned)i+1;
      macro_definitions.push_back("{\"id\":"+number(id)+",\"name\":"+quote(value.name)+
        ",\"form\":"+quote(value.object_like?"object":"function")+
        ",\"variadic\":"+(value.variadic?"true":"false")+
        ",\"dynamic\":"+(value.dynamic?"true":"false")+
        ",\"definitionText\":"+quote(value.text)+",\"origin\":"+quote(value.origin)+
        macro_position(value.position,value.origin)+"}");
      for(size_t p=0;p<value.parameters.size();++p)
        macro_parameters.push_back("{\"definitionId\":"+number(id)+",\"ordinal\":"+std::to_string(p)+
          ",\"name\":"+quote(value.parameters[p])+"}");
    }
    for(size_t i=0;i<captured.events.size();++i) {
      const auto &value=captured.events[i];
      macro_events.push_back("{\"id\":"+number((unsigned)i+1)+",\"kind\":"+quote(value.kind)+
        ",\"name\":"+quote(value.name)+",\"definitionId\":"+number(value.definition)+
        ",\"origin\":"+quote(value.origin)+macro_position(value.position,value.origin)+"}");
    }
  }
  unsigned source_execution_type(unsigned captured) {
    const auto &type=dot_object_source::state().types.at(captured-1);
    if(type.kind=="record" && dot_object_source::state().abi_records.count(type.record_type))
      reject("compiler ABI record cannot masquerade as an original source type");
    if(type.execution)return type.execution;
    if(type.is_int128)return integer128_type(type.is_unsigned_int128);
    if(type.kind=="record")return record_type(type.record_type);
    if(type.kind=="va-list")return va_list_type();
    if(type.kind=="long-double" || type.kind=="float80" || type.kind=="float64x")return f80_type();
    if(type.kind=="routine") {
      std::vector<unsigned> args;for(auto parameter:type.parameters) {
        unsigned unqualified=parameter;
        while(dot_object_source::state().types.at(unqualified-1).kind=="typedef" ||
              dot_object_source::state().types.at(unqualified-1).kind=="qualified")
          unqualified=dot_object_source::state().types.at(unqualified-1).underlying;
        const auto &argument=dot_object_source::state().types.at(unqualified-1);
        args.push_back(argument.kind=="array"?pointer_type(source_execution_type(argument.underlying)):
          argument.kind=="routine"?pointer_type(source_execution_type(unqualified)):source_execution_type(parameter));
      }
      return signature_type(source_execution_type(type.result),args,type.variadic,type.exception_policy);
    }
    if(type.kind=="pointer")return pointer_type(source_execution_type(type.underlying));
    if(type.kind=="array")return array_type(source_execution_type(type.underlying),type.element_count);
    if(type.kind=="qualified" || type.kind=="typedef")return source_execution_type(type.underlying);
    reject("source type outside scalar/data-pointer declaration profile");
  }
  unsigned source_type(unsigned captured) {
    if(!captured)reject("missing pre-lowering scalar type");
    auto prior=source_type_ids.find(captured);if(prior!=source_type_ids.end())return prior->second;
    const auto &type=dot_object_source::state().types.at(captured-1);
    unsigned id=next_source_type++;source_type_ids[captured]=id;
    unsigned execution=source_execution_type(captured);
    unsigned underlying=type.underlying?source_type(type.underlying):0;
    if(type.kind=="routine") {
      source_routine_types.push_back("{\"typeId\":"+number(id)+",\"returnTypeId\":"+number(source_type(type.result))+",\"variadic\":"+(type.variadic?"true":"false")+",\"exceptionPolicy\":"+quote(type.exception_policy)+",\"parameterMode\":"+quote(type.parameter_mode)+"}");
      unsigned ordinal=0;for(auto parameter:type.parameters)source_routine_parameters.push_back("{\"typeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+",\"parameterTypeId\":"+number(source_type(parameter))+"}");
    }
    source_types.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(type.kind)+",\"sourceName\":"+quote(type.name)+
      ",\"underlyingTypeId\":"+number(underlying)+",\"executionTypeId\":"+number(execution)+
      ",\"isConst\":"+(type.is_const?"true":"false")+",\"isVolatile\":"+(type.is_volatile?"true":"false")+",\"elementCount\":"+(type.kind=="array"?std::to_string(type.element_count):"null")+position(type.position)+"}");
    return id;
  }
  void source_declaration(const dot_object_source::Declaration &declaration,
                          const string &kind,unsigned entity,unsigned binding,unsigned owner) {
    unsigned id=next_source_declaration++;
    source_declarations.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(kind)+",\"sourceName\":"+quote(declaration.name)+
      ",\"sourceTypeId\":"+number(source_type(declaration.type))+",\"entityId\":"+number(entity)+",\"bindingId\":"+number(binding)+
      ",\"functionEntityId\":"+number(owner)+position(declaration.position)+"}");
  }
  void source_function(a_routine_ptr r,unsigned entity) {
    auto found=dot_object_source::state().functions.find(r);
    if(found==dot_object_source::state().functions.end())reject("missing pre-lowering routine capture");
    const auto &captured=found->second;
    source_declaration(captured.declaration,"function",entity,0,0);
    source_functions.push_back("{\"entityId\":"+number(entity)+",\"returnSourceTypeId\":"+number(source_type(captured.declaration.type))+
      ",\"capturePhase\":\"before-lowering\",\"routineSourceTypeId\":"+number(source_type(captured.signature_type))+"}");
    unsigned ordinal=0;
    for(auto parameter:captured.parameters)
      source_parameters.push_back("{\"entityId\":"+number(entity)+",\"ordinal\":"+std::to_string(ordinal++)+",\"sourceTypeId\":"+number(source_type(parameter))+"}");
  }
  unsigned node(const string &kind,unsigned ty,const a_source_position &pos,
                unsigned entity=0,unsigned binding=0,const string &op="",
                const string &value="",const string &category="",const string &order="",unsigned field=0,unsigned storage=0,unsigned reserved_id=0) {
    unsigned id=reserved_id?reserved_id:next_node++;
    nodes.push_back("{\"id\":"+number(id)+",\"functionId\":"+number(function)+",\"kind\":"+quote(kind)+",\"typeId\":"+number(ty)+
      ",\"entityId\":"+number(entity)+",\"bindingId\":"+number(binding)+",\"operator\":"+(op.empty()?"null":quote(op))+
      ",\"constantText\":"+(value.empty()?"null":quote(value))+",\"valueCategory\":"+(category.empty()?"null":quote(category))+
      ",\"evaluationOrder\":"+(order.empty()?"null":quote(order))+position(pos)+",\"fieldId\":"+number(field)+",\"storageId\":"+number(storage)+"}");
    return id;
  }
  void edge(unsigned parent,const string &role,unsigned ordinal,unsigned child) {
    if(!child)reject("missing child node");
    edges.push_back("{\"parentId\":"+number(parent)+",\"role\":"+quote(role)+",\"ordinal\":"+std::to_string(ordinal)+",\"childId\":"+number(child)+"}");
  }
  bool copyable_value(a_type_ptr type) {
    if(get_type_qualifiers(type)&(TQ_VOLATILE|TQ_C11_ATOMIC))return false;
    auto t=skip_typerefs(type);
    if(t->kind==tk_array)return t->size && t->variant.array.variant.number_of_elements && copyable_value(t->variant.array.element_type);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
      if(t->incomplete || t->variant.class_struct_union.any_volatile_member)return false;
      auto &source=dot_object_source::state();
      if(source.abi_records.count(t))return false;
      if(!C_mode()) {
        auto original=source.prelower_records.find(t);
        if(original==source.prelower_records.end() || !original->second.trivial_copy_known || !original->second.trivial_copy)return false;
      }
      for(auto field=t->variant.class_struct_union.field_list;field;field=field->next)
        if(!copyable_value(field->type))return false;
    }
    return true;
  }
  void startup_action(a_routine_ptr r,unsigned ordinal) {
#if GNU_EXTENSIONS_ALLOWED
    if(!r->is_initialization_routine)return;
    diagnostic_scope diagnostic(r->source_corresp.decl_position);
    auto type=skip_typerefs(r->type);
    if(type->kind!=tk_routine || r->function_def_number==NULL_function_def_number)
      reject("startup action without a defined routine");
    auto extra=type->variant.routine.extra_info;
    // GNU platform-specific argc/argv/environment entry conventions are not
    // ordinary calls to void(void), and need their own explicit ABI contract.
    if(skip_typerefs(type->variant.routine.return_type)->kind!=tk_void ||
       extra->param_type_list || extra->has_ellipsis)
      reject("startup action outside void(void) calling contract");
    unsigned long priority=r->has_ctor_priority?gnu_routine_supp(r)->ctor_priority:65535;
    if(priority>65535)reject("startup action priority outside GNU range");
    unsigned entity=routine(r);
    initialization_actions.push_back("{\"id\":"+number(unsigned(initialization_actions.size()+1))+",\"entityId\":"+number(entity)+
      ",\"phase\":\"startup\",\"priority\":"+std::to_string(priority)+
      ",\"ordinal\":"+std::to_string(ordinal)+position(r->source_corresp.decl_position)+"}");
#else
    (void)r;(void)ordinal;
#endif
  }
  unsigned routine(a_routine_ptr r) {
    auto found=routine_ids.find(r);if(found!=routine_ids.end())return found->second;
    diagnostic_scope diagnostic(r->source_corresp.decl_position);
    // The C backend preserves this per-function pragma. The owned graph
    // currently uses the default .NET IEEE environment and cannot represent
    // observed exception flags or a dynamically changed rounding direction.
    if(r->fenv_access==stdc_pv_on)reject("floating environment access");

#if GNU_EXTENSIONS_ALLOWED
    if(r->is_finalization_routine)reject("shutdown actions require an owned atexit/finalization ordering contract");
    if(r->is_weak || r->is_weakref || r->is_gnu_alias || r->is_ifunc)
      reject("special routine linkage");
#endif
    auto t=skip_typerefs(r->type);
    if(t->kind!=tk_routine)reject("nonroutine function type");
    auto extra=t->variant.routine.extra_info;
    auto body=r->function_def_number==NULL_function_def_number?nullptr:scope_for_routine_or_null(r);
    if(r->function_def_number!=NULL_function_def_number && !body)reject("unavailable function body");
    bool known_empty_definition=!extra->prototyped && C_mode() && body &&
      !extra->param_type_list && !extra->has_ellipsis && !body->variant.routine.parameters;
    if(!extra->prototyped && !known_empty_definition)reject("unresolved/nonempty unprototyped function");
    if(extra->explicit_calling_convention)reject("explicit calling convention");


    auto record_value=[](a_type_ptr type){type=skip_typerefs(type);return type->kind==tk_struct || type->kind==tk_class || type->kind==tk_union;};
    if(record_value(t->variant.routine.return_type) && !copyable_value(t->variant.routine.return_type))reject("by-value return with array/volatile storage");
    for(auto p=extra->param_type_list;p;p=p->next)
      if(record_value(p->type) && !copyable_value(p->type))reject("by-value parameter with array/volatile storage");
    unsigned id=next_entity++;routine_ids[r]=id;routines.push_back(r);
    auto key=routine_linker_symbol(r);
    bool external=r->source_corresp.name_linkage==nlk_external || r->source_corresp.name_linkage==nlk_cplusplus_external;
    if(external && key.empty())reject("missing exact external identity");
    bool coalescible=r->is_inline || (r->is_template_function && !r->is_specialized);
#if DO_IL_LOWERING && IA64_ABI
    coalescible=coalescible || r->use_comdat;
#endif
    entities.push_back("{\"id\":"+number(id)+",\"sourceName\":"+quote(source_spelling(&r->source_corresp))+
      ",\"linkage\":"+quote(external?"external":"internal")+",\"externalKey\":"+(external?quote(key):"null")+
      ",\"definition\":"+quote(body?(coalescible?"linkonce":"strong"):"declaration")+",\"returnTypeId\":"+number(type_id(t->variant.routine.return_type))+position(r->source_corresp.decl_position)+",\"routineTypeId\":"+number(routine_type(t,known_empty_definition))+"}");
    source_function(r,id);
    unsigned ordinal=0;
    for(auto p=extra->param_type_list;p;p=p->next)
      parameters.push_back("{\"entityId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+",\"typeId\":"+number(type_id(p->type))+"}");
    return id;
  }
  unsigned binding(a_variable_ptr v,unsigned scope,bool parameter=false,unsigned ordinal=0) {
    auto prior=binding_ids.find(v);if(prior!=binding_ids.end())return prior->second;
    diagnostic_scope diagnostic(v->source_corresp.decl_position);
    if(thread_storage(v) || v->storage_class==sc_static || v->storage_class==sc_extern)reject("nonautomatic storage");
    if(v->init_kind!=initk_none && v->init_kind!=initk_dynamic &&
       !(v->init_kind==initk_zero && (skip_typerefs(v->type)->kind==tk_array || skip_typerefs(v->type)->kind==tk_struct)))
      reject("nonexecutably placed local initializer",v->init_kind);
    unsigned id=next_binding++;binding_ids[v]=id;binding_scopes[v]=scope;
    auto source=dot_object_source::state().variables.find(v);
    bool synthetic=false;
    if(source!=dot_object_source::state().variables.end()) {
      source_declaration(source->second,parameter?"parameter":"local",0,id,function);
      if(source->second.cleanup) {
        if(parameter)reject("parameter cleanup attribute");
        // Partially dynamic arrays can become initk_zero plus separate stores.
        // Their original completion boundary is not represented by stmk_init.
        if(v->init_kind==initk_zero)reject("cleanup with split lowered initialization");
        cleanup_variables.insert(v);
      }
    } else {
      // EDG frontend and lowering temporaries have distinct positive flags.
      // A missing capture alone never licenses inventing source provenance.
      if(parameter || !(v->compiler_generated || v->lowering_generated))
        reject("missing pre-lowering original variable capture");
      synthetic=true;
    }
    bindings.push_back("{\"id\":"+number(id)+",\"functionId\":"+number(function)+",\"kind\":"+quote(parameter?"parameter":"local")+
      ",\"ordinal\":"+(parameter?std::to_string(ordinal):"null")+",\"typeId\":"+number(type_id(v->type))+
      ",\"sourceName\":"+quote(source_spelling(&v->source_corresp))+",\"scopeNodeId\":"+number(scope)+position(v->source_corresp.decl_position)+",\"isSynthetic\":"+(synthetic?"true":"false")+"}");
    return id;
  }
  unsigned local_place(a_variable_ptr variable,const a_source_position &pos) {
    unsigned ty=type_id(variable->type);
    unsigned result=node("binding",ty,pos,0,binding_ids.at(variable),"","","place","none");
    auto captured=dot_object_source::state().variables.find(variable);
    if((get_type_qualifiers(variable->type)&TQ_VOLATILE) ||
       (captured!=dot_object_source::state().variables.end() && source_volatile(captured->second.type))) {
      auto t=skip_typerefs(variable->type);
      if(t->kind!=tk_integer && t->kind!=tk_float && t->kind!=tk_pointer)
        reject("volatile aggregate initialization");
      if(integer128(t))reject("volatile integer128 access");
      unsigned wrapper=node("volatile-place",ty,pos,0,0,"","","place","none");
      edge(wrapper,"operand",0,result);return wrapper;
    }
    return result;
  }
  unsigned register_cleanup(a_variable_ptr v,unsigned block,const a_source_position &pos) {
    if(!cleanup_variables.count(v) || binding_scopes.at(v)!=block)
      reject("cleanup activation outside owning lexical block");
    if(!registered_cleanups.insert(v).second)reject("duplicate cleanup activation");
    auto callback=dot_object_source::state().variables.at(v).cleanup;
    auto signature=skip_typerefs(callback->type);
    if(signature->kind!=tk_routine)reject("nonroutine cleanup callback");
    auto parameter=signature->variant.routine.extra_info->param_type_list;
    if(!parameter || parameter->next)reject("cleanup callback arity");
    auto pointer=skip_typerefs(parameter->type);
    if(pointer->kind!=tk_pointer || pointer->variant.pointer.is_reference)
      reject("nonpointer cleanup callback parameter");
    unsigned pointee=type_id(pointer->variant.pointer.type);
    if(pointee!=1 && pointee!=type_id(v->type))reject("cleanup callback pointer conversion");
    return node("register-cleanup",0,pos,routine(callback),binding_ids.at(v));
  }
  void declaration_cleanups(a_statement_ptr s,unsigned block,unsigned &ordinal) {
    unsigned variable_count=0;
    for(auto e=s->variant.decl.entities;e;e=e->next)
      if(e->entity.kind==iek_variable)++variable_count;
    for(auto e=s->variant.decl.entities;e;e=e->next) {
      if(e->entity.kind!=iek_variable)continue;
      auto v=reinterpret_cast<a_variable_ptr>(e->entity.ptr);
      if(!cleanup_variables.count(v))continue;
      const auto &source=dot_object_source::state().variables.at(v);
      if(source.has_initializer)continue;
      // A declaration can contain interleaved initialized/uninitialized
      // declarators; the single stmk_decl precedes all their stmk_init nodes.
      // Reject this case until each individual completion is retained.
      if(variable_count!=1)reject("uninitialized cleanup in multi-declarator declaration");
      edge(block,"statement",ordinal++,register_cleanup(v,block,s->position));
    }
  }
  void scope_bindings(a_scope_ptr scope,unsigned block,unsigned &ordinal) {
    if(!scope || !bound_scopes.insert(scope).second)return;
    check_lifetime(scope->lifetime);
    if(scope->vla_dimensions)reject("scope VLA storage");
    collect_storage(scope);
    for(auto v=scope->nonstatic_variables;v;v=v->next) {
      unsigned id=binding(v,block);
      if(!declared_bindings.insert(id).second)continue;
      edge(block,"statement",ordinal++,node("declare",0,v->source_corresp.decl_position,0,id));
      // lower_init marks partially dynamic aggregates initk_zero, leaving
      // explicit element stores in the statement stream. As c_gen_be's
      // dump_variable_decl does, zero this storage at lexical scope entry.
      if(v->init_kind==initk_zero)
        edge(block,"statement",ordinal++,initialize_aggregate(v,nullptr,v->source_corresp.decl_position));
    }
  }
  unsigned zero_node(unsigned type,const a_source_position &pos) {
    if(type==va_list_id && va_list_id)return node("default-value",type,pos,0,0,"","","value","none");
    if(type==f80_id && f80_id)return node("constant",type,pos,0,0,"",string(20,'0'),"value","none");
    for(const auto &record:record_ids)if(record.second==type)return node("default-value",type,pos,0,0,"","","value","none");
    return node("constant",type,pos,0,0,"",type==10?"00000000":type==11?"0000000000000000":"0","value","none");
  }
  unsigned constant_node(a_constant_ptr c,const a_source_position &pos) {
    unsigned ty=type_id(c->type);
    if(c->kind==ck_string)
      return node("storage",ty,pos,0,0,"","","place","none",0,literal(c));
    if(c->kind==ck_address) {
      auto &address=c->variant.address;unsigned address_type=ty,result=0;
      if(address.kind==abk_routine) {
        if(address.offset)reject("function address offset");
        address_type=pointer_type(routine_type(skip_typerefs(address.variant.routine->type)));
        result=node("function-address",address_type,pos,routine(address.variant.routine),0,"","","value","none");
      } else {
        unsigned target=address.kind==abk_variable?storage(address.variant.variable):
          address.kind==abk_constant?literal(address.variant.constant):0;
        if(!target)reject("constant address base",address.kind);
        // EDG folds pointer-to-integer casts into ck_address constants. Keep
        // the owned address pointer-typed and represent that cast explicitly;
        // storage-address must never masquerade as an integer value node.
        if(skip_typerefs(c->type)->kind!=tk_pointer) {
          auto target_type=address.kind==abk_variable?address.variant.variable->type:address.variant.constant->type;
          address_type=pointer_type(type_id(target_type));
        }
        result=node("storage-address",address_type,pos,0,0,"",std::to_string(address.offset),"value","none",0,target);
      }
      if(address_type==ty)return result;
      unsigned converted=node("convert",ty,pos,0,0,"","","value","left-to-right");
      edge(converted,"operand",0,result);return converted;
    }
    if(c->kind==ck_float)return node("constant",ty,pos,0,0,"",floating_bits(c),"value","none");
    if(c->kind==ck_aggregate) {
      auto type=skip_typerefs(c->type);
      if((type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union) || !copyable_value(type))
        reject("folded aggregate constant outside complete copyable record");
      if(zero_constant_bits(c))return zero_node(ty,pos);
      return node("storage",ty,pos,0,0,"","","place","none",0,aggregate_literal(c,pos));
    }
    if(c->kind!=ck_integer || ty==10 || ty==11 || (f80_id && ty==f80_id))reject("noninteger/nonfloating constant",c->kind);
    a_boolean overflow=FALSE;
    string text;
    if(integer128(c->type))text=decimal_str_for_integer_constant(c).as_temp_characters();
    else if(int_constant_is_signed(c))text=std::to_string((long long)value_of_integer_constant(c,&overflow));
    else text=std::to_string((unsigned long long)unsigned_value_of_integer_constant(c,&overflow));
    if(overflow)reject("out of range scalar constant");
    if(skip_typerefs(c->type)->kind==tk_pointer && text!="0") {
      unsigned bits=node("constant",9,pos,0,0,"",text,"value","none");
      unsigned pointer=node("convert",ty,pos,0,0,"","","value","left-to-right");
      edge(pointer,"operand",0,bits);return pointer;
    }
    return node("constant",ty,pos,0,0,"",text,"value","none");
  }
  bool place(an_expr_node_ptr e) {
    return e && (e->kind==enk_variable ||
      (e->kind==enk_constant && node_constant(e)->kind==ck_string) ||
      (e->kind==enk_operation && (e->variant.operation.kind==eok_indirect ||
                                 e->variant.operation.kind==eok_subscript || e->variant.operation.kind==eok_lvalue_adjust ||
                                 e->variant.operation.kind==eok_dot_field || e->variant.operation.kind==eok_points_to_field)));
  }
  bool source_volatile(unsigned captured) {
    while(captured) {
      const auto &type=dot_object_source::state().types.at(captured-1);
      if(type.is_volatile)return true;
      if(type.kind!="typedef" && type.kind!="qualified")return false;
      captured=type.underlying;
    }
    return false;
  }
  bool volatile_place(an_expr_node_ptr e) {
    if(!e)return false;
    if(e->volatile_fetch || (get_type_qualifiers(e->type)&TQ_VOLATILE) ||
       (e->orig_lvalue_type && (get_type_qualifiers(e->orig_lvalue_type)&TQ_VOLATILE)))return true;
    if(e->kind==enk_variable) {
      auto v=node_variable(e);
      if(get_type_qualifiers(v->type)&TQ_VOLATILE)return true;
      auto found=dot_object_source::state().variables.find(v);
      return found!=dot_object_source::state().variables.end() && source_volatile(found->second.type);
    }
    if(e->kind!=enk_operation)return false;
    auto a=e->variant.operation.operands;auto b=a?a->next:nullptr;
    auto pointee_volatile=[](an_expr_node_ptr pointer) {
      if(!pointer)return false;
      auto t=skip_typerefs(pointer->type);
      return t->kind==tk_pointer && (get_type_qualifiers(t->variant.pointer.type)&TQ_VOLATILE);
    };
    switch(e->variant.operation.kind) {
      case eok_dot_field:case eok_points_to_field: {
        if(!a || !b || b->kind!=enk_field)return false;
        auto field=node_field(b);
        if(get_type_qualifiers(field->type)&TQ_VOLATILE)return true;
        auto record=dot_object_source::state().records.find(parent_class_of(field));
        if(record!=dot_object_source::state().records.end())
          for(const auto &captured:record->second.fields)if(captured.il==field && source_volatile(captured.source_type))return true;
        return e->variant.operation.kind==eok_dot_field?volatile_place(a):pointee_volatile(a);
      }
      case eok_indirect:return pointee_volatile(a);
      case eok_subscript:return pointee_volatile(a) || pointee_volatile(b);
      default:return false;
    }
  }
  unsigned intrinsic(an_expr_node_ptr call,a_routine_ptr r,an_expr_node_ptr first,const string &order) {
    if(!special_kind_is(r,sfk_none))return 0;
    const char *op=nullptr;
    switch(r->variant.builtin_function_kind) {
      case bfk_clz:case bfk_clzl:case bfk_clzll:op="clz";break;
      case bfk_ctz:case bfk_ctzl:case bfk_ctzll:op="ctz";break;
      case bfk_popcount:case bfk_popcountl:case bfk_popcountll:op="popcount";break;
      case bfk_parity:case bfk_parityl:case bfk_parityll:op="parity";break;
      case bfk_ffs:case bfk_ffsl:case bfk_ffsll:op="ffs";break;
      case bfk_clrsb:case bfk_clrsbl:case bfk_clrsbll:op="clrsb";break;
      case bfk_bswap16:case bfk_bswap32:case bfk_bswap64:op="bswap";break;
      case bfk_add_overflow:case bfk_sadd_overflow:case bfk_saddl_overflow:case bfk_saddll_overflow:
      case bfk_uadd_overflow:case bfk_uaddl_overflow:case bfk_uaddll_overflow:op="add-overflow";break;
      case bfk_sub_overflow:case bfk_ssub_overflow:case bfk_ssubl_overflow:case bfk_ssubll_overflow:
      case bfk_usub_overflow:case bfk_usubl_overflow:case bfk_usubll_overflow:op="sub-overflow";break;
      case bfk_mul_overflow:case bfk_smul_overflow:case bfk_smull_overflow:case bfk_smulll_overflow:
      case bfk_umul_overflow:case bfk_umull_overflow:case bfk_umulll_overflow:op="mul-overflow";break;
      case bfk_expect:op="expect";break;
      case bfk_unreachable:op="unreachable";break;
      case bfk_constant_p:
        if(!first || first->next)reject("constant-p intrinsic arity");
        // The C backend delegates this non-evaluating optimization query.
        // A residual query conservatively returns false, without evaluating
        // its argument or introducing a spurious reference to its entities.
        return zero_node(type_id(call->type),call->position);
      case bfk_is_constant_evaluated:
        if(first)reject("constant-evaluation intrinsic arity");
        return zero_node(type_id(call->type),call->position);
      case bufk_launder:
        if(!first || first->next)reject("launder intrinsic arity");
        return expression(first);
      default:return 0;
    }
    string operation=op;
    if(integer128(call->type))reject("integer128 intrinsic result");
    for(auto arg=first;arg;arg=arg->next)
      if(integer128(arg->type))reject("integer128 intrinsic operand");
    if(operation=="add-overflow" || operation=="sub-overflow" || operation=="mul-overflow") {
      if(!first || !first->next || !first->next->next || first->next->next->next)reject("overflow intrinsic arity");
      auto target=skip_typerefs(first->next->next->type);
      if(target->kind!=tk_pointer || skip_typerefs(target->variant.pointer.type)->kind!=tk_integer)
        reject("overflow intrinsic destination");
      if(integer128(target->variant.pointer.type))reject("integer128 overflow intrinsic destination");
      auto qualifiers=get_type_qualifiers(target->variant.pointer.type);
      if(qualifiers&(TQ_CONST|TQ_C11_ATOMIC))reject("overflow intrinsic destination qualifiers");
      if(qualifiers&TQ_VOLATILE)operation+="-volatile";
    }
    unsigned id=node("intrinsic",type_id(call->type),call->position,0,0,operation,"","value",order),ordinal=0;
    for(auto arg=first;arg;arg=arg->next)edge(id,"argument",ordinal++,expression(arg));
    return id;
  }
  unsigned expression(an_expr_node_ptr e) {
    unsigned result=expression_core(e);
    if(e && place(e) && volatile_place(e)) {
      auto t=skip_typerefs(e->type);
      if(t->kind==tk_integer || t->kind==tk_float || t->kind==tk_pointer) {
        if(integer128(t))reject("volatile integer128 access");
        if(e->kind==enk_operation && (e->variant.operation.kind==eok_dot_field || e->variant.operation.kind==eok_points_to_field)) {
          auto field=e->variant.operation.operands->next;
          if(field && field->kind==enk_field && node_field(field)->is_bit_field)reject("volatile bitfield access");
        }
        unsigned wrapper=node("volatile-place",type_id(e->type),e->position,0,0,"","","place","none");
        edge(wrapper,"operand",0,result);return wrapper;
      }
    }
    return result;
  }
  unsigned expression_core(an_expr_node_ptr e) {
    if(!e)reject("null expression");
    diagnostic_scope diagnostic(e->position);
    unsigned ty=type_id(e->type);
    if(e->kind==enk_constant)return constant_node(node_constant(e),e->position);
    if(e->kind==enk_variable) {
      auto it=binding_ids.find(node_variable(e));if(it==binding_ids.end())
        return node("storage",ty,e->position,0,0,"","","place","none",0,storage(node_variable(e)));
      return node("binding",ty,e->position,0,it->second,"","","place","none");
    }
    if(e->kind==enk_routine) {
      auto target=e->variant.routine.ptr;
      unsigned signature=routine_type(skip_typerefs(target->type));
      unsigned pointer=pointer_type(signature);
      // A routine expression can already include function-to-pointer decay
      // (e.g. return callback_name). Its entity signature owns exactly one
      // pointer layer independently of that contextual expression type.
      if(ty!=signature && ty!=pointer)reject("routine designator signature mismatch");
      return node("function-address",pointer,e->position,routine(target),0,"","","value","none");
    }
    if(e->kind!=enk_operation)reject("expression",e->kind);
    auto &operation=e->variant.operation;
    auto a=operation.operands;auto b=a?a->next:nullptr;
    string order=operation.eval_right_to_left?"right-to-left":operation.eval_left_to_right?"left-to-right":"unspecified";
    if(operation.kind==eok_va_start || operation.kind==eok_va_start_single_operand ||
       operation.kind==eok_va_copy || operation.kind==eok_va_end || operation.kind==eok_va_arg) {
      if(!a)reject("missing va_list operand");
      const char *kind=operation.kind==eok_va_start || operation.kind==eok_va_start_single_operand?"va-start":
        operation.kind==eok_va_copy?"va-copy":operation.kind==eok_va_end?"va-end":"va-arg";
      unsigned id=node(kind,ty,e->position,0,0,"","","value",order);
      auto list_pointer=[&](an_expr_node_ptr list) {
        auto t=skip_typerefs(list->type);
        unsigned value=expression(list);
        if(t->kind==tk_pointer)return value;
        if(!place(list) || type_id(t)!=va_list_type())reject("va_list operand outside list/pointer");
        unsigned address=node("address",pointer_type(va_list_type()),list->position,0,0,"","","value","none");
        edge(address,"operand",0,value);return address;
      };
      edge(id,"operand",0,list_pointer(a));
      if(operation.kind==eok_va_copy) {
        if(!b || b->next)reject("va_copy arity");
        edge(id,"source",0,list_pointer(b));
      } else if(operation.kind==eok_va_start || operation.kind==eok_va_start_single_operand) {
        a_routine_ptr current=nullptr;for(const auto &entry:routine_ids)if(entry.second==function){current=entry.first;break;}
        if(!current || !skip_typerefs(current->type)->variant.routine.extra_info->has_ellipsis)
          reject("va_start outside variadic function");
        // The second token names the last fixed parameter; like C's builtin,
        // it is a semantic anchor rather than an evaluated argument.
        if(operation.kind==eok_va_start && (!b || b->next || b->kind!=enk_variable))
          reject("va_start last parameter anchor");
        if(operation.kind==eok_va_start_single_operand && b)reject("va_start arity");
      } else if(b)reject("va_list operator arity");
      return id;
    }
    if(operation.kind==eok_dot_field || operation.kind==eok_points_to_field) {
      if(!a || !b || b->next || b->kind!=enk_field)reject("malformed record member selection");
      auto base=skip_typerefs(a->type);
      if(operation.kind==eok_points_to_field) {
        if(base->kind!=tk_pointer)reject("member arrow without pointer");
        base=skip_typerefs(base->variant.pointer.type);
      } else if(!place(a))reject("record member of nonplace value");
      unsigned owner=type_id(base);auto field=node_field(b);
      if(!field_ids.count(field) || parent_class_of(field)!=base || type_id(field->type)!=ty)
        reject("record member identity mismatch");
      unsigned operand=expression(a);
      if(operation.kind==eok_points_to_field) {
        unsigned indirect=node("dereference",owner,a->position,0,0,"","","place","none");
        edge(indirect,"operand",0,operand);operand=indirect;
      }
      unsigned id=node("member",ty,e->position,0,0,"","","place",order,field_ids.at(field));
      edge(id,"operand",0,operand);return id;
    }
    if(operation.kind==eok_call) {
      if(!a)reject("missing call target");
      bool direct=a->kind==enk_routine;
      if(direct) {
        unsigned builtin=intrinsic(e,a->variant.routine.ptr,b,order);if(builtin)return builtin;
        auto signature=skip_typerefs(a->variant.routine.ptr->type);
        if(!signature->variant.routine.extra_info->prototyped && b)
          reject("actual arguments to an unspecified-parameter function");
      }
      unsigned id=node(direct?"call":"indirect-call",ty,e->position,direct?routine(a->variant.routine.ptr):0,0,"","","value",order),ordinal=0;
      if(!direct)edge(id,"operand",0,expression(a));
      auto callee_type=skip_typerefs(a->type);
      if(callee_type->kind==tk_pointer)callee_type=skip_typerefs(callee_type->variant.pointer.type);
      auto formal=callee_type->kind==tk_routine?callee_type->variant.routine.extra_info->param_type_list:nullptr;
      bool builtin=direct && is_gnu_builtin_function(a->variant.routine.ptr);
      for(auto arg=b;arg;arg=arg->next) {
        unsigned argument=expression(arg);
        if(builtin && formal) {
          // Some polymorphic GNU atomics retain their argument types. The C
          // backend emits the call against its typed builtin prototype, which
          // supplies these standard conversions. Own them explicitly rather
          // than weakening the managed call signature verifier.
          auto target=skip_typerefs(formal->type),source=skip_typerefs(arg->type);
          if(target->kind==tk_integer && target->variant.integer.bool_type &&
             !(source->kind==tk_integer && source->variant.integer.bool_type)) {
            if(source->kind!=tk_integer && source->kind!=tk_float && source->kind!=tk_pointer)
              reject("builtin bool argument outside scalar domain");
            unsigned converted=node("binary",type_id(formal->type),arg->position,0,0,"ne","","value","left-to-right");
            edge(converted,"left",0,argument);edge(converted,"right",0,zero_node(type_id(arg->type),arg->position));argument=converted;
          } else if(target->kind==tk_pointer && source->kind==tk_pointer &&
                    skip_typerefs(target->variant.pointer.type)->kind==tk_void && type_id(formal->type)!=type_id(arg->type)) {
            unsigned converted=node("convert",type_id(formal->type),arg->position,0,0,"","","value","left-to-right");
            edge(converted,"operand",0,argument);argument=converted;
          }
        }
        edge(id,"argument",ordinal++,argument);if(formal)formal=formal->next;
      }
      return id;
    }
    const char *compound=nullptr;
    switch(operation.kind) {
      case eok_add_assign:compound="add";break;case eok_subtract_assign:compound="sub";break;
      case eok_multiply_assign:compound="mul";break;case eok_divide_assign:compound="div";break;
      case eok_remainder_assign:compound="rem";break;case eok_and_assign:compound="bit-and";break;
      case eok_or_assign:compound="bit-or";break;case eok_xor_assign:compound="bit-xor";break;
      case eok_shiftl_assign:compound="shl";break;case eok_shiftr_assign:compound="shr";break;
      case eok_padd_assign:compound="pointer-add";break;case eok_psubtract_assign:compound="pointer-subtract";break;
      default:break;
    }
    if(compound) {
      if(!a || !b || b->next || !place(a) || type_id(a->type)!=ty)reject("malformed compound assignment");
      // Preserve EDG's sequenced operand order. The managed consumer lifts
      // RHS evaluation before the target address/read when C#'s compound
      // assignment order would differ, including x += (x = value).
      unsigned id=node("compound-assign",ty,e->position,0,0,compound,"","value",order);
      edge(id,"target",0,expression(a));edge(id,"value",0,expression(b));return id;
    }
    if(operation.kind==eok_assign) {
      if((skip_typerefs(e->type)->kind==tk_struct || skip_typerefs(e->type)->kind==tk_class || skip_typerefs(e->type)->kind==tk_union) &&
         (!copyable_value(e->type) || volatile_place(a) || volatile_place(b)))reject("record copy with array/volatile storage");
      if(!a || !b || b->next || !place(a))reject("assignment outside supported memory places");
      // C++17 evaluates the RHS before the LHS. Keep that owned order rather
      // than rewriting/revisiting source operands here. The managed emitter
      // captures the RHS then the target address exactly once when required.
      unsigned id=node("assign",ty,e->position,0,0,"","","value",order);
      edge(id,"target",0,expression(a));edge(id,"value",0,expression(b));return id;
    }
    if(operation.kind==eok_lvalue_adjust) {
      if(!a || b || !place(a))reject("lvalue adjustment outside places");
      unsigned original=type_id(a->type);
      unsigned address=node("address",pointer_type(original),e->position,0,0,"","","value","none");
      edge(address,"operand",0,expression(a));
      unsigned cast=node("convert",pointer_type(ty),e->position,0,0,"","","value","left-to-right");
      edge(cast,"operand",0,address);
      unsigned result=node("dereference",ty,e->position,0,0,"","","place","none");
      edge(result,"operand",0,cast);return result;
    }
    if(operation.kind==eok_indirect && a && !b && function_pointer(a->type)) {
      // C's *function_pointer designates that same function. EDG can retain
      // its decayed pointer result, so inspect the operand as the existing dot
      // backend does, not only whether the result still has tk_routine kind.
      unsigned source=type_id(a->type);
      if(ty!=source && pointer_type(ty)!=source)reject("function designator representation mismatch");
      return expression(a);
    }
    if(operation.kind==eok_address_of && a && !b && a->kind==enk_routine)
      return node("function-address",ty,e->position,routine(a->variant.routine.ptr),0,"","","value","none");
    if(operation.kind==eok_address_of && a && !b && skip_typerefs(a->type)->kind==tk_routine)
      return expression(a);
    if(operation.kind==eok_array_to_pointer) {
      if(!a || b || !place(a))reject("array decay outside supported places");
      auto source=skip_typerefs(a->type),target=skip_typerefs(e->type);
      if(source->kind!=tk_array || target->kind!=tk_pointer ||
         type_id(source->variant.array.element_type)!=type_id(target->variant.pointer.type))
        reject("array decay element representation mismatch");
      unsigned id=node("array-decay",ty,e->position,0,0,"","","value",order);
      edge(id,"operand",0,expression(a));return id;
    }
    if(operation.kind==eok_address_of || operation.kind==eok_indirect) {
      if(!a || b)reject("memory-place operator arity");
      bool address=operation.kind==eok_address_of;
      auto pointer=skip_typerefs(address?e->type:a->type);
      if(pointer->kind!=tk_pointer || pointer->variant.pointer.is_reference ||
         skip_typerefs(pointer->variant.pointer.type)->kind==tk_void)
        reject("memory-place operator outside typed data pointers");
      if(address && !place(a))reject("address outside supported memory places");
      if(type_id(pointer->variant.pointer.type)!=type_id(address?a->type:e->type))
        reject("memory-place pointee representation mismatch");
      unsigned id=node(address?"address":"dereference",ty,e->position,0,0,"","",address?"value":"place",order);
      edge(id,"operand",0,expression(a));return id;
    }
    if(operation.kind==eok_subscript) {
      if(!a || !b || b->next)reject("subscript arity");
      auto left=skip_typerefs(a->type),right=skip_typerefs(b->type);
      auto pointer=left->kind==tk_pointer?left:right;
      if(pointer->kind!=tk_pointer || pointer->variant.pointer.is_reference ||
         type_id(pointer->variant.pointer.type)!=ty || ty==1)
        reject("subscript outside typed data pointers");
      unsigned add=node("binary",type_id(pointer),e->position,0,0,"pointer-add","","value",order);
      // Preserve syntactic operand order, including the valid C form index[p].
      edge(add,"left",0,expression(a));edge(add,"right",0,expression(b));
      unsigned id=node("dereference",ty,e->position,0,0,"","","place","none");
      edge(id,"operand",0,add);return id;
    }
    if(operation.kind==eok_comma) {
      if(!a || !b)reject("sequence expression arity");
      unsigned id=node("sequence",ty,e->position,0,0,"","","value","left-to-right"),ordinal=0;
      for(auto item=a;item;item=item->next)edge(id,"item",ordinal++,expression(item));
      return id;
    }
    if(operation.kind==eok_cast) {
      if(!a || b)reject("unsupported scalar conversion");
      unsigned id=node("convert",ty,e->position,0,0,"","","value",order);
      edge(id,"operand",0,expression(a));return id;
    }
    if(operation.kind==eok_question) {
      // Preserve lazy branch selection. The stock C backend requires both
      // lowered arm types to match the result; do not recreate promotions.
      // A scalar lvalue conditional is consumed here as the selected value.
      // Selected conditional places remain outside this bounded profile.
      if(!a || !b || !b->next || b->next->next || ty==1)
        reject("conditional outside scalar value profile");
#if GNU_EXTENSIONS_ALLOWED
      if(operation.is_gnu_two_operand_question_mark)
        reject("GNU omitted-middle conditional");
#endif
      if(type_id(b->type)!=ty || type_id(b->next->type)!=ty)
        reject("conditional arm representation mismatch");
      unsigned id=node("conditional",ty,e->position,0,0,"","","value",order);
      edge(id,"condition",0,expression(a));edge(id,"then",0,expression(b));
      edge(id,"else",0,expression(b->next));return id;
    }
    if(operation.kind==eok_pre_incr || operation.kind==eok_post_incr ||
       operation.kind==eok_pre_decr || operation.kind==eok_post_decr) {
      if(!a || b || !place(a))reject("mutation outside supported memory places");
      auto operand_type=skip_typerefs(a->type);
      bool integer=operand_type->kind==tk_integer && !operand_type->variant.integer.bool_type;
      bool floating=operand_type->kind==tk_float;
      bool pointer=operand_type->kind==tk_pointer && !operand_type->variant.pointer.is_reference &&
        skip_typerefs(operand_type->variant.pointer.type)->kind!=tk_void;
      if((!integer && !floating && !pointer) || type_id(a->type)!=ty)
        reject("mutation outside numeric/data-pointer profile");
      string op=operation.kind==eok_pre_incr?"pre-inc":operation.kind==eok_post_incr?"post-inc":
                operation.kind==eok_pre_decr?"pre-dec":"post-dec";
      unsigned id=node("mutate",ty,e->position,0,0,op,"","value",order);
      edge(id,"operand",0,expression(a));return id;
    }
    const char *op=nullptr;bool unary=false;
    switch(operation.kind) {
      case eok_add:op="add";break;case eok_subtract:op="sub";break;case eok_multiply:op="mul";break;
      // As in c_gen_be.c, these operands are element counts, not byte offsets.
      // The void/empty-struct arithmetic extensions are outside this profile.
      case eok_padd:op="pointer-add";break;
      case eok_psubtract:op="pointer-subtract";break;
      case eok_pdiff:op="pointer-difference";break;
      case eok_divide:op="div";break;case eok_remainder:op="rem";break;
      case eok_and:op="bit-and";break;case eok_or:op="bit-or";break;case eok_xor:op="bit-xor";break;
      case eok_shiftl:op="shl";break;case eok_shiftr:op="shr";break;
      case eok_eq:op="eq";break;case eok_ne:op="ne";break;case eok_lt:op="lt";break;
      case eok_le:op="le";break;case eok_gt:op="gt";break;case eok_ge:op="ge";break;
      case eok_land:op="and";break;case eok_lor:op="or";break;
      case eok_negate:op="neg";unary=true;break;case eok_not:op="not";unary=true;break;
      case eok_unary_plus:op="plus";unary=true;break;
      case eok_complement:op="bit-not";unary=true;break;
      default:reject("operator",operation.kind);
    }
    if(!a || (unary?(b!=nullptr):(!b || b->next)))reject("operator arity");
    if(operation.kind==eok_padd || operation.kind==eok_psubtract || operation.kind==eok_pdiff) {
      auto left=skip_typerefs(a->type),right=skip_typerefs(b->type);
      if((left->kind==tk_pointer && skip_typerefs(left->variant.pointer.type)->kind==tk_void) ||
         (right->kind==tk_pointer && skip_typerefs(right->variant.pointer.type)->kind==tk_void))
        reject("void-pointer arithmetic");
    }
    unsigned id=node(unary?"unary":"binary",ty,e->position,0,0,op,"","value",order);
    edge(id,unary?"operand":"left",0,expression(a));if(!unary)edge(id,"right",0,expression(b));return id;
  }
  a_type_ptr array_leaf(a_type_ptr type) {
    type=skip_typerefs(type);
    while(type->kind==tk_array)type=skip_typerefs(type->variant.array.element_type);
    return type;
  }
  unsigned long long leaf_count(a_type_ptr type) {
    type=skip_typerefs(type);auto leaf=array_leaf(type);
    if(!leaf->size || type->size%leaf->size)reject("array initializer leaf stride");
    return (unsigned long long)(type->size/leaf->size);
  }
  void array_element(unsigned parent,unsigned &ordinal,a_type_ptr type,
                     unsigned long long index,unsigned value,const a_source_position &position) {
    unsigned id=node("array-element",type_id(type),position,0,0,"",std::to_string(index));
    edge(parent,"element",ordinal++,id);edge(id,"value",0,value);
  }
  bool dynamic_constant(a_constant_ptr value) {
    if(value->kind==ck_dynamic_init)return true;
    if(value->kind==ck_aggregate)return value->variant.aggregate.has_dynamic_init_component;
    if(value->kind==ck_init_repeat)return dynamic_constant(value->variant.init_repeat.constant);
    return false;
  }
  void array_initializer(unsigned parent,unsigned &ordinal,a_type_ptr type,a_constant_ptr value,
                         unsigned long long offset,const a_source_position &position) {
    type=skip_typerefs(type);
    if(!value)return; // The array-init operation supplies omitted zero elements.
    if(type->kind!=tk_array) {
      unsigned expr=0;
      if(value->kind==ck_dynamic_init) {
        auto d=value->variant.dynamic_init.ptr;
        if(d->destructor)reject("array element destruction");
        if(d->kind==dik_expression)expr=expression(d->variant.expression);
        else if(d->kind==dik_constant)expr=constant_node(d->variant.constant.ptr,position);
        else if(d->kind==dik_zero)expr=zero_node(type_id(type),position);
        else reject("array element dynamic initializer",d->kind);
      } else expr=constant_node(value,position);
      array_element(parent,ordinal,type,offset,expr,position);return;
    }
    unsigned long long count=type->variant.array.variant.number_of_elements;
    auto element=skip_typerefs(type->variant.array.element_type);
    auto stride=leaf_count(element),total=leaf_count(type);
    if(value->kind==ck_string) {
      if(element->kind!=tk_integer || element->size!=1 || value->variant.string.length>count)
        reject("string initializer outside bounded byte array");
      bool is_signed=type_id(element)==4;
      for(unsigned long long i=0;i<value->variant.string.length;++i) {
        unsigned char byte=(unsigned char)value->variant.string.value[i];
        int number=is_signed && byte>=128?(int)byte-256:byte;
        unsigned expr=node("constant",type_id(element),position,0,0,"",std::to_string(number),"value","none");
        array_element(parent,ordinal,element,offset+i,expr,position);
      }
      return;
    }
    if(value->kind!=ck_aggregate)reject("array initializer outside aggregate/string",value->kind);
    unsigned long long cursor=0;
    for(auto item=value->variant.aggregate.first_constant;item;item=item->next) {
      if(item->kind==ck_designator) {
        if(item->variant.designator.is_generic || item->variant.designator.is_field_designator ||
           !item->next || item->next->kind==ck_designator)
          reject("array initializer designator");
        auto index=item->variant.designator.variant.array_element;
        if(index>=count)reject("array initializer designator bounds");
        cursor=index*stride;item=item->next;
      }
      auto part=item;unsigned long long repetitions=1;
      if(item->kind==ck_init_repeat) {
        part=item->variant.init_repeat.constant;repetitions=item->variant.init_repeat.count;
        if(!repetitions || dynamic_constant(part))reject("effectful/empty repeated array initializer");
      }
      a_type_ptr item_type=element;unsigned long long width=stride;
      // Like c_gen_be::dump_initializer_part, a repeated constant can cover
      // leaf elements across multidimensional array rows rather than whole
      // elements at this level. Preserve the count at its actual type depth.
      if(item->kind==ck_init_repeat && part->type &&
         skip_typerefs(part->type)->size!=element->size) {
        item_type=array_leaf(element);width=1;
        if(skip_typerefs(part->type)->size!=item_type->size)
          reject("array repetition representation mismatch");
      }
      if(cursor>total || repetitions>(total-cursor)/width || cursor%width)
        reject("array initializer bounds/alignment");
      for(unsigned long long repeat=0;repeat<repetitions;++repeat) {
        array_initializer(parent,ordinal,item_type,part,offset+cursor,position);cursor+=width;
      }
    }
  }
  unsigned initialize_array(a_variable_ptr variable,a_constant_ptr value,const a_source_position &position) {
    unsigned id=node("array-init",0,position),ordinal=0;
    edge(id,"target",0,node("binding",type_id(variable->type),position,0,binding_ids.at(variable),"","","place","none"));
    array_initializer(id,ordinal,variable->type,value,0,position);return id;
  }
  struct initializer_step {
    a_field_ptr field;
    a_type_ptr type;
    unsigned long long index;
  };
  using initializer_path=std::vector<initializer_step>;
  unsigned record_place(a_variable_ptr variable,const initializer_path &path,const a_source_position &pos) {
    unsigned target=node("binding",type_id(variable->type),pos,0,binding_ids.at(variable),"","","place","none");
    for(const auto &step:path) {
      if(step.field) {
        unsigned member=node("member",type_id(step.type),pos,0,0,"","","place","none",field_ids.at(step.field));
        edge(member,"operand",0,target);target=member;
      } else {
        unsigned pointer=pointer_type(type_id(step.type));
        unsigned decay=node("array-decay",pointer,pos,0,0,"","","value","none");edge(decay,"operand",0,target);
        unsigned index=node("constant",9,pos,0,0,"",std::to_string(step.index),"value","none");
        unsigned add=node("binary",pointer,pos,0,0,"pointer-add","","value","left-to-right");
        edge(add,"left",0,decay);edge(add,"right",0,index);
        target=node("dereference",type_id(step.type),pos,0,0,"","","place","none");edge(target,"operand",0,add);
      }
    }
    return target;
  }
  void record_store(unsigned block,unsigned &ordinal,a_variable_ptr variable,
                    const initializer_path &path,unsigned value,const a_source_position &pos) {
    auto type=path.empty()?variable->type:path.back().type;
    unsigned assign=node("assign",type_id(type),pos,0,0,"","","value","right-to-left");
    edge(assign,"target",0,record_place(variable,path,pos));edge(assign,"value",0,value);
    unsigned statement=node("expr-stmt",0,pos);edge(statement,"value",0,assign);
    edge(block,"statement",ordinal++,statement);
  }
  void record_initializer(unsigned block,unsigned &ordinal,a_variable_ptr variable,a_type_ptr type,
                          a_constant_ptr value,initializer_path &path,const a_source_position &pos) {
    type=skip_typerefs(type);
    if(value && value->kind==ck_dynamic_init) {
      auto d=value->variant.dynamic_init.ptr;if(d->destructor)reject("record member destruction");
      if(d->kind==dik_constant || d->kind==dik_nonconstant_aggregate)value=d->variant.constant.ptr;
      else if(d->kind==dik_zero)value=nullptr;
      else if(d->kind==dik_expression && type->kind!=tk_array && copyable_value(type)) {
        record_store(block,ordinal,variable,path,expression(d->variant.expression),pos);return;
      } else reject("record member dynamic initializer",d->kind);
    }
    if(type->kind==tk_array) {
      // The complete local was zeroed first. Keep compact array-init payloads
      // for scalar arrays reached exclusively through named record fields.
      bool indexed=false;for(const auto &step:path)if(!step.field)indexed=true;
      auto leaf=array_leaf(type);
      if(!indexed && leaf->kind!=tk_struct && leaf->kind!=tk_union && leaf->kind!=tk_class) {
        unsigned id=node("array-init",0,pos),element=0;
        edge(id,"target",0,record_place(variable,path,pos));
        array_initializer(id,element,type,value,0,pos);edge(block,"statement",ordinal++,id);return;
      }
      if(!value)return;
      auto element=skip_typerefs(type->variant.array.element_type);
      auto stride=leaf_count(element),total=leaf_count(type);
      if(value->kind==ck_string) {
        if(element->kind!=tk_integer || element->size!=1 || value->variant.string.length>total)
          reject("nested string initializer outside bounded byte array");
        for(unsigned long long index=0;index<value->variant.string.length;++index) {
          unsigned char byte=(unsigned char)value->variant.string.value[index];
          int numeric=type_id(element)==4 && byte>=128?(int)byte-256:byte;
          path.push_back({nullptr,element,index});
          record_store(block,ordinal,variable,path,node("constant",type_id(element),pos,0,0,"",std::to_string(numeric),"value","none"),pos);
          path.pop_back();
        }
        return;
      }
      if(value->kind!=ck_aggregate)reject("record array initializer outside aggregate",value->kind);
      unsigned long long cursor=0;
      for(auto item=value->variant.aggregate.first_constant;item;item=item->next) {
        if(item->kind==ck_designator) {
          if(item->variant.designator.is_generic || item->variant.designator.is_field_designator ||
             !item->next || item->next->kind==ck_designator)reject("record array initializer designator");
          auto index=item->variant.designator.variant.array_element;
          if(index>=type->variant.array.variant.number_of_elements)reject("record array initializer designator bounds");
          cursor=index*stride;item=item->next;
        }
        auto part=item;unsigned long long repetitions=1;
        if(item->kind==ck_init_repeat) {
          part=item->variant.init_repeat.constant;repetitions=item->variant.init_repeat.count;
          if(!repetitions || dynamic_constant(part))reject("effectful/empty repeated record array initializer");
        }
        a_type_ptr item_type=element;unsigned long long width=stride;
        if(item->kind==ck_init_repeat && part->type && skip_typerefs(part->type)->size!=element->size) {
          item_type=leaf;width=1;
          if(skip_typerefs(part->type)->size!=leaf->size)reject("record array repetition representation mismatch");
        }
        if(cursor>total || repetitions>(total-cursor)/width || cursor%width)reject("record array initializer bounds/alignment");
        for(unsigned long long repeat=0;repeat<repetitions;++repeat) {
          size_t depth=path.size();auto current=type;auto offset=cursor;
          do {
            auto child=skip_typerefs(current->variant.array.element_type);auto child_width=leaf_count(child);
            path.push_back({nullptr,child,offset/child_width});offset%=child_width;current=child;
          } while(current->kind==tk_array && current!=item_type);
          if(current!=item_type || offset)reject("record array initializer element depth");
          record_initializer(block,ordinal,variable,item_type,part,path,pos);path.resize(depth);cursor+=width;
        }
      }
      return;
    }
    if(type->kind!=tk_struct && type->kind!=tk_union && type->kind!=tk_class) {
      if(value)record_store(block,ordinal,variable,path,constant_node(value,pos),pos);
      return;
    }
    // Match c_gen_be::dump_initializer_part: unnamed bit fields and empty
    // subobjects consume no initializer. A union initializes one active field.
    if(!value)return;
    if(value->kind!=ck_aggregate)reject("record initializer outside aggregate",value->kind);
    auto field=next_non_empty_initializable_field(type->variant.class_struct_union.field_list);
    for(auto item=value->variant.aggregate.first_constant;item;item=item->next) {
      if(item->kind==ck_designator) {
        if(item->variant.designator.is_generic || !item->variant.designator.is_field_designator ||
           !item->next || item->next->kind==ck_designator)reject("record initializer designator");
        field=item->variant.designator.variant.field;item=item->next;
      }
      if(!field || !field_ids.count(field) || parent_class_of(field)!=type)reject("record initializer field");
      path.push_back({field,field->type,0});record_initializer(block,ordinal,variable,field->type,item,path,pos);path.pop_back();
      field=type->kind==tk_union?nullptr:next_non_empty_initializable_field(field->next);
    }
  }
  bool contains_volatile_storage(a_type_ptr type) {
    if(get_type_qualifiers(type)&TQ_VOLATILE)return true;
    auto t=skip_typerefs(type);
    if(t->kind==tk_array)return contains_volatile_storage(t->variant.array.element_type);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
      if(t->variant.class_struct_union.any_volatile_member)return true;
      for(auto field=t->variant.class_struct_union.field_list;field;field=field->next)
        if(contains_volatile_storage(field->type))return true;
    }
    return false;
  }
  unsigned initialize_aggregate(a_variable_ptr variable,a_constant_ptr value,const a_source_position &pos) {
    // Source volatile accesses already have explicit volatile-place nodes.
    // Bulk default/array initialization cannot substitute for the observable
    // stores required by volatile subobjects; reject until those are owned.
    if(contains_volatile_storage(variable->type))reject("automatic aggregate initialization with volatile subobjects");
    auto type=skip_typerefs(variable->type),leaf=array_leaf(type);
    if(type->kind==tk_array && leaf->kind!=tk_struct && leaf->kind!=tk_class && leaf->kind!=tk_union)
      return initialize_array(variable,value,pos);
    unsigned block=node("block",0,pos),ordinal=0;initializer_path path;
    if(type->kind==tk_array) {
      unsigned zero=node("array-init",0,pos);edge(zero,"target",0,record_place(variable,path,pos));edge(block,"statement",ordinal++,zero);
    } else record_store(block,ordinal,variable,path,zero_node(type_id(type),pos),pos);
    record_initializer(block,ordinal,variable,type,value,path,pos);return block;
  }
  void loop_body(unsigned id,a_statement_ptr s,a_statement_ptr body) {
    // The stock C backend emits these compiler-generated targets as gotos.
    // Recover only EDG's marked break/continue targets, using the same label
    // mapping as the existing dot backend. User labels/gotos remain unsupported.
    auto labels=source_context(s,body,true);
    if(!labels.break_target) {
      // lower_c99_for_statement puts declaration initialization and the loop
      // in a synthetic block, retaining the break label after that block.
      // Unlike C++ lower_for_statement, it does not move the label inside.
      // Follow only empty tails of those wrappers, never another construct.
      auto tail=s;
      while(!tail->next && tail->parent && tail->parent->kind==stmk_block &&
            !tail->parent->position.seq)tail=tail->parent;
      if(tail->next && tail->next->kind==stmk_label && tail->next->variant.label.ptr->break_label)
        labels.break_target=tail->next->variant.label.ptr;
    }
    for(auto label:{labels.break_target,labels.continue_target})if(label) {
      if(label->source_corresp.name || label->switch_break_label)reject("nonloop control target");
      control_labels.insert(label);
    }
    controls.push_back({id,labels});
    edge(id,"body",0,statement(body));
    controls.pop_back();
  }
  bool c99_cleanup_initializer_wrapper(a_statement_ptr s,unsigned block) {
    // lower_c99_for_statement::turn_statement_into_block wraps a declaration
    // initializer plus the loop in a scopeless block. Its *parent* still owns
    // the original for-init scope, variables, and trailing break label. Keep
    // that lexical ownership when normalizing an activated cleanup; the extra
    // lowering wrapper does not introduce another object lifetime.
    if(!C_mode() || s->kind!=stmk_block || s->position.seq ||
       s->variant.block.extra_info->assoc_scope || s->variant.block.extra_info->lifetime)
      return false;
    bool own_cleanup=false;
    auto child=s->variant.block.statements;
    if(!child)return false;
    for(;child->next;child=child->next) {
      if(child->kind!=stmk_decl && child->kind!=stmk_init && child->kind!=stmk_empty)
        return false;
      if(child->kind==stmk_decl)for(auto e=child->variant.decl.entities;e;e=e->next) {
        if(e->entity.kind!=iek_variable)continue;
        auto v=reinterpret_cast<a_variable_ptr>(e->entity.ptr);
        if(cleanup_variables.count(v) && binding_scopes.at(v)==block)own_cleanup=true;
      }
    }
    return own_cleanup && child->kind==stmk_for && child->lowering_generated &&
           child->variant.for_loop.extra_info->initialization==nullptr;
  }
  bool implicit_return(a_statement_ptr statement) {
    // Match c_gen_be.c::is_implicit_return exactly: lowering's terminal
    // valueless marker is not a source return and must not become one.
    if(statement->kind!=stmk_return || statement->expr || statement->position.seq || statement->next)return false;
    auto terminal=function_block;
    while(terminal && terminal->kind==stmk_block) {
      terminal=terminal->variant.block.statements;
      if(!terminal)break;
      while(terminal->next)terminal=terminal->next;
    }
    return terminal==statement;
  }
  void append_block_statement(a_statement_ptr child,unsigned block,unsigned &ordinal,bool switch_body) {
    if(c99_cleanup_initializer_wrapper(child,block)) {
      for(auto nested=child->variant.block.statements;nested;nested=nested->next)
        append_block_statement(nested,block,ordinal,switch_body);
      return;
    }
    if(child->kind==stmk_decl) {
      unsigned prior=ordinal;declaration_cleanups(child,block,ordinal);
      if(switch_body && prior!=ordinal)reject("cleanup activation directly in switch body");
      return;
    }
    if(child->kind==stmk_empty || implicit_return(child))return;
    if(child->kind==stmk_label && control_labels.count(child->variant.label.ptr))return;
    edge(block,"statement",ordinal++,statement(child));
    if(child->kind==stmk_init && cleanup_variables.count(child->variant.dynamic_init->variable)) {
      if(switch_body)reject("cleanup activation directly in switch body");
      auto v=child->variant.dynamic_init->variable;
      edge(block,"statement",ordinal++,register_cleanup(v,block,child->position));
    }
  }
  unsigned statement(a_statement_ptr s,a_scope_ptr root_scope=nullptr) {
    if(!s)reject("missing statement");
    diagnostic_scope diagnostic(s->position);
    switch(s->kind) {
      case stmk_block: {
        unsigned id=node("block",0,s->position),ordinal=0;
        auto scope=root_scope?root_scope:s->variant.block.extra_info->assoc_scope;
        check_lifetime(s->variant.block.extra_info->lifetime);
        if(root_scope) {
          unsigned p=0;
          for(auto v=root_scope->variant.routine.parameters;v;v=v->next) {
            declared_bindings.insert(binding(v,id,true,p++));
          }
        }
        scope_bindings(scope,id,ordinal);
        bool switch_body=false;
        for(auto child=s->variant.block.statements;child;child=child->next)
          if(child->kind==stmk_switch_case)switch_body=true;
        for(auto child=s->variant.block.statements;child;child=child->next)
          append_block_statement(child,id,ordinal,switch_body);
        return id;
      }
      case stmk_return: {
        if(s->variant.return_dynamic_init)reject("return dynamic initialization");
        unsigned id=node("return",0,s->position);if(s->expr)edge(id,"value",0,expression(s->expr));return id;
      }
      case stmk_expr: {
        if(!s->expr)reject("empty expression statement");
        unsigned id=node("expr-stmt",0,s->position);edge(id,"value",0,expression(s->expr));return id;
      }
      case stmk_if: {
        unsigned id=node("if",0,s->position);edge(id,"condition",0,expression(s->expr));
        edge(id,"then",0,statement(s->variant.if_stmt.then_statement));
        if(s->variant.if_stmt.else_statement)edge(id,"else",0,statement(s->variant.if_stmt.else_statement));
        return id;
      }
      case stmk_while: case stmk_end_test_while: {
        unsigned id=node(s->kind==stmk_while?"while":"do-while",0,s->position);
        edge(id,"condition",0,expression(s->expr));
        loop_body(id,s,s->variant.loop_statement);return id;
      }
      case stmk_for: {
        unsigned id=node("for",0,s->position);
        auto f=s->variant.for_loop.extra_info;
        // As in c_gen_be, lowering moves declaration initializers into the
        // enclosing scope. The retained for initializer is one expression.
        if(f->initialization) {
          auto init=f->initialization;
          if(init->kind!=stmk_expr || init->next)reject("nonexpression lowered for initializer");
          edge(id,"initializer",0,expression(init->expr));
        }
        if(s->expr)edge(id,"condition",0,expression(s->expr));
        if(f->increment)edge(id,"step",0,expression(f->increment));
        loop_body(id,s,s->variant.for_loop.statement);return id;
      }
      case stmk_switch: {
        if(integer128(s->expr->type))reject("integer128 switch selector");
        auto body=s->variant.switch_stmt.body_statement;
        if(!body || body->kind!=stmk_block)reject("switch body outside block profile");
        auto descriptor=s->variant.switch_stmt.extra_info;
        unsigned id=node("switch",0,s->position);
        // Match c_gen_be::dump_switch_case: each descriptor owns its actual
        // lexical statement. C permits cases inside nested blocks/loops, so
        // the immediate body list need not match descriptor source order.
        // Keep lexical scopes; the managed emitter owns required dispatch.
        std::set<a_switch_case_entry_ptr> seen;
        for(auto entry=descriptor->cases;entry;entry=entry->next) {
          auto child=entry->stmt;
          if(!seen.insert(entry).second || !child || child->kind!=stmk_switch_case ||
             child->variant.switch_case.switch_statement!=s || child->variant.switch_case.extra_info!=entry ||
             switch_cases.count(child))reject("switch case descriptor ownership");
#if GNU_EXTENSIONS_ALLOWED
          if(entry->range_end)reject("GNU switch case range");
#endif
          if(!entry->case_value && descriptor->default_case!=entry)reject("switch default descriptor");
          switch_cases[child]=id;
        }
        edge(id,"condition",0,expression(s->expr));
        auto labels=source_context(s,body,false);
        if(labels.break_target) {
          if(labels.break_target->source_corresp.name || !labels.break_target->switch_break_label)
            reject("invalid switch break target");
          control_labels.insert(labels.break_target);
        }
        controls.push_back({id,labels});
        edge(id,"body",0,statement(body));
        controls.pop_back();return id;
      }
      case stmk_switch_case: {
        if(!switch_cases.count(s))reject("switch case outside owning switch");
        unsigned id=node("case",0,s->position);
        edge(id,"target",0,switch_cases.at(s));
        auto c=s->variant.switch_case.extra_info;
        if(c->case_value)edge(id,"value",0,constant_node(c->case_value,c->position));
        return id;
      }
      case stmk_goto: {
        auto target=s->variant.label.ptr;
        unsigned owner=0;string kind;
        if(!target->source_corresp.name && !controls.empty()) {
          // Keep original structured exits wherever EDG identifies them.
          if(target==controls.back().labels.break_target) {
            owner=controls.back().node;kind="break";
          } else for(auto it=controls.rbegin();it!=controls.rend();++it) {
            if(!it->labels.loop)continue;
            if(target==it->labels.continue_target){owner=it->node;kind="continue";}
            break;
          }
        }
        if(!owner){kind="goto";owner=label_id(target);}
        unsigned id=node(kind,0,s->position);edge(id,"target",0,owner);return id;
      }
      case stmk_label: {
        auto label=s->variant.label.ptr;
        if(control_labels.count(label) && !source_labels.count(label))return node("block",0,s->position);
        if(!emitted_source_labels.insert(label).second)reject("duplicate source label");
        unsigned id=label_id(label);
        string name=source_spelling(&label->source_corresp);if(name.empty())name="__label"+number(id);
        return node("label",0,s->position,0,0,"",name,"","",0,0,id);
      }
      case stmk_empty:return node("block",0,s->position);
      case stmk_init: {
        auto d=s->variant.dynamic_init;
        if(d->destructor)reject("initialization with destructor");
        if(!d->variable || !binding_ids.count(d->variable))reject("initialization without local binding");
        unsigned ty=type_id(d->variable->type);
        auto initialized_type=skip_typerefs(d->variable->type);
        bool initialized_record=initialized_type->kind==tk_struct || initialized_type->kind==tk_class || initialized_type->kind==tk_union;
        if(initialized_record && d->kind==dik_expression && !copyable_value(d->variable->type))reject("record initialization copy with array/volatile storage");
        if(initialized_type->kind==tk_array || (initialized_record && d->kind!=dik_expression)) {
          if(d->kind==dik_constant || d->kind==dik_nonconstant_aggregate)
            return initialize_aggregate(d->variable,d->variant.constant.ptr,s->position);
          if(d->kind==dik_zero)return initialize_aggregate(d->variable,nullptr,s->position);
          reject("array dynamic initialization",d->kind);
        }
        unsigned value=0;
        if(d->kind==dik_expression)value=expression(d->variant.expression);
        else if(d->kind==dik_constant)value=constant_node(d->variant.constant.ptr,s->position);
        else if(d->kind==dik_zero)value=zero_node(ty,s->position);
        else reject("dynamic initializer",d->kind);
        unsigned assign=node("assign",ty,s->position,0,0,"","","value","right-to-left");
        edge(assign,"target",0,local_place(d->variable,s->position));edge(assign,"value",0,value);
        unsigned id=node("expr-stmt",0,s->position);edge(id,"value",0,assign);return id;
      }
      default:reject("statement",s->kind);
    }
  }
  void run() {
    if(targ_sizeof_pointer!=8 || !targ_little_endian)reject("non64-bit/little-endian target");
    // EDG stores float/double constants in host format. Its configured target
    // and host representation must agree with the managed binary32/64 ABI.
    if(!TARG_HAS_IEEE_FLOATING_POINT || sizeof(float)!=4 || sizeof(double)!=8 ||
       targ_sizeof_float!=4 || targ_sizeof_double!=8 ||
       targ_flt_mant_dig!=24 || targ_flt_min_exp!=-125 || targ_flt_max_exp!=128 ||
       targ_dbl_mant_dig!=53 || targ_dbl_min_exp!=-1021 || targ_dbl_max_exp!=1024)
      reject("target outside IEEE binary32/binary64 profile");
    source_files(il_header.primary_source_file,0);
    capture_macros();
    auto file_scope=il_header.primary_scope;
    if(file_scope->dynamic_inits || file_scope->asm_entries)reject("global dynamic initialization/asm");
    collect_storage(file_scope);
#if GNU_EXTENSIONS_ALLOWED
    unsigned startup_ordinal=0;
#endif
    for(auto r=file_scope->routines;r;r=r->next) {
      if(ignore_routine_in_back_end(r))continue;
      if(r->function_def_number==NULL_function_def_number)continue;
#if GNU_EXTENSIONS_ALLOWED
      // Follow stock C dump_scope_routines order, not reachability or first
      // declaration position. Equal-priority ties are defined by this order.
      if(r->is_initialization_routine)startup_action(r,startup_ordinal++);
#endif
      bool root=r->source_corresp.name_linkage==nlk_external || r->source_corresp.name_linkage==nlk_cplusplus_external;
      bool coalescible=r->is_inline || (r->is_template_function && !r->is_specialized);
#if DO_IL_LOWERING && IA64_ABI
      coalescible=coalescible || r->use_comdat;
#endif
      // Like c_gen_be::dump_routine_decl, parsed-only header definitions
      // do not force emitted bodies. Actual uses recursively register them.
      if(coalescible)root=false;
      if(r->explicit_instantiation && !r->explicit_do_not_instantiate)root=true;
#if GNU_EXTENSIONS_ALLOWED
      root=root || r->has_gnu_used_attribute || r->is_initialization_routine || r->is_finalization_routine;
#endif
      if(root)routine(r);
    }
    for(size_t i=0;i<routines.size();++i) {
      auto r=routines[i];if(r->function_def_number==NULL_function_def_number)continue;
      function=routine_ids.at(r);auto scope=scope_for_routine_or_null(r);
      string label="dotobj function "+source_spelling(&r->source_corresp);context=label.c_str();
      binding_ids.clear();binding_scopes.clear();cleanup_variables.clear();registered_cleanups.clear();
      bound_scopes.clear();declared_bindings.clear();controls.clear();switch_cases.clear();control_labels.clear();source_labels.clear();emitted_source_labels.clear();
      check_lifetime(scope->lifetime);
      if(!scope->assoc_block || scope->assoc_block->kind!=stmk_block)reject("nonblock function root");
      function_block=scope->assoc_block;
      unsigned root=statement(scope->assoc_block,scope);
      if(source_labels.size()!=emitted_source_labels.size())reject("goto target missing from function body");
      if(!source_labels.empty() && !cleanup_variables.empty())reject("goto with lexical cleanup obligations");
      if(cleanup_variables!=registered_cleanups)reject("missing cleanup declaration/initialization completion anchor");
      functions.push_back("{\"entityId\":"+number(function)+",\"bodyNodeId\":"+number(root)+"}");
      context="file scope";
    }
    for(const auto &storage_bytes:bitfield_data) {
      string bytes;unsigned long long begin=0,previous=0;
      for(const auto &byte:storage_bytes.second) {
        if(!bytes.empty() && byte.first!=previous+1){data_chunk(storage_bytes.first,begin,bytes);bytes.clear();}
        if(bytes.empty())begin=byte.first;
        bytes+=char(byte.second);previous=byte.first;
      }
      data_chunk(storage_bytes.first,begin,bytes);
    }
    for(const auto &storage:storage_chunks)for(const auto &chunk:storage.second)
      storage_data.push_back("{\"storageId\":"+number(storage.first)+",\"byteOffset\":"+std::to_string(chunk.first)+",\"dataHex\":"+quote(bytes_hex(chunk.second))+"}");
    std::vector<string> types;
    const char *primitive_kinds[]={"void","i32","u8","i8","i16","u16","u32","i64","u64","f32","f64"};
    for(unsigned i=0;i<11;++i)types.push_back("{\"id\":"+number(i+1)+",\"kind\":"+quote(primitive_kinds[i])+",\"pointeeTypeId\":null,\"elementTypeId\":null,\"elementCount\":null}");
    types.insert(types.end(),execution_types.begin(),execution_types.end());
    string result="{\"formatVersion\":1,\"profile\":\"scalar-control-v16\",\"sourceFiles\":"+array(files)+
      ",\"macroDefinitions\":"+array(macro_definitions)+",\"macroParameters\":"+array(macro_parameters)+
      ",\"macroEvents\":"+array(macro_events)+
      ",\"sourceTypes\":"+array(source_types)+",\"sourceDeclarations\":"+array(source_declarations)+
      ",\"sourceFunctions\":"+array(source_functions)+",\"sourceParameters\":"+array(source_parameters)+
      ",\"sourceRecords\":"+array(source_records)+",\"sourceFields\":"+array(source_fields)+
      ",\"recordTypes\":"+array(record_types)+",\"recordFields\":"+array(record_fields)+
      ",\"storages\":"+array(storages)+",\"storageData\":"+array(storage_data)+",\"relocations\":"+array(relocations)+
      ",\"routineTypes\":"+array(routine_types)+",\"routineParameters\":"+array(routine_parameters)+
      ",\"sourceRoutineTypes\":"+array(source_routine_types)+",\"sourceRoutineParameters\":"+array(source_routine_parameters)+
      ",\"initializationActions\":"+array(initialization_actions)+
      ",\"types\":"+array(types)+",\"entities\":"+array(entities)+
      ",\"parameters\":"+array(parameters)+",\"functions\":"+array(functions)+",\"bindings\":"+array(bindings)+",\"nodes\":"+array(nodes)+",\"edges\":"+array(edges)+"}\n";
    if(fwrite(result.data(),1,result.size(),stdout)!=result.size() || fflush(stdout))reject("wire output failure");
  }
};
void emit(){Writer writer;writer.run();}
}
