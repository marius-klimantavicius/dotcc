//type:fn
//options_all:--c++20 -tused -A 
struct B {
  void f(char);
  enum E { e };
  union { int x; };
};

struct C {
  int f();
};

struct D : B {
  using B::f;                   // OK: B is a base of D
  using B::e;                   // OK: e is an enumerator of base B
  using B::x;                   // OK: x is a union member of base B
  using C::f;                   // error: C isn't a base of D
};

template <typename... bases>
struct X : bases... {
  using bases::f...;
};

X<B, C> x;                      // OK: B::f and C::f named
