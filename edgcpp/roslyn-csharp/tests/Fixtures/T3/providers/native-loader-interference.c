/* Checker-only native raw-loader interference; no application binding/body. */
#include <dlfcn.h>
void native_loader_raw_fail(void) { (void)dlopen("/roslynobj-provider-raw-missing/raw", RTLD_NOW); }
