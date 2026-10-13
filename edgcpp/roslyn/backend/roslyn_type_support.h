/* Read-only retained function exception and qualifier descriptors. */
static Rid roslyn_exception_specification(an_exception_specification_ptr p) {
 bool fresh;Rid id=DB.claim(p,"exception_specification",fresh);if(!fresh)return id;
 a_constant_ptr argument=!p->arg_cached&&!p->copy_from_prototype&&p->is_noexcept?p->variant.noexcept_arg:nullptr;
 bool dependent=argument&&argument->kind==ck_template_param;
 RoslynSqlValue effective;if(p->is_noexcept&&!p->arg_cached&&!p->copy_from_prototype&&!p->indeterminate&&!dependent)effective=static_cast<int>(!p->throw_any);
 DB.put("exception_specification",{{"id",id},{"kind_id",K("exception_specification_kind",p->is_noexcept?"noexcept":"dynamic")},{"noexcept_argument_id",R(roslyn_constant(argument))},{"copy_from_prototype_routine_id",R(!p->arg_cached&&p->copy_from_prototype?roslyn_routine(p->variant.routine):0)},{"source_origin_id",R(roslyn_origin(p->source_range.start,&p->source_range.end))},{"argument_cached",static_cast<int>(p->arg_cached)},{"copy_from_prototype",static_cast<int>(p->copy_from_prototype)},{"from_attribute",static_cast<int>(p->from_attribute)},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"throw_any",static_cast<int>(p->throw_any)},{"noexcept_effective",effective},{"is_dependent",static_cast<int>(dependent)},{"is_indeterminate",static_cast<int>(p->indeterminate)}});
 if(!p->arg_cached&&!p->copy_from_prototype&&!p->is_noexcept){int ordinal=0;for(an_exception_specification_type_ptr t=p->variant.exception_specification_type_list;t;t=t->next)DB.put("exception_specification_type",{{"owner_id",id},{"ordinal",ordinal++},{"type_id",roslyn_type(t->type)},{"is_redundant",static_cast<int>(t->redundant)},{"is_pack_expansion",static_cast<int>(t->is_pack_expansion)},{"source_position_id",R(roslyn_position(t->source_position))}});}
 return id;
}
/* Stock dependency traversal is backend-safe after fe_wrapup has cleared
   the frontend translation-unit context.  It reads retained type/constant
   links and populates EDG's dependency cache; it does not reconstruct types. */
static bool roslyn_type_is_dependent(a_type_ptr p) {
 if(in_front_end||curr_translation_unit!=nullptr){DB.fail("Type dependency query requires completed frontend wrapup");return false;}
 return is_instantiation_dependent_type(p)!=FALSE;
}
