#ifndef _DOTCC_PWD_H
#define _DOTCC_PWD_H
#include <sys/types.h>
/* Linux LP64/glibc layout, also the shared runtime's explicit return layout.
   Callers do not own the record/strings; a later lookup on this thread can
   overwrite them. Each runtime/thread has separate reusable storage. */
struct passwd {
    char *pw_name;
    char *pw_passwd;
    uid_t pw_uid;
    gid_t pw_gid;
    char *pw_gecos;
    char *pw_dir;
    char *pw_shell;
};
struct passwd *getpwuid(uid_t uid);
struct passwd *getpwnam(const char *name);
#endif
