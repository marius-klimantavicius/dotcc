//remark:Exception specification instantiation
//options:--c++14 --microsoft_version=1916;fp:--c++17;fp

class X {
  public:
    X(X const&) noexcept(false);
};

template<typename T>
class Y {
  T x;
public:
  Y(Y const&) noexcept(false) = default;
};

Y<X> f(Y<X> const& y) {
  Y<X> var{y};
  return var;
}
