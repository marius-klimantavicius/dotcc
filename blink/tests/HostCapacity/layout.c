#include <stddef.h>
#include <stdio.h>
#include <sys/statvfs.h>
#define FIELD(f) do { struct statvfs value; printf(#f " %zu %zu %zu\n", offsetof(struct statvfs,f), sizeof(value.f), (size_t)((char*)&value.f-(char*)&value)); } while(0)
int CapacityLayout(void) {
  printf("statvfs %zu %zu\n",sizeof(struct statvfs),_Alignof(struct statvfs));
  FIELD(f_bsize);FIELD(f_frsize);FIELD(f_blocks);FIELD(f_bfree);FIELD(f_bavail);
  FIELD(f_files);FIELD(f_ffree);FIELD(f_favail);FIELD(f_fsid);FIELD(f_flag);FIELD(f_namemax);
  printf("flags %u %u %u\n",ST_RDONLY,ST_NOSUID,ST_NODEV);return 0;
}
#ifndef BLINK_MANAGED_CAPACITY
int main(void){return CapacityLayout();}
#endif
