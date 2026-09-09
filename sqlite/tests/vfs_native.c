/* Compile this identical contract harness with native and translated SQLite.
 * The historical filename denotes its first oracle; tests use no OS file APIs.
 */
#include "memory_vfs.h"
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL line %d: %s\n", __LINE__, #x); return 1; } } while (0)
#define OK(x) CHECK((x) == SQLITE_OK)

static sqlite3_file *new_file(void) {
    return (sqlite3_file *)calloc(1, (size_t)dotcc_memory_vfs()->szOsFile);
}

static int open_file(const char *name, sqlite3_file *file, int flags) {
    return dotcc_memory_vfs()->xOpen(dotcc_memory_vfs(), name, file, flags, NULL);
}

static int close_file(sqlite3_file *file) {
    int rc = file->pMethods->xClose(file);
    free(file);
    return rc;
}

static int direct_contract(void) {
    sqlite3_vfs *vfs = dotcc_memory_vfs();
    sqlite3_file *a = new_file(), *b = new_file(), *c = new_file();
    const sqlite3_io_methods *io;
    int rw = SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE | SQLITE_OPEN_MAIN_DB;
    int exists, state;
    char buffer[16], path[1025], random1[16], random2[16];
    double day;
    sqlite3_int64 size;
    CHECK(a && b && c);
    OK(dotcc_memory_vfs_reset());
    CHECK(vfs->iVersion == 1);
    OK(vfs->xFullPathname(vfs, "./a//b/../file", sizeof(path), path));
    CHECK(strcmp(path, "/a/file") == 0);
    CHECK(vfs->xFullPathname(vfs, "abc", 4, path) == SQLITE_CANTOPEN);
    OK(open_file("./contract.db", a, rw));
    OK(open_file("/contract.db", b, rw));
    CHECK(dotcc_memory_vfs_reset() == SQLITE_BUSY);
    CHECK(dotcc_memory_vfs_file_count() == 1 && dotcc_memory_vfs_handle_count() == 2);
    io = a->pMethods;
    CHECK(io->iVersion == 1 && io->xShmMap == NULL && io->xFetch == NULL);
    CHECK(io->xDeviceCharacteristics(a) == 0 && io->xSectorSize(a) == 512);
    CHECK(io->xFileControl(a, 9999, NULL) == SQLITE_NOTFOUND);
    OK(io->xWrite(a, "abc", 3, 3));
    memset(buffer, 99, sizeof(buffer));
    CHECK(io->xRead(b, buffer, 10, 0) == SQLITE_IOERR_SHORT_READ);
    CHECK(memcmp(buffer, "\0\0\0abc\0\0\0\0", 10) == 0);
    memset(buffer, 99, sizeof(buffer));
    CHECK(io->xRead(b, buffer, 5, 100) == SQLITE_IOERR_SHORT_READ);
    CHECK(memcmp(buffer, "\0\0\0\0\0", 5) == 0);
    CHECK(io->xRead(b, buffer, 1, -1) == SQLITE_IOERR_READ);
    CHECK(io->xWrite(a, buffer, 1, 9223372036854775807LL) == SQLITE_IOERR_WRITE);
    OK(io->xTruncate(a, 4));
    OK(io->xTruncate(a, 8));
    OK(io->xRead(b, buffer, 8, 0));
    CHECK(memcmp(buffer, "\0\0\0a\0\0\0\0", 8) == 0);
    OK(io->xFileSize(b, &size));
    CHECK(size == 8);
    OK(io->xSync(a, SQLITE_SYNC_FULL));

    OK(io->xLock(a, SQLITE_LOCK_SHARED));
    OK(io->xLock(b, SQLITE_LOCK_SHARED));
    OK(io->xLock(a, SQLITE_LOCK_RESERVED));
    OK(io->xCheckReservedLock(b, &state)); CHECK(state == 1);
    CHECK(io->xLock(b, SQLITE_LOCK_RESERVED) == SQLITE_BUSY);
    CHECK(io->xLock(a, SQLITE_LOCK_EXCLUSIVE) == SQLITE_BUSY);
    OK(io->xFileControl(a, SQLITE_FCNTL_LOCKSTATE, &state));
    CHECK(state == SQLITE_LOCK_PENDING);
    OK(open_file("contract.db", c, rw));
    CHECK(io->xLock(c, SQLITE_LOCK_SHARED) == SQLITE_BUSY);
    OK(io->xUnlock(b, SQLITE_LOCK_NONE));
    OK(io->xLock(a, SQLITE_LOCK_EXCLUSIVE));
    CHECK(io->xLock(b, SQLITE_LOCK_SHARED) == SQLITE_BUSY);
    OK(io->xUnlock(a, SQLITE_LOCK_SHARED));
    OK(io->xCheckReservedLock(b, &state)); CHECK(state == 0);
    OK(io->xLock(b, SQLITE_LOCK_SHARED));
    CHECK(dotcc_memory_vfs_export("contract.db", NULL, 0, &size) == SQLITE_BUSY);
    CHECK(dotcc_memory_vfs_import("contract.db", "x", 1) == SQLITE_BUSY);
    OK(close_file(c));
    c = new_file(); CHECK(c);
    CHECK(open_file("contract.db", c, rw | SQLITE_OPEN_EXCLUSIVE) == SQLITE_CANTOPEN);
    CHECK(c->pMethods == NULL);
    OK(open_file("contract.db", c, SQLITE_OPEN_READONLY | SQLITE_OPEN_MAIN_DB));
    CHECK(io->xWrite(c, "x", 1, 0) == SQLITE_READONLY);
    CHECK(io->xTruncate(c, 0) == SQLITE_READONLY);
    OK(close_file(c));

    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_WRITE, 1, SQLITE_IOERR_WRITE);
    OK(io->xWrite(a, "1", 1, 0));
    CHECK(io->xWrite(a, "2", 1, 0) == SQLITE_IOERR_WRITE);
    OK(io->xRead(a, buffer, 1, 0)); CHECK(buffer[0] == '1');
    OK(io->xWrite(a, "3", 1, 0));
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_READ, 0, SQLITE_IOERR_READ);
    CHECK(io->xRead(a, buffer, 1, 0) == SQLITE_IOERR_READ);
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_TRUNCATE, 0, SQLITE_IOERR_TRUNCATE);
    CHECK(io->xTruncate(a, 0) == SQLITE_IOERR_TRUNCATE);
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_SYNC, 0, SQLITE_IOERR_FSYNC);
    CHECK(io->xSync(a, SQLITE_SYNC_NORMAL) == SQLITE_IOERR_FSYNC);
    OK(close_file(a)); OK(close_file(b));
    OK(dotcc_memory_vfs_export("contract.db", NULL, 0, &size)); CHECK(size == 8);
    CHECK(dotcc_memory_vfs_export("contract.db", buffer, 7, &size) == SQLITE_TOOBIG);
    OK(dotcc_memory_vfs_export("contract.db", buffer, sizeof(buffer), &size));
    OK(dotcc_memory_vfs_import("copied.db", buffer, size));
    OK(vfs->xAccess(vfs, "copied.db", SQLITE_ACCESS_EXISTS, &exists)); CHECK(exists);
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_DELETE, 0, SQLITE_IOERR_DELETE);
    CHECK(vfs->xDelete(vfs, "copied.db", 0) == SQLITE_IOERR_DELETE);
    OK(vfs->xAccess(vfs, "copied.db", SQLITE_ACCESS_EXISTS, &exists)); CHECK(exists);
    OK(vfs->xDelete(vfs, "copied.db", 0));
    CHECK(vfs->xDelete(vfs, "copied.db", 0) == SQLITE_IOERR_DELETE_NOENT);

    a = new_file(); b = new_file(); CHECK(a && b);
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_OPEN, 0, SQLITE_CANTOPEN);
    CHECK(open_file("missing", a, rw) == SQLITE_CANTOPEN && !a->pMethods);
    CHECK(open_file("missing", a, SQLITE_OPEN_READONLY) == SQLITE_CANTOPEN && !a->pMethods);
    OK(open_file(NULL, a, rw | SQLITE_OPEN_DELETEONCLOSE));
    OK(open_file("temporary", b, rw | SQLITE_OPEN_DELETEONCLOSE));
    OK(close_file(a)); OK(close_file(b));
    OK(vfs->xAccess(vfs, "temporary", SQLITE_ACCESS_EXISTS, &exists)); CHECK(!exists);

    /* Unlink while open retains old handle contents, but a fresh open is new. */
    a = new_file(); b = new_file(); CHECK(a && b);
    OK(open_file("unlinked", a, rw)); OK(io->xWrite(a, "old", 3, 0));
    OK(vfs->xDelete(vfs, "unlinked", 0));
    OK(open_file("unlinked", b, rw)); OK(io->xFileSize(b, &size)); CHECK(size == 0);
    OK(io->xRead(a, buffer, 3, 0)); CHECK(memcmp(buffer, "old", 3) == 0);
    OK(close_file(a)); OK(close_file(b));

    dotcc_memory_vfs_seed(123);
    CHECK(vfs->xRandomness(vfs, sizeof(random1), random1) == sizeof(random1));
    dotcc_memory_vfs_seed(123);
    CHECK(vfs->xRandomness(vfs, sizeof(random2), random2) == sizeof(random2));
    CHECK(memcmp(random1, random2, sizeof(random1)) == 0);
    dotcc_memory_vfs_time(0); OK(vfs->xCurrentTime(vfs, &day)); CHECK(day == 2440587.5);
    CHECK(vfs->xSleep(vfs, 1000000) == 1000000);
    OK(vfs->xCurrentTime(vfs, &day)); CHECK(day > 2440587.5 && day < 2440587.50002);
    OK(dotcc_memory_vfs_reset());
    CHECK(dotcc_memory_vfs_file_count() == 0 && dotcc_memory_vfs_handle_count() == 0);
    puts("PASS VFS direct contract");
    return 0;
}

