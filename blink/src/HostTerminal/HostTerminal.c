/* All descriptor payloads are deliberately unused: the private descriptor
 * model has no terminal devices. Ordinary calls still evaluate each argument. */
#include <sys/ioctl.h>
#include <termios.h>
int blink_io_terminal(int);
int ioctl(int fd, unsigned long request, ...) {
  (void)request;
  return blink_io_terminal(fd);
}
int tcgetattr(int fd, struct termios *record) {
  (void)record; return blink_io_terminal(fd);
}
int tcsetattr(int fd, int action, const struct termios *record) {
  (void)action; (void)record; return blink_io_terminal(fd);
}
int tcdrain(int fd) { return blink_io_terminal(fd); }
int tcflow(int fd, int action) { (void)action; return blink_io_terminal(fd); }
int tcflush(int fd, int queue) { (void)queue; return blink_io_terminal(fd); }
int tcsendbreak(int fd, int duration) { (void)duration; return blink_io_terminal(fd); }
pid_t tcgetpgrp(int fd) { return blink_io_terminal(fd); }
int tcsetpgrp(int fd, pid_t group) { (void)group; return blink_io_terminal(fd); }
pid_t tcgetsid(int fd) { return blink_io_terminal(fd); }
