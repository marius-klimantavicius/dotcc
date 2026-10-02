//type:fp
//options_all:--microsoft
//remark:[4.10.1] Microsoft mode dependent nontype template arguments
// 5/18/15  [EDGcpfe/15742,EDGcpfe/16168,EDGcpfe/16235,EDGcpfe/16239]
//
// Microsoft mode dependent nontype template arguments
//
// The changes for EDGcpfe/15084 etc. (see the entry of 6/10/14) caused some
// nontype template arguments to be processed incorrectly in Microsoft modes.
// This caused template deduction to spuriously fail in various situations
// (typically leading to spurious errors).
//
// One such situation is a member function definition appearing outside its
// parent class template definition with the return type or a parameter of the
// member function involving a nontype template argument expressed using a
// constant-valued variable.
//
// Previously, in Microsoft mode, the front end failed to connect definition (D)
// with declaration (d), and that triggered a spurious error.  This is now fixed.
//
// Another such situation occurred with a dependent default template argument
// for a pointer-to-function template parameter.
//
// Previously, in Microsoft mode, the call "g<int>(42)" could not be resolved
// because the default template argument "f<T>" was not correctly processed.
// This too is now fixed.
int const K = 0xfff;
template<typename T> struct D {
  template<int> struct N { typedef int Type; };
  typename D::template N<K>::Type f();            // (d)
};
template<typename T> 
typename D<T>::template N<K>::Type D<T>::f() {    // (D)
  return 3;
}

template<typename T> void f(T);
template<typename T, void F(T) = f<T> >
void g(T t) {
  F(t);
}
int main() {
  g<int>(42);
}
