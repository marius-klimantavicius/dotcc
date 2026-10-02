//type:fp
//options_all:--c++20 -tused -A
  void f(int, int=0);
  void f(int=0, int);

  void g() {
    f();
  }

//cwg: 418
//title: Imperfect wording on error on multiple default arguments on a called function
//meeting: Virtual 11/20*
//edg_status: Passes
