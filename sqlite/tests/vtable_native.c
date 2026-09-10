/* Explicit module registration contract; compile unchanged for both engines. */
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL vtable line %d: %s\n", __LINE__, #x); return 1; } } while (0)
#define OK(x) CHECK((x) == SQLITE_OK)

typedef struct TableState {
    int connected, disconnected, opened, closed, filtered, constrained, destroyed;
} TableState;
typedef struct NumberTable {
    sqlite3_vtab base;
    TableState *state;
} NumberTable;
typedef struct NumberCursor {
    sqlite3_vtab_cursor base;
    sqlite3_int64 value, end;
} NumberCursor;

static int numbers_connect(sqlite3 *db, void *aux, int argc,
                           const char *const *argv, sqlite3_vtab **out, char **error) {
    NumberTable *table;
    int rc;
    (void)argc; (void)argv; (void)error;
    rc = sqlite3_declare_vtab(db, "CREATE TABLE x(value INTEGER, label TEXT)");
    if (rc != SQLITE_OK) return rc;
    table = (NumberTable *)sqlite3_malloc(sizeof(NumberTable));
    if (!table) return SQLITE_NOMEM;
    memset(table, 0, sizeof(NumberTable));
    table->state = (TableState *)aux;
    ++table->state->connected;
    *out = &table->base;
    return SQLITE_OK;
}
static int numbers_disconnect(sqlite3_vtab *base) {
    NumberTable *table = (NumberTable *)base;
    ++table->state->disconnected;
    sqlite3_free(table);
    return SQLITE_OK;
}
static int numbers_best_index(sqlite3_vtab *base, sqlite3_index_info *info) {
    int i;
    (void)base;
    info->estimatedCost = 5;
    info->estimatedRows = 5;
    for (i = 0; i < info->nConstraint; ++i) {
        if (info->aConstraint[i].usable && info->aConstraint[i].iColumn == 0 &&
            info->aConstraint[i].op == SQLITE_INDEX_CONSTRAINT_EQ) {
            info->aConstraintUsage[i].argvIndex = 1;
            /* Leave omit false: SQLite still applies its affinity/equality rules. */
            info->idxNum = 1;
            info->estimatedCost = 1;
            info->estimatedRows = 1;
            info->idxFlags = SQLITE_INDEX_SCAN_UNIQUE;
            break;
        }
    }
    return SQLITE_OK;
}
static int numbers_open(sqlite3_vtab *base, sqlite3_vtab_cursor **out) {
    NumberCursor *cursor = (NumberCursor *)sqlite3_malloc(sizeof(NumberCursor));
    if (!cursor) return SQLITE_NOMEM;
    memset(cursor, 0, sizeof(NumberCursor));
    cursor->base.pVtab = base;
    ++((NumberTable *)base)->state->opened;
    *out = &cursor->base;
    return SQLITE_OK;
}
static int numbers_close(sqlite3_vtab_cursor *base) {
    ++((NumberTable *)base->pVtab)->state->closed;
    sqlite3_free(base);
    return SQLITE_OK;
}
static int numbers_filter(sqlite3_vtab_cursor *base, int index, const char *text,
                          int argc, sqlite3_value **argv) {
    NumberCursor *cursor = (NumberCursor *)base;
    TableState *state = ((NumberTable *)base->pVtab)->state;
    (void)text;
    ++state->filtered;
    cursor->value = 1;
    cursor->end = 5;
    if (index == 1 && argc == 1) {
        ++state->constrained;
        cursor->value = sqlite3_value_int64(argv[0]);
        cursor->end = cursor->value;
        if (sqlite3_value_type(argv[0]) == SQLITE_NULL || cursor->value < 1 || cursor->value > 5) {
            cursor->value = 1;
            cursor->end = 0;
        }
    }
    return SQLITE_OK;
}
static int numbers_next(sqlite3_vtab_cursor *base) {
    ++((NumberCursor *)base)->value;
    return SQLITE_OK;
}
static int numbers_eof(sqlite3_vtab_cursor *base) {
    NumberCursor *cursor = (NumberCursor *)base;
    return cursor->value > cursor->end;
}
static int numbers_column(sqlite3_vtab_cursor *base, sqlite3_context *context, int column) {
    NumberCursor *cursor = (NumberCursor *)base;
    if (column == 0) sqlite3_result_int64(context, cursor->value);
    else if (cursor->value == 3) sqlite3_result_null(context);
    else sqlite3_result_text(context, cursor->value % 2 ? "odd" : "even", -1, SQLITE_STATIC);
    return SQLITE_OK;
}
static int numbers_rowid(sqlite3_vtab_cursor *base, sqlite3_int64 *rowid) {
    *rowid = ((NumberCursor *)base)->value * 10;
    return SQLITE_OK;
}
static void numbers_destroy(void *context) { ++((TableState *)context)->destroyed; }

