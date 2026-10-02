//type:fp
//options_all:--c++11
//remark:[6.5] Incorrect pack expansion for nested template argument lists
// 1/24/23  [EDGcpfe/17783,EDGcpfe/21995,EDGcpfe/25971]
//
// Incorrect pack expansion for nested template argument lists
//
// In some cases involving nested template argument lists, parameter packs would
// repeatedly expand to their first element, resulting in spurious errors during
// overload resolution.
template<typename T1, typename T2> T2 f(T1, T2);
template<typename T> T g();
template<typename T> struct D { using type = T; };
template<typename... A> struct C {
  template<typename T> auto h() -> decltype(f(g<typename D<A>::type>()...));
};
long *l = C<int, long *>().h<int>();  // Previously a spurious error.
                                      // Now okay.
