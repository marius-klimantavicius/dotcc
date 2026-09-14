#include <stddef.h>
#include <stdio.h>
#include <sys/ioctl.h>
#define OFFSET(f) printf("winsize." #f " %zu\n", offsetof(struct winsize, f))
#define CONSTANT(n) printf(#n " %lu\n", (unsigned long)(n))
int main(void) {
  printf("winsize.size %zu\nwinsize.alignment %zu\n", sizeof(struct winsize), _Alignof(struct winsize));
  OFFSET(ws_row);
  OFFSET(ws_col);
  OFFSET(ws_xpixel);
  OFFSET(ws_ypixel);
  CONSTANT(TIOCGWINSZ);
  CONSTANT(TIOCSWINSZ);
  CONSTANT(FIONREAD);
  CONSTANT(TIOCOUTQ);
  CONSTANT(TIOCSTI);
  return 0;
}
