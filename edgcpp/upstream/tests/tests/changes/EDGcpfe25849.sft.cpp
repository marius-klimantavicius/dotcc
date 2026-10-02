//type:fp
//options_all:--c++20
//remark:[6.6] Nested template arguments with associated constraints in subsumption checking
// 6/22/23  [EDGcpfe/25849,EDGcpfe/26229,EDGcpfe/26377]
//
// Nested template arguments with associated constraints in subsumption checking
//
// Previously, the front end could not decide between the two partial
// specializations of P when instantiating P<int> because it additionally compared
// the associated constraints in the substituted nested template arguments of
// C0<X<T>> and therefore failed to see that C2<T> subsumes C1<T>.  That is now
// fixed.
template<typename T> struct X;
template<typename T> concept C0 = true;
template<typename T> concept C1 = C0<X<T>>;
template<typename T> concept C2 = C0<X<T>> && true;
template<typename> struct P;
template<C1 C> struct P<C> { };
template<C2 C> struct P<C> { };
P<int> p;  // Previously a spurious error.  Now okay.
