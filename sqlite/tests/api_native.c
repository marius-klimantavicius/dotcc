/* Deterministic API contract shared by the native oracle and translated build.
 * No native interop or OS file calls; all databases use the translated C VFS.
 */
#include "memory_vfs.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL API line %d: %s\n", __LINE__, #x); return 1; } } while (0)
#define OK(x) CHECK((x) == SQLITE_OK)

static int bound_destroyed;
static int result_destroyed;
static int function_destroyed;
static int collation_destroyed;

typedef struct CallbackState {
    int scalar_calls;
    int collation_calls;
    int progress_calls;
    int busy_calls;
    int commits;
    int rollbacks;
    int updates;
} CallbackState;

static void destroy_bound(void *p) { ++bound_destroyed; sqlite3_free(p); }
static void destroy_result(void *p) { ++result_destroyed; sqlite3_free(p); }
static void destroy_function(void *p) { (void)p; ++function_destroyed; }
static void destroy_collation(void *p) { (void)p; ++collation_destroyed; }

static int expect_text(sqlite3 *db, const char *sql, const char *expected) {
    sqlite3_stmt *stmt = NULL;
    const unsigned char *text;
    OK(sqlite3_prepare_v2(db, sql, -1, &stmt, NULL));
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    text = sqlite3_column_text(stmt, 0);
    CHECK(text && strcmp((const char *)text, expected) == 0);
    CHECK(sqlite3_step(stmt) == SQLITE_DONE);
    OK(sqlite3_finalize(stmt));
    return 0;
}

static int binding_contract(sqlite3 *db) {
    sqlite3_stmt *stmt = NULL;
    const char *tail = NULL;
    char transient_text[3] = {'a', 0, 'b'};
    unsigned char static_blob[4] = {0, 127, 128, 255};
    char *owned;
    unsigned short utf16_text[3] = {'A', 0x03a9, 0};
    unsigned short utf16_sql[10] = {'S','E','L','E','C','T',' ','?','1',0};
    unsigned short utf16_name[8] = {'u','t','f','1','6','d','b',0};
    sqlite3 *utf16_db = NULL;
    OK(sqlite3_prepare_v3(db, "SELECT :n,:t,:b,:i,:r; SELECT 9", -1,
                          SQLITE_PREPARE_PERSISTENT, &stmt, &tail));
    CHECK(tail && strcmp(tail, " SELECT 9") == 0);
    CHECK(sqlite3_bind_parameter_count(stmt) == 5);
    CHECK(sqlite3_bind_parameter_index(stmt, ":t") == 2);
    CHECK(strcmp(sqlite3_bind_parameter_name(stmt, 3), ":b") == 0);
    CHECK(sqlite3_bind_int(stmt, 6, 0) == SQLITE_RANGE);
    OK(sqlite3_bind_null(stmt, 1));
    OK(sqlite3_bind_text(stmt, 2, transient_text, 3, SQLITE_TRANSIENT));
    OK(sqlite3_bind_blob(stmt, 3, static_blob, 4, SQLITE_STATIC));
    OK(sqlite3_bind_int64(stmt, 4, -9223372036854775807LL));
    OK(sqlite3_bind_double(stmt, 5, 1.25));
    transient_text[0] = 'z';
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    CHECK(sqlite3_column_count(stmt) == 5 && sqlite3_data_count(stmt) == 5);
    CHECK(sqlite3_column_type(stmt, 0) == SQLITE_NULL);
    CHECK(sqlite3_column_text(stmt, 0) == NULL && sqlite3_column_bytes(stmt, 0) == 0);
    CHECK(sqlite3_column_type(stmt, 1) == SQLITE_TEXT && sqlite3_column_bytes(stmt, 1) == 3);
    CHECK(memcmp(sqlite3_column_text(stmt, 1), "a\0b", 3) == 0);
    CHECK(sqlite3_column_type(stmt, 2) == SQLITE_BLOB && sqlite3_column_bytes(stmt, 2) == 4);
    CHECK(memcmp(sqlite3_column_blob(stmt, 2), static_blob, 4) == 0);
    CHECK(sqlite3_column_type(stmt, 3) == SQLITE_INTEGER);
    CHECK(sqlite3_column_int64(stmt, 3) == -9223372036854775807LL);
    CHECK(sqlite3_column_type(stmt, 4) == SQLITE_FLOAT && sqlite3_column_double(stmt, 4) == 1.25);
    CHECK(sqlite3_step(stmt) == SQLITE_DONE && sqlite3_data_count(stmt) == 0);
    OK(sqlite3_reset(stmt));
    CHECK(sqlite3_step(stmt) == SQLITE_ROW); /* reset preserves bindings */
    CHECK(memcmp(sqlite3_column_text(stmt, 1), "a\0b", 3) == 0);
    OK(sqlite3_reset(stmt));
    OK(sqlite3_clear_bindings(stmt));
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    CHECK(sqlite3_column_type(stmt, 1) == SQLITE_NULL);
    OK(sqlite3_reset(stmt));
    owned = (char *)sqlite3_malloc(4); CHECK(owned);
    memcpy(owned, "own", 4);
    OK(sqlite3_bind_text(stmt, 2, owned, 3, destroy_bound));
    CHECK(bound_destroyed == 0);
    OK(sqlite3_clear_bindings(stmt)); CHECK(bound_destroyed == 1);
    owned = (char *)sqlite3_malloc(4); CHECK(owned);
    memcpy(owned, "end", 4);
    OK(sqlite3_bind_blob(stmt, 3, owned, 4, destroy_bound));
    CHECK(sqlite3_close(db) == SQLITE_BUSY); /* caller can still use database */
    OK(sqlite3_finalize(stmt)); CHECK(bound_destroyed == 2);

    OK(sqlite3_prepare16_v2(db, utf16_sql, -1, &stmt, NULL));
    OK(sqlite3_bind_text16(stmt, 1, utf16_text, 4, SQLITE_TRANSIENT));
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    CHECK(sqlite3_column_bytes16(stmt, 0) == 4);
    CHECK(memcmp(sqlite3_column_text16(stmt, 0), utf16_text, 4) == 0);
    CHECK(sqlite3_column_bytes(stmt, 0) == 3);
    CHECK(memcmp(sqlite3_column_text(stmt, 0), "A\xce\xa9", 3) == 0);
    OK(sqlite3_finalize(stmt));
    OK(sqlite3_open16(utf16_name, &utf16_db));
    CHECK(expect_text(utf16_db, "SELECT 'UTF16 open'", "UTF16 open") == 0);
    OK(sqlite3_close(utf16_db));
    CHECK(expect_text(db, "SELECT CAST('123' AS INTEGER)||':'||CAST(3.5 AS TEXT)||':'||hex(x'007FFF')", "123:3.5:007FFF") == 0);
    puts("PASS API bindings ownership UTF8 UTF16 conversions lifecycle");
    return 0;
}

