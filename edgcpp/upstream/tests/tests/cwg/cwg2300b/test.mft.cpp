//type:fn
//options_all:--c++20 -A
//source_files:cwg2300.C
inline void f(bool cond, void (*p)()) {
    if (cond) f(false, []{});
  }
  inline void g(bool cond, void (*p)() = []{}) {
    if (cond) g(false); // ill formed no diagnostic required
  }
  struct X {
    void h(bool cond, void (*p)() = []{}) {
      if (cond) h(false);
    }
  };
