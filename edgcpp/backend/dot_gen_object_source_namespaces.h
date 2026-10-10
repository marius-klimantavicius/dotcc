#ifndef DOT_GEN_OBJECT_SOURCE_NAMESPACES_H
#define DOT_GEN_OBJECT_SOURCE_NAMESPACES_H
// Included within dot_object_source, after immutable original spelling helpers.
struct SourceNamespace {
  unsigned id=0,parent=0;
  std::string name,error;
  bool unnamed=false,is_inline=false,internal=false,unit_local=false;
  a_source_position position={};
};
struct SourceNamespaceGraph {
  std::map<a_namespace_ptr,unsigned> original;
  std::vector<SourceNamespace> declarations;
};
inline SourceNamespaceGraph &source_namespace_graph() {
  static SourceNamespaceGraph graph;return graph;
}
inline unsigned capture_source_namespace(a_namespace_ptr original,unsigned depth=0) {
  if(!original)return 0;
  auto &graph=source_namespace_graph();auto prior=graph.original.find(original);
  if(prior!=graph.original.end())return prior->second;
  unsigned id=unsigned(graph.declarations.size()+1);graph.original[original]=id;graph.declarations.emplace_back();
  SourceNamespace value;value.id=id;value.position=original->source_corresp.decl_position;
  value.unnamed=!original->source_corresp.name || original->source_corresp.unnamed_entity_given_fabricated_name;
  value.name=value.unnamed?std::string():spelling(original->source_corresp);
  value.is_inline=original->is_inline;value.internal=original->has_internal_linkage;
  if(depth>128)value.error="original namespace graph is cyclic or too deep";
  else if(original->is_namespace_alias)value.error="original declaration scope refers to namespace alias rather than owned namespace";
  else {
    auto scope=original->source_corresp.parent_scope;
    if(scope && scope->kind==sck_namespace)value.parent=capture_source_namespace(scope->variant.assoc_namespace,depth+1);
    else if(scope && scope->kind!=sck_file)value.error="original namespace has nonnamespace declaration parent";
    value.unit_local=value.unnamed || (value.parent && graph.declarations.at(value.parent-1).unit_local);
    if(value.internal && !value.unit_local)value.error="internal original namespace has no unnamed ancestor";
    if(!value.unnamed && value.name.empty())value.error="named original namespace lost its spelling";
  }
  graph.declarations[id-1]=value;return id;
}
inline unsigned capture_source_namespace_owner(const a_source_correspondence &source) {
  for(auto scope=source.parent_scope;scope;scope=scope->parent)
    if(scope->kind==sck_namespace)return capture_source_namespace(scope->variant.assoc_namespace);
  return 0;
}
// il_def.h: the frontend-only template_symbol identifies the exact
// template owning a type formal. A formal's own parent_scope may be
// absent, so retain namespace ownership while that original symbol is valid.
inline unsigned capture_parameter_template_namespace(a_type_ptr type) {
  if(type->kind!=tk_template_param || type->variant.template_param.kind!=tptk_param)return 0;
  auto extra=type->variant.template_param.extra_info;
  auto symbol=extra?extra->template_symbol:nullptr;
  if(!symbol || !is_template_symbol(symbol) ||
      !symbol->variant.template_info || !symbol->variant.template_info->il_template_entry)return 0;
  return capture_source_namespace_owner(symbol->variant.template_info->il_template_entry->source_corresp);
}
#endif
