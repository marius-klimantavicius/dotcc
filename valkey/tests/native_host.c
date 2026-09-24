/* Native control for the C embedding seam. It uses the actual Valkey/Lua
 * objects; process isolation here does not qualify managed owner isolation. */
#include "valkey_host.h"
#include <stdio.h>
#include <unistd.h>
#include <fcntl.h>

unsigned long bioPendingJobsOfType(int type);

int main(int argc, char **argv) {
    if (argc != 2) return 2;
    if (valkeyManagedStart(argv[1]) != 0) {
        fprintf(stderr, "HOST_REJECTED %s\n", valkeyManagedLastError());
        if (valkeyManagedCleanup(1) != 0 || valkeyManagedState() != 4) return 7;
        return 3;
    }
    printf("HOST_READY %d\n", valkeyManagedPort());
    fflush(stdout);
    fcntl(0, F_SETFL, O_NONBLOCK);
    for (int i = 0; i < 30000; ++i) {
        char action;
        if (read(0, &action, 1) == 1) break;
        if (valkeyManagedProcessEvents() < 0) return 4;
        usleep(1000);
    }
    if (valkeyManagedStop(2) != 0) { /* SHUTDOWN_NOSAVE */
        fprintf(stderr, "HOST_STOP_ERROR %s\n", valkeyManagedLastError());
        return 5;
    }
    if (valkeyManagedCleanup(0) != 0) {
        fprintf(stderr, "HOST_CLEANUP_ERROR %s\n", valkeyManagedLastError());
        return 6;
    }
    for (int type = 0; type < 6; ++type)
        if (bioPendingJobsOfType(type) != 0) return 8;
    printf("HOST_STOPPED %d\n", valkeyManagedState());
    return 0;
}
