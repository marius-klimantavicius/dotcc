#ifndef BLINK_MANAGED_HOST_SYS_STAT_H
#define BLINK_MANAGED_HOST_SYS_STAT_H
#include <stdint.h>
/* Linux x86-64 LP64 host record, independently measured. This is not the
 * guest struct stat_linux or dotcc's generic partial stat record. */
struct blink_host_stat_time { int64_t tv_sec; int64_t tv_nsec; };
struct blink_host_stat_record {
  uint64_t st_dev, st_ino, st_nlink;
  uint32_t st_mode, st_uid, st_gid;
  int32_t __pad0;
  uint64_t st_rdev;
  int64_t st_size, st_blksize, st_blocks;
  struct blink_host_stat_time st_atim, st_mtim, st_ctim;
  int64_t __reserved[3];
};
#define st_atime st_atim.tv_sec
#define st_mtime st_mtim.tv_sec
#define st_ctime st_ctim.tv_sec
#define S_IFMT   0170000
#define S_IFSOCK 0140000
#define S_IFLNK  0120000
#define S_IFREG  0100000
#define S_IFBLK  0060000
#define S_IFDIR  0040000
#define S_IFCHR  0020000
#define S_IFIFO  0010000

#define S_ISDIR(m)  (((m) & S_IFMT) == S_IFDIR)
#define S_ISREG(m)  (((m) & S_IFMT) == S_IFREG)
#define S_ISSOCK(m) (((m) & S_IFMT) == S_IFSOCK)
#define S_ISLNK(m)  (((m) & S_IFMT) == S_IFLNK)
#define S_ISBLK(m)  (((m) & S_IFMT) == S_IFBLK)
#define S_ISCHR(m)  (((m) & S_IFMT) == S_IFCHR)
#define S_ISFIFO(m) (((m) & S_IFMT) == S_IFIFO)

/* Permission + set-id/sticky bits. */
#define S_ISUID 04000
#define S_ISGID 02000
#define S_ISVTX 01000
#define S_IRWXU 00700
#define S_IRUSR 00400
#define S_IWUSR 00200
#define S_IXUSR 00100
#define S_IRWXG 00070
#define S_IRGRP 00040
#define S_IWGRP 00020
#define S_IXGRP 00010
#define S_IRWXO 00007
#define S_IROTH 00004
#define S_IWOTH 00002
#define S_IXOTH 00001


int blink_host_stat(const char *, struct blink_host_stat_record *);
int blink_host_lstat(const char *, struct blink_host_stat_record *);
int blink_host_fstat(int, struct blink_host_stat_record *);
/* Two-stage macro separates the struct tag from the call symbol. A bare stat
 * function designator is unsupported and cannot bind the generic host stat. */
#define stat blink_host_stat_record
#define blink_host_stat_record(...) blink_host_stat(__VA_ARGS__)
#define fstat blink_host_fstat
#define lstat blink_host_lstat
#define fstatat blink_host_fstatat
#define mkdir blink_host_mkdir
#define mkfifo blink_host_mkfifo
#define chmod blink_host_chmod
#define fchmod blink_host_fchmod
#define fchmodat blink_host_fchmodat
int fstatat(int, const char *, struct stat *, int);
int mkdir(const char *, unsigned int);
int mkfifo(const char *, unsigned int);
int chmod(const char *, unsigned int);
int fchmod(int, unsigned int);
int fchmodat(int, const char *, unsigned int, int);
#endif
