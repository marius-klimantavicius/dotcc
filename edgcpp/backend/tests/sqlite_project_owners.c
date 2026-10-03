/* Consumer of the unchanged pinned SQLite engine. This pointer is deliberately
   a translated global: independent generated owners must never share it. */
#include "sqlite3.h"
static sqlite3 *project_database;

int project_is_open(void) { return project_database != 0; }

int project_execute(const char *sql) {
  return sqlite3_exec(project_database, sql, 0, 0, 0);
}

int project_open(int marker) {
  int rc;
  sqlite3_stmt *statement = 0;
  if (project_database) return SQLITE_MISUSE;
  rc = sqlite3_open(":memory:", &project_database);
  if (rc != SQLITE_OK) return rc;
  rc = project_execute("CREATE TABLE values_table(value INTEGER NOT NULL)");
  if (rc != SQLITE_OK) return rc;
  rc = sqlite3_prepare_v2(project_database, "INSERT INTO values_table VALUES(?)", -1, &statement, 0);
  if (rc != SQLITE_OK) return rc;
  rc = sqlite3_bind_int(statement, 1, marker);
  if (rc == SQLITE_OK) rc = sqlite3_step(statement);
  int finalized = sqlite3_finalize(statement);
  return rc == SQLITE_DONE ? finalized : rc;
}

long long project_sum(void) {
  sqlite3_stmt *statement = 0;
  long long value = -1;
  int rc = sqlite3_prepare_v2(project_database, "SELECT sum(value) FROM values_table", -1, &statement, 0);
  if (rc == SQLITE_OK && sqlite3_step(statement) == SQLITE_ROW)
    value = sqlite3_column_int64(statement, 0);
  if (statement && sqlite3_finalize(statement) != SQLITE_OK) return -2;
  return value;
}

int project_close(void) {
  int rc = sqlite3_close(project_database);
  if (rc != SQLITE_OK) return rc;
  project_database = 0;
  return sqlite3_shutdown();
}
