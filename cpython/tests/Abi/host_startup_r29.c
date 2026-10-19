#include <Python.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "dotcc_cpython_host.h"

/* Link the actual authored owning Host, before or after, to the exact-pin
 * native archive. It is an oracle only and never a delivered native engine. */
static int callbacks;
static int callback(int64_t token, int64_t argument, int64_t *result,
                    char *error, size_t capacity)
{
    (void)error; (void)capacity;
    if (token != 29) return -1;
    ++callbacks; *result = argument * 2; return 0;
}

int main(int argc, char **argv)
{
    if (argc != 5 || strncmp(Py_GetVersion(), "3.13.15 ", 8)) return 20;
    FILE *file = fopen(argv[3], "rb");
    if (!file || fseek(file, 0, SEEK_END)) return 21;
    long length = ftell(file);
    if (length < 0 || fseek(file, 0, SEEK_SET)) return 21;
    char *script = malloc((size_t)length + 1);
    if (!script || fread(script, 1, (size_t)length, file) != (size_t)length) return 21;
    fclose(file); script[length] = 0;
    for (int cycle = 0; cycle < 2; ++cycle) {
        void *ctx = dcp_create();
        if (!ctx) return 22;
        int status = dcp_initialize(ctx, argv[1], argv[2]);
        if (status) {
            printf("native owning PyStatus kind=%d function=%s message=%s clean=%d\n", status,
                   dcp_status_function(ctx) ? dcp_status_function(ctx) : "<null>",
                   dcp_status_message(ctx) ? dcp_status_message(ctx) : "<null>", dcp_failure_clean(ctx));
            free(script);
            return status == 1 && dcp_status_message(ctx) ? 23 : 26;
        }
        if (!strcmp(argv[4], "expect-error")) return 27;
        dcp_restore(ctx);
        dcp_register_callback(ctx, "host", "double", 29, callback);
        int result = dcp_execute(ctx, script, "frozen_startup_checks_r29.py");
        size_t size = 0;
        const char *output = dcp_output(ctx, 0, &size);
        if (output) fwrite(output, 1, size, stdout);
        if (!result) result = dcp_execute(ctx, "import host\nassert host.double(21) == 42\n", "callback_r29.py");
        if (result) { dcp_take_error(ctx); printf("native owning Python error: %s\n", dcp_error_message(ctx)); }
        dcp_join_workers(ctx);
        int finalized = dcp_finalize(ctx);
        int clean = dcp_failure_clean(ctx);
        dcp_destroy(ctx);
        printf("native owning cycle=%d finalized=%d clean=%d initialized=%d\n", cycle, finalized, clean, Py_IsInitialized());
        if (result || finalized || !clean || Py_IsInitialized()) return 24;
    }
    free(script);
    printf("native owning lifecycles=2 callbacks=%d version=%.7s\n", callbacks, Py_GetVersion());
    return callbacks == 2 ? 0 : 25;
}