static sqlite3_module numbers_module = {
    .iVersion = 1, .xCreate = numbers_connect, .xConnect = numbers_connect,
    .xBestIndex = numbers_best_index, .xDisconnect = numbers_disconnect,
    .xDestroy = numbers_disconnect, .xOpen = numbers_open, .xClose = numbers_close,
    .xFilter = numbers_filter, .xNext = numbers_next, .xEof = numbers_eof,
    .xColumn = numbers_column, .xRowid = numbers_rowid
};

static int expect(sqlite3 *db, const char *sql, const char *wanted) {
    sqlite3_stmt *statement = NULL;
    const unsigned char *actual;
    OK(sqlite3_prepare_v2(db, sql, -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW);
    actual = sqlite3_column_text(statement, 0);
    CHECK(actual && strcmp((const char *)actual, wanted) == 0);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    return 0;
}

int main(void) {
    sqlite3 *db = NULL;
    TableState state;
    memset(&state, 0, sizeof(state));
    OK(sqlite3_initialize());
    OK(sqlite3_open("vtable-contract.db", &db));
    OK(sqlite3_create_module_v2(db, "dotcc_numbers", &numbers_module, &state, numbers_destroy));
    OK(sqlite3_exec(db, "CREATE VIRTUAL TABLE numbers USING dotcc_numbers", NULL, NULL, NULL));
    CHECK(expect(db, "SELECT group_concat(value||':'||coalesce(label,'NULL')||':'||rowid) FROM (SELECT * ,rowid FROM numbers ORDER BY value)",
                 "1:odd:10,2:even:20,3:NULL:30,4:even:40,5:odd:50") == 0);
    CHECK(expect(db, "SELECT value||':'||rowid FROM numbers WHERE value=4", "4:40") == 0);
    CHECK(expect(db, "SELECT count(*) FROM numbers WHERE value=NULL OR value=99", "0") == 0);
    CHECK(expect(db, "SELECT count(*) FROM numbers WHERE value=2.5", "0") == 0);
    CHECK(expect(db, "SELECT value FROM numbers WHERE value='2'", "2") == 0);
    CHECK(expect(db, "SELECT json_group_array(value) FROM (SELECT value FROM numbers WHERE label IS NOT NULL ORDER BY value)", "[1,2,4,5]") == 0);
    CHECK(expect(db, "SELECT sum(n.value*j.value) FROM numbers n JOIN json_each('[2,4]') j ON n.value=j.value", "20") == 0);
    CHECK(sqlite3_exec(db, "INSERT INTO numbers VALUES(6,'even')", NULL, NULL, NULL) == SQLITE_ERROR);
    CHECK(strcmp(sqlite3_errmsg(db), "table numbers may not be modified") == 0);
    OK(sqlite3_exec(db, "CREATE VIRTUAL TABLE enabled_fts USING fts5(body); DROP TABLE enabled_fts", NULL, NULL, NULL));
    CHECK(sqlite3_exec(db, "CREATE VIRTUAL TABLE unwanted3 USING fts3(body)", NULL, NULL, NULL) == SQLITE_ERROR);
    CHECK(sqlite3_exec(db, "CREATE VIRTUAL TABLE unwanted4 USING fts4(body)", NULL, NULL, NULL) == SQLITE_ERROR);
    CHECK(state.connected == 1 && state.filtered > 0 && state.constrained > 0);
    CHECK(state.opened == state.closed && state.destroyed == 0);
    OK(sqlite3_exec(db, "DROP TABLE numbers", NULL, NULL, NULL));
    CHECK(state.disconnected == state.connected);
    OK(sqlite3_create_module_v2(db, "dotcc_numbers", NULL, NULL, NULL));
    CHECK(state.destroyed == 1);
    OK(sqlite3_close(db));
    OK(sqlite3_shutdown());
    printf("PASS explicit-vtable: scan, constraints, rowid, NULL, JSON join, read-only, FTS5 enabled, FTS3/4 absent, lifetimes\n");
    return 0;
}
