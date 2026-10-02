//options_all:--c++20 -tused -A
template<typename, typename>
constexpr bool is_same_v = false;
template<typename T>
constexpr bool is_same_v<T, T> = true;

template <class T>
struct A {
  T ar[4];
};
A a = { "foo" };

static_assert(is_same_v<decltype(a), A<char>>);

//cwg: 2685
//title: Aggregate CTAD, string, and brace elision
//meeting: Issaquah 2/23
//edg_status: EDGcpfe/26074
