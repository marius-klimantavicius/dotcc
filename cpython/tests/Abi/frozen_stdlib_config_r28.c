#include <Python.h>
#include <stdio.h>
#include <stdlib.h>

/* Native embedding control: the real initialization APIs and frozen finder.
 * No interpreter algorithm, module implementation, or upstream test is replaced.
 */
int main(int argc, char **argv)
{
    if (argc != 4) return 20;
    PyPreConfig pre;
    PyPreConfig_InitIsolatedConfig(&pre);
    pre.utf8_mode = 1;
    PyStatus status = Py_PreInitialize(&pre);
    if (PyStatus_Exception(status)) return 21;
    PyConfig config;
    PyConfig_InitIsolatedConfig(&config);
    config.install_signal_handlers = 0;
    config.site_import = 0;
    config.parse_argv = 0;
    config.write_bytecode = 0;
    config.module_search_paths_set = 1;
    status = PyConfig_SetBytesString(&config, &config.program_name, "dotcc-cpython");
    wchar_t *stdlib = Py_DecodeLocale(argv[1], NULL);
    if (!stdlib || PyStatus_Exception(status)) return 22;
    status = PyWideStringList_Append(&config.module_search_paths, stdlib);
    if (!PyStatus_Exception(status) && atoi(argv[2]))
        status = PyConfig_SetString(&config, &config.stdlib_dir, stdlib);
    PyMem_RawFree(stdlib);
    if (!PyStatus_Exception(status)) status = Py_InitializeFromConfig(&config);
    PyConfig_Clear(&config);
    if (PyStatus_Exception(status)) return 23;
    int code = PyRun_SimpleString(
        "import sys, os, _imp, importlib.machinery, json\n"
        "_imp._override_frozen_modules_for_tests(1)\n"
        "spec = importlib.machinery.FrozenImporter.find_spec('__hello__')\n"
        "expected = os.path.join(sys.path[0], '__hello__.py')\n"
        "print(json.dumps({'stdlib_dir': sys._stdlib_dir, 'filename': spec.loader_state.filename, 'expected': expected, 'utf8_mode': sys.flags.utf8_mode}), flush=True)\n"
        "assert spec.loader_state.filename == expected\n");
    int finalized = Py_FinalizeEx();
    return code ? 24 : finalized ? 25 : 0;
}
