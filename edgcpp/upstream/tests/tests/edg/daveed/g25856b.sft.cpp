//remark:non-auto vars and constant evaluation
//options:--c++20;fn:--c++23;fp



    constexpr char xdigit(int n) {
      thread_local constexpr char digits[] = "0123456789abcdef";
      return digits[n];
    }

static_assert(xdigit(10) == 'a');
