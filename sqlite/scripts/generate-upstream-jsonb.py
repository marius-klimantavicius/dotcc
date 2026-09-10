#!/usr/bin/env python3
"""Adapt all SQL cases in pinned public jsonb01.test without a Tcl dependency.

The source is immutable. This intentionally recognizes only that pinned test's
small table-driven format, and rejects an unexpected shape rather than skipping.
"""
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[1]
source = ROOT / "ref/upstream-tests/sqlite-src-3530400/test/jsonb01.test"
text = source.read_text()

def sql_body(test):
    match = re.search(r"(?ms)^\s*do_(?:execsql|catchsql)_test " + re.escape(test)
                      + r" \{\n(.*?)^\s*\}", text)
    if not match:
        raise SystemExit("Pinned upstream test shape changed: " + test)
    return match.group(1).strip()

setup = sql_body("jsonb01-1.1")
templates = [sql_body("jsonb01-1.2.$id.1"), sql_body("jsonb01-1.2.$id.2")]
malformed = sql_body("jsonb01-2.0")
rows = re.findall(r"^\s*(\d+)\s+\{([^}]*)\}\s+(\{.*\})\s*$", text, re.M)
if len(rows) != 18:
    raise SystemExit("Expected exactly 18 upstream path/result pairs")
cases = []
for number, path, tcl_result in rows:
    expected = tcl_result[2:-2]  # Tcl list-of-one JSON text, two enclosing braces.
    json.loads(expected)         # Validate adaptation; retain original byte order.
    for variant, sql in enumerate(templates, 1):
        cases.append((f"jsonb01-1.2.{number}.{variant}", sql, path, expected))

# JSON's quoted ASCII strings are also valid C string literals for this corpus.
def literal(value):
    return json.dumps(value, ensure_ascii=True)

out = '''/* Generated from pinned SQLite public-domain test/jsonb01.test. */
#include "sqlite3.h"
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>
struct UpstreamCase { const char *name; const char *sql; const char *path; const char *expected; };
static const struct UpstreamCase cases[] = {
'''
out += ",\n".join("  {" + ", ".join(map(literal, case)) + "}" for case in cases)
out += '''
};
int main(void) {
  sqlite3 *db = 0;
  sqlite3_stmt *stmt = 0;
  int i, rc;
  if (sqlite3_open("upstream-jsonb", &db) != SQLITE_OK) return 1;
'''
out += "  if (sqlite3_exec(db, " + literal(setup) + ", 0, 0, 0) != SQLITE_OK) return 2;\n"
out += '''  for (i = 0; i < (int)(sizeof(cases) / sizeof(cases[0])); ++i) {
    rc = sqlite3_prepare_v2(db, cases[i].sql, -1, &stmt, 0);
    if (rc != SQLITE_OK) { printf("FAIL %s prepare %d\\n", cases[i].name, rc); return 3; }
    rc = sqlite3_bind_text(stmt, sqlite3_bind_parameter_index(stmt, "$path"), cases[i].path, -1, SQLITE_STATIC);
    if (rc != SQLITE_OK || sqlite3_step(stmt) != SQLITE_ROW) return 4;
    if (sqlite3_column_type(stmt, 0) != SQLITE_TEXT
        || sqlite3_column_bytes(stmt, 0) != (int)strlen(cases[i].expected)
        || strcmp((const char *)sqlite3_column_text(stmt, 0), cases[i].expected) != 0) {
      printf("FAIL %s value\\n", cases[i].name); return 5;
    }
    if (sqlite3_step(stmt) != SQLITE_DONE || sqlite3_finalize(stmt) != SQLITE_OK) return 6;
    printf("PASS %s\\n", cases[i].name);
  }
'''
out += "  if (sqlite3_prepare_v2(db, " + literal(malformed) + ", -1, &stmt, 0) != SQLITE_OK) return 7;\n"
out += '''  if (sqlite3_step(stmt) != SQLITE_ERROR || strcmp(sqlite3_errmsg(db), "malformed JSON") != 0) return 8;
  if (sqlite3_finalize(stmt) != SQLITE_ERROR) return 9;
  printf("PASS jsonb01-2.0\\n");
  if (sqlite3_close(db) != SQLITE_OK || dotcc_memory_vfs_reset() != SQLITE_OK) return 10;
  printf("PASS all 37 upstream JSONB SQL cases\\n");
  return 0;
}
'''
(ROOT / "generated").mkdir(exist_ok=True)
(ROOT / "generated/upstream-jsonb.c").write_text(out)
(ROOT / "generated/upstream-jsonb-manifest.json").write_text(json.dumps({
    "source": "sqlite-src-3530400/test/jsonb01.test",
    "source_sha256": hashlib.sha256(source.read_bytes()).hexdigest(),
    "setup": "jsonb01-1.1", "cases": [case[0] for case in cases] + ["jsonb01-2.0"],
    "skipped_cases": [],
}, indent=2) + "\n")
print("Generated 37 upstream JSONB cases and setup")