static void scalar_echo(sqlite3_context *ctx, int count, sqlite3_value **values) {
    CallbackState *state = (CallbackState *)sqlite3_user_data(ctx);
    const void *input;
    void *copy;
    int size;
    ++state->scalar_calls;
    if (count != 1) { sqlite3_result_error(ctx, "arity", -1); return; }
    if (sqlite3_value_type(values[0]) == SQLITE_NULL) { sqlite3_result_null(ctx); return; }
    size = sqlite3_value_bytes(values[0]);
    input = sqlite3_value_blob(values[0]);
    copy = sqlite3_malloc(size ? size : 1);
    if (!copy) { sqlite3_result_error_nomem(ctx); return; }
    if (size) memcpy(copy, input, (size_t)size);
    sqlite3_result_blob(ctx, copy, size, destroy_result);
}

static void sum_step(sqlite3_context *ctx, int count, sqlite3_value **values) {
    sqlite3_int64 *sum = (sqlite3_int64 *)sqlite3_aggregate_context(ctx, sizeof(sqlite3_int64));
    if (!sum) { sqlite3_result_error_nomem(ctx); return; }
    if (count == 1 && sqlite3_value_type(values[0]) != SQLITE_NULL)
        *sum += sqlite3_value_int64(values[0]);
}

static void sum_final(sqlite3_context *ctx) {
    sqlite3_int64 *sum = (sqlite3_int64 *)sqlite3_aggregate_context(ctx, 0);
    sqlite3_result_int64(ctx, sum ? *sum : 0);
}

static int reverse_collation(void *context, int an, const void *a, int bn, const void *b) {
    CallbackState *state = (CallbackState *)context;
    int result;
    ++state->collation_calls;
    result = memcmp(a, b, (size_t)(an < bn ? an : bn));
    if (!result) result = an < bn ? -1 : an > bn;
    return result < 0 ? 1 : result > 0 ? -1 : 0;
}

static int busy_callback(void *context, int previous) {
    CallbackState *state = (CallbackState *)context;
    ++state->busy_calls;
    return previous < 2;
}

