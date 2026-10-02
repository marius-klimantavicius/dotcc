//type:fp
//options_all:--c++11
//remark:[6.8] Abort on parenthesized member function template declaration
// 9/24/25  [EDGcpfe/23041,EDGcpfe/23207]
//
// Abort on parenthesized member function template declaration
//
// In some cases where a member function template of a partially specialized class
// template is declared with a parenthesized declarator, the front end could abort
// with a failed assertion in find_template_class.
template<typename T> using A = T;
template<typename T> struct C;
template<typename T>
struct C<T *> {
  template<typename U>
  A<C> (f)(int);  // Previously triggered an assertion failure.  Now okay.
};
