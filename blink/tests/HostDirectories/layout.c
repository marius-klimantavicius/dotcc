#include <stddef.h>
#include <stdio.h>
#include <dirent.h>
#if defined(BLINK_MANAGED_DIRECTORIES) || defined(BLINK_PROFILE_LAYOUT)
#include "host-directories.h"
#endif
int DirectoryLayout(void) {
  struct dirent entry;
  printf("dirent %zu %zu %zu %zu %zu %zu\n",sizeof(entry),_Alignof(struct dirent),offsetof(struct dirent,d_name),offsetof(struct dirent,d_ino),offsetof(struct dirent,d_type),(size_t)((char*)&entry.d_ino-(char*)&entry));
#if defined(BLINK_MANAGED_DIRECTORIES) || defined(BLINK_PROFILE_LAYOUT)
  printf("private DIR %zu %zu\n",sizeof(DIR),_Alignof(DIR));
#endif
  return 0;
}
#ifndef BLINK_MANAGED_DIRECTORIES
int main(void){return DirectoryLayout();}
#endif
