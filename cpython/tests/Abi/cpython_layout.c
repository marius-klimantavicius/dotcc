/* Compile against ACTUAL pin headers and the selected generated pyconfig.h.
   Linux native has wchar_t=4. DotCC must report wchar_t=2. Do not normalize the
   difference, copy this layout to managed structs, or link -fshort-wchar against
   a normal native libpython. No interpreter/library implementation is needed. */
#include <Python.h>
#include <stddef.h>
#include <stdio.h>

int main(void) {
    printf("{\"pointer\":%zu,\"long\":%zu,\"wchar_t\":%zu,\"config_wchar_t\":%d,\"wchar_unsigned\":%d,"
           "\"Py_ssize_t\":%zu,\"Py_hash_t\":%zu,\"Py_UCS1\":%zu,\"Py_UCS2\":%zu,\"Py_UCS4\":%zu,"
           "\"PyObject\":%zu,\"PyObject_align\":%zu,\"ob_refcnt\":%zu,\"ob_type\":%zu,"
           "\"PyVarObject\":%zu,\"ob_size\":%zu,\"PyTypeObject\":%zu,\"tp_repr\":%zu,\"tp_call\":%zu,"
           "\"PyStatus\":%zu,\"status_func\":%zu,\"status_err_msg\":%zu,\"status_exitcode\":%zu,"
           "\"PyWideStringList\":%zu,\"PyPreConfig\":%zu,\"PyConfig\":%zu,\"config_search_paths\":%zu}\n",
           sizeof(void *), sizeof(long), sizeof(wchar_t), SIZEOF_WCHAR_T, (wchar_t)-1 > 0,
           sizeof(Py_ssize_t), sizeof(Py_hash_t), sizeof(Py_UCS1), sizeof(Py_UCS2), sizeof(Py_UCS4),
           sizeof(PyObject), _Alignof(PyObject), offsetof(PyObject, ob_refcnt), offsetof(PyObject, ob_type),
           sizeof(PyVarObject), offsetof(PyVarObject, ob_size), sizeof(PyTypeObject),
           offsetof(PyTypeObject, tp_repr), offsetof(PyTypeObject, tp_call), sizeof(PyStatus),
           offsetof(PyStatus, func), offsetof(PyStatus, err_msg), offsetof(PyStatus, exitcode),
           sizeof(PyWideStringList), sizeof(PyPreConfig), sizeof(PyConfig), offsetof(PyConfig, module_search_paths));
    return 0;
}
