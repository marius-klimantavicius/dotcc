/* Bounded deterministic SQLite allocator fault injection, shared by both engines. */
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL allocation line %d: %s\n", __LINE__, #x); return 1; } } while (0)
#define OK(x) CHECK((x) == SQLITE_OK)

static sqlite3_mem_methods original_allocator;
static int allocation_countdown = -1;
static int failures;

static int should_fail(void) {
    if (allocation_countdown < 0) return 0;
    if (allocation_countdown-- > 0) return 0;
    ++failures;
    return 1;
}
static void *fault_malloc(int size) {
    return should_fail() ? NULL : original_allocator.xMalloc(size);
}
static void *fault_realloc(void *pointer, int size) {
    return should_fail() ? NULL : original_allocator.xRealloc(pointer, size);
}
static void fault_free(void *pointer) { original_allocator.xFree(pointer); }
static int fault_size(void *pointer) { return original_allocator.xSize(pointer); }
static int fault_roundup(int size) { return original_allocator.xRoundup(size); }
static int fault_init(void *context) {
    (void)context;
    return original_allocator.xInit ? original_allocator.xInit(original_allocator.pAppData) : SQLITE_OK;
}
static void fault_shutdown(void *context) {
    (void)context;
    if (original_allocator.xShutdown) original_allocator.xShutdown(original_allocator.pAppData);
}

static int verify_recovery(sqlite3 *db) {
    sqlite3_stmt *statement = NULL;
    OK(sqlite3_prepare_v2(db, "PRAGMA integrity_check", -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 0), "ok") == 0);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    OK(sqlite3_exec(db, "DELETE FROM data; INSERT INTO data VALUES(1,jsonb('{\"ok\":true}'))", NULL, NULL, NULL));
    OK(sqlite3_prepare_v2(db, "SELECT json_extract(payload,'$.ok') FROM data", -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW && sqlite3_column_int(statement, 0) == 1);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    return 0;
}

int main(void) {
    sqlite3_mem_methods allocator;
    sqlite3 *db = NULL;
    sqlite3_stmt *statement = NULL;
    int phase, i, rc, final_rc, nomem_results[2] = {0, 0};
    const char *insert_sql =
        "WITH RECURSIVE n(v) AS(VALUES(1) UNION ALL SELECT v+1 FROM n WHERE v<32) "
        "INSERT INTO data SELECT v,jsonb_object('n',v,'items',json_array(v,v+1,v+2)) FROM n";
    OK(sqlite3_shutdown());
    OK(sqlite3_config(SQLITE_CONFIG_GETMALLOC, &original_allocator));
    memset(&allocator, 0, sizeof(allocator));
    allocator.xMalloc = fault_malloc;
    allocator.xFree = fault_free;
    allocator.xRealloc = fault_realloc;
    allocator.xSize = fault_size;
    allocator.xRoundup = fault_roundup;
    allocator.xInit = fault_init;
    allocator.xShutdown = fault_shutdown;
    OK(sqlite3_config(SQLITE_CONFIG_MALLOC, &allocator));
    OK(sqlite3_initialize());
    for (phase = 0; phase < 2; ++phase) for (i = 0; i < 64; ++i) {
        OK(sqlite3_open("allocation-contract.db", &db));
        /* Force this test's requests through the configured allocator. */
        OK(sqlite3_db_config(db, SQLITE_DBCONFIG_LOOKASIDE, NULL, 0, 0));
        OK(sqlite3_exec(db, "CREATE TABLE data(id INTEGER PRIMARY KEY,payload BLOB);", NULL, NULL, NULL));
        if (phase == 0) {
            allocation_countdown = i;
            rc = sqlite3_exec(db, insert_sql, NULL, NULL, NULL);
            allocation_countdown = -1;
        } else {
            OK(sqlite3_exec(db, "BEGIN", NULL, NULL, NULL));
            OK(sqlite3_prepare_v2(db, insert_sql, -1, &statement, NULL));
            allocation_countdown = i;
            rc = sqlite3_step(statement);
            allocation_countdown = -1;
            if (rc == SQLITE_DONE) rc = SQLITE_OK;
            final_rc = sqlite3_finalize(statement);
            CHECK(final_rc == rc);
        }
        CHECK(rc == SQLITE_OK || rc == SQLITE_NOMEM);
        if (rc == SQLITE_NOMEM) ++nomem_results[phase];
        if (!sqlite3_get_autocommit(db)) OK(sqlite3_exec(db, "ROLLBACK", NULL, NULL, NULL));
        CHECK(verify_recovery(db) == 0);
        OK(sqlite3_close(db));
        CHECK(dotcc_memory_vfs_handle_count() == 0);
        OK(dotcc_memory_vfs_reset());
    }
    CHECK(failures == 128 && nomem_results[0] > 0 && nomem_results[1] > 0);
    OK(sqlite3_shutdown());
    CHECK(sqlite3_memory_used() == 0);
    OK(sqlite3_config(SQLITE_CONFIG_MALLOC, &original_allocator));
    printf("PASS allocation: 128 injected failures, exec=%d step=%d NOMEM results, recovery, integrity, JSONB, cleanup\n",
           nomem_results[0], nomem_results[1]);
    return 0;
}
