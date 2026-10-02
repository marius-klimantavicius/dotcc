//type:fn
//options_all:--c++23 -tused
template<auto n> struct B { /* ... */ };
struct J1 {
  J1 *self=this;
};
B<J1{}> j1;  // error: initialization of template parameter object
             // is not a constant expression
