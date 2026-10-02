//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1930
//options_all:-tused

namespace minimal
{
  struct C {
    constexpr C() = default;
    constexpr C(const C &) = default;
    constexpr C(C &&c) { c.moved = true; }
    bool moved = false;
  };
  constexpr bool f(C c) {
    C{c};
    return !c.moved;
  }
  static_assert(f({}), "Unexpected");
}

namespace tmpl_defaulted_copy
{
  template<typename T>
  struct R
  {
    constexpr R()
    { }

    constexpr R(const R&) = default;

    constexpr R(R&& r)
    { i = 2; }

    int i = 0;
  };


  constexpr R<int> f()
  {
    R<int> r;
    return R<int>{r};
  }

  static_assert(f().i == 0);


  constexpr int g()
  {
    R<int> r;
    return R<int>{r}.i;
  }

  static_assert(g() == 0);
}

namespace tmpl_user_defined_copy
{
  template<typename T>
  struct R
  {
    constexpr R()
    { }

    constexpr R(const R&)
    { i = 1; }

    constexpr R(R&& r)
    { i = 2; }

    int i = 0;
  };


  constexpr R<int> f()
  {
    R<int> r;
    return R<int>{r};
  }

  static_assert(f().i == 1);


  constexpr int g()
  {
    R<int> r;
    return R<int>{r}.i;
  }

  static_assert(g() == 1);
}

namespace non_tmpl_defaulted_copy
{
  struct R
  {
    constexpr R()
    { }

    constexpr R(const R&) = default;

    constexpr R(R&& r)
    { i = 2; }

    int i = 0;
  };


  constexpr R f()
  {
    R r;
    return R{r};
  }

  static_assert(f().i == 0);


  constexpr int g()
  {
    R r;
    return R{r}.i;
  }

  static_assert(g() == 0);
}

namespace non_tmpl_user_defined_copy
{
  struct R
  {
    constexpr R()
    { }

    constexpr R(const R&)
    { i = 1; }

    constexpr R(R&& r)
    { i = 2; }

    int i = 0;
  };


  constexpr R f()
  {
    R r;
    return R{r};
  }

  static_assert(f().i == 1);


  constexpr int g()
  {
    R r;
    return R{r}.i;
  }

  static_assert(g() == 1);
}

namespace extra_temporary_rv_ref
{
  struct A
  {
    constexpr operator int&&()
    {
      return static_cast<int &&>(i);
    }

    int i = 0;
  };

  constexpr int foo()
  {
    A a;
    int &&i{ a };
    ++a.i;
    return i;
  }

  static_assert(foo() == 0, "foo()::i binds to temporary");
}

namespace extra_temporary_lv_ref
{
  struct A
  {
    constexpr operator int&()
    {
      return i;
    }

    int i = 0;
  };

  constexpr int foo()
  {
    A a;
    const int &i{ a };
    ++a.i;
    return i;
  }

  static_assert(foo() == 0, "foo()::i binds to temporary");
}

namespace copies_and_dtors
{
  struct R
  {
    constexpr R(int &r)
      : r(r)
    { }

    constexpr ~R()
    {
      ++r;
    }

    constexpr R(const R&) = default;

    constexpr R(R&& o)
      : i(2), r(o.r)
    { }

    int i = 0;
    int &r;
  };

  constexpr const R & f(const R &r1, const R &r2, bool &o)
  {
    o = (&r1 != &r2);
    return r1;
  }

  constexpr bool g()
  {
    bool objs = false;
    int dtors = 0;
    R r(dtors);

    int i = f(R{r}, r, objs).i;

    return i == 0 && (dtors != 0) == objs;
  }

  static_assert(g(), "number of copies need to match dtor calls");
}

namespace constrained_move
{
  template<typename T = void>
  struct R
  {
    R() = default;

    R(const R&) = default;
    R(R&& r) requires false;
  };

  using RR = R<>;
  using CRR = const R<>;

  RR r;
  const RR cr;

  RR getR();
  const RR getCR();

  RR &getRRef();
  const RR &getCRRef();

  RR &&getRRvRef();
  const RR &&getCRRvRef();

  void f()
  {
    RR{r};
    RR{cr};
    RR{getR()};
    RR{getCR()};
    RR{getRRef()};
    RR{getCRRef()};
    RR{getRRvRef()};
    RR{getCRRvRef()};

    CRR{r};
    CRR{cr};
    CRR{getR()};
    CRR{getCR()};
    CRR{getRRef()};
    CRR{getCRRef()};
    CRR{getRRvRef()};
    CRR{getCRRvRef()};
  }
}

namespace dont_move
{
  template<typename T = void>
  struct R
  {
    R() = default;

    R(const R&) = default;
    R(R&& r) { T::invalid; };
  };

  using RR = R<>;
  using CRR = const R<>;

  RR r;
  const RR cr;

  RR getR();
  const RR getCR();

  RR &getRRef();
  const RR &getCRRef();

  void f()
  {
    RR{r};
    RR{cr};
    RR{getR()};
    RR{getCR()};
    RR{getRRef()};
    RR{getCRRef()};

    CRR{r};
    CRR{cr};
    CRR{getR()};
    CRR{getCR()};
    CRR{getRRef()};
    CRR{getCRRef()};
  }
}
