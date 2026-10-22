#ifndef _DOTCC_SYS_IPC_H
#define _DOTCC_SYS_IPC_H
#include <sys/types.h>

/* System V IPC data declarations for DotCC's Linux x64 LP64 C ABI.
   These declarations provide no kernel IPC service or native import. */
typedef int key_t;
struct ipc_perm {
    key_t __key;
    uid_t uid;
    gid_t gid;
    uid_t cuid;
    gid_t cgid;
    unsigned int mode;
    unsigned short __seq;
    unsigned short __pad2;
    unsigned long __reserved1;
    unsigned long __reserved2;
};
#define IPC_PRIVATE ((key_t)0)
#define IPC_CREAT 01000
#define IPC_EXCL 02000
#define IPC_NOWAIT 04000
#define IPC_RMID 0
#define IPC_SET 1
#define IPC_STAT 2
#endif
