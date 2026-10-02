//type:cp
//options::--gnu_version 40902:--clang_version 30800
//options_all:--c++11 -tused

template <class T, T...> struct A {};
template <int... I> using i = A<int, I...>;
template <class T, T, T, class> struct B;
 
template <class T, T p1, T p2, T... Idx>
struct B<T, p1, p2, A<T, Idx...>> {
  typedef typename B<T, 1, p2, A<T, p1>>::type type;
};
 
template <class T, T p2, T... Idx>
struct B<T, p2, p2, A<T, Idx...>> {
  typedef A<int, Idx...> type;
};
 
template <class> struct D;
template <int I> struct D<i<I>> {
  using type = i<>;
};
 
template <int, class... Args>
auto foo(Args...) ->
  decltype(D<typename B<int, 0, sizeof...(Args), A<int>>::type>{});

void baz() {
  foo<1>(0);
}
