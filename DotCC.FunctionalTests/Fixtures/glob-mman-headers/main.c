#define _GNU_SOURCE
#include <glob.h>
#include <sys/mman.h>
#include <sys/utsname.h>
#include <stddef.h>
#include <stdio.h>

static int stat_callback(const char *path, struct stat *result) {
    return (path != NULL) + (result == NULL) + 40;
}

int main(void) {
    glob_t result = {0};
    result.gl_stat = stat_callback;
    printf("glob=%zu align=%zu callbacks=%zu,%zu,%zu,%zu,%zu\n",
           sizeof(glob_t), _Alignof(glob_t), offsetof(glob_t, gl_closedir),
           offsetof(glob_t, gl_readdir), offsetof(glob_t, gl_opendir),
           offsetof(glob_t, gl_lstat), offsetof(glob_t, gl_stat));
    printf("paths=%zu,%zu,%zu,%zu callback=%d flags=%d errors=%d,%d,%d,%d\n",
           offsetof(glob_t, gl_pathc), offsetof(glob_t, gl_pathv),
           offsetof(glob_t, gl_offs), offsetof(glob_t, gl_flags),
           result.gl_stat("fixture", NULL), GLOB_BRACE | GLOB_TILDE | GLOB_NOCHECK,
           GLOB_NOSPACE, GLOB_ABORTED, GLOB_NOMATCH, GLOB_NOSYS);
    printf("mmap=%d,%d,%d sync=%d advice=%d,%d lock=%d failed=%d off=%zu\n",
           PROT_READ | PROT_WRITE | PROT_EXEC, MAP_PRIVATE | MAP_ANONYMOUS,
           MAP_FIXED_NOREPLACE, MS_SYNC | MS_INVALIDATE,
           MADV_DONTNEED, POSIX_MADV_DONTNEED, MCL_CURRENT | MCL_FUTURE,
           MAP_FAILED == (void *)-1, sizeof(off_t));
    printf("utsname=%zu align=%zu fields=%zu,%zu,%zu,%zu,%zu,%zu width=%d\n",
           sizeof(struct utsname), _Alignof(struct utsname), offsetof(struct utsname, sysname),
           offsetof(struct utsname, nodename), offsetof(struct utsname, release),
           offsetof(struct utsname, version), offsetof(struct utsname, machine),
           offsetof(struct utsname, domainname), SYS_NMLN);
    return 0;
}
