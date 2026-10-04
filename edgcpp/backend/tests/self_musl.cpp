/* Bounded execution of the headers and libc services used by EDG's musl profile. */
#ifdef EDG_SELF_CONTRACT_PROBE
#include <new>
#endif
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <errno.h>
#include <float.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/mman.h>
#include <unistd.h>
#include <dirent.h>
#include <quadmath.h>

#if defined(__GLIBC__) || defined(_GLIBCXX_RELEASE)
#error Native glibc/libstdc++ headers leaked into the portable self profile
#endif
static_assert(sizeof(long) == 8 && sizeof(void *) == 8, "portable LP64 ABI");
static_assert(sizeof(long double) == 16 && LDBL_MANT_DIG == 64, "binary80 storage ABI");
static_assert(sizeof(off_t) == 8 && sizeof(mode_t) == 4, "managed host-service types");
static_assert(sizeof(struct stat) == 40, "managed stat record, not native OS layout");


#ifdef EDG_SELF_CONTRACT_PROBE
extern "C" int inspect_unimplemented_host(const char *path, char *buffer, size_t size) {
    struct stat metadata;
    int result = stat(path, &metadata);
    if (!getcwd(buffer, size)) return -1;
    std::bad_alloc failure;
    return result + (failure.what()[0] != 0);
}
extern "C" __float128 inspect_unimplemented_quad(const char *text, char **end) {
    return strtoflt128(text, end);
}
#else
int main() {
    volatile __float128 zero = 0;
    __float128 quiet_nan = zero / zero;
    __float128 infinity = (__float128)1 / zero;
    __float128 finite = 1.0e4000Q;
    if (!isnan(quiet_nan) || isnan(infinity) || isnan(finite)) return 10;
    char text[80]; char *end = 0; int consumed = -1;
    errno = 71;
    if (strtol("-123tail", &end, 10) != -123 || strcmp(end, "tail") || errno != 71) return 1;
    if (strtod("0x1.8p2x", &end) != 6.0 || *end != 'x') return 2;
    if (strtold("1.0000000000000000001084202172485504434", &end) != 0x1.0000000000000002p0L || *end) return 3;
    if (snprintf(text, sizeof(text), "%s:%d:%.2f%n", "value", 42, 3.25, &consumed) != 13 ||
        consumed != 13 || strcmp(text, "value:42:3.25")) return 4;
    int parsed = 0; double number = 0; char word[8];
    if (sscanf("37 2.5 sample", "%d %lf %7s", &parsed, &number, word) != 3 ||
        parsed != 37 || number != 2.5 || strcmp(word, "sample")) return 5;
    char *storage = static_cast<char *>(malloc(32));
    if (!storage) return 6;
    memcpy(storage, "abcdef", 7);
    memmove(storage + 1, storage, 7);
    int different = strcmp(storage, "aabcdef");
    free(storage);
    if (different) return 7;
    try { throw 23; } catch (int value) { if (value != 23) return 9; }
    return 0;
}
#endif
