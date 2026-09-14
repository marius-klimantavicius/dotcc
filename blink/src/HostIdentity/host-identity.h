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
#endif
