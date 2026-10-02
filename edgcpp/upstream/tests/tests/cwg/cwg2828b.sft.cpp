//type:fn
//options_all:--c++23 

  int*** ptr = 0;
  auto t = (int const*const*const*)ptr;  // OK, const_cast interpretation

  struct S {
    operator const int*();
    operator volatile int*();
  };
  int *p = (int*)S();  // error: two possible interpretations using static_cast followed by const_cast
