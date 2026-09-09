#include <stdio.h>
typedef int (*Callback)(void *, int);
struct Inner { void *context; Callback callback; };
struct Outer { int guard; struct Inner nested; };
struct Outer state;
static int biased(void *context, int value) { return *(int *)context + value; }
static void configure(void *context, Callback callback) {
  void **slot = &state.nested.context;
  Callback *handler = &state.nested.callback;
  *slot = context;
  *handler = callback;
}
int main(void) {
  int bias = 35;
  struct Outer *alias = &state;
  state.guard = 17;
  configure(&bias, biased);
  printf("%d %d %d\n", state.nested.callback(state.nested.context, 7),
         &state.nested.context == &alias->nested.context, state.guard);
  *(&state.nested.context) = 0;
  *(&state.nested.callback) = 0;
  printf("%d %d\n", state.nested.context == 0, state.nested.callback == 0);
  return 0;
}
