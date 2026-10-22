#ifndef _DOTCC_PWD_H
#define _DOTCC_PWD_H
#include <sys/types.h>
#include <stddef.h>
/* Linux LP64/glibc layout. Non-reentrant lookups return runtime/thread-owned
   record/string storage, overwritten by later lookups. The _r operation uses
   the caller's record, buffer and result-pointer storage. */
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
/* _r storage belongs to the caller; errors are returned numerically, and a
   missing entry is success with *result == NULL. */
int getpwuid_r(uid_t uid, struct passwd *record, char *buffer, size_t length,
               struct passwd **result);
#endif
