#include <stdio.h>
/* These belong to an excluded profile and deliberately have no definitions. */
extern int disabled_function(int);
extern int disabled_table[4];
extern void disabled_callback(int (*)(int));
static int calls;
static int effect(void) { ++calls; return 1; }
static int live(int value) { return value + 1; }
static int label_entry(void) {
  int value = 1;
  goto entry;
  if (0) { entry: value = 42; }
  return value;
}
static int case_entry(int choice) {
  int value = 0;
  switch (choice) {
    case 0:
      if (0) { case 1: value = 42; }
      break;
  }
  return value;
}
int check(void) {
  calls = 0;
  int value = 0;
  if (0 && effect()) {
    disabled_callback(disabled_function);
    value = disabled_table[disabled_function(0)];
  } else {
    int (*run)(int) = live;
    value = run(20);
  }
  if (1 || disabled_function(0)) value += 10;
  else disabled_callback(disabled_function);
  if (!(0 ? effect() : 1)) value += disabled_table[0];
  if (0 ? effect() : (1 && !0)) value += 10;
  /* An unknown left operand must still execute even when the right operand
   * determines the result. A discarded comma operand must not disappear. */
  if (effect() && 0) value = 0;
  if (effect() || 1) ++value;
  if ((effect(), 0)) value = 0;
  /* Preserve label-containing branches, even when no current caller enters. */
  if (0) {
  retained_label:
    if (++value < 3) goto retained_label;
  }
  return value == 42 && calls == 3 && label_entry() == 42 &&
         case_entry(0) == 0 && case_entry(1) == 42 ? 42 : -1;
}
int main(void) { printf("%d\n", check()); return 0; }
