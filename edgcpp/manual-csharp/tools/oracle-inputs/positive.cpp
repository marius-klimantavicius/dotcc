// Authored source; native acceptance baseline for the first semantic slice.
using Number = int;
int add(Number a, int b);
int add(Number a, int b) { return a + b; }
int caller() { int value = 2; { int value = 3; add(value, 4); } return add(value, 1); }
