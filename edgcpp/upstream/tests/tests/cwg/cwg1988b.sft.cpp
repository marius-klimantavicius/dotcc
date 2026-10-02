//type:fn
//options_all:--c++17 -tused -A
//
struct A {
    int m;
  };

  struct B {
    int m;
  };

  template<typename T>
  struct C : A, T {
    int f() { return this->m; }// finds A::m in the template definition context
    int g() { return m; }      // finds A::m in the template definition context
  };

  template int C<B>::f();      // error: finds both A::m and B::m
