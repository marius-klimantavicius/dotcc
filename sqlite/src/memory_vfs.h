#ifndef DOTCC_MEMORY_VFS_H
#define DOTCC_MEMORY_VFS_H

#include "sqlite3.h"

#define DOTCC_MEMORY_VFS_NAME "dotcc-memory"
#define DOTCC_VFS_FAIL_OPEN 1
#define DOTCC_VFS_FAIL_READ 2
#define DOTCC_VFS_FAIL_WRITE 4
#define DOTCC_VFS_FAIL_TRUNCATE 8
#define DOTCC_VFS_FAIL_SYNC 16
#define DOTCC_VFS_FAIL_DELETE 32

/* Serialized, process-local calls only. Files survive close until deleted/reset.
 * No OS files, native interop, shared-memory/WAL, mmap, or durability guarantee.
 * sqlite3_os_init registers this VFS as the default for SQLITE_OS_OTHER=1.
 */
sqlite3_vfs *dotcc_memory_vfs(void);
int dotcc_memory_vfs_reset(void); /* SQLITE_BUSY while any file is open. */
int dotcc_memory_vfs_file_count(void);
int dotcc_memory_vfs_handle_count(void);

/* One-shot error after successful_calls matching operations. mask=0 disables.
 * Failure is injected before mutation. No partial-write/power-loss simulation.
 */
void dotcc_memory_vfs_fail_after(int mask, int successful_calls, int result);
void dotcc_memory_vfs_seed(unsigned int seed);
void dotcc_memory_vfs_time(sqlite3_int64 unix_milliseconds);

/* Copy database images only when that file has no open handles (SQLITE_BUSY).
 * Export sets *size even if buffer is NULL. Too small a buffer: SQLITE_TOOBIG.
 * Import replaces the named image. Caller retains ownership of input/output.
 */
int dotcc_memory_vfs_export(const char *name, void *buffer,
                            sqlite3_int64 capacity, sqlite3_int64 *size);
int dotcc_memory_vfs_import(const char *name, const void *buffer,
                            sqlite3_int64 size);

#endif
