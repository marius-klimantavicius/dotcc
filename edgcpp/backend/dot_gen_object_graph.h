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
#include "scope_stk.h"
#include "exprutil.h"
#include "dot_gen_object_field_state.h"
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
inline void varuint(std::string &destination,unsigned long long value) {
  while(value>=128) {destination+=char((value&127)|128);value>>=7;}
  destination+=char(value);
}
inline bool expanded_fields() {
  static bool value=[](){auto setting=std::getenv("EDG_DOT_GRAPH_EXPANDED_FIELDS");return setting && std::strcmp(setting,"1")==0;}();
  return value;
}
inline bool flat_fields() {
  static bool value=[](){auto setting=std::getenv("EDG_DOT_GRAPH_FLAT_FIELDS");return setting && std::strcmp(setting,"1")==0;}();
  return value;
}
inline unsigned varuint_size(unsigned long long value) {
  unsigned size=1;while(value>=128){++size;value>>=7;}return size;
}
// Public graph-groups-v1 grammar (counts and IDs are shortest ULEB64):
// layoutCount; each layout: fieldCount,(definitionId,ordinal)*;
// groupCount; each group: nodeId,layoutId,payload* in that layout's order.
// Layout IDs are implicit 1..layoutCount. Scalar/ref/length-prefixed payloads
// have exactly graph-fields-v1 encodings; decoded row order is unchanged.
// The private spool framing is two little-endian uint64s: length,rowCount.
struct FieldBlocks {
  FILE *file=nullptr;std::string pending;unsigned pending_rows=0;
  typedef std::vector<std::pair<unsigned,unsigned>> Layout;
  struct Group {unsigned node;Layout layout;std::string payload;};
  std::vector<Group> groups;size_t pending_logical_bytes=0;
  unsigned long long rows=0,bytes=0,blocks=0;
  void block(const char *data,size_t size,unsigned count) {
    if(!file)file=std::tmpfile();
    if(!file){std::fputs("dotobj: cannot create typed field block spool\n",stderr);std::abort();}
    unsigned char header[16];
    for(unsigned i=0;i<8;++i){header[i]=(static_cast<unsigned long long>(size)>>(i*8))&255;header[i+8]=(static_cast<unsigned long long>(count)>>(i*8))&255;}
    if(std::fwrite(header,1,16,file)!=16 || std::fwrite(data,1,size,file)!=size)std::abort();
    ++blocks;bytes+=size;rows+=count;
  }
  void flush() {
    if(!pending_rows)return;
    if(flat_fields())block(pending.data(),pending.size(),pending_rows);
    else {
      // Block-local layouts describe exact ordered field/ordinal sequences.
      // They do not compare, normalize or discard any EDG observations.
      std::map<Layout,unsigned> layouts;
      std::vector<const Layout*> ordered;
      std::vector<unsigned> group_layouts;
      for(const auto &group:groups) {
        auto entry=layouts.emplace(group.layout,unsigned(layouts.size()+1));
        if(entry.second)ordered.push_back(&entry.first->first);
        group_layouts.push_back(entry.first->second);
      }
      std::string encoded;encoded.reserve(pending_logical_bytes);
      varuint(encoded,ordered.size());
      for(auto layout:ordered) {
        varuint(encoded,layout->size());
        for(const auto &field:*layout){varuint(encoded,field.first);varuint(encoded,field.second);}
      }
      varuint(encoded,groups.size());
      for(size_t index=0;index<groups.size();++index) {
        const auto &group=groups[index];varuint(encoded,group.node);varuint(encoded,group_layouts[index]);encoded+=group.payload;
      }
      block(encoded.data(),encoded.size(),pending_rows);
    }
    pending.clear();groups.clear();pending_rows=0;pending_logical_bytes=0;
  }
  void row(unsigned node,unsigned definition,unsigned ordinal,const std::string &payload) {
    size_t logical_size=varuint_size(node)+varuint_size(definition)+varuint_size(ordinal)+payload.size();
    if(pending_rows && pending_logical_bytes+logical_size>65536)flush();
    if(flat_fields()) {
      varuint(pending,node);varuint(pending,definition);varuint(pending,ordinal);pending+=payload;
    } else {
      if(groups.empty() || groups.back().node!=node)groups.push_back({node,{}, {}});
      auto &group=groups.back();group.layout.emplace_back(definition,ordinal);group.payload+=payload;
    }
    ++pending_rows;pending_logical_bytes+=logical_size;
    if(logical_size>65536)flush();
  }
  void write(FILE *destination) {
    flush();std::fputc('[',destination);
    if(file) {
      std::rewind(file);unsigned char header[16];unsigned long long ordinal=0;size_t header_size;
      while((header_size=std::fread(header,1,16,file))==16) {
        unsigned long long length=0,count=0;
        for(unsigned i=0;i<8;++i){length|=(unsigned long long)header[i]<<(i*8);count|=(unsigned long long)header[i+8]<<(i*8);}
        if(ordinal)std::fputc(',',destination);
        std::fprintf(destination,"{\"blockOrdinal\":%llu,\"rowCount\":%llu,\"encoding\":\"%s\",\"dataHex\":\"",ordinal++,count,flat_fields()?"graph-fields-v1":"graph-groups-v1");
        char buffer[4096],hex[8192];const char *digits="0123456789abcdef";
        while(length) {
          size_t size=length>sizeof(buffer)?sizeof(buffer):size_t(length);
          if(std::fread(buffer,1,size,file)!=size)std::abort();
          for(size_t i=0;i<size;++i){unsigned char byte=buffer[i];hex[i*2]=digits[byte>>4];hex[i*2+1]=digits[byte&15];}
          if(std::fwrite(hex,2,size,destination)!=size)std::abort();length-=size;
        }
        std::fputs("\"}",destination);
      }
      if(header_size || std::ferror(file))std::abort();
    }
    std::fputc(']',destination);
  }
  ~FieldBlocks(){if(file)std::fclose(file);}
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
  Spool field_definitions;FieldBlocks field_blocks;std::string encoded_field;unsigned current_field_definition=0;
  std::map<std::tuple<unsigned,std::string,std::string,int>,unsigned> field_definition_ids;
  std::map<std::string,unsigned> field_ordinals;
  std::string field_prefix;
  struct FrontendUse {unsigned node;an_il_entry_kind kind;std::string path;};
  std::map<const void*,std::vector<FrontendUse>> pack_uses;
  std::map<const void*,unsigned> pack_projection_ids;
  std::set<const void*> inline_active;
  std::map<const void*,unsigned> frontend_ids;
  unsigned next_frontend_identity=1;
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
        (unsigned long long)ids.size(),nodes.rows,expanded_fields()?fields.rows:field_blocks.rows+field_blocks.pending_rows,roots.rows,
        nodes.bytes+roots.bytes+(expanded_fields()?fields.bytes:field_blocks.bytes+field_blocks.pending_logical_bytes+field_definitions.bytes));
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
inline bool begin_field(const char *name,const char *kind,int target_kind,std::string &key) {
  auto &g=graph();key=g.field_prefix+field_name(name);if(!g.field_ordinals.emplace(key,0).second)return false;
  if(expanded_fields()){g.coverage.emplace(unsigned(g.current_kind),key,"retained");return true;}
  auto descriptor=std::make_tuple(unsigned(g.current_kind),key,std::string(kind),target_kind);
  auto definition=g.field_definition_ids.emplace(descriptor,unsigned(g.field_definition_ids.size()+1));
  unsigned id=definition.first->second;
  if(definition.second) {
    g.coverage.emplace(unsigned(g.current_kind),key,"retained");
    g.field_definitions.row("{\"id\":"+std::to_string(id)+",\"ownerEntryKind\":"+
      std::to_string(unsigned(g.current_kind))+",\"name\":"+json(key)+",\"valueKind\":"+json(kind)+
      ",\"targetEntryKind\":"+(target_kind<0?"null":std::to_string(target_kind))+"}");
  }
  g.encoded_field.clear();g.current_field_definition=id;
  return true;
}
inline void field(const char *name,const char *kind,const std::string &value,unsigned target=0,int target_kind=-1) {
  auto &g=graph();std::string key;if(!begin_field(name,kind,target_kind,key))return;
  if(!expanded_fields()) {
    if(target_kind>=0)varuint(g.encoded_field,target);
    else {varuint(g.encoded_field,value.size());g.encoded_field+=value;}
    g.field_blocks.row(g.current,g.current_field_definition,0,g.encoded_field);return;
  }
  std::string encoded=value;
  if(std::strcmp(kind,"bytes")==0) {
    encoded.clear();const char *digits="0123456789abcdef";
    for(unsigned char byte:value){encoded+=digits[byte>>4];encoded+=digits[byte&15];}
  }
  unsigned ordinal=0;
  g.fields.row("{\"nodeId\":"+std::to_string(g.current)+",\"name\":"+json(key)+",\"ordinal\":"+std::to_string(ordinal)+
    ",\"valueKind\":"+json(kind)+",\"value\":"+(target_kind<0?json(encoded):"null")+
    ",\"targetId\":"+(target?std::to_string(target):"null")+",\"targetEntryKind\":"+
    (target_kind<0?"null":std::to_string(target_kind))+"}");
}
inline void integer_field(const char *name,unsigned long long value,bool is_signed,bool is_boolean=false) {
  auto &g=graph();const char *kind=is_boolean?"boolean":is_signed?"signed":"unsigned";
  std::string key;if(!begin_field(name,kind,-1,key))return;
  if(expanded_fields()) {
    auto text=is_signed?std::to_string(static_cast<long long>(value)):std::to_string(value);
    g.fields.row("{\"nodeId\":"+std::to_string(g.current)+",\"name\":"+json(key)+",\"ordinal\":0,\"valueKind\":"+
      json(kind)+",\"value\":"+json(text)+",\"targetId\":null,\"targetEntryKind\":null}");return;
  }
  if(is_boolean)g.encoded_field+=char(value);
  else {
    if(is_signed)value=(value<<1)^(0ULL-(value>>63));
    varuint(g.encoded_field,value);
  }
  g.field_blocks.row(g.current,g.current_field_definition,0,g.encoded_field);
}
inline void reference(const char *name,const void *pointer,an_il_entry_kind kind,bool primary=false) {
  // A null tagged pointer can have iek_none. There is no referenced node.
  field(name,"reference","",intern(pointer,kind,primary),int(kind));
}
template<class T,bool IsEnum=std::is_enum<T>::value> struct ScalarBase {typedef T type;};
template<class T> struct ScalarBase<T,true> {typedef typename std::underlying_type<T>::type type;};
template<class T> inline typename std::enable_if<std::is_integral<T>::value || std::is_enum<T>::value>::type
scalar(const char *name,T value,unsigned index=~0U) {
  std::string key=name;if(index!=~0U)key+='['+std::to_string(index)+']';
  integer_field(key.c_str(),static_cast<unsigned long long>(value),std::is_signed<typename ScalarBase<T>::type>::value,std::is_same<T,bool>::value);
}
template<class T> inline typename std::enable_if<!std::is_integral<T>::value && !std::is_enum<T>::value>::type
scalar(const char *name,const T &,unsigned=~0U) {
  graph().coverage.emplace(unsigned(graph().current_kind),name,"gap");
}
inline void string_field(const char *name,const char *pointer,an_il_entry_kind kind,sizeof_t length) {
  if(!pointer)return;
  if(kind!=iek_string_text) {field(name,"text",pointer);return;}
  field(name,"bytes",std::string(pointer,length));
}
inline void observe_frontend_field(const char *,a_pack_expansion_descr_ptr,int);
inline void observe_frontend_field(const char *,an_expr_rescan_info_entry_ptr,int);
inline void observe_frontend_field(const char *,an_arg_operand_ptr,int);
inline void observe_frontend_field(const char *,an_init_component_ptr,int);
inline void frontend_symbol_reference(const char *,a_symbol_ptr);
inline void observe_variable_uninitialized(a_variable_ptr pointer) {
  bool available=field_observation::variable_uninitialized_available(pointer);
  scalar("uninitialized.available",available);
  if(available)scalar("uninitialized",pointer->uninitialized);
  else graph().coverage.emplace(unsigned(graph().current_kind),"uninitialized","gap",
    "No assignment observed: pinned clear_variable leaves this storage bit uninitialized; no value read or synthesized");
}
template<class T> inline auto observe_frontend_field(const char *name,T *pointer,int)
  -> decltype(type_to_il_entry_kind<typename std::remove_cv<T>::type>(), void()) {
  reference(name,pointer,type_to_il_entry_kind<typename std::remove_cv<T>::type>());
}
template<class T> inline void observe_frontend_field(const char *name,T *,long) {
  // Frontend-private structures need their own declared representation before
  // any fields can be retained; never export the address or raw object bytes.
  graph().coverage.emplace(unsigned(graph().current_kind),graph().field_prefix+field_name(name),"gap");
}
#include "dot_gen_object_graph_walk.h"
struct InlineFields {
  std::string previous;
  explicit InlineFields(const std::string &name):previous(graph().field_prefix) {
    graph().field_prefix+=field_name(name)+".";
  }
  ~InlineFields(){graph().field_prefix=previous;}
};
inline void frontend_gap(const char *name,const char *detail) {
  auto &g=graph();g.coverage.emplace(unsigned(g.current_kind),g.field_prefix+name,"gap",detail);
}
inline unsigned frontend_identity(const void *pointer) {
  if(!pointer)return 0;
  auto &g=graph();auto found=g.frontend_ids.find(pointer);
  if(found!=g.frontend_ids.end())return found->second;
  unsigned id=g.next_frontend_identity++;g.frontend_ids.emplace(pointer,id);return id;
}
inline void frontend_symbol_reference(const char *name,a_symbol_ptr symbol) {
  InlineFields prefix(name);scalar("identity",frontend_identity(symbol));if(!symbol)return;
  scalar("kind",symbol->kind);
  if(symbol->header && symbol->header->identifier)
    field("identifier","bytes",std::string(symbol->header->identifier,symbol->header->identifier_length));
  // Read the actual symbol union tag; do not resolve aliases or look up names.
  switch(symbol->kind) {
    case sk_type:reference("variant.type.ptr",symbol->variant.type.ptr,iek_type);break;
    case sk_constant:reference("variant.constant",symbol->variant.constant,iek_constant);break;
    case sk_variable:reference("variant.variable.ptr",symbol->variant.variable.ptr,iek_variable);break;
    case sk_field:reference("variant.field.ptr",symbol->variant.field.ptr,iek_field);break;
    case sk_routine:case sk_member_function:reference("variant.routine.ptr",symbol->variant.routine.ptr,iek_routine);break;
    case sk_class_template:case sk_function_template:
      if(symbol->variant.template_info)reference("variant.template_info.il_template_entry",symbol->variant.template_info->il_template_entry,iek_template);
      break;
    default:frontend_gap("variant","Frontend symbol variant has no declared IL target representation");break;
  }
  frontend_gap("frontend_state","Symbol declaration/lookup bookkeeping is not a standalone retained node");
}
inline void frontend_template_parameter(const char *name,a_template_param_ptr parameter) {
  InlineFields prefix(name);scalar("identity",frontend_identity(parameter));if(!parameter)return;
  scalar("next.identity",frontend_identity(parameter->next));
#define DOT_TEMPLATE_PARAMETER_SCALAR(member) scalar(#member,parameter->member)
  DOT_TEMPLATE_PARAMETER_SCALAR(has_default_arg);DOT_TEMPLATE_PARAMETER_SCALAR(def_arg_involves_template_param);
  DOT_TEMPLATE_PARAMETER_SCALAR(def_arg_has_not_been_scanned);DOT_TEMPLATE_PARAMETER_SCALAR(def_arg_from_other_decl);
  DOT_TEMPLATE_PARAMETER_SCALAR(is_pack);DOT_TEMPLATE_PARAMETER_SCALAR(is_pack_expansion);DOT_TEMPLATE_PARAMETER_SCALAR(is_pack_element);
  DOT_TEMPLATE_PARAMETER_SCALAR(is_empty_pack);DOT_TEMPLATE_PARAMETER_SCALAR(do_prototype_instantiation);
  DOT_TEMPLATE_PARAMETER_SCALAR(is_dependent);DOT_TEMPLATE_PARAMETER_SCALAR(used_in_alias);DOT_TEMPLATE_PARAMETER_SCALAR(uses_auto);
  DOT_TEMPLATE_PARAMETER_SCALAR(param_num);
#undef DOT_TEMPLATE_PARAMETER_SCALAR
  reference("il_template_parameter",parameter->il_template_parameter,iek_template_parameter);
  frontend_symbol_reference("param_symbol",parameter->param_symbol);
  if(parameter->param_symbol)switch(parameter->param_symbol->kind) {
    case sk_type:
      reference("variant.type",parameter->variant.type,iek_type);
      if(parameter->has_default_arg)reference("default_arg.type",parameter->default_arg.type,iek_type);break;
    case sk_constant:
      reference("variant.constant.ptr",parameter->variant.constant.ptr,iek_constant);
      scalar("variant.constant.type_involves_template_param",parameter->variant.constant.type_involves_template_param);
      if(parameter->has_default_arg)reference("default_arg.constant",parameter->default_arg.constant,iek_constant);break;
    case sk_class_template:
      if(parameter->variant.templ)reference("variant.templ.il_template_entry",parameter->variant.templ->il_template_entry,iek_template);
      if(parameter->has_default_arg)reference("default_arg.templ",parameter->default_arg.templ,iek_template);break;
    default:frontend_gap("variant/default_arg","Frontend formal union tag has no represented IL arm");break;
  }
  if(parameter->cache.tokens.ptr() || parameter->cache.decl_info)
    frontend_gap("cache","Reusable declaration token cache/context has no standalone typed representation");
  if(parameter->def_arg_involves_template_param && (parameter->default_arg_cache.tokens.ptr() || parameter->default_arg_cache.decl_info))
    frontend_gap("default_arg_cache","Reusable default-argument token cache/context has no standalone typed representation");
}
inline void source_position_fields(const char *name,const a_source_position &position) {
  InlineFields prefix(name);scalar("seq",position.seq);scalar("column",position.column);
#if FULLY_RESOLVED_MACRO_POSITIONS
  scalar("orig_column",position.orig_column);scalar("orig_seq",position.orig_seq);
#endif
#if RECORD_MACRO_INVOCATIONS
  scalar("macro_context",position.macro_context);
#endif
}
inline void bind_pack_projection(const void *pointer,unsigned projection) {
  auto &g=graph();g.pack_projection_ids[pointer]=projection;
  auto uses=g.pack_uses.find(pointer);if(uses==g.pack_uses.end())return;
  auto current=g.current;auto kind=g.current_kind;auto prefix=g.field_prefix;
  auto ordinals=std::move(g.field_ordinals);
  for(const auto &use:uses->second) {
    g.current=use.node;g.current_kind=use.kind;g.field_prefix=use.path;g.field_ordinals.clear();
    scalar("sourcePackExpansionId",projection);
  }
  g.pack_uses.erase(uses);g.current=current;g.current_kind=kind;g.field_prefix=prefix;g.field_ordinals=std::move(ordinals);
}
inline void observe_frontend_field(const char *name,a_pack_expansion_descr_ptr pointer,int) {
  InlineFields prefix(name);scalar("present",pointer!=nullptr);if(!pointer)return;
  scalar("identity",frontend_identity(pointer));
  auto &g=graph();auto projected=g.pack_projection_ids.find(pointer);
  if(projected!=g.pack_projection_ids.end())scalar("sourcePackExpansionId",projected->second);
  else g.pack_uses[pointer].push_back({g.current,g.current_kind,g.field_prefix});
  scalar("first_token",pointer->first_token);scalar("last_token",pointer->last_token);
  scalar("tentative_pack_expansion_depth",pointer->tentative_pack_expansion_depth);
  scalar("ellipsis_seen",pointer->ellipsis_seen);scalar("is_function_declarator",pointer->is_function_declarator);
  scalar("uses_only_enclosing_packs",pointer->uses_only_enclosing_packs);scalar("uses_any_enclosing_packs",pointer->uses_any_enclosing_packs);
  scalar("is_pack_index",pointer->is_pack_index);scalar("param_symbol_is_template_template",pointer->param_symbol_is_template_template);
  source_position_fields("ellipsis_position",pointer->ellipsis_position);
  reference("param_symbol_type",pointer->param_symbol_type,iek_type);
  if(pointer->param_symbol_header) {
    field("param_symbol_header.identifier","bytes",std::string(pointer->param_symbol_header->identifier,pointer->param_symbol_header->identifier_length));
    frontend_gap("param_symbol_header.symbol","Frontend symbol relationship has no standalone graph representation");
  }
  if(pointer->next || pointer->previous)
    frontend_gap("next/previous","Frontend pack-context list identity has no standalone projection");
  scalar("next.identity",frontend_identity(pointer->next));scalar("previous.identity",frontend_identity(pointer->previous));
  std::set<a_pack_reference_ptr> seen;unsigned ordinal=0;
  for(auto item=pointer->packs_referenced;item;item=item->next,++ordinal) {
    if(!seen.insert(item).second){frontend_gap("packs_referenced.next","Cyclic frontend list relationship requires a represented identity");break;}
    InlineFields item_prefix("packs_referenced["+std::to_string(ordinal)+"]");
    scalar("identity",frontend_identity(item));scalar("next.identity",frontend_identity(item->next));
    scalar("kind",item->kind);scalar("param_or_binding_num",item->param_or_binding_num);
    scalar("token_sequence_number",item->token_sequence_number);scalar("function_or_block_scopes_to_skip",item->function_or_block_scopes_to_skip);
    scalar("uses_enclosing_pack",item->uses_enclosing_pack);scalar("direct_bases",item->direct_bases);
    source_position_fields("position",item->position);
    scalar("coordinates.present",item->coordinates!=nullptr);
    if(item->coordinates){scalar("coordinates.depth",item->coordinates->depth);scalar("coordinates.position",item->coordinates->position);}
    reference("prev_template_arg",item->prev_template_arg,iek_template_arg);
    switch(item->kind) {
      case prk_variable:case prk_binding:reference("curr_argument.variable",item->curr_argument.variable,iek_variable);break;
      case prk_init_capture:reference("curr_argument.field",item->curr_argument.field,iek_field);break;
      case prk_template_param:case prk_bases:reference("curr_argument.template_arg",item->curr_argument.template_arg,iek_template_arg);break;
      case prk_parameter:
        scalar("curr_argument.identity",frontend_identity(item->curr_argument.param_type));
        if(item->param_info)reference("curr_argument.param_type",item->curr_argument.param_type,iek_param_type);
        else frontend_gap("curr_argument","Pointer identity retained; parser/rescan tag and pointed-to contents unavailable");
        break;
    }
    frontend_symbol_reference("symbol",item->symbol);
    frontend_symbol_reference("primary_pack_symbol",item->primary_pack_symbol);
    if(item->param_info) {
      std::set<a_variadic_param_info_ptr> seen_parameters;unsigned parameter_ordinal=0;
      for(auto parameter=item->param_info;parameter;parameter=parameter->next,++parameter_ordinal) {
        if(!seen_parameters.insert(parameter).second){frontend_gap("param_info.next","Cyclic frontend descriptor list needs a represented identity");break;}
        InlineFields parameter_prefix("param_info["+std::to_string(parameter_ordinal)+"]");
        scalar("level",parameter->level);reference("param_type",parameter->param_type,iek_param_type);
        reference("orig_param_type",parameter->orig_param_type,iek_param_type);
      }
      scalar("param_info.count",parameter_ordinal);
    }
    frontend_template_parameter("template_param",item->template_param);
  }
  scalar("packs_referenced.count",ordinal);
}
inline void frontend_operand(const char *name,const an_operand &operand) {
  InlineFields operand_prefix(name);
  reference("type",operand.type,iek_type);
#if OPTIMIZE_VIRTUAL_FUNCTION_CALLS
  reference("orig_routine_type",operand.orig_routine_type,iek_type);
#endif
#define DOT_OPERAND_SCALAR(member) scalar(#member,operand.member)
  DOT_OPERAND_SCALAR(kind);DOT_OPERAND_SCALAR(state);DOT_OPERAND_SCALAR(bound_function);
  DOT_OPERAND_SCALAR(selector_is_object_pointer);DOT_OPERAND_SCALAR(virtual_function);
  DOT_OPERAND_SCALAR(is_id_expression);DOT_OPERAND_SCALAR(is_address_of_id_expression);DOT_OPERAND_SCALAR(id_expression_was_parenthesized);
  DOT_OPERAND_SCALAR(is_qualified_name);DOT_OPERAND_SCALAR(is_microsoft_deferred_name);DOT_OPERAND_SCALAR(access_control_error_reported);
  DOT_OPERAND_SCALAR(is_operand_of_address_of);DOT_OPERAND_SCALAR(has_required_ptr_to_member_form);DOT_OPERAND_SCALAR(is_template_id);
  DOT_OPERAND_SCALAR(is_simple_string_literal);DOT_OPERAND_SCALAR(conditional_simple_string_literal);DOT_OPERAND_SCALAR(is_cfront_null_pointer_constant);
  DOT_OPERAND_SCALAR(is_name_followed_by_left_paren);DOT_OPERAND_SCALAR(is_dummy_lvalue);DOT_OPERAND_SCALAR(is_parenthesized);
  DOT_OPERAND_SCALAR(name_reference_set);DOT_OPERAND_SCALAR(caused_template_instantiation);DOT_OPERAND_SCALAR(pending_capture);DOT_OPERAND_SCALAR(ruled_out_expr_kinds);
#if MICROSOFT_EXTENSIONS_ALLOWED
  DOT_OPERAND_SCALAR(is_microsoft_noop);DOT_OPERAND_SCALAR(allow_addr_of_managed_member);
#endif
#undef DOT_OPERAND_SCALAR
  source_position_fields("position",operand.position);
#if EXTRA_SOURCE_POSITIONS_IN_IL
  source_position_fields("end_position",operand.end_position);
#endif
  source_position_fields("id_position",operand.id_position);source_position_fields("ampersand_position",operand.ampersand_position);
  reference("template_arg_list",operand.template_arg_list,iek_template_arg);
  if(operand.name_reference_set){InlineFields nested("name_reference");capture_entry((char*)&operand.name_reference,iek_name_reference);}
  observe_frontend_field("pack_expansion_descr",operand.pack_expansion_descr,0);
  if(operand.ref_entries_list)frontend_gap("ref_entries_list","Frontend reference-use bookkeeping has no standalone graph representation");
  if(operand.saved_ref_entries_list)frontend_gap("saved_ref_entries_list","Frontend reference-use bookkeeping has no standalone graph representation");
  frontend_symbol_reference("symbol",operand.symbol);
  switch(operand.kind) {
    case ok_expression:reference("variant.expression",operand.variant.expression,iek_expr_node);break;
    case ok_constant:{InlineFields nested("variant.constant");capture_entry((char*)&operand.variant.constant,iek_constant);break;}
    case ok_braced_init_list:observe_frontend_field("variant.braced_init_list",operand.variant.braced_init_list,0);break;
#if MICROSOFT_EXTENSIONS_ALLOWED
    case ok_property_ref:reference("variant.property_ref.object",operand.variant.property_ref.object,iek_expr_node);observe_frontend_field("variant.property_ref.subscripts",operand.variant.property_ref.subscripts,0);break;
    case ok_event_ref:reference("variant.event_ref.object",operand.variant.event_ref.object,iek_expr_node);break;
#endif
    default:break;
  }
}
inline void observe_frontend_field(const char *name,an_expr_rescan_info_entry_ptr pointer,int) {
  InlineFields prefix(name);scalar("present",pointer!=nullptr);if(!pointer)return;
  scalar("identity",frontend_identity(pointer));
  auto &g=graph();if(!g.inline_active.insert(pointer).second){frontend_gap("identity","Shared recursive frontend descriptor needs a standalone identity");return;}
  scalar("expression_kind",pointer->expression_kind);scalar("operator_token_sequence_number",pointer->operator_token_sequence_number);
  source_position_fields("operator_position",pointer->operator_position);
  source_position_fields("secondary_operator_position",pointer->secondary_operator_position);
  reference("type",pointer->type,iek_type);
  frontend_operand("saved_operand",pointer->saved_operand);
  g.inline_active.erase(pointer);
}
inline void observe_frontend_field(const char *name,an_arg_operand_ptr pointer,int) {
  InlineFields prefix(name);scalar("identity",frontend_identity(pointer));
  unsigned ordinal=0;std::set<an_arg_operand_ptr> seen;
  for(auto item=pointer;item;item=item->next,++ordinal) {
    if(!seen.insert(item).second)break;
    InlineFields element("items["+std::to_string(ordinal)+"]");
    scalar("identity",frontend_identity(item));scalar("next.identity",frontend_identity(item->next));
    if(!graph().inline_active.insert(item).second) {
      frontend_gap("operand","Recursive frontend operand content is represented at its enclosing observation");
      continue;
    }
    frontend_operand("operand",item->operand);
    graph().inline_active.erase(item);
  }
  scalar("count",ordinal);
}
inline void observe_frontend_field(const char *name,an_init_component_ptr pointer,int) {
  InlineFields prefix(name);scalar("identity",frontend_identity(pointer));
  unsigned ordinal=0;std::set<an_init_component_ptr> seen;
  for(auto item=pointer;item;item=item->next,++ordinal) {
    if(!seen.insert(item).second)break;
    InlineFields element("items["+std::to_string(ordinal)+"]");
    scalar("identity",frontend_identity(item));scalar("next.identity",frontend_identity(item->next));
    if(!graph().inline_active.insert(item).second) {
      frontend_gap("content","Recursive initializer content is represented at its enclosing observation");
      continue;
    }
#define DOT_INIT_COMPONENT_SCALAR(member) scalar(#member,item->member)
    DOT_INIT_COMPONENT_SCALAR(kind);DOT_INIT_COMPONENT_SCALAR(bundled);
    DOT_INIT_COMPONENT_SCALAR(detached_ref_entries);DOT_INIT_COMPONENT_SCALAR(contains_designator);
    DOT_INIT_COMPONENT_SCALAR(check_narrowing);DOT_INIT_COMPONENT_SCALAR(braced_init_in_parentheses);
    DOT_INIT_COMPONENT_SCALAR(permanently_allocated);
#if CHECKING
    DOT_INIT_COMPONENT_SCALAR(on_free_list);
#endif
    DOT_INIT_COMPONENT_SCALAR(constant_expr_ruled_out);DOT_INIT_COMPONENT_SCALAR(consteval_function_designator_seen);
    DOT_INIT_COMPONENT_SCALAR(preserved_deduced_pack);DOT_INIT_COMPONENT_SCALAR(direct_init_designator);
#undef DOT_INIT_COMPONENT_SCALAR
    observe_frontend_field("pack_expansion_descr",item->pack_expansion_descr,0);
    switch(item->kind) {
      case ick_expression:
        observe_frontend_field("variant.expr.arg_op",item->variant.expr.arg_op,0);
        reference("variant.expr.lifetime",item->variant.expr.lifetime,iek_object_lifetime);
        break;
      case ick_braced:
        observe_frontend_field("variant.braced.list",item->variant.braced.list,0);
        source_position_fields("variant.braced.start_pos",item->variant.braced.start_pos);
        source_position_fields("variant.braced.end_pos",item->variant.braced.end_pos);
        break;
      case ick_designator: {
        const auto &designator=item->variant.designator;
        scalar("variant.designator.field_name.identity",frontend_identity(designator.field_name));
        if(designator.field_name && designator.field_name->identifier)
          field("variant.designator.field_name.identifier","bytes",std::string(designator.field_name->identifier,designator.field_name->identifier_length));
        reference("variant.designator.element_index",designator.element_index,iek_constant);
        reference("variant.designator.last_element_index",designator.last_element_index,iek_constant);
        reference("variant.designator.resolved_field",designator.resolved_field,iek_field);
        source_position_fields("variant.designator.position",designator.position);
        break;
      }
      case ick_continued:
        scalar("variant.continuation.state.identity",frontend_identity(item->variant.continuation.state));
        frontend_gap("variant.continuation.state","Opaque frontend parser suspension/resumption state; pointed-to contents not captured");
        break;
    }
    graph().inline_active.erase(item);
  }
  scalar("count",ordinal);
}
inline void requires_substitutions(an_expr_node_ptr expression) {
  if(expression->kind!=enk_requires)return;
  InlineFields prefix("observation.requires_substitutions");
  scalar("map_present",requires_expr_substs!=nullptr);if(!requires_expr_substs)return;
  const auto &substitutions=requires_expr_substs->get(expression);unsigned ordinal=0;
  for(auto pair=substitutions.begin();pair!=substitutions.end();++pair,++ordinal) {
    InlineFields item("pairs["+std::to_string(ordinal)+"]");
    scalar("args_known_dependent",pair->args_known_dependent);scalar("args_known_nondependent",pair->args_known_nondependent);
    scalar("adjust_coordinates",pair->adjust_coordinates);scalar("alias_deduction_guide",pair->alias_deduction_guide);
    reference("args",pair->args,iek_template_arg);
    unsigned argument_ordinal=0;for(auto argument=pair->args;argument;argument=argument->next,++argument_ordinal)
      reference(("args["+std::to_string(argument_ordinal)+"]").c_str(),argument,iek_template_arg);
    scalar("argument_count",argument_ordinal);
    unsigned parameter_ordinal=0;for(auto parameter=pair->params;parameter;parameter=parameter->next,++parameter_ordinal)
      frontend_template_parameter(("params["+std::to_string(parameter_ordinal)+"]").c_str(),parameter);
    scalar("parameter_count",parameter_ordinal);
  }
  scalar("count",ordinal);
}
inline void drain() {
  auto &g=graph();if(g.draining)return;g.draining=true;
  // FE descriptors can be recycled between parser observations. Identities
  // denote shared storage only within this read-only observation; projection
  // IDs provide explicit links across observations where already captured.
  g.frontend_ids.clear();
  for(size_t index=0;index<g.pending.size();++index) {
    auto node=g.pending[index];g.current=node.id;g.current_kind=node.kind;g.field_ordinals.clear();
    unsigned identity=g.ids.at(std::make_pair(node.pointer,unsigned(node.kind))).first;
    field("observation.identity","reference","",identity,int(node.kind));
    capture_entry(static_cast<char*>(const_cast<void*>(node.pointer)),node.kind);
    if(node.kind==iek_expr_node)requires_substitutions((an_expr_node_ptr)node.pointer);
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
inline void header_roots() {
  auto &g=graph();g.refresh_targets.clear();
  std::vector<a_source_file_ptr> pending_files;
  if(il_header.primary_source_file)pending_files.push_back(il_header.primary_source_file);
  for(auto entry=il_header.seq_number_lookup_entries;entry;entry=entry->next) {
    g.refresh_targets.insert(entry);
    if(entry->source_file)pending_files.push_back(entry->source_file);
  }
  while(!pending_files.empty()) {
    auto file=pending_files.back();pending_files.pop_back();
    if(!g.refresh_targets.insert(file).second)continue;
    if(file->next)pending_files.push_back(file->next);
    if(file->first_child_file)pending_files.push_back(file->first_child_file);
  }
  root(il_header.primary_source_file,iek_source_file,"il-header.primary-source-file",0,0,true,2);
  root(il_header.seq_number_lookup_entries,iek_seq_number_lookup_entry,"il-header.seq-number-lookup-entries",0,0,true,2);
  g.refresh_targets.clear();
}
inline void write(FILE *destination) {
  header_roots();
  for(const auto &entry:graph().pack_uses)for(const auto &use:entry.second)
    graph().coverage.emplace(unsigned(use.kind),use.path+"sourcePackExpansionId","gap","Descriptor has inline fields but no emitted source-pack projection identity");
  auto &g=graph();register_generated_coverage();
  g.coverage.emplace(0,"$frontendIdentity","configuration","Unsigned identity fields are object-local IDs shared within one frontend observation; zero denotes null. They do not assert object continuity across parser allocation reuse. Existing sourcePackExpansionId fields link emitted source-pack projections across observations.");
  std::fputs(",\"edgGraphNodes\":",destination);g.nodes.write(destination);
  if(expanded_fields()) {
    std::fputs(",\"edgGraphFields\":",destination);g.fields.write(destination);
  } else {
    std::fputs(",\"edgGraphFieldDefinitions\":",destination);g.field_definitions.write(destination);
    std::fputs(",\"edgGraphFieldBlocks\":",destination);g.field_blocks.write(destination);
  }
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
