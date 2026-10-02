//type:cp
//options::-DNEG;fn
//options_all:--c++11

template <int... Ints>
struct my_integer_sequence { };

struct Foo {
  int i;
  int i2;
  Foo(int, int) : i(1), i2(2) { }
};

template <class T, int... I>
decltype(T{ I... }) helper(T, my_integer_sequence<I...>);

int main() {
  Foo f{1, 2};
  decltype(helper<>            (f, my_integer_sequence<1, 2>{})) thing1{1, 2};
  decltype(helper<Foo>         (f, my_integer_sequence<1, 2>{})) thing2{1, 2};
  decltype(helper<Foo, 1>      (f, my_integer_sequence<1, 2>{})) thing3{1, 2};
  decltype(helper<Foo, 1, 2>   (f, my_integer_sequence<1, 2>{})) thing4{1, 2};
#ifdef NEG
  decltype(helper<Foo, 1, 2, 3>(f, my_integer_sequence<1, 2>{})) thing5{1, 2};
#endif /* NEG */
}
