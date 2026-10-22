/* Byte/entry/exit fixture only. This cannot answer PostgreSQL -V, --check,
 * --boot or --single and is never supplied to the original initdb workflow.
 * Link it with the complete actual pglitec and real static host provider. */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "dotcc_static_modules.h"

extern int pgl_atexit(void (*callback)(void));
extern _Noreturn void pgl_exit(int status);
extern void dpg_host_poll_signals(void);
static int initialized = 37;
static const char *cleanup_path;
static int fixture_symbol(void) { return 71; }
static const dpg_static_symbol symbols[] = {{"fixture_symbol", (void *) fixture_symbol}};
static const dpg_static_module module = {1, symbols};
const dpg_static_module *dpg_plpgsql_module(void) { return &module; }
/* Explicit byte-fixture catalogs only, not PostgreSQL module exports. */
static const dpg_static_module second_module = {1, symbols};
const dpg_static_module *dpg_snowball_module(void) { return &second_module; }

static void cleanup(void)
{
    FILE *file = fopen(cleanup_path, "wb");
    if (file == NULL) pgl_exit(81);
    if (fputs("actual fork atexit drained\n", file) == EOF || fclose(file) != 0)
        pgl_exit(82);
}

int main(int argc, char **argv)
{
    void *handle;
    int (*symbol)(void);
    int ch;
    if (argc != 5 || strcmp(argv[1], "--byte-fixture") != 0) return 83;
    if (++initialized != 38) return 84;
    cleanup_path = argv[2];
    if (pgl_atexit(cleanup) != 0) return 85;
    if (setenv("LC_ALL", "fixture-child-locale", 1) != 0 ||
        setenv("PGDATA", "fixture-child-data", 1) != 0) return 90;
    handle = dpg_static_dlopen(argv[3], 0);
    if (handle == NULL) return 86;
    symbol = (int (*)(void)) dpg_static_dlsym(handle, "fixture_symbol");
    if (symbol == NULL || symbol() != 71 || dpg_static_dlclose(handle) != 0) return 87;
    dpg_host_poll_signals();
    while ((ch = fgetc(stdin)) != EOF)
        if (fputc(ch, stdout) == EOF) return 88;
    if (ferror(stdin) || fputs("fixture stderr\n", stderr) == EOF) return 89;
    pgl_exit(atoi(argv[4]));
}
