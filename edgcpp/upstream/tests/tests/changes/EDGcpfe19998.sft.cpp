//type:fp
//options_all:--c++20
//remark:[5.1] C++20: Default member initializers for bit fields
// 8/23/18  [EDGcpfe/19998]
//
// C++20: Default member initializers for bit fields
//
// In C++20 mode, the front end now accepts default member initializers for bit
// fields.
//
// This feature was added to the working paper for the next standard through
// paper number P0683R1.
struct S {
  int i: 7 = 42;  // Error in C++17 and earlier modes; okay in C++20 mode.
};
