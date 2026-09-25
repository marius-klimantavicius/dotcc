#include "memory_vfs.h"
#include <stdlib.h>
#include <string.h>

/* Native-only reference VFS for differential tests. Never passed to dotcc.
 * The product and translated harnesses use src/MemoryVfs.cs instead. */
#define MEM_PATH_MAX 1024
#define MEM_I64_MAX 9223372036854775807LL
#ifndef SQLITE_THREADSAFE
#define SQLITE_THREADSAFE 1 /* Match SQLite's default for separate C units. */
#endif

/* VFS2 is reserved for extension VFS state. Allocate/initialize its static
 * mutex before entering it: no SQLite core operation or SQLite allocator is
 * called while the gate is held. Product APPDEF mutexes are recursive; the
 * wrappers do not nest, so native non-recursive static mutexes work too.
 * Standalone THREADSAFE=0 oracle translation units have no mutex symbols.
 */
static int dotcc_mem_enter(sqlite3_mutex **mutex) {
    *mutex = NULL;
#if SQLITE_THREADSAFE
    *mutex = sqlite3_mutex_alloc(SQLITE_MUTEX_STATIC_VFS2);
    if (!*mutex) return SQLITE_NOMEM;
    sqlite3_mutex_enter(*mutex);
#endif
    return SQLITE_OK;
}

static void dotcc_mem_leave(sqlite3_mutex *mutex) {
#if SQLITE_THREADSAFE
    sqlite3_mutex_leave(mutex);
#else
    (void)mutex;
#endif
}

typedef struct DotccMemNode DotccMemNode;
typedef struct DotccMemFile DotccMemFile;
struct DotccMemNode {
    char *name;
    unsigned char *data;
    sqlite3_int64 size;
    size_t capacity;
    int refs;
    int unlinked;
    DotccMemNode *next;
};
struct DotccMemFile {
    sqlite3_file base;
    DotccMemNode *node;
    int flags;
    int lock;
    DotccMemFile *next;
};

static DotccMemNode *dotcc_mem_nodes;
static DotccMemFile *dotcc_mem_handles;
static int dotcc_mem_fail_mask;
static int dotcc_mem_fail_count;
static int dotcc_mem_fail_result;
static unsigned int dotcc_mem_random = 1;
static sqlite3_int64 dotcc_mem_unix_ms = 1735689600000LL; /* 2025-01-01 UTC */

static int dotcc_mem_failure(int operation) {
    if ((dotcc_mem_fail_mask & operation) == 0) return SQLITE_OK;
    if (dotcc_mem_fail_count > 0) {
        --dotcc_mem_fail_count;
        return SQLITE_OK;
    }
    dotcc_mem_fail_mask = 0;
    return dotcc_mem_fail_result;
}

static int dotcc_mem_path(const char *name, int count, char *out) {
    int used = 1;
    const char *start;
    size_t len;
    if (!name || count < 2) return SQLITE_CANTOPEN;
    out[0] = '/';
    while (*name) {
        while (*name == '/') ++name;
        start = name;
        while (*name && *name != '/') ++name;
        len = (size_t)(name - start);
        if (len == 0 || (len == 1 && start[0] == '.')) continue;
        if (len == 2 && start[0] == '.' && start[1] == '.') {
            while (used > 1 && out[used - 1] != '/') --used;
            if (used > 1) --used;
            continue;
        }
        if (len > MEM_PATH_MAX || used + (int)len + (used > 1) >= count)
            return SQLITE_CANTOPEN;
        if (used > 1) out[used++] = '/';
        memcpy(out + used, start, len);
        used += (int)len;
    }
    out[used] = 0;
    return SQLITE_OK;
}

static DotccMemNode *dotcc_mem_find(const char *name) {
    DotccMemNode *node;
    for (node = dotcc_mem_nodes; node; node = node->next)
        if (!node->unlinked && node->name && strcmp(name, node->name) == 0)
            return node;
    return NULL;
}

