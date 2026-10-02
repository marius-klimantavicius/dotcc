  inline void f(bool cond, void (*p)()) {
    if (cond) f(false, []{});
  }
  inline void g(bool cond, void (*p)() = []{}) {
    if (cond) g(false);
  }
  struct X {
    void h(bool cond, void (*p)() = []{}) {
      if (cond) h(false);
    }
  };
