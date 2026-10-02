//type:fp
//options_all:--c++17 --microsoft
//remark:[6.0] Spurious errors on inline static data member template instance
// 10/25/19 [EDGcpfe/20058,EDGcpfe/21530,EDGcpfe/21795,EDGcpfe/21848]
//
// Spurious errors on inline static data member template instance
//
// The front end previously issued spurious errors on certain inline static
// data member template declarations.
//
// That is now fixed.
struct S {
  template<int N> static inline int m = N;  // Previously triggered a
};                                          // spurious error when
int i = S::m<42>;                           // instantiated.
