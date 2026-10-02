//remark:Partial specialization and deducibility
//options:--c++17;fp

  template<auto, auto> struct S;
  template<typename X, typename Y, X X::*x, Y Y::*y> class S<x, y> {};
