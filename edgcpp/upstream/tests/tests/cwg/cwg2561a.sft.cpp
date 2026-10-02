//options_all:--c++23 -tused -A
  struct C {
    C(auto) { }
  };

  void foo() {
    auto a = [](C) { return 0; };
    int (*fp)(C) = a;   // OK
    fp(1);              // same effect as decltype(a){}(1)
    auto b = [](this C) { return 1; };
    fp = b;             // OK
    fp(1);              // same effect as (&decltype(b)::operator())(1)
  }

//cwg: 2561
//title: Conversion to function pointer for lambda with explicit object parameter
//meeting: St Louis 6/24
//edg_status: Passes
