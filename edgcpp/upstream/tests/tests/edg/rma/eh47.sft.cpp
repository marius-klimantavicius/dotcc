//options_all:-r -x -tused
//options: --strict;cn:;cn

class X *px;
template <class T> inline void f(T) throw(px) { }
main() {
  f(0);
}

