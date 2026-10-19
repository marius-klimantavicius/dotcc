/* Novel native CONTROL consumer: dynamic helper visibility is necessary for
 * exact upstream unittest cases. Reuse the unchanged authored Host/archive. */
#include <Python.h>
#include <locale.h>
#include <stdio.h>
#include "dotcc_cpython_host.h"

int main(int argc, char **argv)
{
    if (argc != 4) return 20;
    printf("native configured Host BEFORE preinit locale=%s\n", setlocale(LC_CTYPE, NULL));
    void *context = dcp_create();
    if (!context || dcp_initialize_configured(context, argv[1], argv[2], 1, 0)) return 21;
    printf("native configured Host AFTER preinit locale=%s\n", setlocale(LC_CTYPE, NULL));
    dcp_restore(context);
    char script[4096];
    snprintf(script, sizeof(script),
        "import sys\nassert sys.flags.utf8_mode == 0 and not sys.dont_write_bytecode\n"
        "assert sys.flags.isolated and sys.flags.ignore_environment\n"
        "sys.path.append('%s/lib-dynload')\nimport locale_probe\nlocale_probe.run_probe('%s')\n", argv[1], argv[3]);
    int result = dcp_execute(context, script, "native_configured_locale_r40.py");
    if (result) {
        dcp_take_error(context);
        printf("Python error: %s\n%s\n", dcp_error_message(context), dcp_error_traceback(context));
    }
    size_t length = 0;
    const char *output = dcp_output(context, 0, &length);
    if (output) fwrite(output, 1, length, stdout);
    dcp_join_workers(context);
    int final = dcp_finalize(context);
    dcp_destroy(context);
    return result || final ? 22 : 0;
}
