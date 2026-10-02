//type:cp
//options:--c++11:--c++17:--c++11 --g++:--c++17 --g++:--c++11 --clang:--c++17 --clang:--microsoft_version 1900;fn
//options_all:--diag_suppress 177

struct A {
  A() : mem(42) {}
  int mem;
};

struct B {
  union {
    int i;
    A a{};
  };
};

void f() {
  B b1;
  B b2{};
}
