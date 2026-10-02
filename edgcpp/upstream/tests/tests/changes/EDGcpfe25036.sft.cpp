//type:fp
//options_all:--microsoft_v 1916
//remark:[6.4] Regression on templates with __restrict parameters in Microsoft mode
// 2/10/22  [EDGcpfe/25036]
//
// Regression on templates with __restrict parameters in Microsoft mode
//
// The changes for EDGcpfe/24520 introduced a regression in version 6.3 of the
// front end, causing it to fail to substitute function templates with __restrict
// qualifiers in some cases.
//
// That is now fixed.
template<typename T> int f(T const *__restrict) { return 42; }
template int f<double>(double const *__restrict);  // Previously an error.
