//type:rp
//options_all:--c++17 -w

extern "C" int printf(const char*, ...);

namespace MINE {
  struct A { int i; } a { 42 };
}

namespace std
{
  using size_t = decltype(sizeof(0));

  template<typename T> struct tuple_size;

  template<int, typename> struct tuple_element;
  template<> struct tuple_element<0, MINE::A> { typedef int type; };
  template<> struct tuple_element<1, MINE::A> { typedef long type; };
  template<> struct tuple_element<2, MINE::A> { typedef int type; };
  template<> struct tuple_element<3, MINE::A> { typedef char type; };
}

namespace MINE {
  template <std::size_t I>
  auto get(const A &ra) -> typename std::tuple_element<I, A>::type
  {
    return 10;
  }
}

int main()
{
  auto [ ai ] = MINE::a;
  printf("Result: %d\n", ai);
}
