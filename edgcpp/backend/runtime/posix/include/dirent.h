/* Opaque directory owner and portable entry, not native DIR/dirent layouts. */
#ifndef EDG_POSIX_DIRENT_H
#define EDG_POSIX_DIRENT_H
#include <sys/types.h>
typedef struct __edg_DIR DIR;
struct dirent { char d_name[4096]; };
#ifdef __cplusplus
extern "C" {
#endif
DIR *opendir(const char *);
struct dirent *readdir(DIR *);
int closedir(DIR *);
#ifdef __cplusplus
}
#endif
#endif
