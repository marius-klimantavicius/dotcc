//type:fn
//options:--c++11;cp:--c++17;cp:--c++11 --g++:--c++17 --g++:--c++11 --clang:--c++17 --clang:--microsoft_version 1900
//options_all:--diag_suppress 177

struct A {
  A() : mem(42) {}
  int mem;
};

struct B {
  union {
    A a;
    int i = 1;
  };
};

void f() {
  B b1;
  B b2{};
}
