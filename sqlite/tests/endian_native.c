/* Test-only native oracle. No dotcc overrides, translated code or HostVfs. */
#include "sqlite3.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

static const uint16_t text[] = {0x41,0xe9,0x3bb,0x6c34,0xd83d,0xde00,0};
static const unsigned char little[] = {0x41,0,0xe9,0,0xbb,3,0x34,0x6c,0x3d,0xd8,0,0xde};
static const unsigned char big[] = {0,0x41,0,0xe9,3,0xbb,0x6c,0x34,0xd8,0x3d,0xde,0};
static const char utf8[] = "Aéλ水😀";
static void require(int ok, const char *what) { if(!ok) { fprintf(stderr,"FAIL %s\n",what); exit(1); } }
static void check(int rc) { require(rc==SQLITE_OK,sqlite3_errstr(rc)); }
static void exec(sqlite3 *db, const char *sql) { check(sqlite3_exec(db,sql,0,0,0)); }
static void expect(sqlite3 *db, const char *sql, const char *value) {
  sqlite3_stmt *s=0; check(sqlite3_prepare_v2(db,sql,-1,&s,0));
  require(sqlite3_step(s)==SQLITE_ROW,"expected row");
  require(strcmp((const char*)sqlite3_column_text(s,0),value)==0,sql);
  check(sqlite3_finalize(s));
}
static void utf16_sql(sqlite3 *db, const char *sql, sqlite3_stmt **s) {
  uint16_t buffer[256]; size_t i; for(i=0;sql[i];i++) buffer[i]=(unsigned char)sql[i]; buffer[i]=0;
  check(sqlite3_prepare16_v2(db,buffer,-1,s,0));
}
int main(int argc, char **argv) {
  require(argc==3 && (!strcmp(argv[1],"write") || !strcmp(argv[1],"read")),"usage: endian-native write|read DIRECTORY");
  for(int order=0;order<2;order++) {
    const char *encoding=order ? "UTF-16be" : "UTF-16le";
    char path[4096],sql[256]; sqlite3 *db=0; sqlite3_stmt *s=0;
    snprintf(path,sizeof(path),"%s/%s.db",argv[2],encoding);
    if(!strcmp(argv[1],"write")) {
      remove(path); check(sqlite3_open(path,&db));
      snprintf(sql,sizeof(sql),"PRAGMA encoding='%s'",encoding); exec(db,sql);
      expect(db,"PRAGMA journal_mode=WAL","wal");
      exec(db,"CREATE TABLE endian_text(id INTEGER PRIMARY KEY,value TEXT)");
      utf16_sql(db,"INSERT INTO endian_text VALUES(?1,?2)",&s);
      for(int mode=0;mode<3;mode++) {
        check(sqlite3_bind_int(s,1,mode));
        if(!mode) check(sqlite3_bind_text16(s,2,text,12,SQLITE_TRANSIENT));
        else check(sqlite3_bind_text64(s,2,(const char*)(mode==1 ? little : big),12,SQLITE_TRANSIENT,mode==1 ? SQLITE_UTF16LE : SQLITE_UTF16BE));
        require(sqlite3_step(s)==SQLITE_DONE,"insert"); check(sqlite3_reset(s));
      }
      check(sqlite3_finalize(s));
      check(sqlite3_wal_checkpoint_v2(db,0,SQLITE_CHECKPOINT_TRUNCATE,0,0));
      check(sqlite3_close(db)); db=0;
    }
    check(sqlite3_open(path,&db));
    expect(db,"PRAGMA encoding",encoding); expect(db,"PRAGMA integrity_check","ok");
    expect(db,"SELECT count(*) FROM endian_text WHERE value='Aéλ水😀' AND length(value)=5","3");
    utf16_sql(db,"SELECT value FROM endian_text ORDER BY id",&s);
    for(int row=0;row<3;row++) {
      require(sqlite3_step(s)==SQLITE_ROW,"missing row");
      require(!strcmp((const char*)sqlite3_column_text(s,0),utf8),"UTF8 conversion");
      const void *p=sqlite3_column_text16(s,0);
      require(sqlite3_column_bytes16(s,0)==12 && !memcmp(p,text,12),"native UTF16");
      sqlite3_value *v=sqlite3_column_value(s,0);
      p=sqlite3_value_text16le(v); require(sqlite3_value_bytes16(v)==12 && !memcmp(p,little,12),"UTF16LE");
      p=sqlite3_value_text16be(v); require(sqlite3_value_bytes16(v)==12 && !memcmp(p,big,12),"UTF16BE");
    }
    require(sqlite3_step(s)==SQLITE_DONE,"extra row");
    check(sqlite3_finalize(s)); check(sqlite3_close(db));
    printf("PASS %s: native/LE/BE APIs, conversions, WAL checkpoint and reopen\n",encoding);
  }
  check(sqlite3_shutdown()); return 0;
}
