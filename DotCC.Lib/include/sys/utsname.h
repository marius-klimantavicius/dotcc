#ifndef _DOTCC_SYS_UTSNAME_H
#define _DOTCC_SYS_UTSNAME_H

/* Linux/glibc ABI: six consecutive 65-byte fields, including the domain name.
   uname is declared here; this header does not implement host identity lookup. */
#define _UTSNAME_LENGTH 65
#define _UTSNAME_SYSNAME_LENGTH 65
#define _UTSNAME_NODENAME_LENGTH 65
#define _UTSNAME_RELEASE_LENGTH 65
#define _UTSNAME_VERSION_LENGTH 65
#define _UTSNAME_MACHINE_LENGTH 65
#define _UTSNAME_DOMAIN_LENGTH 65
#define SYS_NMLN 65

struct utsname {
    char sysname[65];
    char nodename[65];
    char release[65];
    char version[65];
    char machine[65];
#ifdef _GNU_SOURCE
    char domainname[65];
#else
    char __domainname[65];
#endif
};

int uname(struct utsname *name);

#endif
