/* Platform port for the generated APPDEF amalgamation. Its one guarded change
 * leaves these two default mutex/barrier hooks to the application. Native
 * reference corpora retain their separate THREADSAFE=0 configuration. */
#if defined(SQLITE_MUTEX_APPDEF) && SQLITE_THREADSAFE
#ifndef DOTCC_HOST_VFS
#error SQLITE_MUTEX_APPDEF requires the managed host platform adapter
#endif
sqlite3_mutex_methods *dotcc_host_mutex_methods(void);
void dotcc_host_memory_barrier(void);
SQLITE_PRIVATE sqlite3_mutex_methods const *sqlite3DefaultMutex(void) {
    return dotcc_host_mutex_methods();
}
SQLITE_PRIVATE void sqlite3MemoryBarrier(void) {
    dotcc_host_memory_barrier();
}
#endif
