//type:cp
//options:--c++11:--c++17

struct Base {};
struct B :Base {
  int type : 8;
};
struct C : B {
  static void foo() {};
};
static_assert(__is_trivial(C), "Assert failure");
