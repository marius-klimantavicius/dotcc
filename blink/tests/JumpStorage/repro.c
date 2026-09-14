#include <setjmp.h>
#include <stdio.h>
#include <stdlib.h>

struct State { jmp_buf halt; int reached; };

static void Leaf(struct State *state) {
  state->reached = 42;
  longjmp(state->halt, 7);
}
static void Middle(struct State *state) { Leaf(state); }
static void Deep(struct State *state) { Middle(state); }

int main(void) {
  struct State *state = malloc(sizeof(*state));
  if (!state) return 2;
  state->reached = 0;
  if (setjmp(state->halt) == 0) {
    Deep(state);
    return 3;
  }
  printf("reached=%d\n", state->reached);
  int result = state->reached != 42;
  free(state);
  return result;
}
