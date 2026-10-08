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
  unsupported((string("dotobj scalar-v6: ")+what).c_str(),kind);
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
  unsigned function=0;
  std::map<a_routine_ptr,unsigned> routine_ids;
  std::map<a_variable_ptr,unsigned> binding_ids;
  std::map<a_source_file_ptr,unsigned> file_ids;
  std::vector<a_routine_ptr> routines;
  std::vector<string> files,entities,parameters,functions,bindings,nodes,edges;
  std::vector<string> source_types,source_declarations,source_functions,source_parameters;
  std::vector<string> macro_definitions,macro_parameters,macro_events;
  std::map<unsigned,unsigned> source_type_ids;
  std::set<a_scope_ptr> bound_scopes;
  std::set<unsigned> declared_bindings;
  struct Control { unsigned node; source_control_context labels; };
  std::vector<Control> controls;
  std::set<a_label_ptr> control_labels;
  std::map<a_statement_ptr,a_statement_ptr> switch_cases;
  string number(unsigned value){return value?std::to_string(value):"null";}
  unsigned type_id(a_type_ptr type) {
    unsigned q=get_type_qualifiers(type);
    if(q & (TQ_VOLATILE|TQ_C11_ATOMIC))reject("volatile/atomic type");
    auto t=skip_typerefs(type);
    if(t->kind==tk_void)return 1;
    // EDG retains the bool marker alongside enum bookkeeping after lowering;
    // unlike a user enum, its executable representation is the lowered byte.
    if(t->kind==tk_integer && t->variant.integer.bool_type && t->size==1)return 3;
    if(t->kind==tk_integer && !t->variant.integer.enum_type) {
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
        default:break;
      }
    }
    reject("type outside void/fixed-width-integer profile",t->kind);
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
  unsigned source_type(unsigned captured) {
    if(!captured)reject("missing pre-lowering scalar type");
    auto prior=source_type_ids.find(captured);if(prior!=source_type_ids.end())return prior->second;
    const auto &type=dot_object_source::state().types.at(captured-1);
    if(!type.execution)reject("source type outside scalar declaration profile");
    unsigned id=next_source_type++;source_type_ids[captured]=id;
    unsigned underlying=type.underlying?source_type(type.underlying):0;
    source_types.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(type.kind)+",\"sourceName\":"+quote(type.name)+
      ",\"underlyingTypeId\":"+number(underlying)+",\"executionTypeId\":"+number(type.execution)+
      ",\"isConst\":"+(type.is_const?"true":"false")+",\"isVolatile\":"+(type.is_volatile?"true":"false")+position(type.position)+"}");
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
      ",\"capturePhase\":\"before-lowering\"}");
    unsigned ordinal=0;
    for(auto parameter:captured.parameters)
      source_parameters.push_back("{\"entityId\":"+number(entity)+",\"ordinal\":"+std::to_string(ordinal++)+",\"sourceTypeId\":"+number(source_type(parameter))+"}");
  }
  unsigned node(const string &kind,unsigned ty,const a_source_position &pos,
                unsigned entity=0,unsigned binding=0,const string &op="",
                const string &value="",const string &category="",const string &order="") {
    unsigned id=next_node++;
    nodes.push_back("{\"id\":"+number(id)+",\"functionId\":"+number(function)+",\"kind\":"+quote(kind)+",\"typeId\":"+number(ty)+
      ",\"entityId\":"+number(entity)+",\"bindingId\":"+number(binding)+",\"operator\":"+(op.empty()?"null":quote(op))+
      ",\"constantText\":"+(value.empty()?"null":quote(value))+",\"valueCategory\":"+(category.empty()?"null":quote(category))+
      ",\"evaluationOrder\":"+(order.empty()?"null":quote(order))+position(pos)+"}");
    return id;
  }
  void edge(unsigned parent,const string &role,unsigned ordinal,unsigned child) {
    if(!child)reject("missing child node");
    edges.push_back("{\"parentId\":"+number(parent)+",\"role\":"+quote(role)+",\"ordinal\":"+std::to_string(ordinal)+",\"childId\":"+number(child)+"}");
  }
  unsigned routine(a_routine_ptr r) {
    auto found=routine_ids.find(r);if(found!=routine_ids.end())return found->second;
    diagnostic_scope diagnostic(r->source_corresp.decl_position);
    if(r->is_inline || r->is_template_function)reject("inline/template definition policy");
#if GNU_EXTENSIONS_ALLOWED
    if(r->is_weak || r->is_weakref || r->is_gnu_alias || r->is_ifunc || r->is_initialization_routine || r->is_finalization_routine)
      reject("special routine linkage/startup policy");
#endif
#if DO_IL_LOWERING && IA64_ABI
    if(r->use_comdat)reject("COMDAT definition policy");
#endif
    auto t=skip_typerefs(r->type);
    if(t->kind!=tk_routine)reject("nonroutine function type");
    auto extra=t->variant.routine.extra_info;
    if(!extra->prototyped || extra->has_ellipsis)reject("unprototyped/variadic function");
    if(extra->explicit_calling_convention)reject("explicit calling convention");
    if(extra->exception_specification)reject("exception specification/noexcept");
    unsigned id=next_entity++;routine_ids[r]=id;routines.push_back(r);
    auto key=routine_linker_symbol(r);
    bool external=r->source_corresp.name_linkage==nlk_external || r->source_corresp.name_linkage==nlk_cplusplus_external;
    if(external && key.empty())reject("missing exact external identity");
    auto body=r->function_def_number==NULL_function_def_number?nullptr:scope_for_routine_or_null(r);
    if(r->function_def_number!=NULL_function_def_number && !body)reject("unavailable function body");
    entities.push_back("{\"id\":"+number(id)+",\"sourceName\":"+quote(source_spelling(&r->source_corresp))+
      ",\"linkage\":"+quote(external?"external":"internal")+",\"externalKey\":"+(external?quote(key):"null")+
      ",\"definition\":"+quote(body?"strong":"declaration")+",\"returnTypeId\":"+number(type_id(t->variant.routine.return_type))+position(r->source_corresp.decl_position)+"}");
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
    if(v->init_kind!=initk_none && v->init_kind!=initk_dynamic)reject("nonexecutably placed local initializer",v->init_kind);
    unsigned id=next_binding++;binding_ids[v]=id;
    auto source=dot_object_source::state().variables.find(v);
    if(source!=dot_object_source::state().variables.end())
      source_declaration(source->second,parameter?"parameter":"local",0,id,function);
    else reject("missing pre-lowering variable capture (including synthetic binding)");
    bindings.push_back("{\"id\":"+number(id)+",\"functionId\":"+number(function)+",\"kind\":"+quote(parameter?"parameter":"local")+
      ",\"ordinal\":"+(parameter?std::to_string(ordinal):"null")+",\"typeId\":"+number(type_id(v->type))+
      ",\"sourceName\":"+quote(source_spelling(&v->source_corresp))+",\"scopeNodeId\":"+number(scope)+position(v->source_corresp.decl_position)+"}");
    return id;
  }
  void scope_bindings(a_scope_ptr scope,unsigned block,unsigned &ordinal) {
    if(!scope || !bound_scopes.insert(scope).second)return;
    check_lifetime(scope->lifetime);
    if(scope->variables || scope->local_static_variable_inits || scope->vla_dimensions)reject("scope static/TLS/VLA storage");
    for(auto v=scope->nonstatic_variables;v;v=v->next) {
      unsigned id=binding(v,block);
      if(!declared_bindings.insert(id).second)continue;
      edge(block,"statement",ordinal++,node("declare",0,v->source_corresp.decl_position,0,id));
    }
  }
  unsigned constant_node(a_constant_ptr c,const a_source_position &pos) {
    if(c->kind!=ck_integer)reject("noninteger constant",c->kind);
    unsigned ty=type_id(c->type);a_boolean overflow=FALSE;
    string text;
    if(int_constant_is_signed(c))text=std::to_string((long long)value_of_integer_constant(c,&overflow));
    else text=std::to_string((unsigned long long)unsigned_value_of_integer_constant(c,&overflow));
    if(overflow)reject("out of range scalar constant");
    return node("constant",ty,pos,0,0,"",text,"value","none");
  }
  unsigned expression(an_expr_node_ptr e) {
    if(!e)reject("null expression");
    diagnostic_scope diagnostic(e->position);
    unsigned ty=type_id(e->type);
    if(e->kind==enk_constant)return constant_node(node_constant(e),e->position);
    if(e->kind==enk_variable) {
      auto it=binding_ids.find(node_variable(e));if(it==binding_ids.end())reject("unbound/global variable expression");
      return node("binding",ty,e->position,0,it->second,"","","place","none");
    }
    if(e->kind!=enk_operation)reject("expression",e->kind);
    auto &operation=e->variant.operation;
    auto a=operation.operands;auto b=a?a->next:nullptr;
    string order=operation.eval_right_to_left?"right-to-left":operation.eval_left_to_right?"left-to-right":"unspecified";
    if(operation.kind==eok_call) {
      if(!a || a->kind!=enk_routine)reject("indirect function call");
      unsigned id=node("call",ty,e->position,routine(a->variant.routine.ptr),0,"","","value",order),ordinal=0;
      for(auto arg=b;arg;arg=arg->next)edge(id,"argument",ordinal++,expression(arg));
      return id;
    }
    if(operation.kind==eok_assign) {
      if(!a || !b || b->next || a->kind!=enk_variable)reject("nonbinding assignment");
      unsigned id=node("assign",ty,e->position,0,0,"","","value",order);
      edge(id,"target",0,expression(a));edge(id,"value",0,expression(b));return id;
    }
    if(operation.kind==eok_cast) {
      if(!a || b || ty==1)reject("unsupported scalar conversion");
      unsigned id=node("convert",ty,e->position,0,0,"","","value",order);
      edge(id,"operand",0,expression(a));return id;
    }
    if(operation.kind==eok_question) {
      // Preserve lazy branch selection. The stock C backend requires both
      // lowered arm types to match the result; do not recreate promotions.
      // A scalar lvalue conditional is consumed here as the selected value.
      // Assignment/mutation targets require a direct variable above/below;
      // no address or reference operation can export its selected place.
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
      if(!a || b || a->kind!=enk_variable)reject("nonbinding scalar mutation");
      auto operand_type=skip_typerefs(a->type);
      if(operand_type->kind!=tk_integer || operand_type->variant.integer.bool_type || type_id(a->type)!=ty)
        reject("mutation outside integer binding profile");
      string op=operation.kind==eok_pre_incr?"pre-inc":operation.kind==eok_post_incr?"post-inc":
                operation.kind==eok_pre_decr?"pre-dec":"post-dec";
      unsigned id=node("mutate",ty,e->position,0,0,op,"","value",order);
      edge(id,"operand",0,expression(a));return id;
    }
    const char *op=nullptr;bool unary=false;
    switch(operation.kind) {
      case eok_add:op="add";break;case eok_subtract:op="sub";break;case eok_multiply:op="mul";break;
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
    unsigned id=node(unary?"unary":"binary",ty,e->position,0,0,op,"","value",order);
    edge(id,unary?"operand":"left",0,expression(a));if(!unary)edge(id,"right",0,expression(b));return id;
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
        for(auto child=s->variant.block.statements;child;child=child->next) {
          if(child->kind==stmk_decl || child->kind==stmk_empty)continue;
          if(child->kind==stmk_label && control_labels.count(child->variant.label.ptr))continue;
          edge(id,"statement",ordinal++,statement(child));
        }
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
        auto body=s->variant.switch_stmt.body_statement;
        if(!body || body->kind!=stmk_block)reject("switch body outside block profile");
        auto descriptor=s->variant.switch_stmt.extra_info;
        auto expected=descriptor->cases;
        bool labeled=false;
        // Keep the lexical switch body instead of the legacy emitter's
        // source_switch_items flattening. Flattening nested blocks would lose
        // binding scopes. Case labels inside blocks/control statements require
        // additional dispatch semantics and are deliberately rejected below.
        for(auto child=body->variant.block.statements;child;child=child->next) {
          if(child->kind==stmk_switch_case) {
            auto c=child->variant.switch_case.extra_info;
            if(child->variant.switch_case.switch_statement!=s || c!=expected || c->stmt!=child)
              reject("switch case descriptor ownership/order");
#if GNU_EXTENSIONS_ALLOWED
            if(c->range_end)reject("GNU switch case range");
#endif
            if(!c->case_value && descriptor->default_case!=c)reject("switch default descriptor");
            switch_cases[child]=s;expected=expected->next;labeled=true;
          } else if(!labeled && child->kind!=stmk_decl && child->kind!=stmk_empty)
            reject("executable statement before first switch case");
        }
        if(expected)reject("switch case nested below switch body");
        unsigned id=node("switch",0,s->position);
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
        if(!switch_cases.count(s))reject("switch case outside immediate switch body");
        unsigned id=node("case",0,s->position);
        auto c=s->variant.switch_case.extra_info;
        if(c->case_value)edge(id,"value",0,constant_node(c->case_value,c->position));
        return id;
      }
      case stmk_goto: {
        auto target=s->variant.label.ptr;
        if(controls.empty() || target->source_corresp.name)reject("goto outside structured control transfer");
        unsigned owner=0;string kind;
        // A break always belongs to the nearest loop or switch. A continue
        // skips intervening switches, but never an intervening loop.
        if(target==controls.back().labels.break_target) {
          owner=controls.back().node;kind="break";
        } else for(auto it=controls.rbegin();it!=controls.rend();++it) {
          if(!it->labels.loop)continue;
          if(target==it->labels.continue_target){owner=it->node;kind="continue";}
          break;
        }
        if(!owner)reject("nonlocal structured control transfer");
        unsigned id=node(kind,0,s->position);
        // This is a non-owning control reference, not an evaluation child.
        edge(id,"target",0,owner);return id;
      }
      case stmk_label: {
        if(!control_labels.count(s->variant.label.ptr))reject("label outside structured control transfer");
        return node("block",0,s->position);
      }
      case stmk_empty:return node("block",0,s->position);
      case stmk_init: {
        auto d=s->variant.dynamic_init;
        if(d->destructor)reject("initialization with destructor");
        if(!d->variable || !binding_ids.count(d->variable))reject("initialization without local binding");
        unsigned binding=binding_ids.at(d->variable),ty=type_id(d->variable->type);
        unsigned value=0;
        if(d->kind==dik_expression)value=expression(d->variant.expression);
        else if(d->kind==dik_constant)value=constant_node(d->variant.constant.ptr,s->position);
        else if(d->kind==dik_zero)value=node("constant",ty,s->position,0,0,"","0","value","none");
        else reject("dynamic initializer",d->kind);
        unsigned assign=node("assign",ty,s->position,0,0,"","","value","right-to-left");
        edge(assign,"target",0,node("binding",ty,s->position,0,binding,"","","place","none"));edge(assign,"value",0,value);
        unsigned id=node("expr-stmt",0,s->position);edge(id,"value",0,assign);return id;
      }
      default:reject("statement",s->kind);
    }
  }
  void run() {
    if(targ_sizeof_pointer!=8)reject("non64-bit target");
    source_files(il_header.primary_source_file,0);
    capture_macros();
    auto file_scope=il_header.primary_scope;
    if(file_scope->variables || file_scope->dynamic_inits || file_scope->asm_entries)reject("global storage/initialization/asm");
    for(auto r=file_scope->routines;r;r=r->next) {
      if(ignore_routine_in_back_end(r))continue;
      if(r->compiler_generated && r->function_def_number==NULL_function_def_number)continue;
      routine(r);
    }
    for(size_t i=0;i<routines.size();++i) {
      auto r=routines[i];if(r->function_def_number==NULL_function_def_number)continue;
      function=routine_ids.at(r);auto scope=scope_for_routine_or_null(r);
      string label="dotobj function "+source_spelling(&r->source_corresp);context=label.c_str();
      binding_ids.clear();bound_scopes.clear();declared_bindings.clear();controls.clear();switch_cases.clear();control_labels.clear();
      check_lifetime(scope->lifetime);
      if(!scope->assoc_block || scope->assoc_block->kind!=stmk_block)reject("nonblock function root");
      unsigned root=statement(scope->assoc_block,scope);
      functions.push_back("{\"entityId\":"+number(function)+",\"bodyNodeId\":"+number(root)+"}");
      context="file scope";
    }
    string result="{\"formatVersion\":1,\"profile\":\"scalar-control-v6\",\"sourceFiles\":"+array(files)+
      ",\"macroDefinitions\":"+array(macro_definitions)+",\"macroParameters\":"+array(macro_parameters)+
      ",\"macroEvents\":"+array(macro_events)+
      ",\"sourceTypes\":"+array(source_types)+",\"sourceDeclarations\":"+array(source_declarations)+
      ",\"sourceFunctions\":"+array(source_functions)+",\"sourceParameters\":"+array(source_parameters)+
      ",\"types\":[{\"id\":1,\"kind\":\"void\"},{\"id\":2,\"kind\":\"i32\"},{\"id\":3,\"kind\":\"u8\"},"
      "{\"id\":4,\"kind\":\"i8\"},{\"id\":5,\"kind\":\"i16\"},{\"id\":6,\"kind\":\"u16\"},"
      "{\"id\":7,\"kind\":\"u32\"},{\"id\":8,\"kind\":\"i64\"},{\"id\":9,\"kind\":\"u64\"}],\"entities\":"+array(entities)+
      ",\"parameters\":"+array(parameters)+",\"functions\":"+array(functions)+",\"bindings\":"+array(bindings)+",\"nodes\":"+array(nodes)+",\"edges\":"+array(edges)+"}\n";
    if(fwrite(result.data(),1,result.size(),stdout)!=result.size() || fflush(stdout))reject("wire output failure");
  }
};
void emit(){Writer writer;writer.run();}
}
