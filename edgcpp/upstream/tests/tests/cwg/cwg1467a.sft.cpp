//type:fp
//options_all:--c++17 -tused -A
  struct X {
    X() = default;
    X(const X&) = default;
  #ifdef OK
    X(int) { }
  #endif
  };

  X x;
  X x2{x}; 