static int progress_callback(void *context) {
    CallbackState *state = (CallbackState *)context;
    ++state->progress_calls;
    return state->progress_calls >= 20;
}

static int commit_callback(void *context) {
    ++((CallbackState *)context)->commits;
    return 0;
}

static void rollback_callback(void *context) { ++((CallbackState *)context)->rollbacks; }

static void update_callback(void *context, int operation, const char *db,
                            const char *table, sqlite3_int64 rowid) {
    (void)operation; (void)db; (void)table; (void)rowid;
    ++((CallbackState *)context)->updates;
}

static int callback_contract(sqlite3 *db) {
    CallbackState state;
    sqlite3 *other = NULL;
    int commits, rollbacks;
    memset(&state, 0, sizeof(state));
    OK(sqlite3_create_function_v2(db, "owned_echo", 1, SQLITE_UTF8,
        &state, scalar_echo, NULL, NULL, destroy_function));
    OK(sqlite3_create_function_v2(db, "test_sum", 1, SQLITE_UTF8,
        NULL, NULL, sum_step, sum_final, NULL));
    OK(sqlite3_create_collation_v2(db, "reverse_bytes", SQLITE_UTF8,
        &state, reverse_collation, destroy_collation));
    CHECK(expect_text(db, "SELECT hex(owned_echo(x'006180FF'))", "006180FF") == 0);
    CHECK(state.scalar_calls == 1 && result_destroyed == 1);
    CHECK(expect_text(db, "SELECT owned_echo(NULL) IS NULL", "1") == 0);
    CHECK(expect_text(db, "WITH x(v) AS (VALUES(1),(2),(NULL),(4)) SELECT test_sum(v) FROM x", "7") == 0);
    CHECK(expect_text(db, "SELECT test_sum(1) WHERE 0", "0") == 0);
    CHECK(expect_text(db, "SELECT group_concat(v,'') FROM (SELECT column1 AS v FROM (VALUES('a'),('c'),('b')) ORDER BY v COLLATE reverse_bytes)", "cba") == 0);
    CHECK(state.collation_calls > 0);
    sqlite3_progress_handler(db, 1, progress_callback, &state);
    CHECK(sqlite3_exec(db, "WITH RECURSIVE n(x) AS (VALUES(0) UNION ALL SELECT x+1 FROM n WHERE x<1000) SELECT sum(x) FROM n", NULL, NULL, NULL) == SQLITE_INTERRUPT);
    CHECK(state.progress_calls >= 20);
    sqlite3_progress_handler(db, 0, NULL, NULL);
    CHECK(expect_text(db, "SELECT 1", "1") == 0);
    OK(sqlite3_open_v2("api.db", &other, SQLITE_OPEN_READWRITE, DOTCC_MEMORY_VFS_NAME));
    OK(sqlite3_busy_handler(other, busy_callback, &state));
    OK(sqlite3_exec(db, "BEGIN EXCLUSIVE", NULL, NULL, NULL));
    CHECK(sqlite3_exec(other, "BEGIN IMMEDIATE", NULL, NULL, NULL) == SQLITE_BUSY);
    CHECK(state.busy_calls == 3);
    OK(sqlite3_exec(db, "ROLLBACK", NULL, NULL, NULL));
    OK(sqlite3_busy_handler(other, NULL, NULL));
    OK(sqlite3_close(other));

    sqlite3_commit_hook(db, commit_callback, &state);
    sqlite3_rollback_hook(db, rollback_callback, &state);
    sqlite3_update_hook(db, update_callback, &state);
    OK(sqlite3_exec(db, "CREATE TABLE hooks(v); INSERT INTO hooks VALUES(1);", NULL, NULL, NULL));
    commits = state.commits; rollbacks = state.rollbacks;
    OK(sqlite3_exec(db, "BEGIN; UPDATE hooks SET v=2; ROLLBACK;", NULL, NULL, NULL));
    CHECK(state.commits == commits && state.rollbacks == rollbacks + 1);
    CHECK(state.updates == 2);
    CHECK(sqlite3_commit_hook(db, NULL, NULL) == &state);
    CHECK(sqlite3_rollback_hook(db, NULL, NULL) == &state);
    CHECK(sqlite3_update_hook(db, NULL, NULL) == &state);
    OK(sqlite3_create_function_v2(db, "owned_echo", 1, SQLITE_UTF8, NULL, NULL, NULL, NULL, NULL));
    OK(sqlite3_create_collation_v2(db, "reverse_bytes", SQLITE_UTF8, NULL, NULL, NULL));
    CHECK(function_destroyed == 1 && collation_destroyed == 1);
    puts("PASS API scalar aggregate collation progress busy transaction callbacks");
    return 0;
}

