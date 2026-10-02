/* Complete C17 declaration surface for the portable LP64 provider.
   Only registry-backed calls are implemented; declarations do not bind calls. */
#ifndef __EDG_DOT_LIBC_STDLIB_H
#define __EDG_DOT_LIBC_STDLIB_H
#include <stddef.h>
#define EXIT_SUCCESS 0
#define EXIT_FAILURE 1
#define RAND_MAX 2147483647
#define MB_CUR_MAX (__edg_mb_cur_max())
typedef struct { int quot, rem; } div_t;
typedef struct { long quot, rem; } ldiv_t;
typedef struct { long long quot, rem; } lldiv_t;
#ifdef __cplusplus
extern "C" {
#endif
size_t __edg_mb_cur_max(void);
double atof(const char *);
int atoi(const char *);
long atol(const char *);
long long atoll(const char *);
double strtod(const char *, char **);
float strtof(const char *, char **);
long double strtold(const char *, char **);
long strtol(const char *, char **, int);
long long strtoll(const char *, char **, int);
unsigned long strtoul(const char *, char **, int);
unsigned long long strtoull(const char *, char **, int);
int rand(void);
void srand(unsigned int);
void *malloc(size_t);
void *calloc(size_t, size_t);
void *realloc(void *, size_t);
void free(void *);
void *aligned_alloc(size_t, size_t);
int posix_memalign(void **, size_t, size_t);
void abort(void) __attribute__((__noreturn__));
int atexit(void (*)(void));
void exit(int) __attribute__((__noreturn__));
void _Exit(int) __attribute__((__noreturn__));
int at_quick_exit(void (*)(void));
void quick_exit(int) __attribute__((__noreturn__));
char *getenv(const char *);
int system(const char *);
void *bsearch(const void *, const void *, size_t, size_t, int (*)(const void *, const void *));
void qsort(void *, size_t, size_t, int (*)(const void *, const void *));
int abs(int);
long labs(long);
long long llabs(long long);
div_t div(int, int);
ldiv_t ldiv(long, long);
lldiv_t lldiv(long long, long long);
int mblen(const char *, size_t);
int mbtowc(wchar_t *, const char *, size_t);
int wctomb(char *, wchar_t);
size_t mbstowcs(wchar_t *, const char *, size_t);
size_t wcstombs(char *, const wchar_t *, size_t);
#ifdef __cplusplus
}
#endif
#endif
