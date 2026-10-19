#define _GNU_SOURCE
#include <errno.h>
#include <limits.h>
#include <malloc.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>

/* This identical C test calls either native libc or current Libc through
 * test-only managed callbacks. The shared test library is never a product
 * dependency or an implementation of getcwd/chdir/allocator services. */
typedef char *(*cwd_fn)(char *, size_t);
typedef int (*change_fn)(const char *);
typedef void (*release_fn)(void *);
typedef int (*error_fn)(void);
typedef void (*set_error_fn)(int);

int b29_getcwd_contract(const char *root, cwd_fn cwd, change_fn change,
                       release_fn release, error_fn error, set_error_fn set_error,
                       int virtual_owner)
{
    int checks = 0, failures = 0;
#define CHECK(test, label) do { ++checks; if (!(test)) { ++failures; printf("FAIL %s\n", label); } } while (0)
    char *original = cwd(NULL, 0);
    if (!original) return 21;
    char first[4096], renamed[4096], deleted[4096], child[4096];
    if (snprintf(first, sizeof(first), "%s/first", root) >= (int)sizeof(first) ||
        snprintf(renamed, sizeof(renamed), "%s/renamed", root) >= (int)sizeof(renamed) ||
        snprintf(deleted, sizeof(deleted), "%s/deleted", root) >= (int)sizeof(deleted) ||
        snprintf(child, sizeof(child), "%s/first/child", root) >= (int)sizeof(child)) return 22;
    if (mkdir(root, 0700) || mkdir(first, 0700) || mkdir(child, 0700) || mkdir(deleted, 0700)) return 23;
    CHECK(change(first) == 0, "enter-directory");
    char buffer[8192];
    size_t need = strlen(first) + 1;
    const size_t capacities[] = {need, sizeof(buffer) - 2, (size_t)INT_MAX + 31, SIZE_MAX};
    for (size_t i = 0; i < sizeof(capacities) / sizeof(capacities[0]); ++i) {
        memset(buffer, 0xa5, sizeof(buffer)); set_error(123);
        CHECK(cwd(buffer + 1, capacities[i]) == buffer + 1, "caller-buffer-identity");
        CHECK(strcmp(buffer + 1, first) == 0, "exact-utf8-path-and-NUL");
        CHECK((unsigned char)buffer[0] == 0xa5 && (unsigned char)buffer[need + 1] == 0xa5,
              "caller-buffer-sentinels");
        CHECK(error() == 123, "success-preserves-errno");
    }
    const size_t short_capacities[] = {0, 1, need - 1};
    for (size_t i = 0; i < sizeof(short_capacities) / sizeof(short_capacities[0]); ++i) {
        memset(buffer, 0xa5, sizeof(buffer)); set_error(123);
        CHECK(cwd(buffer + 1, short_capacities[i]) == NULL, "short-caller-buffer");
        CHECK(error() == (short_capacities[i] ? ERANGE : EINVAL), "short-caller-errno");
        CHECK((unsigned char)buffer[0] == 0xa5 && (unsigned char)buffer[sizeof(buffer) - 1] == 0xa5,
              "short-buffer-outer-sentinels");
    }
    for (size_t capacity = 0; capacity <= 1; ++capacity) {
        set_error(123); char *value = cwd(NULL, capacity);
        CHECK(capacity ? value == NULL : value != NULL, "GNU-auto-versus-too-small");
        CHECK(error() == (capacity ? ERANGE : 123), "GNU-auto-errno");
        if (value) { CHECK(strcmp(value, first) == 0, "GNU-auto-content"); release(value); }
    }
    set_error(123); char *allocated = cwd(NULL, sizeof(buffer));
    CHECK(allocated != NULL && error() == 123, "GNU-explicit-allocation");
    if (allocated) {
        CHECK(strcmp(allocated, first) == 0, "GNU-explicit-content");
        /* NativeMemory.Alloc and native malloc both use this host allocator.
         * Read its real capacity instead of writing past a broken allocation. */
        CHECK(malloc_usable_size(allocated) >= sizeof(buffer), "GNU-requested-allocation-capacity");
        release(allocated);
    }
    CHECK(change("child") == 0, "relative-child");
    CHECK(cwd(buffer, sizeof(buffer)) == buffer && strcmp(buffer, child) == 0, "relative-child-cwd");
    CHECK(change("..") == 0, "relative-parent");
    CHECK(rename(first, renamed) == 0, "rename-active-directory");
    set_error(123); char *value = cwd(buffer, sizeof(buffer));
    if (virtual_owner) {
        CHECK(value == NULL && error() == ENOENT, "virtual-cwd-is-path-based-no-stale-success");
    } else {
        CHECK(value == buffer && strcmp(buffer, renamed) == 0, "process-cwd-follows-rename");
    }
    CHECK(change(renamed) == 0, "absolute-rename-recovery");
    CHECK(cwd(buffer, sizeof(buffer)) == buffer && strcmp(buffer, renamed) == 0, "renamed-cwd");
    CHECK(change(deleted) == 0 && rmdir(deleted) == 0, "delete-active-directory");
    set_error(123);
    CHECK(cwd(buffer, sizeof(buffer)) == NULL && error() == ENOENT, "deleted-cwd-ENOENT");
    set_error(123);
    value = cwd(NULL, 0);
    CHECK(value == NULL && error() == ENOENT, "deleted-cwd-GNU-ENOENT");
    if (value) release(value);
    set_error(123);
    CHECK(cwd(buffer, 0) == NULL && error() == EINVAL, "deleted-cwd-size-zero-precedence");
    CHECK(change(original) == 0, "absolute-path-restore");
    CHECK(cwd(buffer, sizeof(buffer)) == buffer && strcmp(buffer, original) == 0, "restored-cwd");
    release(original);
    printf("getcwd SAME-C checks=%d failures=%d virtual_owner=%d\n", checks, failures, virtual_owner);
    return failures ? 29 : 0;
}

#ifdef B29_NATIVE_MAIN
static int get_error(void) { return errno; }
static void set_error(int value) { errno = value; }
int main(int argc, char **argv)
{
    if (argc != 2) return 20;
    return b29_getcwd_contract(argv[1], getcwd, chdir, free, get_error, set_error, 0);
}
#endif
