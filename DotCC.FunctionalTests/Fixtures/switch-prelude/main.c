#include <stdio.h>
struct Pair { int x; int y; };
int effects;
int initialize(void) { ++effects; return 91; }
int prelude(int choice) {
  switch (choice) {
    int skipped = initialize();
    struct Pair pair;
    int values[3] = {initialize(), 2, 3};
    case 1:
      skipped = 7;
      pair.x = 11;
      pair.y = 13;
      values[0] = 17;
      values[1] = 19;
      values[2] = 23;
      return skipped + pair.x + pair.y + values[0] + values[1] + values[2];
    default: return -1;
  }
}
int nested(int choice) {
  switch (choice) {
    case 0: {
      int values[2] = {initialize(), 7};
      case 1:
        values[0] = 42;
        values[1] = 17;
        return values[0] + values[1];
    }
    default: return -1;
  }
}
int prelude_label(int choice) {
  switch (choice) {
    int value = initialize();
    again: value = 31;
    case 1:
      if (choice == 1) { choice = 2; goto again; }
      return value;
    default: return -1;
  }
}
int other_arrays(int choice) {
  switch (choice) {
    struct Pair pairs[2];
    int rows[2][3];
    case 1:
      pairs[1].x = 29;
      rows[1][2] = 31;
      return pairs[1].x + rows[1][2];
    default: return -1;
  }
}
int main(void) {
  int a = prelude(1);
  printf("%d %d\n", a, effects);
  int b = nested(1);
  printf("%d %d\n", b, effects);
  int c = nested(0);
  printf("%d %d\n", c, effects);
  int d = prelude_label(1);
  printf("%d %d\n", d, effects);
  int e = other_arrays(1);
  printf("%d %d\n", e, effects);
  return 0;
}
