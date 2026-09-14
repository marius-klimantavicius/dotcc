#include <errno.h>
#include <fcntl.h>
/* Native staged oracle only; never included in product or translated inputs. */
int blink_io_terminal(int fd) {
  if (fcntl(fd,F_GETFD)==-1) return -1;
  errno=ENOTTY;return -1;
}
