//type:fn
//options_all:--c++20 -tused -A
  typedef __SIZE_TYPE__ size_t;
  template<size_t N>
  struct J {
    unsigned char array[N];
  };

  J j = { "ghi" };  // error: cannot bind reference to array of unsigned char to array of char in deduction
