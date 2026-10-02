//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

template <class T> class A { T t; };
struct S {
  void f() { A<int> ai; }
} s;

