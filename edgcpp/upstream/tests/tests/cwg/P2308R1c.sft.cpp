//type:fn
//options_all:--c++23 -tused
template<auto n> struct B { /* ... */ };
struct J2 {
  J2 *self=this;
  constexpr J2() {}
  constexpr J2(const J2&) {}
};
B<J2{}> j2;  // error: template parameter object not
             // template-argument-equivalent to introduced temporary
