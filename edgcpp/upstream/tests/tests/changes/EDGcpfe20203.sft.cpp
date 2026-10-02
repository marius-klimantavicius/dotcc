//type:fn
//options_all:--c++14
//remark:[6.7] Diagnosing "mutable mutable" in lambda expressions
// 2/22/24  [EDGcpfe/20203]
//
// Diagnosing "mutable mutable" in lambda expressions
//
// The changes for EDGcpfe/17690,EDGcpfe/17995 introduced a regression in version
// 4.14 of the front end causing it to no longer diagnose a repeated "mutable"
// specifier in a lambda expression.
//
// That is now fixed.
auto lm = []() mutable mutable { return 1; };  // Now an error again.