static DotccMemNode *dotcc_mem_create(const char *name) {
    DotccMemNode *node = (DotccMemNode *)malloc(sizeof(DotccMemNode));
    if (!node) return NULL;
    memset(node, 0, sizeof(DotccMemNode));
    if (name) {
        node->name = (char *)malloc(strlen(name) + 1);
        if (!node->name) { free(node); return NULL; }
        strcpy(node->name, name);
    } else {
        node->unlinked = 1;
    }
    node->next = dotcc_mem_nodes;
    dotcc_mem_nodes = node;
    return node;
}

static void dotcc_mem_destroy(DotccMemNode *node) {
    DotccMemNode **link = &dotcc_mem_nodes;
    while (*link && *link != node) link = &(*link)->next;
    if (*link) *link = node->next;
    free(node->name);
    free(node->data);
    free(node);
}

static int dotcc_mem_resize(DotccMemNode *node, sqlite3_int64 size) {
    unsigned char *data;
    size_t capacity;
    if (size < 0 || (sqlite3_uint64)size > (size_t)-1) return SQLITE_FULL;
    if ((size_t)size > node->capacity) {
        capacity = (size_t)size;
        if (capacity <= ((size_t)-1) - 4095)
            capacity = ((capacity + 4095) / 4096) * 4096;
        data = (unsigned char *)realloc(node->data, capacity);
        if (!data) return SQLITE_NOMEM;
        node->data = data;
        node->capacity = capacity;
    }
    if (size > node->size)
        memset(node->data + (size_t)node->size, 0, (size_t)(size - node->size));
    node->size = size;
    return SQLITE_OK;
}

static int dotcc_mem_close_locked(sqlite3_file *base) {
    DotccMemFile *file = (DotccMemFile *)base;
    DotccMemFile **link = &dotcc_mem_handles;
    DotccMemNode *node = file->node;
    while (*link && *link != file) link = &(*link)->next;
    if (*link) *link = file->next;
    if (file->flags & SQLITE_OPEN_DELETEONCLOSE) node->unlinked = 1;
    --node->refs;
    if (!node->refs && node->unlinked) dotcc_mem_destroy(node);
    base->pMethods = NULL;
    return SQLITE_OK;
}

static int dotcc_mem_close(sqlite3_file *base) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_close_locked(base);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_read_locked(sqlite3_file *base, void *buffer, int count, sqlite3_int64 offset) {
    DotccMemNode *node = ((DotccMemFile *)base)->node;
    sqlite3_int64 available;
    int rc = dotcc_mem_failure(DOTCC_VFS_FAIL_READ);
    if (rc != SQLITE_OK) return rc;
    if (offset < 0 || count < 0) return SQLITE_IOERR_READ;
    if (!count) return SQLITE_OK;
    available = offset < node->size ? node->size - offset : 0;
    if (available > count) available = count;
    if (available) memcpy(buffer, node->data + (size_t)offset, (size_t)available);
    if (available < count) {
        memset((unsigned char *)buffer + (size_t)available, 0, (size_t)(count - available));
        return SQLITE_IOERR_SHORT_READ;
    }
    return SQLITE_OK;
}

static int dotcc_mem_read(sqlite3_file *base, void *buffer, int count, sqlite3_int64 offset) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_read_locked(base, buffer, count, offset);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_write_locked(sqlite3_file *base, const void *buffer, int count, sqlite3_int64 offset) {
    DotccMemFile *file = (DotccMemFile *)base;
    int rc;
    if (file->flags & SQLITE_OPEN_READONLY) return SQLITE_READONLY;
    rc = dotcc_mem_failure(DOTCC_VFS_FAIL_WRITE);
    if (rc != SQLITE_OK) return rc;
    if (offset < 0 || count < 0 || offset > MEM_I64_MAX - count) return SQLITE_IOERR_WRITE;
    if (!count) return SQLITE_OK;
    if (offset + count > file->node->size) {
        rc = dotcc_mem_resize(file->node, offset + count);
        if (rc != SQLITE_OK) return rc;
    }
    memcpy(file->node->data + (size_t)offset, buffer, (size_t)count);
    return SQLITE_OK;
}

