//type:cp
//options::--g++
//options_all:--c++14

struct S {};

template<typename, typename> struct X {};

template<typename> struct A {
  static constexpr bool val = true;
};

template<bool, typename = void> struct B;
template<typename T> struct B<true, T> { typedef T type; };

template<typename... T> struct C : S {
  C(const T&...) = delete; // EDG takes this path
  template<typename... U,
	   typename = typename B<A<X<T, U>...>::val>::type
	  >
  C(U&&...); // GCC, Clang, MSVC take this path
};

struct D : C<S> {
  using C::C;
};

void f(S s) {
  D var(s);
}
