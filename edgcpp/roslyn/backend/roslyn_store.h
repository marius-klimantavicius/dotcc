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
  for(const auto &k:roslyn_catalog) if(std::strcmp(k.family,family)==0&&std::strcmp(k.name,name)==0)return k.id;
  return 0;
}
struct RoslynStore {
  sqlite3 *connection=nullptr; bool failed=false; std::string failure;
  sqlite3_int64 sequence=0, atom_sequence=0;
  std::map<std::pair<int,const void*>,sqlite3_int64> identities;
  std::map<std::string,sqlite3_int64> atoms;
  std::map<std::string,sqlite3_stmt*> statements;
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
  void put(const char *table,std::initializer_list<std::pair<const char*,RoslynSqlValue>> fields) {
    if(failed)return;
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
