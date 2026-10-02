//type:fp
//options_all:--c++14 -tused -A
  void f(int, int, int);
  template<int ...N> void g() {
    f((N+N)...);
  }
  void h() {
    g<1, 2, 3>();
  }
