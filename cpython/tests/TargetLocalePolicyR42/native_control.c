/* Native CONTROL ONLY. Unchanged Host and complete feature-configured archive.
 * No target implementation or substitute conversion algorithm lives here. */
#define Py_BUILD_CORE
#include <Python.h>
#include "internal/pycore_fileutils.h"
#include <errno.h>
#include <locale.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <wchar.h>
#include "dotcc_cpython_host.h"

#if !defined(_Py_FORCE_UTF8_LOCALE) || !defined(_Py_FORCE_UTF8_FS_ENCODING)
#error "Whole native control must select the actual CPython feature"
#endif

static int preinit_boundaries(void)
{
    const char *input = "\xc3\xa9\xe2\x82\xac\xf4\x8f\xbf\xbf";
    const char *locales[] = {"C", "C.UTF-8"};
    int checks = 0;
    for (int locale = 0; locale < 2; locale++) {
        if (!setlocale(LC_CTYPE, locales[locale])) return 1;
        errno = 0;
        wchar_t lower[8];
        size_t n = mbstowcs(lower, input, 8);
        if (locale == 0) {
            if (n != (size_t)-1 || errno != EILSEQ) return 2;
        } else if (n != 3 || lower[2] != 0x10ffff) return 3;
        checks++;
        for (int current = 0; current < 2; current++) {
            wchar_t *wide = NULL;
            size_t length = 0, position = 0;
            const char *reason = NULL;
            char *encoded = NULL;
            if (_Py_DecodeLocaleEx(input, &wide, &length, &reason, current, _Py_ERROR_STRICT)) return 4;
            if (length != 3 || wide[0] != 0xe9 || wide[1] != 0x20ac || wide[2] != 0x10ffff || wide[3]) return 5;
            if (_Py_EncodeLocaleEx(wide, &encoded, &position, &reason, current, _Py_ERROR_STRICT)) return 6;
            if (strcmp(encoded, input)) return 7;
            PyMem_RawFree(wide);
            PyMem_RawFree(encoded);
            checks++;
        }
    }
    printf("Native wchar=%zu preinit locale/fs UTF8=%d lower-C-ASCII-preserved=1\n", sizeof(wchar_t), checks);
    return 0;
}

int main(int argc, char **argv)
{
    if (argc != 6) return 20;
    int mode = atoi(argv[4]);
    int control = preinit_boundaries();
    if (control) return 30 + control;
    if (!setlocale(LC_CTYPE, argv[5])) return 21;
    void *context = dcp_create();
    if (!context || dcp_initialize_configured(context, argv[1], argv[2], 1, mode)) return 22;
    dcp_restore(context);
    char script[8192];
    snprintf(script, sizeof(script),
        "import sys\nassert sys.flags.utf8_mode == %d and not sys.dont_write_bytecode\n"
        "assert sys.flags.isolated and sys.flags.ignore_environment\n"
        "sys.path.append('%s/lib-dynload')\nimport locale_probe\n"
        "assert locale_probe.run_probe('%s') == 0\n", mode, argv[1], argv[3]);
    int result = dcp_execute(context, script, "native_target_locale_r42.py");
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
    return result || final ? 23 : 0;
}
