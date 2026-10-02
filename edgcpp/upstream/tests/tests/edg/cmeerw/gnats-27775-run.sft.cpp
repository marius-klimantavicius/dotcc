//type:rp
//options:--c++26

extern "C" int printf(const char *, ...);

namespace std
{
  template<typename>
  struct tuple_size;

  template<int, typename>
  struct tuple_element;
}

namespace tuple_like
{
  struct C
  {
    template<int I>
    typename std::tuple_element<I, C>::type get() const {
      printf("get<%d>\n", I);
      return {};
    }
  };

  template<int I>
  struct B {
    B() { printf("B::B<%d>\n", I); }
  };

  void f(auto v)
  {
    auto [ b1, ... b, b4 ] = v;
  }
}

template<>
struct std::tuple_size<tuple_like::C>
{
  static constexpr int value = 4;
};

template<int I>
struct std::tuple_element<I, tuple_like::C>
{
  using type = tuple_like::B<I>;
};

namespace struct_like
{
  struct C
  {
    short s;
    int i;
    long l;
  };

  long f(auto c)
  {
    auto [ ... b ] = c;
    return ( b + ... );
  }
}

int main()
{
  // expected output: 6 == 6
  printf("6 == %d\n", f(struct_like::C{ 1, 2, 3 }));

  // expected output: get<0>
  // expected output: B::B<0>
  // expected output: get<1>
  // expected output: B::B<1>
  // expected output: get<2>
  // expected output: B::B<2>
  // expected output: get<3>
  // expected output: B::B<3>
  f(tuple_like::C{});
}
