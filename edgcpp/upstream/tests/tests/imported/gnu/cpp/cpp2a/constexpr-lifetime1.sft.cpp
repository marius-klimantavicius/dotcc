//type: fp
//options: --c++20
# 0 "./cpp2a/constexpr-lifetime1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/constexpr-lifetime1.C"


# 1 "./cpp2a/construct_at.h" 1



namespace std
{
  typedef long unsigned int size_t;

  template <typename T>
  struct allocator
  {
    constexpr allocator () noexcept {}

    constexpr T *allocate (size_t n)
    { return static_cast<T *> (::operator new (n * sizeof(T))); }

    constexpr void
    deallocate (T *p, size_t n)
    { ::operator delete (p); }
  };

  template <typename T, typename U = T &&>
  U __declval (int);
  template <typename T>
  T __declval (long);
  template <typename T>
  auto declval () noexcept -> decltype (__declval<T> (0));

  template <typename T>
  struct remove_reference
  { typedef T type; };
  template <typename T>
  struct remove_reference<T &>
  { typedef T type; };
  template <typename T>
  struct remove_reference<T &&>
  { typedef T type; };

  template <typename T>
  constexpr T &&
  forward (typename std::remove_reference<T>::type &t) noexcept
  { return static_cast<T&&> (t); }

  template<typename T>
  constexpr T &&
  forward (typename std::remove_reference<T>::type &&t) noexcept
  { return static_cast<T&&> (t); }

  template <typename T, typename... A>
  constexpr auto
  construct_at (T *l, A &&... a)
  noexcept (noexcept (::new ((void *) 0) T (std::declval<A> ()...)))
  -> decltype (::new ((void *) 0) T (std::declval<A> ()...))
  { return ::new ((void *) l) T (std::forward<A> (a)...); }

  template <typename T>
  constexpr inline void
  destroy_at (T *l)
  { l->~T (); }
}




inline

void *operator new (std::size_t, void *p) noexcept
{ return p; }




inline

void *operator new[] (std::size_t, void *p) noexcept
{ return p; }
# 4 "./cpp2a/constexpr-lifetime1.C" 2

struct S { int x; };
constexpr int f() {
  S s;
  s.~S();
  std::construct_at(&s, 5);
  return s.x;
}
static_assert(f() == 5);

struct T { int x; constexpr ~T() {} };
constexpr int g() {
  T t;
  t.~T();
  std::construct_at(&t, 12);
  return t.x;
}
static_assert(g() == 12);
