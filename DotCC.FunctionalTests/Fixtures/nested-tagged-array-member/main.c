#include <stdio.h>
struct Outer {
  struct Inner { int value; } items[2];
  union Number { long integer; double real; } numbers[2];
};
int main(void) {
  struct Outer outer;
  outer.items[0].value = 19;
  outer.items[1].value = 23;
  outer.numbers[1].integer = 17;
  printf("%d %ld\n", outer.items[0].value + outer.items[1].value, outer.numbers[1].integer);
  return 0;
}
