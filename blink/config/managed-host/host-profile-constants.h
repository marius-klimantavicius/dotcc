#ifndef BLINK_HOST_PROFILE_CONSTANTS_H
#define BLINK_HOST_PROFILE_CONSTANTS_H
/* Linux LP64 host tokens used by unchanged syscall translation. These values
 * identify requests; they do not advertise support for their operations. */
#ifndef AT_SYMLINK_FOLLOW
#define AT_SYMLINK_FOLLOW 1024
#endif
#ifndef UTIME_NOW
#define UTIME_NOW 1073741823L
#endif
#ifndef UTIME_OMIT
#define UTIME_OMIT 1073741822L
#endif
#ifndef IPPROTO_RAW
#define IPPROTO_RAW 255
#endif
#ifndef IPPROTO_IPV6
#define IPPROTO_IPV6 41
#endif
#ifndef CLOCK_PROCESS_CPUTIME_ID
#define CLOCK_PROCESS_CPUTIME_ID 2
#endif
#ifndef CLOCK_THREAD_CPUTIME_ID
#define CLOCK_THREAD_CPUTIME_ID 3
#endif
#endif
