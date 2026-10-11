#ifndef DOT_GEN_OBJECT_GRAPH_H
#define DOT_GEN_OBJECT_GRAPH_H
// EDG observations are data. This capture never performs language lookup,
// expression normalization, overload selection, or template substitution.
#include <cstdio>
#include <cstdlib>
#include <map>
#include <set>
#include <string>
#include <tuple>
#include <vector>
#include <type_traits>
#include <cstring>
#include "mem_tables.h"
BEGIN_EDG_NAMESPACE
namespace dot_object_source { namespace typed_graph {
inline std::string json(const std::string &s) {
  std::string r="\"";const char *hex="0123456789abcdef";
  for(unsigned char c:s) {
    if(c=='"' || c=='\\'){r+='\\';r+=char(c);}
    else if(c<32){r+="\\u00";r+=hex[c>>4];r+=hex[c&15];}
    else r+=char(c);
  }
  return r+'"';
}
struct Spool {
  FILE *file=nullptr;bool first=true;unsigned long long rows=0,bytes=0;
  void row(const std::string &value) {
    ++rows;bytes+=value.size()+(first?0:1);
    if(!file)file=std::tmpfile();
    if(!file){std::fputs("dotobj: cannot create typed graph spool\n",stderr);std::abort();}
    if(!first)std::fputc(',',file);first=false;
    if(std::fwrite(value.data(),1,value.size(),file)!=value.size())std::abort();
  }
  void write(FILE *destination) {
    std::fputc('[',destination);
    if(file) {
      std::rewind(file);char buffer[65536];size_t count;
      while((count=std::fread(buffer,1,sizeof(buffer),file))!=0)
        if(std::fwrite(buffer,1,count,destination)!=count)std::abort();
      if(std::ferror(file))std::abort();
    }
    std::fputc(']',destination);
  }
  ~Spool(){if(file)std::fclose(file);}
};
struct Node {const void *pointer=nullptr;an_il_entry_kind kind=iek_none;unsigned id=0;};
struct Graph {
  struct Identity {unsigned first=0,latest=0;};
  std::map<std::pair<const void*,unsigned>,Identity> ids;
  std::vector<Node> pending;
  std::map<std::pair<const void*,unsigned>,unsigned> epoch_ids;
  unsigned next_node=1;bool refreshing=false,refresh_file_scope=false;
  unsigned refresh_mode=0;std::set<const void*> refresh_targets;
  Spool nodes,fields,roots;
  std::map<std::string,unsigned> field_ordinals;
  struct Coverage {
    std::map<std::pair<unsigned,std::string>,std::string> entries,details;
    void emplace(unsigned kind,const std::string &name,const std::string &disposition,const std::string &detail="") {
      auto inserted=entries.emplace(std::make_pair(kind,name),disposition);
      if(!inserted.second && disposition=="gap")inserted.first->second="gap";
      if(!detail.empty())details[std::make_pair(kind,name)]=detail;
    }
  } coverage;
  std::set<std::pair<std::string,unsigned>> written_roots;
  unsigned current=0;an_il_entry_kind current_kind=iek_none;bool draining=false;
  ~Graph() {
    if(std::getenv("EDG_DOT_GRAPH_STATS"))
      std::fprintf(stderr,"dotobj typed graph: canonical=%llu nodes=%llu fields=%llu roots=%llu bytes=%llu\n",
        (unsigned long long)ids.size(),nodes.rows,fields.rows,roots.rows,nodes.bytes+fields.bytes+roots.bytes);
  }
};
inline Graph &graph(){static Graph value;return value;}
inline std::string field_name(std::string value) {
  // Remove only the implementation's local cast variable, not field paths.
  size_t at;
  while((at=value.find("eptr->"))!=std::string::npos)value.erase(at,6);
  return value;
}
inline unsigned intern(const void *pointer,an_il_entry_kind kind,bool primary=false) {
  if(!pointer)return 0;
  auto &g=graph();auto key=std::make_pair(pointer,unsigned(kind));
  auto found=g.ids.find(key);
  if(found!=g.ids.end()) {
    if(!primary || !g.refreshing)return found->second.latest;
    auto epoch=g.epoch_ids.find(key);if(epoch!=g.epoch_ids.end())return epoch->second;
    if(g.refresh_mode==1 && !g.refresh_file_scope && il_entry_prefix_of(pointer).file_scope)return found->second.latest;
    if(g.refresh_mode==2 && !g.refresh_targets.count(pointer))return found->second.latest;
  }
  unsigned id=g.next_node++;
  auto identity=g.ids.emplace(key,Graph::Identity{id,id});identity.first->second.latest=id;
  g.epoch_ids.emplace(key,id);
  g.pending.push_back({pointer,kind,id});
  g.nodes.row("{\"id\":"+std::to_string(id)+",\"entryKind\":"+std::to_string(unsigned(kind))+
    ",\"entryKindName\":"+json(il_entry_kind_names[int(kind)])+",\"capturePhase\":\"frontend-observation\"}");
  return id;
}
inline void field(const char *name,const char *kind,const std::string &value,unsigned target=0,int target_kind=-1) {
  auto &g=graph();auto key=field_name(name);if(!g.field_ordinals.emplace(key,0).second)return;unsigned ordinal=0;
  g.coverage.emplace(unsigned(g.current_kind),key,"retained");
  g.fields.row("{\"nodeId\":"+std::to_string(g.current)+",\"name\":"+json(key)+",\"ordinal\":"+std::to_string(ordinal)+
    ",\"valueKind\":"+json(kind)+",\"value\":"+(target_kind<0?json(value):"null")+
    ",\"targetId\":"+(target?std::to_string(target):"null")+",\"targetEntryKind\":"+
    (target_kind<0?"null":std::to_string(target_kind))+"}");
}
inline void reference(const char *name,const void *pointer,an_il_entry_kind kind,bool primary=false) {
  // A null tagged pointer can have iek_none. There is no referenced node.
  field(name,"reference","",intern(pointer,kind,primary),int(kind));
}
template<class T> inline typename std::enable_if<std::is_integral<T>::value || std::is_enum<T>::value>::type
scalar(const char *name,T value,unsigned index=~0U) {
  std::string key=name;if(index!=~0U)key+='['+std::to_string(index)+']';
  if(std::is_signed<T>::value)field(key.c_str(),"signed",std::to_string(static_cast<long long>(value)));
  else field(key.c_str(),"unsigned",std::to_string(static_cast<unsigned long long>(value)));
}
template<class T> inline typename std::enable_if<!std::is_integral<T>::value && !std::is_enum<T>::value>::type
scalar(const char *name,const T &,unsigned=~0U) {
  graph().coverage.emplace(unsigned(graph().current_kind),name,"gap");
}
inline void string_field(const char *name,const char *pointer,an_il_entry_kind kind,sizeof_t length) {
  if(!pointer)return;
  if(kind!=iek_string_text) {field(name,"text",pointer);return;}
  std::string bytes;const char *hex="0123456789abcdef";
  for(sizeof_t i=0;i<length;++i){unsigned char byte=static_cast<unsigned char>(pointer[i]);bytes+=hex[byte>>4];bytes+=hex[byte&15];}
  field(name,"bytes",bytes);
}
template<class T> inline auto observe_frontend_field(const char *name,T *pointer,int)
  -> decltype(type_to_il_entry_kind<typename std::remove_cv<T>::type>(), void()) {
  reference(name,pointer,type_to_il_entry_kind<typename std::remove_cv<T>::type>());
}
template<class T> inline void observe_frontend_field(const char *name,T *,long) {
  // Frontend-private structures need their own declared representation before
  // any fields can be retained; never export the address or raw object bytes.
  graph().coverage.emplace(unsigned(graph().current_kind),field_name(name),"gap");
}
#include "dot_gen_object_graph_walk.h"
inline void drain() {
  auto &g=graph();if(g.draining)return;g.draining=true;
  for(size_t index=0;index<g.pending.size();++index) {
    auto node=g.pending[index];g.current=node.id;g.current_kind=node.kind;g.field_ordinals.clear();
    unsigned identity=g.ids.at(std::make_pair(node.pointer,unsigned(node.kind))).first;
    field("observation.identity","reference","",identity,int(node.kind));
    capture_entry(static_cast<char*>(const_cast<void*>(node.pointer)),node.kind);
  }
  g.pending.clear();g.current=0;g.current_kind=iek_none;g.draining=false;
}
inline unsigned root(const void *pointer,an_il_entry_kind kind,const char *role,unsigned ordinal,unsigned context=0,bool refresh=false,unsigned refresh_mode=0) {
  if(!pointer)return 0;
  auto &capture=graph();capture.epoch_ids.clear();capture.refreshing=refresh;capture.refresh_mode=0;
  unsigned id=intern(pointer,kind,refresh);if(!id)return 0;
  auto &g=graph();g.refresh_mode=refresh_mode;g.refreshing=refresh && refresh_mode!=0;
  g.refresh_file_scope=il_entry_prefix_of(pointer).file_scope;
  if(g.written_roots.emplace(role,ordinal).second)
    g.roots.row("{\"nodeId\":"+std::to_string(id)+",\"contextNodeId\":"+(context?std::to_string(context):"null")+
      ",\"role\":"+json(role)+",\"ordinal\":"+std::to_string(ordinal)+"}");
  drain();g.refreshing=false;return id;
}
inline void scope_boundary(a_scope_ptr scope) {
  static unsigned ordinal=0;
  // Match the stock IL walk boundary: a function memory region never clones
  // the file-scope entries it references. Those have their own observations.
  root(scope,iek_scope,"scope-before-lowering",ordinal++,0,true,1);
}
inline void record_boundary(a_type_ptr type) {
  static unsigned ordinal=0;auto &g=graph();g.refresh_targets.clear();
  g.refresh_targets.insert(type);auto supplement=class_type_supp(type);
  g.refresh_targets.insert(supplement);
  for(auto field=type->variant.class_struct_union.field_list;field;field=field->next)g.refresh_targets.insert(field);
  if(supplement)for(auto base=supplement->base_classes;base;base=base->next) {
    g.refresh_targets.insert(base);
    for(auto derivation=base->derivation;derivation;derivation=derivation->next)g.refresh_targets.insert(derivation);
  }
  root(type,iek_type,"record-before-prelowering",ordinal++,0,true,2);
  g.refresh_targets.clear();
}
inline unsigned context(a_type_ptr type,a_routine_ptr routine,a_type_ptr record,
                        a_template_decl_ptr header,a_template_arg_ptr arguments,
                        const a_source_position &position,unsigned scope_ordinal,unsigned ordinal) {
  auto &capture=graph();capture.refresh_targets.clear();capture.refresh_targets.insert(type);
  auto signature=type->variant.routine.extra_info;capture.refresh_targets.insert(signature);
  if(signature) {
    for(auto parameter=signature->param_type_list;parameter;parameter=parameter->next)capture.refresh_targets.insert(parameter);
    capture.refresh_targets.insert(signature->exception_specification);
  }
  unsigned owner=root(type,iek_type,"routine-declarator-observation",ordinal,0,true,2);
  capture.refresh_targets.clear();
  static unsigned header_ordinal=0,argument_ordinal=0;
  unsigned observed_header=root(header,iek_template_decl,"declarator-header-observation",header_ordinal++,owner,true);
  std::vector<unsigned> observed_arguments;
  for(auto argument=arguments;argument;argument=argument->next)
    observed_arguments.push_back(root(argument,iek_template_arg,"declarator-argument-observation",argument_ordinal++,owner,true));
  auto &g=graph();g.current=owner;g.current_kind=iek_type;g.field_ordinals.clear();
  reference("observation.context.routine",routine,iek_routine);
  reference("observation.context.record",record,iek_type);
  field("observation.context.header","reference","",observed_header,int(iek_template_decl));
  field("observation.context.arguments","reference","",observed_arguments.empty()?0:observed_arguments[0],int(iek_template_arg));
  for(unsigned index=0;index<observed_arguments.size();++index)
    field(("observation.context.argument["+std::to_string(index)+"]").c_str(),"reference","",observed_arguments[index],int(iek_template_arg));
  scalar("observation.context.scope_ordinal",scope_ordinal);
  scalar("observation.context.position.seq",position.seq);
  scalar("observation.context.position.column",position.column);
  drain();return owner;
}
inline void context_factory(unsigned owner,unsigned ordinal,a_routine_ptr routine,
                            a_template_decl_ptr header,a_template_arg_ptr arguments) {
  auto &g=graph();g.current=owner;g.current_kind=iek_type;g.field_ordinals.clear();
  auto prefix="observation.factory["+std::to_string(ordinal)+"].";
  reference((prefix+"routine").c_str(),routine,iek_routine);
  reference((prefix+"header").c_str(),header,iek_template_decl);
  reference((prefix+"arguments").c_str(),arguments,iek_template_arg);
  drain();
}
inline void projection(unsigned node,const char *role,unsigned ordinal) {
  if(node && graph().written_roots.emplace(role,ordinal).second)
    graph().roots.row("{\"nodeId\":"+std::to_string(node)+",\"contextNodeId\":null,\"role\":"+json(role)+",\"ordinal\":"+std::to_string(ordinal)+"}");
}
inline void write(FILE *destination) {
  auto &g=graph();register_generated_coverage();std::fputs(",\"edgGraphNodes\":",destination);g.nodes.write(destination);
  std::fputs(",\"edgGraphFields\":",destination);g.fields.write(destination);
  std::fputs(",\"edgGraphRoots\":",destination);g.roots.write(destination);
  std::fputs(",\"edgGraphCoverage\":[",destination);
  std::fputs("{\"entryKind\":0,\"field\":\"$configuration\",\"disposition\":\"configuration\",\"detail\":\"" DOT_GRAPH_FINGERPRINT "\"}",destination);
  for(unsigned kind=0;kind<unsigned(iek_last);++kind) {
    auto row=",{\"entryKind\":"+std::to_string(kind)+",\"field\":\"$entryKind\",\"disposition\":\"configuration\",\"detail\":"+
      json(il_entry_kind_names[kind])+"}";
    std::fwrite(row.data(),1,row.size(),destination);
  }
  for(const auto &item:g.coverage.entries) {
    auto detail=g.coverage.details.find(item.first);
    auto row=",{\"entryKind\":"+std::to_string(item.first.first)+",\"field\":"+json(item.first.second)+
      ",\"disposition\":"+json(item.second)+",\"detail\":"+json(detail!=g.coverage.details.end()?detail->second:item.second=="retained"?
        "configured EDG field observation":"additional configured field capture required")+"}";
    std::fwrite(row.data(),1,row.size(),destination);
  }
  std::fputc(']',destination);
}
}} // namespace dot_object_source::typed_graph
END_EDG_NAMESPACE
#endif
