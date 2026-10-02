//type:fp
//options_all:--c++14 --gnu_version=80300
//remark:[6.2] Spurious GNU-mode ambiguity on member overloaded with using-declaration
// 10/5/20  [EDGcpfe/23191]
//
// Spurious GNU-mode ambiguity on member overloaded with using-declaration
//
// The changes for EDGcpfe/21627 introduced a regression in some GNU C++ modes in
// situations where a member template is overloaded with a member projected by a
// using-declaration.
//
// That regression (introduced in version 6.0) is now fixed.
template<typename> struct B {
  template<typename U> B& operator=(U v);
};
template<typename T> struct D: public B<T> {
  using B<T>::operator=;
  template<typename U> D& operator=(U v);
};
int main() {
  D<int> di;
  di = 2;  // Previously ambiguous in GNU C++ modes.  Now okay.
}
