#ifndef _STDLIB_H
#define _STDLIB_H
float strtof(const char *text, char **end);

#include <stddef.h>

/* dotcc's <stdlib.h> — memory, conversions, RNG, environment, program
   control, and generic sort/search. Implementations: malloc/free/strtod/
   atof in DotCC.Libc/Libc.cs, the rest in DotCC.Libc/StdlibLib.cs. Allocation sizes use the LP64
   size_t ABI, including calls through function pointers. */

#ifndef NULL
#define NULL ((void*)0)
#endif

#define EXIT_SUCCESS 0
#define EXIT_FAILURE 1
#define RAND_MAX     32767

/* div/ldiv/lldiv result structs. These names are pre-registered in
   dotcc's TypeNameRewriter (Compiler.PredefinedTypeNames) and resolve to
   the Libc.div_t / ldiv_t / lldiv_t value structs — no typedef needed. */

/* Memory management. */
void* malloc(size_t size);
void* calloc(size_t n, size_t size);
void* realloc(void* p, size_t size);
void free(void* p);
/* POSIX: returns an error number directly, without modifying errno. */
int posix_memalign(void **memptr, size_t alignment, size_t size);

/* String -> number conversions. strtod parses a leading double and (if endptr
   is non-null) reports where parsing stopped; atof is strtod without endptr. */
double strtod(const char *nptr, char **endptr);
long double strtold(const char *nptr, char **endptr);
double atof(const char *nptr);
long strtol(const char *nptr, char **endptr, int base);
long strtoll(const char *nptr, char **endptr, int base);
unsigned long strtoul(const char *nptr, char **endptr, int base);
unsigned long strtoull(const char *nptr, char **endptr, int base);
int atoi(const char *nptr);
long atol(const char *nptr);
long atoll(const char *nptr);
/* Counts are bytes for wcstombs and UTF-16 units for mbstowcs. A NULL
   destination measures the complete NUL-terminated conversion, ignoring n.
   C/POSIX use ASCII; C.UTF-8 uses strict UTF-8, including surrogate pairs.
   Invalid byte sequences/lone surrogates return (size_t)-1 with EILSEQ. */
size_t mbstowcs(wchar_t *destination, const char *source, size_t n);
size_t wcstombs(char *destination, const wchar_t *source, size_t n);

/* Integer arithmetic. */
int abs(int n);
long labs(long n);
long llabs(long n);
div_t div(int num, int den);
ldiv_t ldiv(long num, long den);
lldiv_t lldiv(long num, long den);

/* Pseudo-random numbers. */
int rand(void);
void srand(unsigned int seed);
long random(void);
void srandom(unsigned int seed);

/* Secure byte generation (BSD/glibc); distinct from rand/srand. */
void arc4random_buf(void *buffer, size_t length);

/* Environment + program control. */
char* getenv(const char *name);
int setenv(const char *name, const char *value, int overwrite);
int unsetenv(const char *name);
char *realpath(const char *path, char *resolved_path);
int system(const char *command);
#if defined(__STDC_VERSION__) && __STDC_VERSION__ >= 201112L
_Noreturn void exit(int code);
_Noreturn void _Exit(int code);
_Noreturn void abort(void);
#else
void exit(int code);
void _Exit(int code);
void abort(void);
#endif

/* Generic sort / search — comparator is a function pointer. */
void qsort(void* base, int n, int size, int (*cmp)(const void*, const void*));
void* bsearch(const void* key, const void* base, int n, int size, int (*cmp)(const void*, const void*));

int mkstemp(char *template);

#endif
