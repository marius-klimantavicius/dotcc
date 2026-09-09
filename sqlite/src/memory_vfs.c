#include "memory_vfs.h"
#include <stdlib.h>
#include <string.h>

/* Intentionally portable C: the same adapter is compiled with the native oracle
 * and translated with SQLite. Allocation/memory calls use dotcc's libc port. */
#define MEM_PATH_MAX 1024
#define MEM_I64_MAX 9223372036854775807LL

typedef struct MemNode MemNode;
typedef struct MemFile MemFile;
struct MemNode {
    char *name;
    unsigned char *data;
    sqlite3_int64 size;
    size_t capacity;
    int refs;
    int unlinked;
    MemNode *next;
};
struct MemFile {
    sqlite3_file base;
    MemNode *node;
    int flags;
    int lock;
    MemFile *next;
};

static MemNode *mem_nodes;
static MemFile *mem_handles;
static int mem_fail_mask;
static int mem_fail_count;
static int mem_fail_result;
static unsigned int mem_random = 1;
static sqlite3_int64 mem_unix_ms = 1735689600000LL; /* 2025-01-01 UTC */

static int mem_failure(int operation) {
    if ((mem_fail_mask & operation) == 0) return SQLITE_OK;
    if (mem_fail_count > 0) {
        --mem_fail_count;
        return SQLITE_OK;
    }
    mem_fail_mask = 0;
    return mem_fail_result;
}

