#ifndef DOT_GEN_SEMANTIC_SQLITE_H
#define DOT_GEN_SEMANTIC_SQLITE_H
// Storage adapter only: SQLite prepared statements over ordinary program rows.
#include <sqlite3.h>
#include <cstdint>
#include <initializer_list>
#include <map>
#include <cstdio>
#include <cstdlib>
#include <string>
#include <type_traits>
#include <utility>
#include "dot_gen_semantic_schema.h"
namespace dot_semantic_sqlite {
struct Value {
  enum Kind { Null, Integer, Text } kind=Null;
  sqlite3_int64 integer=0;
  std::string text;
  Value(std::nullptr_t):kind(Null) {}
  Value(const char *value):kind(value?Text:Null),text(value?value:"") {}
  Value(const std::string &value):kind(Text),text(value) {}
  template<class T, typename std::enable_if<std::is_integral<T>::value,int>::type=0>
  Value(T value):kind(Integer),integer(static_cast<sqlite3_int64>(value)) {}
};
class Database {
  sqlite3 *db=nullptr;
  std::map<std::string,sqlite3_stmt*> statements;
  bool finished=false;
  void check(int code) {
    if(code!=SQLITE_OK && code!=SQLITE_DONE) {
      std::fprintf(stderr,"semantic SQLite: %s\n",db?sqlite3_errmsg(db):"open failed");
      std::exit(1);
    }
  }
public:
  explicit Database(const std::string &path) {
    check(sqlite3_open_v2(path.c_str(),&db,SQLITE_OPEN_READWRITE|SQLITE_OPEN_CREATE,nullptr));
      exec("PRAGMA foreign_keys=ON; PRAGMA cache_size=-8192; PRAGMA temp_store=FILE;");
      exec("BEGIN IMMEDIATE; PRAGMA defer_foreign_keys=ON;");
    exec(dot_semantic_schema);
  }
  Database(const Database&)=delete;
  Database& operator=(const Database&)=delete;
  ~Database() {
    for(auto &entry:statements)sqlite3_finalize(entry.second);
    if(db) {
      if(!finished)sqlite3_exec(db,"ROLLBACK",nullptr,nullptr,nullptr);
      sqlite3_close_v2(db);
    }
  }
  void exec(const std::string &sql) { check(sqlite3_exec(db,sql.c_str(),nullptr,nullptr,nullptr)); }
  void insert(const std::string &table,std::initializer_list<std::pair<const char*,Value>> columns) {
    std::string sql="INSERT INTO "+table+" (",values;
    for(const auto &column:columns) {
      if(!values.empty()){sql+=',';values+=',';}
      sql+='"';sql+=column.first;sql+='"';values+='?';
    }
    sql+=") VALUES ("+values+")";
    auto &statement=statements[sql];
    if(!statement)check(sqlite3_prepare_v2(db,sql.c_str(),-1,&statement,nullptr));
    int ordinal=0;
    {
      for(const auto &column:columns) {
        ++ordinal;
        const auto &value=column.second;
        switch(value.kind) {
          case Value::Null: check(sqlite3_bind_null(statement,ordinal));break;
          case Value::Integer: check(sqlite3_bind_int64(statement,ordinal,value.integer));break;
          case Value::Text: check(sqlite3_bind_text(statement,ordinal,value.text.data(),int(value.text.size()),SQLITE_TRANSIENT));break;
        }
      }
      check(sqlite3_step(statement));
    }
    check(sqlite3_reset(statement));check(sqlite3_clear_bindings(statement));
  }
  void update(const std::string &table,const std::string &key,Value id,
              std::initializer_list<std::pair<const char*,Value>> columns) {
    std::string sql="UPDATE "+table+" SET ";
    bool first=true;
    for(const auto &column:columns) {
      if(!first)sql+=',';
      first=false;
      sql+='"';sql+=column.first;sql+="\"=?";
    }
    sql+=" WHERE "+key+"=?";
    auto &statement=statements[sql];
    if(!statement)check(sqlite3_prepare_v2(db,sql.c_str(),-1,&statement,nullptr));
    int ordinal=0;
    auto bind=[&](const Value &value) {
      ++ordinal;
      switch(value.kind) {
        case Value::Null: check(sqlite3_bind_null(statement,ordinal));break;
        case Value::Integer: check(sqlite3_bind_int64(statement,ordinal,value.integer));break;
        case Value::Text: check(sqlite3_bind_text(statement,ordinal,value.text.data(),int(value.text.size()),SQLITE_TRANSIENT));break;
      }
    };
    {
      for(const auto &column:columns)bind(column.second);
      bind(id);check(sqlite3_step(statement));
    }
    check(sqlite3_reset(statement));check(sqlite3_clear_bindings(statement));
  }
  void finish() { exec("COMMIT");finished=true; }
};
}
#endif
