//type:fp
//options_all:--g++ --c++14
//remark:[4.11] Attributes in class template declarations
// 12/17/15 [EDGcpfe/16693]
//
// Attributes in class template declarations
//
// The presence of an attribute (either a standard attribute or a GNU attribute)
// immediately following a template parameter list in a class template declaration
// had caused such templates to mistakenly be parsed as function templates,
// typically resulting in spurious errors.  A change has been made to allow (and
// ignore) attributes in this case, as well as after a "friend" keyword in
// templated friend declarations.
template <class T> [[deprecated]] class A {};
template <class T> __attribute((deprecated)) class B {};
struct X {
  template <class T> friend [[deprecated]] class Y;
  template <class T> friend __attribute((deprecated)) class Z;
};
