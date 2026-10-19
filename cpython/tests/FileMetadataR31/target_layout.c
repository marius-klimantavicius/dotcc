#include <sys/stat.h>
#include <stddef.h>
#ifdef __GNUC__
#define offsetof(type, field) __builtin_offsetof(type, field)
#endif

/* Compile with the actual target headers, independently of the host oracle. */
void target_layout(unsigned long *out) {
    out[0] = sizeof(struct stat);
    out[1] = _Alignof(struct stat);
#define FIELD(n, f) out[n] = offsetof(struct stat, f)
    FIELD(2, st_dev); FIELD(3, st_ino); FIELD(4, st_mode);
    FIELD(5, st_nlink); FIELD(6, st_uid); FIELD(7, st_gid);
    FIELD(8, st_size); FIELD(9, st_atime); FIELD(10, st_mtime);
    FIELD(11, st_ctime); FIELD(12, st_rdev); FIELD(13, st_blksize);
    FIELD(14, st_blocks);
}
