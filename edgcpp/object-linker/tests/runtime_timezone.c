#include <errno.h>

#ifdef NATIVE_ORACLE
#include <fcntl.h>
#include <sys/mman.h>
#include <sys/stat.h>
#include <unistd.h>
/* Native comparison boundary only. Translated builds use the shared BCL
   LibcTimezone service and its pinned, hashed timezone bundle. */
static void __edg_timezone_enter(void) {}
static void __edg_timezone_exit(void) {}
static unsigned char *__edg_timezone_map(const char *path, unsigned long *size)
{
    struct stat info;
    int fd = open(path, O_RDONLY);
    void *view;
    if (fd < 0) return 0;
    if (fstat(fd, &info)) { close(fd); return 0; }
    *size = (unsigned long)info.st_size;
    view = mmap(0, *size, PROT_READ, MAP_PRIVATE, fd, 0);
    close(fd);
    return view == MAP_FAILED ? 0 : view;
}
static int __edg_timezone_unmap(void *address, unsigned long size)
{ return munmap(address, size); }
#else
void __edg_timezone_enter(void);
void __edg_timezone_exit(void);
unsigned char *__edg_timezone_map(const char *, unsigned long *);
int __edg_timezone_unmap(void *, unsigned long);
#endif

int main(void)
{
    unsigned long length = 0;
    unsigned char *zone;
    __edg_timezone_enter();
    zone = __edg_timezone_map("/usr/share/zoneinfo/UTC", &length);
    if (!zone || length < 44) return 1;
    if (zone[0] != 'T' || zone[1] != 'Z' || zone[2] != 'i' || zone[3] != 'f') return 2;
    if (__edg_timezone_unmap(zone, length)) return 3;
    __edg_timezone_exit();
    errno = 0;
    if (__edg_timezone_map("/dotobj-missing-timezone-file", &length)) return 4;
    if (errno != ENOENT) return 5;
    return 0;
}
