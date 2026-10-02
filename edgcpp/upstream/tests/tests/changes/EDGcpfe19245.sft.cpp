//type:fp
//options_all:--c++17
//remark:[5.0] Spurious C++17 error on range-based-for loop in lambda
// 2/1/18   [EDGcpfe/19245]
//
// Spurious C++17 error on range-based-for loop in lambda
//
// In C++17 mode, a range-based-for loop appearing in a lambda previously elicited
// a spurious error if the loop variable has a non-literal type.
//
// The error mistakenly mentioned a "constexpr" function because the front end
// tentatively considers the lambda call operator a "constexpr" function in C++17
// mode.  This is now fixed.
struct D { ~D(); } da[1] = {};
auto lm = []{ for (auto x: da) ; };  // Previously a spurious error.
                                     // Now okay.
