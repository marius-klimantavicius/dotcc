#include <stdio.h>
struct Outer {
  struct Inner { int value; } *item;
  union Number { long integer; double real; } *number;
};
int main(void) {
  struct Inner inner = { 42 };
  union Number number = { 17 };
  struct Outer outer = { &inner, &number };
  printf("%d %ld\n", outer.item->value, outer.number->integer);
  return 0;
}
