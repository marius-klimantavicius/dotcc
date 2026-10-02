//type:fp
//options_all:--c++17
//remark:[6.5] Constexpr static data members
// 6/19/23  [EDGcpfe/26437]
//
// Constexpr static data members
//
// C++17 (via WG21 paper P0386R2) made constexpr static data members
// implicitly inline and removed the previous requirement that such members
// have an explicit initializer in the class definition; a constexpr default
// constructor in the type of such members is sufficient.  The front end now
// follows these rules in C++17 and later modes.
struct S {
  constexpr S() : i(0) { }
  int i;
};
struct X {
  constexpr static S s;   // Previously an error, now accepted in C++17 mode
};
