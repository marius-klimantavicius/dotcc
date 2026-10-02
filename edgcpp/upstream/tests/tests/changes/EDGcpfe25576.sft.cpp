//type:fp
//options_all:--c++11 --g++
//remark:[6.5] GNU compatibility: Substitution failure for two-operand conditional operator
// 1/30/23  [EDGcpfe/25576,EDGcpfe/25717]
//
// GNU compatibility: Substitution failure for two-operand conditional operator
//
// Previously, the first operand with a conversion to "bool" already applied was
// used as the implicit second operand during substitution of a GNU two-operand
// conditional operator.  This could cause spurious substitution failures due to
// invalid conversions.
enum E { E0, E1 };
template<E e> struct C { };
template<E e> C<e ?: E0> f();
using type = decltype(f<E1>());  // Previously a spurious error.  Now okay.
