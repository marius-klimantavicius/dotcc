/* Declaration-only managed host-service boundary for EDG self-translation. */
#ifndef EDG_SELF_UNISTD_H
#define EDG_SELF_UNISTD_H
#include <sys/types.h>
#include <stdio.h>
#ifdef __cplusplus
extern "C" {
#endif
char *getcwd(char *, size_t);
int chdir(const char *);
int unlink(const char *);
pid_t getpid(void);
int isatty(int);
int mkstemp(char *);
#ifdef __cplusplus
}
#endif
#endif
