/* Host loader boundary, not PostgreSQL module-management algorithms. Remap
 * only dfmgr's dl* calls to these symbols; do not change its checks/caches. */
#include "dotcc_static_modules.h"
#include <errno.h>
#include <stdlib.h>
#include <string.h>
#include <limits.h>

typedef struct dpg_static_registration
{
    char *path;
    const dpg_static_module *module;
    int open_count;
} dpg_static_registration;

/* Only the two reviewed original bootstrap modules are required. */
#define DPG_STATIC_MODULE_LIMIT 2
static dpg_static_registration registrations[DPG_STATIC_MODULE_LIMIT];
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
    dpg_static_registration *entry = NULL;
    if (path == NULL || *path == '\0' || module == NULL || module->symbols == NULL || module->count == 0)
    { fail("invalid static module registration", EINVAL); return -1; }
    for (i = 0; i < DPG_STATIC_MODULE_LIMIT; i++)
    {
        if (registrations[i].path != NULL && strcmp(path, registrations[i].path) == 0)
        { fail("static module path already registered", EEXIST); return -1; }
        if (entry == NULL && registrations[i].path == NULL) entry = &registrations[i];
    }
    if (entry == NULL) { fail("static module registration limit reached", ENOSPC); return -1; }
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
    entry->path = copy;
    entry->module = module;
    entry->open_count = 0;
    return 0;
}

int dpg_static_unregister(void)
{
    size_t i;
    for (i = 0; i < DPG_STATIC_MODULE_LIMIT; i++)
        if (registrations[i].open_count != 0)
        { fail("static module is still open", EBUSY); return -1; }
    for (i = 0; i < DPG_STATIC_MODULE_LIMIT; i++)
    {
        free(registrations[i].path);
        registrations[i].path = NULL;
        registrations[i].module = NULL;
    }
    return 0;
}

void *dpg_static_dlopen(const char *path, int flags)
{
    (void) flags; /* Statically linked symbols are already resolved and visible. */
    size_t i;
    if (path != NULL)
        for (i = 0; i < DPG_STATIC_MODULE_LIMIT; i++)
        {
            dpg_static_registration *entry = &registrations[i];
            if (entry->path == NULL || strcmp(path, entry->path) != 0) continue;
            if (entry->open_count == INT_MAX) return fail("too many static module handles", EMFILE);
            entry->open_count++;
            return entry;
        }
    return fail("module is not registered as translated code", ENOENT);
}

static dpg_static_registration *find_handle(void *handle)
{
    size_t i;
    for (i = 0; i < DPG_STATIC_MODULE_LIMIT; i++)
        if (handle == &registrations[i] && registrations[i].module != NULL && registrations[i].open_count != 0)
            return &registrations[i];
    return NULL;
}

void *dpg_static_dlsym(void *handle, const char *name)
{
    size_t i;
    dpg_static_registration *entry = find_handle(handle);
    if (entry == NULL)
        return fail("invalid static module handle", EBADF);
    if (name == NULL) return fail("invalid static module symbol", EINVAL);
    for (i = 0; i < entry->module->count; i++)
        if (strcmp(name, entry->module->symbols[i].name) == 0)
            return entry->module->symbols[i].address;
    return fail("undefined translated module symbol", ENOENT);
}

int dpg_static_dlclose(void *handle)
{
    dpg_static_registration *entry = find_handle(handle);
    if (entry == NULL)
    { fail("invalid static module handle", EBADF); return -1; }
    entry->open_count--;
    return 0;
}

char *dpg_static_dlerror(void)
{
    const char *result = loader_error;
    loader_error = NULL;
    return (char *) result;
}
