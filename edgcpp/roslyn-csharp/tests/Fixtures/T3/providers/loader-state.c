/* Genuine application input: loader operations and observations stay translated. */
#include <dlfcn.h>
extern int *__errno_location(void);
const char *loader_provider_path;
char *(*loader_error_address)(void) = dlerror;
void *(*loader_symbol_address)(void *, const char *) = dlsym;
void *(*loader_open_address)(const char *, int) = dlopen;
int (*loader_close_address)(void *) = dlclose;
static int observations[17];
void loader_fail_pending(void) { dlopen("/roslynobj-provider-application-missing/app", RTLD_NOW); }
char *loader_read(void) { return dlerror(); }
char *loader_read_indirect(void) { return loader_error_address(); }
int loader_address_identity(void) { return dlerror == loader_error_address; }
int fixture(void) {
    observations[0] = dlerror() == 0;
    observations[1] = dlopen("/roslynobj-provider-application-missing/app", RTLD_NOW) == 0;
    observations[2] = loader_read() != 0;
    observations[3] = loader_read_indirect() == 0;
    loader_fail_pending();
    void *host = loader_open_address("libc.so.6", RTLD_NOW | RTLD_LOCAL);
    observations[4] = host != 0 && loader_read() == 0;
    observations[5] = dlsym(host, "__roslynobj_missing_symbol") == 0;
    observations[6] = dlsym(host, "getpid") != 0;
    observations[7] = loader_read_indirect() == 0;
    void *zero = dlopen(loader_provider_path, RTLD_NOW | RTLD_LOCAL);
    observations[8] = loader_symbol_address(zero, "native_loader_zero") == 0;
    observations[9] = loader_read() == 0;
    dlsym(host, "__roslynobj_missing_symbol");
    observations[10] = loader_read() != 0;
    observations[11] = loader_read() == 0;
    observations[12] = loader_symbol_address(host, "__roslynobj_missing_symbol") == 0 && loader_read_indirect() != 0;
    dlsym(host, "__roslynobj_missing_symbol");
    observations[13] = loader_close_address(host) == 0;
    observations[14] = loader_read() == 0;
    observations[15] = dlclose(zero) == 0;
    *__errno_location() = 3456;
    loader_read();
    observations[16] = *__errno_location() == 3456 && loader_address_identity();
    int result = 0;
    for (int index = 0; index < 17; ++index) result += observations[index];
    return result;
}
int loader_observation_count(void) { return 17; }
int loader_observe(int index) { return observations[index]; }
