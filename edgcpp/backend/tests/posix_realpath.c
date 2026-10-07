#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <limits.h>
#include <errno.h>
#include <unistd.h>

#define CHECK(x) do { if (!(x)) { printf("failure %d errno %d\n", __LINE__, errno); return 1; } ++checks; } while (0)
int main(int argc, char **argv) {
    int checks = 0;
    char result[PATH_MAX], expected[PATH_MAX], input[PATH_MAX];
    CHECK(argc == 2);
    CHECK(chdir(argv[1]) == 0);
    CHECK(getcwd(expected, sizeof(expected)) == expected);
    CHECK(realpath(".", result) == result && strcmp(result, expected) == 0);
    char *allocated = realpath(".", NULL);
    CHECK(allocated && strcmp(allocated, expected) == 0);
    free(allocated);
    CHECK(realpath("child/..", result) == result && strcmp(result, expected) == 0);
    CHECK(realpath("link/..", result) == result && strcmp(result, expected) == 0);
    CHECK(realpath("link/file", result) == result);
    snprintf(input, sizeof(input), "%s/child/file", expected);
    CHECK(strcmp(input, result) == 0);
    /* Kernel traversal must not lexically erase a nonexistent component. */
    errno = 0;
    CHECK(realpath("missing/..", result) == NULL && errno == ENOENT);
    errno = 0;
    CHECK(realpath("child/file/..", result) == NULL && errno == ENOTDIR);
    errno = 0;
    CHECK(realpath("", result) == NULL && errno == ENOENT);
    errno = 0;
    CHECK(realpath("dangling", NULL) == NULL && errno == ENOENT);
    errno = 0;
    CHECK(realpath("loop", result) == NULL && errno == ELOOP);
    puts("realpath checks passed");
    return checks != 14;
}