static int dotcc_mem_write(sqlite3_file *base, const void *buffer, int count, sqlite3_int64 offset) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_write_locked(base, buffer, count, offset);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_truncate_locked(sqlite3_file *base, sqlite3_int64 size) {
    DotccMemFile *file = (DotccMemFile *)base;
    int rc;
    if (file->flags & SQLITE_OPEN_READONLY) return SQLITE_READONLY;
    rc = dotcc_mem_failure(DOTCC_VFS_FAIL_TRUNCATE);
    if (rc != SQLITE_OK) return rc;
    if (size < 0) return SQLITE_IOERR_TRUNCATE;
    return dotcc_mem_resize(file->node, size);
}

static int dotcc_mem_truncate(sqlite3_file *base, sqlite3_int64 size) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_truncate_locked(base, size);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_sync_locked(sqlite3_file *base, int flags) {
    (void)base; (void)flags;
    return dotcc_mem_failure(DOTCC_VFS_FAIL_SYNC);
}

static int dotcc_mem_sync(sqlite3_file *base, int flags) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_sync_locked(base, flags);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_size_locked(sqlite3_file *base, sqlite3_int64 *size) {
    *size = ((DotccMemFile *)base)->node->size;
    return SQLITE_OK;
}

static int dotcc_mem_size(sqlite3_file *base, sqlite3_int64 *size) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_size_locked(base, size);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_lock_locked(sqlite3_file *base, int level) {
    DotccMemFile *file = (DotccMemFile *)base;
    DotccMemFile *other;
    if (level <= file->lock) return SQLITE_OK;
    if (level < SQLITE_LOCK_SHARED || level > SQLITE_LOCK_EXCLUSIVE) return SQLITE_IOERR_LOCK;
    for (other = dotcc_mem_handles; other; other = other->next) {
        if (other == file || other->node != file->node) continue;
        if ((level == SQLITE_LOCK_SHARED && other->lock >= SQLITE_LOCK_PENDING) ||
            (level >= SQLITE_LOCK_RESERVED && other->lock >= SQLITE_LOCK_RESERVED))
            return SQLITE_BUSY;
    }
    if (level == SQLITE_LOCK_EXCLUSIVE) {
        /* Retain PENDING on contention to prevent starvation from new readers. */
        file->lock = SQLITE_LOCK_PENDING;
        for (other = dotcc_mem_handles; other; other = other->next)
            if (other != file && other->node == file->node && other->lock >= SQLITE_LOCK_SHARED)
                return SQLITE_BUSY;
    }
    file->lock = level;
    return SQLITE_OK;
}

static int dotcc_mem_lock(sqlite3_file *base, int level) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_lock_locked(base, level);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_unlock_locked(sqlite3_file *base, int level) {
    DotccMemFile *file = (DotccMemFile *)base;
    if (level != SQLITE_LOCK_NONE && level != SQLITE_LOCK_SHARED) return SQLITE_IOERR_UNLOCK;
    if (level < file->lock) file->lock = level;
    return SQLITE_OK;
}

static int dotcc_mem_unlock(sqlite3_file *base, int level) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_unlock_locked(base, level);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_reserved_locked(sqlite3_file *base, int *result) {
    DotccMemFile *file = (DotccMemFile *)base;
    DotccMemFile *other;
    *result = 0;
    for (other = dotcc_mem_handles; other; other = other->next)
        if (other->node == file->node && other->lock >= SQLITE_LOCK_RESERVED) *result = 1;
    return SQLITE_OK;
}

static int dotcc_mem_reserved(sqlite3_file *base, int *result) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_reserved_locked(base, result);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_control_locked(sqlite3_file *base, int operation, void *argument) {
    if (operation == SQLITE_FCNTL_LOCKSTATE) {
        *(int *)argument = ((DotccMemFile *)base)->lock;
        return SQLITE_OK;
    }
    return SQLITE_NOTFOUND;
}

static int dotcc_mem_control(sqlite3_file *base, int operation, void *argument) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_control_locked(base, operation, argument);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_sector_locked(sqlite3_file *base) { (void)base; return 512; }

static int dotcc_mem_sector(sqlite3_file *base) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return 0;
    rc = dotcc_mem_sector_locked(base);
    dotcc_mem_leave(mutex);
    return rc;
}
static int dotcc_mem_device_locked(sqlite3_file *base) { (void)base; return 0; }

static int dotcc_mem_device(sqlite3_file *base) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return 0;
    rc = dotcc_mem_device_locked(base);
    dotcc_mem_leave(mutex);
    return rc;
}

