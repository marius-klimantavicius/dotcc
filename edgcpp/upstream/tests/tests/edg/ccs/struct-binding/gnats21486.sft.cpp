//type:cp
//options_all:--c++20 -tused

namespace std {
  using size_t = decltype(sizeof(0));

  template<typename T> struct tuple_size;
  template<size_t, typename> struct tuple_element;
  template<size_t I, typename T> auto get(const T&) ->
    typename std::tuple_element<I, T>::type;

  template<typename T> struct tuple { T arr[1]; };
  template<typename T> struct tuple_size<tuple<T>> {
    static const size_t value = 1;
  };
  template<size_t I, typename T> struct tuple_element<I, tuple<T>> {
    using type = T;
  };
  template<size_t I, typename T> auto get(const tuple<T>& t) {
    return t.arr[I];
  };
  template<size_t I, typename T> auto& get(tuple<T>& t) {
    return t.arr[I];
  };
};
using namespace std;

struct S {
  int x;
} s;

int arr[1];
tuple<int> t;

template<class T> int f()
{
  static auto [sb] = s;
  static auto [ab] = arr;
  static auto [tb] = t;
  return sb + ab + tb;
}
