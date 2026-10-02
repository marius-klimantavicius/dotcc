//options_all:--c++20 -tused -A
  typedef __SIZE_TYPE__ size_t;
  template<class T, size_t N>
  struct H {
    T array[N];
  };
  template<class T, size_t N>
  struct I {
    volatile T array[N];
  };

  H h = { "abc" };  // OK, deduces H<char, 4> (not T = const char)
  I i = { "def" };  // OK, deduces I<char, 4>

//cwg: 2681
//title: Deducing member array type from string literal
//meeting: Issaquah 2/23
//edg_status: Passes
