//remark:Friend and constexpr in cp_gen_be
//options:--c++17 -tused;cp

template<class> struct bar {
  friend constexpr bool operator==(bar, bar) { return true; }
};
 
template<class P, bool = (P() == P()) >
void foo(P) { }
 
void qqq() { foo(bar<void>()); }
