//type:fp
//options_all:--c++11
//remark:[4.13] constexpr pointer-to-member expressions
// 11/29/16 [EDGcpfe/17491]
//
// constexpr pointer-to-member expressions
//
// The front end previously issued a spurious "must have a constant value"
// diagnostic for a pointer-to-member expression in which the object expression
// is a constexpr value. This is now fixed.
struct A {
 int i;
 static constexpr int A::*p = &A::i;
};
constexpr A a = { 42 };
static_assert(a.*A::p == 42, "");  // Previously an error, now accepted
