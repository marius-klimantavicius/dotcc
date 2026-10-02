//type:fp
//options_all:--c++20 -tused -A 
struct A {
  friend void f();    // #1
};
struct B {
  friend void f() {}  // corresponds to, and defines, #1
};

//cwg: 2165
//title: Namespaces, declarative regions, and translation units
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23846
