// Normalized, transactional storage for one unlowered EDG translation unit.
// Facts reach SQLite through typed records. JSON is only a final encoding for
// nested position/range payloads and graph_metadata values required by schema.
#ifndef EDG_SEMANTIC_SQLITE_H
#define EDG_SEMANTIC_SQLITE_H
#include <sqlite3.h>
#include <cstdio>
#include <cstring>
#include <cstdint>
#include <limits>
#include <map>
#include <stdexcept>
#include <string>
#include <utility>
#include <vector>
#include "semantic_schema_sql.h"

namespace semantic_storage {

// The old semantic contract maps each EDG byte to the same Latin1 code point.
// Retain that mapping explicitly; do not mistake EDG byte strings for UTF-8.
inline std::string latin1_to_utf8(const char *bytes,size_t length) {
  std::string result; result.reserve(length);
  for(size_t n=0;n<length;++n) {
    unsigned char c=static_cast<unsigned char>(bytes[n]);
    if(c<128) result.push_back(static_cast<char>(c));
    else { result.push_back(static_cast<char>(0xc0|(c>>6))); result.push_back(static_cast<char>(0x80|(c&63))); }
  }
  return result;
}
inline std::string latin1_to_utf8(const char *text) {
  return text?latin1_to_utf8(text,std::strlen(text)):std::string();
}

enum class ValueKind { SignedInteger, UnsignedInteger, Boolean, Text, Object, Array, Null };
enum class StoredKind { Integer, Boolean, Text, Json, Null };

struct Value {
  ValueKind kind=ValueKind::Null;
  std::int64_t signed_value=0;
  std::uint64_t unsigned_value=0;
  std::string text_value;
  std::vector<std::pair<std::string,Value>> members;
  std::vector<Value> elements;
  static Value signed_integer(std::int64_t n) { Value v; v.kind=ValueKind::SignedInteger; v.signed_value=n; return v; }
  static Value unsigned_integer(std::uint64_t n) { Value v; v.kind=ValueKind::UnsignedInteger; v.unsigned_value=n; return v; }
  static Value boolean(bool b) { Value v; v.kind=ValueKind::Boolean; v.unsigned_value=b?1:0; return v; }
  static Value text(std::string utf8) { Value v; v.kind=ValueKind::Text; v.text_value=std::move(utf8); return v; }
  static Value latin1(const char *bytes,size_t length) { return text(latin1_to_utf8(bytes,length)); }
  static Value latin1(const char *bytes) { return text(latin1_to_utf8(bytes)); }
  static Value null() { return Value(); }
  static Value object(std::vector<std::pair<std::string,Value>> fields) {
    Value v; v.kind=ValueKind::Object; std::map<std::string,size_t> positions;
    for(auto &field:fields) {
      auto found=positions.find(field.first);
      if(found==positions.end()) { positions[field.first]=v.members.size(); v.members.push_back(std::move(field)); }
      else v.members[found->second].second=std::move(field.second);
    }
    return v;
  }
  static Value array(std::vector<Value> items) { Value v; v.kind=ValueKind::Array; v.elements=std::move(items); return v; }
  StoredKind stored_kind() const {
    switch(kind) {
    case ValueKind::SignedInteger: case ValueKind::UnsignedInteger: return StoredKind::Integer;
    case ValueKind::Boolean: return StoredKind::Boolean;
    case ValueKind::Text: return StoredKind::Text;
    case ValueKind::Object: case ValueKind::Array: return StoredKind::Json;
    case ValueKind::Null: return StoredKind::Null;
    }
    throw std::runtime_error("Unknown typed semantic value");
  }
};

struct Properties {
  std::vector<std::pair<std::string,Value>> fields;
  std::map<std::string,size_t> positions;
  void set(const std::string &name,Value value) {
    auto found=positions.find(name);
    if(found==positions.end()) { positions[name]=fields.size(); fields.push_back({name,std::move(value)}); }
    else fields[found->second].second=std::move(value);
  }
};
struct NativeEdge { std::uint64_t target; std::string kind; };
struct NodeRecord:Properties {
  std::uint64_t id;
  std::string kind;
  bool has_native_edges=false;
  std::vector<NativeEdge> edges;
  NodeRecord(std::uint64_t identity,std::string node_kind):id(identity),kind(std::move(node_kind)) {}
  void begin_edges() { has_native_edges=true; edges.clear(); }
  void edge(std::uint64_t target,std::string target_kind) { has_native_edges=true; edges.push_back({target,std::move(target_kind)}); }
};
struct SourceLocation {
  std::uint64_t sequence,column,line,source;
  std::string presumed_file,physical_file;
};
struct SourcePosition { SourceLocation expansion,spelling; std::uint64_t macro_context; };
struct SourceRange { bool available; SourcePosition start,end; };
struct MacroRecord {
  std::int64_t invocation_id,parent_index;
  std::uint64_t definition_id;
  SourceLocation start,end;
  std::string arguments;
};
inline Value location_value(const SourceLocation &p) {
  return Value::object({{"sequence",Value::unsigned_integer(p.sequence)}, {"column",Value::unsigned_integer(p.column)},
    {"line",Value::unsigned_integer(p.line)}, {"source",Value::unsigned_integer(p.source)},
    {"presumedFile",Value::text(p.presumed_file)}, {"physicalFile",Value::text(p.physical_file)}});
}
inline Value position_value(const SourcePosition &p) {
  return Value::object({{"expansion",location_value(p.expansion)}, {"spelling",location_value(p.spelling)},
    {"macroContext",Value::unsigned_integer(p.macro_context)}});
}
inline Value range_value(const SourceRange &p) {
  return Value::object({{"available",Value::boolean(p.available)}, {"start",position_value(p.start)}, {"end",position_value(p.end)}});
}

inline void json_hex_escape(std::string &out,unsigned code) {
  static const char hex[]="0123456789abcdef";
  out+="\\u";
  for(int shift=12;shift>=0;shift-=4) out.push_back(hex[(code>>shift)&15]);
}
inline void json_string(std::string &out,const std::string &utf8) {
  out.push_back('"');
  for(size_t n=0;n<utf8.size();) {
    unsigned char c=static_cast<unsigned char>(utf8[n++]);
    if(c<128) {
      if(c=='"'||c=='\\') { out.push_back('\\'); out.push_back(static_cast<char>(c)); }
      else if(c<32 || c==127) json_hex_escape(out,c);
      else out.push_back(static_cast<char>(c));
      continue;
    }
    unsigned code,continuations,minimum;
    if(c>=0xc2 && c<=0xdf) { code=c&31; continuations=1; minimum=0x80; }
    else if(c>=0xe0 && c<=0xef) { code=c&15; continuations=2; minimum=0x800; }
    else if(c>=0xf0 && c<=0xf4) { code=c&7; continuations=3; minimum=0x10000; }
    else throw std::runtime_error("Invalid UTF-8 semantic text");
    for(unsigned k=0;k<continuations;++k) {
      if(n==utf8.size()) throw std::runtime_error("Truncated UTF-8 semantic text");
      unsigned char next=static_cast<unsigned char>(utf8[n++]);
      if((next&0xc0)!=0x80) throw std::runtime_error("Invalid UTF-8 semantic continuation");
      code=(code<<6)|(next&63);
    }
    if(code<minimum || code>0x10ffff || (code>=0xd800 && code<=0xdfff)) throw std::runtime_error("Invalid Unicode semantic value");
    if(code<0x10000) json_hex_escape(out,code);
    else { code-=0x10000; json_hex_escape(out,0xd800+(code>>10)); json_hex_escape(out,0xdc00+(code&1023)); }
  }
  out.push_back('"');
}
inline void append_json(std::string &out,const Value &value) {
  switch(value.kind) {
  case ValueKind::SignedInteger: out+=std::to_string(value.signed_value); break;
  case ValueKind::UnsignedInteger: out+=std::to_string(value.unsigned_value); break;
  case ValueKind::Boolean: out+=value.unsigned_value?"true":"false"; break;
  case ValueKind::Text: json_string(out,value.text_value); break;
  case ValueKind::Null: out+="null"; break;
  case ValueKind::Object:
    out.push_back('{');
    for(size_t n=0;n<value.members.size();++n) { if(n) out.push_back(','); json_string(out,value.members[n].first); out.push_back(':'); append_json(out,value.members[n].second); }
    out.push_back('}'); break;
  case ValueKind::Array:
    out.push_back('[');
    for(size_t n=0;n<value.elements.size();++n) { if(n) out.push_back(','); append_json(out,value.elements[n]); }
    out.push_back(']'); break;
  }
}
inline std::string json_value(const Value &value) { std::string out; append_json(out,value); return out; }
inline const char *stored_kind_name(StoredKind kind) {
  switch(kind) {
  case StoredKind::Integer: return "integer"; case StoredKind::Boolean: return "boolean";
  case StoredKind::Text: return "text"; case StoredKind::Json: return "json"; case StoredKind::Null: return "null";
  }
  throw std::runtime_error("Unknown semantic storage kind");
}

// The query is already bound to the encoding/hash bucket. Exposed separately
// so regression tests can exercise deliberately colliding candidates without
// weakening the production hash or changing the database writer's API.
inline sqlite3_int64 find_exact_literal(sqlite3_stmt *query,const std::string &payload) {
  int rc;
  while((rc=sqlite3_step(query))==SQLITE_ROW) {
    if(static_cast<size_t>(sqlite3_column_bytes(query,1))==payload.size() &&
       (payload.empty() || std::memcmp(sqlite3_column_blob(query,1),payload.data(),payload.size())==0))
      return sqlite3_column_int64(query,0);
  }
  if(rc!=SQLITE_DONE) throw std::runtime_error(std::string("Looking up semantic literal failed: ")+sqlite3_errmsg(sqlite3_db_handle(query)));
  return 0;
}

class Database {
  struct Kind { sqlite3_int64 id; std::string table; std::map<std::string,StoredKind> fields; };
  sqlite3 *db=nullptr;
  std::map<std::string,sqlite3_stmt*> statements;
  std::map<std::string,Kind> kinds;
  std::map<std::string,sqlite3_int64> small_literals;
  sqlite3_int64 diagnostic_count=0;
  bool committed=false;
  static std::string quoted(const std::string &s) { std::string result="\""; for(char c:s) { result.push_back(c); if(c=='"') result.push_back(c); } return result+'"'; }
  [[noreturn]] void fail(const std::string &what) { throw std::runtime_error(what+": "+sqlite3_errmsg(db)); }
  void exec(const std::string &sql) { if(sqlite3_exec(db,sql.c_str(),nullptr,nullptr,nullptr)!=SQLITE_OK) fail("Semantic SQLite statement failed"); }
  sqlite3_stmt *prepare(const std::string &sql) {
    auto found=statements.find(sql); if(found!=statements.end()) return found->second;
    sqlite3_stmt *statement=nullptr; if(sqlite3_prepare_v2(db,sql.c_str(),-1,&statement,nullptr)!=SQLITE_OK) fail("Preparing semantic SQLite statement failed");
    statements[sql]=statement; return statement;
  }
  void text(sqlite3_stmt *s,int index,const std::string &value) { if(sqlite3_bind_text64(s,index,value.data(),static_cast<sqlite3_uint64>(value.size()),SQLITE_TRANSIENT,SQLITE_UTF8)!=SQLITE_OK) fail("Binding semantic text failed"); }
  void blob(sqlite3_stmt *s,int index,const std::string &value) { if(sqlite3_bind_blob64(s,index,value.data(),static_cast<sqlite3_uint64>(value.size()),SQLITE_TRANSIENT)!=SQLITE_OK) fail("Binding semantic bytes failed"); }
  void integer(sqlite3_stmt *s,int index,sqlite3_int64 value) { if(sqlite3_bind_int64(s,index,value)!=SQLITE_OK) fail("Binding semantic integer failed"); }
  void finish(sqlite3_stmt *s) { if(sqlite3_step(s)!=SQLITE_DONE) fail("Writing semantic SQLite record failed"); sqlite3_reset(s); sqlite3_clear_bindings(s); }
  sqlite3_int64 literal(const std::string &encoding,const std::string &payload) {
    bool cache=payload.size()<=256; std::string key;
    if(cache) { key=encoding+'\0'+payload; auto found=small_literals.find(key); if(found!=small_literals.end()) return found->second; }
    // FNV-1a is only a compact index hint, never content identity. Keeping the
    // full payload out of the index avoids SQLite overflow-page amplification.
    // Compare every candidate's exact bytes so hash collisions remain distinct.
    std::uint64_t hash=UINT64_C(14695981039346656037);
    for(unsigned char c:payload) { hash^=c; hash*=UINT64_C(1099511628211); }
    sqlite3_int64 signed_hash=hash<=static_cast<std::uint64_t>(std::numeric_limits<sqlite3_int64>::max())
        ? static_cast<sqlite3_int64>(hash)
        : -1-static_cast<sqlite3_int64>(std::numeric_limits<std::uint64_t>::max()-hash);
    sqlite3_stmt *query=prepare("SELECT literal_id,payload FROM literals WHERE encoding=?1 AND payload_hash=?2"); text(query,1,encoding); integer(query,2,signed_hash);
    sqlite3_int64 result=find_exact_literal(query,payload);
    sqlite3_reset(query); sqlite3_clear_bindings(query);
    if(!result) { sqlite3_stmt *insert=prepare("INSERT INTO literals(encoding,payload_hash,payload) VALUES(?1,?2,?3)"); text(insert,1,encoding); integer(insert,2,signed_hash); blob(insert,3,payload); finish(insert); result=sqlite3_last_insert_rowid(db); }
    if(cache && small_literals.size()<65536) small_literals.emplace(std::move(key),result);
    return result;
  }
  Kind &kind(const std::string &name) {
    auto found=kinds.find(name); if(found!=kinds.end()) return found->second;
    std::string table="node_";
    for(unsigned char c:name) table.push_back((c>='a'&&c<='z')||(c>='A'&&c<='Z')||(c>='0'&&c<='9')||c=='_'?static_cast<char>(c):'_');
    Kind entry{static_cast<sqlite3_int64>(kinds.size()+1),table,{}};
    sqlite3_stmt *insert=prepare("INSERT INTO node_kinds(kind_id,name,table_name) VALUES(?1,?2,?3)"); integer(insert,1,entry.id); text(insert,2,name); text(insert,3,table); finish(insert);
    exec("CREATE TABLE "+quoted(table)+"(unit_id INTEGER NOT NULL,node_id INTEGER NOT NULL,PRIMARY KEY(unit_id,node_id)) WITHOUT ROWID");
    return kinds.emplace(name,std::move(entry)).first->second;
  }
  void field(Kind &owner,const std::string &name,StoredKind value_kind) {
    auto found=owner.fields.find(name);
    if(found!=owner.fields.end()) { if(found->second!=value_kind) throw std::runtime_error("Semantic field changes type: "+owner.table+"."+name); return; }
    exec("ALTER TABLE "+quoted(owner.table)+" ADD COLUMN "+quoted(name)+" INTEGER");
    sqlite3_stmt *insert=prepare("INSERT INTO node_fields(kind_id,ordinal,name,value_kind) VALUES(?1,?2,?3,?4)"); integer(insert,1,owner.id); integer(insert,2,static_cast<sqlite3_int64>(owner.fields.size())); text(insert,3,name); text(insert,4,stored_kind_name(value_kind)); finish(insert);
    owner.fields.emplace(name,value_kind);
  }
  void identity(sqlite3_stmt *s,int index,std::uint64_t value) {
    if(value>static_cast<std::uint64_t>(std::numeric_limits<sqlite3_int64>::max())) throw std::runtime_error("Semantic identity exceeds SQLite integer range");
    integer(s,index,static_cast<sqlite3_int64>(value));
  }
  void scalar(sqlite3_stmt *s,int index,const Value &value) {
    switch(value.kind) {
    case ValueKind::SignedInteger: integer(s,index,value.signed_value); break;
    case ValueKind::UnsignedInteger:
      if(value.unsigned_value>static_cast<std::uint64_t>(std::numeric_limits<sqlite3_int64>::max())) blob(s,index,std::to_string(value.unsigned_value));
      else integer(s,index,static_cast<sqlite3_int64>(value.unsigned_value));
      break;
    case ValueKind::Boolean: integer(s,index,value.unsigned_value?1:0); break;
    case ValueKind::Text: integer(s,index,literal("text",value.text_value)); break;
    case ValueKind::Object: case ValueKind::Array: integer(s,index,literal("json",json_value(value))); break;
    case ValueKind::Null: integer(s,index,0); break;
    }
  }

public:
  explicit Database(const char *path) {
    FILE *existing=std::fopen(path,"rb"); if(existing) { std::fclose(existing); throw std::runtime_error("Semantic SQLite output must be a fresh file"); }
    int rc=sqlite3_open_v2(path,&db,SQLITE_OPEN_READWRITE|SQLITE_OPEN_CREATE,nullptr);
    if(rc!=SQLITE_OK) { std::string message=db?sqlite3_errmsg(db):"allocation failed"; if(db) sqlite3_close(db); db=nullptr; throw std::runtime_error("Opening semantic SQLite output: "+message); }
    try { exec("PRAGMA journal_mode=DELETE; PRAGMA synchronous=NORMAL; PRAGMA cache_size=-65536; PRAGMA temp_store=MEMORY; BEGIN IMMEDIATE"); exec(semantic_schema_sql); exec("INSERT INTO units(unit_id) VALUES(1)"); }
    catch(...) { sqlite3_close(db); db=nullptr; throw; }
  }
  ~Database() {
    for(const auto &entry:statements) sqlite3_finalize(entry.second);
    if(db) { if(!committed) sqlite3_exec(db,"ROLLBACK",nullptr,nullptr,nullptr); sqlite3_close(db); }
  }
  void metadata(const Properties &record) {
    sqlite3_stmt *insert=prepare("INSERT INTO graph_metadata(unit_id,key,value_json) VALUES(1,?1,?2)");
    for(const auto &entry:record.fields) { text(insert,1,entry.first); text(insert,2,json_value(entry.second)); finish(insert); }
  }
  void node(const NodeRecord &record) {
    Kind &owner=kind(record.kind);
    const Value *form=nullptr;
    std::vector<const std::pair<std::string,Value>*> fields;
    std::string sql="INSERT INTO "+quoted(owner.table)+"(unit_id,node_id";
    for(const auto &entry:record.fields) {
      if(entry.first=="form") { form=&entry.second; continue; }
      if(entry.first=="id" || entry.first=="kind" || entry.first=="nativeEdges") throw std::runtime_error("Reserved semantic field must use its typed record member");
      field(owner,entry.first,entry.second.stored_kind()); fields.push_back(&entry); sql+=","+quoted(entry.first);
    }
    sql+=") VALUES(1,?1"; for(size_t n=0;n<fields.size();++n) sql+=",?"+std::to_string(n+2); sql+=")";
    sqlite3_stmt *typed=prepare(sql); identity(typed,1,record.id);
    for(size_t n=0;n<fields.size();++n) scalar(typed,static_cast<int>(n+2),fields[n]->second);
    finish(typed);
    sqlite3_stmt *base=prepare("INSERT INTO nodes(unit_id,node_id,kind_id,form_literal_id,edge_count) VALUES(1,?1,?2,?3,?4)"); identity(base,1,record.id); integer(base,2,owner.id);
    if(form) { if(form->kind!=ValueKind::Text) throw std::runtime_error("Semantic form is not text"); integer(base,3,literal("text",form->text_value)); }
    if(record.has_native_edges) integer(base,4,static_cast<sqlite3_int64>(record.edges.size()));
    else if(!record.edges.empty()) throw std::runtime_error("Absent semantic edge list has entries");
    finish(base);
    sqlite3_stmt *edge_insert=prepare("INSERT INTO native_edges(unit_id,source_id,ordinal,target_id,target_kind_id) VALUES(1,?1,?2,?3,?4)");
    for(size_t n=0;n<record.edges.size();++n) {
      const auto &edge=record.edges[n]; auto &target_kind=kind(edge.kind);
      identity(edge_insert,1,record.id); integer(edge_insert,2,static_cast<sqlite3_int64>(n)); identity(edge_insert,3,edge.target); integer(edge_insert,4,target_kind.id); finish(edge_insert);
    }
  }
  void macro(const MacroRecord &record) {
    sqlite3_stmt *insert=prepare("INSERT INTO macros(unit_id,invocation_id,parent_index,definition_id,start_literal_id,end_literal_id,arguments_literal_id) VALUES(1,?1,?2,?3,?4,?5,?6)");
    integer(insert,1,record.invocation_id); integer(insert,2,record.parent_index); identity(insert,3,record.definition_id);
    integer(insert,4,literal("json",json_value(location_value(record.start)))); integer(insert,5,literal("json",json_value(location_value(record.end)))); integer(insert,6,literal("text",record.arguments)); finish(insert);
  }
  void diagnostic(unsigned node_id,const std::string &message) {
    sqlite3_stmt *insert=prepare("INSERT INTO diagnostics(unit_id,ordinal,node_id,code_literal_id,message_literal_id) VALUES(1,?1,?2,?3,?4)");
    integer(insert,1,diagnostic_count++); integer(insert,2,node_id); integer(insert,3,literal("text","SEM001")); integer(insert,4,literal("text",message)); finish(insert);
  }
  void commit() { exec("COMMIT"); committed=true; }
};
} // namespace semantic_storage
#endif
