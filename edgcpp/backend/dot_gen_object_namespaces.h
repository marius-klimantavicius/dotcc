// Included in the owned Writer class; original namespace graph only.
std::set<unsigned> written_source_namespaces;
std::vector<string> source_namespaces,source_namespace_owners;
string source_namespace_identity(unsigned id,unsigned depth=0) {
  if(!id)return "";
  if(depth>128)reject("original namespace identity cycle/depth");
  const auto &value=dot_object_source::source_namespace_graph().declarations.at(id-1);
  if(!value.error.empty())reject(value.error.c_str());
  string parent=value.parent?source_namespace_identity(value.parent,depth+1):"";
  if(value.unnamed) {
    const char *unit=std::getenv("EDG_DOT_OBJECT_UNIT_ID");
    if(!unit || !*unit)reject("unnamed namespace lacks owned translation unit identity");
    return "unnamed:"+source_frame(parent)+source_frame(unit);
  }
  return "named:"+source_frame(parent)+source_frame(value.name);
}
string source_private_namespace(unsigned id) {
  return id && dot_object_source::source_namespace_graph().declarations.at(id-1).unit_local?source_namespace_identity(id):"";
}
string source_namespace_nominal_identity(unsigned id,const string &identity) {
  string owner=source_private_namespace(id);
  return owner.empty()?identity:"namespace-private:"+source_frame(owner)+source_frame(identity);
}
unsigned source_namespace(unsigned id) {
  if(!id)return 0;
  if(!written_source_namespaces.insert(id).second)return id;
  const auto value=dot_object_source::source_namespace_graph().declarations.at(id-1);
  if(!value.error.empty())reject(value.error.c_str());
  unsigned parent=source_namespace(value.parent);
  source_namespaces.push_back("{\"id\":"+number(id)+",\"parentId\":"+number(parent)+",\"sourceName\":"+quote(value.name)+
    ",\"isUnnamed\":"+(value.unnamed?"true":"false")+",\"isInline\":"+(value.is_inline?"true":"false")+
    ",\"hasInternalLinkage\":"+(value.internal?"true":"false")+",\"captureProof\":\"original-namespace-declaration\""+position(value.position)+"}");
  return id;
}
void source_namespace_owner(const char *kind,unsigned owner,unsigned namespace_id) {
  if(!namespace_id)return;
  source_namespace_owners.push_back("{\"ownerKind\":"+quote(kind)+",\"ownerId\":"+number(owner)+
    ",\"namespaceId\":"+number(source_namespace(namespace_id))+",\"captureProof\":\"original-enclosing-namespace\"}");
}
