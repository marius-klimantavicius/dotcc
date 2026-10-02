//type:fp
//options_all:--g++
//remark:[4.11] clang and g++ compatibility: qualified friend declarations
// 12/15/15 [EDGcpfe/16435]
//
// clang and g++ compatibility: qualified friend declarations
//
// When a qualified name occurs in a friend declaration, the standard mandates
// that the name refers to a previously declared name, but g++ and clang seem to
// only require that some function with that name exist in the proper scope.
// clang only displays this behavior when the friend declaration is in a template.
// The front end now emulates this behavior.
int f();
class A {
  friend int ::f(const A&);                       // g++ allows
  template <class T> friend int ::f(const A&, T); // g++ and clang allow
} a;
int x = f();
int y = f(a);
int z = f(a, 0);
