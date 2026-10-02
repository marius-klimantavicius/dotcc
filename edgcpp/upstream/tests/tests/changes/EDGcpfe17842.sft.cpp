//type:fp
//options_all:--c++11
//remark:[4.13] constexpr member initializers referring to previously-initialized members
// 12/21/16 [EDGcpfe/17842]
//
// constexpr member initializers referring to previously-initialized members
//
// The front end issued a spurious diagnostic when a constexpr constructor
// initializes one member using the value of a previously-initialized member.
// This is now fixed.
struct S {
  int a = 2;
  int b = a + 1;
  constexpr S() {}
};
constexpr S d; // Previously a spurious error
