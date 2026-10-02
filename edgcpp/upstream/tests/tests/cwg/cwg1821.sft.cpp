//type:fn
//options_all:--c++20 -tused -A
  struct A {
    template<class> struct B {void f();};
    template<> void B<int>::f() {h()};
  };

//cwg: 1821
//title: Qualified redeclarations in a class member-specification
//meeting: Virtual 11/20*
//edg_status: Passes
