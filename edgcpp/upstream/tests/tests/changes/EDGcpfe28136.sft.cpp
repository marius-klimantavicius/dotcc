//type:fp
//options_all:--c++17 --clang_v 999999
//remark:Deduction of OpenCL vector types in Clang modes
// 3/11/26  [EDGcpfe/28136,EDGcpfe/28316,EDGcpfe/28733]
//
// Deduction of OpenCL vector types in Clang modes
//
// The front end now can deduce the element type and the length of template-
// dependent OpenCL vector types in Clang C++ modes (such types are introduced
// with the ext_vector_type attribute).
template<class T, unsigned long N>
  using Vec __attribute__((__ext_vector_type__(N))) = T;
template<unsigned long N> void f(Vec<int, N>) {}
void g(Vec<int, 4> v) {
  f(v);  // Now okay in Clang C++ modes.
}
