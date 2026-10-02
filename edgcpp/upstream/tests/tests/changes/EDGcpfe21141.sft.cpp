//type:fp
//options_all:--c++20
//remark:[6.3] Unbounded recursion when mangling some lambda types
// 3/22/21  [EDGcpfe/21141,EDGcpfe/23339,EDGcpfe/24078]
//
// Unbounded recursion when mangling some lambda types
//
// A stack overflow had occurred when mangling certain lambda types that had
// occurred in unevaluated contexts.  Now fixed.
auto x = [](decltype([]{}) y) { return y; };
