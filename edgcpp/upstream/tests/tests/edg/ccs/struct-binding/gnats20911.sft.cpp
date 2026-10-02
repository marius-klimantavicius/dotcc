//type:rp
//options_all:--c++20

extern "C" int printf(const char*, ...);

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
  template<size_t I, typename T> auto& get(tuple<T>& t) {
    return t.arr[I];
  };
};
using namespace std;

struct S {
  int a = 1, b = 2, c = 3;
};

int main() {
  int arr[3] = {4, 5, 6};
  tuple<int> t{7, 8, 9};
  S s;

  auto& [aa, ab, ac] = arr;
  auto& [ta, tb, tc] = t;
  auto& [sa, sb, sc] = s;

  [&sa, &ab, &tc] { sa = 5; ab = 50; tc = 500; }();
  [&] {
    sb = sa + 1;
    sc = sb + 1;
    aa = ab - 10;
    ac = ab + 10;
    tb = tc - 100;
    ta = tb - 100;
  }();

  printf("Struct values are: %d, %d, %d\n", s.a, s.b, s.c);
  printf("Array values are: %d, %d, %d\n", arr[0], arr[1], arr[2]);
  printf("Tuple values are: %d, %d, %d\n", t.arr[0], t.arr[1], t.arr[2]);

  if (s.a != 5 || s.b != 6 || s.c != 7) return 1;
  if (arr[0] != 40 || arr[1] != 50 || arr[2] != 60) return 2;
  if (t.arr[0] != 300 || t.arr[1] != 400 || t.arr[2] != 500) return 3;
}
