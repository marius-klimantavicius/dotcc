//type:fn
//options_all:--c++20 -tused -A
     typedef int I;
     struct S {
       constexpr ~S() { }
     };
     I i;
     S s;
     constexpr int f() {
       i.~I();
       s.~S();
       return 1;
     }
     int a[f()];

//cwg: 2490
//title: Restrictions on destruction in constant expressions
//meeting: Virtual 10/21
//edg_status: Passes
