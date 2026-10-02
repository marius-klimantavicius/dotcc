//type:cp
//options_all:--c++11

template <typename T, typename U>
struct A {
  static constexpr bool value = true;
};

template <bool B, typename T>
struct enable_if { };

template <typename T>
struct enable_if<true, T> {
  using type = T;
};

template <typename... Ts>
struct Base {
  /* The joint expansion of Ts/Args causes the Ts symbol to be left around
     until it can be substitued as the same time with Args.  Thus, when we
     instantiate this ctor, we'll need to have pushed the template instantiation
     scope for Base.  When we instantiate inherited versions of this ctor they
     still use the sample template parameters and thus also need the template
     instantiation scope for Base, which was not happening.  The lack of this
     instantiation scope causes the lookup for the Ts argument to spuriously
     fail, which triggers SFINAE because it treats Ts as an empty pack (zero
     elements), which apparently mismatches with Args (which has one
     parameter). */
  template<typename... Args,
           typename enable_if<A<Ts, Args>::value..., int>::type = 0>
  Base(Args && ...) { }
};

template <typename T>
struct Derived : T {
  using T::T;
};

int main()
{
  Derived<Base<int>> var{0};
  return 0;
}
