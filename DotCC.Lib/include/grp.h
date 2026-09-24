#ifndef _DOTCC_GRP_H
#define _DOTCC_GRP_H

#include <sys/types.h>

/* POSIX group database shape under the LP64 C model. Database access must
   be supplied by a host boundary; declarations are not successful stubs. */
struct group {
    char *gr_name;
    char *gr_passwd;
    gid_t gr_gid;
    char **gr_mem;
};

struct group *getgrnam(const char *name);
struct group *getgrgid(gid_t gid);
int getgrnam_r(const char *name, struct group *entry, char *buffer,
               size_t size, struct group **result);
int getgrgid_r(gid_t gid, struct group *entry, char *buffer,
               size_t size, struct group **result);
void setgrent(void);
void endgrent(void);
struct group *getgrent(void);

#endif
