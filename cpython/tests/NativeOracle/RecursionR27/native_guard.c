/* Same-pin native control, never a product implementation. Link the existing
   exact-pin libpython; no replacement guard, threshold or extension module. */
#include <Python.h>
#include <stdio.h>

static int guard_counters(void)
{
    PyThreadState *state = PyThreadState_Get();
    int initial = state->c_recursion_remaining;
    int headroom = state->recursion_headroom;
    int entered = 0, failed_remaining = 0, ok = initial > 0 && headroom == 0;
    while (entered <= initial + 1) {
        if (Py_EnterRecursiveCall(" in A27 guard counter probe")) break;
        entered++;
    }
    failed_remaining = state->c_recursion_remaining;
    ok = ok && entered == initial + 1 && PyErr_ExceptionMatches(PyExc_RecursionError)
        && state->recursion_headroom == headroom;
    PyErr_Clear();
    ok = ok && Py_EnterRecursiveCall(" in A27 repeated overflow") != 0;
    ok = ok && state->c_recursion_remaining == failed_remaining
        && PyErr_ExceptionMatches(PyExc_RecursionError);
    PyErr_Clear();
    for (int i = 0; i < entered; i++) Py_LeaveRecursiveCall();
    ok = ok && state->c_recursion_remaining == initial && state->recursion_headroom == headroom;
    if (Py_EnterRecursiveCall(" after A27 guard unwind")) ok = 0;
    else Py_LeaveRecursiveCall();
    ok = ok && state->c_recursion_remaining == initial && PyErr_Occurred() == NULL;
    printf("initial=%d entered=%d failed_remaining=%d headroom=%d restored=%d ok=%d\n",
        initial, entered, failed_remaining, headroom, state->c_recursion_remaining, ok);
    return ok ? 0 : 21;
}

int main(int argc, char **argv)
{
    if (argc != 2) return 24;
    PyConfig config;
    PyConfig_InitIsolatedConfig(&config);
    config.install_signal_handlers = 0;
    config.site_import = 0;
    config.write_bytecode = 0;
    PyStatus status = PyConfig_SetBytesString(&config, &config.stdlib_dir, argv[1]);
    if (!PyStatus_Exception(status)) status = PyWideStringList_Append(&config.module_search_paths, config.stdlib_dir);
    config.module_search_paths_set = 1;
    if (!PyStatus_Exception(status)) status = Py_InitializeFromConfig(&config);
    PyConfig_Clear(&config);
    if (PyStatus_Exception(status)) return 24;
    int result = guard_counters();
    if (Py_FinalizeEx() < 0) return 25;
    return result;
}
