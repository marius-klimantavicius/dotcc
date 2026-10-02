//type: fn
//options:  --c++20
# 1 "SemaTemplate/concepts-recursive-inst.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 493 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaTemplate/concepts-recursive-inst.cpp" 2

namespace GH53213 {
template<typename T>
concept c = requires(T t) { f(t); };

auto f(c auto);

void g() {
  f(0);




}
}

namespace GH45736 {
struct constrained;

template<typename T>
  struct type {
  };
template<typename T>
  constexpr bool f(type<T>) {
      return true;
  }

template<typename T>
  concept matches = f(type<T>());


struct constrained {
    template<typename U> requires matches<U>
        explicit constrained(U value) {
            }
};

bool f(constrained const &) {
    return true;
}

struct outer {
    constrained state;
};

bool f(outer const & x) {
    return f(x.state);
}
}

namespace DirectRecursiveCheck {
template<class T>
concept NotInf = true;
template<class T>
concept Inf = requires(T& v){
  {begin(v)};
};

void begin(NotInf auto& v){ }



void begin(Inf auto& v){ }

struct my_range{
} rng;

void baz() {
auto it = begin(rng);
# 85 "SemaTemplate/concepts-recursive-inst.cpp"
}
}

namespace GH50891 {
  template <typename T>
  concept Numeric = requires(T a) {
      foo(a);
    };

  struct Deferred {
    friend void foo(Deferred);
    template <Numeric TO> operator TO();
  };

  static_assert(Numeric<Deferred>);
# 114 "SemaTemplate/concepts-recursive-inst.cpp"
}


namespace GH60323 {

  struct End {
        template<class T>
              void go(T t) { }

            template<class T>
                  auto endparens(T t)
                          requires requires { go(t); }
                { return go(t); }
  };

  struct Size {
        template<class T>
              auto go(T t)
                  { return End().endparens(t); }

            template<class T>
                  auto sizeparens(T t)
                          requires requires { go(t); }
                { return go(t); }
  };

  int f()
  {
        int i = 42;
            Size().sizeparens(i);
  }
}

namespace CWG2369_Regressions {


namespace GCC_103997 {

template<typename _type, typename _stream>
concept streamable = requires(_stream &s, _type &&v) {
  s << static_cast<_type &&>(v);
};

struct type_a {
  template<typename _arg>
  type_a &operator<<(_arg &&) {

    return *this;
  }
};

struct type_b {
  type_b &operator<<(type_a const &) {

    return *this;
  }
};

struct type_c {
  type_b b;
  template<typename _arg>
  requires streamable<_arg, type_b>
  friend type_c &operator<<(type_c &c, _arg &&a) {

    c.b << static_cast<_arg &&>(a);
    return c;
  }
};

void foo() {
  type_a a;
  type_c c;
  a << c;
  c << a;
}

}


namespace GCC_108393 {

template<class>
struct iterator_traits
{};

template<class T>
  requires requires(T __t, T __u) { __t == __u; }
struct iterator_traits<T>
{};

template<class T>
concept C = requires { typename iterator_traits<T>::A; };

struct unreachable_sentinel_t
{
  template<C _Iter>
  friend constexpr bool operator==(unreachable_sentinel_t, const _Iter&) noexcept;
};

template<class T>
struct S
{};

static_assert(!C<S<unreachable_sentinel_t>>);

}


namespace GCC_107429 {

struct tag_foo { } inline constexpr foo;
struct tag_bar { } inline constexpr bar;

template<typename... T>
auto f(tag_foo, T... x)
{
  return (x + ...);
}

template<typename... T>
concept fooable = requires (T... x) { f(foo, x...); };

template<typename... T> requires (fooable<T...>)
auto f(tag_bar, T... x)
{
  return f(foo, x...);
}

auto test()
{
  return f(bar, 1, 2, 3);
}

}

namespace GCC_99599 {

struct foo_tag {};
struct bar_tag {};

template <class T>
concept fooable = requires(T it) {
  invoke_tag(foo_tag{}, it);
};

template <class T> auto invoke_tag(foo_tag, T in) { return in; }

template <fooable T> auto invoke_tag(bar_tag, T it) { return it; }

int main() {

  return invoke_tag(foo_tag{}, 2) + invoke_tag(bar_tag{}, 2);
}

}


namespace GCC_99599_2 {

template<typename T> class indirect {
public:
  template<typename U> requires
    requires (const T& t, const U& u) { t == u; }
  friend constexpr bool operator==(const indirect&, const U&) { return false; }

private:
  T* _M_ptr{};
};

indirect<int> i;
bool b = i == 1;

}

namespace GCC_99599_3 {

template<typename T>
struct S { T t; };

template<typename T>
concept C = sizeof(S<T>) > 0;

struct I;

struct from_range_t {
    explicit from_range_t() = default;
};
inline constexpr from_range_t from_range;

template<typename T>
concept FromRange = __is_same_as (T, from_range_t);






template<C T>
void f(from_range_t, T*);


void f(...);

void g(I* p) {
  f(0, p);
}

}

namespace GCC_99599_4 {

struct A {
  A(...);
};

template <class T> void f(A, T) { }

int main()
{
  f(42, 24);
}

}

namespace FAILED_GCC_110160 {
# 363 "SemaTemplate/concepts-recursive-inst.cpp"
}
}
