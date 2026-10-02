//type: fn
//options: --c++26
# 0 "./cpp26/constexpr-new3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp26/constexpr-new3.C"



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
# 5 "./cpp26/constexpr-new3.C" 2

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
  new (b + 1) int[] {2, 3};
  a.deallocate (b, 3);
  return true;
}

constexpr bool
bar ()
{
  std::allocator<int> a;
  auto b = a.allocate (3);
  new (b) int[] {1, 2, 3, 4};
  a.deallocate (b, 3);
  return true;
}

constexpr bool
baz ()
{
  std::allocator<int> a;
  auto b = a.allocate (2);
  new (b) long (42);
  a.deallocate (b, 2);
  return true;
}

constexpr bool a = foo ();
constexpr bool b = bar ();
constexpr bool c = baz ();
