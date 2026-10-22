/* Native oracle setup only: permit upstream initdb's helper subprocess calls.
 * This is not a delivered product initialization path. */
#include <stdlib.h>
#include <sys/types.h>

extern void pgl_set_system_fn(ssize_t (*fn)(const char *));
extern void pgl_run_atexit_funcs(void);

static ssize_t native_system(const char *command) { return system(command); }

__attribute__((constructor)) static void install_native_setup_host(void)
{
    pgl_set_system_fn(native_system);
    atexit(pgl_run_atexit_funcs);
}
