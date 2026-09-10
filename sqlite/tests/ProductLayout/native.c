/* Layout-only native oracle. Compile the exact adapted product C/profile, but
 * never initialize or execute SQLite. These aborting link stubs are not an OS
 * VFS or a mutex implementation and provide no runtime-safety evidence.
 */
#define main product_layout_common
#include "layout_probe.c"
#undef main

#if defined(SQLITE_MUTEX_APPDEF)
SQLITE_PRIVATE sqlite3_mutex_methods const *sqlite3DefaultMutex(void) { abort(); }
SQLITE_PRIVATE void sqlite3MemoryBarrier(void) { abort(); }
#endif
#ifdef DOTCC_HOST_VFS
int dotcc_host_vfs_init(void) { abort(); }
int dotcc_host_vfs_end(void) { abort(); }
#endif

int main(void) {
    int rc = product_layout_common();
    if (rc) return rc;
    HEADER(sqlite3); FIELD(sqlite3, mutex);
    HEADER(BtShared); FIELD(BtShared, mutex);
    HEADER(Pager); FIELD(Pager, nMmapOut); FIELD(Pager, szMmap); FIELD(Pager, pMmapFreelist);
    HEADER(Wal); FIELD(Wal, pDbFd); FIELD(Wal, apWiData);
    HEADER(sqlite3_mutex_methods); FIELD(sqlite3_mutex_methods, xMutexAlloc);
    return 0;
}
