#define _POSIX_C_SOURCE 200809L
#include <stddef.h>
#include <stdio.h>
#include <signal.h>
#include <netinet/in.h>
#include <sys/shm.h>

#define REQUIRE(x) do { if (!(x)) return __LINE__; } while (0)

static unsigned address_evaluations;

static const struct in6_addr *next_address(const struct in6_addr *address)
{
    address_evaluations++;
    return address;
}

int dph_header_compat(unsigned long *observations)
{
    sigset_t signals;
    struct in6_addr address = IN6ADDR_ANY_INIT;
    struct shmid_ds status;

    observations[0] = SIGURG;
    observations[1] = sizeof(struct shmid_ds);
    observations[2] = _Alignof(struct shmid_ds);
    observations[3] = offsetof(struct shmid_ds, shm_segsz);
    observations[4] = offsetof(struct shmid_ds, shm_nattch);
    REQUIRE(sigemptyset(&signals) == 0);
    REQUIRE(sigaddset(&signals, SIGURG) == 0);
    REQUIRE(sigismember(&signals, SIGURG) == 1);
    REQUIRE(sigdelset(&signals, SIGURG) == 0);
    REQUIRE(sigismember(&signals, SIGURG) == 0);

    address_evaluations = 0;
    REQUIRE(IN6_IS_ADDR_UNSPECIFIED(next_address(&address)));
    REQUIRE(address_evaluations == 1);
    for (unsigned index = 0; index < 16; index++)
    {
        address.s6_addr[index] = 1;
        REQUIRE(!IN6_IS_ADDR_UNSPECIFIED(&address));
        address.s6_addr[index] = 0x80;
        REQUIRE(!IN6_IS_ADDR_UNSPECIFIED(&address));
        address.s6_addr[index] = 0;
        REQUIRE(IN6_IS_ADDR_UNSPECIFIED(&address));
    }

    /* Separate test-only definition verifies the remapped C prototype and
       implicit C void-pointer conversion, without requesting kernel SysV IPC. */
    REQUIRE(shmctl(37, IPC_RMID, NULL) == 17);
    void *empty = NULL;
    REQUIRE(shmctl(37, IPC_RMID, empty) == 17);
    REQUIRE(shmctl(37, IPC_STAT, &status) == 19);
    REQUIRE(status.shm_segsz == 4097 && status.shm_nattch == 3);
    unsigned char *mapped_address = shmat(37, NULL, 0);
    REQUIRE(mapped_address != (void *)-1);
    REQUIRE(mapped_address[0] == 0x5a && mapped_address[1] == 0xa5);
    return 0;
}

#ifdef DPH_NATIVE_MAIN
int main(void)
{
    unsigned long observations[5];
    int result = dph_header_compat(observations);
    if (result != 0)
    {
        fprintf(stderr, "header compatibility assertion failed at line %d\n", result);
        return 1;
    }
    for (unsigned index = 0; index < 5; index++)
        printf("%lu%c", observations[index], index == 4 ? '\n' : ' ');
    return 0;
}
#endif
