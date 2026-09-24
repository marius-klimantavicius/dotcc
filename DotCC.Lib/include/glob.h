#ifndef _DOTCC_GLOB_H
#define _DOTCC_GLOB_H

#include <stddef.h>
#ifdef _GNU_SOURCE
#include <dirent.h>
#include <sys/stat.h>
#endif

/* Linux/glibc ABI declarations. The managed runtime supports filesystem glob
   with C-locale matching; unsupported option flags report GLOB_ABORTED/ENOTSUP. */
#define GLOB_ERR (1 << 0)
#define GLOB_MARK (1 << 1)
#define GLOB_NOSORT (1 << 2)
#define GLOB_DOOFFS (1 << 3)
#define GLOB_NOCHECK (1 << 4)
#define GLOB_APPEND (1 << 5)
#define GLOB_NOESCAPE (1 << 6)
#define GLOB_PERIOD (1 << 7)
#define GLOB_MAGCHAR (1 << 8)
#define GLOB_ALTDIRFUNC (1 << 9)
#define GLOB_BRACE (1 << 10)
#define GLOB_NOMAGIC (1 << 11)
#define GLOB_TILDE (1 << 12)
#define GLOB_ONLYDIR (1 << 13)
#define GLOB_TILDE_CHECK (1 << 14)
#define GLOB_NOSPACE 1
#define GLOB_ABORTED 2
#define GLOB_NOMATCH 3
#define GLOB_NOSYS 4
#define GLOB_ABEND GLOB_ABORTED

typedef struct {
    size_t gl_pathc;
    char **gl_pathv;
    size_t gl_offs;
    int gl_flags;
    void (*gl_closedir)(void *);
#ifdef _GNU_SOURCE
    struct dirent *(*gl_readdir)(void *);
#else
    void *(*gl_readdir)(void *);
#endif
    void *(*gl_opendir)(const char *);
#ifdef _GNU_SOURCE
    int (*gl_lstat)(const char *, struct stat *);
    int (*gl_stat)(const char *, struct stat *);
#else
    int (*gl_lstat)(const char *, void *);
    int (*gl_stat)(const char *, void *);
#endif
} glob_t;

int glob(const char *pattern, int flags, int (*errfunc)(const char *, int), glob_t *result);
void globfree(glob_t *result);
int glob_pattern_p(const char *pattern, int quote);

#endif
