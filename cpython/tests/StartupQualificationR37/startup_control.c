/* The actual authored Host linked to the same-pin archive is a CONTROL ONLY. */
#include <Python.h>
#include <stdio.h>
#include <string.h>
#include "dotcc_cpython_host.h"

int main(int argc, char **argv)
{
    if (argc != 3) return 20;
    const int invalid[4][2] = {{-1, 1}, {2, 1}, {0, -1}, {0, 2}};
    void *context = dcp_create();
    if (!context) return 21;
    for (int i = 0; i < 4; ++i) {
        if (dcp_initialize_configured(context, argv[1], argv[2], invalid[i][0], invalid[i][1]) != 1 ||
            dcp_status_kind(context) != 1 || !dcp_status_message(context) ||
            !dcp_failure_clean(context) || Py_IsInitialized()) return 22;
    }
    dcp_destroy(context);
    /* Defaults, all explicit boolean combinations, then defaults again.
     * Reinitialization uses real CPython config, never altered sys.flags. */
    for (int cycle = 0; cycle < 6; ++cycle) {
        int write = cycle == 0 || cycle == 5 ? 0 : (cycle - 1) / 2;
        int utf8 = cycle == 0 || cycle == 5 ? 1 : (cycle - 1) % 2;
        context = dcp_create();
        if (!context) return 23;
        int status = cycle == 0 || cycle == 5 ? dcp_initialize(context, argv[1], argv[2]) :
            dcp_initialize_configured(context, argv[1], argv[2], write, utf8);
        if (status) {
            printf("startup error: %s clean=%d\n", dcp_status_message(context), dcp_failure_clean(context));
            return 24;
        }
        dcp_restore(context);
        char script[2048];
        snprintf(script, sizeof(script),
            "import sys, os, importlib.util\n"
            "assert sys.flags.isolated == 1 and sys.flags.ignore_environment == 1\n"
            "assert sys.flags.utf8_mode == %d\n"
            "assert sys.dont_write_bytecode is %s\n"
            "assert 'poison-r37' not in sys.path\n"
            "name = 'startup_fixture_%d'\n"
            "module = __import__(name)\n"
            "assert module.VALUE == 37\n"
            "cache = importlib.util.cache_from_source(module.__file__)\n"
            "assert os.path.exists(cache) is %s, cache\n"
            "print('startup cycle=%d write=%d utf8=%d cache=verified')\n",
            utf8, write ? "False" : "True", cycle, write ? "True" : "False", cycle, write, utf8);
        int result = dcp_execute(context, script, "startup_options_r37.py");
        if (result) {
            dcp_take_error(context);
            printf("Python error: %s\n%s\n", dcp_error_message(context), dcp_error_traceback(context));
        }
        size_t length = 0;
        const char *output = dcp_output(context, 0, &length);
        if (output) fwrite(output, 1, length, stdout);
        dcp_join_workers(context);
        int finalized = dcp_finalize(context);
        int clean = dcp_failure_clean(context);
        dcp_destroy(context);
        if (result || finalized || !clean || Py_IsInitialized()) return 25;
    }
    printf("invalidBooleanControls=4 clean=true generations=6 defaultsPreserved=true\n");
    return 0;
}
