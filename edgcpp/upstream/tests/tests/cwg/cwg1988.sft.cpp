//type:fp
//options_all:--c++17 -tused -A
struct A {
    int m;
  };

  struct B {
    int m;
  };

  template<typename T>
  struct C : A, T {
    int f() { return this->m; }
     int g() { return m; }      // finds A::m in the template definition context
    };// finds A::m in the template definition context

  template int C<B>::g();      // OK: transformation to class member access syntax

//cwg: 1988
//title: Ambiguity between dependent and non-dependent bases in implicit member access
//meeting: Lenexa 5/15
//edg_status: Passes
