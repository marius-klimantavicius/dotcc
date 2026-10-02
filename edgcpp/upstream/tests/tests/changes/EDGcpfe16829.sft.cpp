//type:fp
//options_all:--c++11
//remark:[4.11] Operator functions with incomplete return types and decltype
// 2/4/16   [EDGcpfe/16829]
//
// Operator functions with incomplete return types and decltype
//
// In C++11, the return type in a function call need not be complete if the
// function call appears as the immediate operand of a decltype construct (see
// the Changes entry of 7/19/13 for EDGcpfe/11740,EDGcpfe/14277).  The front end
// failed to implement that rule for calls resulting from overloaded operator
// uses.
//
// This is now fixed.
struct I;
struct C {};
I operator*(C);
decltype(*C()) g();  // Previously an error.  Now okay.
