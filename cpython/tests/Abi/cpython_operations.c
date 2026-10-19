/* Link this test-only unit against real CPython exports, never a substitute.
   Callable from a managed ABI harness after isolated initialization with GIL.
   The native control is built against the ordinary native ABI (wchar_t=4). */
#define PY_SSIZE_T_CLEAN
#include <Python.h>
#include <assert.h>

static PyObject *cp_b_double(PyObject *context, PyObject *argument) {
    (void)context;
    long value = PyLong_AsLong(argument);
    if (value == -1 && PyErr_Occurred()) return NULL;
    return PyLong_FromLong(value * 2);
}

int cp_b_abi_operations(void) {
    assert(Py_IsInitialized());
    PyObject *number = PyLong_FromLong(1000000);
    assert(number != NULL);
    Py_ssize_t references = Py_REFCNT(number);
    Py_INCREF(number);
    assert(Py_REFCNT(number) == references + 1);
    Py_DECREF(number);
    assert(Py_REFCNT(number) == references);
    assert(Py_TYPE(number) == &PyLong_Type);
    assert(Py_TYPE((PyObject *)&PyLong_Type) == &PyType_Type);
    PyObject *repr = Py_TYPE(number)->tp_repr(number);
    assert(repr != NULL && PyUnicode_CompareWithASCIIString(repr, "1000000") == 0);
    Py_DECREF(repr);

    PyObject *none = Py_NewRef(Py_None);
    Py_ssize_t immortal_references = Py_REFCNT(none);
    Py_INCREF(none);
    Py_DECREF(none);
    assert(none == Py_None && Py_REFCNT(none) == immortal_references);
    Py_DECREF(none);

    /* Exercise real build/parse varargs, pointers, default promotions and sizes. */
    PyObject *tuple = Py_BuildValue("(idLs#)", 7, 1.25, (long long)1234567890123LL, "a\0b", (Py_ssize_t)3);
    assert(tuple != NULL);
    int integer = 0;
    double real = 0;
    long long big = 0;
    const char *text = NULL;
    Py_ssize_t length = 0;
    assert(PyArg_ParseTuple(tuple, "idLs#", &integer, &real, &big, &text, &length));
    assert(integer == 7 && real == 1.25 && big == 1234567890123LL);
    assert(length == 3 && text[0] == 'a' && text[1] == 0 && text[2] == 'b');
    Py_DECREF(tuple);

    static PyMethodDef method = { "double", cp_b_double, METH_O, NULL };
    PyObject *function = PyCFunction_NewEx(&method, NULL, NULL);
    PyObject *argument = PyLong_FromLong(21);
    assert(function != NULL && argument != NULL);
    PyObject *args[] = { argument };
    PyObject *result = PyObject_Vectorcall(function, args, 1, NULL);
    assert(result != NULL && PyLong_AsLong(result) == 42 && !PyErr_Occurred());
    Py_DECREF(result); Py_DECREF(argument); Py_DECREF(function);
    Py_DECREF(number);
    return 0;
}

#ifdef CP_B_ABI_MAIN
#include <stdio.h>
int main(int argc, char **argv) {
    if (argc != 2) { fputs("expected explicit stdlib directory\n", stderr); return 2; }
    for (int cycle = 0; cycle < 2; ++cycle) {
        PyPreConfig pre;
        PyPreConfig_InitIsolatedConfig(&pre);
        pre.utf8_mode = 1;
        PyStatus status = Py_PreInitialize(&pre);
        if (PyStatus_Exception(status)) { fprintf(stderr, "preinit: %s\n", status.err_msg); return 3; }
        PyConfig config;
        PyConfig_InitIsolatedConfig(&config);
        config.install_signal_handlers = 0;
        config.site_import = 0;
        config.write_bytecode = 0;
        config.module_search_paths_set = 1;
        wchar_t *library = Py_DecodeLocale(argv[1], NULL);
        if (!library) { PyConfig_Clear(&config); return 4; }
        status = PyWideStringList_Append(&config.module_search_paths, library);
        PyMem_RawFree(library);
        if (!PyStatus_Exception(status)) status = Py_InitializeFromConfig(&config);
        PyConfig_Clear(&config);
        if (PyStatus_Exception(status)) { fprintf(stderr, "init: %s\n", status.err_msg); return 5; }
        assert(cp_b_abi_operations() == 0);
        if (Py_FinalizeEx() != 0) return 6;
    }
    puts("{\"cycles\":2,\"refcounts\":true,\"immortals\":true,\"slots\":true,\"varargs\":true,\"vectorcall\":true}");
    return 0;
}
#endif
