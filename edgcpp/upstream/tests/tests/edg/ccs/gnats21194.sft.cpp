//type:cp
//options::-DNEG;fn
//options_all:--c++17

template <unsigned long, typename...> struct B;
template <unsigned long I, typename H> struct B<I, H> { int b; };
template <typename... E> struct C
{
   B<0, E...> c;
   C (C &) = default;
   C (C &&);
};

namespace std
{
   template <typename> struct tuple_size;
   template <> struct tuple_size<C<int>> { static constexpr int value = 1; };
   template <int, typename> struct tuple_element;
   template <typename H, typename... T>
   struct tuple_element<0, C<H, T...>> { typedef int type; };
}

template <int, typename... E>
int get (C<E...> &&c);

int foo (C<int> t)
{
#ifndef NEG
  auto [x0] = t;
#else /* !NEG */
  auto& [x0] = t;
#endif /* NEG */
  return x0;
}
