//type:fn
//options_all:--c++20

struct A { int x, y; };
struct B { int y, x; };

void f(A a, int); // #1
void f(B b, ...) = delete; // #2
void g(A a) = delete; // #3
void g(B b) = delete; // #4

void h() {
  f({.x = 1, .y = 2}, 0); // OK; calls #1
  f({.y = 2, .x = 1}, 0); // error: selects #1, initialization of a fails due to non-matching member order
  f({.x = 1, .y = 2, .y = 2}, 0); // error: selects #1, initialization of a fails due to duplicate initializers
  g({.x = 1, .y = 2}); // error: ambiguous between #3 and #4
  g({.x = 1, .y = 2, .y = 2}); // error: ambiguous between #3 and #4
}