static int backup_blob_contract(sqlite3 *db) {
    sqlite3_blob *blob = NULL;
    sqlite3_backup *backup;
    sqlite3 *copy = NULL;
    unsigned char bytes[16];
    int i, rc, steps = 0;
    OK(sqlite3_exec(db, "CREATE TABLE payload(id INTEGER PRIMARY KEY,b BLOB); INSERT INTO payload VALUES(1,zeroblob(16));", NULL, NULL, NULL));
    CHECK(sqlite3_last_insert_rowid(db) == 1 && sqlite3_changes(db) == 1);
    OK(sqlite3_blob_open(db, "main", "payload", "b", 1, 1, &blob));
    CHECK(sqlite3_blob_bytes(blob) == 16);
    OK(sqlite3_blob_write(blob, "DATA", 4, 4));
    OK(sqlite3_blob_read(blob, bytes, 16, 0));
    CHECK(memcmp(bytes + 4, "DATA", 4) == 0);
    for (i = 0; i < 16; ++i) if (i < 4 || i >= 8) CHECK(bytes[i] == 0);
    CHECK(sqlite3_blob_write(blob, "x", 1, 16) == SQLITE_ERROR);
    OK(sqlite3_blob_close(blob));
    OK(sqlite3_blob_open(db, "main", "payload", "b", 1, 0, &blob));
    CHECK(sqlite3_blob_write(blob, "x", 1, 0) == SQLITE_READONLY);
    CHECK(sqlite3_blob_close(blob) == SQLITE_READONLY); /* carries prior I/O error */
    OK(sqlite3_open_v2("backup.db", &copy, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, DOTCC_MEMORY_VFS_NAME));
    backup = sqlite3_backup_init(copy, "main", db, "main"); CHECK(backup);
    do {
        rc = sqlite3_backup_step(backup, 1);
        CHECK(rc == SQLITE_OK || rc == SQLITE_DONE);
        CHECK(++steps < 1000);
    } while (rc == SQLITE_OK);
    CHECK(sqlite3_backup_remaining(backup) == 0 && sqlite3_backup_pagecount(backup) > 0);
    OK(sqlite3_backup_finish(backup));
    CHECK(expect_text(copy, "SELECT hex(b) FROM payload", "00000000444154410000000000000000") == 0);
    CHECK(expect_text(copy, "PRAGMA integrity_check", "ok") == 0);
    OK(sqlite3_close(copy));
    puts("PASS API incremental blob backup and read-only handles");
    return 0;
}

static unsigned int next_random(unsigned int *seed) {
    *seed = *seed * 1664525U + 1013904223U;
    return *seed;
}

