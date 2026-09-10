typedef int (*Callback)(int);
static int same(int value) { return value + 1; }
Callback other_same(void) { return same; }
