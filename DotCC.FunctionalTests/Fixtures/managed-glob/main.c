#include <glob.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/stat.h>
#include <errno.h>
static int errors;
static int onerror(const char *path, int error) { errors++; printf("error %s %d\n", path, error); return 0; }
static void check(const char *pattern, int flags) {
    glob_t result;
    int status = glob(pattern, flags, onerror, &result);
    printf("%s %d %zu", pattern, status, result.gl_pathc);
    for (size_t i = 0; i < result.gl_pathc; i++) printf(" %s", result.gl_pathv[i]);
    printf("\n");
    globfree(&result);
}
int main(void) {
    char root[128];
    char original[4096];
    if (!getcwd(original, sizeof original)) return 1;
    int attempt = 0;
    do {
        snprintf(root, sizeof root, "/tmp/dotcc-glob-%d-%d", getpid(), attempt++);
        if (!mkdir(root, 0700)) break;
        if (errno != EEXIST || attempt > 100) return 1;
    } while (1);
    if (chdir(root)) return 1;
    const char *files[] = {"a.conf", "b.conf", "é.conf", ".hidden.conf", "literal*.conf"};
    for (int i = 0; i < 5; i++) { FILE *f = fopen(files[i], "w"); if (!f) return 2; fclose(f); }
    if (mkdir("sub", 0700)) return 3;
    check("*.conf", 0);
    check("?.conf", 0);
    check("[a-b].conf", 0);
    check("[[:alpha:]].conf", 0);
    check("[!b].conf", 0);
    check("literal\\*.conf", 0);
    check("*.conf", GLOB_PERIOD);
    check("*/", 0);
    check("absent/*", GLOB_ERR);
    check("a.conf/*", GLOB_ERR);
    check("missing*", GLOB_NOCHECK);
    printf("errors %d\n", errors);
    for (int i = 0; i < 5; i++) unlink(files[i]);
    rmdir("sub");
    chdir(original); rmdir(root);
    return 0;
}
