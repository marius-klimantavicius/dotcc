//type:fp
//options_all:--c++11 --microsoft
//remark:[6.3] Failure to deduce array length from braced-list argument
// 3/2/21   [EDGcpfe/23293,EDGcpfe/23497,EDGcpfe/23983]
//
// Failure to deduce array length from braced-list argument
//
// The front end previously sometimes failed to deduce the length of a template-
// dependent array parameter from a braced-list argument when the element type of
// the array parameter is non-dependent.
//
// That is now fixed.
template <decltype(sizeof(1)) N> void f(long (&&arr)[N]) {
  (void)arr;
}
int main() {
  f({1, 2});  // Previously an error.  Now okay.
}
