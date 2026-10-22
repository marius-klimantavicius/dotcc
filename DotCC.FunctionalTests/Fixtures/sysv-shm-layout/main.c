#include <sys/shm.h>
#include <sys/ipc.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    struct shmid_ds record = {0};
    record.shm_perm.__key = (key_t)-17;
    record.shm_segsz = ((size_t)1 << 33) + 3;
    record.shm_nattch = 7;
    record.shm_atime = -21;
    printf("ipc=%lu shm=%lu size=%lu atime=%lu pid=%lu nattch=%lu\n",
           (unsigned long)sizeof(struct ipc_perm), (unsigned long)sizeof(record),
           (unsigned long)offsetof(struct shmid_ds, shm_segsz),
           (unsigned long)offsetof(struct shmid_ds, shm_atime),
           (unsigned long)offsetof(struct shmid_ds, shm_cpid),
           (unsigned long)offsetof(struct shmid_ds, shm_nattch));
    printf("key=%d wide=%d time=%d attach=%lu commands=%d flags=%d\n",
           record.shm_perm.__key, record.shm_segsz == 8589934595UL,
           record.shm_atime == -21, (unsigned long)record.shm_nattch,
           IPC_RMID == 0 && IPC_SET == 1 && IPC_STAT == 2,
           IPC_CREAT == 01000 && IPC_EXCL == 02000 && SHM_RDONLY == 010000);
    return 0;
}
