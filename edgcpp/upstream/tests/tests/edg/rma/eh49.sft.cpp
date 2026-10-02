//options_all:-r -x -tused
//options: --strict;cn:;cn

class X;
template <class T> inline void f(T) throw(X) { }
template<> inline void f<int>(int) throw(X) { }
main() {
  f(0);
  f(0L);
}

