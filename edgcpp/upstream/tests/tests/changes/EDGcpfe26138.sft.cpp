//type:fp
//options_all:--c++17
//remark:[6.6] Spurious Microsoft-mode error on in-class constexpr static data member
// 11/15/23 [EDGcpfe/26138]
//
// Spurious Microsoft-mode error on in-class constexpr static data member
//
// In Microsoft mode, this previously triggered spurious errors suggesting a
// missing initializer for N.  That is now fixed.
template<typename> struct S {
  template<typename> static constexpr int N = 0;
};
template<typename T> struct X: S<T> {};