static const sqlite3_io_methods dotcc_mem_methods = {
    1, dotcc_mem_close, dotcc_mem_read, dotcc_mem_write, dotcc_mem_truncate, dotcc_mem_sync, dotcc_mem_size,
    dotcc_mem_lock, dotcc_mem_unlock, dotcc_mem_reserved, dotcc_mem_control, dotcc_mem_sector, dotcc_mem_device,
    NULL, NULL, NULL, NULL, NULL, NULL
};

static int dotcc_mem_open_locked(sqlite3_vfs *vfs, const char *name, sqlite3_file *base,
                    int flags, int *out_flags) {
    DotccMemFile *file = (DotccMemFile *)base;
    DotccMemNode *node;
    char path[MEM_PATH_MAX + 1];
    int rc;
    (void)vfs;
    memset(file, 0, sizeof(DotccMemFile));
    rc = dotcc_mem_failure(DOTCC_VFS_FAIL_OPEN);
    if (rc != SQLITE_OK) return rc;
    if (!(flags & (SQLITE_OPEN_READONLY | SQLITE_OPEN_READWRITE)) ||
        ((flags & SQLITE_OPEN_READONLY) && (flags & SQLITE_OPEN_READWRITE)) ||
        ((flags & SQLITE_OPEN_CREATE) && !(flags & SQLITE_OPEN_READWRITE)))
        return SQLITE_CANTOPEN;
    if (name) {
        rc = dotcc_mem_path(name, sizeof(path), path);
        if (rc != SQLITE_OK) return rc;
        node = dotcc_mem_find(path);
        if (node && (flags & SQLITE_OPEN_EXCLUSIVE) && (flags & SQLITE_OPEN_CREATE))
            return SQLITE_CANTOPEN;
        if (!node && !(flags & SQLITE_OPEN_CREATE)) return SQLITE_CANTOPEN;
        if (!node) node = dotcc_mem_create(path);
    } else {
        if (!(flags & SQLITE_OPEN_DELETEONCLOSE)) return SQLITE_CANTOPEN;
        node = dotcc_mem_create(NULL);
    }
    if (!node) return SQLITE_NOMEM;
    file->node = node;
    file->flags = flags;
    file->next = dotcc_mem_handles;
    dotcc_mem_handles = file;
    ++node->refs;
    base->pMethods = &dotcc_mem_methods;
    if (out_flags) *out_flags = flags;
    return SQLITE_OK;
}

static int dotcc_mem_open(sqlite3_vfs *vfs, const char *name, sqlite3_file *base,
                    int flags, int *out_flags) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_open_locked(vfs, name, base, flags, out_flags);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_delete_locked(sqlite3_vfs *vfs, const char *name, int sync_dir) {
    char path[MEM_PATH_MAX + 1];
    DotccMemNode *node;
    int rc = dotcc_mem_failure(DOTCC_VFS_FAIL_DELETE);
    (void)vfs; (void)sync_dir;
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = dotcc_mem_find(path);
    if (!node) return SQLITE_IOERR_DELETE_NOENT;
    node->unlinked = 1;
    if (!node->refs) dotcc_mem_destroy(node);
    return SQLITE_OK;
}

static int dotcc_mem_delete(sqlite3_vfs *vfs, const char *name, int sync_dir) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_delete_locked(vfs, name, sync_dir);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_access_locked(sqlite3_vfs *vfs, const char *name, int flags, int *result) {
    char path[MEM_PATH_MAX + 1];
    int rc;
    (void)vfs;
    *result = 0;
    if (flags != SQLITE_ACCESS_EXISTS && flags != SQLITE_ACCESS_READ && flags != SQLITE_ACCESS_READWRITE)
        return SQLITE_IOERR_ACCESS;
    rc = dotcc_mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    *result = dotcc_mem_find(path) != NULL;
    return SQLITE_OK;
}

static int dotcc_mem_access(sqlite3_vfs *vfs, const char *name, int flags, int *result) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_access_locked(vfs, name, flags, result);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_fullpath_locked(sqlite3_vfs *vfs, const char *name, int count, char *out) {
    (void)vfs;
    return dotcc_mem_path(name, count, out);
}

