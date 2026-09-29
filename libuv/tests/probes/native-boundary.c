/* Fixed signatures from Linux glibc x64; qualification probe, not SDK headers. */
typedef unsigned long size_t;
typedef long ssize_t;
int pipe(int fds[2]);
int close(int fd);
ssize_t read(int fd, void* data, size_t size);
ssize_t write(int fd, const void* data, size_t size);
int* __errno_location(void);
int native_boundary_probe(void) {
    int fds[2];
    char sent[4] = { 0, 42, 127, -1 };
    char received[4] = { 0 };
    if (pipe(fds) != 0) return 1;
    if (write(fds[1], sent, sizeof sent) != sizeof sent) return 2;
    if (read(fds[0], received, sizeof received) != sizeof received) return 3;
    for (int i = 0; i < 4; i++) if (sent[i] != received[i]) return 4;
    if (close(fds[0]) != 0 || close(fds[1]) != 0) return 5;
    if (close(-1) != -1 || *__errno_location() != 9) return 6;
    return 0;
}
/* Called through an authored unmanaged pthread start thunk. */
void* translated_worker(void* context) {
    int* value = (int*)context;
    *value += 1;
    return context;
}
