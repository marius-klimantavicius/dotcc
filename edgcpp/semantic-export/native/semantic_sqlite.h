// Normalized, transactional storage for one unlowered EDG translation unit.
// Serialization is bounded to one node or macro record. No graph JSON is stored.
#ifndef EDG_SEMANTIC_SQLITE_H
#define EDG_SEMANTIC_SQLITE_H
#include <sqlite3.h>
#include <cstdarg>
#include <cstdio>
#include <cstdlib>
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
class JsonBuffer {
public:
  std::string value;
  void clear() { value.clear(); }
  void character(int c) { value.push_back(static_cast<char>(c)); }
  void text(const char *s) { value.append(s); }
  void format(const char *fmt, ...) {
    char small[1024]; va_list args; va_start(args, fmt);
    va_list copy; va_copy(copy, args);
    int length = std::vsnprintf(small, sizeof(small), fmt, args); va_end(args);
    if (length < 0) { va_end(copy); throw std::runtime_error("Semantic JSON formatting failed"); }
    if (static_cast<size_t>(length) < sizeof(small)) value.append(small, static_cast<size_t>(length));
    else {
      std::vector<char> large(static_cast<size_t>(length) + 1);
      std::vsnprintf(large.data(), large.size(), fmt, copy);
      value.append(large.data(), static_cast<size_t>(length));
    }
    va_end(copy);
  }
};

