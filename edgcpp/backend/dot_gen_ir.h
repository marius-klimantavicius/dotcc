// Backend-owned physical type graph. No EDG pointer or IL declaration is stored.
// This first migration slice does not yet own operations, symbols or lifetimes.
namespace dot_ir {
using TypeId = unsigned;
struct Field {
  std::string name;
  TypeId type = 0;
  unsigned long long offset = 0, bit_offset = 0, bit_width = 0;
  bool bitfield = false;
};
struct Type {
  std::string kind, name;
  unsigned long long size = 0, alignment = 0, count = 0;
  unsigned qualifiers = 0;
  bool complete = false, variadic = false;
  TypeId element = 0, result = 0;
  std::vector<TypeId> parameters;
  std::vector<Field> fields;
};
struct Graph {
  // Zero is the absent edge, never an object type.
  std::vector<Type> types = std::vector<Type>(1);
  void clear() {types.assign(1,Type{});}
  TypeId reserve() {types.emplace_back();return (TypeId)types.size()-1;}
  bool edge(TypeId id) const {return id>0 && id<types.size();}
  std::string verify() const {
    for(TypeId id=1;id<types.size();++id) {
      const auto &t=types[id];
      if(t.kind.empty())return "unfinished type "+std::to_string(id);
      if(t.alignment && (t.alignment&(t.alignment-1)))return "non-power-of-two type alignment";
      if(t.kind=="pointer" && (!edge(t.element) || t.size!=8))return "invalid pointer representation";
      if(t.kind=="array") {
        if(!edge(t.element))return "missing array element";
        const auto &e=types[t.element];
        if(t.complete && (!e.size || t.size%e.size || t.size/e.size!=t.count))return "array extent/stride mismatch";
      }
      if(t.kind=="function") {
        if(!edge(t.result))return "missing function result";
        for(auto parameter:t.parameters)if(!edge(parameter))return "missing function parameter";
      }
      for(const auto &f:t.fields) {
        if(!edge(f.type))return "missing record field type";
        if(f.offset>t.size)return "field offset outside record";
        if(f.bitfield) {
          if(f.bit_offset>7 || f.bit_width>64 || f.bit_offset>8*(t.size-f.offset) || f.bit_width>8*(t.size-f.offset)-f.bit_offset)
            return "bitfield outside record";
        } else if(types[f.type].size>t.size-f.offset)return "field extent outside record";
      }
    }
    // Recursive pointers are legal; a cycle composed of embedded objects is
    // not. Kahn's algorithm avoids recursion on large externally loaded graphs.
    std::vector<unsigned> pending(types.size(),0);
    std::vector<std::vector<TypeId>> users(types.size());
    auto dependency=[&](TypeId owner,TypeId child) {++pending[owner];users[child].push_back(owner);};
    for(TypeId id=1;id<types.size();++id) {
      const auto &t=types[id];
      if(t.kind=="array" && t.complete)dependency(id,t.element);
      if(t.kind=="record" || t.kind=="union")for(const auto &f:t.fields)dependency(id,f.type);
    }
    std::vector<TypeId> ready;
    for(TypeId id=1;id<types.size();++id)if(!pending[id])ready.push_back(id);
    for(size_t i=0;i<ready.size();++i)for(auto user:users[ready[i]])if(--pending[user]==0)ready.push_back(user);
    if(ready.size()+1!=types.size())return "recursive embedded-object layout";
    return "";
  }
};
std::string quote(const std::string &value) {
  const char *hex="0123456789abcdef";
  std::string result="\"";
  for(char raw:value) {
    auto c=static_cast<unsigned char>(raw);
    if(c=='"' || c=='\\'){result+='\\';result+=(char)c;}
    else if(c<32){result+="\\u00";result+=hex[c>>4];result+=hex[c&15];}
    else result+=(char)c;
  }
  return result+'"';
}
std::string serialize(const Graph &graph,const std::string &target) {
  std::string text="{\"schema\":1,\"scope\":\"physical-types-only\",\"target\":"+quote(target)+",\"types\":[";
  for(TypeId id=1;id<graph.types.size();++id) {
    const auto &t=graph.types[id];if(id>1)text+=',';
    text+="{\"id\":"+std::to_string(id)+",\"kind\":"+quote(t.kind)+",\"name\":"+quote(t.name)+
      ",\"size\":"+std::to_string(t.size)+",\"alignment\":"+std::to_string(t.alignment)+
      ",\"qualifiers\":"+std::to_string(t.qualifiers)+",\"complete\":"+(t.complete?"true":"false")+
      ",\"element\":"+std::to_string(t.element)+",\"count\":"+std::to_string(t.count)+
      ",\"result\":"+std::to_string(t.result)+",\"variadic\":"+(t.variadic?"true":"false")+",\"parameters\":[";
    for(size_t i=0;i<t.parameters.size();++i){if(i)text+=',';text+=std::to_string(t.parameters[i]);}
    text+="],\"fields\":[";
    for(size_t i=0;i<t.fields.size();++i) {
      const auto &f=t.fields[i];if(i)text+=',';
      text+="{\"name\":"+quote(f.name)+",\"type\":"+std::to_string(f.type)+",\"offset\":"+std::to_string(f.offset)+
        ",\"bitfield\":"+(f.bitfield?"true":"false")+",\"bitOffset\":"+std::to_string(f.bit_offset)+",\"bitWidth\":"+std::to_string(f.bit_width)+"}";
    }
    text+="]}";
  }
  return text+"]}";
}
}
