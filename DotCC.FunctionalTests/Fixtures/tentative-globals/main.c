#include <stdio.h>
int value;
extern int value;
int *pointer;
int **pointer_address(void) { return &pointer; }
int value = 17;
int value;
int *pointer = &value;
extern int *pointer;
static int private_value;
static int private_value = 9;
extern int private_value;
extern int extern_initialized = 7;
int extern_initialized;
int read_early(void) { return *pointer; }
int zero;
extern int zero;
int zero;
typedef int (*Callback)(int);
typedef int (*SameCallback)(int);
int increment(int x) { return x + 1; }
Callback callback;
extern SameCallback callback;
Callback callback = increment;
SameCallback callback;
int main(void) {
  int **saved = pointer_address();
  printf("%d %d %d %d %d\n", value, read_early(), private_value, extern_initialized, zero);
  **saved = 42;
  printf("%d %d %d\n", value, *pointer, saved == &pointer);
  printf("%d\n", callback(41));
  return 0;
}
