#ifndef _DOTCC_SYS_SHM_H
#define _DOTCC_SYS_SHM_H
#include <sys/ipc.h>

/* Linux x64 LP64 layout, including ABI-reserved fields. PGlite supplies its
   own shm operations over allocated storage. This header adds data declarations
   only: the shared runtime does not implement kernel System V shared memory. */
typedef unsigned long shmatt_t;
struct shmid_ds {
    struct ipc_perm shm_perm;
    size_t shm_segsz;
    time_t shm_atime;
    time_t shm_dtime;
    time_t shm_ctime;
    pid_t shm_cpid;
    pid_t shm_lpid;
    shmatt_t shm_nattch;
    unsigned long __reserved5;
    unsigned long __reserved6;
};
#define SHM_RDONLY 010000
#define SHM_RND 020000
#define SHM_REMAP 040000
#define SHM_EXEC 0100000
#endif
