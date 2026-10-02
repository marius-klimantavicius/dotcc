//type:fp
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

  template int C<B>::g();      // OK: transformation to class member access syntax
                               // does not occur in the template definition context; see 11.3.2 [class.mfct.non-static]
