//type:cp
//options_all:--c++20

namespace std {
  using size_t = decltype(sizeof(0));

  template<typename T> struct tuple_size;
  template<size_t, typename> struct tuple_element;
  template<size_t I, typename T> auto get(const T&) ->
    typename std::tuple_element<I, T>::type;

  template<typename T> struct tuple { T arr[3]; };
  template<typename T> struct tuple_size<tuple<T>> {
    static const size_t value = 3;
  };
  template<size_t I, typename T> struct tuple_element<I, tuple<T>> {
    using type = T;
  };
  template<size_t I, typename T> auto get(const tuple<T>& t) {
    return t.arr[I];
  };
};
using namespace std;

struct A {
  int a, b, c;
};

void f() {
  tuple<int> t{1, 2, 3};

  static auto [ta, tb, tc] = t;
  thread_local auto [a, b, c] = A();

  // Make variables used
  (void)(a + b + c + ta + tb + tc);
}
