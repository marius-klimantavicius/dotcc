/* Separate-process native oracle for real file locking and hot-journal recovery. */
#include "sqlite3.h"
#include <stdio.h>
#include <string.h>

int main(int argc, char **argv) {
    sqlite3 *db = NULL;
    char command[262144];
    int rc;
    if (argc < 2) return 2;
    rc = sqlite3_open_v2(argv[1], &db, argc > 2 ? SQLITE_OPEN_READONLY : SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, NULL);
    if (rc != SQLITE_OK) { fprintf(stderr, "native open: %d %s\n", rc, sqlite3_errmsg(db)); return 1; }
    setvbuf(stdout, NULL, _IONBF, 0);
    puts("READY");
    while (fgets(command, sizeof(command), stdin)) {
        command[strcspn(command, "\r\n")] = 0;
        if (strcmp(command, "X") == 0) break;
        if (strncmp(command, "E ", 2) == 0) {
            rc = sqlite3_exec(db, command + 2, NULL, NULL, NULL);
            printf("RC %d\n", rc);
        } else if (strncmp(command, "Q ", 2) == 0) {
            sqlite3_stmt *statement = NULL;
            rc = sqlite3_prepare_v2(db, command + 2, -1, &statement, NULL);
            if (rc == SQLITE_OK) rc = sqlite3_step(statement);
            if (rc != SQLITE_ROW) { fprintf(stderr, "native query: %d %s\n", rc, sqlite3_errmsg(db)); return 1; }
            {
                const unsigned char *text = sqlite3_column_text(statement, 0);
                printf("ROW %s\n", text ? (const char *)text : "NULL");
            }
            if (sqlite3_step(statement) != SQLITE_DONE || sqlite3_finalize(statement) != SQLITE_OK) return 1;
        } else return 2;
    }
    return sqlite3_close(db) == SQLITE_OK ? 0 : 1;
}
