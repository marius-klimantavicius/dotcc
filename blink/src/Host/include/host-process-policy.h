#ifndef BLINK_HOST_PROCESS_POLICY_H
#define BLINK_HOST_PROCESS_POLICY_H
#include <stdint.h>
#include <unistd.h>
#include <sys/wait.h>
#define fork blink_host_fork
#define execv blink_host_execv
#define execve blink_host_execve
#define execvp blink_host_execvp
#define waitpid blink_host_waitpid
#define setuid blink_host_setuid
#define seteuid blink_host_seteuid
#define setgid blink_host_setgid
#define setegid blink_host_setegid
#define setpgid blink_host_setpgid
#define setsid blink_host_setsid
int fork(void);
int execv(const char *, char *const []);
int execve(const char *, char *const [], char *const []);
int execvp(const char *, char *const []);
int waitpid(int, int *, int);
int setuid(uint32_t);
int seteuid(uint32_t);
int setgid(uint32_t);
int setegid(uint32_t);
int setpgid(int, int);
int setsid(void);
#endif
