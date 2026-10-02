//remark:Partial specialization matching
//options:--c++14;fp

template<typename> struct V { typedef void Type; };
template <typename, typename = void> struct X;
template <typename T> struct X< T, typename V<decltype(bool(T()))>::Type> {
  static constexpr bool value = true;
};
static_assert(X<int>::value, "");
