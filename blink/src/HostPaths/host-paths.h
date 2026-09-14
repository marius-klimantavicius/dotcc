#ifndef BLINK_CAMPAIGN_HOST_PATHS_H
#define BLINK_CAMPAIGN_HOST_PATHS_H
#include <stddef.h>
#include <stdlib.h>
#include <unistd.h>
#define getcwd blink_host_getcwd
#define chdir blink_host_chdir
#define fchdir blink_host_fchdir
#define realpath blink_host_realpath
char *getcwd(char *, size_t);
int chdir(const char *);
int fchdir(int);
char *realpath(const char *, char *);
#endif
