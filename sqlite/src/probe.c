/* Shared native/translated deterministic SQL probe. No platform I/O is used. */
#include "sqlite3.h"
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>

static void print_bytes(const unsigned char *p, int n) {
  int i;
  for (i = 0; i < n; ++i) printf("%02x", (unsigned int)p[i]);
}
static int run_sql(sqlite3 *db, const char *sql, int number) {
  sqlite3_stmt *stmt = 0;
  const char *tail = sql;
  int rc, i, columns;
  printf("case %d\n", number);
  while (*tail) {
    rc = sqlite3_prepare_v2(db, tail, -1, &stmt, &tail);
    if (rc != SQLITE_OK) {
      printf("prepare %d ", rc);
      print_bytes((const unsigned char *)sqlite3_errmsg(db), (int)strlen(sqlite3_errmsg(db)));
      printf("\n");
      return rc;
    }
    if (!stmt) continue;
    columns = sqlite3_column_count(stmt);
    while ((rc = sqlite3_step(stmt)) == SQLITE_ROW) {
      printf("row");
      for (i = 0; i < columns; ++i) {
        int type = sqlite3_column_type(stmt, i);
        printf(" %d:", type);
        if (type == SQLITE_INTEGER) printf("%lld", sqlite3_column_int64(stmt, i));
        else if (type == SQLITE_FLOAT) printf("%.17g", sqlite3_column_double(stmt, i));
        else if (type == SQLITE_TEXT || type == SQLITE_BLOB) {
          const unsigned char *value = type == SQLITE_TEXT
            ? sqlite3_column_text(stmt, i) : sqlite3_column_blob(stmt, i);
          int length = sqlite3_column_bytes(stmt, i);
          printf("%d:", length);
          print_bytes(value, length);
        }
      }
      printf("\n");
    }
    printf("step %d changes %d total %d autocommit %d\n", rc,
      sqlite3_changes(db), sqlite3_total_changes(db), sqlite3_get_autocommit(db));
    sqlite3_finalize(stmt);
    stmt = 0;
    if (rc != SQLITE_DONE) return rc;
  }
  return SQLITE_OK;
}
static const char *const corpus[] = {
  "SELECT sqlite_version(), 1, NULL, 1.25, x'0001ff', 'hello';",
  "SELECT sqlite_compileoption_used('THREADSAFE=0'), sqlite_compileoption_used('OMIT_LOAD_EXTENSION'), sqlite_compileoption_used('ENABLE_FTS3'), sqlite_compileoption_used('ENABLE_FTS4'), sqlite_compileoption_used('ENABLE_FTS5');",
  "PRAGMA foreign_keys=ON; CREATE TABLE parent(id INTEGER PRIMARY KEY, name TEXT UNIQUE); CREATE TABLE child(id INTEGER PRIMARY KEY, parent_id REFERENCES parent(id), amount INTEGER CHECK(amount>=0)); INSERT INTO parent VALUES(1,'one'),(2,'two'); INSERT INTO child VALUES(1,1,10),(2,1,20),(3,2,5);",
  "SELECT p.name, sum(c.amount), count(*) FROM parent p JOIN child c ON c.parent_id=p.id GROUP BY p.id ORDER BY p.id;",
  "CREATE INDEX child_amount ON child(amount); CREATE VIEW totals AS SELECT parent_id,sum(amount) AS total FROM child GROUP BY parent_id; SELECT * FROM totals ORDER BY parent_id;",
  "CREATE TABLE audit(value TEXT); CREATE TRIGGER audited AFTER INSERT ON parent BEGIN INSERT INTO audit VALUES(new.name); END; INSERT INTO parent VALUES(3,'three') RETURNING id,name; SELECT * FROM audit ORDER BY rowid;",
  "INSERT INTO parent VALUES(1,'ONE') ON CONFLICT(id) DO UPDATE SET name=excluded.name RETURNING id,name;",
  "WITH RECURSIVE seq(x) AS(VALUES(1) UNION ALL SELECT x+1 FROM seq WHERE x<8) SELECT x,x*x FROM seq ORDER BY x;",
  "SELECT id, row_number() OVER(ORDER BY amount,id), sum(amount) OVER(ORDER BY amount,id ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM child ORDER BY amount,id;",
  "SELECT p.id,c.id FROM parent p LEFT JOIN child c ON p.id=c.parent_id WHERE p.id IN(SELECT id FROM parent) ORDER BY p.id,c.id;",
  "SELECT id FROM parent UNION SELECT parent_id FROM child ORDER BY 1;",
  "BEGIN; UPDATE child SET amount=99 WHERE id=1; SAVEPOINT s; DELETE FROM child WHERE id=2; ROLLBACK TO s; RELEASE s; ROLLBACK; SELECT * FROM child ORDER BY id;",
  "BEGIN; UPDATE child SET amount=11 WHERE id=1; COMMIT; SELECT * FROM child ORDER BY id;",
  "INSERT INTO child VALUES(4,99,5);",
  "INSERT INTO child VALUES(4,1,-1);",
  "INSERT INTO parent VALUES(4,'ONE');",
  "SELECT date('2024-02-28','+1 day'),datetime(0,'unixepoch'),strftime('%Y-%m-%d','2000-01-02'),unixepoch('2000-01-01'),timediff('2024-02-01','2024-01-01');",
  "SELECT hex(char(0,65,0,66)),length(x'00410042'),quote(9223372036854775807),typeof(9223372036854775807+1),abs(-9223372036854775807);",
  "ATTACH 'attached' AS aux; CREATE TABLE aux.a(x); INSERT INTO aux.a VALUES(42); SELECT x FROM aux.a; DETACH aux;",
  "PRAGMA integrity_check; PRAGMA foreign_key_check; PRAGMA journal_mode=WAL; PRAGMA mmap_size=1000000;",
  "SELECT json('{\"b\":2,\"a\":[true,null,\"x\"]}'),json_array(1,'x',NULL,json('[2]')),json_object('k',1,'nested',json('[2]'));",
  "SELECT json_extract('{\"x\":[10,20]}','$.x[1]'),'{\"x\":[10,20]}'->'$.x','{\"x\":[10,20]}'->>'$.x[0]',json_type('[1,null]','$[1]'),json_array_length('[1,2,3]');",
  "SELECT json_insert('{}','$.a',1),json_replace('{\"a\":1}','$.a',2),json_set('{}','$.a',json('[1,2]')),json_remove('[1,2,3]','$[1]'),json_patch('{\"a\":1,\"b\":2}','{\"a\":null,\"c\":3}');",
  "SELECT json_valid('{}'),json_valid('{a:1}'),json_valid('{a:1}',2),json_error_position('{bad'),json('{a:1, b:Infinity,}'),json_quote('a'||char(10)||'b');",
  "SELECT json_group_array(amount),json_group_object(id,amount) FROM (SELECT id,amount FROM child ORDER BY id);",
  "SELECT key,value,type,atom,id,parent,fullkey,path FROM json_each('{\"a\":1,\"b\":[2,3]}') ORDER BY key;",
  "SELECT key,value,type,atom,id,parent,fullkey,path FROM json_tree('[{\"a\":1},null]') ORDER BY id;",
  "SELECT typeof(jsonb('{\"a\":[1,2]}')),hex(jsonb('{\"a\":[1,2]}')),json(jsonb('{\"a\":[1,2]}')),jsonb_extract(jsonb('{\"a\":[1,2]}'),'$.a[1]');",
  "SELECT json(jsonb_array(1,'x',NULL)),json(jsonb_object('k',1)),json(jsonb_insert('{}','$.a',1)),json(jsonb_replace('{\"a\":1}','$.a',2)),json(jsonb_set('{}','$.a',jsonb('[1,2]'))),json(jsonb_remove('[1,2,3]','$[1]')),json(jsonb_patch('{\"a\":1}','{\"b\":2}'));",
  "SELECT json(jsonb_group_array(amount)),json(jsonb_group_object(id,amount)) FROM (SELECT id,amount FROM child ORDER BY id);",
  "SELECT json_valid(jsonb('[1]'),4),json_valid(jsonb('[1]'),8),json_extract('{\"\\u263a\":\"\\ud83d\\ude00\"}','$.☺'),json_extract('[1,2,3]','$[#-1]');",
  "SELECT json('{broken');",
  "CREATE VIRTUAL TABLE forbidden_fts USING fts5(content);",
  "CREATE TABLE json_store(id PRIMARY KEY, doc BLOB); INSERT INTO json_store VALUES(1,jsonb('{\"items\":[1,2,3]}')); UPDATE json_store SET doc=jsonb_set(doc,'$.items[1]',99); SELECT id,typeof(doc),json(doc) FROM json_store ORDER BY id;",
  "SELECT json_pretty('{\"a\":[1,2]}'),json_pretty('{\"a\":[1,2]}','..'),json_array_length('{\"a\":[1,2]}','$.a'),json_type('null'),json_extract('{\"a\":1,\"b\":2}','$.a','$.b'),hex(jsonb_extract('{\"a\":[1,2]}','$.a'));",
  "PRAGMA integrity_check; PRAGMA foreign_key_check;"
};
int main(void) {
  sqlite3 *db = 0;
  int i, rc;
  dotcc_memory_vfs_seed(12345);
  dotcc_memory_vfs_time(1704067200000LL);
  rc = sqlite3_open("campaign", &db);
  printf("open %d\n",rc);
  if (rc != SQLITE_OK) return 1;
  for (i = 0; i < (int)(sizeof(corpus)/sizeof(corpus[0])); ++i) run_sql(db,corpus[i],i);
  rc = sqlite3_close(db);
  printf("close %d handles %d\n",rc,dotcc_memory_vfs_handle_count());
  if (rc != SQLITE_OK) return 2;
  rc = dotcc_memory_vfs_reset();
  printf("reset %d files %d\n",rc,dotcc_memory_vfs_file_count());
  return rc == SQLITE_OK ? 0 : 3;
}
