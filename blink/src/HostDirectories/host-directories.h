#ifndef BLINK_CAMPAIGN_HOST_DIRECTORIES_H
#define BLINK_CAMPAIGN_HOST_DIRECTORIES_H
#include <dirent.h>
#include <stddef.h>
/* HostDirectories writes this supplied profile layout, never native dirent. */
_Static_assert(sizeof(DIR) == 8, "private DIR token storage changed");
_Static_assert(sizeof(struct dirent) == 272, "private dirent storage changed");
_Static_assert(offsetof(struct dirent, d_name) == 0, "private dirent name moved");
_Static_assert(offsetof(struct dirent, d_ino) == 256, "private dirent inode moved");
_Static_assert(offsetof(struct dirent, d_type) == 264, "private dirent type moved");
DIR *blink_host_opendir(const char *);
DIR *blink_host_fdopendir(int);
struct dirent *blink_host_readdir(DIR *);
int blink_host_closedir(DIR *);
void blink_host_rewinddir(DIR *);
int blink_host_dirfd(DIR *);
long blink_host_telldir(DIR *);
void blink_host_seekdir(DIR *, long);
#define opendir blink_host_opendir
#define fdopendir blink_host_fdopendir
#define readdir blink_host_readdir
#define closedir blink_host_closedir
#define rewinddir blink_host_rewinddir
#define dirfd blink_host_dirfd
#define telldir blink_host_telldir
#define seekdir blink_host_seekdir
#endif
