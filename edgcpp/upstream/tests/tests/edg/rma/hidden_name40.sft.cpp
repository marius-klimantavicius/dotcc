//options_all:-r -x -tused
//options: --strict;cn:;cp

template <class T> void f(T) { }
void g() {
  extern void f(int);
  ::f<double>(0.0);
}
void g(long) {
  struct A {
    friend void f(long);
  };
  ::f<float>(0.0);
}

