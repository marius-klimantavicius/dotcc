//options_all:-r -x -tused
//options: --strict;cn:;rp

template <class T> class A {
  friend void f(T = 0) { }
};
A<int> a;
main() {
  f();
}

