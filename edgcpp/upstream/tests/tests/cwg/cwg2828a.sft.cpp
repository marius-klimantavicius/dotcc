//options_all:--c++23 -A 

  int*** ptr = 0;
  auto t = (int const*const*const*)ptr;  // OK, const_cast interpretation

  struct S {
    operator const int*();
    operator volatile int*();
  };

//cwg: 2828
//title: Ambiguous interpretation of C-style cas
//meeting: Tokyo 3/24
//edg_status: Passes
