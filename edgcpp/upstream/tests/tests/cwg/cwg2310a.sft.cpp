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
    X, check_derived_from<Z, X>,  // #1 
  }; 

//cwg: 2310
//title: Type completeness and derived-to-base pointer conversions
//meeting: Kona 02/19
//edg_status: EDGcpfe/22468
//fixed_in: 6.1
