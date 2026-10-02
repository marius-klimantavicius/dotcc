//type:fp
//options:--c++20:--ms_c++20
//options_all:-w

namespace minimal
{
  struct C {
    constexpr C() = default;
    constexpr C(const C &o) : i(o.i) { }
    int i{1};
  };
  struct X {
    C c{};
  };
  constexpr auto f(auto ... x) {
    auto l = [... cx = x]() { return (cx.c.i + ...); };
    return l;
  };
  static_assert(f(X(), X())() == 2);
}

namespace const_eval
{
  struct C
  {
    constexpr C() : value(42) { }
    constexpr C(const C& other) : value(other.value) { }
    int value;
  };

  constexpr auto g(auto x, int) { return x; }

  constexpr auto f(auto ... v)
  {
    auto l = [... cv = v]() { return g(cv ...); };
    return l;
  };

  struct X
  {
    C c{};
  };

  static_assert(f(X(), 1)().c.value == 42);
}

namespace const_eval_with_additional_object
{
  struct C
  {
    constexpr C() = default;
    constexpr C(const C &) { }
  };

  constexpr auto g(auto x, int) { return x; }

  constexpr auto f(auto ... v)
  {
    auto l = [... cv = v]() { return g(cv ...); };
    return l;
  };

  struct X
  {
    int i{1};
    C c{};
  };

  static_assert(f(X(), 1)().i == 1);
}

namespace implicit_this_pointer_capture
{
  struct X
  {
    constexpr X(int v)
      : i(v)
    { }

    constexpr X(const X &o)
      : i(o.i)
    { }

    int i;
  };

  struct C
  {
    constexpr auto f(X x)
    {
      auto l = [=] () {
        return i + x.i;
      };

      return l;
    }

    int i;
  };

  static_assert(C{ 1 }.f(X{ 2 })() == 3);
}

namespace explicit_this_pointer_capture
{
  struct X
  {
    constexpr X(int v)
      : i(v)
    { }

    constexpr X(const X &o)
      : i(o.i)
    { }

    int i;
  };

  struct C
  {
    constexpr auto f(X x)
    {
      auto l = [this, x] () {
        return i + x.i;
      };

      return l;
    }

    int i;
  };

  static_assert(C{ 1 }.f(X{ 2 })() == 3);
}

namespace explicit_this_value_capture
{
  struct X
  {
    constexpr X(int v)
      : i(v)
    { }

    constexpr X(const X &o)
      : i(o.i)
    { }

    int i;
  };

  struct C
  {
    constexpr auto f(X x)
    {
      auto l = [*this, x] () {
        return i + x.i;
      };

      return l;
    }

    int i;
  };

  static_assert(C{ 1 }.f(X{ 2 })() == 3);
}

namespace only_this_capture
{
  struct C
  {
    constexpr C(int i)
      : m(i)
    { }

    constexpr C(const C &o)
      : m(o.m)
    { }

    constexpr auto f()
    {
      auto l = [this] () {
        return m;
      };
      return l;
    }

    int m;
  };

  static_assert(C(42).f()() == 42);
}

namespace only_this_value_capture
{
  struct C
  {
    constexpr C(int i)
      : m(i)
    { }

    constexpr C(const C &o)
      : m(o.m)
    { }

    constexpr auto f()
    {
      auto l = [*this] () {
        return m;
      };
      return l;
    }

    int m;
  };

  static_assert(C(42).f()() == 42);
}
