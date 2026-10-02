// Preserve the source identity behind EDG's rewritten member-pointer and
// function signatures. In particular, a struct-returning function can acquire
// the same lowered call signature as a different source void function.
// lower_eh.c creates canonical RTTI before that rewrite; its symbol is an
// opaque identity here, never parsed to reconstruct source syntax.
a_type_ptr exception_identity_type(a_type_ptr input,bool &nothrow) {
  auto t=get_underlying_type(input);
  nothrow=(t->kind==tk_routine ||
      (t->kind==tk_ptr_to_member && is_function_type(pm_member_type(t)))) && is_nothrow_type(t);
  if(nothrow) {
    // il.c::function_type_without_noexcept_exception_spec caches this exact
    // source type. Do not create/mangle a fresh type after structural lowering.
    a_type_ptr plain=nullptr;
    // overwrite_type_with_new_type leaves based_types on the rewritten
    // wrapper. Its orig_type copy deliberately clears that cache (il.c).
    for(auto candidate=input;candidate && !plain;) {
      for(auto p=candidate->based_types;p;p=p->next)
        if(p->kind==btk_no_noexcept_exception_spec) {plain=p->based_type;break;}
      candidate=candidate->kind==tk_typeref?candidate->variant.typeref.type:nullptr;
    }
    if(!plain)unsupported("missing source noexcept exception identity");
    t=get_underlying_type(plain);
  }
  return t;
}
string extended_exception_leaf(a_type_ptr input,bool &nothrow,unsigned &member_kind) {
  auto t=get_underlying_type(input);
  if(t->kind!=tk_ptr_to_member && t->kind!=tk_routine && t->kind!=tk_array)return "";
  if(t->kind==tk_ptr_to_member)
    member_kind=is_function_type(pm_member_type(t))?3:2;
  t=exception_identity_type(input,nothrow);
  auto v=t->typeinfo_var;
  if(!v || !v->source_corresp.name || !v->source_corresp.name_has_been_mangled)
    unsupported("missing canonical source exception typeinfo identity");
  return "R"+string(v->source_corresp.name);
}
