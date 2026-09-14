/* Storage-only operations for the native-measured Linux LP64 termios profile.
 * No descriptor access or device operation. See HOST-TERMINAL.md for the
 * native full-record comparison and explicit input-speed-zero convention. */
#include <errno.h>
#include <termios.h>
#define HOST_BAUD_BITS 0x100fu
#define HOST_INPUT_ZERO 0x80000000u
static int ValidSpeed(speed_t speed) { return (speed & ~HOST_BAUD_BITS) == 0; }
speed_t cfgetospeed(const struct termios *record) {
  if (!record) { errno=EFAULT; return (speed_t)-1; }
  return record->c_cflag & HOST_BAUD_BITS;
}
speed_t cfgetispeed(const struct termios *record) {
  if (!record) { errno=EFAULT; return (speed_t)-1; }
  return record->c_iflag & HOST_INPUT_ZERO ? 0 : record->c_cflag & HOST_BAUD_BITS;
}
int cfsetospeed(struct termios *record, speed_t speed) {
  if (!record) { errno=EFAULT; return -1; }
  if (!ValidSpeed(speed)) { errno=EINVAL; return -1; }
  record->c_ospeed=speed;
  record->c_cflag=(record->c_cflag & ~HOST_BAUD_BITS) | speed;
  return 0;
}
int cfsetispeed(struct termios *record, speed_t speed) {
  if (!record) { errno=EFAULT; return -1; }
  if (!ValidSpeed(speed)) { errno=EINVAL; return -1; }
  record->c_ispeed=speed;
  if (!speed) record->c_iflag |= HOST_INPUT_ZERO;
  else {
    record->c_iflag &= ~HOST_INPUT_ZERO;
    record->c_cflag=(record->c_cflag & ~HOST_BAUD_BITS) | speed;
  }
  return 0;
}
