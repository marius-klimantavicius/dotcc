//type:fp
//options_all:--c++20 --microsoft
//remark:[6.4] Spurious substitution failure of static data member
// 10/10/22 [EDGcpfe/25616]
//
// Spurious substitution failure of static data member
//
// The friend operator== was previously discarded during constraint checking
// because the substitution of "cond" in the requires clause (which is really
// S<int>::N<true>::cond in this case) spuriously failed.  That is now fixed.
template<typename> struct S {
  template <bool> struct N {
    static constexpr bool cond = true;
    friend constexpr bool operator==(const N &, const N &) requires cond {
      return true;
    }
  };
};
bool g(S<int>::N<true> p) {
  return p != p;  // Previously an error.  Now okay.
}
