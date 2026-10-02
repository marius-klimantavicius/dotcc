//options_all:--c++20 -tused
  template<bool> struct A { };
  template<auto> struct B;
  template<auto X, void (*F)() noexcept(X)> struct B<F> {
    A<X> ax;
  };
  void f_nothrow() noexcept;
  B<f_nothrow> bn;   // OK: type of X deduced as bool
