/* Portable metadata exchanged only with a managed host-service adapter.
   Never pass this record directly to a native stat/fstat implementation. */
#ifndef EDG_SELF_SYS_STAT_H
#define EDG_SELF_SYS_STAT_H
#include <sys/types.h>
struct stat {
    dev_t st_dev;
    ino_t st_ino;
    mode_t st_mode;
    off_t st_size;
    time_t st_mtime;
};
#define S_IFMT 0170000
#define S_IFREG 0100000
#define S_IFDIR 0040000
#define S_ISREG(mode) (((mode) & S_IFMT) == S_IFREG)
#define S_ISDIR(mode) (((mode) & S_IFMT) == S_IFDIR)
#ifdef __cplusplus
extern "C" {
#endif
int stat(const char *, struct stat *);
int fstat(int, struct stat *);
#ifdef __cplusplus
}
#endif
#endif
