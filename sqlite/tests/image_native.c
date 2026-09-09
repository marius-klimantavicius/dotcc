/* Image exchange driver. Host files only transport closed VFS images. */
#include "memory_vfs.h"
#include <stdio.h>
#include <string.h>

#define CHECK(x) do { if (!(x)) { printf("FAIL image line %d: %s\n", __LINE__, #x); return 1; } } while (0)
#define OK(x) CHECK((x) == SQLITE_OK)

static int verify_image(sqlite3 *db) {
    sqlite3_stmt *statement = NULL;
    const char *sql = "SELECT id,json(payload),length(bytes),hex(substr(bytes,32767,6)),typeof(payload) FROM data ORDER BY id";
    OK(sqlite3_prepare_v2(db, "PRAGMA integrity_check", -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 0), "ok") == 0);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    OK(sqlite3_prepare_v2(db, sql, -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW);
    CHECK(sqlite3_column_int(statement, 0) == 1);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 1), "{\"n\":[1,2,3],\"text\":\"Ω\"}") == 0);
    CHECK(sqlite3_column_int(statement, 2) == 65536);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 3), "00007F80FF00") == 0);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 4), "blob") == 0);
    CHECK(sqlite3_step(statement) == SQLITE_ROW);
    CHECK(sqlite3_column_int(statement, 0) == 2);
    CHECK(strcmp((const char *)sqlite3_column_text(statement, 1), "[null,true,\"a\\u0000b\"]") == 0);
    CHECK(sqlite3_column_int(statement, 2) == 3);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    OK(sqlite3_prepare_v2(db, "PRAGMA user_version", -1, &statement, NULL));
    CHECK(sqlite3_step(statement) == SQLITE_ROW && sqlite3_column_int(statement, 0) == 35004);
    CHECK(sqlite3_step(statement) == SQLITE_DONE);
    OK(sqlite3_finalize(statement));
    return 0;
}

int main(int argc, char **argv) {
    sqlite3 *db = NULL;
    sqlite3_blob *blob = NULL;
    sqlite3_int64 size = 0;
    void *image;
    FILE *file;
    unsigned char marker[4] = {0, 127, 128, 255};
    int write_mode;
    CHECK(argc == 3);
    write_mode = strcmp(argv[1], "write") == 0;
    CHECK(write_mode || strcmp(argv[1], "read") == 0);
    OK(sqlite3_initialize());
    if (!write_mode) {
        file = fopen(argv[2], "rb");
        CHECK(file != NULL);
        CHECK(fseek(file, 0, SEEK_END) == 0);
        size = ftell(file);
        CHECK(size > 0 && size < 16 * 1024 * 1024);
        CHECK(fseek(file, 0, SEEK_SET) == 0);
        image = sqlite3_malloc64((sqlite3_uint64)size);
        CHECK(image != NULL);
        CHECK(fread(image, 1, (size_t)size, file) == (size_t)size);
        CHECK(fclose(file) == 0);
        OK(dotcc_memory_vfs_import("exchange.db", image, size));
        sqlite3_free(image);
    }
    OK(sqlite3_open("exchange.db", &db));
    if (write_mode) {
        OK(sqlite3_exec(db,
            "PRAGMA page_size=4096; PRAGMA user_version=35004;"
            "CREATE TABLE data(id INTEGER PRIMARY KEY,payload BLOB,bytes BLOB);"
            "INSERT INTO data VALUES(1,jsonb('{\"n\":[1,2,3],\"text\":\"Ω\"}'),zeroblob(65536));"
            "INSERT INTO data VALUES(2,jsonb('[null,true,\"a\\u0000b\"]'),x'007fff');",
            NULL, NULL, NULL));
        OK(sqlite3_blob_open(db, "main", "data", "bytes", 1, 1, &blob));
        OK(sqlite3_blob_write(blob, marker, 4, 32767));
        OK(sqlite3_blob_close(blob));
    }
    CHECK(verify_image(db) == 0);
    OK(sqlite3_close(db));
    if (write_mode) {
        OK(dotcc_memory_vfs_export("exchange.db", NULL, 0, &size));
        image = sqlite3_malloc64((sqlite3_uint64)size);
        CHECK(image != NULL);
        OK(dotcc_memory_vfs_export("exchange.db", image, size, &size));
        file = fopen(argv[2], "wb");
        CHECK(file != NULL);
        CHECK(fwrite(image, 1, (size_t)size, file) == (size_t)size);
        CHECK(fclose(file) == 0);
        sqlite3_free(image);
    }
    CHECK(dotcc_memory_vfs_handle_count() == 0);
    OK(dotcc_memory_vfs_reset());
    OK(sqlite3_shutdown());
    printf("PASS image %s: integrity, JSONB, Unicode/NUL, 64KiB blob, metadata, cleanup\n", argv[1]);
    return 0;
}
