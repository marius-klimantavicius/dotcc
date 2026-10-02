//type:fp
//options_all:--c++17
//remark:[5.0] C++17: Class template argument deduction
// 8/8/18   [EDGcpfe/17697,EDGcpfe/18314]
//
// C++17: Class template argument deduction
//
// In C++17 mode, the front end now implements "class template argument
// deduction": A feature that permits the deduction of class template arguments
// from certain initializers.
//
// This feature was added to C++17 through the standardization committee's paper
// number P0091r3 (with additional provisions added through P0620r0).
template<typename T> struct S {
  S(T);
};
S s = 42;  // Same as "S<int> s = 42".