static int mem_path(const char *name, int count, char *out) {
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

static MemNode *mem_find(const char *name) {
    MemNode *node;
    for (node = mem_nodes; node; node = node->next)
        if (!node->unlinked && node->name && strcmp(name, node->name) == 0)
            return node;
    return NULL;
}

static MemNode *mem_create(const char *name) {
    MemNode *node = (MemNode *)malloc(sizeof(MemNode));
    if (!node) return NULL;
    memset(node, 0, sizeof(MemNode));
    if (name) {
        node->name = (char *)malloc(strlen(name) + 1);
        if (!node->name) { free(node); return NULL; }
        strcpy(node->name, name);
    } else {
        node->unlinked = 1;
    }
    node->next = mem_nodes;
    mem_nodes = node;
    return node;
}

static void mem_destroy(MemNode *node) {
    MemNode **link = &mem_nodes;
    while (*link && *link != node) link = &(*link)->next;
    if (*link) *link = node->next;
    free(node->name);
    free(node->data);
    free(node);
}

static int mem_resize(MemNode *node, sqlite3_int64 size) {
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

static int mem_close(sqlite3_file *base) {
    MemFile *file = (MemFile *)base;
    MemFile **link = &mem_handles;
    MemNode *node = file->node;
    while (*link && *link != file) link = &(*link)->next;
    if (*link) *link = file->next;
    if (file->flags & SQLITE_OPEN_DELETEONCLOSE) node->unlinked = 1;
    --node->refs;
    if (!node->refs && node->unlinked) mem_destroy(node);
    base->pMethods = NULL;
    return SQLITE_OK;
}

static int mem_read(sqlite3_file *base, void *buffer, int count, sqlite3_int64 offset) {
    MemNode *node = ((MemFile *)base)->node;
    sqlite3_int64 available;
    int rc = mem_failure(DOTCC_VFS_FAIL_READ);
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

static int mem_write(sqlite3_file *base, const void *buffer, int count, sqlite3_int64 offset) {
    MemFile *file = (MemFile *)base;
    int rc;
    if (file->flags & SQLITE_OPEN_READONLY) return SQLITE_READONLY;
    rc = mem_failure(DOTCC_VFS_FAIL_WRITE);
    if (rc != SQLITE_OK) return rc;
    if (offset < 0 || count < 0 || offset > MEM_I64_MAX - count) return SQLITE_IOERR_WRITE;
    if (!count) return SQLITE_OK;
    if (offset + count > file->node->size) {
        rc = mem_resize(file->node, offset + count);
        if (rc != SQLITE_OK) return rc;
    }
    memcpy(file->node->data + (size_t)offset, buffer, (size_t)count);
    return SQLITE_OK;
}

static int mem_truncate(sqlite3_file *base, sqlite3_int64 size) {
    MemFile *file = (MemFile *)base;
    int rc;
    if (file->flags & SQLITE_OPEN_READONLY) return SQLITE_READONLY;
    rc = mem_failure(DOTCC_VFS_FAIL_TRUNCATE);
    if (rc != SQLITE_OK) return rc;
    if (size < 0) return SQLITE_IOERR_TRUNCATE;
    return mem_resize(file->node, size);
}

static int mem_sync(sqlite3_file *base, int flags) {
    (void)base; (void)flags;
    return mem_failure(DOTCC_VFS_FAIL_SYNC);
}

static int mem_size(sqlite3_file *base, sqlite3_int64 *size) {
    *size = ((MemFile *)base)->node->size;
    return SQLITE_OK;
}

static int mem_lock(sqlite3_file *base, int level) {
    MemFile *file = (MemFile *)base;
    MemFile *other;
    if (level <= file->lock) return SQLITE_OK;
    if (level < SQLITE_LOCK_SHARED || level > SQLITE_LOCK_EXCLUSIVE) return SQLITE_IOERR_LOCK;
    for (other = mem_handles; other; other = other->next) {
        if (other == file || other->node != file->node) continue;
        if ((level == SQLITE_LOCK_SHARED && other->lock >= SQLITE_LOCK_PENDING) ||
            (level >= SQLITE_LOCK_RESERVED && other->lock >= SQLITE_LOCK_RESERVED))
            return SQLITE_BUSY;
    }
    if (level == SQLITE_LOCK_EXCLUSIVE) {
        /* Retain PENDING on contention to prevent starvation from new readers. */
        file->lock = SQLITE_LOCK_PENDING;
        for (other = mem_handles; other; other = other->next)
            if (other != file && other->node == file->node && other->lock >= SQLITE_LOCK_SHARED)
                return SQLITE_BUSY;
    }
    file->lock = level;
    return SQLITE_OK;
}

static int mem_unlock(sqlite3_file *base, int level) {
    MemFile *file = (MemFile *)base;
    if (level != SQLITE_LOCK_NONE && level != SQLITE_LOCK_SHARED) return SQLITE_IOERR_UNLOCK;
    if (level < file->lock) file->lock = level;
    return SQLITE_OK;
}

static int mem_reserved(sqlite3_file *base, int *result) {
    MemFile *file = (MemFile *)base;
    MemFile *other;
    *result = 0;
    for (other = mem_handles; other; other = other->next)
        if (other->node == file->node && other->lock >= SQLITE_LOCK_RESERVED) *result = 1;
    return SQLITE_OK;
}

static int mem_control(sqlite3_file *base, int operation, void *argument) {
    if (operation == SQLITE_FCNTL_LOCKSTATE) {
        *(int *)argument = ((MemFile *)base)->lock;
        return SQLITE_OK;
    }
    return SQLITE_NOTFOUND;
}

static int mem_sector(sqlite3_file *base) { (void)base; return 512; }
static int mem_device(sqlite3_file *base) { (void)base; return 0; }

static const sqlite3_io_methods mem_methods = {
    1, mem_close, mem_read, mem_write, mem_truncate, mem_sync, mem_size,
    mem_lock, mem_unlock, mem_reserved, mem_control, mem_sector, mem_device,
    NULL, NULL, NULL, NULL, NULL, NULL
};

static int mem_open(sqlite3_vfs *vfs, const char *name, sqlite3_file *base,
                    int flags, int *out_flags) {
    MemFile *file = (MemFile *)base;
    MemNode *node;
    char path[MEM_PATH_MAX + 1];
    int rc;
    (void)vfs;
    memset(file, 0, sizeof(MemFile));
    rc = mem_failure(DOTCC_VFS_FAIL_OPEN);
    if (rc != SQLITE_OK) return rc;
    if (!(flags & (SQLITE_OPEN_READONLY | SQLITE_OPEN_READWRITE)) ||
        ((flags & SQLITE_OPEN_READONLY) && (flags & SQLITE_OPEN_READWRITE)) ||
        ((flags & SQLITE_OPEN_CREATE) && !(flags & SQLITE_OPEN_READWRITE)))
        return SQLITE_CANTOPEN;
    if (name) {
        rc = mem_path(name, sizeof(path), path);
        if (rc != SQLITE_OK) return rc;
        node = mem_find(path);
        if (node && (flags & SQLITE_OPEN_EXCLUSIVE) && (flags & SQLITE_OPEN_CREATE))
            return SQLITE_CANTOPEN;
        if (!node && !(flags & SQLITE_OPEN_CREATE)) return SQLITE_CANTOPEN;
        if (!node) node = mem_create(path);
    } else {
        if (!(flags & SQLITE_OPEN_DELETEONCLOSE)) return SQLITE_CANTOPEN;
        node = mem_create(NULL);
    }
    if (!node) return SQLITE_NOMEM;
    file->node = node;
    file->flags = flags;
    file->next = mem_handles;
    mem_handles = file;
    ++node->refs;
    base->pMethods = &mem_methods;
    if (out_flags) *out_flags = flags;
    return SQLITE_OK;
}

static int mem_delete(sqlite3_vfs *vfs, const char *name, int sync_dir) {
    char path[MEM_PATH_MAX + 1];
    MemNode *node;
    int rc = mem_failure(DOTCC_VFS_FAIL_DELETE);
    (void)vfs; (void)sync_dir;
    if (rc != SQLITE_OK) return rc;
    rc = mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = mem_find(path);
    if (!node) return SQLITE_IOERR_DELETE_NOENT;
    node->unlinked = 1;
    if (!node->refs) mem_destroy(node);
    return SQLITE_OK;
}

static int mem_access(sqlite3_vfs *vfs, const char *name, int flags, int *result) {
    char path[MEM_PATH_MAX + 1];
    int rc;
    (void)vfs;
    *result = 0;
    if (flags != SQLITE_ACCESS_EXISTS && flags != SQLITE_ACCESS_READ && flags != SQLITE_ACCESS_READWRITE)
        return SQLITE_IOERR_ACCESS;
    rc = mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    *result = mem_find(path) != NULL;
    return SQLITE_OK;
}

static int mem_fullpath(sqlite3_vfs *vfs, const char *name, int count, char *out) {
    (void)vfs;
    return mem_path(name, count, out);
}

static int mem_randomness(sqlite3_vfs *vfs, int count, char *out) {
    int i;
    (void)vfs;
    for (i = 0; i < count; ++i) {
        mem_random = mem_random * 1664525U + 1013904223U;
        out[i] = (char)(mem_random >> 24);
    }
    return count;
}

static int mem_sleep(sqlite3_vfs *vfs, int microseconds) {
    (void)vfs;
    if (microseconds > 0) mem_unix_ms += microseconds / 1000;
    return microseconds > 0 ? microseconds : 0;
}

static int mem_time(sqlite3_vfs *vfs, double *julian_day) {
    (void)vfs;
    *julian_day = 2440587.5 + (double)mem_unix_ms / 86400000.0;
    return SQLITE_OK;
}

static int mem_error(sqlite3_vfs *vfs, int count, char *out) {
    (void)vfs;
    if (count > 0) out[0] = 0;
    return 0;
}

static sqlite3_vfs mem_vfs = {
    1, sizeof(MemFile), MEM_PATH_MAX, NULL, DOTCC_MEMORY_VFS_NAME, NULL,
    mem_open, mem_delete, mem_access, mem_fullpath,
    NULL, NULL, NULL, NULL, /* Dynamic loading is deliberately unsupported. */
    mem_randomness, mem_sleep, mem_time, mem_error,
    NULL, NULL, NULL, NULL
};

sqlite3_vfs *dotcc_memory_vfs(void) { return &mem_vfs; }
int sqlite3_os_init(void) { return sqlite3_vfs_register(&mem_vfs, 1); }
int sqlite3_os_end(void) { return sqlite3_vfs_unregister(&mem_vfs); }

int dotcc_memory_vfs_reset(void) {
    if (mem_handles) return SQLITE_BUSY;
    while (mem_nodes) mem_destroy(mem_nodes);
    mem_fail_mask = 0;
    mem_random = 1;
    mem_unix_ms = 1735689600000LL;
    return SQLITE_OK;
}

int dotcc_memory_vfs_file_count(void) {
    MemNode *node;
    int count = 0;
    for (node = mem_nodes; node; node = node->next) ++count;
    return count;
}

int dotcc_memory_vfs_handle_count(void) {
    MemFile *file;
    int count = 0;
    for (file = mem_handles; file; file = file->next) ++count;
    return count;
}

void dotcc_memory_vfs_fail_after(int mask, int successful_calls, int result) {
    mem_fail_mask = mask;
    mem_fail_count = successful_calls > 0 ? successful_calls : 0;
    mem_fail_result = result == SQLITE_OK ? SQLITE_IOERR : result;
}

void dotcc_memory_vfs_seed(unsigned int seed) { mem_random = seed; }
void dotcc_memory_vfs_time(sqlite3_int64 unix_milliseconds) { mem_unix_ms = unix_milliseconds; }

int dotcc_memory_vfs_export(const char *name, void *buffer,
                            sqlite3_int64 capacity, sqlite3_int64 *size) {
    char path[MEM_PATH_MAX + 1];
    MemNode *node;
    int rc;
    if (!size || capacity < 0) return SQLITE_MISUSE;
    rc = mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = mem_find(path);
    if (!node) return SQLITE_NOTFOUND;
    if (node->refs) return SQLITE_BUSY;
    *size = node->size;
    if (!buffer) return SQLITE_OK;
    if (capacity < node->size) return SQLITE_TOOBIG;
    if (node->size) memcpy(buffer, node->data, (size_t)node->size);
    return SQLITE_OK;
}

int dotcc_memory_vfs_import(const char *name, const void *buffer, sqlite3_int64 size) {
    char path[MEM_PATH_MAX + 1];
    MemNode *node;
    int created = 0;
    int rc;
    if (size < 0 || (!buffer && size)) return SQLITE_MISUSE;
    rc = mem_path(name, sizeof(path), path);
    if (rc != SQLITE_OK) return rc;
    node = mem_find(path);
    if (node && node->refs) return SQLITE_BUSY;
    if (!node) { node = mem_create(path); created = 1; }
    if (!node) return SQLITE_NOMEM;
    rc = mem_resize(node, size);
    if (rc != SQLITE_OK) {
        if (created) mem_destroy(node);
        return rc;
    }
    if (size) memcpy(node->data, buffer, (size_t)size);
    return SQLITE_OK;
}
