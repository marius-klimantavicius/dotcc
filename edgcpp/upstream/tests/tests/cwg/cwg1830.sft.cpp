//type:fn
//options_all:--c++14 -tused -A
     struct A {
          virtual virtual void f() = 0;
     };

//cwg: 1830
//title: Repeated specifiers
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
