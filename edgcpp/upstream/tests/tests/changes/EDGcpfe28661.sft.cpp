//type:fp
//options_all:--c++11
//remark:Incorrect reuse of dependent class instantiation during alias instantiation
// 5/28/26  [EDGcpfe/28661]
//
// Incorrect reuse of dependent class instantiation during alias instantiation
//
// During alias template instantiation, a dependent class template instance could
// be incorrectly reused, resulting in spurious errors.
// --c++11:
template<int> struct B;
template<> struct B<2> {
  template<typename T, typename> using A = T;
};
template<typename ... Ts>
using A1 = typename B<sizeof ... (Ts)>::template A<Ts ...>;
template<typename ... Ts> using A2 = A1<char, Ts...>;
template<typename ... Ts> struct C;
template<typename ... Ts> struct C<A2<Ts ...>, Ts ...> { };
C<char, int> c;  // Previously a spurious error, now okay.
