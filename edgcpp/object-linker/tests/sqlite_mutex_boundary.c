#include "sqlite3.h"

extern const sqlite3_mutex_methods *edg_sqlite_mutex_methods(void);
extern void edg_sqlite_memory_barrier(void);
extern int edg_sqlite_database_mmap_supported(void);

/* Exercises the actual public SQLite header and callback ABI independently of
   the engine. Full engine/threading acceptance remains a separate gate. */
int main(void) {
    const sqlite3_mutex_methods *methods = edg_sqlite_mutex_methods();
    if (!methods || methods != edg_sqlite_mutex_methods()) return 1;
    if (methods->xMutexInit() != SQLITE_OK) return 2;
    sqlite3_mutex *mutex = methods->xMutexAlloc(SQLITE_MUTEX_RECURSIVE);
    if (!mutex) return 3;
    methods->xMutexEnter(mutex);
    methods->xMutexEnter(mutex);
    if (!methods->xMutexHeld(mutex) || methods->xMutexNotheld(mutex)) return 4;
    if (methods->xMutexTry(mutex) != SQLITE_OK) return 5;
    methods->xMutexLeave(mutex);
    methods->xMutexLeave(mutex);
    methods->xMutexLeave(mutex);
    if (methods->xMutexHeld(mutex) || !methods->xMutexNotheld(mutex)) return 6;
    methods->xMutexFree(mutex);
    sqlite3_mutex *identity = methods->xMutexAlloc(SQLITE_MUTEX_STATIC_MAIN);
    if (!identity || methods->xMutexEnd() != SQLITE_OK || methods->xMutexInit() != SQLITE_OK) return 7;
    if (identity != methods->xMutexAlloc(SQLITE_MUTEX_STATIC_MAIN)) return 8;
    edg_sqlite_memory_barrier();
    int mmap_supported = edg_sqlite_database_mmap_supported();
    if (mmap_supported != 0 && mmap_supported != 1) return 9;
    return methods->xMutexEnd();
}
