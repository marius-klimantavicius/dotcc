//type:fp
//options_all:--clang -w
//remark:[6.1] Clang C++ compatibility: Conversion from pointer to _Atomic to void*
// 2/20/20  [EDGcpfe/19901]
//
// Clang C++ compatibility: Conversion from pointer to _Atomic to void*
//
// In Clang C++ mode, the front end now permits (with a warning) implicit
// conversions from _Atomic-qualified pointers to void*.
_Atomic(int) *p;
void *q = p;  // Now accepted in Clang C++ mode (previously an error).