static int dotcc_mem_fullpath(sqlite3_vfs *vfs, const char *name, int count, char *out) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_fullpath_locked(vfs, name, count, out);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_randomness_locked(sqlite3_vfs *vfs, int count, char *out) {
    int i;
    (void)vfs;
    for (i = 0; i < count; ++i) {
        dotcc_mem_random = dotcc_mem_random * 1664525U + 1013904223U;
        out[i] = (char)(dotcc_mem_random >> 24);
    }
    return count;
}

static int dotcc_mem_randomness(sqlite3_vfs *vfs, int count, char *out) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return 0;
    rc = dotcc_mem_randomness_locked(vfs, count, out);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_sleep_locked(sqlite3_vfs *vfs, int microseconds) {
    (void)vfs;
    if (microseconds > 0) dotcc_mem_unix_ms += microseconds / 1000;
    return microseconds > 0 ? microseconds : 0;
}

static int dotcc_mem_sleep(sqlite3_vfs *vfs, int microseconds) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return 0;
    rc = dotcc_mem_sleep_locked(vfs, microseconds);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_time_locked(sqlite3_vfs *vfs, double *julian_day) {
    (void)vfs;
    *julian_day = 2440587.5 + (double)dotcc_mem_unix_ms / 86400000.0;
    return SQLITE_OK;
}

static int dotcc_mem_time(sqlite3_vfs *vfs, double *julian_day) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_mem_time_locked(vfs, julian_day);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_mem_error_locked(sqlite3_vfs *vfs, int count, char *out) {
    (void)vfs;
    if (count > 0) out[0] = 0;
    return 0;
}

static int dotcc_mem_error(sqlite3_vfs *vfs, int count, char *out) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return 0;
    rc = dotcc_mem_error_locked(vfs, count, out);
    dotcc_mem_leave(mutex);
    return rc;
}

static sqlite3_vfs dotcc_mem_vfs = {
    1, sizeof(DotccMemFile), MEM_PATH_MAX, NULL, DOTCC_MEMORY_VFS_NAME, NULL,
    dotcc_mem_open, dotcc_mem_delete, dotcc_mem_access, dotcc_mem_fullpath,
    NULL, NULL, NULL, NULL, /* Dynamic loading is deliberately unsupported. */
    dotcc_mem_randomness, dotcc_mem_sleep, dotcc_mem_time, dotcc_mem_error,
    NULL, NULL, NULL, NULL
};

sqlite3_vfs *dotcc_memory_vfs(void) { return &dotcc_mem_vfs; }
#ifdef DOTCC_HOST_VFS
/* Supplied by the explicitly compiled managed VFS sidecar. Native/differential
 * fixtures retain the deterministic memory default without this definition. */
int dotcc_host_vfs_init(void);
int dotcc_host_vfs_end(void);
int sqlite3_os_init(void) {
    int rc = sqlite3_vfs_register(&dotcc_mem_vfs, 0);
    return rc == SQLITE_OK ? dotcc_host_vfs_init() : rc;
}
int sqlite3_os_end(void) {
    int rc = dotcc_host_vfs_end();
    return rc == SQLITE_OK ? sqlite3_vfs_unregister(&dotcc_mem_vfs) : rc;
}
#else
int sqlite3_os_init(void) { return sqlite3_vfs_register(&dotcc_mem_vfs, 1); }
int sqlite3_os_end(void) { return sqlite3_vfs_unregister(&dotcc_mem_vfs); }
#endif

static int dotcc_memory_vfs_reset_locked(void) {
    if (dotcc_mem_handles) return SQLITE_BUSY;
    while (dotcc_mem_nodes) dotcc_mem_destroy(dotcc_mem_nodes);
    dotcc_mem_fail_mask = 0;
    dotcc_mem_random = 1;
    dotcc_mem_unix_ms = 1735689600000LL;
    return SQLITE_OK;
}

int dotcc_memory_vfs_reset(void) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_memory_vfs_reset_locked();
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_memory_vfs_file_count_locked(void) {
    DotccMemNode *node;
    int count = 0;
    for (node = dotcc_mem_nodes; node; node = node->next) ++count;
    return count;
}

