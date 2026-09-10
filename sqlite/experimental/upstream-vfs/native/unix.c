/* OS ABI adapter only: no SQLite code, locking policy, or handle registry here.
 * This first profile is Linux x64. Reject other targets instead of assuming
 * their errno, flags or struct layouts match Linux. */
#define _GNU_SOURCE
#include <unistd.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/mman.h>
#include <stdint.h>
#include <stddef.h>
#include <sys/time.h>
#if !defined(__linux__) || !defined(__x86_64__) || __SIZEOF_LONG__ != 8
#error This experimental binding requires Linux x64
#endif
struct uvfs_stat {
  uint64_t dev, ino; uint32_t mode; uint64_t nlink;
  uint32_t uid, gid; int64_t size, atime, mtime, ctime;
  uint64_t rdev; int64_t blksize, blocks;
};
_Static_assert(sizeof(struct uvfs_stat)==96 && offsetof(struct uvfs_stat,size)==40, "dotcc stat bridge layout");
static void copy_stat(struct uvfs_stat *d, const struct stat *s) {
  *d = (struct uvfs_stat){s->st_dev,s->st_ino,s->st_mode,s->st_nlink,
    s->st_uid,s->st_gid,s->st_size,s->st_atim.tv_sec,s->st_mtim.tv_sec,
    s->st_ctim.tv_sec,s->st_rdev,s->st_blksize,s->st_blocks};
}
int uvfs_stat(const char *p, struct uvfs_stat *d) { struct stat s; int rc=stat(p,&s); if(!rc) copy_stat(d,&s); return rc; }
int uvfs_fstat(int f, struct uvfs_stat *d) { struct stat s; int rc=fstat(f,&s); if(!rc) copy_stat(d,&s); return rc; }
int uvfs_lstat(const char *p, struct uvfs_stat *d) { struct stat s; int rc=lstat(p,&s); if(!rc) copy_stat(d,&s); return rc; }
int uvfs_open(const char *p,int f,int m) { return open(p,f,m); }
int uvfs_close(int f) { return close(f); }
int uvfs_access(const char *p,int m) { return access(p,m); }
char *uvfs_getcwd(char *p,uint64_t n) { return getcwd(p,n); }
int uvfs_ftruncate(int f,int64_t n) { return ftruncate(f,n); }
int64_t uvfs_read(int f,void *p,uint64_t n) { return read(f,p,n); }
int64_t uvfs_write(int f,const void *p,uint64_t n) { return write(f,p,n); }
int64_t uvfs_pread(int f,void *p,uint64_t n,int64_t o) { return pread(f,p,n,o); }
int64_t uvfs_pwrite(int f,const void *p,uint64_t n,int64_t o) { return pwrite(f,p,n,o); }
int64_t uvfs_lseek(int f,int64_t o,int w) { return lseek(f,o,w); }
int uvfs_fchmod(int f,unsigned m) { return fchmod(f,m); }
int uvfs_fchown(int f,unsigned u,unsigned g) { return fchown(f,u,g); }
unsigned uvfs_geteuid(void) { return geteuid(); }
int uvfs_getpid(void) { return getpid(); }
int uvfs_unlink(const char *p) { return unlink(p); }
int uvfs_mkdir(const char *p,unsigned m) { return mkdir(p,m); }
int uvfs_rmdir(const char *p) { return rmdir(p); }
int64_t uvfs_readlink(const char *p,char *b,uint64_t n) { return readlink(p,b,n); }
int uvfs_fsync(int f) { return fsync(f); }
int uvfs_fdatasync(int f) { return fdatasync(f); }
int uvfs_getpagesize(void) { return getpagesize(); }
void *uvfs_mmap(void *p,uint64_t n,int prot,int flags,int f,int64_t o) { return mmap(p,n,prot,flags,f,o); }
int uvfs_munmap(void *p,uint64_t n) { return munmap(p,n); }
struct uvfs_flock { short type, whence; int64_t start,len; int pid; };
int uvfs_fcntl(int f,int cmd,int64_t arg) {
  if (cmd==5 || cmd==6 || cmd==7) {
    struct uvfs_flock *a=(void*)(intptr_t)arg;
    struct flock b={.l_type=a->type,.l_whence=a->whence,.l_start=a->start,.l_len=a->len,.l_pid=a->pid};
    int rc=fcntl(f,cmd==5?F_GETLK:cmd==6?F_SETLK:F_SETLKW,&b);
    if(rc>=0) { a->type=b.l_type; a->whence=b.l_whence; a->start=b.l_start; a->len=b.l_len; a->pid=b.l_pid; }
    return rc;
  }
  return fcntl(f,cmd,(intptr_t)arg);
}

int64_t uvfs_sysconf(int n) { return sysconf(n); }
int uvfs_utimes(const char *p, const void *t) { return utimes(p,t); }
