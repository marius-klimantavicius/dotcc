//type:fp
//options_all:--microsoft_version 1913
//remark:[5.0] Microsoft compatibility: constant expressions as arguments to alignas
// 6/19/18  [EDGcpfe/19664]
//
// Microsoft compatibility: constant expressions as arguments to alignas
//
// A spurious error had been issued in Microsoft emulation mode when parsing
// certain constant expressions in an alignas (or possibly other) attributes.
// Now fixed.
template <int N> struct A;
template <int T> struct alignas(A<T>::X ? 1: 2) B {};
