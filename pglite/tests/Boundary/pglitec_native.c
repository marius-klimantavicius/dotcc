/* Native oracle for the ACTUAL selected fork source, not an engine model. */
#include <stdbool.h>
#include <stdlib.h>
#include DPG_PGLITEC_SOURCE

static int callbacks;
static ssize_t io(void *buffer, size_t length) {
    unsigned char *bytes = buffer;
    if (bytes[0] != 0xce || bytes[1] != 0xbb || bytes[2] != 0) return -2;
    bytes[3] = 42;
    return length == 0 ? -1 : (ssize_t)length;
}
static void first(void) { callbacks = callbacks * 10 + 1; }
static void second(void) { callbacks = callbacks * 10 + 2; }

int main(int argc, char **argv) {
    if (argc == 2 && strcmp(argv[1], "--top-jump") == 0) {
        /* Real setjmp frame remains alive, unlike the yielded engine frame. */
        if (sigsetjmp((void *)postgresmain_sigjmp_buf, 1) == 0) {
            pgl_setPGliteActive(1);
            pgl_longjmp((void *)postgresmain_sigjmp_buf, 7);
        }
        return 7; /* Must be intercepted as synthetic exit 100. */
    }
    unsigned char buffer[4] = {0xce, 0xbb, 0, 0};
    size_t wide = ((size_t)1 << 33) + 9;
    pgl_set_rw_cbs(io, io);
    if (pgl_recv(1, buffer, wide, 0) != (ssize_t)wide || buffer[3] != 42) return 1;
    if (pgl_send(1, buffer, 0, 0) != -1) return 2;
    if (pgl_atexit(first) != 0 || pgl_atexit(second) != 0) return 3;
    pgl_run_atexit_funcs();
    if (callbacks != 21) return 4;
    pgl_run_atexit_funcs();
    if (callbacks != 21) return 5;
    int id = pgl_shmget(12345, 4097, IPC_CREAT);
    if (id < 0) return 6;
    unsigned char *memory = pgl_shmat(id, NULL, 0);
    if (memory == (void *)-1) return 7;
    memory[4096] = 42;
    if (pgl_shmget(12345, 4097, 0) != id || memory[4096] != 42) return 8;
    if (pgl_shmdt(memory) != 0 || pgl_shmctl(id, IPC_RMID, NULL) != 0) return 9;
    if (pgl_shmget(12345, 4097, 0) != -1 || errno != ENOENT) return 10;
    if (pgl_setPGliteActive(1) != 0 || pgl_setPGliteActive(0) != 1) return 11;
    puts("PASS actual fork callbacks, atexit LIFO/reset, shared-memory lifecycle and active toggle; no SQL workflow");
    return 0;
}
