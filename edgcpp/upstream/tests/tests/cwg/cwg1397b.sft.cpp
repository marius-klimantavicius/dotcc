//type:fn
//options_all:--c++14 -tused -A

  struct A {
    void *p = A{};
    operator void*() const { return p; }
  };
