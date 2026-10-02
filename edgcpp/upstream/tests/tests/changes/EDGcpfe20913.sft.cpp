//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Initializing aggregates from a parenthesized list of values
// 6/10/19  [EDGcpfe/20913]
//
// C++20: Initializing aggregates from a parenthesized list of values
//
// C++20 allows initializing aggregate types from a parenthesized list of values
// when no constructor applies (Committee paper P0960R3).  Unlike
// braced-initialization, narrowing of values is allowed.
//
// Note that array types can be initialized this way as well, including in new
// expressions (in conjunction with Committee paper P1009R2; see EDGcpfe/20914):
struct A {
  int x;
  double y;
};
A a1(1.0, 1); // Well-formed, no narrowing conversion diagnostic
A a2{1.0, 1}; // Narrowing conversion diagnostic
