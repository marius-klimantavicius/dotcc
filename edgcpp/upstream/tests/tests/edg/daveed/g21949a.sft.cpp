//remark:Folding local initializers
//options:--c++11 --g++;fp

enum E {e};
constexpr E f() { return e; }
template <class> struct A {
  A() {
    const E g = f();
  }
};
struct B {
  A<char> a;
  B() : a() {}
};
