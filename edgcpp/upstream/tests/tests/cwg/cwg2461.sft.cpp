//type:fn
//options_all:--c++20 -tused -A
  template <typename T> struct A {};
  template <typename T> void f() requires (sizeof(A<T>)) {}

//cwg: 2461
//title: Diagnosing non-bool type constraints
//meeting: Virtual 11/20
//edg_status: Passes
