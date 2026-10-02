//type:fp
//options_all:--c++20
//remark:Incorrect handling of deducibility in C++17 (and later) modes
// 4/16/26  [EDGcpfe/24619,EDGcpfe/26682,EDGcpfe/28602]
//
// Incorrect handling of deducibility in C++17 (and later) modes
//
// The parameter type X<T::x> of function template f is "not deducible", and so
// T should be deduced from the other parameter (of deducible type W<T>).
// However, in C++17 mode, the front end failed to mark the first parameter as not
// deducible and when attempting deduction on it for the call f(1, w) then failed
// deduction altogether.  This issue also affected partial specialization (which
// is based on deduction).
//
// This previously elicited a spurious error about template parameter X not
// being deducible.  This is now fixed.
template<auto, auto> struct S;
template<typename X, typename Y, X X::*x, Y Y::*y> class S<x, y> {};
