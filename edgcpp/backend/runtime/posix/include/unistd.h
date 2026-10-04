#ifndef EDG_POSIX_UNISTD_H
#define EDG_POSIX_UNISTD_H
#include <sys/types.h>
#define _SC_PAGESIZE 30
#define _SC_PAGE_SIZE _SC_PAGESIZE
#define STDIN_FILENO 0
#define STDOUT_FILENO 1
#define STDERR_FILENO 2
#define F_OK 0
#define X_OK 1
#define W_OK 2
#define R_OK 4
#ifndef SEEK_SET
#define SEEK_SET 0
#define SEEK_CUR 1
#define SEEK_END 2
#endif
#ifdef __cplusplus
extern "C" {
#endif
int close(int);
ssize_t read(int, void *, size_t);
ssize_t write(int, const void *, size_t);
ssize_t pread(int, void *, size_t, off_t);
ssize_t pwrite(int, const void *, size_t, off_t);
off_t lseek(int, off_t, int);
int access(const char *, int);
char *getcwd(char *, size_t);
int unlink(const char *);
int rmdir(const char *);
ssize_t readlink(const char *, char *, size_t);
int ftruncate(int, off_t);
int fsync(int);
int fdatasync(int);
int fchown(int, uid_t, gid_t);
uid_t geteuid(void);
pid_t getpid(void);
unsigned int sleep(unsigned int);
int usleep(unsigned int);
long sysconf(int);
#ifdef __cplusplus
}
#endif
#endif
