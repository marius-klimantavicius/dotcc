/* Native-only selected-glibc protocol oracle; not a translated dependency. */
#define _GNU_SOURCE 1
#include <dlfcn.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>

static int error_present(void) { return dlerror() != NULL; }
int main(int count, char **arguments) {
    if (count != 2) return 2;
    dlerror();
    void *host = dlopen("libc.so.6", RTLD_NOW | RTLD_LOCAL);
    if (!host || error_present()) return 3;
    dlopen("/roslynobj-provider-missing/dlopen", RTLD_NOW);
    void *second = dlopen("libc.so.6", RTLD_NOW | RTLD_LOCAL);
    if (!second) return 4;
    int open_after_error = error_present();
    printf("pending-error-after-successful-dlopen=%d\n", open_after_error);
    printf("dlerror-read-clear=%d\n", error_present());
    dlsym(host, "__roslynobj_missing_symbol");
    void *symbol = dlsym(host, "getpid");
    if (!symbol) return 5;
    printf("pending-error-after-successful-dlsym=%d\n", error_present());
    dlsym(host, "__roslynobj_missing_symbol");
    if (dlclose(second) != 0) return 6;
    printf("pending-error-after-successful-dlclose=%d\n", error_present());
    void *zero_provider = dlopen(arguments[1], RTLD_NOW | RTLD_LOCAL);
    if (!zero_provider || error_present()) return 7;
    void *zero = dlsym(zero_provider, "native_loader_zero");
    printf("null-symbol-value=%d\n", zero == NULL);
    printf("null-symbol-error=%d\n", error_present());
    dlsym(host, "__roslynobj_missing_symbol");
    char *message = dlerror();
    if (!message) return 8;
    /* Copy while the native borrowed pointer is valid, before any loader call. */
    char copy[2048];
    size_t length = strlen(message);
    if (length >= sizeof(copy)) return 9;
    memcpy(copy, message, length + 1);
    printf("borrowed-message-copy=%d\n", strstr(copy, "__roslynobj_missing_symbol") != NULL);
    printf("read-clear-next=%d\n", error_present());
    errno = 2721;
    dlerror();
    printf("dlerror-errno=%d\n", errno);
    if (dlclose(zero_provider) != 0 || dlclose(host) != 0) return 10;
    return 0;
}
