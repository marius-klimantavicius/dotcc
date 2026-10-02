//type:fp
//options_all:--c++20 --gn 130200
//remark:[6.7] Incorrect handling of subsumption with OR-ed constraints
// 2/6/24   [EDGcpfe/26961]
//
// Incorrect handling of subsumption with OR-ed constraints
//
// In this example, the last partial specialization S<T> is more specialized than
// the prior partial specialization (which ORs two constraints) and should thus
// be selected for S<float>.  However, the front end previously failed to
// correctly order the two partial specializations.  That is now fixed.
template<typename T, typename U> concept Same = __is_same_as(T, U);
template<typename T> concept A = Same<T, float>;
template<typename T> concept B = Same<T, double>;

template<typename T> struct S {};
template<typename T> requires A<T> || B<T> struct S<T> {};
template<typename T> requires A<T> struct S<T> {};

S<float> s;
