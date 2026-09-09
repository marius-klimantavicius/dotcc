#include <stdio.h>
struct Module { int (*find)(void (**callback)(int)); };
static int result;
static void callback(int value) { result = value + 3; }
static int find(void (**output)(int)) { *output = callback; return 1; }
static void invoke(void (**output)(int)) { (*output)(39); }
int main(void) {
  struct Module module = { find };
  void (*fn)(int) = 0;
  int found = module.find(&fn);
  invoke(&fn);
  printf("%d %d\n", found, result);
  return 0;
}