static int scalar(sqlite3 *db, const char *sql, const char *expected) {
    sqlite3_stmt *stmt = NULL;
    OK(sqlite3_prepare_v2(db, sql, -1, &stmt, NULL));
    CHECK(sqlite3_step(stmt) == SQLITE_ROW);
    CHECK(sqlite3_column_text(stmt, 0) != NULL);
    CHECK(strcmp((const char *)sqlite3_column_text(stmt, 0), expected) == 0);
    CHECK(sqlite3_step(stmt) == SQLITE_DONE);
    OK(sqlite3_finalize(stmt));
    return 0;
}

static int sql_contract(void) {
    sqlite3 *a = NULL, *b = NULL;
    void *image;
    sqlite3_int64 size;
    int rw = SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE;
    int rc, exists;
    sqlite3_vfs *vfs = dotcc_memory_vfs();
    OK(sqlite3_open_v2("engine.db", &a, rw, DOTCC_MEMORY_VFS_NAME));
    OK(sqlite3_exec(a, "CREATE TABLE t(id INTEGER PRIMARY KEY, v TEXT); INSERT INTO t VALUES(1,'before');", NULL, NULL, NULL));
    OK(sqlite3_open_v2("./engine.db", &b, rw, DOTCC_MEMORY_VFS_NAME));
    CHECK(scalar(b, "SELECT v FROM t", "before") == 0);
    OK(sqlite3_exec(a, "BEGIN IMMEDIATE; UPDATE t SET v='after';", NULL, NULL, NULL));
    OK(vfs->xAccess(vfs, "engine.db-journal", SQLITE_ACCESS_EXISTS, &exists)); CHECK(exists);
    CHECK(sqlite3_exec(b, "BEGIN IMMEDIATE", NULL, NULL, NULL) == SQLITE_BUSY);
    CHECK(scalar(b, "SELECT v FROM t", "before") == 0);
    OK(sqlite3_exec(a, "ROLLBACK", NULL, NULL, NULL));
    CHECK(scalar(a, "SELECT v FROM t", "before") == 0);
    OK(vfs->xAccess(vfs, "engine.db-journal", SQLITE_ACCESS_EXISTS, &exists)); CHECK(!exists);
    OK(sqlite3_exec(a, "BEGIN; UPDATE t SET v='committed'; SAVEPOINT s; INSERT INTO t VALUES(2,'discarded'); ROLLBACK TO s; RELEASE s; COMMIT;", NULL, NULL, NULL));
    CHECK(scalar(b, "SELECT v FROM t", "committed") == 0);
    CHECK(scalar(b, "SELECT count(*) FROM t", "1") == 0);
    CHECK(scalar(a, "PRAGMA journal_mode=WAL", "delete") == 0);
    CHECK(scalar(a, "SELECT json_extract(jsonb('{\"answer\":42}'),'$.answer')", "42") == 0);
    CHECK(scalar(a, "PRAGMA integrity_check", "ok") == 0);

    /* An injected journal sync failure aborts the transaction. Reopen verifies
     * rollback/recovery left the previous committed image intact. */
    OK(sqlite3_exec(a, "BEGIN IMMEDIATE; UPDATE t SET v='must not commit';", NULL, NULL, NULL));
    dotcc_memory_vfs_fail_after(DOTCC_VFS_FAIL_SYNC, 0, SQLITE_IOERR_FSYNC);
    rc = sqlite3_exec(a, "COMMIT", NULL, NULL, NULL);
    CHECK((rc & 255) == SQLITE_IOERR);
    dotcc_memory_vfs_fail_after(0, 0, SQLITE_OK);
    if (!sqlite3_get_autocommit(a)) OK(sqlite3_exec(a, "ROLLBACK", NULL, NULL, NULL));
    OK(sqlite3_close(a)); OK(sqlite3_close(b));
    CHECK(dotcc_memory_vfs_handle_count() == 0);
    OK(sqlite3_open_v2("engine.db", &a, rw, DOTCC_MEMORY_VFS_NAME));
    CHECK(scalar(a, "SELECT v FROM t", "committed") == 0);
    CHECK(scalar(a, "PRAGMA integrity_check", "ok") == 0);
    OK(sqlite3_close(a));

    OK(dotcc_memory_vfs_export("engine.db", NULL, 0, &size)); CHECK(size > 0);
    image = malloc((size_t)size); CHECK(image);
    OK(dotcc_memory_vfs_export("engine.db", image, size, &size));
    OK(dotcc_memory_vfs_import("reopened.db", image, size));
    free(image);
    OK(sqlite3_open_v2("reopened.db", &a, SQLITE_OPEN_READONLY, DOTCC_MEMORY_VFS_NAME));
    CHECK(scalar(a, "SELECT v FROM t", "committed") == 0);
    CHECK(scalar(a, "PRAGMA integrity_check", "ok") == 0);
    CHECK(sqlite3_exec(a, "DELETE FROM t", NULL, NULL, NULL) == SQLITE_READONLY);
    OK(sqlite3_close(a));
    OK(dotcc_memory_vfs_reset());
    CHECK(dotcc_memory_vfs_file_count() == 0 && dotcc_memory_vfs_handle_count() == 0);
    puts("PASS VFS SQL transactions, recovery, JSONB, image transfer");
    return 0;
}

int main(void) {
    CHECK(direct_contract() == 0);
    OK(sqlite3_initialize());
    CHECK(sqlite3_vfs_find(NULL) == dotcc_memory_vfs());
    CHECK(sql_contract() == 0);
    OK(sqlite3_shutdown());
    puts("PASS all memory VFS contracts");
    return 0;
}
