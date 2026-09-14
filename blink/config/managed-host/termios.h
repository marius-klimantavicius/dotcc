#ifndef BLINK_MANAGED_TERMIOS_H
#define BLINK_MANAGED_TERMIOS_H
#include <sys/types.h>
#include "abi.h"
#include "termios-constants.h"
typedef uint8_t cc_t;
typedef uint32_t tcflag_t;
typedef uint32_t speed_t;
#define termios blink_host_termios
#define tcgetattr blink_host_tcgetattr
#define tcsetattr blink_host_tcsetattr
#define cfgetospeed blink_host_cfgetospeed
#define cfgetispeed blink_host_cfgetispeed
#define cfsetospeed blink_host_cfsetospeed
#define cfsetispeed blink_host_cfsetispeed
#define tcdrain blink_host_tcdrain
#define tcflow blink_host_tcflow
#define tcflush blink_host_tcflush
#define tcsendbreak blink_host_tcsendbreak
#define tcgetpgrp blink_host_tcgetpgrp
#define tcsetpgrp blink_host_tcsetpgrp
#define tcgetsid blink_host_tcgetsid
int tcgetattr(int, struct termios *);
int tcsetattr(int, int, const struct termios *);
speed_t cfgetospeed(const struct termios *);
speed_t cfgetispeed(const struct termios *);
int cfsetospeed(struct termios *, speed_t);
int cfsetispeed(struct termios *, speed_t);
int tcdrain(int);
int tcflow(int, int);
int tcflush(int, int);
int tcsendbreak(int, int);
pid_t tcgetpgrp(int);
int tcsetpgrp(int, pid_t);
pid_t tcgetsid(int);
#endif
