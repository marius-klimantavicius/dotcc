//type:fp
//options_all:--c++17
//remark:[4.14] C++17 compatibility: constexpr lambdas
// 5/8/17   [EDGcpfe/17690,EDGcpfe/17995]
//
// C++17 compatibility: constexpr lambdas
//
// In modes enabling C++17 compatibility, lambdas may now be declared constexpr
// and invocations of qualifying lambdas may appear in constant expressions,
// as described in Committee document P0170R1.
auto ID = [](int n) constexpr { return n; };
constexpr int I = ID(3);  // Now permitted in C++17 mode
