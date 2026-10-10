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
  unsupported((string("dotobj scalar-v40: ")+what).c_str(),kind);
}
}
namespace dot_object {
struct Writer {
  unsigned next_entity=1,next_binding=1,next_node=1,next_file=1,next_source_type=1,next_source_declaration=1;
  unsigned function=0,next_execution_type=12,va_list_id=0,f80_id=0,i128_id=0,u128_id=0;
  a_statement_ptr function_block=nullptr;
  unsigned owned_function_root=0;
  a_routine_ptr current_routine=nullptr;
  struct PendingCleanup {
    unsigned state_id=0,action_id=0;
    unsigned long long next=0;
    a_routine_ptr callback=nullptr;
    a_dynamic_init_ptr original=nullptr;
    const dot_object_source::CleanupSnapshot *observed=nullptr;
    unsigned long long array_count=0,array_element_size=0;
    bool allocation_release=false,sized_release=false,guard_abort=false,static_cleanup=false;
  };
  unsigned next_cleanup_state=1,next_cleanup_action=1;
  std::map<unsigned long long,PendingCleanup> active_cleanup;
  std::set<a_dynamic_init_ptr> active_source_obligations;
  std::map<a_dynamic_init_ptr,const dot_object_source::CleanupSnapshot*> active_payload_subobjects;
  std::map<an_expr_node_ptr,a_dynamic_init_ptr> active_normal_calls;
  std::set<a_dynamic_init_ptr> active_normal_obligations,emitted_normal_obligations;
  std::vector<string> normal_destructions,cleanup_arrays,cleanup_vtt_arguments,array_runtime_operations,destruction_elisions;
  std::map<an_expr_node_ptr,std::pair<a_dynamic_init_ptr,const dot_object_source::ArrayRuntimeSnapshot*>> active_array_runtime;
  std::set<a_dynamic_init_ptr> active_array_obligations,emitted_array_obligations,covered_source_obligations,required_source_obligations;
  an_expr_node_ptr active_cleanup_prologue=nullptr;
  bool active_expression_lifetimes=false;
  std::set<a_variable_ptr> cleanup_metadata;
  std::set<a_routine_ptr> emitted_cleanup_sources;
  std::set<an_object_lifetime_ptr> emitted_source_lifetimes;
  std::set<a_dynamic_init_ptr> emitted_source_obligations;
  std::map<an_object_lifetime_ptr,unsigned> source_lifetime_ids;
  std::map<a_dynamic_init_ptr,unsigned> source_obligation_ids;
  std::vector<string> source_lifetimes,source_destruction_obligations,cleanup_actions,cleanup_states,cleanup_transitions,cleanup_frame_markers;
  std::vector<string> expression_lifetimes,cleanup_conditions;
  std::vector<string> exception_types,exception_base_conversions,throw_expressions,throw_object_addresses,throw_markers,rethrow_expressions;
  std::vector<string> throw_initialization_boundaries,throw_cleanup_transitions;
  std::vector<string> internal_try_expressions;
  std::vector<string> source_allocation_obligations;
  std::vector<string> allocation_cleanup_actions,local_static_guards,guard_operations,guard_cleanup_actions;
  std::map<a_variable_ptr,unsigned> local_static_guard_ids;
  std::set<a_dynamic_init_ptr> active_guard_cleanups;
  std::map<an_expr_node_ptr,a_dynamic_init_ptr> active_allocation_internal_tries;
  std::set<a_dynamic_init_ptr> active_allocation_obligations,emitted_allocation_obligations;
  std::vector<string> cleanup_payload_targets;
  std::vector<string> throw_payload_subobjects,throw_payload_paths;
  std::map<an_expr_node_ptr,unsigned> active_throw_ids;
  std::map<a_try_supplement_ptr,unsigned> active_try_ids;
  std::map<a_handler_ptr,unsigned> active_handler_ids;
  a_handler_ptr emitting_handler=nullptr;
  std::vector<string> source_tries,source_handlers,handler_markers,caught_object_addresses;
  std::map<unsigned,unsigned> exception_type_ids;
  std::map<unsigned,a_routine_ptr> exception_source_destructors;
  std::map<a_routine_ptr,a_routine_ptr> complete_exception_destructors;
  bool exception_destructors_indexed=false;
  an_expr_node_ptr active_throw_origin=nullptr;
  unsigned active_throw_node=0;
  struct NodeShape {unsigned type=0;bool place=false;size_t json_index=0;};
  std::vector<NodeShape> node_shapes;
  std::map<a_routine_ptr,unsigned> routine_ids;
  std::map<a_variable_ptr,unsigned> binding_ids;
  std::map<a_variable_ptr,unsigned> binding_scopes;
  std::set<a_variable_ptr> cleanup_variables,registered_cleanups;
  std::map<a_source_file_ptr,unsigned> file_ids;
  std::vector<a_routine_ptr> routines;
  std::vector<string> files,entities,parameters,functions,bindings,nodes,edges;
  std::vector<string> source_types,source_declarations,source_functions,source_parameters;
  std::vector<string> source_type_qualifiers,source_enum_types,source_enumerators,source_member_pointer_types;
  std::vector<string> source_routine_abis,source_routine_abi_parameters;
  struct AbiSlot {string role;unsigned source_ordinal,type;};
  std::map<a_routine_ptr,std::vector<AbiSlot>> routine_abi_slots;
  std::vector<string> macro_definitions,macro_parameters,macro_events;
  // Action order is independent of executable reachability/entity numbering.
  std::vector<string> initialization_actions;
  std::vector<string> generated_routine_origins,dynamic_initializers,dynamic_initializer_statements,generated_routine_scaffolds;
  std::vector<string> source_initializers,source_static_lifetimes,source_static_destructions,initializer_lifetime_executions;
  std::vector<string> static_destruction_registrations,generated_static_destructors,static_destruction_dispositions;
  std::map<a_dynamic_init_ptr,unsigned> source_initializer_ids,static_obligation_ids;
  std::map<an_object_lifetime_ptr,unsigned> static_lifetime_ids;
  std::set<a_dynamic_init_ptr> registered_static_obligations,emitted_static_cleanup_obligations;
  unsigned source_initializer_id(a_dynamic_init_ptr original) {
    if(!original || !dot_object_source::state().source_initializers.count(original))
      reject("static source initializer lacks original capture");
    return source_initializer_ids.emplace(original,unsigned(source_initializer_ids.size()+1)).first->second;
  }
  bool source_initializer_emitted(a_dynamic_init_ptr original) {
    const auto &source=dot_object_source::state().source_initializers.at(original);
    return !ignore_variable_in_back_end(source.variable) && !source.variable->suppress_inline_definition;
  }
  unsigned static_obligation_id(a_dynamic_init_ptr original) {
    if(!original || !dot_object_source::state().static_destructions.count(original))
      reject("static source obligation lacks original capture");
    return static_obligation_ids.emplace(original,unsigned(static_obligation_ids.size()+1)).first->second;
  }
  unsigned static_lifetime_id(an_object_lifetime_ptr original) {
    if(!original || !dot_object_source::state().static_lifetimes.count(original))
      reject("static source lifetime lacks original capture");
    return static_lifetime_ids.emplace(original,unsigned(static_lifetime_ids.size()+1)).first->second;
  }
  std::map<a_routine_ptr,unsigned> generated_scaffold_ordinals;
  std::set<unsigned> generated_root_nodes,owned_generated_root_nodes;
  std::map<a_routine_ptr,unsigned> initialization_action_ids;
  std::map<a_statement_ptr,unsigned> initializer_statement_nodes;
  std::set<a_statement_ptr> initializer_statement_anchors;
  std::vector<string> memory_barriers,memory_barrier_operands,memory_barrier_dimensions;
  std::map<unsigned,unsigned> source_type_ids;
  std::vector<string> source_dependent_types;
  std::map<unsigned,unsigned> pointer_types;
  std::map<std::pair<unsigned,std::pair<unsigned long long,bool>>,unsigned> array_types;
  std::vector<string> execution_types,record_types,record_fields;
  std::vector<string> source_records,source_fields,source_bases,record_base_subobjects,abi_routine_origins;
  std::vector<string> source_record_owners;
  std::vector<string> thunk_wrappers,wrapper_calls;
  std::vector<string> compile_time_selections;
  std::vector<string> base_subobject_expressions,virtual_base_subobjects,virtual_base_expressions,source_record_operations,record_value_transfers,record_storage_initializations,record_constant_initializations;
  std::map<unsigned,unsigned> binding_source_declarations;
  std::map<an_expr_node_ptr,unsigned> emitted_expression_ids;
  std::vector<string> virtual_base_adjustments;
  std::map<a_base_class_ptr,unsigned> source_base_ids;
  unsigned next_source_base=1;
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
  std::vector<string> deferred_routine_abis;
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
  string original_record_identity(a_type_ptr type,unsigned depth=0) {
    if(depth>128)reject("original record ownership cycle/depth");
    auto &captured=dot_object_source::state();
    auto found=captured.records.find(type);
    if(found==captured.records.end())reject("original record owner lacks source snapshot");
    const auto &source=found->second;
    auto owner=captured.record_owners.find(type);
    if(owner!=captured.record_owners.end()) {
      if(!source.name.empty() || !owner->second.owner || owner->second.owner==type || !owner->second.field)
        reject("invalid original anonymous member ownership");
      auto identity=original_record_identity(owner->second.owner,depth+1);
      return "record-member:"+std::to_string(identity.size())+":"+identity+":field:"+std::to_string(owner->second.ordinal);
    }
    string identity=source.identity;
    if(source.template_identity || (!C_mode() && identity.find("local-record:")==0)) {
      // A genuine finalized name retains namespace/template/function ownership.
      // Anonymous member C names were excluded in favor of their owner above.
      auto finalized=captured.finalized_record_identities.find(type);
      auto local=captured.owned_local_record_identities.find(type);
      if(finalized==captured.finalized_record_identities.end() && local==captured.owned_local_record_identities.end()) {
        const auto &actual=type->source_corresp;
        reject((string("template/local C++ record lacks finalized frontend identity: ")+source.identity+
          " actual-name="+(actual.name?actual.name:"<null>")+
          " mangled="+(actual.name_has_been_mangled?"true":"false")+
          " pending="+(actual.final_name_mangling_pending?"true":"false")+
          " template="+(source.template_identity?"true":"false")).c_str());
      }
      identity=finalized!=captured.finalized_record_identities.end()?
        "record-abi:"+finalized->second:"record-local-owner:"+local->second;
    }
    if(identity.find("local-record:")==0) {
      a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
      conv_seq_to_file_and_line(source.position.seq,&display,&full,&line,&at_end);
      identity+=":"+string(full?full:display?display:"")+":"+std::to_string((unsigned long long)line)+":"+
        std::to_string(source.position.column)+":occurrence:"+std::to_string(source.source_occurrence);
    }
    return identity;
  }
  unsigned record_type(a_type_ptr t) {
    auto prior=record_ids.find(t);if(prior!=record_ids.end())return prior->second;
    auto &captured=dot_object_source::state();
    auto view=captured.observed_subobjects.find(t);
    bool subobject=view!=captured.observed_subobjects.end();
    a_type_ptr complete=subobject?view->second:t;
    unsigned reserved=0,complete_id=0;
    if(subobject){reserved=next_execution_type++;record_ids[t]=reserved;complete_id=record_type(complete);}
    bool abi=!subobject && captured.abi_records.count(t)!=0;
    auto found=captured.records.find(complete);
    if(!abi && found==captured.records.end())reject("incomplete or uncaptured record definition");
    auto pre=captured.prelower_records.find(complete);
    const dot_object_source::Record *source=abi?nullptr:&found->second;
    if(source && !source->error.empty())reject((source->identity+": "+source->error).c_str());
    if(source && source->has_bases && pre==captured.prelower_records.end())reject("base layout lacks prelowering provenance");
    if(source && source->has_virtual_functions && pre==captured.prelower_records.end())
      reject("virtual record lacks an observed prelowering boundary");
    if(pre!=captured.prelower_records.end() && !pre->second.error.empty())reject(pre->second.error.c_str());
    if(source && pre!=captured.prelower_records.end()) {
      if(source->fields.size()!=pre->second.fields.size())reject("source field snapshot disagrees with prelowering boundary");
      for(size_t i=0;i<source->fields.size();++i) {
        const auto &field=source->fields[i];const auto &observed=pre->second.fields[i];
        if(field.il!=observed.il || field.original_name!=observed.name || field.source_type!=observed.source_type_snapshot ||
           field.offset!=observed.byte_offset || field.is_bit_field!=observed.is_bit_field || field.bit_width!=observed.bit_width ||
           field.has_initializer!=observed.has_initializer)
          reject("original field identity/type differs at prelowering boundary");
      }
    }
    if(source && !subobject && ((!t->incomplete && t->size!=source->size) || (!t->incomplete && t->alignment!=source->alignment)))
      reject("record complete-object extent changed during lowering");
    if(!t->incomplete && (!t->size || t->size>2147483647ULL || !t->alignment || t->alignment>16))
      reject("record native extent/alignment outside execution profile");
    string identity,name,layout=t->kind==tk_union?"union":t->kind==tk_class?"class":"struct";
    a_source_position pos={};
    if(abi) {
      auto key=captured.abi_record_keys.find(t);
      if(key==captured.abi_record_keys.end())reject((string("unclassified compiler ABI record creation: ")+
        source_spelling(&t->source_corresp)+" fields="+[&](){string r;for(auto f=t->variant.class_struct_union.field_list;f;f=f->next)r+=source_spelling(&f->source_corresp)+",";return r;}()).c_str());
      identity=key->second;name=source_spelling(&t->source_corresp);
    } else {
      identity=original_record_identity(complete);name=source->name;layout=source->layout_kind;pos=source->position;
    }
    if(subobject)identity+="/subobject";
    unsigned id=reserved?reserved:next_execution_type++;record_ids[t]=id;
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
      for(const auto &field:source->fields)originals[field.il]=&field;
      if(subobject)source_record=source_record_ids.at(complete);
      else {
      source_record=next_source_record++;source_record_ids[t]=source_record;
      // Reserve every original member/base identity before following any
      // source type. A field can point to a derived class whose physical base
      // view re-enters this record while its source rows are still being
      // emitted (for example basic_ios/basic_ostream). That view must use the
      // very same original descriptors, independent of traversal order.
      for(const auto &field:source->fields)
        source_field_ids.emplace(field.il,next_source_field++);
      if(pre!=captured.prelower_records.end())for(const auto &base:pre->second.bases)
        source_base_ids.emplace(base.il,next_source_base++);
      auto owner=captured.record_owners.find(t);
      if(owner!=captured.record_owners.end()) {
        // A child may be the first exported type (e.g. decltype(obj.member)).
        // Its IDs are already reserved before walking the owner back to it.
        record_type(owner->second.owner);
        if(!source_record_ids.count(owner->second.owner) || !source_field_ids.count(owner->second.field))
          reject("anonymous record owner lacks original record/member IDs");
        source_record_owners.push_back("{\"sourceRecordId\":"+number(source_record)+
          ",\"ownerSourceRecordId\":"+number(source_record_ids.at(owner->second.owner))+
          ",\"ownerFieldId\":"+number(source_field_ids.at(owner->second.field))+
          ",\"kind\":\"anonymous-member\",\"captureProof\":\"original-member-declaration\"}");
      }
      auto source_type_it=captured.type_ids.find(t);
      if(source_type_it==captured.type_ids.end())reject("record source type was not captured before lowering");
      source_records.push_back("{\"id\":"+number(source_record)+",\"sourceTypeId\":"+number(source_type(source_type_it->second))+
        ",\"identityKey\":"+quote(identity)+",\"sourceName\":"+quote(name)+",\"recordKind\":"+quote(layout)+
        ",\"isComplete\":"+(t->incomplete?"false":"true")+",\"nativeSize\":"+std::to_string(source->size)+
        ",\"nativeAlignment\":"+std::to_string(source->alignment)+",\"isTriviallyCopyable\":"+
        (t->incomplete?"null":trivial?"true":"false")+",\"capturePhase\":"+
        quote(pre==captured.prelower_records.end()?"before-lowering":"before-class-prelowering")+position(pos)+
        ",\"isPacked\":"+(source->is_packed?"true":"false")+",\"maxMemberAlignment\":"+std::to_string(source->max_member_alignment)+
        ",\"explicitAlignment\":"+std::to_string(source->explicit_alignment)+"}");
      if(!C_mode() && !t->incomplete) {
        if(pre==captured.prelower_records.end() || !pre->second.operations_known)
          reject("complete source class lacks observed copy-operation policies");
        const auto &operations=pre->second;
        source_record_operations.push_back("{\"sourceRecordId\":"+number(source_record)+
          ",\"bitwiseConstructionAllowed\":"+(operations.bitwise_construction?"true":"false")+
          ",\"bitwiseAssignmentAllowed\":"+(operations.bitwise_assignment?"true":"false")+
          ",\"requiresReturnStorage\":"+(operations.requires_return_storage?"true":"false")+
          ",\"requiresParameterStorage\":"+(operations.requires_parameter_storage?"true":"false")+
          ",\"captureProof\":\"before-class-prelowering\"}");
      }
      unsigned source_ordinal=0;
      for(const auto &field:source->fields) {
        unsigned original_id=source_field_ids.at(field.il);originals[field.il]=&field;
        source_fields.push_back("{\"id\":"+number(original_id)+",\"sourceRecordId\":"+number(source_record)+
          ",\"ordinal\":"+std::to_string(source_ordinal++)+",\"sourceTypeId\":"+number(source_type(field.source_type))+
          ",\"sourceName\":"+quote(field.original_name)+
          ",\"isMutable\":"+(field.is_mutable?"true":"false")+",\"hasNoUniqueAddress\":"+
          (field.no_unique_address?"true":"false")+",\"bitWidth\":"+
          (field.is_bit_field?std::to_string(field.bit_width):"null")+position(field.position)+",\"hasInitializer\":"+(field.has_initializer?"true":"false")+
          ",\"isPacked\":"+(field.is_packed?"true":"false")+",\"explicitAlignment\":"+std::to_string(field.explicit_alignment)+"}");
      }
      if(pre!=captured.prelower_records.end()) {
        unsigned base_ordinal=0;
        for(const auto &base:pre->second.bases) {
          unsigned base_id=source_base_ids.at(base.il);
          source_bases.push_back("{\"id\":"+number(base_id)+",\"sourceRecordId\":"+number(source_record)+
            ",\"ordinal\":"+std::to_string(base_ordinal++)+",\"baseSourceTypeId\":"+number(source_type(base.source_type_snapshot))+
            ",\"accessKind\":"+quote(base.access)+",\"isVirtual\":"+(base.is_virtual?"true":"false")+position(base.position)+"}");
        }
      }
      }
    }
    record_types.push_back("{\"typeId\":"+number(id)+",\"identityKey\":"+quote(identity)+",\"sourceName\":"+quote(name)+
      ",\"size\":"+std::to_string(t->incomplete?0:t->size)+",\"alignment\":"+std::to_string(t->incomplete?0:t->alignment)+
      position(pos)+",\"layoutKind\":"+quote(layout)+",\"sourceRecordId\":"+number(source_record)+
      ",\"representationKind\":"+quote(subobject?"subobject":abi?"abi":"complete")+",\"valueCopyKind\":"+quote(trivial && !subobject?"ordinary":"explicit-only")+
      ",\"captureProof\":"+quote(subobject?"observed-subobject-creation":abi?"observed-abi-creation":pre==captured.prelower_records.end()?"source-snapshot":"observed-class-lowering")+",\"completeTypeId\":"+number(complete_id)+"}");
    unsigned ordinal=0;std::set<string> names;
    for(auto f=t->variant.class_struct_union.field_list;f;f=f->next) {
      if(f->is_bit_field && !f->bit_size)continue;
      auto child=skip_typerefs(f->type);
      if(f->is_bit_field && integer128(child))reject("integer128 bitfield representation");
      auto original_field=f;
      while(captured.copied_fields.count(original_field))original_field=captured.copied_fields.at(original_field);
      auto original=originals.find(original_field);
      auto base_field=captured.generated_base_fields.find(f);
      string role,field_name;unsigned source_type_id=0,source_field=0,source_base=0;a_source_position field_pos={};
      if(original!=originals.end()) {
        const auto &field=*original->second;role="source";field_name=field.name;field_pos=field.position;
        source_type_id=source_type(field.source_type);source_field=source_field_ids.at(original_field);
        if(f->offset!=field.offset)reject("original field offset changed during lowering");
      } else if(base_field!=captured.generated_base_fields.end()) {
        if(base_field->second->is_virtual) {
          bool observed=false;
          if(pre!=captured.prelower_records.end())for(const auto &base:pre->second.virtual_bases)
            if(base.il==base_field->second)observed=true;
          if(!observed)reject("physical virtual base lacks exact prelower placement");
          role="virtual-base";
        } else {
          auto mapped=source_base_ids.find(base_field->second);
          if(mapped==source_base_ids.end())reject((string("physical base lacks direct original declaration: record=")+identity+
            " base="+source_spelling(&base_field->second->type->source_corresp)+
            " direct="+(base_field->second->direct?"true":"false")+
            " subobject="+(subobject?"true":"false")+
            " source-bases="+std::to_string(pre==captured.prelower_records.end()?0:pre->second.bases.size())).c_str());
          source_base=mapped->second;role="base";
        }
        field_name=source_spelling(&f->source_corresp);
      } else if(abi) {
        role=captured.erased_code_pointer_types.count(child)?"abi-code-address":"abi";
        field_name=source_spelling(&f->source_corresp);
      }
      else {
        auto generated=captured.generated_field_roles.find(f);
        if(generated==captured.generated_field_roles.end() || generated->second!="vptr")reject("unclassified lowered physical field");
        role="vptr";field_name="__edg_vptr";
      }
      if(field_name.empty())field_name="__edg_abi_field_"+std::to_string(ordinal);
      bool flexible=child->kind==tk_array && array_type_has_no_bound(child);
      bool zero_array=child->kind==tk_array && !child->size && complete_fixed_array(child);
      if(flexible && (f->next || layout=="union"))reject("flexible array outside final struct/class member");
      // A GNU fixed [0] member contributes alignment and a named address but
      // no occupied bytes. It is not the [] flexible tail, and may be interior
      // or a union member. Never add a dummy physical element to its extent.
      if(!names.insert(field_name).second || (!child->size && !flexible && !zero_array) || !child->alignment)
        reject("record physical field identity/extent");
      unsigned long long extent=f->is_bit_field?(f->offset_bit_remainder+f->bit_size+7)/8:child->size;
      if(f->offset>t->size || child->alignment>16 || extent>t->size-f->offset)reject("record field outside containing storage");
      if(layout=="union" && f->offset)reject("union field nonzero storage offset");
      string observed_alignment="null";
      if(role=="source" && !f->is_bit_field) {
        auto alignment=captured.observed_field_alignments.find(original_field);
        if(alignment==captured.observed_field_alignments.end() || !alignment->second ||
           (alignment->second&(alignment->second-1)))reject("source field lacks observed physical layout alignment");
        observed_alignment=std::to_string(alignment->second);
      }
      unsigned field_id=next_field++;field_ids[f]=field_id;
      record_fields.push_back("{\"id\":"+number(field_id)+",\"recordTypeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"typeId\":"+number(type_id(f->type))+",\"sourceName\":"+quote(field_name)+",\"byteOffset\":"+std::to_string(f->offset)+
        ",\"sourceTypeId\":"+number(source_type_id)+position(field_pos)+
        ",\"bitOffset\":"+(f->is_bit_field?std::to_string(f->offset_bit_remainder):"null")+
        ",\"bitWidth\":"+(f->is_bit_field?std::to_string(f->bit_size):"null")+
        ",\"bitSigned\":"+(f->is_bit_field?(f->bit_field_is_signed?"true":"false"):"null")+
        ",\"sourceFieldId\":"+number(source_field)+",\"fieldRole\":"+quote(role)+",\"sourceBaseId\":"+number(source_base)+",\"observedAlignment\":"+observed_alignment+"}");
    }
    if(source && pre!=captured.prelower_records.end())for(const auto &base:pre->second.bases) {
      if(base.is_virtual)continue;
      a_field_ptr physical=nullptr;
      for(auto f=t->variant.class_struct_union.field_list;f;f=f->next) {
        auto entry=captured.generated_base_fields.find(f);
        if(entry!=captured.generated_base_fields.end() && entry->second==base.il) {
          if(physical)reject("duplicate physical direct base");
          physical=f;
        }
      }
      if(!physical && !base.il->is_optimized_empty_base)reject("missing physical nonempty direct base");
      if(physical && physical->offset!=base.il->offset)reject("base placement differs from original descriptor");
      unsigned base_complete=record_type(skip_typerefs(base.il->type));
      record_base_subobjects.push_back("{\"recordTypeId\":"+number(id)+",\"sourceBaseId\":"+number(source_base_ids.at(base.il))+
        ",\"completeBaseTypeId\":"+number(base_complete)+",\"physicalViewTypeId\":"+number(physical?type_id(physical->type):0)+
        ",\"byteOffset\":"+std::to_string(base.il->offset)+",\"extent\":"+std::to_string(physical?skip_typerefs(physical->type)->size:0)+
        ",\"fieldId\":"+number(physical?field_ids.at(physical):0)+"}");
    }
    if(source && pre!=captured.prelower_records.end())for(const auto &base:pre->second.virtual_bases) {
      a_field_ptr physical=nullptr;
      for(auto field=t->variant.class_struct_union.field_list;field;field=field->next) {
        auto entry=captured.generated_base_fields.find(field);
        if(entry!=captured.generated_base_fields.end() && entry->second==base.il) {
          if(physical)reject("duplicate physical virtual base");
          physical=field;
        }
      }
      // Nonprimary virtual bases live only in the complete object. A primary
      // virtual base can be retained in the subobject's physical field list.
      if(subobject && !physical)continue;
      auto base_type=skip_typerefs(base.il->type);
      auto partner=base_type->variant.class_struct_union.extra_info->subobject_partner;
      auto view_type=physical?skip_typerefs(physical->type):partner?partner:base_type;
      if(!physical && !base.il->is_optimized_empty_base && !base.il->shares_virtual_function_info)
        reject("virtual base placement lacks physical/empty/shared proof");
      if(physical && physical->offset!=base.il->offset)reject("virtual base placement differs from observed descriptor");
      auto extent=base.il->is_optimized_empty_base?0:view_type->size;
      if(base.il->offset>t->size || extent>t->size-base.il->offset)reject("virtual base placement exceeds containing extent");
      virtual_base_subobjects.push_back("{\"recordTypeId\":"+number(id)+",\"completeBaseTypeId\":"+number(record_type(base_type))+
        ",\"physicalViewTypeId\":"+number(extent?type_id(view_type):0)+",\"byteOffset\":"+std::to_string(base.il->offset)+
        ",\"extent\":"+std::to_string(extent)+",\"fieldId\":"+number(physical?field_ids.at(physical):0)+
        ",\"captureProof\":\"observed-virtual-base-placement\"}");
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
    auto type=skip_typerefs(value->type);
    if(type->kind==tk_array && !type->size)reject("zero-sized aggregate literal storage needs an owned allocation contract");
    unsigned id=next_storage++;literal_ids[value]=id;
    storages.push_back("{\"id\":"+number(id)+",\"sourceName\":\"\",\"linkage\":\"internal\",\"externalKey\":null,\"definition\":\"strong\",\"typeId\":"+number(type_id(type))+
      ",\"sourceTypeId\":null,\"isThreadLocal\":false,\"isConst\":true,\"alignment\":"+std::to_string(type->alignment)+position(pos)+"}");
    initialize_storage(id,type,value,0);return id;
  }
  void initialize_storage(unsigned id,a_type_ptr type,a_constant_ptr c,unsigned long long offset) {
    auto t=skip_typerefs(type);if(!c)return;
    if(t->kind==tk_array && !t->size && complete_fixed_array(t)) {
      if(!zero_constant_bits(c))reject("effectful/nonempty zero-sized array initializer");
      return;
    }
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
    // Match c_gen_be::dump_variable_decl: a COMDAT group is an explicit
    // definition property, not a guess based on inline spelling or names.
    bool coalescible=false;
#if DO_IL_LOWERING
    coalescible=v->comdat_group!=nullptr;
    if(coalescible && (declaration || (v->storage_class!=sc_unspecified && !v->is_inline)))
      reject("COMDAT storage lacks the stock C definition contract");
#endif
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
      ",\"definition\":"+quote(declaration?"declaration":coalescible?"linkonce":"strong")+",\"typeId\":"+number(type_id(t))+
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
      if(ignore_variable_in_back_end(init->variable) || cleanup_metadata.count(init->variable))continue;
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
      if(ignore_variable_in_back_end(v) || cleanup_metadata.count(v) || v->storage_class==sc_extern || v->suppress_inline_definition)continue;
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
  bool complete_fixed_array(a_type_ptr type) {
    auto t=skip_typerefs(type);
    if(t->kind!=tk_array || t->variant.array.is_variable_size_array ||
       t->variant.array.is_vla || t->variant.array.is_template_dependent_size_array ||
       array_type_has_no_bound(t))return false;
    auto element=skip_typerefs(t->variant.array.element_type);
    return element->kind==tk_array?complete_fixed_array(element):element->size!=0;
  }
  unsigned array_type(unsigned element,unsigned long long count,bool bound_known=true) {
    if(!bound_known && count)reject("unknown array bound has a fixed element count");
    auto key=std::make_pair(element,std::make_pair(count,bound_known));
    auto prior=array_types.find(key);if(prior!=array_types.end())return prior->second;
    if(count>2147483647ULL)reject("array count outside bounded fixed storage profile");
    unsigned id=next_execution_type++;array_types[key]=id;
    execution_types.push_back("{\"id\":"+number(id)+",\"kind\":\"array\",\"pointeeTypeId\":null,\"elementTypeId\":"+
      number(element)+",\"elementCount\":"+(bound_known?std::to_string(count):"null")+"}");
    return id;
  }
  unsigned type_id(a_type_ptr type) {
    unsigned q=get_type_qualifiers(type);
    if(q & TQ_C11_ATOMIC)reject("atomic type");
    if(q & ~(TQ_CONST|TQ_VOLATILE|TQ_RESTRICT|TQ_NULLABILITY))
      reject("extension qualifier has no owned execution/storage contract");
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
      if(dot_object_source::state().erased_code_pointer_types.count(t)) {
        // Stock make_vptp_type creates an erased ABI address, not a callable
        // source signature. C lowering casts it to the true member/virtual
        // routine type before calling; do not invent void() call permission.
        auto target=skip_typerefs(t->variant.pointer.type);
        if(target->kind!=tk_routine || target->variant.routine.extra_info->param_type_list ||
           target->variant.routine.extra_info->has_ellipsis ||
           skip_typerefs(target->variant.routine.return_type)->kind!=tk_void)
          reject("observed erased code pointer has unexpected stock shape");
        return pointer_type(1);
      }
      return pointer_type(type_id(t->variant.pointer.type));
    }
    if(t->kind==tk_array) {
      if(t->variant.array.is_variable_size_array || t->variant.array.is_vla ||
         t->variant.array.is_template_dependent_size_array || t->size>2147483647ULL)
        reject("array outside bounded fixed storage profile");
      auto element=skip_typerefs(t->variant.array.element_type);

      auto count=t->variant.array.variant.number_of_elements;
      bool bound_known=count!=0 || t->variant.array.bound_is_zero;
      if(count>2147483647ULL || (!element->size && !complete_fixed_array(element)) ||
         t->size!=count*element->size)
        reject("array size/stride mismatch");
      return array_type(type_id(t->variant.array.element_type),count,bound_known);
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
  const dot_object_source::RoutineAbi &source_abi(unsigned captured) {
    const auto &state=dot_object_source::state();
    auto found=state.routine_abis.find(captured);
    if(found==state.routine_abis.end())reject((string("source routine has no observed calling-method ABI: type=")+std::to_string(captured)+
      " object="+std::to_string(state.types.at(captured-1).object_type)+" params="+std::to_string(state.types.at(captured-1).parameters.size())).c_str());
    const auto &abi=found->second;
    const auto &source=state.types.at(captured-1);
    if(!abi.error.empty())reject(abi.error.c_str());
    if(source.kind!="routine" || abi.indirect_parameters.size()!=source.parameters.size())
      reject("source routine ABI original parameter count mismatch");
    if(source.variadic && (abi.result_buffer ||
       std::find(abi.indirect_parameters.begin(),abi.indirect_parameters.end(),true)!=abi.indirect_parameters.end()))
      reject("variadic indirect-value/result-buffer ABI");
    return abi;
  }
  unsigned adjusted_source_parameter(unsigned parameter) {
    auto &types=dot_object_source::state().types;
    unsigned unqualified=parameter;
    while(types.at(unqualified-1).kind=="typedef" || types.at(unqualified-1).kind=="qualified")
      unqualified=types.at(unqualified-1).underlying;
    const auto &argument=types.at(unqualified-1);
    return argument.kind=="array"?pointer_type(source_execution_type(argument.underlying)):
      argument.kind=="routine"?pointer_type(source_execution_type(unqualified)):source_execution_type(parameter);
  }
  std::vector<AbiSlot> source_abi_slots(unsigned captured) {
    const auto &source=dot_object_source::state().types.at(captured-1);
    const auto &abi=source_abi(captured);
    std::vector<AbiSlot> result;
    auto result_slot=[&](){result.push_back({"result-buffer",0,pointer_type(source_execution_type(source.result))});};
    if(abi.result_buffer && (!source.object_type || !abi.result_follows_this))result_slot();
    if(source.object_type)result.push_back({"this",0,pointer_type(source_execution_type(source.object_type))});
    if(abi.vtt_type)result.push_back({"vtt",0,type_id(abi.vtt_type)});
    if(abi.result_buffer && source.object_type && abi.result_follows_this)result_slot();
    for(unsigned i=0;i<source.parameters.size();++i) {
      unsigned type=adjusted_source_parameter(source.parameters[i]);
      result.push_back({abi.indirect_parameters[i]?"indirect-source":"source",i,
        abi.indirect_parameters[i]?pointer_type(type):type});
    }
    return result;
  }
  unsigned source_execution_type(unsigned captured) {
    const auto &type=dot_object_source::state().types.at(captured-1);
    if(!type.error.empty())reject(type.error.c_str());
    if(type.kind=="record" && dot_object_source::state().abi_records.count(type.record_type))
      reject("compiler ABI record cannot masquerade as an original source type");
    if(type.is_atomic)reject("atomic source type has no supported execution representation");
    if(type.kind=="qualified" || type.kind=="typedef")return source_execution_type(type.underlying);
    if(type.execution)return type.execution;
    if(type.is_int128)return integer128_type(type.is_unsigned_int128);
    if(type.kind=="record")return record_type(type.record_type);
    if(type.kind=="va-list")return va_list_type();
    if(type.kind=="long-double" || type.kind=="float80" || type.kind=="float64x")return f80_type();
    if(type.kind=="routine") {
      std::vector<unsigned> args;
      for(const auto &slot:source_abi_slots(captured))args.push_back(slot.type);
      return signature_type(source_abi(captured).result_buffer?1:source_execution_type(type.result),args,type.variadic,type.exception_policy);
    }
    if(type.kind=="pointer" || type.kind=="lvalue-reference" || type.kind=="rvalue-reference")
      return pointer_type(source_execution_type(type.underlying));
    if(type.kind=="array")return array_type(source_execution_type(type.underlying),type.element_count,type.array_bound_known);
    if(type.kind=="nullptr" || type.kind=="member-pointer") {
      const auto &lowered=dot_object_source::state().lowered_source_types;
      auto found=lowered.find(type.original_type);
      if(found==lowered.end())reject("source type lacks observed lowered representation");
      if(type.kind=="nullptr" && (found->second->kind!=tk_pointer ||
         skip_typerefs(found->second->variant.pointer.type)->kind!=tk_void))
        reject("nullptr lowering differs from stock void-pointer representation");
      return type_id(found->second);
    }
    reject((string("source type has no supported execution representation: ")+type.kind).c_str());
  }
  bool source_execution_available(unsigned captured,std::set<unsigned> &active) {
    const auto &state=dot_object_source::state();const auto &type=state.types.at(captured-1);
    if(type.is_atomic)return false;
    if(type.kind=="qualified" || type.kind=="typedef")return source_execution_available(type.underlying,active);
    if(type.execution || type.is_int128 || type.kind=="record" || type.kind=="va-list" ||
       type.kind=="long-double" || type.kind=="float80" || type.kind=="float64x")return true;
    if(!active.insert(captured).second)return true;
    bool available=false;
    if(type.kind=="nullptr" || type.kind=="member-pointer")
      available=state.lowered_source_types.count(type.original_type)!=0;
    else if(type.kind=="pointer" || type.kind=="lvalue-reference" || type.kind=="rvalue-reference" ||
            type.kind=="array" || type.kind=="qualified" || type.kind=="typedef")
      available=source_execution_available(type.underlying,active);
    else if(type.kind=="routine") {
      auto abi=state.routine_abis.find(captured);
      available=abi!=state.routine_abis.end() && abi->second.error.empty() &&
        abi->second.indirect_parameters.size()==type.parameters.size() &&
        !(type.variadic && (abi->second.result_buffer ||
          std::find(abi->second.indirect_parameters.begin(),abi->second.indirect_parameters.end(),true)!=abi->second.indirect_parameters.end()));
      if(available)available=source_execution_available(type.result,active);
      if(available && type.object_type)available=source_execution_available(type.object_type,active);
      for(auto parameter:type.parameters)if(available)available=source_execution_available(parameter,active);
    }
    active.erase(captured);return available;
  }
  unsigned optional_source_execution_type(unsigned captured) {
    std::set<unsigned> active;
    return source_execution_available(captured,active)?source_execution_type(captured):0;
  }
  unsigned source_type(unsigned captured) {
    if(!captured)reject("missing pre-lowering scalar type");
    auto prior=source_type_ids.find(captured);if(prior!=source_type_ids.end())return prior->second;
    const auto &type=dot_object_source::state().types.at(captured-1);
    if(!type.error.empty())reject(type.error.c_str());
    if(type.kind.empty()) {
      a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
      conv_seq_to_file_and_line(type.position.seq,&display,&full,&line,&at_end);
      reject((string("source type family not yet represented in semantic schema: original-kind=")+
        std::to_string(type.original_type?(int)type.original_type->kind:-1)+" name="+type.name+
        " source="+(full?full:display?display:"")+":"+std::to_string(line)+":"+std::to_string(type.position.column)).c_str());
    }
    unsigned id=next_source_type++;source_type_ids[captured]=id;
    unsigned execution=optional_source_execution_type(captured);
    unsigned underlying=type.underlying?source_type(type.underlying):0;
    if(type.kind=="dependent")source_dependent_types.push_back("{\"typeId\":"+number(id)+
      ",\"kind\":"+quote(type.dependent_kind)+",\"ownerTypeId\":"+number(type.dependent_owner?source_type(type.dependent_owner):0)+
      ",\"nestingDepth\":"+(type.dependent_kind=="parameter"?std::to_string(type.dependent_depth):"null")+
      ",\"parameterPosition\":"+(type.dependent_kind=="parameter"?std::to_string(type.dependent_position):"null")+
      ",\"isPack\":"+(type.dependent_pack?"true":"false")+",\"identityKey\":"+quote(type.dependent_identity)+
      ",\"captureProof\":\"before-lowering\"}");
    if(type.kind=="routine") {
      if(execution) {
      const auto &abi=source_abi(captured);
      const auto &saved=dot_object_source::state();
      auto incomplete_value=[&](unsigned source) {
        auto value=&saved.types.at(source-1);
        while(value->kind=="qualified" || value->kind=="typedef")value=&saved.types.at(value->underlying-1);
        return value->kind=="record" && value->record_type && value->record_type->incomplete;
      };
      bool deferred=incomplete_value(type.result);
      for(auto parameter:type.parameters)deferred=deferred || incomplete_value(parameter);
      if(deferred) {
        if(abi.proof!="observed-routine-lowering")reject("deferred routine lacks original ABI lowering observation");
        deferred_routine_abis.push_back("{\"routineTypeId\":"+number(execution)+",\"sourceRoutineTypeId\":"+number(id)+
          ",\"reason\":\"incomplete-value-type\",\"captureProof\":\"observed-incomplete-routine-lowering\"}");
      }
      source_routine_abis.push_back("{\"sourceRoutineTypeId\":"+number(id)+",\"returnPassing\":"+
        quote(abi.result_buffer?"result-buffer":"direct")+",\"captureProof\":"+quote(abi.proof)+"}");
      unsigned execution_ordinal=0;
      for(const auto &slot:source_abi_slots(captured)) {
        source_routine_abi_parameters.push_back("{\"sourceRoutineTypeId\":"+number(id)+",\"executionOrdinal\":"+
          std::to_string(execution_ordinal++)+",\"parameterRole\":"+quote(slot.role)+",\"sourceOrdinal\":"+
          (slot.role=="source" || slot.role=="indirect-source"?std::to_string(slot.source_ordinal):"null")+"}");
      }
      }
      source_routine_types.push_back("{\"typeId\":"+number(id)+",\"returnTypeId\":"+number(source_type(type.result))+",\"variadic\":"+(type.variadic?"true":"false")+",\"exceptionPolicy\":"+quote(type.exception_policy)+",\"parameterMode\":"+quote(type.parameter_mode)+",\"objectTypeId\":"+number(type.object_type?source_type(type.object_type):0)+
        ",\"objectIsConst\":"+(type.object_is_const?"true":"false")+",\"objectIsVolatile\":"+(type.object_is_volatile?"true":"false")+
        ",\"objectRefQualifier\":"+quote(type.object_ref_qualifier)+"}");
      unsigned ordinal=0;for(auto parameter:type.parameters)source_routine_parameters.push_back("{\"typeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+",\"parameterTypeId\":"+number(source_type(parameter))+"}");
    }
    if(type.kind=="enum") {
      const auto &state=dot_object_source::state();const auto &enumeration=state.enums.at(type.original_type);
      if(!enumeration.error.empty())reject(enumeration.error.c_str());
      string identity=enumeration.identity;
      auto finalized=state.finalized_enum_identities.find(type.original_type);
      if(enumeration.needs_final_identity && finalized!=state.finalized_enum_identities.end())
        identity="enum-abi:"+finalized->second;
      else if(enumeration.needs_final_identity && identity.find("local-enum:")!=0)
        reject("template enum lacks finalized frontend identity");
      if(identity.find("local-enum:")==0) {
        a_const_char *display=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
        conv_seq_to_file_and_line(type.position.seq,&display,&full,&line,&at_end);
        identity+=":"+string(full?full:display?display:"")+":"+std::to_string((unsigned long long)line)+":"+
          std::to_string(type.position.column)+":occurrence:"+std::to_string(enumeration.occurrence);
        const char *unit=std::getenv("EDG_DOT_OBJECT_UNIT_ID");
        if(unit)identity+=":unit:"+std::to_string(std::string(unit).size())+":"+unit;
      }
      source_enum_types.push_back("{\"typeId\":"+number(id)+",\"identityKey\":"+quote(identity)+
        ",\"underlyingTypeId\":"+number(source_type(enumeration.underlying))+
        ",\"isScoped\":"+(enumeration.scoped?"true":"false")+",\"isComplete\":"+(enumeration.complete?"true":"false")+
        ",\"enumeratorListComplete\":"+(enumeration.list_complete?"true":"false")+
        ",\"hasExplicitBase\":"+(enumeration.explicit_base?"true":"false")+"}");
      unsigned ordinal=0;for(const auto &enumerator:enumeration.enumerators)
        source_enumerators.push_back("{\"typeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+
          ",\"sourceName\":"+quote(enumerator.name)+",\"valueText\":"+quote(enumerator.value)+position(enumerator.position)+"}");
    }
    if(type.kind=="member-pointer")
      source_member_pointer_types.push_back("{\"typeId\":"+number(id)+",\"ownerTypeId\":"+number(source_type(type.member_owner))+
        ",\"memberTypeId\":"+number(underlying)+",\"captureProof\":"+
        quote(execution?"observed-member-pointer-lowering":"observed-source-type")+"}");
    for(const auto &qualifier:type.qualifiers)
      source_type_qualifiers.push_back("{\"typeId\":"+number(id)+",\"name\":"+quote(qualifier)+"}");
    source_types.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(type.kind)+",\"sourceName\":"+quote(type.name)+
      ",\"underlyingTypeId\":"+number(underlying)+",\"executionTypeId\":"+number(execution)+
      ",\"isConst\":"+(type.is_const?"true":"false")+",\"isVolatile\":"+(type.is_volatile?"true":"false")+
      ",\"isRestrict\":"+(type.is_restrict?"true":"false")+",\"isAtomic\":"+(type.is_atomic?"true":"false")+
      ",\"elementCount\":"+(type.kind=="array" && type.array_bound_known?std::to_string(type.element_count):"null")+position(type.position)+"}");
    return id;
  }
  void source_declaration(const dot_object_source::Declaration &declaration,
                          const string &kind,unsigned entity,unsigned binding,unsigned owner) {
    unsigned id=next_source_declaration++;
    if(binding)binding_source_declarations.emplace(binding,id);
    source_declarations.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(kind)+",\"sourceName\":"+quote(declaration.name)+
      ",\"sourceTypeId\":"+number(source_type(declaration.type))+",\"entityId\":"+number(entity)+",\"bindingId\":"+number(binding)+
      ",\"functionEntityId\":"+number(owner)+position(declaration.position)+"}");
  }
  void source_function(a_routine_ptr r,unsigned entity) {
    auto found=dot_object_source::state().functions.find(r);
    if(found==dot_object_source::state().functions.end())reject((string("missing pre-lowering routine capture: ")+source_spelling(&r->source_corresp)).c_str());
    const auto &captured=found->second;
    if(captured.function_kind=="runtime" &&
       (!dot_object_source::state().runtime_declarations.count(r) || !r->compiler_generated ||
        r->function_def_number!=NULL_function_def_number ||
        (r->source_corresp.name_linkage!=nlk_external && r->source_corresp.name_linkage!=nlk_cplusplus_external)))
      reject("runtime declaration lacks exact generated external origin");
    source_declaration(captured.declaration,"function",entity,0,0);
    source_functions.push_back("{\"entityId\":"+number(entity)+",\"returnSourceTypeId\":"+number(source_type(captured.declaration.type))+
      ",\"capturePhase\":"+quote(captured.function_kind=="runtime"?"runtime-declaration":"before-lowering")+",\"routineSourceTypeId\":"+number(source_type(captured.signature_type))+",\"functionKind\":"+quote(captured.function_kind)+"}");
    if(captured.parameter_defaults.size()!=captured.parameters.size())reject("source default-argument snapshot arity mismatch");
    unsigned ordinal=0;
    for(auto parameter:captured.parameters) {
      unsigned parameter_ordinal=ordinal++;
      source_parameters.push_back("{\"entityId\":"+number(entity)+",\"ordinal\":"+std::to_string(parameter_ordinal)+",\"sourceTypeId\":"+number(source_type(parameter))+
        ",\"hasDefaultArgument\":"+(captured.parameter_defaults.at(parameter_ordinal)?"true":"false")+"}");
    }
  }
  unsigned node(const string &kind,unsigned ty,const a_source_position &pos,
                unsigned entity=0,unsigned binding=0,const string &op="",
                const string &value="",const string &category="",const string &order="",unsigned field=0,unsigned storage=0,unsigned reserved_id=0) {
    unsigned id=reserved_id?reserved_id:next_node++;
    if(node_shapes.size()<=id)node_shapes.resize(id+1);
    node_shapes[id].type=ty;node_shapes[id].place=category=="place";
    node_shapes[id].json_index=nodes.size();
    nodes.push_back("{\"id\":"+number(id)+",\"functionId\":"+number(function)+",\"kind\":"+quote(kind)+",\"typeId\":"+number(ty)+
      ",\"entityId\":"+number(entity)+",\"bindingId\":"+number(binding)+",\"operator\":"+(op.empty()?"null":quote(op))+
      ",\"constantText\":"+(value.empty()?"null":quote(value))+",\"valueCategory\":"+(category.empty()?"null":quote(category))+
      ",\"evaluationOrder\":"+(order.empty()?"null":quote(order))+position(pos)+",\"fieldId\":"+number(field)+",\"storageId\":"+number(storage)+"}");
    return id;
  }
  void edge(unsigned parent,const string &role,unsigned ordinal,unsigned child) {
    if(!child)reject("missing child node");
    if(current_routine && parent==owned_function_root && role=="statement" &&
       dot_object_source::state().generated_initializers.count(current_routine))
      generated_root_nodes.insert(child);
    edges.push_back("{\"parentId\":"+number(parent)+",\"role\":"+quote(role)+",\"ordinal\":"+std::to_string(ordinal)+",\"childId\":"+number(child)+"}");
  }
  bool copyable_value(a_type_ptr type,bool base_payload=false) {
    if(get_type_qualifiers(type)&(TQ_VOLATILE|TQ_C11_ATOMIC))return false;
    auto t=skip_typerefs(type);
    if(t->kind==tk_array)return complete_fixed_array(t) && copyable_value(t->variant.array.element_type);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
      if(t->incomplete || t->variant.class_struct_union.any_volatile_member)return false;
      auto &source=dot_object_source::state();
      if(source.abi_records.count(t) && !(base_payload && source.observed_subobjects.count(t)))return false;
      if(!C_mode() && !base_payload) {
        auto original=source.prelower_records.find(t);
        if(original==source.prelower_records.end() || !original->second.trivial_copy_known || !original->second.trivial_copy)return false;
      }
      for(auto field=t->variant.class_struct_union.field_list;field;field=field->next)
        if(!copyable_value(field->type,source.generated_base_fields.count(field)!=0))return false;
    }
    return true;
  }
  bool record_value_policy(a_type_ptr type,const string &kind,bool base_payload=false) {
    if(get_type_qualifiers(type)&(TQ_VOLATILE|TQ_C11_ATOMIC))return false;
    auto t=skip_typerefs(type);auto &captured=dot_object_source::state();
    if(t->kind==tk_array)return complete_fixed_array(t) && record_value_policy(t->variant.array.element_type,kind);
    if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
      if(t->incomplete || t->variant.class_struct_union.any_volatile_member)return false;
      if(captured.abi_records.count(t) && !(base_payload && captured.observed_subobjects.count(t)))return false;
      if(!C_mode() && !base_payload) {
        auto original=captured.prelower_records.find(t);
        if(original==captured.prelower_records.end() || !original->second.operations_known)return false;
        const auto &ops=original->second;
        if(kind=="temporary") {
          if(!ops.bitwise_construction && !ops.bitwise_assignment)return false;
        } else if(!ops.bitwise_construction || (kind=="transport" && ops.requires_return_storage) ||
                  (kind=="argument" && ops.requires_parameter_storage))return false;
      }
      for(auto field=t->variant.class_struct_union.field_list;field;field=field->next)
        if(!record_value_policy(field->type,kind,captured.generated_base_fields.count(field)!=0))return false;
    }
    return true;
  }
  unsigned record_transfer(a_type_ptr type,const a_source_position &pos,const string &order,const string &kind) {
    auto t=skip_typerefs(type);
    if((t->kind!=tk_struct && t->kind!=tk_class && t->kind!=tk_union) || !record_value_policy(type,kind))
      reject("record transfer lacks observed complete-object copy policy");
    unsigned id=node("record-transfer",type_id(type),pos,0,0,"","","value",order);
    record_value_transfers.push_back("{\"nodeId\":"+number(id)+",\"kind\":"+quote(kind)+
      ",\"captureProof\":"+quote(kind=="construction"?"observed-dynamic-init":"observed-temporary-assignment")+"}");
    return id;
  }
  unsigned long long cleanup_number(a_constant_ptr value) {
    if(!value || value->kind!=ck_integer)reject("noninteger cleanup metadata");
    a_boolean overflow=FALSE;auto result=unsigned_value_of_integer_constant(value,&overflow);
    if(overflow)reject("cleanup metadata integer overflow");
    return result;
  }
  std::vector<a_constant_ptr> cleanup_fields(a_constant_ptr value,unsigned count=0) {
    if(!value || value->kind!=ck_aggregate)reject("nonaggregate cleanup metadata");
    std::vector<a_constant_ptr> fields;
    for(auto part=value->variant.aggregate.first_constant;part;part=part->next)fields.push_back(part);
    if(count && fields.size()!=count)reject("cleanup metadata field count");
    return fields;
  }
  void find_cleanup_frames(a_statement_ptr statement,std::vector<an_expr_node_ptr> &frames) {
    if(!statement)return;
    if(statement->kind==stmk_expr && statement->expr && statement->expr->kind==enk_lowered_eh_construct &&
       statement->expr->variant.lowered_eh.kind==leck_function_prologue)frames.push_back(statement->expr);
    if(statement->kind==stmk_block)
      for(auto child=statement->variant.block.statements;child;child=child->next)find_cleanup_frames(child,frames);
  }
  a_constant_ptr cleanup_initializer(a_scope_ptr scope,a_variable_ptr variable) {
    if(!scope || !variable)return nullptr;
    for(auto entry=scope->local_static_variable_inits;entry;entry=entry->next)if(entry->variable==variable) {
      if(entry->init_kind!=initk_static)reject("dynamic cleanup metadata initializer");
      return entry->initializer.constant;
    }
    for(auto child=scope->scopes;child;child=child->next)
      if(auto value=cleanup_initializer(child,variable))return value;
    return nullptr;
  }
  void prepare_owned_cleanup(a_routine_ptr owner,a_scope_ptr scope) {
    active_try_ids.clear();active_handler_ids.clear();emitting_handler=nullptr;
    active_throw_ids.clear();
    active_cleanup.clear();active_source_obligations.clear();active_cleanup_prologue=nullptr;
    active_payload_subobjects.clear();
    active_expression_lifetimes=false;
    active_normal_calls.clear();active_normal_obligations.clear();emitted_normal_obligations.clear();
    active_array_runtime.clear();active_array_obligations.clear();emitted_array_obligations.clear();
    active_allocation_internal_tries.clear();active_allocation_obligations.clear();emitted_allocation_obligations.clear();
    active_guard_cleanups.clear();
    auto &captured=dot_object_source::state();
    for(const auto &entry:captured.allocation_internal_tries)if(entry.second.first==owner) {
      auto original=dot_object_source::source_destruction(entry.second.second,owner);
      if(!original || !captured.destructions.at(original).allocation_release)
        reject("internal allocation rollback lacks original source ownership");
      active_allocation_internal_tries.emplace(entry.first,original);
      active_allocation_obligations.insert(original);
    }
    for(const auto &observed:captured.payload_subobjects)if(observed.owner==owner) {
      auto original=dot_object_source::source_destruction(observed.original,owner);
      if(!observed.error.empty())reject(observed.error.c_str());
      if(!original || !active_payload_subobjects.emplace(original,&observed).second)
        reject("payload subobject lacks unique original destruction");
    }
    for(auto call:captured.normal_destruction_order[owner]) {
      const auto &observed=captured.normal_destruction_calls.at(call);
      auto source=dot_object_source::source_destruction(observed.original,owner);
      auto static_source=dot_object_source::original_static_destruction(observed.original);
      auto generated=captured.generated_static_destructors.find(owner);
      if(!source && generated!=captured.generated_static_destructors.end() && generated->second.original==static_source) {
        auto callback=dot_object_source::abi_primary(observed.callback);if(!callback)callback=observed.callback;
        if(!static_source || callback!=captured.static_destructions.at(static_source).selected_routine)
          reject("generated static destruction callback origin mismatch");
        active_normal_calls.emplace(call,static_source);continue;
      }
      if(!source)continue; // Unowned/cross-function origins never license coverage.
      auto callback=dot_object_source::abi_primary(observed.callback);if(!callback)callback=observed.callback;
      if(callback!=captured.destructions.at(source).destructor)reject("normal destruction callback origin mismatch");
      active_normal_calls.emplace(call,source);active_normal_obligations.insert(source);
    }
    for(auto call:captured.array_runtime_order[owner]) {
      const auto &observed=captured.array_runtime_calls.at(call);
      auto source=dot_object_source::source_destruction(observed.original,owner);
      auto static_source=dot_object_source::original_static_destruction(observed.original);
      auto generated=captured.generated_static_destructors.find(owner);
      if(!source && generated!=captured.generated_static_destructors.end() && generated->second.original==static_source) {
        auto callback=dot_object_source::abi_primary(observed.callback);if(!callback)callback=observed.callback;
        if(!static_source || callback!=captured.static_destructions.at(static_source).selected_routine || observed.operation!="destroy")
          reject("generated static array callback origin mismatch");
        active_array_runtime.emplace(call,std::make_pair(static_source,&observed));continue;
      }
      if(!source)continue;
      auto callback=dot_object_source::abi_primary(observed.callback);if(!callback)callback=observed.callback;
      if(callback!=captured.destructions.at(source).destructor)reject("array runtime callback origin mismatch");
      active_array_runtime.emplace(call,std::make_pair(source,&observed));active_array_obligations.insert(source);
    }
    std::vector<an_expr_node_ptr> frames;find_cleanup_frames(scope->assoc_block,frames);
    if(frames.size()>1)reject("multiple observed cleanup prologues");
    if(frames.empty())return;
    auto frame=frames[0]->variant.lowered_eh.variant.prologue_info;
    if(frame->routine!=owner)reject("cleanup prologue routine mismatch");
    active_cleanup_prologue=frames[0];
    std::vector<a_constant_ptr> array_entries;
    if(frame->array_table) {
      cleanup_metadata.insert(frame->array_table);
      auto array_initializer=cleanup_initializer(scope,frame->array_table);
      if(!array_initializer)reject("cleanup prologue lacks exact array initializer");
      array_entries=cleanup_fields(array_initializer);
    }
    if(!frame->region_table)return;
    cleanup_metadata.insert(frame->region_table);
    auto initializer=cleanup_initializer(scope,frame->region_table);
    if(!initializer)reject("cleanup prologue lacks exact region initializer");
    auto entries=cleanup_fields(initializer);
    for(unsigned raw=0;raw<entries.size();++raw) {
      auto entry=entries[raw];unsigned state_raw=raw;
      auto fields=cleanup_fields(entry,4);auto callback=fields[0];
      auto flags=cleanup_number(fields[3]);
      if(flags&~0xffULL || ((flags&0x11)==0x11) || ((flags&0x48)==0x48) ||
         ((flags&0x20) && ((flags&0x48)!=0x40)))
        reject("cleanup action outside fixed array/local/member/base/VTT flags");
      auto found=dot_object_source::state().cleanup_regions.find(entry);
      if(found==dot_object_source::state().cleanup_regions.end())reject("cleanup region lacks typed lowering observation");
      const auto &observed=found->second;
      if(observed.owner!=owner || !observed.error.empty())
        reject(observed.error.empty()?"cleanup region owner mismatch":observed.error.c_str());
      if(observed.guard_abort) {
        auto guard=captured.local_static_guard_cleanups.find(observed.original);
        if(flags!=0x80)reject("guard region has unexpected flags");
        // Stock add_region_table_entry casts zero to its generic function
        // pointer type. is_zero_constant deliberately excludes implicit casts.
        if(!is_null_pointer_value(callback))reject("guard region callback is not null");
        if(guard==captured.local_static_guard_cleanups.end())
          reject("guard region lacks observed local-static source rollback");
        if(guard->second.first!=owner)reject("guard region rollback owner mismatch");
        if(guard->second.second!=observed.variable)reject("guard region rollback storage mismatch");
        if(observed.indirect ||
           !observed.path.empty() || observed.array || observed.conditional_flag || observed.vtt_argument)
          reject("guard region lacks exact local-static storage rollback");
        PendingCleanup value;value.state_id=next_cleanup_state++;value.action_id=next_cleanup_action++;
        value.next=cleanup_number(fields[2]);value.observed=&observed;value.guard_abort=true;
        active_cleanup.emplace(state_raw,value);active_guard_cleanups.insert(observed.original);continue;
      }
      if(callback->kind!=ck_address || callback->variant.address.kind!=abk_routine || callback->variant.address.offset)
        reject("cleanup callback lacks exact routine address");
      if(observed.release_handler) {
        if(flags || cleanup_number(fields[1]))reject("handler release region has unexpected flags or handle");
        PendingCleanup value;value.state_id=next_cleanup_state++;
        value.next=cleanup_number(fields[2]);value.observed=&observed;
        active_cleanup.emplace(state_raw,value);continue;
      }
      if(bool(flags&0x40)!=bool(observed.base_subobject || observed.delegation))reject("cleanup region base-subobject proof mismatch");
      if(bool(flags&0x04)!=observed.allocation_release)reject("cleanup allocation proof mismatch");
      if(!observed.allocation_release && bool(flags&0x08)!=observed.array)reject("cleanup array proof mismatch");
      if(bool(flags&0x20)!=bool(observed.vtt_argument))reject("cleanup VTT proof mismatch");
      unsigned long long array_count=0,array_element_size=0;
      if(observed.allocation_release) {
        if(flags&~0x0fULL || !observed.indirect || !observed.variable || !observed.path.empty() || observed.array ||
           skip_typerefs(observed.variable->type)->kind!=tk_pointer)
          reject("allocation region outside owned scalar pointer rollback");
        if(flags&0x08) {
          auto index=cleanup_number(fields[1]);
          if(index>=array_entries.size())reject("sized allocation region lacks exact size supplement");
          auto array=cleanup_fields(array_entries[index],3);
          array_element_size=cleanup_number(array[1]);array_count=cleanup_number(array[2]);
          auto allocated=skip_typerefs(skip_typerefs(observed.variable->type)->variant.pointer.type);
          if(array_count!=0 || !array_element_size || array_element_size!=allocated->size)
            reject("sized allocation cleanup size differs from allocated object");
        }
      } else if(observed.array) {
        auto index=cleanup_number(fields[1]);
        if(index>=array_entries.size())reject("cleanup array table index outside exact table");
        auto array=cleanup_fields(array_entries[index],3);
        array_element_size=cleanup_number(array[1]);array_count=cleanup_number(array[2]);
        if(!observed.element_type || array_element_size!=skip_typerefs(observed.element_type)->size ||
           array_count!=observed.element_count || !array_count || !array_element_size || array_count>2147483647ULL)
          reject("cleanup array extent differs from observed typed position");
      }
      if(bool(flags&0x02)!=bool(observed.conditional_flag))reject("cleanup conditional flag proof mismatch");
      if(observed.conditional_flag) {
        // Stock C stores the condition in an adjacent supplementary region.
        // It has no independent cleanup state; preserve physical indices for
        // all actual state transitions and decode only this exact observation.
        if(raw+1>=entries.size())reject("conditional cleanup lacks flag supplement");
        auto supplement=cleanup_fields(entries[++raw],4);
        auto handle=supplement[1];auto flag=observed.conditional_flag;
        if(cleanup_number(supplement[0]) || cleanup_number(supplement[2])!=null_eh_region_number ||
           cleanup_number(supplement[3]) || handle->kind!=ck_stack_offset ||
           handle->variant.stack_offset.variable!=flag || handle->variant.stack_offset.offset)
          reject("conditional cleanup supplement differs from observed flag");
        if(var_has_static_or_thread_storage_duration(flag) || type_id(flag->type)!=2 ||
           !(flag->compiler_generated || flag->lowering_generated) ||
           dot_object_source::state().variables.count(flag))
          reject("conditional cleanup flag is not an observed synthetic automatic int");
      }
      if(observed.vtt_argument) {
        if(raw+1>=entries.size())reject("VTT cleanup lacks argument supplement");
        auto supplement=cleanup_fields(entries[++raw],4);auto handle=supplement[1];
        if(cleanup_number(supplement[0]) || cleanup_number(supplement[2])!=null_eh_region_number ||
           cleanup_number(supplement[3])!=1 || handle->kind!=ck_stack_offset ||
           handle->variant.stack_offset.variable!=observed.vtt_argument || handle->variant.stack_offset.offset ||
           skip_typerefs(observed.vtt_argument->type)->kind!=tk_pointer ||
           var_has_static_or_thread_storage_duration(observed.vtt_argument))
          reject("VTT cleanup supplement differs from observed automatic argument");
      }
      auto original=dot_object_source::source_destruction(observed.original,owner);
      bool static_cleanup=false;
      a_routine_ptr source_destructor=nullptr;
      bool source_allocation_release=false,source_array_allocation=false;
      if(!original) {
        original=dot_object_source::original_static_destruction(observed.original);
        if(!original)reject("cleanup region lacks original automatic or static destruction");
        const auto &source=captured.static_destructions.at(original);
        auto source_initializer_entry=captured.source_initializers.find(source.initializer);
        bool owner_matches=source_initializer_entry!=captured.source_initializers.end() &&
          (source_initializer_entry->second.owner?dot_object_source::abi_primary(owner)==source_initializer_entry->second.owner || owner==source_initializer_entry->second.owner:
           captured.dynamic_initializers.count(source.initializer) && captured.dynamic_initializers.at(source.initializer).routine==owner);
        if(!source.partial_aggregate || !source.error.empty() || !owner_matches || !source_initializer_emitted(source.initializer) ||
           observed.variable!=source_initializer_entry->second.variable || observed.indirect || observed.path.empty() ||
           observed.throw_origin || observed.allocation_release || observed.base_subobject || observed.delegation || observed.vtt_argument ||
           !observed.original->destruction_is_for_partially_constructed_aggregate)
          reject((string("static construction rollback correspondence: persistent=")+(source.persistent?"yes":"no")+
            " initializer="+(source.initializer?"yes":"no")+" owner="+(owner_matches?"yes":"no")+
            " storage="+(observed.variable==source.variable?"same":"different")+
            " source-variable="+(source.variable?source_spelling(&source.variable->source_corresp):"none")+
            " target-variable="+(observed.variable?source_spelling(&observed.variable->source_corresp):"none")+
            " path="+std::to_string(observed.path.size())+" partial="+(observed.original->destruction_is_for_partially_constructed_aggregate?"yes":"no")+
            " source-ordinal="+std::to_string(source.ordinal)+" source-error="+source.error).c_str());
        auto original_type=skip_typerefs(source_initializer_entry->second.variable->type);
        if(original_type->kind!=tk_array)reject("static construction rollback requires its original fixed array");
        for(const auto &part:observed.path)if(part.field || part.base)
          reject("static construction rollback outside fixed array element path");
        source_destructor=source.selected_routine;static_cleanup=true;
      } else {
        const auto &source=captured.destructions.at(original);
        if(!source.error.empty())reject(source.error.c_str());
        source_destructor=source.destructor;source_allocation_release=source.allocation_release;source_array_allocation=source.array_allocation;
      }
      auto selected=callback->variant.address.variant.routine;
      auto primary=dot_object_source::abi_primary(selected);
      if((primary?primary:selected)!=source_destructor)reject("cleanup callback differs from original destructor");
      auto signature=skip_typerefs(selected->type);
      if(signature->kind!=tk_routine)reject("cleanup callback is not routine");
      auto extra=signature->variant.routine.extra_info;
      auto parameter=extra->param_type_list;
      if(observed.allocation_release) {
        if(!source_allocation_release || source_array_allocation ||
           skip_typerefs(signature->variant.routine.return_type)->kind!=tk_void || !parameter || extra->has_ellipsis ||
           skip_typerefs(parameter->type)->kind!=tk_pointer ||
           skip_typerefs(skip_typerefs(parameter->type)->variant.pointer.type)->kind!=tk_void ||
           (bool(flags&0x08)?(!parameter->next || parameter->next->next || type_id(parameter->next->type)!=9):parameter->next!=nullptr))
          reject("allocation region differs from original unsized/sized delete ABI");
      } else if(selected->special_kind!=sfk_destructor || skip_typerefs(signature->variant.routine.return_type)->kind!=tk_void ||
         !parameter || extra->has_ellipsis || skip_typerefs(parameter->type)->kind!=tk_pointer ||
         (observed.vtt_argument?(!parameter->next || parameter->next->next ||
           type_id(parameter->next->type)!=type_id(observed.vtt_argument->type)):parameter->next!=nullptr))
        reject("cleanup callback outside observed receiver/VTT void destructor ABI");
      if(state_raw==null_eh_region_number)reject("cleanup state collides with frontend null sentinel");
      PendingCleanup value;value.state_id=next_cleanup_state++;value.action_id=next_cleanup_action++;
      value.next=cleanup_number(fields[2]);value.callback=selected;value.original=original;value.observed=&observed;
      value.array_count=array_count;value.array_element_size=array_element_size;
      value.static_cleanup=static_cleanup;value.allocation_release=observed.allocation_release;value.sized_release=observed.allocation_release && (flags&0x08);
      active_cleanup.emplace(state_raw,value);
      if(value.allocation_release)active_allocation_obligations.insert(original);
      else if(!value.static_cleanup)active_source_obligations.insert(original);
    }
    for(const auto &entry:active_cleanup) {
      std::set<unsigned long long> visited;auto state=entry.first;
      for(;;) {
        if(state==null_eh_region_number)break;
        if(!visited.insert(state).second)reject("cyclic cleanup state chain");
        auto next=active_cleanup.find(state);
        if(next==active_cleanup.end())reject("cleanup chain references unknown state");
        state=next->second.next;
      }
    }
  }
  void check_lifetime(an_object_lifetime_ptr lifetime) {
    if(!lifetime)return;
    for(auto dip=lifetime->destructions;dip;dip=dip->next_in_destruction_list) {
      if(dip->is_guard_var_for_local_static_var_init && active_guard_cleanups.count(dip))continue;
      if(dip->is_freeing_of_exception_object) {
        auto release=dot_object_source::state().handler_releases.find(dip);
        if(release!=dot_object_source::state().handler_releases.end() && release->second.first==current_routine)continue;
      }
      auto source=current_routine?dot_object_source::source_destruction(dip,current_routine):nullptr;
      if(!source) {
        auto original=dot_object_source::original_static_destruction(dip);
        auto &captured=dot_object_source::state();
        if(original && captured.static_destructions.at(original).partial_aggregate) {
          const auto &obligation=captured.static_destructions.at(original);
          auto initializer=captured.source_initializers.find(obligation.initializer);
          bool owner_matches=initializer!=captured.source_initializers.end() &&
            (initializer->second.owner?dot_object_source::abi_primary(current_routine)==initializer->second.owner || current_routine==initializer->second.owner:
             captured.dynamic_initializers.count(obligation.initializer) && captured.dynamic_initializers.at(obligation.initializer).routine==current_routine);
          bool active=false;
          for(const auto &entry:active_cleanup)if(entry.second.static_cleanup && entry.second.original==original)active=true;
          if(owner_matches && (active || captured.static_partial_terminal.count(original)))continue;
        }
      }
      if(source && dot_object_source::state().automatic_partial_terminal.count(std::make_pair(source,current_routine)))continue;
      if(!source || (!active_source_obligations.count(source) && !active_normal_obligations.count(source) && !active_payload_subobjects.count(source) && !active_array_obligations.count(source) && !active_allocation_obligations.count(source)))
        reject((string("destruction obligations lack owned cleanup action or normal call: ")+
          (dip->destructor?source_spelling(&dip->destructor->source_corresp):"no destructor")+
          (source?" (source captured)":" (source missing)")+" active-actions="+std::to_string(active_cleanup.size())+
          " active-obligations="+std::to_string(active_source_obligations.size())+
          " variable="+(dip->variable?source_spelling(&dip->variable->source_corresp):"none")+
          " lifetime-kind="+std::to_string(lifetime->kind)+
          " source-type="+(source?dot_object_source::state().types.at(dot_object_source::state().destructions.at(source).source_type-1).kind:"unknown")+
          " source-ordinal="+(source?std::to_string(dot_object_source::state().destructions.at(source).ordinal):"none")+
          " action-source-ordinals="+[&](){string r;for(auto original:active_source_obligations)r+=std::to_string(dot_object_source::state().destructions.at(original).ordinal)+":"+
            dot_object_source::state().types.at(dot_object_source::state().destructions.at(original).source_type-1).kind+",";return r;}()).c_str());
    }
    for(auto child=lifetime->child_lifetime;child;child=child->next)check_lifetime(child);
  }
  void source_cleanup_metadata(a_routine_ptr execution) {
    auto &captured=dot_object_source::state();auto primary=dot_object_source::abi_primary(execution);
    auto owner=primary?primary:execution;
    auto original_lifetimes=captured.lifetime_order[owner];
    auto original_destructions=captured.destruction_order[owner];
    for(auto original:captured.lifetime_order[nullptr])
      if(captured.initializer_lifetime_adoptions.count(std::make_pair(original,execution)))original_lifetimes.push_back(original);
    for(auto original:captured.destruction_order[nullptr])
      if(captured.initializer_lifetime_adoptions.count(std::make_pair(captured.destructions.at(original).lifetime,execution)))
        original_destructions.push_back(original);
    for(auto original:original_destructions)required_source_obligations.insert(original);
    // An observed ABI forwarding entry has no independent source lifetime.
    // Its source primary owns that graph; only a body with actions republishes
    // the original obligations under the exact ABI-origin relation.
    if(primary && active_cleanup.empty() && emitted_normal_obligations.empty() && active_payload_subobjects.empty() && emitted_array_obligations.empty() && emitted_allocation_obligations.empty() && !active_expression_lifetimes)return;
    // Each execution variant owns only the actions present in its body. EDG
    // complete/subobject/delegation entry points share the original source
    // graph; prove coverage across emitted variants after all bodies exist.
    bool has_terminal_elision=false;
    for(const auto &elision:captured.automatic_partial_terminal)if(elision.second==execution)has_terminal_elision=true;
    for(const auto &elision:captured.automatic_destructor_no_effect)if(elision.second==execution)has_terminal_elision=true;
    if(active_cleanup.empty() && emitted_normal_obligations.empty() && active_payload_subobjects.empty() && emitted_array_obligations.empty() && emitted_allocation_obligations.empty() && !active_expression_lifetimes && !has_terminal_elision)return;
    if(!emitted_cleanup_sources.insert(owner).second)return;
    for(auto original:original_lifetimes)
      source_lifetime_ids.emplace(original,unsigned(source_lifetime_ids.size()+1));
    for(auto original:original_destructions)
      source_obligation_ids.emplace(original,unsigned(source_obligation_ids.size()+1));
    for(auto original:original_lifetimes) {
      if(!emitted_source_lifetimes.insert(original).second)continue;
      const auto &value=captured.lifetimes.at(original);
      if(!value.error.empty())reject(value.error.c_str());
      unsigned parent=0,obligation=0,allocation_obligation=0;
      if(value.parent) {
        if(!source_lifetime_ids.count(value.parent))reject("source lifetime parent outside captured function");
        parent=source_lifetime_ids.at(value.parent);
      }
      if(value.parent_obligation) {
        auto found=source_obligation_ids.find(value.parent_obligation);
        if(found==source_obligation_ids.end() || captured.destructions.at(value.parent_obligation).lifetime!=value.parent)
          reject("source parent destruction relation is not exact parent ownership");
        if(captured.destructions.at(value.parent_obligation).allocation_release)allocation_obligation=found->second;
        else obligation=found->second;
      }
      source_lifetimes.push_back("{\"id\":"+number(source_lifetime_ids.at(original))+",\"functionEntityId\":"+number(value.owner?routine(value.owner):0)+
        ",\"sourceInitializerId\":"+number(value.initializer?source_initializer_id(value.initializer):0)+
        ",\"parentLifetimeId\":"+number(parent)+",\"siblingOrdinal\":"+std::to_string(value.sibling_ordinal)+
        ",\"parentObligationId\":"+number(obligation)+",\"parentAllocationObligationId\":"+number(allocation_obligation)+
        ",\"kind\":"+quote(value.kind)+position(value.position)+",\"captureProof\":\"before-lowering\"}");
    }
    for(auto original:original_destructions) {
      if(!emitted_source_obligations.insert(original).second)continue;
      const auto &value=captured.destructions.at(original);
      if(!value.error.empty())reject(value.error.c_str());
      if(!source_lifetime_ids.count(value.lifetime))reject("invalid original destruction lifetime provenance");
      if(value.allocation_release) {
        source_allocation_obligations.push_back("{\"id\":"+number(source_obligation_ids.at(original))+
          ",\"functionEntityId\":"+number(value.owner?routine(value.owner):0)+",\"lifetimeId\":"+number(source_lifetime_ids.at(value.lifetime))+
          ",\"sourceInitializerId\":"+number(value.initializer?source_initializer_id(value.initializer):0)+
          ",\"sourceStaticObligationId\":"+number(value.initializer?static_obligation_id(original):0)+
          ",\"ordinal\":"+std::to_string(value.ordinal)+",\"deallocatorEntityId\":"+number(routine(value.destructor))+
          ",\"allocatedSourceTypeId\":"+number(source_type(value.source_type))+
          ",\"isArray\":"+(value.array_allocation?"true":"false")+position(value.position)+",\"captureProof\":\"before-lowering\"}");
        continue;
      }
      source_destruction_obligations.push_back("{\"id\":"+number(source_obligation_ids.at(original))+
        ",\"functionEntityId\":"+number(value.owner?routine(value.owner):0)+",\"lifetimeId\":"+number(source_lifetime_ids.at(value.lifetime))+
        ",\"sourceInitializerId\":"+number(value.initializer?source_initializer_id(value.initializer):0)+
        ",\"sourceStaticObligationId\":"+number(value.initializer?static_obligation_id(original):0)+
        ",\"isPartialAggregate\":"+(value.partial_aggregate?"true":"false")+
        ",\"ordinal\":"+std::to_string(value.ordinal)+",\"destructorEntityId\":"+number(routine(value.destructor))+
        ",\"sourceTypeId\":"+number(source_type(value.source_type))+position(value.position)+",\"captureProof\":\"before-lowering\"}");
    }
  }
  a_field_ptr cleanup_path_field(a_type_ptr parent,const dot_object_source::CleanupPathPart &part) {
    auto &captured=dot_object_source::state();parent=skip_typerefs(parent);
    if(parent->kind!=tk_struct && parent->kind!=tk_class && parent->kind!=tk_union)reject("cleanup member parent is not a record");
    type_id(parent);
    for(auto field=parent->variant.class_struct_union.field_list;field;field=field->next) {
      if(part.base) {
        auto base=captured.generated_base_fields.find(field);
        if(base!=captured.generated_base_fields.end() && base->second==part.base)return field;
      } else {
        auto original=field;std::set<a_field_ptr> visited;
        while(original && visited.insert(original).second) {
          if(original==part.field)return field;
          auto copied=captured.copied_fields.find(original);
          if(copied==captured.copied_fields.end())break;
          original=copied->second;
        }
      }
    }
    if(part.base && (part.base->is_optimized_empty_base || (part.base->is_virtual && part.base->shares_virtual_function_info))) {
      auto complete=parent;auto view=captured.observed_subobjects.find(parent);
      if(view!=captured.observed_subobjects.end())complete=view->second;
      auto original=captured.prelower_records.find(complete);
      if(original!=captured.prelower_records.end())
        for(const auto &base:part.base->is_virtual?original->second.virtual_bases:original->second.bases)if(base.il==part.base)return nullptr;
    }
    reject("cleanup member/base path lacks exact lowered field correspondence");
  }
  unsigned local_static_guard(a_variable_ptr guard) {
    auto prior=local_static_guard_ids.find(guard);if(prior!=local_static_guard_ids.end())return prior->second;
    auto &captured=dot_object_source::state();
    auto found=captured.local_static_guard_variables.find(guard);
    if(found==captured.local_static_guard_variables.end())reject("guard lacks original static initialization");
    const auto &source=captured.local_static_initializers.at(found->second);
    auto owner=dot_object_source::abi_primary(current_routine);if(!owner)owner=current_routine;
    if(source.owner!=owner || !source.probe || !source.acquire || !source.release ||
       var_has_thread_storage_duration(guard) || type_id(guard->type)!=9 || !source.lifetime ||
       !captured.lifetimes.count(source.lifetime))reject("guard outside observed non-TLS IA64 acquire/release contract");
    unsigned id=unsigned(local_static_guard_ids.size()+1);local_static_guard_ids.emplace(guard,id);
    unsigned lifetime=source_lifetime_ids.emplace(source.lifetime,unsigned(source_lifetime_ids.size()+1)).first->second;
    local_static_guards.push_back("{\"id\":"+number(id)+",\"functionEntityId\":"+number(routine(source.owner))+
      ",\"storageId\":"+number(storage(source.variable))+",\"guardStorageId\":"+number(storage(guard))+
      ",\"sourceTypeId\":"+number(source_type(source.source_type))+",\"sourceLifetimeId\":"+number(lifetime)+
      ",\"guardPolicy\":\"ia64-acquire-release\""+position(source.position)+",\"captureProof\":\"observed-local-static-guard\"}");
    return id;
  }
  unsigned guard_address(a_variable_ptr guard,const a_source_position &pos) {
    if(type_id(guard->type)!=9)reject("local-static guard address is not ulong storage");
    return node("storage-address",pointer_type(9),pos,0,0,"","0","value","none",0,storage(guard));
  }
  unsigned guard_operation(an_expr_node_ptr expr,a_variable_ptr guard,const string &kind) {
    unsigned type=type_id(expr->type);
    if(type!=(kind=="release"?1:2))reject("observed guard operation result type changed");
    unsigned id=node("guard-operation",type,expr->position,0,0,"","","value","left-to-right");
    edge(id,"guard",0,guard_address(guard,expr->position));
    guard_operations.push_back("{\"nodeId\":"+number(id)+",\"guardId\":"+number(local_static_guard(guard))+
      ",\"kind\":"+quote(kind)+",\"captureProof\":\"observed-local-static-guard-operation\"}");
    return id;
  }
  unsigned allocation_target(const PendingCleanup &entry) {
    auto variable=entry.observed->variable;
    if(!variable || !binding_ids.count(variable) || !entry.observed->indirect || !entry.observed->path.empty())
      reject("allocation rollback target is not its observed pointer binding");
    unsigned type=type_id(variable->type);
    auto pos=dot_object_source::state().destructions.at(entry.original).position;
    unsigned target=node("binding",type,pos,0,binding_ids.at(variable),"","","place","none");
    auto parameter=skip_typerefs(entry.callback->type)->variant.routine.extra_info->param_type_list;
    unsigned expected=type_id(parameter->type);
    unsigned converted=node("convert",expected,pos,0,0,"","","value","left-to-right");
    edge(converted,"operand",0,target);return converted;
  }
  unsigned cleanup_target(const PendingCleanup &entry) {
    const auto &observed=*entry.observed;auto variable=observed.variable;
    auto pos=entry.static_cleanup?dot_object_source::state().static_destructions.at(entry.original).position:
      dot_object_source::state().destructions.at(entry.original).position;
    a_type_ptr type;unsigned result;
    if(observed.throw_origin) {
      auto found=active_throw_ids.find(observed.throw_origin);
      if(found==active_throw_ids.end())reject("payload cleanup lacks its emitted owning throw");
      type=skip_typerefs(observed.base_type);
      unsigned address=node("cleanup-payload-address",pointer_type(type_id(type)),pos,0,0,"","","value","none");
      cleanup_payload_targets.push_back("{\"nodeId\":"+number(address)+",\"actionId\":"+number(entry.action_id)+
        ",\"throwNodeId\":"+number(found->second)+",\"captureProof\":\"observed-thrown-object-position\"}");
      result=node("dereference",type_id(type),pos,0,0,"","","place","none");
      edge(result,"operand",0,address);
    } else if(entry.static_cleanup) {
      type=skip_typerefs(variable->type);
      result=node("storage",type_id(type),pos,0,0,"","","place","none",0,storage(variable));
    } else {
      if(!binding_ids.count(variable))reject("cleanup object is not owned function binding");
      type=skip_typerefs(variable->type);
      result=node("binding",type_id(type),pos,0,binding_ids.at(variable),"","","place","none");
    }
    if(observed.indirect) {
      if(type->kind!=tk_pointer)reject("indirect cleanup target is not pointer binding");
      type=skip_typerefs(type->variant.pointer.type);
      unsigned dereference=node("dereference",type_id(type),pos,0,0,"","","place","none");
      edge(dereference,"operand",0,result);result=dereference;
    }
    for(const auto &part:observed.path) {
      if(part.field || part.base) {
        auto field=cleanup_path_field(type,part);
        if(!field) {
          // Empty-base optimization removes the physical field, not the
          // source base identity or its observed address. Keep an address-only
          // place tied to recordBaseSubobjects; never allocate a dummy byte.
          type=skip_typerefs(part.base->type);
          unsigned base=node(part.base->is_virtual?"virtual-base-subobject":"base-subobject",type_id(type),pos,0,0,"","","place","none");
          edge(base,"operand",0,result);
          if(part.base->is_virtual)virtual_base_expressions.push_back("{\"nodeId\":"+number(base)+
            ",\"completeBaseTypeId\":"+number(type_id(type))+"}");
          else base_subobject_expressions.push_back("{\"nodeId\":"+number(base)+
            ",\"sourceBaseId\":"+number(source_base_ids.at(part.base))+"}");
          result=base;continue;
        }
        if(field->is_bit_field)reject("bitfield destruction target");
        type=skip_typerefs(field->type);
        unsigned member=node("member",type_id(type),pos,0,0,"","","place","none",field_ids.at(field));
        edge(member,"operand",0,result);result=member;
      } else {
        if(type->kind!=tk_array || array_type_has_no_bound(type) || part.index>=type->variant.array.variant.number_of_elements)
          reject("cleanup array-element path outside fixed bounds");
        type=skip_typerefs(type->variant.array.element_type);unsigned pointer=pointer_type(type_id(type));
        unsigned decay=node("array-decay",pointer,pos,0,0,"","","value","none");edge(decay,"operand",0,result);
        unsigned index=node("constant",9,pos,0,0,"",std::to_string(part.index),"value","none");
        unsigned add=node("binary",pointer,pos,0,0,"pointer-add","","value","left-to-right");
        edge(add,"left",0,decay);edge(add,"right",0,index);
        result=node("dereference",type_id(type),pos,0,0,"","","place","none");edge(result,"operand",0,add);
      }
    }
    if(observed.array) {
      // Retain typed array decay to the underlying leaf, including all fixed
      // dimensions. Counts/strides come from the observed EDG array region.
      while(type->kind==tk_array) {
        type=skip_typerefs(type->variant.array.element_type);
        unsigned decay=node("array-decay",pointer_type(type_id(type)),pos,0,0,"","","value","none");
        edge(decay,"operand",0,result);
        result=node("dereference",type_id(type),pos,0,0,"","","place","none");edge(result,"operand",0,decay);
      }
      if(type!=skip_typerefs(observed.element_type))reject("cleanup array leaf differs from observed element");
    }
    if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)
      reject("cleanup target is not object leaf");
    unsigned address_type=pointer_type(type_id(type));
    unsigned address=node("address",address_type,pos,0,0,"","","value","none");edge(address,"operand",0,result);
    auto callback=skip_typerefs(entry.callback->type);auto parameter=callback->variant.routine.extra_info->param_type_list;
    auto expected=skip_typerefs(skip_typerefs(parameter->type)->variant.pointer.type);
    unsigned expected_pointer=type_id(parameter->type);
    if(address_type!=expected_pointer) {
      auto found=dot_object_source::state().observed_subobjects.find(type);
      if(!(observed.base_subobject || observed.delegation) || found==dot_object_source::state().observed_subobjects.end() || found->second!=expected)
        reject("cleanup target pointer differs from exact destructor receiver");
      unsigned convert=node("convert",expected_pointer,pos,0,0,"","","value","left-to-right");
      edge(convert,"operand",0,address);address=convert;
    }
    return address;
  }
  unsigned cleanup_marker(an_expr_node_ptr expr) {
    auto &value=expr->variant.lowered_eh;
    if(value.kind==leck_internal_try) {
      // lower_eh::make_internal_try_expr owns precisely two expressions.
      // Its result is void: the attempt's value is discarded, and failure
      // unwinds to this boundary before running the cleanup and rethrowing.
      auto attempt=value.variant.try_and_catch_expr;
      if(type_id(expr->type)!=1 || !attempt || !attempt->next || attempt->next->next)
        reject("internal try lacks its exact void/two-expression contract");
      unsigned id=node("internal-try-expression",1,expr->position,0,0,"","","value","left-to-right");
      unsigned obligation=0;
      auto allocation=active_allocation_internal_tries.find(expr);
      if(allocation!=active_allocation_internal_tries.end()) {
        auto original=allocation->second;
        obligation=source_obligation_ids.emplace(original,unsigned(source_obligation_ids.size()+1)).first->second;
        emitted_allocation_obligations.insert(original);
      }
      internal_try_expressions.push_back("{\"nodeId\":"+number(id)+
        ",\"sourceAllocationObligationId\":"+number(obligation)+
        ",\"cleanupPolicy\":\"unwind-to-entry-before-handler\",\"captureProof\":\"observed-internal-try\"}");
      edge(id,"attempt",0,expression(attempt));
      edge(id,"cleanup",0,expression(attempt->next));
      return id;
    }
    if(value.kind==leck_caught_object_address) {
      auto found=active_handler_ids.find(value.variant.caught_object_handler);
      if(found==active_handler_ids.end() || !value.variant.caught_object_handler->parameter)
        reject("caught object address lacks its typed handler");
      unsigned id=node("caught-object-address",type_id(expr->type),expr->position,0,0,"","","value","none");
      caught_object_addresses.push_back("{\"nodeId\":"+number(id)+",\"handlerNodeId\":"+number(found->second)+
        ",\"captureProof\":\"observed-caught-object-address\"}");return id;
    }
    if(value.kind==leck_exception_caught || value.kind==leck_catch_epilogue || value.kind==leck_try_epilogue) {
      unsigned owner=0;string kind;
      if(value.kind==leck_try_epilogue) {
        auto found=active_try_ids.find(value.variant.epilogue_try_block);
        if(found==active_try_ids.end())reject("try epilogue lacks its owned try");
        owner=found->second;kind="try-exit";
      } else {
        auto handler=value.kind==leck_exception_caught?emitting_handler:value.variant.epilogue_handler;
        auto found=active_handler_ids.find(handler);
        if(found==active_handler_ids.end())reject("handler marker lacks its owned handler");
        owner=found->second;kind=value.kind==leck_exception_caught?"caught":"handler-exit";
      }
      unsigned id=node("handler-marker",1,expr->position,0,0,"","","value","none");
      handler_markers.push_back("{\"nodeId\":"+number(id)+",\"ownerNodeId\":"+number(owner)+
        ",\"kind\":"+quote(kind)+",\"captureProof\":\"observed-handler-marker\"}");return id;
    }
    if(value.kind==leck_cleanup_state || value.kind==leck_unreachable_cleanup_state) {
      unsigned state=0;auto raw=value.variant.cleanup_region_number;
      if(raw!=null_eh_region_number) {
        auto found=active_cleanup.find(raw);if(found==active_cleanup.end())reject("cleanup transition references unknown state");
        state=found->second.state_id;
      }
      unsigned result=node("cleanup-transition",1,expr->position,0,0,"","","value","left-to-right");
      cleanup_transitions.push_back("{\"nodeId\":"+number(result)+",\"stateId\":"+number(state)+
        ",\"captureProof\":\"observed-cleanup-state\",\"isUnreachable\":"+(value.kind==leck_unreachable_cleanup_state?"true":"false")+"}");
      if(active_throw_node)throw_cleanup_transitions.push_back("{\"nodeId\":"+number(result)+
        ",\"throwNodeId\":"+number(active_throw_node)+",\"captureProof\":\"observed-throw-initializer-transition\"}");
      return result;
    }
    if(value.kind==leck_function_prologue || value.kind==leck_function_epilogue) {
      if(!active_cleanup_prologue || (value.kind==leck_function_prologue && expr!=active_cleanup_prologue) ||
         (value.kind==leck_function_epilogue && value.variant.epilogue_routine!=current_routine))
        reject("cleanup frame marker lacks exact prologue ownership");
      unsigned result=node("cleanup-frame",1,expr->position,0,0,"","","value","left-to-right");
      cleanup_frame_markers.push_back("{\"nodeId\":"+number(result)+",\"kind\":"+quote(value.kind==leck_function_prologue?"prologue":"epilogue")+
        ",\"captureProof\":\"observed-cleanup-frame\"}");return result;
    }
    reject("exception construct outside owned scalar cleanup",value.kind);
  }
  void write_owned_cleanup(a_routine_ptr owner) {
    source_cleanup_metadata(owner);
    for(const auto &entry:active_try_ids) {
      auto original=dot_object_source::source_lifetime(entry.first->lifetime,owner);
      if(!original || !source_lifetime_ids.count(original))reject("source try lacks original lifetime identity");
      const auto &saved=dot_object_source::state().tries.at(entry.first);
      string kind="ordinary";
      if(saved.function_try) {
        if(owner->special_kind==sfk_constructor)kind="constructor";
        else if(owner->special_kind==sfk_destructor)kind="destructor";
        else reject("function-try owner is not a constructor or destructor");
      }
      source_tries.push_back("{\"nodeId\":"+number(entry.second)+",\"sourceLifetimeId\":"+number(source_lifetime_ids.at(original))+
        ",\"tryKind\":"+quote(kind)+",\"captureProof\":\"observed-source-try\"}");
    }
    std::vector<std::pair<a_dynamic_init_ptr,const dot_object_source::CleanupSnapshot*>> payloads(
      active_payload_subobjects.begin(),active_payload_subobjects.end());
    std::sort(payloads.begin(),payloads.end(),[&](const auto &left,const auto &right) {
      return source_obligation_ids.at(left.first)<source_obligation_ids.at(right.first);
    });
    for(const auto &entry:payloads) {
      const auto &observed=*entry.second;auto found=active_throw_ids.find(observed.throw_origin);
      if(found==active_throw_ids.end())reject("payload ownership lacks its emitted throw");
      unsigned obligation=source_obligation_ids.at(entry.first);
      throw_payload_subobjects.push_back("{\"sourceObligationId\":"+number(obligation)+
        ",\"throwNodeId\":"+number(found->second)+",\"completionPolicy\":\"complete-payload-destructor\",\"captureProof\":\"observed-payload-subobject-init\"}");
      auto type=skip_typerefs(observed.base_type);unsigned ordinal=0;
      for(const auto &part:observed.path) {
        if((!part.field && !part.base) || (part.base && part.base->is_virtual))reject("payload ownership outside nonvirtual scalar member/base path");
        auto field=cleanup_path_field(type,part);
        unsigned field_id=field?field_ids.at(field):0,base_id=field?0:source_base_ids.at(part.base);
        throw_payload_paths.push_back("{\"sourceObligationId\":"+number(obligation)+",\"ordinal\":"+std::to_string(ordinal++)+
          ",\"fieldId\":"+number(field_id)+",\"sourceBaseId\":"+number(base_id)+"}");
        type=skip_typerefs(field?field->type:part.base->type);
      }
      if(!ordinal)reject("payload ownership requires a proper subobject");
    }
    for(const auto &pair:active_cleanup) {
      const auto &entry=pair.second;
      unsigned next=entry.next==null_eh_region_number?0:active_cleanup.at(entry.next).state_id;
      if(entry.observed->release_handler) {
        auto found=active_handler_ids.find(entry.observed->release_handler);
        if(found==active_handler_ids.end())reject("release state lacks its emitted handler");
        cleanup_states.push_back("{\"id\":"+number(entry.state_id)+",\"functionEntityId\":"+number(function)+
          ",\"actionId\":null,\"allocationActionId\":null,\"guardActionId\":null,\"nextStateId\":"+number(next)+",\"handlerNodeId\":"+number(found->second)+"}");
        continue;
      }
      if(entry.guard_abort) {
        auto guard=entry.observed->variable;unsigned id=local_static_guard(guard);
        auto &captured=dot_object_source::state();
        auto pos=captured.local_static_initializers.at(captured.local_static_guard_variables.at(guard)).position;
        unsigned target=guard_address(guard,pos);
        guard_cleanup_actions.push_back("{\"id\":"+number(entry.action_id)+",\"functionEntityId\":"+number(function)+
          ",\"guardId\":"+number(id)+",\"targetNodeId\":"+number(target)+",\"captureProof\":\"observed-guard-region\"}");
        cleanup_states.push_back("{\"id\":"+number(entry.state_id)+",\"functionEntityId\":"+number(function)+
          ",\"actionId\":null,\"allocationActionId\":null,\"guardActionId\":"+number(entry.action_id)+
          ",\"nextStateId\":"+number(next)+",\"handlerNodeId\":null}");continue;
      }
      if(entry.allocation_release) {
        unsigned flag=entry.observed->conditional_flag?binding_ids.at(entry.observed->conditional_flag):0;
        allocation_cleanup_actions.push_back("{\"id\":"+number(entry.action_id)+",\"functionEntityId\":"+number(function)+
          ",\"sourceAllocationObligationId\":"+number(source_obligation_ids.at(entry.original))+
          ",\"callbackEntityId\":"+number(routine(entry.callback))+",\"targetNodeId\":"+number(allocation_target(entry))+
          ",\"allocationSize\":"+(entry.sized_release?std::to_string(entry.array_element_size):"null")+
          ",\"conditionBindingId\":"+number(flag)+",\"nullPolicy\":\"skip-null\",\"captureProof\":\"observed-allocation-region\"}");
        cleanup_states.push_back("{\"id\":"+number(entry.state_id)+",\"functionEntityId\":"+number(function)+
          ",\"actionId\":null,\"allocationActionId\":"+number(entry.action_id)+",\"guardActionId\":null"+
          ",\"nextStateId\":"+number(next)+",\"handlerNodeId\":null}");
        emitted_allocation_obligations.insert(entry.original);continue;
      }
      unsigned callback=routine(entry.callback),target=cleanup_target(entry);
      if(auto flag=entry.observed->conditional_flag) {
        auto binding=binding_ids.find(flag);
        if(binding==binding_ids.end())reject("conditional cleanup flag lacks owned local binding");
        // The ordinary lowered body owns initialization and all arming/reset
        // stores. Unwinding only tests the flag before evaluating the target,
        // exactly as the stock runtime does; it adds no flag reset store.
        cleanup_conditions.push_back("{\"actionId\":"+number(entry.action_id)+",\"flagBindingId\":"+number(binding->second)+
          ",\"captureProof\":\"observed-conditional-cleanup-flag\"}");
      }
      if(entry.observed->array)cleanup_arrays.push_back("{\"actionId\":"+number(entry.action_id)+
        ",\"elementTypeId\":"+number(type_id(entry.observed->element_type))+",\"elementCount\":"+std::to_string(entry.array_count)+
        ",\"elementSize\":"+std::to_string(entry.array_element_size)+",\"captureProof\":\"observed-array-region\"}");
      if(entry.observed->vtt_argument) {
        auto binding=binding_ids.find(entry.observed->vtt_argument);
        if(binding==binding_ids.end())reject("VTT cleanup argument lacks owned binding");
        cleanup_vtt_arguments.push_back("{\"actionId\":"+number(entry.action_id)+",\"bindingId\":"+number(binding->second)+
          ",\"captureProof\":\"observed-subobject-vtt\"}");
      }
      if(entry.static_cleanup)emitted_static_cleanup_obligations.insert(entry.original);
      cleanup_actions.push_back("{\"id\":"+number(entry.action_id)+",\"functionEntityId\":"+number(function)+
        ",\"sourceObligationId\":"+number(entry.static_cleanup?0:source_obligation_ids.at(entry.original))+
        ",\"sourceStaticObligationId\":"+number(entry.static_cleanup?static_obligation_id(entry.original):0)+",\"callbackEntityId\":"+number(callback)+
        ",\"targetNodeId\":"+number(target)+",\"captureProof\":\"observed-region-entry\"}");
      cleanup_states.push_back("{\"id\":"+number(entry.state_id)+",\"functionEntityId\":"+number(function)+
        ",\"actionId\":"+number(entry.action_id)+",\"allocationActionId\":null,\"guardActionId\":null"+
        ",\"nextStateId\":"+number(next)+",\"handlerNodeId\":null}");
    }
    for(const auto &elision:dot_object_source::state().automatic_partial_terminal)if(elision.second==owner) {
      if(!source_obligation_ids.count(elision.first))reject("terminal partial elision lacks original source obligation");
      // __cxa_vec_ctor/__cxa_vec_cctor own rollback of the constructed prefix
      // inside the call. Stock may independently omit cleanup after that call
      // has completed; only a subsequent destruction conflicts with elision.
      bool array_destruction=false;
      for(const auto &operation:active_array_runtime)
        if(operation.second.first==elision.first && operation.second.second->operation=="destroy" &&
           emitted_expression_ids.count(operation.first))array_destruction=true;
      if(active_source_obligations.count(elision.first) || emitted_normal_obligations.count(elision.first) ||
         array_destruction || active_payload_subobjects.count(elision.first))
        reject((string("terminal partial elision also has an executable destruction in the same function: ordinal=")+
          std::to_string(dot_object_source::state().destructions.at(elision.first).ordinal)+
          " normal="+std::to_string(emitted_normal_obligations.count(elision.first))+
          " array="+std::to_string(emitted_array_obligations.count(elision.first))+
          " payload="+std::to_string(active_payload_subobjects.count(elision.first))+
          " regions="+[&](){string detail;for(const auto &region:active_cleanup)if(region.second.original==elision.first && region.second.observed) {
            auto observed=region.second.observed;detail+="same-dip="+std::to_string(dot_object_source::state().automatic_partial_terminal_copies.at(elision).count(observed->original))+
            "/array="+std::to_string(observed->array)+"/count="+std::to_string(observed->element_count)+"/path="+std::to_string(observed->path.size())+",";
          }return detail;}()).c_str());
      destruction_elisions.push_back("{\"sourceObligationId\":"+number(source_obligation_ids.at(elision.first))+
        ",\"functionEntityId\":"+number(function)+",\"kind\":\"terminal-no-throw\",\"captureProof\":\"observed-terminal-partial-elision\"}");
      covered_source_obligations.insert(elision.first);
    }
    for(const auto &elision:dot_object_source::state().automatic_destructor_no_effect)if(elision.second==owner) {
      if(!source_obligation_ids.count(elision.first))reject("no-effect elision lacks original source obligation");
      if(active_source_obligations.count(elision.first) || emitted_normal_obligations.count(elision.first) ||
         emitted_array_obligations.count(elision.first) || active_payload_subobjects.count(elision.first) ||
         dot_object_source::state().automatic_partial_terminal.count(elision))
        reject("no-effect destruction also has an executable destruction or another disposition");
      destruction_elisions.push_back("{\"sourceObligationId\":"+number(source_obligation_ids.at(elision.first))+
        ",\"functionEntityId\":"+number(function)+",\"kind\":\"stock-no-effect\",\"captureProof\":\"observed-no-effect-destruction-removal\"}");
      covered_source_obligations.insert(elision.first);
    }
    covered_source_obligations.insert(active_source_obligations.begin(),active_source_obligations.end());
    covered_source_obligations.insert(emitted_normal_obligations.begin(),emitted_normal_obligations.end());
    covered_source_obligations.insert(emitted_array_obligations.begin(),emitted_array_obligations.end());
    covered_source_obligations.insert(emitted_allocation_obligations.begin(),emitted_allocation_obligations.end());
    for(const auto &entry:active_payload_subobjects)covered_source_obligations.insert(entry.first);
  }
  void validate_file_initializers(a_scope_ptr file_scope) {
    auto &captured=dot_object_source::state();
    if(file_scope->asm_entries)reject("file-scope assembly");
    // Persistent and initializer-temporary obligations are validated against
    // their original static graph and observed execution ownership below.
    for(const auto &entry:captured.dynamic_initializers) {
      const auto &init=entry.second;
      diagnostic_scope diagnostic(init.position);
      if(!init.variable)reject("observed file initializer lacks source storage");
      if(ignore_variable_in_back_end(init.variable) || init.variable->suppress_inline_definition)continue;
      if(!init.error.empty())reject(init.error.c_str());
      if(!captured.file_initializers_observed || !init.begun || !init.complete ||
         !init.routine || !captured.generated_initializers.count(init.routine))
        reject("file initializer lacks complete observed lowering");
      if(init.guarded)reject("guarded inline/template file initialization");
      if(thread_storage(init.variable))reject("dynamic TLS initialization schedule");
      if(!captured.variables.count(init.variable))reject("file initializer lacks original source declaration");
      for(auto statement:init.statements) {
        if(!initializer_statement_anchors.insert(statement).second)
          reject("overlapping observed file initializer intervals");
      }
    }
    // GNU priority lowering may leave the consumed dik_none head attached.
    // Accept only entries whose exact lowering was observed (or definitions
    // stock C explicitly suppresses), never an arbitrary residual list.
    for(auto dip=file_scope->dynamic_inits;dip;dip=dip->next) {
      auto observed=captured.dynamic_initializers.find(dip);
      if(observed==captured.dynamic_initializers.end())reject("unobserved residual file initializer");
      auto v=observed->second.variable;
      if(!v || (!(ignore_variable_in_back_end(v) || v->suppress_inline_definition) &&
                !observed->second.complete))reject("unconsumed residual file initializer");
    }
  }
  void prepare_static_source_metadata() {
    auto &captured=dot_object_source::state();
    for(auto original:captured.source_initializer_order)source_initializer_id(original);
    for(auto original:captured.static_lifetime_order)static_lifetime_id(original);
    for(auto original:captured.static_destruction_order)static_obligation_id(original);
    for(auto original:captured.source_initializer_order) {
      const auto &value=captured.source_initializers.at(original);
      source_initializers.push_back("{\"id\":"+number(source_initializer_id(original))+
        ",\"storageId\":"+number(source_initializer_emitted(original)?storage(value.variable):0)+",\"sourceTypeId\":"+number(source_type(value.source_type))+
        ",\"sourceName\":"+quote(value.source_name)+",\"executionDisposition\":"+quote(source_initializer_emitted(original)?"emitted":"stock-not-emitted")+
        ",\"functionEntityId\":"+number(value.owner?routine(value.owner):0)+
        ",\"sourceOrdinal\":"+(value.owner?"null":std::to_string(value.ordinal))+
        ",\"kind\":"+quote(value.owner?"local-static":"file")+position(value.position)+",\"captureProof\":\"before-lowering\"}");
    }
    for(auto original:captured.static_lifetime_order) {
      const auto &value=captured.static_lifetimes.at(original);
      if(!value.error.empty())reject(value.error.c_str());
      source_static_lifetimes.push_back("{\"id\":"+number(static_lifetime_id(original))+
        ",\"functionEntityId\":"+number(value.owner?routine(value.owner):0)+
        ",\"sourceInitializerId\":"+number(value.initializer?source_initializer_id(value.initializer):0)+
        ",\"parentLifetimeId\":"+number(value.parent?static_lifetime_id(value.parent):0)+
        ",\"parentObligationId\":"+number(value.parent_obligation?static_obligation_id(value.parent_obligation):0)+
        ",\"siblingOrdinal\":"+(value.has_sibling_ordinal?std::to_string(value.sibling_ordinal):"null")+
        ",\"kind\":"+quote(value.kind)+position(value.position)+",\"captureProof\":\"before-lowering\"}");
    }
    for(auto original:captured.static_destruction_order) {
      const auto &value=captured.static_destructions.at(original);
      if(!value.error.empty())reject(value.error.c_str());
      source_static_destructions.push_back("{\"id\":"+number(static_obligation_id(original))+
        ",\"lifetimeId\":"+number(static_lifetime_id(value.lifetime))+",\"ordinal\":"+std::to_string(value.ordinal)+
        ",\"storageId\":"+number(value.persistent && value.variable && value.initializer && source_initializer_emitted(value.initializer)?storage(value.variable):0)+
        ",\"sourceTypeId\":"+number(source_type(value.source_type))+",\"selectedRoutineEntityId\":"+number(routine(value.selected_routine))+
        ",\"sourceInitializerId\":"+number(value.initializer?source_initializer_id(value.initializer):0)+
        ",\"kind\":"+quote(value.persistent?"persistent":value.partial_aggregate?"partial-aggregate":value.allocation?"allocation":"automatic")+
        ",\"isArrayAllocation\":"+(value.allocation?(value.array_allocation?"true":"false"):"null")+
        ",\"domain\":"+quote(value.thread?"thread":"process")+position(value.position)+",\"captureProof\":\"before-lowering\"}");
    }
  }
  void finish_static_source_metadata() {
    auto &captured=dot_object_source::state();
    for(const auto &entry:captured.initializer_lifetime_adoptions) {
      auto original=entry.first.first;auto execution=entry.first.second;
      if(!routine_ids.count(execution) || !source_lifetime_ids.count(original))continue;
      initializer_lifetime_executions.push_back("{\"sourceLifetimeId\":"+number(source_lifetime_ids.at(original))+
        ",\"sourceStaticLifetimeId\":"+number(static_lifetime_id(original))+
        ",\"sourceInitializerId\":"+number(source_initializer_id(entry.second))+
        ",\"functionEntityId\":"+number(routine_ids.at(execution))+",\"captureProof\":\"observed-initializer-lifetime-adoption\"}");
    }
    for(auto original:captured.static_destruction_order) {
      const auto &value=captured.static_destructions.at(original);
      if(value.partial_aggregate) {
        if(!value.initializer) {
          diagnostic_scope diagnostic(value.position);
          const auto &lifetime=captured.static_lifetimes.at(value.lifetime);
          reject((string("original static partial aggregate lacks source initializer containment: type=")+captured.types.at(value.source_type-1).name+
            " owner="+(value.owner?source_spelling(&value.owner->source_corresp):"none")+" lifetime="+lifetime.kind+
            " entity="+std::to_string(value.lifetime->entity.kind)+" original-ordinal="+std::to_string(value.ordinal)+lifetime.entity_description).c_str());
        }
        bool action=emitted_static_cleanup_obligations.count(original),terminal=captured.static_partial_terminal.count(original);
        bool omitted=!source_initializer_emitted(value.initializer);
        if(omitted?(action || terminal):(action==terminal))
          reject("original static partial aggregate lacks exactly one action or observed terminal elision");
        static_destruction_dispositions.push_back("{\"sourceObligationId\":"+number(static_obligation_id(original))+
          ",\"kind\":"+quote(omitted?"stock-not-emitted":action?"cleanup-action":"stock-terminal-no-throw")+
          ",\"captureProof\":\"observed-static-destruction-disposition\"}");
        continue;
      }
      if(!value.persistent)continue;
      bool registered=registered_static_obligations.count(original),no_effect=captured.static_no_effect.count(original);
      bool not_emitted=value.initializer && !source_initializer_emitted(value.initializer);
      if(not_emitted?registered:(registered==no_effect))reject("original static destruction lacks exactly one observed disposition");
      static_destruction_dispositions.push_back("{\"sourceObligationId\":"+number(static_obligation_id(original))+
        ",\"kind\":"+quote(not_emitted?"stock-not-emitted":registered?"registered":"stock-proven-no-effect")+
        ",\"captureProof\":\"observed-static-destruction-disposition\"}");
    }
  }
  void write_dynamic_initializer_origins() {
    auto &captured=dot_object_source::state();
    // Action order, not pointer-map order, determines persistent row IDs.
    unsigned next=1;
    std::set<a_dynamic_init_ptr> emitted;
    for(auto routine:routines) {
      auto origin=captured.generated_initializers.find(routine);
      if(origin==captured.generated_initializers.end())continue;
      auto action=initialization_action_ids.find(routine);
      if(action==initialization_action_ids.end())reject("observed startup routine has no scheduled action");
      unsigned ordinal=0;
      for(auto original:origin->second.initializers) {
        const auto &init=captured.dynamic_initializers.at(original);
        if(init.ordinal!=ordinal || init.routine!=routine || !init.complete)
          reject("file initializer action membership/order changed");
        if(!emitted.insert(original).second)reject("source initializer has multiple actions");
        unsigned id=next++;
        dynamic_initializers.push_back("{\"id\":"+number(id)+",\"actionId\":"+number(action->second)+
          ",\"sourceInitializerId\":"+number(source_initializer_id(original))+
          ",\"storageId\":"+number(storage_ids.at(init.variable))+",\"sourceOrdinal\":"+std::to_string(init.source_ordinal)+
          ",\"ordinal\":"+std::to_string(ordinal++)+",\"captureProof\":\"observed-file-scope-init-lowering\""+position(init.position)+"}");
        unsigned statement_ordinal=0;
        for(auto statement:init.statements) {
          auto owned=initializer_statement_nodes.find(statement);
          if(owned==initializer_statement_nodes.end()) {
            // These stock nonexecuting nodes intentionally have no owned
            // statement; dropping executable nodes would lose provenance.
            if(statement->kind==stmk_empty || statement->kind==stmk_decl)continue;
            reject("observed file initializer statement missing from owned graph");
          }
          if(!owned_generated_root_nodes.insert(owned->second).second)
            reject("initializer statement has duplicate source/scaffold ownership");
          dynamic_initializer_statements.push_back("{\"initializerId\":"+number(id)+
            ",\"ordinal\":"+std::to_string(statement_ordinal++)+",\"nodeId\":"+number(owned->second)+"}");
        }
      }
    }
    for(const auto &entry:captured.dynamic_initializers) {
      auto variable=entry.second.variable;
      if(ignore_variable_in_back_end(variable) || variable->suppress_inline_definition)continue;
      if(!emitted.count(entry.first))reject("observed source initializer lacks owned scheduled action");
    }
    if(generated_root_nodes!=owned_generated_root_nodes) {
      string missing,extra;
      for(auto id:generated_root_nodes)if(!owned_generated_root_nodes.count(id))missing+=nodes.at(node_shapes.at(id).json_index).substr(0,200)+",";
      for(auto id:owned_generated_root_nodes)if(!generated_root_nodes.count(id))extra+=nodes.at(node_shapes.at(id).json_index).substr(0,200)+",";
      reject((string("generated initializer body has unexplained or nonroot statements: missing=")+missing+" extra="+extra).c_str());
    }
  }
  void generated_scope_scaffold(unsigned node_id,unsigned binding_id,const string &kind,const string &proof="observed-scope-binding") {
    if(!current_routine || !dot_object_source::state().generated_initializers.count(current_routine))return;
    unsigned ordinal=generated_scaffold_ordinals[current_routine]++;
    generated_routine_scaffolds.push_back("{\"entityId\":"+number(function)+",\"ordinal\":"+std::to_string(ordinal)+
      ",\"nodeId\":"+number(node_id)+",\"bindingId\":"+number(binding_id)+",\"kind\":"+quote(kind)+
      ",\"captureProof\":"+quote(proof)+"}");
    if(!owned_generated_root_nodes.insert(node_id).second)reject("duplicate generated scope scaffold");
  }
  void startup_action(a_routine_ptr r,unsigned ordinal) {
#if GNU_EXTENSIONS_ALLOWED
    auto observed=dot_object_source::state().generated_initializers.find(r);
    if(!r->is_initialization_routine && observed==dot_object_source::state().generated_initializers.end())return;
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
    unsigned long priority=observed!=dot_object_source::state().generated_initializers.end()?
      (observed->second.priority?observed->second.priority:65535):
      r->has_ctor_priority?gnu_routine_supp(r)->ctor_priority:65535;
    if(priority>65535)reject("startup action priority outside GNU range");
    unsigned entity=routine(r);
    if(observed!=dot_object_source::state().generated_initializers.end())
      for(auto original:observed->second.initializers)
        storage(dot_object_source::state().dynamic_initializers.at(original).variable);
    initialization_action_ids[r]=unsigned(initialization_actions.size()+1);
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
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
    bool is_wrapper=r->overriding_function_for_wrapper!=nullptr;
#else
    bool is_wrapper=false;
#endif
    auto wrapper_it=dot_object_source::state().thunk_wrappers.find(r);
    const dot_object_source::ThunkWrapper *wrapper=wrapper_it==dot_object_source::state().thunk_wrappers.end()?nullptr:&wrapper_it->second;
    if(is_wrapper!=bool(wrapper))reject("virtual wrapper lacks observed creation provenance");
    auto primary=dot_object_source::abi_primary(r);
    auto adapter_it=dot_object_source::state().default_adapters.find(r);
    const dot_object_source::DefaultAdapter *adapter=adapter_it==dot_object_source::state().default_adapters.end()?nullptr:&adapter_it->second;
    string entry_kind;
#if DO_IL_LOWERING && IA64_ABI
    if(primary) {
      if(dot_object_source::abi_primary(primary) || primary==r)reject("cyclic/generated primary ABI origin");
      entry_kind=adapter?"constructor-default-adapter":r->ctor_dtor_kind==cdk_deleting?"deleting":r->ctor_dtor_kind==cdk_subobject?"subobject":
        r->ctor_dtor_kind==cdk_complete?"complete":r->ctor_dtor_kind==cdk_delegation?"delegation":"unsupported";
      if(entry_kind=="unsupported" || ctors_return_this || dtors_return_this || targ_ia64_abi_variant_ctors_and_dtors_return_this)
        reject("alternate entry outside ordinary nondelegating void-return ABI");
      auto original=dot_object_source::state().functions.find(primary);
      if(original==dot_object_source::state().functions.end() || original->second.function_kind=="ordinary" ||
         ((entry_kind=="deleting" || entry_kind=="delegation") && original->second.function_kind!="destructor"))reject("alternate lacks original constructor/destructor proof");
      const auto &signature=dot_object_source::state().types.at(original->second.signature_type-1);
      if(signature.variadic || !signature.object_type)reject("variadic/receiverless alternate entry");
      if(adapter) {
        if(original->second.function_kind!="constructor" || adapter->implied_arguments || adapter->copy_constructor ||
           !adapter->has_arguments || primary->is_initializer_list_ctor || r->storage_class!=sc_static || r->is_alias_entry)
          reject("default constructor adapter outside ordinary receiver-only contract");
        if(dot_object_source::state().default_adapters.count(adapter->target))reject("recursive default constructor adapter target");
        if(adapter->target!=primary &&
           (dot_object_source::abi_primary(adapter->target)!=primary ||
            (adapter->target->ctor_dtor_kind!=cdk_complete && adapter->target->ctor_dtor_kind!=cdk_subobject)))
          reject("default constructor adapter target lacks direct original entry proof");
        if(original->second.parameters.empty() || original->second.parameter_defaults.size()!=original->second.parameters.size())
          reject("default constructor adapter lacks original default-parameter facts");
        for(bool has_default:original->second.parameter_defaults)
          if(!has_default)reject("receiver-only adapter omits a nondefault original parameter");
        auto formal=skip_typerefs(r->type)->variant.routine.extra_info->param_type_list;
        if(!formal || formal->next)reject("default constructor adapter has additional execution parameters");
      }
      if(skip_typerefs(r->type)->variant.routine.extra_info->has_ellipsis ||
         skip_typerefs(skip_typerefs(r->type)->variant.routine.return_type)->kind!=tk_void ||
         skip_typerefs(skip_typerefs(primary->type)->variant.routine.return_type)->kind!=tk_void)
        reject("alternate entry return/variadic ABI");
    }
#endif
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
    if(adapter && !body)reject("observed default constructor adapter lacks its executable body");
    bool known_empty_definition=!extra->prototyped && C_mode() && body &&
      !extra->param_type_list && !extra->has_ellipsis && !body->variant.routine.parameters;
    if(!extra->prototyped && !known_empty_definition)reject("unresolved/nonempty unprototyped function");
    if(extra->explicit_calling_convention)reject("explicit calling convention");


    auto record_value=[](a_type_ptr type){type=skip_typerefs(type);return type->kind==tk_struct || type->kind==tk_class || type->kind==tk_union;};
    if(record_value(t->variant.routine.return_type) && !record_value_policy(t->variant.routine.return_type,"transport"))reject("by-value return lacks safe observed transport policy");
    for(auto p=extra->param_type_list;p;p=p->next)
      if(record_value(p->type) && !record_value_policy(p->type,"argument"))reject("by-value parameter lacks safe observed transport policy");
    unsigned id=next_entity++;routine_ids[r]=id;routines.push_back(r);
    auto key=routine_linker_symbol(r);
    bool external=r->source_corresp.name_linkage==nlk_external || r->source_corresp.name_linkage==nlk_cplusplus_external;
    if(external && key.empty())reject("missing exact external identity");
    if(adapter && external)reject("default constructor adapter is not TU-private");
    bool coalescible=r->is_inline || (r->is_template_function && !r->is_specialized);
#if DO_IL_LOWERING && IA64_ABI
    coalescible=coalescible || r->use_comdat;
#endif
    entities.push_back("{\"id\":"+number(id)+",\"sourceName\":"+quote(source_spelling(&r->source_corresp))+
      ",\"linkage\":"+quote(external?"external":"internal")+",\"externalKey\":"+(external?quote(key):"null")+
      ",\"definition\":"+quote(body?(coalescible?"linkonce":"strong"):"declaration")+",\"returnTypeId\":"+number(type_id(t->variant.routine.return_type))+position(r->source_corresp.decl_position)+",\"routineTypeId\":"+number(routine_type(t,known_empty_definition))+"}");
    if(wrapper) {
#if DO_IL_LOWERING && IA64_ABI
      if(primary || adapter || !r->compiler_generated || !wrapper->target || !wrapper->overridden ||
         wrapper->target==r || wrapper->overridden==r || wrapper->target->overriding_function_for_wrapper ||
         r->overriding_function_for_wrapper!=wrapper->target || r->overridden_function_for_wrapper!=wrapper->overridden ||
         r->delta!=wrapper->delta || r->vcall_index!=wrapper->vcall_index ||
         r->return_delta!=wrapper->return_delta || r->vbase_index!=wrapper->vbase_index || extra->has_ellipsis ||
         (body && (body!=wrapper->scope || !wrapper->forwarding)))
        reject("wrapper differs from its observed nonvariadic creation/body");
      unsigned target=routine(wrapper->target),overridden=routine(wrapper->overridden);
      auto original=dot_object_source::abi_primary(wrapper->target);
      unsigned original_id=routine(original?original:wrapper->target);
      auto target_type=skip_typerefs(wrapper->target->type);
      auto overridden_type=skip_typerefs(wrapper->overridden->type);
      if(type_id(t->variant.routine.return_type)!=type_id(overridden_type->variant.routine.return_type) ||
         target_type->variant.routine.extra_info->has_ellipsis)
        reject("wrapper return/variadic contract differs from selected declarations");
      const auto slots=routine_abi_slots.at(wrapper->target);
      if(slots.empty() || std::count_if(slots.begin(),slots.end(),[](const AbiSlot &slot){return slot.role=="this";})!=1)
        reject("wrapper target lacks one original receiver");
      routine_abi_slots[r]=slots;
      unsigned ordinal=0;
      for(auto formal=extra->param_type_list;formal;formal=formal->next,++ordinal) {
        if(ordinal>=slots.size() || type_id(formal->type)!=slots[ordinal].type || slots[ordinal].role=="result-buffer")
          reject("wrapper formal differs from selected target ABI slot");
        const auto &slot=slots[ordinal];
        bool explicit_source=slot.role=="source" || slot.role=="indirect-source";
        parameters.push_back("{\"entityId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal)+
          ",\"typeId\":"+number(slot.type)+",\"parameterRole\":"+quote(explicit_source?"forwarded-source":slot.role)+
          ",\"sourceOrdinal\":"+(explicit_source?std::to_string(slot.source_ordinal):"null")+"}");
      }
      if(ordinal!=slots.size() || (body && wrapper->parameters.size()!=slots.size()))
        reject("wrapper parameter count differs from observed target/body");
      thunk_wrappers.push_back("{\"entityId\":"+number(id)+",\"targetEntityId\":"+number(target)+
        ",\"overriddenEntityId\":"+number(overridden)+",\"primaryEntityId\":"+number(original_id)+
        ",\"thisDelta\":"+std::to_string(wrapper->delta)+",\"vcallIndex\":"+std::to_string(wrapper->vcall_index)+
        ",\"returnDelta\":"+std::to_string(wrapper->return_delta)+",\"vbaseIndex\":"+std::to_string(wrapper->vbase_index)+
        ",\"captureProof\":\"observed-wrapper-creation\"}");
      return id;
#else
      reject("wrapper requires observed IA64 ABI");
#endif
    }
    auto generated=dot_object_source::state().generated_initializers.find(r);
    auto static_destructor=dot_object_source::state().generated_static_destructors.find(r);
    if(static_destructor!=dot_object_source::state().generated_static_destructors.end()) {
      const auto &origin=static_destructor->second;
      auto formal=extra->param_type_list;
      if(primary || adapter || !origin.complete || !origin.error.empty() || !r->compiler_generated || !body ||
         body!=origin.scope || extra->has_ellipsis || !formal || formal->next || type_id(formal->type)!=pointer_type(1) ||
         !origin.parameter || origin.parameter->next || body->variant.routine.parameters!=origin.parameter ||
         skip_typerefs(t->variant.routine.return_type)->kind!=tk_void)
        reject("generated static destructor outside observed void(void*) callback contract");
      routine_abi_slots[r]={{"static-object",0,pointer_type(1)}};
      parameters.push_back("{\"entityId\":"+number(id)+",\"ordinal\":0,\"typeId\":"+number(pointer_type(1))+
        ",\"parameterRole\":\"static-object\",\"sourceOrdinal\":null}");
      return id;
    }
    if(generated!=dot_object_source::state().generated_initializers.end()) {
      const auto &origin=generated->second;
      if(primary || adapter || !origin.complete || !origin.error.empty() || !r->compiler_generated || !body ||
         origin.is_thread_local || origin.needed_bit || extra->param_type_list || extra->has_ellipsis ||
         skip_typerefs(t->variant.routine.return_type)->kind!=tk_void ||
         body->variant.routine.parameters || coalescible)
        reject("generated initializer outside observed ordinary void() startup contract");
      generated_routine_origins.push_back("{\"entityId\":"+number(id)+
        ",\"kind\":\"file-scope-initializer\",\"captureProof\":\"observed-file-scope-init-creation\",\"failurePolicy\":\"terminate\"}");
      routine_abi_slots[r]={};
      return id;
    }
    if(primary) {
#if DO_IL_LOWERING && IA64_ABI
      unsigned primary_id=routine(primary);
      unsigned target_id=adapter?routine(adapter->target):0;
      abi_routine_origins.push_back("{\"entityId\":"+number(id)+",\"primaryEntityId\":"+number(primary_id)+
        ",\"entryKind\":"+quote(entry_kind)+",\"isAliasEntry\":"+(r->is_alias_entry?"true":"false")+
        ",\"captureProof\":"+quote(adapter?"observed-default-wrapper-creation":"primary-ctor-dtor-link")+
        ",\"targetEntityId\":"+number(target_id)+"}");
#endif
    } else source_function(r,id);
    unsigned ordinal=0;
    auto source_function_it=dot_object_source::state().functions.find(primary?primary:r);
    if(source_function_it==dot_object_source::state().functions.end())reject("missing routine ABI source snapshot");
    const auto &source_function_value=source_function_it->second;
    const auto &source_signature=dot_object_source::state().types.at(source_function_value.signature_type-1);
    bool has_this=source_signature.object_type!=0;
    if(!adapter && has_this!=(extra->this_class!=nullptr))reject("source/execution receiver disagreement");
    auto slots=source_abi_slots(source_function_value.signature_type);
#if DO_IL_LOWERING && IA64_ABI
    if(primary && !adapter && r->ctor_dtor_kind!=cdk_subobject && r->ctor_dtor_kind!=cdk_delegation)
      slots.erase(std::remove_if(slots.begin(),slots.end(),[](const AbiSlot &slot){return slot.role=="vtt";}),slots.end());
#endif
    if(adapter) {
      slots.erase(std::remove_if(slots.begin(),slots.end(),[](const AbiSlot &slot){return slot.role!="this";}),slots.end());
      if(slots.size()!=1)reject("default adapter lacks its original receiver ABI");
    }
    const auto &abi=source_abi(source_function_value.signature_type);
    unsigned expected_return=abi.result_buffer?1:source_execution_type(source_signature.result);
    if(type_id(t->variant.routine.return_type)!=expected_return)
      reject("source/execution return ABI representation mismatch");
    routine_abi_slots[r]=slots;
    for(auto p=extra->param_type_list;p;p=p->next,++ordinal) {
      if(ordinal>=slots.size())reject("unclassified extra execution ABI parameter");
      const auto &slot=slots[ordinal];
      if(type_id(p->type)!=slot.type)reject("source/execution parameter ABI representation mismatch");
      bool explicit_source=slot.role=="source" || slot.role=="indirect-source";
      parameters.push_back("{\"entityId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal)+",\"typeId\":"+number(type_id(p->type))+
        ",\"parameterRole\":"+quote(primary && explicit_source?"forwarded-source":slot.role)+",\"sourceOrdinal\":"+
        (explicit_source?std::to_string(slot.source_ordinal):"null")+"}");
    }
    if(ordinal!=slots.size())
      reject((string("missing source/receiver ABI parameters in ")+source_spelling(&r->source_corresp)+" ["+key+"] execution="+
        std::to_string(ordinal)+" expected="+std::to_string(slots.size())).c_str());
    return id;
  }
  unsigned binding(a_variable_ptr v,unsigned scope,bool parameter=false,unsigned ordinal=0) {
    auto prior=binding_ids.find(v);if(prior!=binding_ids.end())return prior->second;
    diagnostic_scope diagnostic(v->source_corresp.decl_position);
    if(thread_storage(v) || v->storage_class==sc_static || v->storage_class==sc_extern)reject("nonautomatic storage");
    auto object_type=skip_typerefs(v->type);
    if(object_type->kind==tk_array && !object_type->size)
      reject("automatic zero-sized/incomplete array storage needs an owned allocation contract");
    if(v->init_kind!=initk_none && v->init_kind!=initk_dynamic &&
       !(v->init_kind==initk_zero && (skip_typerefs(v->type)->kind==tk_array || skip_typerefs(v->type)->kind==tk_struct)))
      reject("nonexecutably placed local initializer",v->init_kind);
    unsigned id=next_binding++;binding_ids[v]=id;binding_scopes[v]=scope;
    auto source=dot_object_source::state().variables.find(v);
    bool synthetic=false;
    auto primary=dot_object_source::abi_primary(current_routine);
    auto wrapper_it=dot_object_source::state().thunk_wrappers.find(current_routine);
    const dot_object_source::ThunkWrapper *wrapper=wrapper_it==dot_object_source::state().thunk_wrappers.end()?nullptr:&wrapper_it->second;
    if(parameter) {
      if(!current_routine || !routine_abi_slots.count(current_routine) || ordinal>=routine_abi_slots.at(current_routine).size())
        reject("parameter outside observed routine ABI");
      const auto &slot=routine_abi_slots.at(current_routine)[ordinal];
      auto function_scope=scope_for_routine_or_null(current_routine);
      if(!function_scope)reject("parameter lacks its execution function scope");
      auto formal=skip_typerefs(current_routine->type)->variant.routine.extra_info->param_type_list;
      auto actual=function_scope->variant.routine.parameters;
      for(unsigned i=0;i<ordinal && formal && actual;++i){formal=formal->next;actual=actual->next;}
      if(actual!=v || !formal || type_id(v->type)!=slot.type || type_id(formal->type)!=slot.type)
        reject("execution parameter identity/type disagrees with observed ABI slot");
      if(wrapper) {
        if(function_scope!=wrapper->scope || ordinal>=wrapper->parameters.size() || wrapper->parameters[ordinal]!=v ||
           source!=dot_object_source::state().variables.end() || v->is_this_parameter!=(slot.role=="this"))
          reject("wrapper formal lacks observed parameter identity/role");
        synthetic=true;
      } else if(slot.role=="static-object") {
        auto generated=dot_object_source::state().generated_static_destructors.find(current_routine);
        if(generated==dot_object_source::state().generated_static_destructors.end() ||
           generated->second.parameter!=v || source!=dot_object_source::state().variables.end() || ordinal)
          reject("generated static destructor parameter lacks observed identity");
        synthetic=true;
      } else if(primary) {
        if(source!=dot_object_source::state().variables.end() || slot.role=="result-buffer")
          reject("generated alternate parameter has unproved source/result origin");
        if(slot.role=="this" ? (!v->is_this_parameter || function_scope->variant.routine.this_param_variable!=v) :
           (v->is_this_parameter || v->variant.assoc_param_type!=formal))
          reject("generated alternate parameter lacks exact forwarding/receiver proof");
        synthetic=true;
      } else {
        const auto &original=dot_object_source::state().functions.at(current_routine);
        if(slot.role=="this") {
          if(!v->is_this_parameter || !original.scope_captured || original.implicit_this!=v ||
             function_scope->variant.routine.this_param_variable!=v || source!=dot_object_source::state().variables.end())
            reject("implicit receiver lacks original scope identity proof");
          synthetic=true;
        } else if(slot.role=="vtt") {
          auto vtt=dot_object_source::state().vtt_parameters.find(current_routine);
          if(vtt==dot_object_source::state().vtt_parameters.end() || vtt->second!=v ||
             source!=dot_object_source::state().variables.end() || !(v->compiler_generated || v->lowering_generated))
            reject("VTT parameter lacks observed lowering variable identity");
          synthetic=true;
        } else if(slot.role=="result-buffer") {
          auto result=dot_object_source::state().result_parameters.find(current_routine);
          if(result==dot_object_source::state().result_parameters.end() || result->second!=v ||
             source!=dot_object_source::state().variables.end() || !(v->compiler_generated || v->lowering_generated))
            reject("result buffer lacks observed lowering variable identity");
          synthetic=true;
        } else if(!original.scope_captured || slot.source_ordinal>=original.parameter_variables.size() ||
                  original.parameter_variables[slot.source_ordinal]!=v || v->is_this_parameter) {
          reject("original parameter identity/order changed during lowering");
        }
      }
    }
    if((primary || wrapper) && !parameter) {
      if(source!=dot_object_source::state().variables.end() || !(v->compiler_generated || v->lowering_generated))
        reject("alternate local lacks generated provenance");
      synthetic=true;
    }
    if(synthetic) {
      // The implicit object is an explicit ABI parameter, not a source declaration.
    } else if(source!=dot_object_source::state().variables.end()) {
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
      unsigned declaration=node("declare",0,v->source_corresp.decl_position,0,id);
      edge(block,"statement",ordinal++,declaration);
      if(block==owned_function_root)generated_scope_scaffold(declaration,id,"declaration");
      // lower_init marks partially dynamic aggregates initk_zero, leaving
      // explicit element stores in the statement stream. As c_gen_be's
      // dump_variable_decl does, zero this storage at lexical scope entry.
      if(v->init_kind==initk_zero) {
        unsigned initialization=initialize_aggregate(v,nullptr,v->source_corresp.decl_position);
        edge(block,"statement",ordinal++,initialization);
        if(block==owned_function_root)generated_scope_scaffold(initialization,id,"zero-initialization");
      }
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
    if(c->kind==ck_void) {
      if(ty!=1)reject("folded void constant has nonvoid type");
      // Stock il_to_str renders a C++14 folded void value as ((void)0).
      unsigned zero=node("constant",2,pos,0,0,"","0","value","none");
      unsigned discarded=node("convert",ty,pos,0,0,"","","value","left-to-right");
      edge(discarded,"operand",0,zero);return discarded;
    }
    if(c->kind==ck_aggregate) {
      auto type=skip_typerefs(c->type);
      if((type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union) || !record_value_policy(type,"construction"))
        reject("folded aggregate constant outside complete constructible record");
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
    if(e && e->kind==enk_object_lifetime)return place(e->variant.object_lifetime.expr);
    if(e && e->kind==enk_operation && e->variant.operation.kind==eok_comma) {
      auto last=e->variant.operation.operands;if(!last)return false;
      while(last->next)last=last->next;
      // Execution addressability: source rvalues may already reside in a
      // lowered temporary. Prefix effects do not copy that final storage.
      return il_identical_types(skip_typerefs(e->type),skip_typerefs(last->type)) && place(last);
    }
    return e && (e->kind==enk_variable ||
      (e->kind==enk_constant && node_constant(e)->kind==ck_string) ||
      (e->kind==enk_operation && (e->variant.operation.kind==eok_indirect || e->variant.operation.kind==eok_ref_indirect ||
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
      case eok_indirect:case eok_ref_indirect:return pointee_volatile(a);
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
    bool mathematical_overflow=operation=="add-overflow" || operation=="sub-overflow" || operation=="mul-overflow";
    if(integer128(call->type))reject("integer128 intrinsic result");
    for(auto arg=first;arg;arg=arg->next)
      if(integer128(arg->type) && !mathematical_overflow)reject("integer128 intrinsic operand");
    if(mathematical_overflow) {
      if(!first || !first->next || !first->next->next || first->next->next->next)reject("overflow intrinsic arity");
      auto target=skip_typerefs(first->next->next->type);
      if(target->kind!=tk_pointer || skip_typerefs(target->variant.pointer.type)->kind!=tk_integer)
        reject("overflow intrinsic destination");
      auto qualifiers=get_type_qualifiers(target->variant.pointer.type);
      if(qualifiers&(TQ_CONST|TQ_C11_ATOMIC))reject("overflow intrinsic destination qualifiers");
      if(qualifiers&TQ_VOLATILE) {
        if(integer128(target->variant.pointer.type))reject("volatile integer128 overflow intrinsic destination");
        operation+="-volatile";
      }
    }
    unsigned id=node("intrinsic",type_id(call->type),call->position,0,0,operation,"","value",order),ordinal=0;
    for(auto arg=first;arg;arg=arg->next)edge(id,"argument",ordinal++,expression(arg));
    return id;
  }
  unsigned expression(an_expr_node_ptr e) {
    unsigned result=expression_core(e);
    auto array=active_array_runtime.find(e);
    if(array!=active_array_runtime.end()) {
      auto original=array->second.first;const auto &observed=*array->second.second;
      bool is_static=dot_object_source::state().generated_static_destructors.count(current_routine)!=0;
      unsigned source=is_static?0:source_obligation_ids.emplace(original,unsigned(source_obligation_ids.size()+1)).first->second;
      array_runtime_operations.push_back("{\"nodeId\":"+number(result)+",\"sourceObligationId\":"+number(source)+
        ",\"sourceStaticObligationId\":"+number(is_static?static_obligation_id(original):0)+
        ",\"callbackEntityId\":"+number(routine(observed.callback))+",\"operationKind\":"+quote(observed.operation)+
        ",\"captureProof\":\"observed-array-runtime\"}");
      if(!is_static)emitted_array_obligations.insert(original);
    }
    auto normal=active_normal_calls.find(e);
    if(normal!=active_normal_calls.end()) {
      auto original=normal->second;
      bool is_static=dot_object_source::state().generated_static_destructors.count(current_routine)!=0;
      unsigned source=is_static?0:source_obligation_ids.emplace(original,unsigned(source_obligation_ids.size()+1)).first->second;
      normal_destructions.push_back("{\"nodeId\":"+number(result)+",\"sourceObligationId\":"+number(source)+
        ",\"sourceStaticObligationId\":"+number(is_static?static_obligation_id(original):0)+
        ",\"captureProof\":\"observed-normal-destruction\"}");
      if(!is_static)emitted_normal_obligations.insert(original);
    }
    // Transparent lifetime/sequence wrappers already own their final operand,
    // including its volatile-place marker. Do not wrap the same access twice.
    bool transparent=e && (e->kind==enk_object_lifetime ||
      (e->kind==enk_operation && e->variant.operation.kind==eok_comma));
    if(e && !transparent && place(e) && volatile_place(e)) {
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
    auto adjustment=dot_object_source::state().virtual_base_adjustments.find(e);
    if(adjustment!=dot_object_source::state().virtual_base_adjustments.end()) {
      const auto &observed=adjustment->second;
      if(observed.owner!=current_routine || !observed.source_object || !observed.lookup_object || !observed.offset ||
         observed.source_object==e || observed.offset==e || skip_typerefs(e->type)->kind!=tk_pointer ||
         !emitted_expression_ids.count(observed.source_object) || !emitted_expression_ids.count(observed.lookup_object) ||
         !emitted_expression_ids.count(observed.offset))
        reject((string("virtual base adjustment lost exact correspondence: owner=")+std::to_string(observed.owner==current_routine)+
          ", source="+std::to_string(observed.source_object && emitted_expression_ids.count(observed.source_object))+
          ", lookup="+std::to_string(observed.lookup_object && emitted_expression_ids.count(observed.lookup_object))+
          ", offset="+std::to_string(observed.offset && emitted_expression_ids.count(observed.offset))+
          ", expression-kind="+std::to_string(int(e->kind))+
          ", operation="+std::to_string(e->kind==enk_operation?int(e->variant.operation.kind):-1)).c_str());
      unsigned wrapper=node("virtual-base-adjustment",type_id(e->type),e->position,0,0,"","","value","none");
      edge(wrapper,"operand",0,result);
      auto base=skip_typerefs(observed.base_type),derived=skip_typerefs(observed.derived_type);
      auto &views=dot_object_source::state().observed_subobjects;
      if(views.count(base))base=views.at(base);if(views.count(derived))derived=views.at(derived);
      virtual_base_adjustments.push_back("{\"nodeId\":"+number(wrapper)+",\"operandNodeId\":"+number(result)+
        ",\"sourceNodeId\":"+number(emitted_expression_ids.at(observed.source_object))+
        ",\"lookupSourceNodeId\":"+number(emitted_expression_ids.at(observed.lookup_object))+
        ",\"derivedTypeId\":"+number(type_id(derived))+",\"completeBaseTypeId\":"+number(type_id(base))+
        ",\"offsetNodeId\":"+number(emitted_expression_ids.at(observed.offset))+
        ",\"vtableIndex\":"+std::to_string(observed.vtable_index)+",\"isCompleteObject\":false,\"captureProof\":\"observed-virtual-base-adjustment\"}");
      result=wrapper;
    }
    emitted_expression_ids[e]=result;
    return result;
  }
  unsigned exception_descriptor(const dot_object_source::ThrowSnapshot &saved) {
    if(!saved.error.empty())reject(saved.error.c_str());
    auto prior=exception_type_ids.find(saved.source_type);
    if(prior!=exception_type_ids.end()) {
      if(exception_source_destructors.at(saved.source_type)!=saved.destructor)
        reject("inconsistent observed source exception destructor");
      return prior->second;
    }
    if(!saved.size || !saved.alignment || saved.alignment>4096 ||
       (saved.alignment&(saved.alignment-1)))reject("invalid observed exception payload storage");
    auto payload=skip_typerefs(saved.payload_type);
    if(payload->size!=saved.size || payload->alignment!=saved.alignment)
      reject("exception payload layout changed after source observation");
    unsigned id=unsigned(exception_type_ids.size()+1);exception_type_ids[saved.source_type]=id;
    exception_source_destructors[saved.source_type]=saved.destructor;
    unsigned source=source_type(saved.source_type),leaf=source_type(saved.leaf_source_type);
    unsigned source_destructor=saved.destructor?routine(saved.destructor):0;
    exception_types.push_back("{\"id\":"+number(id)+",\"sourceTypeId\":"+number(source)+
      ",\"leafSourceTypeId\":"+number(leaf)+",\"payloadTypeId\":"+number(type_id(saved.payload_type))+
      ",\"payloadSize\":"+std::to_string(saved.size)+",\"payloadAlignment\":"+std::to_string(saved.alignment)+
      ",\"sourceDestructorEntityId\":"+number(source_destructor)+
      ",\"captureProof\":\"before-throw-lowering\"}");
    auto type=saved.leaf_type;
    if(dot_object_source::state().types.at(saved.leaf_source_type-1).kind=="record") {
      auto found=dot_object_source::state().prelower_records.find(type);
      if(found==dot_object_source::state().prelower_records.end() || !found->second.complete)
        reject("exception leaf lacks complete original record layout");
      unsigned ordinal=0;
      for(const auto &base:found->second.exception_bases) {
        if(base.is_virtual)reject("virtual exception base conversion not yet owned");
        if(base.offset>found->second.size)reject("exception base outside complete leaf storage");
        exception_base_conversions.push_back("{\"exceptionTypeId\":"+number(id)+",\"ordinal\":"+
          std::to_string(ordinal++)+",\"baseSourceTypeId\":"+number(source_type(base.source_type))+
          ",\"byteOffset\":"+std::to_string(base.offset)+",\"isPublic\":"+(base.is_public?"true":"false")+
          ",\"isAmbiguous\":"+(base.is_ambiguous?"true":"false")+
          ",\"captureProof\":\"observed-complete-base-layout\"}");
      }
    }
    return id;
  }
  a_routine_ptr complete_exception_destructor(a_routine_ptr primary) {
    if(!exception_destructors_indexed) {
      for(auto r=il_header.primary_scope->routines;r;r=r->next)if(r->ctor_dtor_kind==cdk_complete && r->special_kind==sfk_destructor) {
        auto origin=dot_object_source::abi_primary(r);if(!origin)origin=r;
        if(!complete_exception_destructors.emplace(origin,r).second)
          reject("duplicate complete exception destructor entry");
      }
      exception_destructors_indexed=true;
    }
    auto found=complete_exception_destructors.find(primary);
    if(found==complete_exception_destructors.end())reject("exception destructor lacks observed complete ABI entry");
    auto result=found->second;auto type=skip_typerefs(result->type);
    auto param=type->variant.routine.extra_info->param_type_list;
    if(!param || param->next || skip_typerefs(param->type)->kind!=tk_pointer ||
       skip_typerefs(type->variant.routine.return_type)->kind!=tk_void)
      reject("exception destructor outside one-pointer void ABI");
    return result;
  }
  unsigned throw_expression(an_expr_node_ptr expr) {
    auto found=dot_object_source::state().throws.find(expr);
    if(found==dot_object_source::state().throws.end())reject("throw lacks original lowering observation");
    const auto &saved=found->second;
    if(!saved.complete || saved.owner!=current_routine || type_id(expr->type)!=1)
      reject("throw ownership/type/lowering proof changed");
    if(!saved.error.empty())reject(saved.error.c_str());
    auto supplement=expr->variant.throw_info;
    if(saved.rethrow) {
      if(supplement)reject("rethrow unexpectedly has payload");
      unsigned id=node("rethrow",1,expr->position,0,0,"","","value","none");
      rethrow_expressions.push_back("{\"nodeId\":"+number(id)+",\"captureProof\":\"observed-rethrow\"}");return id;
    }
    if(!supplement || supplement->dynamic_init || supplement->expr!=saved.initializer || supplement->type!=saved.payload_type)
      reject("throw payload/initializer observation changed");
    auto current_destructor=supplement->destructor;
    if(current_destructor) {auto primary=dot_object_source::abi_primary(current_destructor);if(primary)current_destructor=primary;}
    if(current_destructor!=saved.destructor)reject("throw source destructor observation changed");
    unsigned descriptor=exception_descriptor(saved),destructor=0;
    if(saved.destructor) {
      auto type=skip_typerefs(saved.payload_type);
      if(type->kind!=tk_struct && type->kind!=tk_class && type->kind!=tk_union)
        reject("nonrecord exception payload destructor");
      destructor=routine(complete_exception_destructor(saved.destructor));
    }
    unsigned id=node("throw",1,expr->position,0,0,"","","value","left-to-right");
    bool owns_subobjects=std::any_of(active_payload_subobjects.begin(),active_payload_subobjects.end(),
      [&](const auto &entry){return entry.second->throw_origin==saved.original;});
    if(owns_subobjects && !active_throw_ids.emplace(saved.original,id).second)
      reject("multiple emitted throws share one cleanup origin");
    throw_expressions.push_back("{\"nodeId\":"+number(id)+",\"exceptionTypeId\":"+number(descriptor)+
      ",\"destructorEntityId\":"+number(destructor)+
      ",\"initializationKind\":"+quote(saved.initializer?"expression":"empty")+
      ",\"captureProof\":\"observed-throw-lowering\"}");
    // The entry cursor is a runtime boundary, not an inferred source scope.
    // Locals activated by this initializer unwind before its payload is freed;
    // the enclosing function retains ownership of the predecessor chain.
    throw_initialization_boundaries.push_back("{\"throwNodeId\":"+number(id)+
      ",\"cleanupPolicy\":\"unwind-to-entry-before-release\",\"captureProof\":\"observed-throw-initializer\"}");
    auto old_origin=active_throw_origin;auto old_node=active_throw_node;
    active_throw_origin=saved.original;active_throw_node=id;
    if(saved.initializer)edge(id,"initializer",0,expression(saved.initializer));
    active_throw_origin=old_origin;active_throw_node=old_node;
    return id;
  }
  unsigned throw_construct(an_expr_node_ptr expr) {
    const bool marker=expr->variant.lowered_eh.kind==leck_exception_started;
    const auto &map=marker?dot_object_source::state().throw_markers:dot_object_source::state().throw_addresses;
    auto found=map.find(expr);
    if(found==map.end() || !active_throw_node || found->second!=active_throw_origin)
      reject("throw address/start lacks exact enclosing initializer ownership");
    unsigned type=type_id(expr->type);
    if(marker ? type!=1 : type!=pointer_type(1))reject("throw address/start type mismatch");
    unsigned id=node(marker?"throw-marker":"thrown-object-address",type,expr->position,0,0,"","","value","none");
    if(marker)throw_markers.push_back("{\"nodeId\":"+number(id)+",\"throwNodeId\":"+number(active_throw_node)+
      ",\"kind\":\"started\",\"captureProof\":\"observed-throw-start\"}");
    else throw_object_addresses.push_back("{\"nodeId\":"+number(id)+",\"throwNodeId\":"+number(active_throw_node)+
      ",\"captureProof\":\"observed-thrown-object-address\"}");
    return id;
  }
  unsigned expression_core(an_expr_node_ptr e) {
    if(!e)reject("null expression");
    diagnostic_scope diagnostic(e->position);
    auto registration=dot_object_source::state().static_registrations.find(e);
    if(registration!=dot_object_source::state().static_registrations.end() && e->kind==enk_operation &&
       e->variant.operation.kind==eok_call) {
      auto &captured=dot_object_source::state();const auto &observed=registration->second;
      if(!observed.error.empty())reject(observed.error.c_str());
      const auto &original=captured.static_destructions.at(observed.original);
      if(observed.owner!=current_routine || observed.thread || original.thread ||
         observed.variable!=original.variable || !original.initializer || type_id(e->type)!=2)
        reject("static registration changed original owner, process domain or storage");
      auto generated=captured.generated_static_destructors.find(observed.callback);
      auto primary=dot_object_source::abi_primary(observed.callback);if(!primary)primary=observed.callback;
      if(generated==captured.generated_static_destructors.end()?primary!=original.selected_routine:
         generated->second.original!=observed.original)
        reject("static registration callback differs from original selected destructor");
      auto callback=e->variant.operation.operands->next;
      auto object=callback?callback->next:nullptr;auto dso=object?object->next:nullptr;
      if(!callback || !object || !dso || dso->next)reject("observed static registration lost its three arguments");
      unsigned id=node("static-destruction-register",2,e->position,0,0,"","","value","left-to-right");
      edge(id,"callback",0,expression(callback));unsigned object_id=expression(object);edge(id,"object",0,object_id);
      static_destruction_registrations.push_back("{\"nodeId\":"+number(id)+",\"sourceObligationId\":"+number(static_obligation_id(observed.original))+
        ",\"sourceInitializerId\":"+number(source_initializer_id(original.initializer))+",\"functionEntityId\":"+number(function)+
        ",\"callbackEntityId\":"+number(routine(observed.callback))+",\"objectNodeId\":"+number(object_id)+
        ",\"domain\":\"process\",\"captureProof\":\"observed-static-destruction-registration\"}");
      if(!registered_static_obligations.insert(observed.original).second)
        reject("original persistent destruction registered at multiple emitted sites");
      return id;
    }
    auto guard=dot_object_source::state().local_static_guard_operations.find(e);
    if(guard!=dot_object_source::state().local_static_guard_operations.end() && e->kind==enk_operation &&
       e->variant.operation.kind==(guard->second.second=="probe"?eok_eq:eok_call))
      return guard_operation(e,guard->second.first,guard->second.second);
    unsigned ty=type_id(e->type);
    if(e->kind==enk_result_of_overriding_function) {
      auto found=dot_object_source::state().thunk_wrappers.find(current_routine);
      if(found==dot_object_source::state().thunk_wrappers.end() || found->second.forwarding!=e ||
         !found->second.scope || found->second.scope!=scope_for_routine_or_null(current_routine))
        reject("wrapper result lacks its observed forwarding expression/body");
      const auto &wrapper=found->second;
      auto target_type=skip_typerefs(wrapper.target->type);
      if(ty!=type_id(target_type->variant.routine.return_type) || target_type->variant.routine.extra_info->has_ellipsis)
        reject("wrapper forwarding result/variadic contract mismatch");
      unsigned id=node("call",ty,e->position,routine(wrapper.target),0,"","","value","left-to-right"),ordinal=0;
      for(auto parameter:wrapper.parameters) {
        if(!binding_ids.count(parameter))reject("wrapper forwarding parameter lacks owned binding");
        edge(id,"argument",ordinal++,local_place(parameter,e->position));
      }
      wrapper_calls.push_back("{\"nodeId\":"+number(id)+",\"wrapperEntityId\":"+number(function)+
        ",\"captureProof\":\"observed-wrapper-forwarding\"}");
      return id;
    }
    if(e->kind==enk_object_lifetime) {
      auto &wrapper=e->variant.object_lifetime;
      if(!wrapper.expr || !wrapper.ptr || !il_identical_types(e->type,wrapper.expr->type))
        reject("object lifetime wrapper/operand type mismatch");
      auto original=dot_object_source::source_lifetime(wrapper.ptr,current_routine);
      if(!original || dot_object_source::state().lifetimes.at(original).kind!="expression-temporary")
        reject("object lifetime wrapper lacks exact original temporary boundary");
      check_lifetime(wrapper.ptr);
      unsigned operand=expression(wrapper.expr);
      const auto shape=node_shapes.at(operand);
      if(shape.type!=ty)reject("object lifetime operand execution type changed");
      unsigned result=node("lifetime-scope",ty,e->position,0,0,"","",shape.place?"place":"value","none");
      edge(result,"operand",0,operand);
      auto source=source_lifetime_ids.emplace(original,unsigned(source_lifetime_ids.size()+1)).first->second;
      expression_lifetimes.push_back("{\"nodeId\":"+number(result)+",\"sourceLifetimeId\":"+number(source)+
        ",\"captureProof\":\"observed-lifetime-wrapper\"}");
      active_expression_lifetimes=true;return result;
    }
    if(e->kind==enk_throw)return throw_expression(e);
    if(e->kind==enk_lowered_eh_construct) {
      if(e->variant.lowered_eh.kind==leck_thrown_object_address ||
         e->variant.lowered_eh.kind==leck_exception_started)return throw_construct(e);
      return cleanup_marker(e);
    }
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
      }
      unsigned owner=type_id(base);auto field=node_field(b);
      if(!field_ids.count(field) || parent_class_of(field)!=base || type_id(field->type)!=ty)
        reject("record member identity mismatch");
      unsigned operand=expression(a);
      if(operation.kind==eok_points_to_field) {
        unsigned indirect=node("dereference",owner,a->position,0,0,"","","place","none");
        edge(indirect,"operand",0,operand);operand=indirect;
      } else if(!node_shapes.at(operand).place) {
        // The stock C backend materializes genuine record values before
        // selecting a field. This is execution storage, not an extra C++
        // construction: existing source lifetime/cleanup nodes remain owners.
        if(node_shapes.at(operand).type!=owner ||
           (base->kind!=tk_struct && base->kind!=tk_class && base->kind!=tk_union))
          reject("record member materialization type mismatch");
        if(!record_value_policy(a->type,"transport"))reject("record member materialization lacks safe observed transport policy");
        unsigned temporary=node("materialize",owner,a->position,0,0,"","","place","left-to-right");
        edge(temporary,"operand",0,operand);operand=temporary;
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
        if(formal) {
          // Lowered runtime calls can retain a typed data pointer for a void*
          // formal. Stock C supplies this standard conversion at the call.
          // Polymorphic GNU atomics additionally retain scalar bool arguments.
          // Own these conversions; never weaken the typed call verifier.
          auto target=skip_typerefs(formal->type),source=skip_typerefs(arg->type);
          if(builtin && target->kind==tk_integer && target->variant.integer.bool_type &&
             !(source->kind==tk_integer && source->variant.integer.bool_type)) {
            if(source->kind!=tk_integer && source->kind!=tk_float && source->kind!=tk_pointer)
              reject("builtin bool argument outside scalar domain");
            unsigned converted=node("binary",type_id(formal->type),arg->position,0,0,"ne","","value","left-to-right");
            edge(converted,"left",0,argument);edge(converted,"right",0,zero_node(type_id(arg->type),arg->position));argument=converted;
          } else if(target->kind==tk_pointer && source->kind==tk_pointer &&
                    skip_typerefs(target->variant.pointer.type)->kind==tk_void &&
                    skip_typerefs(source->variant.pointer.type)->kind!=tk_routine &&
                    type_id(formal->type)!=type_id(arg->type)) {
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
    if(operation.kind==eok_bassign) {
      if(!a || !b || b->next || !place(a) || !place(b) || ty!=1 || volatile_place(a) || volatile_place(b))
        reject("block copy outside nonvolatile addressable places");
      auto source=skip_typerefs(b->type),target=skip_typerefs(a->type);
      if(!source->size || source->size>target->size)reject("block copy extent outside destination");
      unsigned id=node("block-copy",1,e->position,0,0,"",std::to_string(source->size),"value",order);
      edge(id,"target",0,expression(a));edge(id,"source",0,expression(b));return id;
    }
    if(operation.kind==eok_assign) {
      if(!a || !b || b->next || !place(a))reject("assignment outside supported memory places");
      auto type=skip_typerefs(e->type);
      bool record=type->kind==tk_struct || type->kind==tk_class || type->kind==tk_union;
      auto observed=dot_object_source::state().record_transfers.find(e);
      if(record && observed!=dot_object_source::state().record_transfers.end()) {
        const auto &proof=observed->second;
        if(proof.owner!=current_routine || proof.target!=a || proof.value!=b ||
           volatile_place(a) || volatile_place(b))reject("record transfer observation changed");
        if(proof.kind=="temporary" && (a->kind!=enk_variable ||
           !(node_variable(a)->compiler_generated || node_variable(a)->lowering_generated)))
          reject("observed temporary transfer lacks generated destination");
        if(proof.kind=="construction" && proof.constant_storage) {
          auto pooled=dot_object_source::state().pooled_record_constants.find(proof.constant_storage);
          if(b->kind!=enk_variable || node_variable(b)!=proof.constant_storage ||
             pooled==dot_object_source::state().pooled_record_constants.end() ||
             type_id(a->type)!=ty || type_id(b->type)!=ty || contains_volatile_storage(a->type) ||
             !source_record_ids.count(type) ||
             (proof.constant_initialization_kind!="zero" && proof.constant_initialization_kind!="constant"))
            reject("record constant initialization lost original constant/storage correspondence");
          unsigned source_storage=storage(proof.constant_storage);
          unsigned id=node("record-constant-init",ty,e->position,0,0,"","","value",order);
          edge(id,"target",0,expression(a));edge(id,"value",0,expression(b));
          record_constant_initializations.push_back("{\"nodeId\":"+number(id)+
            ",\"sourceRecordId\":"+number(source_record_ids.at(type))+
            ",\"storageId\":"+number(source_storage)+",\"initializationKind\":"+quote(proof.constant_initialization_kind)+
            ",\"captureProof\":\"observed-pooled-record-initialization\"}");
          return id;
        }
        if(!record_value_policy(e->type,proof.kind)) {
          auto original=dot_object_source::state().prelower_records.find(type);
          string detail="record transfer policy: operation="+proof.kind+", target-kind="+std::to_string(int(a->kind))+
            ", rhs-kind="+std::to_string(int(b->kind))+", type="+source_spelling(&type->source_corresp);
          if(a->kind==enk_variable)detail+=", target="+source_spelling(&node_variable(a)->source_corresp);
          if(a->kind==enk_operation)detail+=", target-op="+std::to_string(int(a->variant.operation.kind));
          if(b->kind==enk_variable)detail+=", rhs-variable="+source_spelling(&node_variable(b)->source_corresp)+
            ", rhs-init="+std::to_string(int(node_variable(b)->init_kind))+
            ", rhs-generated="+std::to_string(node_variable(b)->compiler_generated || node_variable(b)->lowering_generated);
          if(b->kind==enk_constant)detail+=", constant-kind="+std::to_string(int(node_constant(b)->kind));
          if(original!=dot_object_source::state().prelower_records.end())detail+=
            ", construct="+std::to_string(original->second.bitwise_construction)+
            ", assign="+std::to_string(original->second.bitwise_assignment)+
            ", result-storage="+std::to_string(original->second.requires_return_storage)+
            ", parameter-storage="+std::to_string(original->second.requires_parameter_storage);
          reject(detail.c_str());
        }
        unsigned id=record_transfer(e->type,e->position,order,proof.kind);
        edge(id,"target",0,expression(a));edge(id,"value",0,expression(b));return id;
      }
      if(record && (!copyable_value(e->type) || volatile_place(a) || volatile_place(b)))
        reject("record assignment requires ordinary copy policy or exact construction observation");
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
    if((operation.kind==eok_indirect || operation.kind==eok_ref_indirect) && a && !b && function_pointer(a->type)) {
      // C's *function_pointer designates that same function. EDG can retain
      // its decayed pointer result, so inspect the operand as the existing dot
      // backend does, not only whether the result still has tk_routine kind.
      unsigned source=type_id(a->type);
      if(ty!=source && pointer_type(ty)!=source)reject("function designator representation mismatch");
      return expression(a);
    }
    if((operation.kind==eok_address_of || operation.kind==eok_reference_to) && a && !b && a->kind==enk_routine)
      return node("function-address",ty,e->position,routine(a->variant.routine.ptr),0,"","","value","none");
    if((operation.kind==eok_address_of || operation.kind==eok_reference_to) && a && !b && skip_typerefs(a->type)->kind==tk_routine)
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
    if(operation.kind==eok_address_of || operation.kind==eok_reference_to ||
       operation.kind==eok_indirect || operation.kind==eok_ref_indirect) {
      if(!a || b)reject("memory-place operator arity");
      bool address=operation.kind==eok_address_of || operation.kind==eok_reference_to;
      auto pointer=skip_typerefs(address?e->type:a->type);
      if(pointer->kind!=tk_pointer ||
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
      std::vector<unsigned> items;
      for(auto item=a;item;item=item->next)items.push_back(expression(item));
      const auto shape=node_shapes.at(items.back());
      if(shape.type!=ty)reject("sequence final operand execution type mismatch");
      unsigned id=node("sequence",ty,e->position,0,0,"","",shape.place?"place":"value","left-to-right"),ordinal=0;
      for(auto item:items)edge(id,"item",ordinal++,item);
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
      // Void arms preserve the same lazy selection and become structured if/else.
      // Selected conditional places remain outside this bounded profile.
      if(!a || !b || !b->next || b->next->next)
        reject("conditional outside scalar/void value profile");
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
    if(type->kind==tk_array && !type->size && complete_fixed_array(type)) {
      if(!zero_constant_bits(value))reject("effectful/nonempty zero-sized array initializer");
      return;
    }
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
                    const initializer_path &path,unsigned value,const a_source_position &pos,bool construction=false) {
    auto type=path.empty()?variable->type:path.back().type;
    auto raw=skip_typerefs(type);
    bool record=raw->kind==tk_struct || raw->kind==tk_class || raw->kind==tk_union;
    unsigned assign=construction && record?record_transfer(type,pos,"right-to-left","construction"):
      node("assign",type_id(type),pos,0,0,"","","value","right-to-left");
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
      else if(d->kind==dik_expression && type->kind!=tk_array && record_value_policy(type,"construction")) {
        record_store(block,ordinal,variable,path,expression(d->variant.expression),pos,true);return;
      } else reject("record member dynamic initializer",d->kind);
    }
    if(type->kind==tk_array) {
      if(!type->size && complete_fixed_array(type)) {
        if(!zero_constant_bits(value))reject("effectful/nonempty zero-sized array initializer");
        return;
      }
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
  unsigned initialize_aggregate(a_variable_ptr variable,a_constant_ptr value,const a_source_position &pos,a_dynamic_init_ptr dynamic=nullptr,bool lowered_assignment=false) {
    // Source volatile accesses already have explicit volatile-place nodes.
    // Bulk default/array initialization cannot substitute for the observable
    // stores required by volatile subobjects; reject until those are owned.
    if(contains_volatile_storage(variable->type))reject("automatic aggregate initialization with volatile subobjects");
    auto type=skip_typerefs(variable->type),leaf=array_leaf(type);
    if(type->kind==tk_array && leaf->kind!=tk_struct && leaf->kind!=tk_class && leaf->kind!=tk_union)
      return initialize_array(variable,value,pos);
    unsigned block=node("block",0,pos),ordinal=0;initializer_path path;
    auto source=dot_object_source::state().variables.find(variable);
    bool owned=!C_mode() && source!=dot_object_source::state().variables.end() &&
      binding_source_declarations.count(binding_ids.at(variable)) &&
      (leaf->kind==tk_struct || leaf->kind==tk_class || leaf->kind==tk_union) &&
      leaf->variant.class_struct_union.extra_info;
    unsigned initialization=0;
    if(owned) {
      // This initializes storage, not a C++ record value or an assignment.
      // Match c_gen_be::start_initializer_assignments exactly: its memset
      // prefix is required only for explicit zero or partial initialization.
      bool zero=variable->init_kind==initk_zero ||
        (variable->init_kind==initk_dynamic && variable->initializer.dynamic->is_partially_initialized);
      initialization=node("record-storage-init",0,pos);
      edge(initialization,"target",0,record_place(variable,path,pos));edge(initialization,"body",0,block);
      record_storage_initializations.push_back("{\"nodeId\":"+number(initialization)+
        ",\"bindingId\":"+number(binding_ids.at(variable))+",\"sourceDeclarationId\":"+number(binding_source_declarations.at(binding_ids.at(variable)))+
        ",\"sourceTypeId\":"+number(source_type(source->second.type))+",\"sourceInitKind\":"+number(source->second.initializer_kind)+
        ",\"loweredInitKind\":"+number(unsigned(variable->init_kind))+",\"siteKind\":"+quote(lowered_assignment?"lowered-assignment":dynamic?"dynamic-init":"scope-entry")+
        ",\"zeroStorage\":"+(zero?"true":"false")+",\"partiallyInitialized\":"+
        (variable->init_kind==initk_dynamic && variable->initializer.dynamic->is_partially_initialized?"true":"false")+
        ",\"captureProof\":\"observed-storage-initialization\"}");
    } else if(type->kind==tk_array) {
      unsigned zero=node("array-init",0,pos);edge(zero,"target",0,record_place(variable,path,pos));edge(block,"statement",ordinal++,zero);
    } else record_store(block,ordinal,variable,path,zero_node(type_id(type),pos),pos);
    record_initializer(block,ordinal,variable,type,value,path,pos);return initialization?initialization:block;
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
  bool generated_cleanup_frame_statement(a_statement_ptr statement) {
    if(statement->kind==stmk_expr && statement->expr && statement->expr->kind==enk_lowered_eh_construct) {
      auto &value=statement->expr->variant.lowered_eh;
      return (value.kind==leck_function_prologue && statement->expr==active_cleanup_prologue) ||
        (value.kind==leck_function_epilogue && value.variant.epilogue_routine==current_routine);
    }
    if(statement->kind!=stmk_block || !statement->variant.block.statements)return false;
    bool has_frame=false;
    for(auto child=statement->variant.block.statements;child;child=child->next) {
      // add_eh_function_prologue wraps each return in an epilogue block.
      // The stock implicit return is deliberately absent from our owned tree.
      if(implicit_return(child) || child->kind==stmk_empty)continue;
      if(!generated_cleanup_frame_statement(child))return false;
      has_frame=true;
    }
    return has_frame;
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
    unsigned owned_statement=statement(child);
    edge(block,"statement",ordinal++,owned_statement);
    if(block==owned_function_root && !initializer_statement_anchors.count(child) &&
       generated_cleanup_frame_statement(child))
      generated_scope_scaffold(owned_statement,0,"cleanup-frame","observed-function-cleanup-frame");
    if(initializer_statement_anchors.count(child)) {
      if(!initializer_statement_nodes.emplace(child,owned_statement).second)
        reject("file initializer statement emitted more than once");
    }
    if(child->kind==stmk_init && cleanup_variables.count(child->variant.dynamic_init->variable)) {
      if(switch_body)reject("cleanup activation directly in switch body");
      auto v=child->variant.dynamic_init->variable;
      edge(block,"statement",ordinal++,register_cleanup(v,block,child->position));
    }
  }
  unsigned memory_barrier(a_statement_ptr statement) {
    auto entry=statement->variant.asm_entry;
    auto &captured=dot_object_source::state();
    auto found=captured.asm_statements.find(entry);
    if(found==captured.asm_statements.end() || !found->second.lowering_observed)
      reject("empty asm lacks observed source/lowering boundary");
    const auto &source=found->second;
    if(!source.error.empty())reject(source.error.c_str());
    unsigned id=node("memory-barrier",0,statement->position);
    memory_barriers.push_back("{\"nodeId\":"+number(id)+
      ",\"isVolatile\":"+(source.is_volatile?"true":"false")+
      ",\"hasVolatileKeyword\":"+(source.has_volatile_keyword?"true":"false")+
      ",\"hasInlineKeyword\":"+(source.has_inline_keyword?"true":"false")+
      ",\"clobbersMemory\":"+(source.clobbers_memory?"true":"false")+
      ",\"captureProof\":\"observed-empty-gnu-asm\"}");
#if GNU_EXTENSIONS_ALLOWED
    unsigned ordinal=0;
    for(auto operand=entry->operands;operand;operand=operand->next,++ordinal) {
      if(ordinal>=source.order.size() || source.order[ordinal]!=operand || !source.operands.count(operand))
        reject("empty asm operand identity/order changed during lowering");
      const auto &original=source.operands.at(operand);
      diagnostic_scope diagnostic(original.position);
      if(!original.error.empty())reject(original.error.c_str());
      auto expr=operand->expression;
      if(!expr || !original.element_type || !original.source_element_type || !original.element_size)
        reject("empty asm operand lacks captured element representation");
      unsigned element=type_id(original.element_type),pointer=pointer_type(element),address=0;
      if(original.array_address) {
        if(skip_typerefs(expr->type)->kind!=tk_pointer)reject("empty asm array address did not lower to pointer");
        address=expression(expr);
        if(type_id(expr->type)!=pointer) {
          unsigned cast=node("convert",pointer,expr->position,0,0,"","","value","left-to-right");
          edge(cast,"operand",0,address);address=cast;
        }
      } else {
        if(!place(expr))reject("empty asm operand lost its memory place");
        auto actual=skip_typerefs(expr->type);
        if(original.shape=="fixed-array" ? (actual->kind!=tk_array || type_id(array_leaf(actual))!=element) :
           type_id(actual)!=element)reject("empty asm operand lost its original element representation");
        unsigned actual_pointer=pointer_type(type_id(expr->type));
        address=node("address",actual_pointer,expr->position,0,0,"","","value","left-to-right");
        edge(address,"operand",0,expression(expr));
        if(actual_pointer!=pointer) {
          unsigned cast=node("convert",pointer,expr->position,0,0,"","","value","left-to-right");
          edge(cast,"operand",0,address);address=cast;
        }
      }
      // The address can contain the stock assignment of a VLA bound. Keep
      // its evaluation before reading the saved total. Both expressions are
      // owned children of this statement; neither performs a memory access.
      edge(id,"address",ordinal,address);
      bool variable=original.shape=="vla-array";
      unsigned extent=0,saved_binding=0;
      if(variable) {
        if(original.dimensions.size()!=1 || !original.dimensions[0].saved_total)
          reject("empty asm VLA operand lacks a rank-one saved total");
        auto total=original.dimensions[0].saved_total;
        auto binding=binding_ids.find(total);
        if(binding==binding_ids.end() || type_id(total->type)!=8 || !(total->compiler_generated || total->lowering_generated))
          reject("empty asm VLA total lacks its observed synthetic i64 binding");
        saved_binding=binding->second;
        unsigned read=node("binding",8,original.position,0,saved_binding,"","","place","none");
        unsigned count=node("convert",9,original.position,0,0,"","","value","left-to-right");
        edge(count,"operand",0,read);
        unsigned size=node("constant",9,original.position,0,0,"",std::to_string(original.element_size),"value","none");
        extent=node("binary",9,original.position,0,0,"mul","","value","left-to-right");
        edge(extent,"left",0,count);edge(extent,"right",0,size);
      } else {
        if(original.fixed_count>(~0ULL)/original.element_size)reject("empty asm fixed byte extent overflow");
        extent=node("constant",9,original.position,0,0,"",std::to_string(original.fixed_count*original.element_size),"value","none");
      }
      edge(id,"extent",ordinal,extent);
      memory_barrier_operands.push_back("{\"nodeId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal)+
        ",\"direction\":"+quote(original.direction)+",\"constraintText\":"+quote(original.constraint)+
        ",\"sourceName\":"+quote(original.name)+",\"sourceElementTypeId\":"+number(source_type(original.source_element_type))+
        ",\"elementSize\":"+std::to_string(original.element_size)+",\"sourceShape\":"+quote(original.shape)+
        ",\"extentKind\":"+quote(variable?"vla-total":"fixed")+
        ",\"fixedElementCount\":"+(variable?"null":std::to_string(original.fixed_count))+
        ",\"extentBindingId\":"+number(saved_binding)+
        ",\"captureProof\":"+quote(variable?"observed-asm-vla-total":"observed-asm-place")+position(original.position)+"}");
      unsigned dimension_ordinal=0;
      for(const auto &dimension:original.dimensions) {
        bool dynamic=dimension.original!=nullptr;
        memory_barrier_dimensions.push_back("{\"nodeId\":"+number(id)+",\"operandOrdinal\":"+std::to_string(ordinal)+
          ",\"ordinal\":"+std::to_string(dimension_ordinal++)+",\"kind\":"+quote(dynamic?"vla-total":"fixed")+
          ",\"fixedCount\":"+(dynamic?"null":std::to_string(dimension.fixed_count))+
          ",\"extentBindingId\":"+number(dynamic?saved_binding:0)+position(dimension.position)+"}");
      }
    }
    if(ordinal!=source.order.size())reject("empty asm lost an original operand during lowering");
#else
    reject("GNU memory asm support is not configured");
#endif
    return id;
  }
  unsigned statement(a_statement_ptr s,a_scope_ptr root_scope=nullptr) {
    if(!s)reject("missing statement");
    diagnostic_scope diagnostic(s->position);
    switch(s->kind) {
      case stmk_try_block: {
        auto t=s->variant.try_block;auto found=dot_object_source::state().tries.find(t);
        if(found==dot_object_source::state().tries.end() || found->second.owner!=current_routine)
          reject("source try lacks observed statement ownership");
        const auto &saved=found->second;
        if(saved.handlers.empty() || t->handlers!=saved.handlers[0])reject("source try handler list changed");
        unsigned id=node("try",0,s->position);
        if(!active_try_ids.emplace(t,id).second)reject("duplicated source try ownership");
        std::vector<unsigned> ids;
        for(auto h:saved.handlers) {
          unsigned hid=node("catch",0,h->catch_position);ids.push_back(hid);
          if(!active_handler_ids.emplace(h,hid).second)reject("duplicated source handler ownership");
        }
        check_lifetime(t->lifetime);
        edge(id,"body",0,statement(t->statement));
        for(unsigned i=0;i<ids.size();++i) {
          auto h=saved.handlers[i];const auto &caught=dot_object_source::state().handlers.at(h);
          if(h->parameter!=caught.parameter || h->next!=(i+1<ids.size()?saved.handlers[i+1]:nullptr))
            reject("source handler parameter or ordering changed");
          auto previous=emitting_handler;emitting_handler=h;
          edge(ids[i],"body",0,statement(h->statement));emitting_handler=previous;
          unsigned param=0;
          if(caught.parameter) {
            if(!binding_ids.count(caught.parameter))reject("catch parameter lacks owned local binding");
            param=binding_ids.at(caught.parameter);
          }
          source_handlers.push_back("{\"nodeId\":"+number(ids[i])+",\"tryNodeId\":"+number(id)+
            ",\"ordinal\":"+std::to_string(i)+",\"matchKind\":"+quote(param?"typed":"ellipsis")+
            ",\"sourceTypeId\":"+number(caught.source_type?source_type(caught.source_type):0)+
            ",\"matchTypeId\":"+number(caught.match_type?source_type(caught.match_type):0)+
            ",\"parameterBindingId\":"+number(param)+",\"captureProof\":\"observed-source-handler\"}");
          edge(id,"handler",i,ids[i]);
        }
        return id;
      }
      case stmk_block: {
        unsigned id=node("block",0,s->position),ordinal=0;
        if(root_scope)owned_function_root=id;
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
        // add_init_assignment may replace a constexpr constructor with a
        // constant aggregate assigned to its original local storage. The
        // observed operation initializes that object; it does not copy one.
        auto initialization=dot_object_source::state().record_transfers.find(s->expr);
        if(initialization!=dot_object_source::state().record_transfers.end()) {
          const auto &proof=initialization->second;
          if(proof.kind=="construction" && proof.owner==current_routine &&
             s->expr->kind==enk_operation && s->expr->variant.operation.kind==eok_assign &&
             proof.target==s->expr->variant.operation.operands && proof.target->next==proof.value &&
             !proof.value->next && proof.target->kind==enk_variable && proof.value->kind==enk_constant) {
            auto variable=node_variable(proof.target);auto value=node_constant(proof.value);
            if(value->kind==ck_aggregate && binding_ids.count(variable) &&
               binding_source_declarations.count(binding_ids.at(variable)) &&
               dot_object_source::state().variables.count(variable) &&
               type_id(proof.target->type)==type_id(proof.value->type)) {
              if(value->is_partially_initialized && variable->init_kind!=initk_zero &&
                 !(variable->init_kind==initk_dynamic && variable->initializer.dynamic->is_partially_initialized))
                reject("lowered aggregate initializer lacks observed zero prefix");
              return initialize_aggregate(variable,value,s->position,nullptr,true);
            }
          }
        }
        unsigned id=node("expr-stmt",0,s->position);edge(id,"value",0,expression(s->expr));return id;
      }
      case stmk_constexpr_if: {
        // c_gen_be::dump_statement emits only the frontend-selected branch.
        // A discarded template branch may not have been instantiated at all.
        auto choice=s->variant.constexpr_if;
        if(!choice || !choice->value_known)reject("dependent constexpr selection reached executable graph");
        unsigned id=node("compile-time-if",0,s->position);
        auto selected=choice->value?choice->then_statement:choice->else_statement;
        edge(id,"body",0,selected?statement(selected):node("block",0,s->position));
        compile_time_selections.push_back("{\"nodeId\":"+number(id)+
          ",\"conditionValue\":"+(choice->value?"true":"false")+
          ",\"hasElse\":"+(choice->else_statement?"true":"false")+
          ",\"captureProof\":\"observed-constexpr-selection\"}");
        return id;
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
        unsigned selector=expression(s->expr);
        auto selector_type=skip_typerefs(s->expr->type);
        // Stock C relies on C's integral promotion at switch. Preserve that
        // operation explicitly when EDG retains a narrow scalar expression.
        if(selector_type->kind==tk_integer && selector_type->size<targ_sizeof_int) {
          unsigned promoted=node("convert",2,s->expr->position,0,0,"","","value","left-to-right");
          edge(promoted,"operand",0,selector);selector=promoted;
        }
        edge(id,"condition",0,selector);
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
      case stmk_asm:return memory_barrier(s);
      case stmk_empty:return node("block",0,s->position);
      case stmk_init: {
        auto d=s->variant.dynamic_init;
        if(d->destructor) {
          // As in c_gen_be::dump_whole_variable_dynamic_init, this statement
          // performs initialization only. Lowering emits destruction and
          // activation separately; retain the destructor's original owner
          // and require those observed actions instead of duplicating them.
          auto original=dot_object_source::source_destruction(d,current_routine);
          auto callback=dot_object_source::abi_primary(d->destructor);
          if(!callback)callback=d->destructor;
          if(!original || callback!=dot_object_source::state().destructions.at(original).destructor ||
             (!active_source_obligations.count(original) && !active_normal_obligations.count(original) &&
              !active_array_obligations.count(original)))
            reject("initialization destructor lacks its observed cleanup ownership");
        }
        if(!d->variable || !binding_ids.count(d->variable))reject("initialization without local binding");
        // A destructor-only initialization has no value store. Stock C also
        // treats dik_none as already initialized; the subsequent transition
        // is still emitted and activates the owned destruction obligation.
        if(d->kind==dik_none)return node("block",0,s->position);
        unsigned ty=type_id(d->variable->type);
        auto initialized_type=skip_typerefs(d->variable->type);
        bool initialized_record=initialized_type->kind==tk_struct || initialized_type->kind==tk_class || initialized_type->kind==tk_union;
        if(initialized_record && d->kind==dik_expression && !record_value_policy(d->variable->type,"construction"))reject("record initialization lacks observed construction policy");
        if(initialized_type->kind==tk_array || (initialized_record && d->kind!=dik_expression)) {
          if(d->kind==dik_constant || d->kind==dik_nonconstant_aggregate)
            return initialize_aggregate(d->variable,d->variant.constant.ptr,s->position,d);
          if(d->kind==dik_zero)return initialize_aggregate(d->variable,nullptr,s->position,d);
          reject("array dynamic initialization",d->kind);
        }
        unsigned value=0;
        if(d->kind==dik_expression)value=expression(d->variant.expression);
        else if(d->kind==dik_constant)value=constant_node(d->variant.constant.ptr,s->position);
        else if(d->kind==dik_zero)value=zero_node(ty,s->position);
        else reject("dynamic initializer",d->kind);
        unsigned assign=initialized_record?record_transfer(d->variable->type,s->position,"right-to-left","construction"):
          node("assign",ty,s->position,0,0,"","","value","right-to-left");
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
    validate_file_initializers(file_scope);
    collect_storage(file_scope);
    prepare_static_source_metadata();
#if GNU_EXTENSIONS_ALLOWED
    unsigned startup_ordinal=0;
#endif
    for(auto r=file_scope->routines;r;r=r->next) {
      if(ignore_routine_in_back_end(r))continue;
      if(r->function_def_number==NULL_function_def_number)continue;
#if GNU_EXTENSIONS_ALLOWED
      // Follow stock C dump_scope_routines order, not reachability or first
      // declaration position. Equal-priority ties are defined by this order.
      if(r->is_initialization_routine || dot_object_source::state().generated_initializers.count(r))
        startup_action(r,startup_ordinal++);
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
      function=routine_ids.at(r);current_routine=r;auto scope=scope_for_routine_or_null(r);
      string label="dotobj function "+source_spelling(&r->source_corresp);context=label.c_str();
      binding_ids.clear();binding_scopes.clear();cleanup_variables.clear();registered_cleanups.clear();
      emitted_expression_ids.clear();
      bound_scopes.clear();declared_bindings.clear();controls.clear();switch_cases.clear();control_labels.clear();source_labels.clear();emitted_source_labels.clear();
      prepare_owned_cleanup(r,scope);
      check_lifetime(scope->lifetime);
      if(!scope->assoc_block || scope->assoc_block->kind!=stmk_block)reject("nonblock function root");
      function_block=scope->assoc_block;
      unsigned root=statement(scope->assoc_block,scope);
      write_owned_cleanup(r);
      auto static_destructor=dot_object_source::state().generated_static_destructors.find(r);
      if(static_destructor!=dot_object_source::state().generated_static_destructors.end()) {
        const auto &origin=static_destructor->second;
        if(!binding_ids.count(origin.parameter))reject("generated static destructor omitted its exact synthetic parameter");
        generated_static_destructors.push_back("{\"functionEntityId\":"+number(function)+
          ",\"sourceObligationId\":"+number(static_obligation_id(origin.original))+
          ",\"parameterBindingId\":"+number(binding_ids.at(origin.parameter))+",\"storageId\":"+number(storage(origin.variable))+
          ",\"elementTypeId\":"+number(origin.array?type_id(origin.element_type):0)+
          ",\"elementCount\":"+(origin.array?std::to_string(origin.element_count):"null")+
          ",\"elementSize\":"+(origin.array?std::to_string(origin.element_size):"null")+
          ",\"captureProof\":\"observed-static-destructor-body\"}");
      }
      if(source_labels.size()!=emitted_source_labels.size())reject("goto target missing from function body");
      if(!source_labels.empty() && !cleanup_variables.empty())reject("goto with lexical cleanup obligations");
      if(cleanup_variables!=registered_cleanups)reject("missing cleanup declaration/initialization completion anchor");
      functions.push_back("{\"entityId\":"+number(function)+",\"bodyNodeId\":"+number(root)+"}");
      context="file scope";
    }
    for(auto obligation:required_source_obligations)
      if(!covered_source_obligations.count(obligation) || !source_obligation_ids.count(obligation)) {
        const auto &source=dot_object_source::state().destructions.at(obligation);
        diagnostic_scope diagnostic(source.position.seq?source.position:
          source.owner?source.owner->source_corresp.decl_position:source.position);
        reject((string("original source cleanup lacks an owned emitted action across its ABI variants: ")+
          (source.allocation_release?"allocation":"destruction")+
          " owner="+(source.owner?source_spelling(&source.owner->source_corresp):"file-initializer")+
          " ordinal="+std::to_string(source.ordinal)+
          " callback="+(source.destructor?source_spelling(&source.destructor->source_corresp):"none")+
          " source-type="+(source.source_type?dot_object_source::state().types.at(source.source_type-1).name:"none")+
          " emitted="+(covered_source_obligations.count(obligation)?"yes":"no")+
          " metadata="+(source_obligation_ids.count(obligation)?"yes":"no")+
          " observed-no-effect="+(dot_object_source::state().automatic_no_effect_observed.count(obligation)?"yes":"no")).c_str());
      }
    write_dynamic_initializer_origins();
    finish_static_source_metadata();
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
    string result="{\"formatVersion\":1,\"profile\":\"scalar-control-v40\",\"sourceLanguage\":"+quote(C_mode()?"c":"c++")+",\"sourceFiles\":"+array(files)+
      ",\"macroDefinitions\":"+array(macro_definitions)+",\"macroParameters\":"+array(macro_parameters)+
      ",\"macroEvents\":"+array(macro_events)+
      ",\"sourceTypes\":"+array(source_types)+",\"sourceDeclarations\":"+array(source_declarations)+
      ",\"sourceTypeQualifiers\":"+array(source_type_qualifiers)+",\"sourceEnumTypes\":"+array(source_enum_types)+
      ",\"sourceEnumerators\":"+array(source_enumerators)+",\"sourceMemberPointerTypes\":"+array(source_member_pointer_types)+
      ",\"sourceFunctions\":"+array(source_functions)+",\"sourceParameters\":"+array(source_parameters)+
      ",\"sourceDependentTypes\":"+array(source_dependent_types)+
      ",\"sourceInitializers\":"+array(source_initializers)+",\"sourceStaticLifetimes\":"+array(source_static_lifetimes)+
      ",\"sourceStaticDestructions\":"+array(source_static_destructions)+",\"initializerLifetimeExecutions\":"+array(initializer_lifetime_executions)+
      ",\"staticDestructionRegistrations\":"+array(static_destruction_registrations)+",\"generatedStaticDestructors\":"+array(generated_static_destructors)+
      ",\"staticDestructionDispositions\":"+array(static_destruction_dispositions)+
      ",\"sourceLifetimes\":"+array(source_lifetimes)+",\"sourceDestructionObligations\":"+array(source_destruction_obligations)+
      ",\"cleanupActions\":"+array(cleanup_actions)+",\"cleanupArrays\":"+array(cleanup_arrays)+",\"cleanupVttArguments\":"+array(cleanup_vtt_arguments)+",\"arrayRuntimeOperations\":"+array(array_runtime_operations)+",\"cleanupStates\":"+array(cleanup_states)+
      ",\"cleanupTransitions\":"+array(cleanup_transitions)+",\"cleanupFrameMarkers\":"+array(cleanup_frame_markers)+
      ",\"expressionLifetimes\":"+array(expression_lifetimes)+
      ",\"destructionElisions\":"+array(destruction_elisions)+",\"normalDestructions\":"+array(normal_destructions)+",\"cleanupConditions\":"+array(cleanup_conditions)+
      ",\"sourceRecords\":"+array(source_records)+",\"sourceRecordOperations\":"+array(source_record_operations)+
      ",\"sourceRecordOwners\":"+array(source_record_owners)+
      ",\"exceptionTypes\":"+array(exception_types)+",\"exceptionBaseConversions\":"+array(exception_base_conversions)+
      ",\"throwExpressions\":"+array(throw_expressions)+",\"throwObjectAddresses\":"+array(throw_object_addresses)+
      ",\"throwMarkers\":"+array(throw_markers)+",\"rethrowExpressions\":"+array(rethrow_expressions)+
      ",\"throwInitializationBoundaries\":"+array(throw_initialization_boundaries)+",\"throwCleanupTransitions\":"+array(throw_cleanup_transitions)+
      ",\"internalTryExpressions\":"+array(internal_try_expressions)+
      ",\"sourceAllocationObligations\":"+array(source_allocation_obligations)+
      ",\"allocationCleanupActions\":"+array(allocation_cleanup_actions)+
      ",\"localStaticGuards\":"+array(local_static_guards)+",\"guardOperations\":"+array(guard_operations)+
      ",\"guardCleanupActions\":"+array(guard_cleanup_actions)+
      ",\"cleanupPayloadTargets\":"+array(cleanup_payload_targets)+
      ",\"throwPayloadSubobjects\":"+array(throw_payload_subobjects)+
      ",\"throwPayloadPaths\":"+array(throw_payload_paths)+
      ",\"sourceTries\":"+array(source_tries)+",\"sourceHandlers\":"+array(source_handlers)+
      ",\"handlerMarkers\":"+array(handler_markers)+",\"caughtObjectAddresses\":"+array(caught_object_addresses)+
      ",\"recordValueTransfers\":"+array(record_value_transfers)+",\"recordStorageInitializations\":"+array(record_storage_initializations)+",\"recordConstantInitializations\":"+array(record_constant_initializations)+",\"sourceFields\":"+array(source_fields)+
      ",\"sourceBases\":"+array(source_bases)+",\"recordBaseSubobjects\":"+array(record_base_subobjects)+",\"virtualBaseSubobjects\":"+array(virtual_base_subobjects)+",\"virtualBaseExpressions\":"+array(virtual_base_expressions)+",\"virtualBaseAdjustments\":"+array(virtual_base_adjustments)+
      ",\"baseSubobjectExpressions\":"+array(base_subobject_expressions)+
      ",\"abiRoutineOrigins\":"+array(abi_routine_origins)+
      ",\"compileTimeSelections\":"+array(compile_time_selections)+
      ",\"thunkWrappers\":"+array(thunk_wrappers)+",\"wrapperCalls\":"+array(wrapper_calls)+
      ",\"recordTypes\":"+array(record_types)+",\"recordFields\":"+array(record_fields)+
      ",\"storages\":"+array(storages)+",\"storageData\":"+array(storage_data)+",\"relocations\":"+array(relocations)+
      ",\"routineTypes\":"+array(routine_types)+",\"routineParameters\":"+array(routine_parameters)+
      ",\"sourceRoutineTypes\":"+array(source_routine_types)+",\"sourceRoutineParameters\":"+array(source_routine_parameters)+
      ",\"deferredRoutineAbis\":"+array(deferred_routine_abis)+
      ",\"sourceRoutineAbis\":"+array(source_routine_abis)+",\"sourceRoutineAbiParameters\":"+array(source_routine_abi_parameters)+
      ",\"initializationActions\":"+array(initialization_actions)+
      ",\"generatedRoutineOrigins\":"+array(generated_routine_origins)+
      ",\"generatedRoutineScaffolds\":"+array(generated_routine_scaffolds)+
      ",\"dynamicInitializers\":"+array(dynamic_initializers)+
      ",\"dynamicInitializerStatements\":"+array(dynamic_initializer_statements)+
      ",\"memoryBarriers\":"+array(memory_barriers)+",\"memoryBarrierOperands\":"+array(memory_barrier_operands)+
      ",\"memoryBarrierDimensions\":"+array(memory_barrier_dimensions)+
      ",\"types\":"+array(types)+",\"entities\":"+array(entities)+
      ",\"parameters\":"+array(parameters)+",\"functions\":"+array(functions)+",\"bindings\":"+array(bindings)+",\"nodes\":"+array(nodes)+",\"edges\":"+array(edges)+"}\n";
    if(fwrite(result.data(),1,result.size(),stdout)!=result.size() || fflush(stdout))reject("wire output failure");
  }
};
void emit(){Writer writer;writer.run();}
}
