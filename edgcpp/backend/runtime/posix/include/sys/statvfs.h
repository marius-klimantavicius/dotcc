/* Portable LP64 filesystem statistics ABI. statvfs has a runtime-selected
   platform binding; fstatvfs is a declaration only until a binding is added. */
#ifndef EDG_POSIX_STATVFS_H
#define EDG_POSIX_STATVFS_H
struct statvfs {
    unsigned long f_bsize;
    unsigned long f_frsize;
    unsigned long f_blocks;
    unsigned long f_bfree;
    unsigned long f_bavail;
    unsigned long f_files;
    unsigned long f_ffree;
    unsigned long f_favail;
    unsigned long f_fsid;
    unsigned long f_flag;
    unsigned long f_namemax;
};
#define ST_RDONLY 1
#define ST_NOSUID 2
#ifdef __cplusplus
extern "C" {
#endif
int statvfs(const char *, struct statvfs *);
int fstatvfs(int, struct statvfs *);
#ifdef __cplusplus
}
#endif
#endif
