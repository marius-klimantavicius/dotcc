#include <stdio.h>
#include <stdarg.h>
static void inspect(int unused, ...) {
  va_list ap;
  int a, b, c, d, e, floating;
  unsigned int wide;
  va_start(ap, unused);
  a = va_arg(ap, int);
  b = va_arg(ap, int);
  c = va_arg(ap, int);
  d = va_arg(ap, int);
  e = va_arg(ap, int);
  floating = (int)(va_arg(ap, double) * 10);
  wide = va_arg(ap, unsigned int);
  va_end(ap);
  printf("%d %d %d %d %d %d %u\n", a, b, c, d, e, floating, wide);
}
int main(void) {
  signed char a = -7;
  short b = -123;
  unsigned char c = 255;
  unsigned short values[2] = {65535, 42};
  int index = 0;
  _Bool truth = 1;
  float fraction = 1.5f;
  inspect(0, a, b, c, values[index++], truth, fraction, 4294967295u);
  printf("%d\n", index);
  return 0;
}
