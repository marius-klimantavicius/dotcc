#include <stdio.h>
#include <threads.h>

unsigned long long *get_tls(void) {
  _Thread_local static unsigned long long values[2];
  return values;
}
static int *marker(void) {
  static _Thread_local int value;
  return &value;
}
static int *echo(void) {
  _Thread_local static int value = 0;
  return &value;
}
void set_tls(unsigned long long value) {
  unsigned long long *p = get_tls();
  p[0] = value; p[1] = value + 1;
  *marker() = (int)value; *echo() = (int)value + 1;
}
int check_tls(unsigned long long value) {
  unsigned long long *p = get_tls();
  return p[0] == value && p[1] == value + 1 &&
    *marker() == (int)value && *echo() == (int)value + 1;
}
static int worker(void *argument) {
  unsigned long long *p = get_tls();
  if (p[0] || p[1] || *marker() || *echo()) return -1;
  unsigned long long value = *(unsigned long long *)argument;
  set_tls(value);
  return check_tls(value) ? (int)(p[0] + p[1]) : -2;
}
int main(void) {
  thrd_t a, b; unsigned long long first = 10, second = 20;
  set_tls(3);
  if (thrd_create(&a, worker, &first) || thrd_create(&b, worker, &second)) return 1;
  int x, y; thrd_join(a, &x); thrd_join(b, &y);
  printf("tls=%d,%d,%llu\n", x, y, get_tls()[0] + get_tls()[1]);
  return x == 21 && y == 41 && check_tls(3) ? 0 : 2;
}
