#ifndef CONSTEXPR
#define CONSTEXPR constexpr
#endif
template<typename> bool X;
template<typename T> struct Y {
  static CONSTEXPR bool var = X<T>;
};
