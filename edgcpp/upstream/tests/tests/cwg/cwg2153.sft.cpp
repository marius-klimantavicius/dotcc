//type:fn
//options_all:--c++17 -tused -A
//
  struct A { virtual int f(); };
  struct B { friend int f() = 0; };

//cwg: 2153
//title: pure-specifier in friend declaration
//meeting: Jacksonville 2/16
//edg_status: Passes
