//options_all:-r -x -tused --microsoft_version=1400 --set_flag=no_checking_pragmas
//type:rp

inline void f();
template <class T> class A { };
template <class T> struct S {
  friend void f() { A<T> x; }
};
main() {
  S<int> s;
  f();
}

