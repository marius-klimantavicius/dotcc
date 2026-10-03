#ifndef EDG_ENVIRONMENT_COMPAT_H
#define EDG_ENVIRONMENT_COMPAT_H
#include <stddef.h>
#include <stdlib.h>
#include <string.h>
#ifdef __cplusplus
extern "C" {
#endif
extern char **environ;
char **__edg_environment_snapshot(void);
void __env_rm_add(char *,char *);
int __putenv(char *,size_t,char *);
#ifdef __cplusplus
}
#endif
#define __environ environ
static inline char *__edg_env_strchrnul(const char *s,int c)
{ while (*s && *s!=c) ++s; return (char *)s; }
#define __strchrnul __edg_env_strchrnul
#endif
