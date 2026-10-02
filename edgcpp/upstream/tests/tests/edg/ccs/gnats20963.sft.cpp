//type:fp
//options::--gnu_version 70400:--clang_version 50100:--microsoft_version 1920
//options_all:--c++17

using size_t = decltype(sizeof(0));

namespace std { template<typename T> struct tuple_size; }
namespace std { template<size_t, typename> struct tuple_element; }
template<typename T> struct std::tuple_size<const T> : std::tuple_size<T> {};
template<size_t N, typename T> struct std::tuple_element<N, const T> {
  typedef const typename std::tuple_element<N, T>::type type;
};

template<typename ElemType, typename GetTypeLV, typename GetTypeRV>
struct wrap {
  template<size_t> GetTypeLV get() &;
  template<size_t> GetTypeRV get() &&;
};
template<typename ET, typename GTL, typename GTR>
struct std::tuple_size<wrap<ET, GTL, GTR>> {
  static const int value = 1;
};
template<typename ET, typename GTL, typename GTR>
struct std::tuple_element<0, wrap<ET, GTL, GTR>> {
  using type = ET;
};

template<typename T> T &lvalue();

void test_value_category() {
  // If the declared variable is an lvalue reference, the operand to get is an
  // lvalue. Otherwise it's an xvalue.
  { auto [a] = wrap<int, void, int>(); }
  { auto [a] = lvalue<wrap<int, void, int>>(); }
  { auto &[a] = lvalue<wrap<int, int, void>>(); }
  { auto &&[a] = wrap<int, void, int>(); }
  // If the initializer (call to get) is an lvalue, the binding is an lvalue
  // reference to the element type. Otherwise it's an rvalue reference to the
  // element type.
  { auto [a] = wrap<int, void, int&>(); }
  { auto [a] = wrap<int&, void, int&>(); }
  { auto [a] = wrap<int&&, void, int&>(); }

  { auto [a] = wrap<int, void, int&&>(); }
  { auto [a] = wrap<const int&, void, int&&>(); }
  { auto [a] = wrap<int&&, void, int&&>(); }

  { auto [a] = wrap<const int, void, float&>(); }
  { auto [a] = wrap<int, void, float>(); }
}
