//type:fp
//options_all:--c++11
//remark:[4.11] Member function calls with incomplete return types and decltype
// 10/30/15 [EDGcpfe/16599]
//
// Member function calls with incomplete return types and decltype
//
// In C++11, the return type in a function call need not be complete if the
// function call appears as the immediate operand of a decltype construct (see
// the Changes entry of 7/19/13 for EDGcpfe/11740,EDGcpfe/14277).  The front end
// failed to implement that rule for member function calls.
//
// This is now fixed.
struct R;
struct S {
  R f();
} s;
decltype(s.f()) g();  // Previously an error; now okay.
