//type: fp
//options: --c++26
# 0 "./cpp26/constexpr-new1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp26/constexpr-new1.C"



# 1 "./cpp26/../cpp2a/construct_at.h" 1



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


constexpr



void *operator new (std::size_t, void *p) noexcept
{ return p; }


constexpr



void *operator new[] (std::size_t, void *p) noexcept
{ return p; }
# 5 "./cpp26/constexpr-new1.C" 2

struct S {
  constexpr S () : a (42), b (43) {}
  constexpr S (int c, int d) : a (c), b (d) {}
  int a, b;
};
struct T {
  int a, b;
};

constexpr bool
foo ()
{
  std::allocator<int> a;
  auto b = a.allocate (3);
  ::new (b) int ();
  ::new (b + 1) int (1);
  ::new (b + 2) int {2};
  if (b[0] != 0 || b[1] != 1 || b[2] != 2)
    return false;
  a.deallocate (b, 3);
  std::allocator<S> c;
  auto d = c.allocate (4);
  ::new (d) S;
  ::new (d + 1) S ();
  ::new (d + 2) S (7, 8);
  ::new (d + 3) S { 9, 10 };
  if (d[0].a != 42 || d[0].b != 43
      || d[1].a != 42 || d[1].b != 43
      || d[2].a != 7 || d[2].b != 8
      || d[3].a != 9 || d[3].b != 10)
    return false;
  d[0].~S ();
  d[1].~S ();
  d[2].~S ();
  d[3].~S ();
  c.deallocate (d, 4);
  std::allocator<T> e;
  auto f = e.allocate (3);
  ::new (f) T ();
  ::new (f + 1) T (7, 8);
  ::new (f + 2) T { .a = 9, .b = 10 };
  if (f[0].a != 0 || f[0].b != 0
      || f[1].a != 7 || f[1].b != 8
      || f[2].a != 9 || f[2].b != 10)
    return false;
  f[0].~T ();
  f[1].~T ();
  f[2].~T ();
  e.deallocate (f, 3);
  auto g = a.allocate (3);
  new (g) int[] {1, 2, 3};
  if (g[0] != 1 || g[1] != 2 || g[2] != 3)
    return false;
  new (g) int[] {4, 5};
  if (g[0] != 4 || g[1] != 5)
    return false;
  a.deallocate (g, 3);
  return true;
}

static_assert (foo ());
