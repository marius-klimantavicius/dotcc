//type:cp
//options::-DNEG;fn
//options_all:--c++20 -A
//fixing_pr:22113
struct A {
  int a;
  int&& r;
};

int f();
int &&rf();

A a1{1, f()};    // OK, lifetime is extended
A a2(1, f());    // well-formed, but dangling reference
#ifdef NEG
A a3{1.0, 1};    // error: narrowing conversion
#endif
A a4(1.0, 1);    // well-formed, but dangling reference
A a5(1.0, rf()); // OK
