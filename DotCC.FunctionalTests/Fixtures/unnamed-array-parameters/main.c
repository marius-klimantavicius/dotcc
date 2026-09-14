#include <stdio.h>

/* Reduced from Blink util.h's GetOpt(int, char *const[], const char *).
 * Unnamed array parameters in prototypes adjust to pointers, including an
 * array whose element type is itself a const-qualified pointer.
 */
int second(int []);
int choose(int, char *const[], const char *);
int first(int [2]);

int second(int *p) { return p[1]; }
int choose(int n, char *const *p, const char *q) {
  return n == 1 && p[0][0] == q[0];
}
int first(int p[2]) { return p[0]; }

int main(void) {
  int values[2] = {7, 42};
  char word[2] = {'x', 0};
  char *items[1] = {word};
  int a = second(values);
  int b = choose(1, items, "x");
  int c = first(values);
  printf("second=%d choose=%d first=%d\n", a, b, c);
  return a == 42 && b == 1 && c == 7 ? 0 : 1;
}
