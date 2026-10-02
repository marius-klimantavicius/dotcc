//type:fp
//options_all:--c++20 -tused -A
  template<typename T> struct A { typename T::error e; };
  template<typename T> struct B { };
  B<A<void>> b1, &b2 = (b1 = b1);

//cwg: 2007
//title: Argument-dependent lookup for operator=
//meeting: Virtual 11/20*
//edg_status: Passes
