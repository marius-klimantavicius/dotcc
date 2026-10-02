//options_all:-tused -A --c++20
  template<typename T = int>
  int f();

  int x = f();      // ill-formed per the wording
  int (*y)() = f;   // ditto
