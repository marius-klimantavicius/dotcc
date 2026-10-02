//type:fp
//options_all:--c++11

template <int... Ints>
struct my_integer_sequence { };

struct Foo {
  Foo(int, int);
};

template <class T, int... I>
decltype(T{ I... }) A(my_integer_sequence<I...>);

template <int... I>
decltype(Foo{ I... }) B(my_integer_sequence<I...>);

template <int x, int y, int... I>
decltype(Foo{ x, y, I... }) C(my_integer_sequence<x, y, I...>);


int main() {
  Foo f{1, 2};
  A<Foo>(my_integer_sequence<1, 2>{});
  B<>(my_integer_sequence<1, 2>{});
  C(my_integer_sequence<1, 2>{});
}
