#include <stdio.h>
int effects;
int condition(void) { ++effects; return effects < 3; }
int while_literal(int n) { while (1) { if (n-- > 0) continue; return 11; } }
int do_literal(int n) { do { if (n-- > 0) continue; return 13; } while ((1)); }
int for_literal(int n) { for (; 1; --n) { if (n > 0) continue; return 17; } }
enum Flag { Forever = 1 };
int enum_literal(void) { while (Forever) { return 19; } }
int zero_loops(void) {
  int result = 0;
  while (0) ++result;
  do { result += 23; } while (0);
  for (; 0;) ++result;
  while (~0xffffffffu) ++result;
  return result;
}
int effectful(void) { int result = 0; while (condition()) ++result; return result; }
int main(void) {
  printf("%d %d %d %d %d\n", while_literal(3), do_literal(3), for_literal(3), enum_literal(), zero_loops());
  int value = effectful();
  printf("%d %d\n", value, effects);
  return 0;
}
