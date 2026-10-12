/* Resident C++ module provenance; no front-end module entity graph. */
static Rid roslyn_module(a_module_ptr p) {
 bool fresh;Rid id=DB.claim(p,"module",fresh);if(!fresh)return id;
 Rid name=0,header=0,unit=0;int system=0,suppress=0,internal=0;
 switch(p->kind){
 case mk_none:break;
 case mk_header_unit:name=DB.atom(p->variant.header_unit.name);header=DB.atom(p->variant.header_unit.resolved_header);system=p->variant.header_unit.is_sys_include;suppress=p->variant.header_unit.suppress_macro_export;break;
 case mk_unit:name=DB.atom(p->variant.unit.name);break;
 case mk_unit_partition:name=DB.atom(p->variant.unit_partition.name);unit=roslyn_module(p->variant.unit_partition.unit);internal=p->variant.unit_partition.is_internal;break;
 default:DB.fail("Invalid retained module kind");break;
 }
 DB.put("module",{{"id",id},{"kind_id",roslyn_kind_edg_module(p->kind)},{"resolved_file_id",R(DB.atom(p->resolved_file))},{"file_kind_id",roslyn_kind_edg_module_file(p->file_kind)},{"contains_unsupported_constructs",static_cast<int>(p->contains_unsupported_constructs)},{"name_id",R(name)},{"resolved_header_id",R(header)},{"unit_id",R(unit)},{"uses_angle_brackets",system},{"suppress_macro_export",suppress},{"is_internal_partition",internal}});
 return id;
}
static Rid roslyn_module_import(a_module_import_decl_ptr p) {
 bool fresh;Rid id=DB.claim(p,"module_import",fresh);if(!fresh)return id;
 DB.put("module_import",{{"id",id},{"module_id",roslyn_module(p->module_info)},{"source_position_id",R(roslyn_position(p->position))},{"module_name_position_id",R(roslyn_position(p->module_name_position))},{"implementation_unit_importing_self",static_cast<int>(p->impl_unit_importing_self)}});
 roslyn_attributes(id,p->attributes);return id;
}
