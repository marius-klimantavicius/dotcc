#include <sys/shm.h>

/* Test-only callee: the original PGlite definition is also int(int, int,
   struct shmid_ds *), in a different translation unit from its callers. */
int shmctl(int shmid, int command, struct shmid_ds *status)
{
    if (shmid != 37) return -1;
    if (command == IPC_RMID && status == 0) return 17;
    if (command == IPC_STAT && status != 0)
    {
        status->shm_segsz = 4097;
        status->shm_nattch = 3;
        return 19;
    }
    return -1;
}

static unsigned char mapped_bytes[2] = {0x5a, 0xa5};

void *shmat(int shmid, const void *address, int flags)
{
    if (shmid == 37 && address == 0 && flags == 0) return mapped_bytes;
    return (void *)-1;
}
