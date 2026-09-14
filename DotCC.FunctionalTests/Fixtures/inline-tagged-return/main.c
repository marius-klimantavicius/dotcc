#include <stdio.h>
struct Node { int value; };
union Box { int value; };
enum Choice { choice = 4 };
static inline struct Node *node(struct Node *value) { return value; }
static inline const struct Node *constant(struct Node *value) { return value; }
static inline union Box box(int value) { union Box b; b.value=value; return b; }
static inline enum Choice select(void) { return choice; }
int main(void) {
  struct Node n = {9}; union Box b = box(7);
  printf("%d %d %d %d\n",node(&n)->value,constant(&n)->value,b.value,(int)select());
  return 0;
}
