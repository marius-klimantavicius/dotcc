/* Actual same-pin lifecycle algorithms. Native --wrap=fork is a rejecting
 * oracle service only; emitted code uses the unchanged shared Libc service. */
#define Py_BUILD_CORE
#include <Python.h>
#include "pycore_runtime.h"
#include <errno.h>
#include <stddef.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

static int released_owners;

#ifdef FORK_PROBE_NATIVE
/* The native executable must never create a process during these controls. */
pid_t __wrap_fork(void) { errno = EPERM; return -1; }
pid_t __wrap_vfork(void) { errno = EPERM; return -1; }
#endif

static PyObject *pair(PyObject *self, PyObject *unused)
{
    PyOS_BeforeFork();
    pid_t pid = fork();
    int saved_errno = errno;
    /* This control deliberately executes the rejected-parent path only. */
    PyOS_AfterFork_Parent();
    if (pid != -1 || saved_errno != EPERM) {
        PyErr_SetString(PyExc_AssertionError, "fork did not reject with EPERM");
        return NULL;
    }
    errno = saved_errno;
    return PyErr_SetFromErrno(PyExc_OSError);
}

static PyObject *state(PyObject *self, PyObject *unused)
{
    /* An actual runtime field, not a mirror layout or a managed registry. */
    return PyLong_FromLong(_PyRuntime.stoptheworld.world_stopped);
}

static PyObject *released(PyObject *self, PyObject *unused)
{
    released_owners++;
    Py_RETURN_NONE;
}

static PyMethodDef methods[] = {
    {"pair", pair, METH_NOARGS, NULL},
    {"state", state, METH_NOARGS, NULL},
    {"released", released, METH_NOARGS, NULL},
    {NULL}
};
static struct PyModuleDef module = {PyModuleDef_HEAD_INIT, "fork_probe", NULL, -1, methods};
static PyObject *PyInit_fork_probe(void) { return PyModule_Create(&module); }

#ifdef FORK_PROBE_SUBPROCESS
PyMODINIT_FUNC PyInit__posixsubprocess(void);
#endif

int main(int argc, char **argv)
{
    if (argc != 3) return 24;
    printf("fork-layout interp=%lu before=%lu parent=%lu child=%lu runtime=%lu\n",
           (unsigned long)sizeof(PyInterpreterState),
           (unsigned long)offsetof(PyInterpreterState, before_forkers),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_parent),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_child),
           (unsigned long)sizeof(_PyRuntimeState));
    if (PyImport_AppendInittab("fork_probe", PyInit_fork_probe) < 0) return 24;
#ifdef FORK_PROBE_SUBPROCESS
    if (PyImport_AppendInittab("_posixsubprocess", PyInit__posixsubprocess) < 0) return 24;
#endif
    FILE *file = fopen(argv[2], "rb");
    if (!file || fseek(file, 0, SEEK_END)) return 24;
    long length = ftell(file);
    if (length < 0 || fseek(file, 0, SEEK_SET)) return 24;
    char *program = malloc((size_t)length + 1);
    if (!program || fread(program, 1, (size_t)length, file) != (size_t)length) return 24;
    program[length] = 0;
    fclose(file);
    for (int cycle = 0; cycle < 2; cycle++) {
        PyConfig config;
        PyConfig_InitIsolatedConfig(&config);
        config.install_signal_handlers = 0;
        config.site_import = 0;
        config.write_bytecode = 0;
        config.module_search_paths_set = 1;
        wchar_t *lib = Py_DecodeLocale(argv[1], NULL);
        if (!lib) return 24;
        PyStatus status = PyWideStringList_Append(&config.module_search_paths, lib);
        PyMem_RawFree(lib);
        if (!PyStatus_Exception(status)) status = Py_InitializeFromConfig(&config);
        PyConfig_Clear(&config);
        if (PyStatus_Exception(status)) return 24;
        PyObject *main = PyImport_AddModule("__main__");
        PyObject *globals = main ? PyModule_GetDict(main) : NULL;
        if (!globals) return 24;
        PyObject *result = PyRun_StringFlags(program, Py_file_input, globals, globals, NULL);
        if (!result) { PyErr_Print(); return 23; }
        Py_DECREF(result);
        if (Py_FinalizeEx() < 0) return 24;
        if (released_owners != (cycle+1)*3) {
            fprintf(stderr, "callback owners released %d expected %d\n", released_owners, (cycle+1)*3);
            return 23;
        }
    }
    free(program);
    puts("fork-hooks-passed cycles=2 child-executions=0 callback-owners-released=6");
    return 0;
}
