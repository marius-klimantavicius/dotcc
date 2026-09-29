#ifndef DOTCC_LWIP_SYS_ARCH_H
#define DOTCC_LWIP_SYS_ARCH_H
#include <stdint.h>
typedef intptr_t sys_sem_t;
typedef intptr_t sys_mbox_t;
typedef intptr_t sys_mutex_t;
typedef intptr_t sys_thread_t;
typedef uintptr_t sys_prot_t;
#define SYS_SEM_NULL 0
#define SYS_MBOX_NULL 0
#define sys_sem_valid(sem) ((sem) != 0 && *(sem) != 0)
#define sys_sem_set_invalid(sem) (*(sem) = 0)
#define sys_mbox_valid(mbox) ((mbox) != 0 && *(mbox) != 0)
#define sys_mbox_set_invalid(mbox) (*(mbox) = 0)
#define sys_mutex_valid(mutex) ((mutex) != 0 && *(mutex) != 0)
#define sys_mutex_set_invalid(mutex) (*(mutex) = 0)
extern sys_sem_t *lwip_host_netconn_sem_get(void);
extern void lwip_host_netconn_sem_alloc(void);
extern void lwip_host_netconn_sem_free(void);
#endif
