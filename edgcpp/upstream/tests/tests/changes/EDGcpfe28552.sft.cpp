//type:fp
//options_all:--c++14 --g++
//remark:Assertion failure in diagnostic_pragma
// 12/1/25  [EDGcpfe/28552]
//
// Assertion failure in diagnostic_pragma
//
// In some cases, the use of "#pragma diagnostic" could result in an assertion
// failure in diagnostic_pragma.
#pragma diagnostic push
template <class T> constexpr void f() {
  _Pragma("diagnostic push")
  _Pragma("diagnostic pop")
}
#pragma diagnostic pop
#pragma diagnostic push
void g() {
    f<void>();
}
#pragma diagnostic pop
