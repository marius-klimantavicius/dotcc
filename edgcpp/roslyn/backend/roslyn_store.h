/* Backend-owned SQLite writer: prepared inserts, interned text, invocation-local identities. */
#include <map>
#include <vector>
#include <string>
#include <initializer_list>
#include <cstring>
#include <utility>
#include <unistd.h>
#include "roslyn_schema.h"
#include "roslyn_kinds.h"
struct RoslynSqlValue {
  int tag; sqlite3_int64 number; std::string text;
  RoslynSqlValue():tag(0),number(0) {}
  RoslynSqlValue(int n):tag(1),number(n) {}
  RoslynSqlValue(sqlite3_int64 n):tag(1),number(n) {}
  RoslynSqlValue(const std::string &s):tag(2),number(0),text(s) {}
  RoslynSqlValue(const char *s):tag(s?2:0),number(0),text(s?s:"") {}
};
static RoslynSqlValue roslyn_ref(sqlite3_int64 n) { return n?RoslynSqlValue(n):RoslynSqlValue(); }
static int roslyn_tag(const char *family,const char *name) {
  size_t first=0,last=sizeof(roslyn_catalog)/sizeof(roslyn_catalog[0]);
  while(first<last){size_t middle=first+(last-first)/2;const auto &k=roslyn_catalog[middle];int comparison=std::strcmp(k.family,family);if(!comparison)comparison=std::strcmp(k.name,name);if(comparison<0)first=middle+1;else last=middle;}
  if(first==sizeof(roslyn_catalog)/sizeof(roslyn_catalog[0]))return 0;
  const auto &k=roslyn_catalog[first];return std::strcmp(k.family,family)==0&&std::strcmp(k.name,name)==0?k.id:0;
}
struct RoslynStore {
  sqlite3 *connection=nullptr; bool failed=false; std::string failure;
  sqlite3_int64 sequence=0, atom_sequence=0;
  std::map<std::pair<int,const void*>,sqlite3_int64> identities;
  std::map<std::string,sqlite3_int64> atoms;
  std::map<std::string,sqlite3_stmt*> statements;
  std::map<sqlite3_int64,int> semantic_roles;
  std::map<sqlite3_int64,std::vector<sqlite3_int64>> evaluated_children;
  std::set<sqlite3_int64> persisted_kinds;
  std::map<std::string,std::set<std::string>> kind_columns;
  ~RoslynStore() { close(); }
  void fail(const std::string &s) { if(!failed) failure=s; failed=true; }
  void close() {
    for(auto &p:statements)if(sqlite3_finalize(p.second)!=SQLITE_OK)fail("SQLite finalize failed");
    statements.clear(); if(connection){if(sqlite3_close(connection)!=SQLITE_OK)fail("SQLite close failed");else connection=nullptr;}
  }
  void exec(const char *s) {
    if(failed)return; char *message=nullptr;
    if(sqlite3_exec(connection,s,nullptr,nullptr,&message)!=SQLITE_OK)fail(message?message:sqlite3_errmsg(connection));
    sqlite3_free(message);
  }
  void check_foreign_keys() {
    if(failed)return;sqlite3_stmt *query=nullptr;
    if(sqlite3_prepare_v2(connection,"PRAGMA foreign_key_check",-1,&query,nullptr)!=SQLITE_OK){fail(sqlite3_errmsg(connection));return;}
    int rc=sqlite3_step(query);if(rc==SQLITE_ROW){const char *table=reinterpret_cast<const char*>(sqlite3_column_text(query,0));const char *parent=reinterpret_cast<const char*>(sqlite3_column_text(query,2));fail(std::string("Foreign key violation: ")+(table?table:"")+" row "+std::to_string(sqlite3_column_int64(query,1))+" -> "+(parent?parent:"")+" (constraint "+std::to_string(sqlite3_column_int(query,3))+")");}else if(rc!=SQLITE_DONE)fail(sqlite3_errmsg(connection));
    if(sqlite3_finalize(query)!=SQLITE_OK)fail("Foreign key check finalize failed");
  }
  void ensure_kind(sqlite3_int64 id) {
    if(!id||persisted_kinds.count(id))return;
    for(const auto &entry:roslyn_catalog)if(entry.id==id){
      persisted_kinds.insert(id);sqlite3_stmt *insert=nullptr;
      if(sqlite3_prepare_v2(connection,"INSERT OR IGNORE INTO kind(id,family,name) VALUES(?,?,?)",-1,&insert,nullptr)!=SQLITE_OK){fail(sqlite3_errmsg(connection));return;}
      int rc=sqlite3_bind_int64(insert,1,id);if(rc==SQLITE_OK)rc=sqlite3_bind_text(insert,2,entry.family,-1,SQLITE_STATIC);if(rc==SQLITE_OK)rc=sqlite3_bind_text(insert,3,entry.name,-1,SQLITE_STATIC);if(rc==SQLITE_OK)rc=sqlite3_step(insert);if(rc!=SQLITE_DONE)fail(sqlite3_errmsg(connection));if(sqlite3_finalize(insert)!=SQLITE_OK)fail("kind insert finalize failed");
      if(sqlite3_prepare_v2(connection,"INSERT OR IGNORE INTO kind_disposition(kind_id,state) VALUES(?,'semantic')",-1,&insert,nullptr)!=SQLITE_OK){fail(sqlite3_errmsg(connection));return;}
      rc=sqlite3_bind_int64(insert,1,id);if(rc==SQLITE_OK)rc=sqlite3_step(insert);if(rc!=SQLITE_DONE)fail(sqlite3_errmsg(connection));if(sqlite3_finalize(insert)!=SQLITE_OK)fail("kind disposition finalize failed");return;
    }
    fail("Unknown shared kind ID");
  }
  const std::set<std::string> &kind_foreign_keys(const char *table) {
    auto found=kind_columns.find(table);if(found!=kind_columns.end())return found->second;
    std::set<std::string> columns;std::string sql="PRAGMA foreign_key_list("+std::string(table)+")";sqlite3_stmt *query=nullptr;
    if(sqlite3_prepare_v2(connection,sql.c_str(),-1,&query,nullptr)!=SQLITE_OK)fail(sqlite3_errmsg(connection));
    else {int rc;while((rc=sqlite3_step(query))==SQLITE_ROW){const char *target=reinterpret_cast<const char*>(sqlite3_column_text(query,2));const char *column=reinterpret_cast<const char*>(sqlite3_column_text(query,3));if(target&&column&&std::strcmp(target,"kind")==0)columns.insert(column);}if(rc!=SQLITE_DONE)fail(sqlite3_errmsg(connection));if(sqlite3_finalize(query)!=SQLITE_OK)fail("FK introspection finalize failed");}
    return kind_columns.emplace(table,std::move(columns)).first->second;
  }
  template<class Fields> void put_fields(const char *table,const Fields &fields) {
    if(failed)return;
    const auto &kind_fields=kind_foreign_keys(table);for(const auto &field:fields)if(field.second.tag==1&&kind_fields.count(field.first))ensure_kind(field.second.number);if(failed)return;
    std::string sql="INSERT INTO ";sql+=table;sql+='(';bool first=true;
    for(const auto &f:fields){if(!first)sql+=',';first=false;sql+=f.first;}
    sql+=") VALUES (";first=true;for(const auto &f:fields){(void)f;if(!first)sql+=',';first=false;sql+='?';}sql+=')';
    sqlite3_stmt *stmt=nullptr;auto found=statements.find(sql);
    if(found!=statements.end())stmt=found->second;
    else {if(sqlite3_prepare_v2(connection,sql.c_str(),-1,&stmt,nullptr)!=SQLITE_OK){fail(sql+": "+sqlite3_errmsg(connection));return;}statements.emplace(sql,stmt);}
    if(sqlite3_reset(stmt)!=SQLITE_OK||sqlite3_clear_bindings(stmt)!=SQLITE_OK){fail("SQLite statement reset failed");return;}int i=1;
    for(const auto &f:fields){int rc;if(f.second.tag==0)rc=sqlite3_bind_null(stmt,i);else if(f.second.tag==1)rc=sqlite3_bind_int64(stmt,i,f.second.number);else rc=sqlite3_bind_text(stmt,i,f.second.text.data(),static_cast<int>(f.second.text.size()),SQLITE_TRANSIENT);if(rc!=SQLITE_OK){fail("SQLite bind failed");return;}++i;}
    if(sqlite3_step(stmt)!=SQLITE_DONE)fail(sql+": "+sqlite3_errmsg(connection));
  }
  void put(const char *table,std::initializer_list<std::pair<const char*,RoslynSqlValue>> fields) { put_fields(table,fields); }
  int semantic_role(sqlite3_int64 id,int role) {
    auto found=semantic_roles.find(id);if(found!=semantic_roles.end()&&(found->second==7001||found->second==7003))return found->second;
    semantic_roles[id]=role;return role;
  }
  void evaluated_child(sqlite3_int64 owner,sqlite3_int64 child) {
    evaluated_children[owner].push_back(child);auto found=semantic_roles.find(owner);
    if(found!=semantic_roles.end())promote_execution(child,found->second);
  }
  void promote_execution(sqlite3_int64 id,int role) {
    if(!id||(role!=7001&&role!=7003)||failed)return;
    auto known=semantic_roles.find(id);if(known!=semantic_roles.end()&&known->second==role)return;
    semantic_roles[id]=role;
    const std::string sql="UPDATE semantic_node SET representation_role_id=? WHERE id=? AND representation_role_id=7002";
    sqlite3_stmt *stmt=nullptr;auto found=statements.find(sql);
    if(found!=statements.end())stmt=found->second;
    else {if(sqlite3_prepare_v2(connection,sql.c_str(),-1,&stmt,nullptr)!=SQLITE_OK){fail(sqlite3_errmsg(connection));return;}statements.emplace(sql,stmt);}
    if(sqlite3_reset(stmt)!=SQLITE_OK||sqlite3_clear_bindings(stmt)!=SQLITE_OK||sqlite3_bind_int(stmt,1,role)!=SQLITE_OK||sqlite3_bind_int64(stmt,2,id)!=SQLITE_OK||sqlite3_step(stmt)!=SQLITE_DONE)fail(sqlite3_errmsg(connection));
    auto children=evaluated_children.find(id);if(children!=evaluated_children.end())for(auto child:children->second)promote_execution(child,role);
  }
  sqlite3_int64 atom(const char *s) { return s?atom(std::string(s)):0; }
  sqlite3_int64 atom(const std::string &s) {
    auto p=atoms.find(s);if(p!=atoms.end())return p->second;
    sqlite3_int64 n=++atom_sequence;atoms.emplace(s,n);put("text_atom",{{"id",n},{"value",s}});return n;
  }
  sqlite3_int64 fresh(const char *family) {sqlite3_int64 n=++sequence;put("entity",{{"id",n},{"kind_id",roslyn_tag("entity",family)}});return n;}
  sqlite3_int64 claim(const void *p,const char *family,bool &is_new) {
    is_new=false;if(!p)return 0;auto key=std::make_pair(roslyn_tag("entity",family),p);
    auto f=identities.find(key);if(f!=identities.end())return f->second;
    is_new=true;sqlite3_int64 n=fresh(family);identities.emplace(key,n);return n;
  }
  sqlite3_int64 existing(const void *p,const char *family) {
    auto f=identities.find(std::make_pair(roslyn_tag("entity",family),p));return f==identities.end()?0:f->second;
  }
};
