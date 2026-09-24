#ifndef DOTCC_VALKEY_HOST_H
#define DOTCC_VALKEY_HOST_H

/* The core aliases ValkeyModuleString to robj. Only the separate module-facing
 * bridge unit needs the opaque API declarations used by engine_lua.c. */
#ifndef VALKEYMODULE_CORE
typedef struct ValkeyModuleCtx ValkeyModuleCtx;
typedef struct ValkeyModuleString ValkeyModuleString;

int ValkeyModule_OnLoad_lua(ValkeyModuleCtx *ctx, ValkeyModuleString **argv, int argc);
int ValkeyModule_OnUnload_lua(ValkeyModuleCtx *ctx);
#endif

/* Same C_OK/C_ERR convention and output contract as moduleLoadStaticSymbol. */
int valkeyManagedResolveStaticModuleSymbol(void **out, void **handle,
                                         const char *symbol_name, const char *module_name);

#endif
