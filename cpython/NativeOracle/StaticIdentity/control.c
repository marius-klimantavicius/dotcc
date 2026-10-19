#include <Python.h>
#include <stdio.h>
#include <string.h>

int main(void)
{
    for (int cycle = 0; cycle < 2; cycle++) {
        if (Py_TYPE((PyObject *)&PyType_Type) != &PyType_Type ||
            Py_TYPE((PyObject *)&PyLong_Type) != &PyType_Type ||
            Py_TYPE((PyObject *)&PyBool_Type) != &PyType_Type ||
            PyBool_Type.tp_base != &PyLong_Type ||
            Py_TYPE(Py_True) != &PyBool_Type || Py_TYPE(Py_False) != &PyBool_Type ||
            Py_True == Py_False || Py_None == Py_True || Py_None == Py_False ||
            !Py_TYPE(Py_None) || Py_TYPE(Py_None) == &PyLong_Type || Py_TYPE(Py_None) == &PyBool_Type ||
            memcmp(PyLong_Type.tp_name, "int", 4) || memcmp(PyBool_Type.tp_name, "bool", 5))
            return 23;
    }
    puts("static identity: 20 checks, exact names and singleton/type/boolean base graph");
    return 0;
}
