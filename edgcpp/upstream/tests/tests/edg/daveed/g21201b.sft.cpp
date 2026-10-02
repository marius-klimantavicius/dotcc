//remark:Exception specification instantiation
//options:--c++14 --microsoft_version=1916;fp:--c++17;fp

  struct X { X(X const&) noexcept(false); };
  template<typename T> struct Y {
    T x;
    Y(Y const&) noexcept(false) = default;
  };
  Y<X> f(Y<X> const &p) {
    return p;  // Previously a spurious error
  }

