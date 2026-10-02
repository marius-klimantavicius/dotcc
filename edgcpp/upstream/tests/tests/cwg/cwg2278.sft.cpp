//type: fn
//options_all: -A --c++20 -tused -e 200 --no_wrap

  struct B { B *self = this; }; 
  extern const B b; 
  constexpr B f() { 
    B b; 
    if (&b == &::b) return B(); 
    else return b; 
  } 
  constexpr B b = f(); // is b.self == &b?

//cwg: 2278
//title: Copy elision in constant expressions reconsidered
//meeting: Kona 02/19
//edg_status: Passes
