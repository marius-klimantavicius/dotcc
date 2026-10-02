//type:fp
//remark:[4.13] Core issue 1579: Return statement and move-conversion
// 11/21/16  [EDGcpfe/17773]
//
// Core issue 1579: Return statement and move-conversion
//
// The front end now implements the resolution of Core issue 1579, which allows a
// returned lvalue to be "moved from" even though the type of the lvalue is of a
// class type different from the return type of the function.
struct A {};
struct B { B(A&&) {} };
B g(A a) {
  return a;  // Previously an error.  Now accepted.
}
