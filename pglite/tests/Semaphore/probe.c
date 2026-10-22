#include <semaphore.h>
#include <errno.h>
#include <stddef.h>
_Static_assert(sizeof(sem_t) == 32, "Linux x64 semaphore storage");
_Static_assert(_Alignof(sem_t) == 8, "Linux x64 semaphore alignment");
int dpg_semaphore_probe(void) {
    sem_t semaphore;
    int count = -1;
    if (sem_init(&semaphore, 1, 2) != 0) return 1;
    if (sem_getvalue(&semaphore, &count) != 0 || count != 2) return 2;
    if (sem_wait(&semaphore) != 0 || sem_trywait(&semaphore) != 0) return 3;
    if (sem_trywait(&semaphore) != -1 || errno != EAGAIN) return 4;
    if (sem_post(&semaphore) != 0) return 5;
    if (sem_getvalue(&semaphore, &count) != 0 || count != 1) return 6;
    if (sem_destroy(&semaphore) != 0) return 7;
    return 0;
}
#ifdef DPG_SEMAPHORE_NATIVE
int main(void) { return dpg_semaphore_probe(); }
#endif
