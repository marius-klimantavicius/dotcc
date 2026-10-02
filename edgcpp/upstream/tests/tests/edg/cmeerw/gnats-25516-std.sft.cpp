//type:fp
//options:-DPOS:-DNEG;fn
//options_all:--c++23 -A -tused

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace std
{
  using size_t = decltype(sizeof 0);
}

template <typename T> struct B {
  B(T);
};
template <typename T> struct C : public B<T> {
  using B<T>::B;
};
template <typename T> struct D : public B<T> {};

C c(42);            // OK, deduces C<int>
static_assert(is_same_v<decltype(c), C<int>>);
#if NEG
D d(42);            // error: deduction failed, no inherited deduction guides
#endif
B(int) -> B<char>;
C c2(42);           // OK, deduces C<char>
static_assert(is_same_v<decltype(c2), C<char>>);

template <typename T> struct E : public B<int> {
  using B<int>::B;
};

#if NEG
E e(42);            // error: deduction failed, arguments of E cannot be deduced from introduced guides
#endif

template <typename T, typename U, typename V> struct F {
  F(T, U, V);
};
template <typename T, typename U> struct G : F<U, T, int> {
  using G::F::F;
};

G g(true, 'a', 1);  // OK, deduces G<char, bool>
static_assert(is_same_v<decltype(g), G<char, bool>>);

template<class T, std::size_t N>
struct H {
  T array[N];
};
template<class T, std::size_t N>
struct I {
  volatile T array[N];
};
template<std::size_t N>
struct J {
  unsigned char array[N];
};

H h = { "abc" };    // OK, deduces H<char, 4> (not T = const char)
static_assert(is_same_v<decltype(h), H<char, 4>>);
I i = { "def" };    // OK, deduces I<char, 4>
static_assert(is_same_v<decltype(i), I<char, 4>>);
#if NEG
J j = { "ghi" };    // error: cannot bind reference to array of unsigned char to array of char in deduction
#endif
