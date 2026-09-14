#include <stdio.h>
#include <stdarg.h>
#include <string.h>

static int format(char *dst, size_t size, const char *fmt, ...) {
  va_list ap, copy;
  va_start(ap, fmt);
  va_copy(copy, ap);
  int required = vsnprintf(NULL, 0, fmt, copy);
  va_end(copy);
  int result = vsnprintf(dst, size, fmt, ap);
  va_end(ap);
  return required == result ? result : -1000;
}
static int after_one(char *dst, const char *fmt, ...) {
  va_list ap;
  va_start(ap, fmt);
  int ignored = va_arg(ap, int);
  int n = vsnprintf(dst, 64, fmt, ap);
  va_end(ap);
  return n + ignored;
}
int main(void) {
  char b[256], shortbuf[5] = {'!', '!', '!', '!', '!'}, one = '!';
  int n = format(b, sizeof(b), "%+d|%u|%lld|%#08x|%.5u|%hhd|%hu", -17, 4000000000u, -9000000000LL, 42u, 7u, 255, 65537);
  printf("numbers=%s n=%d\n", b, n);
  n = format(b, sizeof(b), "[%*.*s][%.*f][%.*d]%%", -7, 3, "abcdef", 2, 1.25, -1, 12);
  printf("dynamic=%s n=%d\n", b, n);
  n = format(shortbuf, sizeof(shortbuf), "%s-%d", "abcdef", 99);
  printf("truncated=%s n=%d guard=%d\n", shortbuf, n, (int)shortbuf[4]);
  n = format(&one, 0, "abc");
  printf("zero=%d n=%d\n", one, n);
  n = format(&one, 1, "abc");
  printf("one=%d n=%d\n", one, n);
  int count = -1; short narrow = -1; long wide = -1;
  n = format(b, sizeof(b), "ab%nC%hnDE%ln", &count, &narrow, &wide);
  printf("counts=%d,%d,%ld n=%d text=%s\n", count, narrow, wide, n, b);
  n = after_one(b, "%s:%d", 10, "next", 42);
  printf("cursor=%s n=%d\n", b, n);
  n = format(b, sizeof(b), "%s:%c", "\xc3\xa9", 255);
  printf("bytes=%u,%u,%u,%u n=%d\n", (unsigned char)b[0], (unsigned char)b[1], (unsigned char)b[2], (unsigned char)b[3], n);
  return 0;
}
