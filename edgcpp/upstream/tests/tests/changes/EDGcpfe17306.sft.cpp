//type:fp
//options_all:--c++11 --g++
//remark:[4.12] GNU compatibility: Cast to incomplete class type in template default argument
// 6/15/16  [EDGcpfe/17306]
//
// GNU compatibility: Cast to incomplete class type in template default argument
//
// In GNU C++ modes, the front end now accepts functional-notation casts to
// incomplete types if they appear as an unevaluated operand in a template default
// argument.
struct S;
template<typename = decltype(S(0))> void g() {}
  // Normally an error (S is incomplete), but now accepted in
  // GNU C++11 mode.
int main() {
  g<>();
}
