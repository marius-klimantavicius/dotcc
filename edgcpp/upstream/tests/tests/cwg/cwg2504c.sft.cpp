//options_all:--c++23 -tused -A
struct V { V() = default; V(int); };
struct Q { Q(); };
struct A : virtual V, Q {
  using V::V;
  A() = delete;
};
int bar() { return 42; }
struct B : A {
  B() : A(bar()) {}  // ok
};
struct C : B {};
void foo() { C c; } // bar is not invoked, because the V subobject is not initialized as part of B
