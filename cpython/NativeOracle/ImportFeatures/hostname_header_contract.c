/* Actual upstream layouts plus the same real shared hostname fixture. */
#define main hostname_service_main
#include "hostname_services.c"
#undef main
#define Py_BUILD_CORE
#include <Python.h>
#include "pycore_runtime.h"

int main(int argc, char **argv)
{
    printf("layout %lu %lu %lu %lu %lu %lu %lu %lu\n",
           (unsigned long)sizeof(PyInterpreterState),
           (unsigned long)offsetof(PyInterpreterState, warnings),
           (unsigned long)sizeof(_PyRuntimeState),
           (unsigned long)offsetof(_PyRuntimeState, _main_interpreter),
           (unsigned long)sizeof(wchar_t),
#ifdef HAVE_FORK
           (unsigned long)offsetof(PyInterpreterState, before_forkers),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_parent),
           (unsigned long)offsetof(PyInterpreterState, after_forkers_child)
#else
           0UL, 0UL, 0UL
#endif
    );
    return hostname_service_main(argc, argv);
}
