/* Execute real PGlite backend objects in this test process. No engine code here.
 * Emscripten ExitStatus has a test-only native equivalent using --wrap=exit. */
#define _GNU_SOURCE
#include <arpa/inet.h>
#include <errno.h>
#include <setjmp.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <unistd.h>

extern int pgl_postgres_main(int, char **);
extern int pgl_setPGliteActive(int);
extern void pgl_set_rw_cbs(ssize_t (*)(void *, size_t), ssize_t (*)(void *, size_t));
extern void pgl_startPGlite(void);
extern void pgl_pq_flush(void);
extern void PostgresMainLoopOnce(void);
extern void PostgresMainLongJmp(void);
extern void PostgresSendReadyForQueryIfNecessary(void);
extern void proc_exit(int);
extern void pgl_run_atexit_funcs(void);
extern void __real_exit(int);
extern int postmaster_alive_fds[2];

static jmp_buf exit_target;
static int exit_armed, exit_status;
static unsigned char input[1024 * 1024], output[4 * 1024 * 1024];
static size_t input_length, input_offset, output_length;

void __wrap_exit(int status)
{
    if (!exit_armed) __real_exit(status);
    exit_status = status;
    longjmp(exit_target, 1);
}

static void fail(const char *text)
{
    fprintf(stderr, "native-control harness: %s (status=%d errno=%d)\n", text, exit_status, errno);
    __real_exit(24);
}

static ssize_t read_cb(void *buffer, size_t count)
{
    size_t available = input_length - input_offset;
    if (count > available) count = available;
    memcpy(buffer, input + input_offset, count);
    input_offset += count;
    return (ssize_t) count;
}

static ssize_t write_cb(void *buffer, size_t count)
{
    if (count > sizeof(output) - output_length) fail("response exceeds test limit");
    memcpy(output + output_length, buffer, count);
    output_length += count;
    return (ssize_t) count;
}

static void execute(const char *sql)
{
    uint32_t length = (uint32_t)strlen(sql) + 5, wire_length = htonl(length);
    if (length + 1 > sizeof(input)) fail("query exceeds test limit");
    input[0] = 'Q';
    memcpy(input + 1, &wire_length, 4);
    memcpy(input + 5, sql, strlen(sql) + 1);
    input_length = length + 1;
    input_offset = output_length = 0;
    exit_status = -1;
    if (setjmp(exit_target) == 0) {
        exit_armed = 1;
        PostgresMainLoopOnce();
    } else if (exit_status == 100) {
        /* The upstream host handles this synthetic exit, then resumes. */
        PostgresMainLongJmp();
    } else {
        fail("unexpected query exit");
    }
    PostgresSendReadyForQueryIfNecessary();
    pgl_pq_flush();
    exit_armed = 0;
    if (input_offset != input_length) fail("query not fully consumed");
}

int main(int argc, char **argv)
{
    FILE *queries;
    char *line = NULL;
    size_t capacity = 0;
    int index = 0;
    char filename[4096];
    int saved_stdout, transport[2];
    if (argc != 5) fail("usage: embedding-control postgres-path data-dir queries-file output-dir");
    queries = fopen(argv[3], "r");
    if (!queries) fail("open queries");
    /* Upstream's dummy Port uses descriptor 1. Native epoll requires that
     * descriptor to support polling even though remapped recv/send use our
     * callbacks. A real socket descriptor supplies that native host service. */
    saved_stdout = dup(STDOUT_FILENO);
    if (saved_stdout < 0 || socketpair(AF_UNIX, SOCK_STREAM, 0, transport) ||
        dup2(transport[0], STDOUT_FILENO) < 0) fail("native pollable dummy descriptor");
    close(transport[0]);
    /* Single-user startup expects EOF and yields 99 while retaining state. */
    if (!freopen("/dev/null", "r", stdin)) fail("open empty startup input");
    pgl_set_rw_cbs(read_cb, write_cb);
    pgl_setPGliteActive(1);
    exit_status = -1;
    if (setjmp(exit_target) == 0) {
        char *backend_argv[] = {argv[1], "--single", "-D", argv[2],
            "-c", "shared_buffers=16MB", "-c", "max_connections=10",
            "-c", "autovacuum=off", "-c", "log_min_messages=warning", "template1", NULL};
        exit_armed = 1;
        pgl_postgres_main(13, backend_argv);
        fail("backend main unexpectedly returned");
    }
    if (exit_status != 99) fail("startup did not yield 99");
    /* pq_init unconditionally registers postmaster-death polling on Linux.
     * A pipe held open for this test lifetime supplies the liveness handle. */
    if (pipe(postmaster_alive_fds)) fail("native liveness pipe");
    if (setjmp(exit_target) == 0) pgl_startPGlite();
    else fail("pgl_startPGlite failed");
    exit_armed = 0;
    while (getline(&line, &capacity, queries) >= 0) {
        FILE *response;
        size_t length = strlen(line);
        if (length && line[length - 1] == '\n') line[--length] = '\0';
        if (!length) continue;
        execute(line);
        snprintf(filename, sizeof(filename), "%s/%03d.wire", argv[4], index++);
        response = fopen(filename, "wb");
        if (!response || fwrite(output, 1, output_length, response) != output_length || fclose(response))
            fail("save response");
    }
    free(line);
    fclose(queries);
    pgl_setPGliteActive(0);
    exit_status = -1;
    if (setjmp(exit_target) == 0) {
        exit_armed = 1;
        proc_exit(0);
        fail("proc_exit unexpectedly returned");
    }
    if (exit_status != 0) fail("shutdown failed");
    pgl_run_atexit_funcs();
    exit_armed = 0;
    fflush(stdout);
    if (dup2(saved_stdout, STDOUT_FILENO) < 0) fail("restore stdout");
    close(saved_stdout);
    close(transport[1]);
    printf("native embedded queries=%d startup_yield=99 shutdown=0\n", index);
    return 0;
}
