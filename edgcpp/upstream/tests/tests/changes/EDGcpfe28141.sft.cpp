//type:fp
//options_all:--c++20 --clang_v 999999
//remark:[6.8] Clang compatibility: Conversion of _Nullable pointers
// 9/30/25  [EDGcpfe/28141,EDGcpfe/28464]
//
// Clang compatibility: Conversion of _Nullable pointers
//
// The front end now accepts the conversion of a _Nullable pointer to a plain
// pointer (in Clang modes).
int *_Nullable p = nullptr;
int *& q = p;  // Previously an error.  Now okay.
