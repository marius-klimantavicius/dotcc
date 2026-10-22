#ifndef DOTCC_STATIC_MODULES_H
#define DOTCC_STATIC_MODULES_H
#include <stddef.h>

typedef struct dpg_static_symbol
{
    const char *name;
    void *address;
} dpg_static_symbol;

typedef struct dpg_static_module
{
    size_t count;
    const dpg_static_symbol *symbols;
} dpg_static_module;

/* This bounded provider supports the two required PL/pgSQL and Snowball modules.
 * Actual translated symbols, ABI magic, and initializers stay original PG code.
 * Names/addresses must remain rooted until unregister or program disposal. */
int dpg_static_register(const char *path, const dpg_static_module *module);
int dpg_static_unregister(void);
void *dpg_static_dlopen(const char *path, int flags);
void *dpg_static_dlsym(void *handle, const char *name);
int dpg_static_dlclose(void *handle);
char *dpg_static_dlerror(void);
const dpg_static_module *dpg_plpgsql_module(void);
const dpg_static_module *dpg_snowball_module(void);
#endif
