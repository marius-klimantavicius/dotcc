//type:fn
//option_all:--c++20 -tused -A
  void g(double) = delete;

  template<class T> auto f(T t) -> decltype(g(t));

  void g(int);

  void h() {
    typedef int T;
    T t = 42;
    g(t);  // Ok (I “wrote the substituted arguments”, and it seems fine)
    f(42); // Presumably substitution is meant to fail.
  }

//cwg: 1724
//title: Unclear rules for deduction failure
//meeting: Virtual 10/21
//edg_status: Passes
