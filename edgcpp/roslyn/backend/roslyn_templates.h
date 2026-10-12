/* Typed template adapters; included inside EDG namespace after common callbacks. */
static Rid roslyn_instance(const void *pointer,Rid result,a_template_ptr templ,a_template_arg_ptr primary,a_template_arg_ptr partial) {
 bool fresh;Rid id=DB.claim(pointer,"template_instance",fresh);if(!fresh)return id;DB.put("template_instance",{{"id",id},{"result_id",result},{"template_id",R(roslyn_template(templ))},{"state_id",K("instantiation_state","concrete")}});
 int n=0;for(a_template_arg_ptr a=primary;a;a=a->next)DB.put("template_instance_argument",{{"owner_id",id},{"role_id",K("template_argument_role","primary")},{"ordinal",n++},{"argument_id",roslyn_argument(a)}});n=0;for(a_template_arg_ptr a=partial;a;a=a->next)DB.put("template_instance_argument",{{"owner_id",id},{"role_id",K("template_argument_role","selected_partial")},{"ordinal",n++},{"argument_id",roslyn_argument(a)}});return id;
}
static Rid roslyn_template(a_template_ptr p) {
 bool fresh;Rid id=DB.claim(p,"template",fresh);if(!fresh)return id;DB.put("template_definition",{{"id",id},{"kind_id",roslyn_kind_template(p->kind)},{"name_id",roslyn_name(p->source_corresp)},{"owner_id",R(roslyn_owner(p->source_corresp))},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position))}});return id;
}
static Rid roslyn_argument(a_template_arg_ptr p) {
 bool fresh;Rid id=DB.claim(p,"template_argument",fresh);if(!fresh)return id;Rid type=0,constant=0,templ=0;switch(p->kind){case tak_type:type=roslyn_type(p->variant.type);break;case tak_nontype:constant=roslyn_constant(p->variant.constant);break;case tak_template:templ=roslyn_template(p->variant.templ.ptr);break;case tak_start_of_pack_expansion:break;}
 DB.put("template_argument",{{"id",id},{"kind_id",roslyn_kind_template_argument(p->kind)},{"type_id",R(type)},{"constant_id",R(constant)},{"template_id",R(templ)},{"is_pack",static_cast<int>(p->is_pack)}});return id;
}
