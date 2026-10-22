/* Host loader boundary, not PostgreSQL module-management algorithms. Remap
 * only dfmgr's dl* calls to these symbols; do not change its checks/caches. */
#include "dotcc_static_modules.h"
#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>

static char *module_path;
static const dpg_static_module *registered_module;
static int open_count;
static _Thread_local const char *loader_error;

static void *fail(const char *message, int error)
{
    loader_error = message;
    errno = error;
    return NULL;
}

int dpg_static_register(const char *path, const dpg_static_module *module)
{
    size_t i, j;
    char *copy;
    if (registered_module != NULL) { fail("static module already registered", EBUSY); return -1; }
    if (path == NULL || *path == '\0' || module == NULL || module->symbols == NULL || module->count == 0)
    { fail("invalid static module registration", EINVAL); return -1; }
    for (i = 0; i < module->count; i++)
    {
        if (module->symbols[i].name == NULL || *module->symbols[i].name == '\0' || module->symbols[i].address == NULL)
        { fail("invalid static module symbol", EINVAL); return -1; }
        for (j = 0; j < i; j++)
            if (strcmp(module->symbols[j].name, module->symbols[i].name) == 0)
            { fail("duplicate static module symbol", EINVAL); return -1; }
    }
    copy = malloc(strlen(path) + 1);
    if (copy == NULL) { fail("out of memory registering static module", ENOMEM); return -1; }
    strcpy(copy, path);
    module_path = copy;
    registered_module = module;
    open_count = 0;
    return 0;
}

int dpg_static_unregister(void)
{
    if (open_count != 0) { fail("static module is still open", EBUSY); return -1; }
    free(module_path);
    module_path = NULL;
    registered_module = NULL;
    return 0;
}

void *dpg_static_dlopen(const char *path, int flags)
{
    (void) flags; /* Statically linked symbols are already resolved and visible. */
    if (registered_module == NULL || path == NULL || strcmp(path, module_path) != 0)
        return fail("module is not registered as translated code", ENOENT);
    if (open_count == INT_MAX) return fail("too many static module handles", EMFILE);
    open_count++;
    return (void *) registered_module;
}

void *dpg_static_dlsym(void *handle, const char *name)
{
    size_t i;
    if (registered_module == NULL || handle != registered_module || open_count == 0)
        return fail("invalid static module handle", EBADF);
    if (name == NULL) return fail("invalid static module symbol", EINVAL);
    for (i = 0; i < registered_module->count; i++)
        if (strcmp(name, registered_module->symbols[i].name) == 0)
            return registered_module->symbols[i].address;
    return fail("undefined translated module symbol", ENOENT);
}

int dpg_static_dlclose(void *handle)
{
    if (registered_module == NULL || handle != registered_module || open_count == 0)
    { fail("invalid static module handle", EBADF); return -1; }
    open_count--;
    return 0;
}

char *dpg_static_dlerror(void)
{
    const char *result = loader_error;
    loader_error = NULL;
    return (char *) result;
}
