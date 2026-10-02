//type:fp
//options_all:--c++20 -tused -A
//IFNDR
  void f(int, int);
  template<typename T> void g(T t) { f(t); }
  void f(int, int = 0);
  void h() { g(0); }

//cwg: 1907
//title: using-declarations and default arguments
//meeting: Virtual 11/20*
//edg_status: Passes (IFNDR)
