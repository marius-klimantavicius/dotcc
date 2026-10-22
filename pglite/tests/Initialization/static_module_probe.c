/* This fixture exercises a host symbol registry, never a PostgreSQL engine. */
#include "dotcc_static_modules.h"
#include <errno.h>
#include <string.h>

static int twice(int value) { return value * 2; }
static const dpg_static_symbol exports[] = {{"twice", (void *) twice}};
static const dpg_static_module fixture = {1, exports};

int dpg_static_register_fixture(void) { return dpg_static_register("/private/translated-module", &fixture); }
void *dpg_static_open_fixture(void) { return dpg_static_dlopen("/private/translated-module", 2); }

int dpg_static_module_probe(void)
{
    void *handle, *second;
    int (*function)(int);
    char path[] = "/private/translated-module";
    if (dpg_static_register(path, &fixture) != 0) return 1;
    path[1] = 'X'; /* Registry must retain its own stable path copy. */
    if (dpg_static_dlopen(path, 2) != NULL || errno != ENOENT) return 2;
    if (dpg_static_dlerror() == NULL || dpg_static_dlerror() != NULL) return 3;
    handle = dpg_static_dlopen("/private/translated-module", 2);
    if (handle == NULL) return 4;
    if (dpg_static_dlsym((void *)1, "twice") != NULL || errno != EBADF) return 5;
    if (dpg_static_dlsym(handle, "missing") != NULL || errno != ENOENT) return 6;
    function = (int (*)(int)) dpg_static_dlsym(handle, "twice");
    if (function == NULL || function(21) != 42) return 7;
    if (dpg_static_unregister() != -1 || errno != EBUSY) return 8;
    second = dpg_static_dlopen("/private/translated-module", 2);
    if (second != handle || dpg_static_dlclose(second) != 0) return 9;
    if (dpg_static_dlsym(handle, "twice") == NULL) return 10;
    if (dpg_static_dlclose(handle) != 0) return 11;
    if (dpg_static_dlclose(handle) != -1 || errno != EBADF) return 12;
    if (dpg_static_unregister() != 0) return 13;
    if (dpg_static_dlopen("/native/plpgsql.so", 2) != NULL || errno != ENOENT) return 14;
    if (dpg_static_register(NULL, &fixture) != -1 || errno != EINVAL) return 15;
    return 0;
}

#ifdef DPG_NATIVE_MAIN
int main(void) { return dpg_static_module_probe(); }
#endif
