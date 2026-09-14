/* Definitions for the campaign fcntl.h host names. Arguments are evaluated by
 * ordinary C calls; only commands with an integer argument perform va_arg.
 * Unsupported pointer/locking commands do not inspect a fabricated argument. */
#include <stdarg.h>
#include <fcntl.h>

int blink_io_open_at(int, const char *, int);
int blink_io_control(int, int, int);

int fcntl(int descriptor, int command, ...) {
  int argument = 0;
  if (command == F_DUPFD || command == F_DUPFD_CLOEXEC ||
      command == F_SETFD || command == F_SETFL) {
    va_list arguments;
    va_start(arguments, command);
    argument = va_arg(arguments, int);
    va_end(arguments);
  }
  return blink_io_control(descriptor, command, argument);
}

int open(const char *path, int flags, ...) {
  return blink_io_open_at(AT_FDCWD, path, flags);
}

int openat(int directory, const char *path, int flags, ...) {
  return blink_io_open_at(directory, path, flags);
}
