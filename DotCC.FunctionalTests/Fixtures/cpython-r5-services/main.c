/* Shared host services reached by CPython's actual 193-unit raw build.
   The larger native/JIT/AOT controls live in cpython/tests/Abi. */
#define _DEFAULT_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include <locale.h>
#include <langinfo.h>
#include <wchar.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <pwd.h>
#include <time.h>
#include <sys/mman.h>

#define CHECK(x) do { if (!(x)) { fprintf(stderr, "r5 services line %d errno %d\n", __LINE__, errno); return 1; } } while (0)

int main(void) {
    CHECK(signbit(copysign(0.0, -1.0)));
    CHECK(nextafter(1.0, 2.0) == 0x1.0000000000001p0);
    CHECK(fma(0x1.0000000000001p0, 0x1.ffffffffffffep-1, -1.0) == -0x1p-104);
    CHECK(erf(INFINITY) == 1 && erfc(-INFINITY) == 2 && exp2(-1074.0) == 0x1p-1074);
    /* CPython's math wrappers pass libc functions by address. */
    double (*unary[3])(double) = {erf, erfc, exp2};
    double (*binary)(double, double) = copysign;
    double (*fused)(double, double, double) = fma;
    CHECK(unary[0](0.0) == 0.0 && unary[1](0.0) == 1.0 && unary[2](2.0) == 4.0);
    CHECK(signbit(binary(0.0, -1.0)) && fused(2.0, 3.0, 4.0) == 10.0);
    CHECK(setlocale(LC_CTYPE, "C.UTF-8") != NULL && strcmp(nl_langinfo(CODESET), "UTF-8") == 0);
    wchar_t wide[4]; char narrow[8];
    CHECK(mbstowcs(wide, "A\xce\xbb", 4) == 2);
    CHECK(wcstombs(narrow, wide, 8) == 3 && strcmp(narrow, "A\xce\xbb") == 0);
    struct passwd *user = getpwuid(getuid());
    CHECK(user != NULL && user->pw_uid == getuid() && user->pw_name != NULL);
    char username[1024];
    CHECK(strlen(user->pw_name) < sizeof(username));
    strcpy(username, user->pw_name);
    CHECK(getpwnam(username) != NULL);
    struct timespec resolution;
    CHECK(clock_getres(CLOCK_MONOTONIC, &resolution) == 0 && resolution.tv_nsec > 0);
    size_t page = (size_t)sysconf(_SC_PAGESIZE);
    char *arena = mmap(NULL, page, PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    CHECK(arena != MAP_FAILED && arena[0] == 0);
    arena[page - 1] = 42;
    CHECK(arena[page - 1] == 42 && munmap(arena, page) == 0);
    char path[] = ".dotcc-cpython-r5-XXXXXX";
    int descriptor = mkstemp(path);
    CHECK(descriptor >= 0);
    FILE *file = fdopen(descriptor, "r+");
    CHECK(file != NULL && fileno(file) == descriptor);
    setbuf(file, NULL);
    CHECK(fputs("shared", file) >= 0 && fseek(file, 0, SEEK_SET) == 0);
    CHECK(fread(narrow, 1, 6, file) == 6 && memcmp(narrow, "shared", 6) == 0);
    CHECK(fclose(file) == 0 && unlink(path) == 0);
    errno = 0;
    CHECK(close(descriptor) == -1 && errno == EBADF);
    puts("shared R5: IEEE math, UTF-16 boundary, passwd, clock, arenas, fdopen/setbuf");
    return 0;
}
