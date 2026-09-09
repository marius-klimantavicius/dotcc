#include <stdio.h>
int effects;
int initialize(void) { ++effects; return 91; }
void fill(double *destination, int value) { *destination = value; }
int named(int choice) {
  switch (choice) {
    case 0: {
      int *pointer = 0;
      double value = initialize();
      int cells[2] = {initialize(), 7};
      shared:
      pointer = cells;
      fill(&value, 31 + choice);
      pointer[0] = 11;
      pointer[1] = 13;
      return (int)value + pointer[0] + pointer[1];
    }
    case 1: goto shared;
    default: return -1;
  }
}
int shared_fallthrough(int choice) {
  int total = 0;
  switch (choice) {
    case 0: {
      int value = initialize();
      common: value = 17;
      total += value;
    }
    case 1: total += 19; break;
    case 2: goto common;
  }
  return total;
}
int continuing(int choice) {
  int total = 0;
  for (int index = 0; index < 4; ++index) {
    switch (choice) {
      case 0: {
        int value = initialize();
        repeat: value = index;
        if (index < 2) continue;
        total += value;
        break;
      }
      case 1: goto repeat;
    }
  }
  return total;
}
int main(void) {
  int a = named(1);
  printf("%d %d\n", a, effects);
  int b = named(0);
  printf("%d %d\n", b, effects);
  int c = shared_fallthrough(2);
  printf("%d %d\n", c, effects);
  int d = shared_fallthrough(0);
  printf("%d %d\n", d, effects);
  int e = continuing(1);
  printf("%d %d\n", e, effects);
  int f = continuing(0);
  printf("%d %d\n", f, effects);
  return 0;
}
