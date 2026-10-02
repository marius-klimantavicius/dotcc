//type:fp
//options_all:--c++17 --parse --microsoft
//remark:[6.8] Abort in Clang and Microsoft modes on some CTAD cases
// 7/22/25  [EDGcpfe/22555,EDGcpfe/28231]
//
// Abort in Clang and Microsoft modes on some CTAD cases
//
// During CTAD (class template argument deduction) in Clang or Microsoft mode,
// this triggered a null pointer indirection in the front end.  That is now fixed.
template<typename = int> struct S {};
template<typename> void g() {
  S s;
}
