/* Managed-profile C boundary. This unit is translated with --instance-methods
 * alongside the actual server and Lua engine; callback addresses therefore use
 * the same owner/calling convention as their translated consumers. */
#include "valkey_host.h"
#include <stddef.h>
#include <string.h>

int valkeyManagedResolveStaticModuleSymbol(void **out, void **handle,
                                         const char *symbol_name, const char *module_name) {
    if (out) *out = NULL;
    if (handle) *handle = NULL;
    if (!out || !handle || !symbol_name || !module_name) return -1;
    if (strcmp(module_name, "lua") != 0) return -1;
    if (strcmp(symbol_name, "ValkeyModule_OnLoad") == 0) {
        *out = (void *)ValkeyModule_OnLoad_lua;
        return 0;
    }
    if (strcmp(symbol_name, "ValkeyModule_OnUnload") == 0) {
        *out = (void *)ValkeyModule_OnUnload_lua;
        return 0;
    }
    return -1;
}
