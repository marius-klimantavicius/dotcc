//type: fn
//options: --c++20
# 0 "./cpp2a/constexpr-union6.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/constexpr-union6.C"



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
# 5 "./cpp2a/constexpr-union6.C" 2

struct S { const int a; int b; };
union U { int k; S s; };

constexpr int test1() {
  U u {};
  std::construct_at(&u.s, S{ 1, 2 });
  return u.s.b;
}
static_assert(test1() == 2);

constexpr int test2() {
  U u {};
  int* p = &u.s.b;
  std::construct_at(p, 5);
  return u.s.b;
}
constexpr int x2 = test2();

constexpr void foo(S* s) {
  s->b = 10;
}
constexpr int test3() {
  U u {};
  foo(&u.s);
  return u.s.b;
}
constexpr int x3 = test3();

struct S2 { int a; int b; };
union U2 { int k; S2 s; };
constexpr int test4() {
  U2 u;
  int* p = &u.s.b;
  std::construct_at(p, 8);
  return u.s.b;
};
constexpr int x4 = test4();

constexpr int test5() {
  union {
    int data[1];
  } u;
  std::construct_at(u.data, 0);
  return 0;
}
constexpr int x5 = test5();
