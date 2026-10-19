/* Reduction of the actual test_exceptions.test_errno_ENOTDIR failure. */
#include <dirent.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

static int failures;
static void check(const char *path, int expected)
{
    errno = 0;
    DIR *directory = opendir(path);
    int observed = errno;
    int failed = directory != 0 || observed != expected;
    printf("opendir expected=%d actual=%d null=%d failed=%d\n",
           expected, observed, directory == 0, failed);
    failures += failed;
    if (directory) closedir(directory);
}

int main(int argc, char **argv)
{
    if (argc != 3) return 42;
    char child[4096], slash[4096], missing[4096];
    if (strlen(argv[1]) + 20 >= sizeof(child) || strlen(argv[2]) + 20 >= sizeof(child)) return 42;
    strcpy(child, argv[2]); strcat(child, "/child");
    strcpy(slash, argv[2]); strcat(slash, "/");
    strcpy(missing, argv[1]); strcat(missing, "/missing-child");
    check(argv[2], ENOTDIR);
    check(child, ENOTDIR);
    check(slash, ENOTDIR);
    check(missing, ENOENT);
    DIR *directory = opendir(argv[1]);
    if (!directory) return 43;
    int entries = 0;
    while (readdir(directory)) ++entries;
    if (closedir(directory) != 0 || entries < 3) return 43;
    printf("opendir controls=5 failures=%d\n", failures);
    return failures ? 41 : 0;
}
