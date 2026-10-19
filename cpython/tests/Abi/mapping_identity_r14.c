#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/mman.h>
#include <unistd.h>

static int checks;
#define CHECK(x) do { ++checks; if (!(x)) { fprintf(stderr, "mapping identity line=%d errno=%d\n", __LINE__, errno); exit(1); } } while (0)

static int mapped(const char *path)
{
    char line[4096];
    FILE *maps = fopen("/proc/self/maps", "r");
    if (!maps) exit(2);
    int found = 0;
    while (fgets(line, sizeof(line), maps))
        if (strstr(line, path)) found = 1;
    fclose(maps);
    return found;
}

int main(void)
{
    char path[] = "/tmp/dotcc-mapping-r14-XXXXXX";
    size_t page = (size_t)sysconf(_SC_PAGESIZE);
    int fd = mkstemp(path);
    CHECK(fd >= 0);
    CHECK(ftruncate(fd, (off_t)(page * 3)) == 0);
    CHECK(!mapped(path));
    unsigned char *memory = mmap(NULL, page * 3, PROT_READ | PROT_WRITE, MAP_SHARED, fd, 0);
    CHECK(memory != MAP_FAILED);
    memory[0] = 42; memory[page] = 17; memory[page * 2] = 91;
    CHECK(mapped(path)); /* Deliberately retained mapping is detected. */
    CHECK(close(fd) == 0);
    CHECK(mapped(path) && memory[page] == 17);
    CHECK(munmap(memory + page, page) == 0);
    CHECK(munmap(memory + page, page) == 0);
    CHECK(memory[0] == 42 && memory[page * 2] == 91 && mapped(path));
    CHECK(munmap(memory, page) == 0);
    CHECK(mapped(path)); /* The remaining partial interval still counts. */
    CHECK(munmap(memory + page * 2, page) == 0);
    CHECK(!mapped(path));

    /* A new anonymous allocation at the old address is a different object.
       NOREPLACE cannot overwrite live storage; no permanent-address oracle. */
    unsigned char *replacement = mmap(memory, page * 3, PROT_READ | PROT_WRITE,
        MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED_NOREPLACE, -1, 0);
    CHECK(replacement == memory);
    unsigned char resident;
    CHECK(mincore(replacement, page, &resident) == 0);
    CHECK(!mapped(path));
    CHECK(munmap(replacement, page * 3) == 0);
    CHECK(unlink(path) == 0);
    printf("native mapping identity passed checks=%d address-reuse-mincore=0 original-storage-absent\n", checks);
    return 0;
}
