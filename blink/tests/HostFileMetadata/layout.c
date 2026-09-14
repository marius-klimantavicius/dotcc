#include <stddef.h>
#include <errno.h>
#include <stdio.h>
#include <sys/stat.h>
#define FIELD(f) do { struct stat value; printf(#f " %zu %zu %zu\n", offsetof(struct stat,f), sizeof(value.f), (size_t)((char*)&value.f-(char*)&value)); } while(0)
int MetadataLayout(void) {
  printf("stat %zu %zu\n",sizeof(struct stat),_Alignof(struct stat));
  FIELD(st_dev); FIELD(st_ino); FIELD(st_nlink); FIELD(st_mode);
  FIELD(st_uid); FIELD(st_gid); FIELD(st_rdev); FIELD(st_size);
  FIELD(st_blksize); FIELD(st_blocks);
  FIELD(st_atim); FIELD(st_mtim); FIELD(st_ctim);
  FIELD(st_atim.tv_sec); FIELD(st_atim.tv_nsec);
  printf("modes %u %u %u %u %u %u %u %u\n",S_IFMT,S_IFREG,S_IFDIR,S_IFSOCK,S_IFIFO,S_IFLNK,S_IRUSR,S_IXUSR);
#ifdef BLINK_MANAGED_METADATA
  printf("pathname errno %d\n",36);
#else
  printf("pathname errno %d\n",ENAMETOOLONG);
#endif
  return 0;
}
#ifndef BLINK_MANAGED_METADATA
int main(void) { return MetadataLayout(); }
#endif