static int randomized_contract(sqlite3 *db) {
    unsigned int seed = 0x51a17eU;
    sqlite3_int64 values[16], saved_values[16];
    int present[16], saved_present[16];
    sqlite3_stmt *upsert = NULL, *remove = NULL, *rows = NULL;
    int batch, operation, key, i, row;
    sqlite3_int64 value;
    memset(values, 0, sizeof(values)); memset(present, 0, sizeof(present));
    OK(sqlite3_exec(db, "CREATE TABLE randomized(k INTEGER PRIMARY KEY,v INTEGER NOT NULL);", NULL, NULL, NULL));
    OK(sqlite3_prepare_v2(db, "INSERT INTO randomized VALUES(?1,?2) ON CONFLICT(k) DO UPDATE SET v=v+excluded.v", -1, &upsert, NULL));
    OK(sqlite3_prepare_v2(db, "DELETE FROM randomized WHERE k=?1", -1, &remove, NULL));
    for (batch = 0; batch < 16; ++batch) {
        memcpy(saved_values, values, sizeof(values));
        memcpy(saved_present, present, sizeof(present));
        OK(sqlite3_exec(db, "SAVEPOINT random_batch", NULL, NULL, NULL));
        for (operation = 0; operation < 16; ++operation) {
            key = (int)((next_random(&seed) >> 16) & 15);
            value = (int)(next_random(&seed) % 2001) - 1000;
            if ((next_random(&seed) >> 16) % 4 == 0) {
                OK(sqlite3_bind_int(remove, 1, key));
                CHECK(sqlite3_step(remove) == SQLITE_DONE);
                OK(sqlite3_reset(remove)); present[key] = 0; values[key] = 0;
            } else {
                OK(sqlite3_bind_int(upsert, 1, key)); OK(sqlite3_bind_int64(upsert, 2, value));
                CHECK(sqlite3_step(upsert) == SQLITE_DONE); OK(sqlite3_reset(upsert));
                values[key] = present[key] ? values[key] + value : value; present[key] = 1;
            }
        }
        if (batch % 3 == 0) {
            OK(sqlite3_exec(db, "ROLLBACK TO random_batch", NULL, NULL, NULL));
            memcpy(values, saved_values, sizeof(values)); memcpy(present, saved_present, sizeof(present));
        }
        OK(sqlite3_exec(db, "RELEASE random_batch", NULL, NULL, NULL));
        OK(sqlite3_prepare_v2(db, "SELECT k,v FROM randomized ORDER BY k", -1, &rows, NULL));
        for (i = 0; i < 16; ++i) if (present[i]) {
            row = sqlite3_step(rows); CHECK(row == SQLITE_ROW);
            CHECK(sqlite3_column_type(rows, 0) == SQLITE_INTEGER && sqlite3_column_type(rows, 1) == SQLITE_INTEGER);
            CHECK(sqlite3_column_int(rows, 0) == i && sqlite3_column_int64(rows, 1) == values[i]);
        }
        CHECK(sqlite3_step(rows) == SQLITE_DONE); OK(sqlite3_finalize(rows));
    }
    OK(sqlite3_finalize(upsert)); OK(sqlite3_finalize(remove));
    printf("RANDOM seed=0x51a17e operations=256 state=%u\n", seed);
    for (i = 0; i < 16; ++i) if (present[i]) printf("RANDOM row INTEGER:%d INTEGER:%lld\n", i, (long long)values[i]);
    CHECK(expect_text(db, "PRAGMA integrity_check", "ok") == 0);
    puts("PASS API deterministic randomized transactions versus independent model");
    return 0;
}

static int error_contract(sqlite3 *db) {
    sqlite3_stmt *stmt = NULL;
    int rc;
    CHECK(sqlite3_prepare_v2(db, "SELECT FROM", -1, &stmt, NULL) == SQLITE_ERROR);
    CHECK(stmt == NULL && sqlite3_errcode(db) == SQLITE_ERROR);
    CHECK(sqlite3_error_offset(db) >= 0);
    OK(sqlite3_exec(db, "CREATE TABLE errors(v INTEGER UNIQUE NOT NULL); INSERT INTO errors VALUES(1);", NULL, NULL, NULL));
    OK(sqlite3_extended_result_codes(db, 1));
    OK(sqlite3_prepare_v2(db, "INSERT INTO errors VALUES(1)", -1, &stmt, NULL));
    rc = sqlite3_step(stmt); CHECK(rc == SQLITE_CONSTRAINT_UNIQUE);
    CHECK(sqlite3_extended_errcode(db) == SQLITE_CONSTRAINT_UNIQUE);
    CHECK(sqlite3_reset(stmt) == SQLITE_CONSTRAINT_UNIQUE);
    CHECK(sqlite3_step(stmt) == SQLITE_CONSTRAINT_UNIQUE);
    CHECK(sqlite3_finalize(stmt) == SQLITE_CONSTRAINT_UNIQUE);
    CHECK(sqlite3_exec(db, "INSERT INTO errors VALUES(NULL)", NULL, NULL, NULL) == SQLITE_CONSTRAINT_NOTNULL);
    OK(sqlite3_extended_result_codes(db, 0));
    CHECK(expect_text(db, "SELECT count(*) FROM errors", "1") == 0);
    puts("PASS API prepare errors extended constraints reset finalize error propagation");
    return 0;
}

#include "optional_features.h"

int main(void) {
    sqlite3 *db = NULL;
    OK(dotcc_memory_vfs_reset());
    OK(sqlite3_open_v2("api.db", &db, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, DOTCC_MEMORY_VFS_NAME));
    CHECK(optional_features_contract(db) == 0);
    CHECK(binding_contract(db) == 0);
    CHECK(callback_contract(db) == 0);
    CHECK(backup_blob_contract(db) == 0);
    CHECK(randomized_contract(db) == 0);
    CHECK(error_contract(db) == 0);
    OK(sqlite3_close(db));
    CHECK(dotcc_memory_vfs_handle_count() == 0);
    OK(dotcc_memory_vfs_reset());
    OK(sqlite3_shutdown());
    puts("PASS all memory VFS C API contracts");
    return 0;
}
