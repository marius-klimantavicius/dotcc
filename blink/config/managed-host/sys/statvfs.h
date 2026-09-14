#ifndef BLINK_MANAGED_HOST_SYS_STATVFS_H
#define BLINK_MANAGED_HOST_SYS_STATVFS_H
/* Native-measured Linux LP64 storage. No filesystem capacity implementation. */
struct blink_host_statvfs_record {
 unsigned long f_bsize, f_frsize;
 unsigned long f_blocks, f_bfree, f_bavail;
 unsigned long f_files, f_ffree, f_favail;
 unsigned long f_fsid, f_flag, f_namemax;
 int __reserved[6];
};
#define ST_RDONLY (1)
#define ST_NOSUID (2)
#define ST_NODEV (4)
#define ST_NOEXEC (8)
#define ST_SYNCHRONOUS (16)
#define ST_MANDLOCK (64)
#define ST_WRITE (128)
#define ST_APPEND (256)
#define ST_IMMUTABLE (512)
#define ST_NOATIME (1024)
#define ST_NODIRATIME (2048)
#define ST_RELATIME (4096)
int blink_host_statvfs(const char *, struct blink_host_statvfs_record *);
int blink_host_fstatvfs(int, struct blink_host_statvfs_record *);
#define statvfs blink_host_statvfs_record
#define blink_host_statvfs_record(...) blink_host_statvfs(__VA_ARGS__)
#define fstatvfs blink_host_fstatvfs
#endif
