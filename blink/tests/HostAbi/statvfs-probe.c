#define _GNU_SOURCE 1
#include <stddef.h>
#include <stdio.h>
#include <sys/statvfs.h>
#define FIELD(f) printf(#f ".offset %zu\n" #f ".size %zu\n", offsetof(struct statvfs, f), sizeof(((struct statvfs *)0)->f))
#define CONST(n) printf(#n " %lu\n", (unsigned long)(n))
int main(void) {
 printf("statvfs.size %zu\nstatvfs.alignment %zu\n", sizeof(struct statvfs), _Alignof(struct statvfs));
 FIELD(f_bsize); FIELD(f_frsize); FIELD(f_blocks); FIELD(f_bfree); FIELD(f_bavail);
 FIELD(f_files); FIELD(f_ffree); FIELD(f_favail); FIELD(f_fsid); FIELD(f_flag); FIELD(f_namemax);
 CONST(ST_RDONLY); CONST(ST_NOSUID); CONST(ST_NODEV); CONST(ST_NOEXEC); CONST(ST_SYNCHRONOUS);
 CONST(ST_MANDLOCK); CONST(ST_WRITE); CONST(ST_APPEND); CONST(ST_IMMUTABLE); CONST(ST_NOATIME); CONST(ST_NODIRATIME); CONST(ST_RELATIME);
 return 0;
}
