//type:fn
//options_all:--c++17 -tused -A
 struct A { virtual void f(); };
  struct B { friend void A::f() final; };

//cwg: 1983
//title: Inappropriate use of virt-specifier
//meeting: Jacksonville 2/18
//edg_status: Passes
