//type:fp
//options_all:--gnu_version 40500 -tused --c++11
//remark:[4.14] Abort on dependent array bound
// 6/8/17   [EDGcpfe/18324,EDGcpfe/18468]
//
// Abort on dependent array bound
//
// The front end sometimes aborted with an internal error in num_array_elements
// when handling a default-initialized array variable with a dependent bound.
//
// This regression introduced in version 4.13 (by the changes for EDGcpfe/17934)
// is now fixed.
struct S { constexpr S() {} };
template<typename T> void g() {
  S xs[T::N][2];  // Previously triggered an internal error.  Now fixed.
}
