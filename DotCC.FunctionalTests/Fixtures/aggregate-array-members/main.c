#include <stdio.h>

typedef int (*Callback)(int);
struct Counters { int nowValue[10]; int mxValue[10]; };
struct Pair { int values[3]; };
struct Payload {
  int values[3];
  double matrix[2][2];
  struct Pair pairs[2];
  int *pointers[2];
  Callback callbacks[2];
  int tail;
};

static int basis = 40;
static int plus_two(int value) { return value + 2; }
static struct Counters counters = {{0,}, {0,}};
static struct Payload global = {
  {1, 2}, {{3}, {4, 5}}, {{{6}}, {{7, 8}}},
  {&basis}, {plus_two}, 9
};
static int evaluations = 0;
static int value(int input) { evaluations++; return input; }
static int sum(struct Pair pair) {
  return pair.values[0] + pair.values[1] + pair.values[2];
}
static int persistent(void) {
  static struct Pair pair = {{10, 20}};
  pair.values[2]++;
  return sum(pair);
}

int main(void) {
  struct Payload empty = {0};
  struct Payload local = {
    {value(10), value(20)}, {{1}, {2}}, {{{30}}, {{40}}},
    {&basis}, {plus_two}, 50
  };
  struct Pair copy = (struct Pair){{value(5), 6}};
  int compound = sum((struct Pair){{11, 12}});
  printf("%d %d %d %d %d %d\n", global.values[0], global.values[2],
    (int)global.matrix[0][1], (int)global.matrix[1][1],
    global.pairs[0].values[0], global.pairs[1].values[2]);
  printf("%d %d %d %d\n", *global.pointers[0], global.pointers[1] == 0,
    global.callbacks[0](40), global.callbacks[1] == 0);
  printf("%d %d %d %d %d %d\n", local.values[0] + local.values[1],
    local.values[2], (int)local.matrix[1][1], local.pairs[1].values[0],
    local.callbacks[0](*local.pointers[0]), local.tail);
  printf("%d %d %d %d %d\n", sum(copy), compound, evaluations,
    counters.nowValue[9], counters.mxValue[0]);
  printf("%d\n", persistent());
  printf("%d\n", persistent());
  printf("%d %d %d\n", empty.values[2], empty.pointers[1] == 0, empty.callbacks[1] == 0);
  return 0;
}
