/* Valid C setjmp controlling expressions. All state modified between setjmp
 * and longjmp lives in allocated storage, avoiding indeterminate auto locals. */
#include <setjmp.h>
#include <stdio.h>
#include <stdlib.h>

struct State { jmp_buf outer; jmp_buf inner; int visits; int hits; };
static struct State *state;
static struct State *Select(void) { ++state->visits; return state; }
#ifdef DOTCC_TEST_GC
/* Optional managed test instrumentation; no guest or product host service. */
extern void DotccTestCollect(void);
#endif
static void Leaf(jmp_buf target, int value) {
#ifdef DOTCC_TEST_GC
  DotccTestCollect();
#endif
  longjmp(target, value);
}
static void Middle(jmp_buf target, int value) { Leaf(target, value); }
static void Deep(jmp_buf target, int value) { Middle(target, value); }

int main(void) {
  state = malloc(sizeof(*state));
  if (!state) return 2;
  state->visits = 0;
  state->hits = 0;
  /* Zero normalizes to one, and the outer target bypasses the inner handler.
   * Select must run exactly once, not again in an exception filter. */
  switch (setjmp(Select()->outer)) {
    case 0:
      if (setjmp(state->inner) == 0) Deep(state->outer, 0);
      else return 3;
      return 4;
    case 1: ++state->hits; break;
    default: return 5;
  }
  /* Repeated visits rearm the buffer, and capture preserves nonzero values. */
  for (int repeat = 0; repeat != 2; ++repeat) {
    switch (setjmp(state->inner)) {
      case 0: Deep(state->inner, repeat + 2); return 6;
      case 2: state->hits += 2; break;
      case 3: state->hits += 3; break;
      default: return 7;
    }
  }
  /* Two simultaneously active sites may use the same buffer; its newest
   * identity targets the inner site, never the older surrounding handler. */
  if (setjmp(state->inner) == 0) {
    if (setjmp(state->inner) == 0) Deep(state->inner, 9);
    else state->hits += 4;
  } else return 8;
  printf("visits=%d hits=%d\n", state->visits, state->hits);
  int result = state->visits != 1 || state->hits != 10;
  free(state);
  return result;
}
