#define _GNU_SOURCE
#include <sys/stat.h>
#include <stddef.h>
#include <stdio.h>
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>

/* Host ABI and values come from system headers, never a managed mirror. */
void host_layout(unsigned long *out) {
    out[0] = sizeof(struct stat); out[1] = _Alignof(struct stat);
#define FIELD(n, f) out[n] = offsetof(struct stat, f)
    FIELD(2, st_dev); FIELD(3, st_ino); FIELD(4, st_mode);
    FIELD(5, st_nlink); FIELD(6, st_uid); FIELD(7, st_gid);
    FIELD(8, st_size); FIELD(9, st_atime); FIELD(10, st_mtime);
    FIELD(11, st_ctime); FIELD(12, st_rdev); FIELD(13, st_blksize);
    FIELD(14, st_blocks);
}
int host_values(const char *path, int nofollow, unsigned long *out) {
    struct stat s;
    int r = nofollow ? lstat(path, &s) : stat(path, &s);
    if (r) return errno;
    out[0]=s.st_dev; out[1]=s.st_ino; out[2]=s.st_mode;
    out[3]=s.st_nlink; out[4]=s.st_uid; out[5]=s.st_gid;
    out[6]=s.st_size; out[7]=s.st_atime; out[8]=s.st_mtime;
    out[9]=s.st_ctime; out[10]=s.st_rdev; out[11]=s.st_blksize;
    out[12]=s.st_blocks;
    return 0;
}
#ifdef STANDALONE
int main(int argc, char **argv) {
    if (argc != 2) return 2;
    unsigned long l[15]; host_layout(l);
    printf("host-layout"); for (int i=0;i<15;i++) printf(" %lu",l[i]); puts("");
    int fd=open(argv[1],O_RDWR|O_CREAT|O_EXCL,0640);
    if (fd<0 || write(fd,"metadata",8)!=8) return 3;
    struct stat p,f;
    if (stat(argv[1],&p) || fstat(fd,&f) || p.st_mode!=(S_IFREG|0640)
        || p.st_uid!=getuid() || p.st_gid!=getgid() || p.st_size!=8
        || p.st_ino!=f.st_ino || p.st_dev!=f.st_dev || p.st_nlink!=1) return 4;
    if (unlink(argv[1]) || fstat(fd,&f) || f.st_nlink!=0 || f.st_ino!=p.st_ino) return 5;
    close(fd); puts("native creation/metadata/descriptor/unlink checks passed"); return 0;
}
#endif
