//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap

  template<typename A, typename B> struct check_derived_from { 
    static A a; 
    static constexpr B *p = &a; 
  }; 
  struct W {}; 
  struct X {}; 
  struct Y {}; 
  struct Z : W, 
    check_derived_from<Z, W> cdf; // #3 
  }; 
