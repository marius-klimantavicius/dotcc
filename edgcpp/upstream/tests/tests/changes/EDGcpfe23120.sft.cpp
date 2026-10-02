//type:fp
//options_all:--c++20
//remark:[6.2] Improvements to parenthesized aggregate initializers
// 12/17/20 [EDGcpfe/23120]
//
// Improvements to parenthesized aggregate initializers
//
// In C++20, aggregates can be initialized from a parenthesized list of values
// (see the entry for EDGcpfe/20913).  The standardization committee's paper
// P1975R0 made a few changes to that, enabling
// an aggregate type whose first element matches the value.
struct X { int i; char *pc = nullptr; };
X x1 = X(1, nullptr);      // Enabled by the changes for EDGcpfe/20913.
X x2 = X(1);               // Now also accepted in C++20 mode.
X x3 = (X)(1);             // Ditto.
X x4 = static_cast<X>(1);  // Ditto.
