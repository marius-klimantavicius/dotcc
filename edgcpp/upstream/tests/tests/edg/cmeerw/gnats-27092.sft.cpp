//tpye:fp
//options:--c++11:--c++20:--c++20 --gn 130200
//options_all:-tused

namespace minimal {
  template<typename T> T z();
  template<typename ... TT>
  void f(decltype(x(y(z<TT>()...)))) {}
  template<typename ... TT>
  void f(decltype(x(y(z<TT>())...))) {}
}

template<typename T> T v();
template<typename T> using A = T;

template<typename...> struct C;

template <typename... TT>
using R = C<A<decltype(f(v<TT>()))> ...>;

template<typename... TT>
R<TT...> g() {
  using V = decltype(f(v<TT>() ...));
  A<V> a;
  return {};
}
