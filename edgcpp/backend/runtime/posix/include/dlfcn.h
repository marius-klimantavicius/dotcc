/* Declaration surface only: dynamic native loading is not a portable binding.
   Unreferenced inline wrappers can parse; actual calls remain unresolved. */
#ifndef EDG_POSIX_DLFCN_H
#define EDG_POSIX_DLFCN_H
#define RTLD_LAZY 1
#define RTLD_NOW 2
#define RTLD_LOCAL 0
#define RTLD_GLOBAL 256
#ifdef __cplusplus
extern "C" {
#endif
void *dlopen(const char *, int);
void *dlsym(void *, const char *);
int dlclose(void *);
char *dlerror(void);
#ifdef __cplusplus
}
#endif
#endif
