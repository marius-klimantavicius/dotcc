#include <stdio.h>
#include <stddef.h>
typedef struct Outer {
  union { long integer; double real; } value;
  int (*callback)(int);
} Outer;
static int plus_two(int value) { return value + 2; }
int main(void) {
  Outer outer;
  outer.value.integer = 40;
  outer.callback = plus_two;
  printf("%d %lu\n", outer.callback(outer.value.integer), offsetof(Outer, callback));
  return 0;
}
