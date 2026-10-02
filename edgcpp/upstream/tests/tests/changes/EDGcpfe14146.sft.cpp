//type:fp
//options_all:--c++14
//remark:[4.9] C++14: Aggregate class types with nonstatic data member initializers
// 3/5/14   [EDGcpfe/14146]
//
// C++14: Aggregate class types with nonstatic data member initializers
//
// In C++14 mode, aggregate class types are permitted to have nonstatic data
// member initializers.
//
// This relaxation of the class aggregate rules was introduced by the C++
// standards committee's paper N3653.
struct S { int x, y = x; } s = { 11 };  // Initializes s.y to 11 also.
