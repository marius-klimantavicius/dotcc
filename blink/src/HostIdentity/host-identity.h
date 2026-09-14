#ifndef BLINK_CAMPAIGN_HOST_IDENTITY_H
#define BLINK_CAMPAIGN_HOST_IDENTITY_H
#include <stdint.h>
#include <unistd.h>
#define getpid blink_host_getpid
#define getppid blink_host_getppid
#define getuid blink_host_getuid
#define geteuid blink_host_geteuid
#define getgid blink_host_getgid
#define getegid blink_host_getegid
int32_t getpid(void);
int32_t getppid(void);
uint32_t getuid(void);
uint32_t geteuid(void);
uint32_t getgid(void);
uint32_t getegid(void);
/* Native-measured selector values for this virtual host contract. */
#define _SC_CLK_TCK 2
#define _SC_NGROUPS_MAX 3
#define _SC_PAGESIZE 30
#define sysconf blink_host_sysconf
#define getgroups blink_host_getgroups
#define getresuid blink_host_getresuid
#define getresgid blink_host_getresgid
#define getpgid blink_host_getpgid
#define getsid blink_host_getsid
#define gethostname blink_host_gethostname
long sysconf(int);
int getgroups(int, uint32_t *);
int getresuid(uint32_t *, uint32_t *, uint32_t *);
int getresgid(uint32_t *, uint32_t *, uint32_t *);
int getpgid(int);
int getsid(int);
int gethostname(char *, unsigned long);
#endif