// These tokens preserve the exact integer spelling. In particular, parsing a
// UINT64 value through SQLite JSON1's numeric atom would round it to double.
struct JsonValue {
  std::string kind;
  std::string value;
};
class JsonParser {
  const std::string &input;
  size_t cursor = 0;
  void space() { while (cursor < input.size() && (input[cursor]==' ' || input[cursor]=='\n' || input[cursor]=='\r' || input[cursor]=='\t')) ++cursor; }
  void expect(char c) { space(); if (cursor >= input.size() || input[cursor++] != c) throw std::runtime_error("Malformed semantic record"); }
  unsigned hex4() {
    unsigned result = 0;
    for (int n=0;n<4;++n) {
      if (cursor == input.size()) throw std::runtime_error("Truncated semantic string escape");
      char c=input[cursor++]; result <<= 4;
      if (c>='0'&&c<='9') result += static_cast<unsigned>(c-'0');
      else if (c>='a'&&c<='f') result += static_cast<unsigned>(c-'a'+10);
      else if (c>='A'&&c<='F') result += static_cast<unsigned>(c-'A'+10);
      else throw std::runtime_error("Invalid semantic string escape");
    }
    return result;
  }
  static void utf8(std::string &out, unsigned code) {
    if (code < 0x80) out.push_back(static_cast<char>(code));
    else if (code < 0x800) { out.push_back(static_cast<char>(0xc0|(code>>6))); out.push_back(static_cast<char>(0x80|(code&63))); }
    else if (code < 0x10000) { out.push_back(static_cast<char>(0xe0|(code>>12))); out.push_back(static_cast<char>(0x80|((code>>6)&63))); out.push_back(static_cast<char>(0x80|(code&63))); }
    else { out.push_back(static_cast<char>(0xf0|(code>>18))); out.push_back(static_cast<char>(0x80|((code>>12)&63))); out.push_back(static_cast<char>(0x80|((code>>6)&63))); out.push_back(static_cast<char>(0x80|(code&63))); }
  }
  std::string string() {
    expect('"'); std::string result;
    while (cursor < input.size()) {
      char c=input[cursor++]; if (c=='"') return result;
      if (c!='\\') { result.push_back(c); continue; }
      if (cursor == input.size()) throw std::runtime_error("Truncated semantic string");
      c=input[cursor++];
      switch(c) {
      case '"': case '\\': case '/': result.push_back(c); break;
      case 'b': result.push_back('\b'); break; case 'f': result.push_back('\f'); break;
      case 'n': result.push_back('\n'); break; case 'r': result.push_back('\r'); break; case 't': result.push_back('\t'); break;
      case 'u': { unsigned code=hex4();
        if (code>=0xd800 && code<=0xdbff) { expect('\\'); expect('u'); unsigned low=hex4(); if(low<0xdc00 || low>0xdfff) throw std::runtime_error("Invalid semantic surrogate"); code=0x10000+((code-0xd800)<<10)+(low-0xdc00); }
        else if (code>=0xdc00 && code<=0xdfff) throw std::runtime_error("Unpaired semantic surrogate");
        utf8(result,code); break; }
      default: throw std::runtime_error("Unknown semantic string escape");
      }
    }
    throw std::runtime_error("Unterminated semantic string");
  }
  void compound() {
    char first=input[cursor++]; char close=first=='{'?'}':']';
    while (cursor < input.size()) {
      char c=input[cursor];
      if (c=='"') { string(); continue; }
      if (c=='{' || c=='[') { compound(); continue; }
      ++cursor; if(c==close) return;
    }
    throw std::runtime_error("Unterminated semantic JSON object/array");
  }
public:
  explicit JsonParser(const std::string &text):input(text) {}
  JsonValue value() {
    space(); if (cursor == input.size()) throw std::runtime_error("Missing semantic value");
    size_t start=cursor; char c=input[cursor];
    if(c=='"') return {"text",string()};
    if(c=='{' || c=='[') { compound(); return {"json",input.substr(start,cursor-start)}; }
    while(cursor<input.size() && input[cursor]!=',' && input[cursor]!=']' && input[cursor]!='}' && input[cursor]!=' ' && input[cursor]!='\n' && input[cursor]!='\r' && input[cursor]!='\t') ++cursor;
    std::string token=input.substr(start,cursor-start);
    if(token=="true" || token=="false") return {"boolean",token};
    if(token=="null") return {"null",token};
    if(token.empty()) throw std::runtime_error("Empty semantic integer");
    size_t digit=token[0]=='-'?1:0;
    if(digit==token.size()) throw std::runtime_error("Invalid semantic integer");
    for(;digit<token.size();++digit) if(token[digit]<'0'||token[digit]>'9') throw std::runtime_error("Noninteger semantic numeric value");
    return {"integer",token};
  }
  std::vector<std::pair<std::string,JsonValue>> object() {
    expect('{'); std::vector<std::pair<std::string,JsonValue>> result;
    std::map<std::string,size_t> seen; space();
    if(cursor<input.size() && input[cursor]=='}') { ++cursor; return result; }
    while(true) {
      std::string name=string(); expect(':'); JsonValue item=value();
      auto found=seen.find(name);
      if(found==seen.end()) { seen[name]=result.size(); result.push_back({std::move(name),std::move(item)}); }
      else result[found->second].second=std::move(item);
      space(); if(cursor<input.size() && input[cursor]=='}') { ++cursor; return result; } expect(',');
    }
  }
  std::vector<JsonValue> array() {
    expect('['); std::vector<JsonValue> result; space();
    if(cursor<input.size() && input[cursor]==']') { ++cursor; return result; }
    while(true) { result.push_back(value()); space(); if(cursor<input.size() && input[cursor]==']') { ++cursor; return result; } expect(','); }
  }
};

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
  struct Kind { sqlite3_int64 id; std::string table; std::map<std::string,std::string> fields; };
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
  void field(Kind &owner,const std::string &name,const std::string &value_kind) {
    auto found=owner.fields.find(name);
    if(found!=owner.fields.end()) { if(found->second!=value_kind) throw std::runtime_error("Semantic field changes type: "+owner.table+"."+name); return; }
    exec("ALTER TABLE "+quoted(owner.table)+" ADD COLUMN "+quoted(name)+" INTEGER");
    sqlite3_stmt *insert=prepare("INSERT INTO node_fields(kind_id,ordinal,name,value_kind) VALUES(?1,?2,?3,?4)"); integer(insert,1,owner.id); integer(insert,2,static_cast<sqlite3_int64>(owner.fields.size())); text(insert,3,name); text(insert,4,value_kind); finish(insert);
    owner.fields.emplace(name,value_kind);
  }
  void scalar(sqlite3_stmt *s,int index,const JsonValue &value) {
    if(value.kind=="text" || value.kind=="json") integer(s,index,literal(value.kind,value.value));
    else if(value.kind=="boolean") integer(s,index,value.value=="true"?1:0);
    else if(value.kind=="null") integer(s,index,0);
    else if(value.kind=="integer") {
      try { size_t consumed=0; long long n=std::stoll(value.value,&consumed,10); if(consumed!=value.value.size()) throw std::runtime_error("Invalid integer"); integer(s,index,static_cast<sqlite3_int64>(n)); }
      catch(const std::out_of_range&) { blob(s,index,value.value); }
    } else throw std::runtime_error("Unknown semantic scalar category");
  }
  static const JsonValue &get(const std::vector<std::pair<std::string,JsonValue>> &record,const char *name) {
    for(const auto &member:record) if(member.first==name) return member.second;
    throw std::runtime_error(std::string("Semantic record missing ")+name);
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
  void metadata(const std::string &json) {
    auto record=JsonParser(json).object();
    sqlite3_stmt *insert=prepare("INSERT INTO graph_metadata(unit_id,key,value_json) VALUES(1,?1,?2)");
    for(const auto &entry:record) {
      // Preserve the original JSON value, including quoted scalar strings.
      std::string encoded;
      if(entry.second.kind=="text") {
        JsonBuffer buffer; buffer.character('"');
        for(unsigned char c:entry.second.value) { if(c=='"'||c=='\\') { buffer.character('\\'); buffer.character(c); } else if(c<32) buffer.format("\\u%04x",c); else buffer.character(c); }
        buffer.character('"'); encoded=std::move(buffer.value);
      } else encoded=entry.second.value;
      text(insert,1,entry.first); text(insert,2,encoded); finish(insert);
    }
  }
  void node(const std::string &json) {
    auto record=JsonParser(json).object(); sqlite3_int64 node_id=std::stoll(get(record,"id").value); Kind &owner=kind(get(record,"kind").value);
    const JsonValue *form=nullptr,*edges=nullptr;
    std::vector<const std::pair<std::string,JsonValue>*> fields;
    std::string sql="INSERT INTO "+quoted(owner.table)+"(unit_id,node_id";
    for(const auto &entry:record) {
      if(entry.first=="id" || entry.first=="kind") continue;
      if(entry.first=="form") { form=&entry.second; continue; }
      if(entry.first=="nativeEdges") { edges=&entry.second; continue; }
      field(owner,entry.first,entry.second.kind); fields.push_back(&entry); sql+=","+quoted(entry.first);
    }
    sql+=") VALUES(1,?1"; for(size_t n=0;n<fields.size();++n) sql+=",?"+std::to_string(n+2); sql+=")";
    sqlite3_stmt *typed=prepare(sql); integer(typed,1,node_id);
    for(size_t n=0;n<fields.size();++n) scalar(typed,static_cast<int>(n+2),fields[n]->second);
    finish(typed);
    std::vector<JsonValue> edge_records; if(edges) edge_records=JsonParser(edges->value).array();
    sqlite3_stmt *base=prepare("INSERT INTO nodes(unit_id,node_id,kind_id,form_literal_id,edge_count) VALUES(1,?1,?2,?3,?4)"); integer(base,1,node_id); integer(base,2,owner.id);
    if(form) { if(form->kind!="text") throw std::runtime_error("Semantic form is not text"); integer(base,3,literal("text",form->value)); }
    if(edges) integer(base,4,static_cast<sqlite3_int64>(edge_records.size()));
    finish(base);
    sqlite3_stmt *edge_insert=prepare("INSERT INTO native_edges(unit_id,source_id,ordinal,target_id,target_kind_id) VALUES(1,?1,?2,?3,?4)");
    for(size_t n=0;n<edge_records.size();++n) {
      auto edge=JsonParser(edge_records[n].value).object(); auto &target_kind=kind(get(edge,"kind").value);
      integer(edge_insert,1,node_id); integer(edge_insert,2,static_cast<sqlite3_int64>(n)); integer(edge_insert,3,std::stoll(get(edge,"target").value)); integer(edge_insert,4,target_kind.id); finish(edge_insert);
    }
  }
  void macro(const std::string &json) {
    auto record=JsonParser(json).object(); sqlite3_stmt *insert=prepare("INSERT INTO macros(unit_id,invocation_id,parent_index,definition_id,start_literal_id,end_literal_id,arguments_literal_id) VALUES(1,?1,?2,?3,?4,?5,?6)");
    integer(insert,1,std::stoll(get(record,"index").value)); integer(insert,2,std::stoll(get(record,"parentIndex").value)); integer(insert,3,std::stoll(get(record,"definition").value));
    integer(insert,4,literal("json",get(record,"start").value)); integer(insert,5,literal("json",get(record,"end").value)); integer(insert,6,literal("text",get(record,"arguments").value)); finish(insert);
  }
  void diagnostic(unsigned node_id,const std::string &message) {
    sqlite3_stmt *insert=prepare("INSERT INTO diagnostics(unit_id,ordinal,node_id,code_literal_id,message_literal_id) VALUES(1,?1,?2,?3,?4)");
    integer(insert,1,diagnostic_count++); integer(insert,2,node_id); integer(insert,3,literal("text","SEM001")); integer(insert,4,literal("text",message)); finish(insert);
  }
  void commit() { exec("COMMIT"); committed=true; }
};
} // namespace semantic_storage
#endif
