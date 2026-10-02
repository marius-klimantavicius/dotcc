//type:fp
//options_all:--c++20 -A
//source_files:cwg2300.C
inline void f(bool cond, void (*p)()) {
    if (cond) f(false, []{});
  }
  struct X {
    void h(bool cond, void (*p)() = []{}) {
      if (cond) h(false);
    }
  };

//cwg: 2300
//title: Lambdas in multiple definitions
//meeting: Cologne 07/19
//edg_status: Passes
