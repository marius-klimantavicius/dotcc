#ifndef DOTCC_VALKEY_HOST_H
#define DOTCC_VALKEY_HOST_H

/* Module-facing declarations for the authored C# static-Lua resolver.
 * The core exposes the same objects through its robj alias. */
#ifndef VALKEYMODULE_CORE
typedef struct ValkeyModuleCtx ValkeyModuleCtx;
typedef struct serverObject ValkeyModuleString;

int ValkeyModule_OnLoad_lua(ValkeyModuleCtx *ctx, ValkeyModuleString **argv, int argc);
int ValkeyModule_OnUnload_lua(ValkeyModuleCtx *ctx);
#endif

/* Same C_OK/C_ERR convention and output contract as moduleLoadStaticSymbol. */
int valkeyManagedResolveStaticModuleSymbol(void **out, void **handle,
                                         const char *symbol_name, const char *module_name);

/* All lifecycle calls run serially on the owning executor with its libc context
 * bound. The owner selects its working directory before Start. A failed startup
 * or bound termination must call Cleanup before disposing that runtime owner. */
int valkeyManagedStart(const char *options);
int valkeyManagedProcessEvents(void);
int valkeyManagedStop(int flags);
int valkeyManagedCleanup(int abandon);
int valkeyManagedPort(void);
int valkeyManagedState(void); /* 0=new, 1=starting, 2=ready, 3=stopped, 4=cleaned */
const char *valkeyManagedLastError(void);

/* Hash-checked upstream adaptation hooks, not public command implementations. */
struct client;
int valkeyManagedCommandAllowed(struct client *c);
int valkeyManagedConfigAllowed(const char *name, const char *value, int startup);
void valkeyManagedShutdownCompleted(void);
int bioManagedStop(int abandon);
void valkeyManagedBioQueueCreated(void *queue);
void valkeyManagedBioWorkerStarted(void *queue, long thread);
int valkeyManagedBioJobAllowed(void);
int valkeyManagedBioShouldStop(void *job);
/* Read the owning io_threads.c state without relying on internal linker names. */
int valkeyManagedIoThreadsInitialized(void);
void *valkeyManagedIoSharedOutbox(void);

#endif
