#define _GNU_SOURCE
#include <errno.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <unistd.h>

#define CHECK(test) do { if (!(test)) { printf("failed line=%d errno=%d\n", __LINE__, errno); return __LINE__; } } while (0)

static long locked_kilobytes(void)
{
    FILE *file = fopen("/proc/self/status", "r");
    char line[512];
    if (!file) return -1;
    while (fgets(line, sizeof line, file)) {
        if (!strncmp(line, "VmLck:", 6)) {
            long value = strtol(line + 6, NULL, 10);
            fclose(file);
            return value;
        }
    }
    fclose(file);
    return -1;
}

int main(void)
{
    long page = sysconf(_SC_PAGESIZE);
    long physical = sysconf(_SC_PHYS_PAGES);
    CHECK(page > 0 && physical > 0);
    errno = 0;
    CHECK(sysconf(-12345) == -1 && errno == EINVAL);
    long before = locked_kilobytes();
    CHECK(before >= 0);
    char *area = (char *)mmap(NULL, (size_t)page * 2, PROT_READ | PROT_WRITE,
                             MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    CHECK(area != MAP_FAILED);
    memset(area, 42, (size_t)page * 2);
    CHECK(mlock(area, (size_t)page * 2) == 0);
    CHECK(locked_kilobytes() == before + page * 2 / 1024);
    /* Locks do not stack; a single unlock releases repeated locks. */
    CHECK(mlock(area, (size_t)page * 2) == 0);
    CHECK(locked_kilobytes() == before + page * 2 / 1024);
    CHECK(munlock(area, (size_t)page) == 0);
    CHECK(locked_kilobytes() == before + page / 1024);
    CHECK(munlock(area, (size_t)page) == 0);
    CHECK(munlock(area, (size_t)page * 2) == 0);
    CHECK(locked_kilobytes() == before);
    CHECK(mlock(area, 0) == 0 && munlock(area, 0) == 0);
    errno = 0;
    CHECK(mlock(NULL, (size_t)page) == -1 && errno == ENOMEM);
    errno = 0;
    CHECK(munlock(NULL, (size_t)page) == -1 && errno == ENOMEM);
    errno = 0;
    CHECK(mlock((void *)(UINTPTR_MAX - 1), 4) == -1 && errno == EINVAL);
    /* Linux DONTNEED discards anonymous contents; the mapping stays usable and
       a subsequent read faults zero pages in. The neighboring page survives. */
    CHECK(madvise(area, (size_t)page, MADV_DONTNEED) == 0);
    for (long i = 0; i < page; ++i) CHECK(area[i] == 0 && area[page + i] == 42);
    memset(area, 91, (size_t)page);
    CHECK(madvise(area, (size_t)page * 2, MADV_DONTNEED) == 0);
    for (long i = 0; i < page * 2; ++i) CHECK(area[i] == 0);
    area[0] = 11;
    area[page] = 22;
    CHECK(area[0] == 11 && area[page] == 22);
    errno = 0;
    CHECK(madvise(area + 1, (size_t)page, MADV_DONTNEED) == -1 && errno == EINVAL);
    errno = 0;
    CHECK(madvise(area, (size_t)page, -1) == -1 && errno == EINVAL);
    CHECK(munmap(area, (size_t)page * 2) == 0);
    printf("physical-pages=%ld page-size=%ld; kernel locking, repeated/partial unlock, discard zero-fill, errors passed\n", physical, page);
    return 0;
}
