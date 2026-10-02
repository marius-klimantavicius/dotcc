//type:fn
//options_all:--c++17 -tused -A
  struct A {
    A() = default;          // OK
    A(int v) : v(v) { }     // OK
    const int& v = 42;      // OK
  };
  A a1;                     // error: ill-formed binding of temporary to reference
  A a2(1);                  // OK, unfortunately
