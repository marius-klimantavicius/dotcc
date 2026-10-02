//type:fp
//options_all:--microsoft_v 1911
//remark:[6.7] Cast to incomplete type in decltype construct
// 4/3/24   [EDGcpfe/19086,EDGcpfe/26821,EDGcpfe/26870,EDGcpfe/26896]
//
// Cast to incomplete type in decltype construct
//
// The front end now accepts some casts to an incomplete type within a decltype
// construct if that cast appears in a template.  The details of which cases are
// accepted depend on the C++ dialect that is selected.
struct S;
template<typename T> struct X {
  decltype(S(T{0})) *p1;  // Accepted in Microsoft, Clang, and GCC modes.
  decltype(X<T>(0)) *p2;  // Accepted in all C++ modes.
};