int dotcc_memory_vfs_file_count(void) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return -1;
    rc = dotcc_memory_vfs_file_count_locked();
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_memory_vfs_handle_count_locked(void) {
    DotccMemFile *file;
    int count = 0;
    for (file = dotcc_mem_handles; file; file = file->next) ++count;
    return count;
}

int dotcc_memory_vfs_handle_count(void) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return -1;
    rc = dotcc_memory_vfs_handle_count_locked();
    dotcc_mem_leave(mutex);
    return rc;
}

static void dotcc_memory_vfs_fail_after_locked(int mask, int successful_calls, int result) {
    dotcc_mem_fail_mask = mask;
    dotcc_mem_fail_count = successful_calls > 0 ? successful_calls : 0;
    dotcc_mem_fail_result = result == SQLITE_OK ? SQLITE_IOERR : result;
}

void dotcc_memory_vfs_fail_after(int mask, int successful_calls, int result) {
    sqlite3_mutex *mutex;
    if (dotcc_mem_enter(&mutex) != SQLITE_OK) return;
    dotcc_memory_vfs_fail_after_locked(mask, successful_calls, result);
    dotcc_mem_leave(mutex);
}

static void dotcc_memory_vfs_seed_locked(unsigned int seed) { dotcc_mem_random = seed; }

void dotcc_memory_vfs_seed(unsigned int seed) {
    sqlite3_mutex *mutex;
    if (dotcc_mem_enter(&mutex) != SQLITE_OK) return;
    dotcc_memory_vfs_seed_locked(seed);
    dotcc_mem_leave(mutex);
}
static void dotcc_memory_vfs_time_locked(sqlite3_int64 unix_milliseconds) { dotcc_mem_unix_ms = unix_milliseconds; }

void dotcc_memory_vfs_time(sqlite3_int64 unix_milliseconds) {
    sqlite3_mutex *mutex;
    if (dotcc_mem_enter(&mutex) != SQLITE_OK) return;
    dotcc_memory_vfs_time_locked(unix_milliseconds);
    dotcc_mem_leave(mutex);
}

static int dotcc_memory_vfs_export_locked(const char *name, void *buffer,
                            sqlite3_int64 capacity, sqlite3_int64 *size) {
    char path[MEM_PATH_MAX + 1];
    DotccMemNode *node;
    int rc;
    if (!size || capacity < 0) return SQLITE_MISUSE;
    rc = dotcc_mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = dotcc_mem_find(path);
    if (!node) return SQLITE_NOTFOUND;
    if (node->refs) return SQLITE_BUSY;
    *size = node->size;
    if (!buffer) return SQLITE_OK;
    if (capacity < node->size) return SQLITE_TOOBIG;
    if (node->size) memcpy(buffer, node->data, (size_t)node->size);
    return SQLITE_OK;
}

int dotcc_memory_vfs_export(const char *name, void *buffer,
                            sqlite3_int64 capacity, sqlite3_int64 *size) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_memory_vfs_export_locked(name, buffer, capacity, size);
    dotcc_mem_leave(mutex);
    return rc;
}

static int dotcc_memory_vfs_import_locked(const char *name, const void *buffer, sqlite3_int64 size) {
    char path[MEM_PATH_MAX + 1];
    DotccMemNode *node;
    int created = 0;
    int rc;
    if (size < 0 || (!buffer && size)) return SQLITE_MISUSE;
    rc = dotcc_mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = dotcc_mem_find(path);
    if (node && node->refs) return SQLITE_BUSY;
    if (!node) { node = dotcc_mem_create(path); created = 1; }
    if (!node) return SQLITE_NOMEM;
    rc = dotcc_mem_resize(node, size);
    if (rc != SQLITE_OK) {
        if (created) dotcc_mem_destroy(node);
        return rc;
    }
    if (size) memcpy(node->data, buffer, (size_t)size);
    return SQLITE_OK;
}

int dotcc_memory_vfs_import(const char *name, const void *buffer, sqlite3_int64 size) {
    sqlite3_mutex *mutex;
    int rc = dotcc_mem_enter(&mutex);
    if (rc != SQLITE_OK) return rc;
    rc = dotcc_memory_vfs_import_locked(name, buffer, size);
    dotcc_mem_leave(mutex);
    return rc;
}
