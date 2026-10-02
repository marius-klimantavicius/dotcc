//remark:Copy-elision
//options:--c++17;fp

template<typename T> auto v() noexcept -> T&&;
template<typename S, typename D> struct Q {
  template<typename T> static void f(T) noexcept;
  decltype(f<D>(v<S>())) *p;
};
class N {
  N(N const &) {}  // Private.
};
struct X {
  operator N() const;
};
auto r = Q<X&, N>{};

