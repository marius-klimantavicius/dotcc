//type:fp
//options:--c++17:--c++17 --clang:--ms_c++17:--c++17 -DFAIL=1;fn
enum CLS
{
  PRIMARY_TMPL,
  IN_CLASS_EXPLICIT_SPEC,
  OUT_OF_CLASS_EXPLICIT_SPEC
};

namespace static_mbr_fn
{
  template<int I>
  struct C
  {
    template<int J>
    static constexpr CLS fn()
    {
      return PRIMARY_TMPL;
    }

    template<>
    constexpr CLS fn<0>()
    {
      return IN_CLASS_EXPLICIT_SPEC;
    }
  };

  template<> template<>
  constexpr CLS C<0>::fn<0>()
  {
    return OUT_OF_CLASS_EXPLICIT_SPEC;
  }

  static_assert(C<0>::fn<0>() == OUT_OF_CLASS_EXPLICIT_SPEC);
  static_assert(C<0>::fn<1>() == PRIMARY_TMPL);
  static_assert(C<1>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<1>::fn<1>() == PRIMARY_TMPL);
}

namespace mbr_fn
{
  template<int I>
  struct C
  {
    template<int J>
    constexpr CLS fn()
    {
      return PRIMARY_TMPL;
    }

    template<>
    constexpr CLS fn<0>()
    {
      return IN_CLASS_EXPLICIT_SPEC;
    }
  };

  template<> template<>
  constexpr CLS C<0>::fn<0>()
  {
    return OUT_OF_CLASS_EXPLICIT_SPEC;
  }

  static_assert(C<0>().fn<0>() == OUT_OF_CLASS_EXPLICIT_SPEC);
  static_assert(C<0>().fn<1>() == PRIMARY_TMPL);
  static_assert(C<1>().fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<1>().fn<1>() == PRIMARY_TMPL);
}

namespace static_mbr_var
{
  template<int I>
  struct C
  {
    template<int J>
    static constexpr CLS var = PRIMARY_TMPL;

    template<>
    constexpr CLS var<0> = IN_CLASS_EXPLICIT_SPEC;
  };

  template<> template<>
  constexpr CLS C<0>::var<0> = OUT_OF_CLASS_EXPLICIT_SPEC;


  static_assert(C<0>::var<0> == OUT_OF_CLASS_EXPLICIT_SPEC);
  static_assert(C<0>::var<1> == PRIMARY_TMPL);
  static_assert(C<1>::var<0> == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<1>::var<1> == PRIMARY_TMPL);
}

namespace static_mbr_var_out_of_class_init
{
  struct C
  {
    template<int J>
    static int var;

    template<>
    int var<0>;
  };

  template<>
  int C::var<0> = 0;


  template<int I>
  struct X
  {
    template<int J>
    static int var;

    template<>
    int var<0>;
  };

  template<> template<>
  int X<0>::var<0> = 0;
}

#ifdef FAIL
namespace use_before_spec
{
  template<int I>
  struct C
  {
    template<int J>
    static constexpr CLS fn()
    {
      return PRIMARY_TMPL;
    }

    template<>
    constexpr CLS fn<0>()
    {
      return IN_CLASS_EXPLICIT_SPEC;
    }
  };

  static_assert(C<0>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);

  template<> template<>
  constexpr CLS C<0>::fn<0>()   // already defined
  {
    return OUT_OF_CLASS_EXPLICIT_SPEC;
  }

  static_assert(C<0>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<0>::fn<1>() == PRIMARY_TMPL);
  static_assert(C<1>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<1>::fn<1>() == PRIMARY_TMPL);
}

namespace use_before_spec
{
  template<int I>
  struct C
  {
    template<int J>
    static constexpr auto fn()
    {
      return PRIMARY_TMPL;
    }

    template<>
    constexpr auto fn<0>()
    {
      return IN_CLASS_EXPLICIT_SPEC;
    }
  };

  // trigger return type deduction
  static_assert(sizeof(C<0>::fn<0>()) == sizeof(CLS), "Unexpected");

  template<> template<>
  constexpr auto C<0>::fn<0>()   // already defined
  {
    return OUT_OF_CLASS_EXPLICIT_SPEC;
  }

  static_assert(C<0>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<0>::fn<1>() == PRIMARY_TMPL);
  static_assert(C<1>::fn<0>() == IN_CLASS_EXPLICIT_SPEC);
  static_assert(C<1>::fn<1>() == PRIMARY_TMPL);
}

namespace non_tmpl
{
  struct C
  {
    template<int J>
    static void fn()
    { }

    template<>
    void fn<0>()
    { }

    template<>
    void fn<1>()
    { }

    template<>
    void fn<1>()                // already defined
    { }

    template<int J>
    static int var;

    template<>
    inline int var<0> = 0;

    template<>
    inline int var<0> = 0;      // already defined

    template<>
    inline int var<1> = 1;
  };

  template<>
  void C::fn<0>()               // already defined
  { }

  template<>
  inline int C::var<1> = 1;     // already defined
}

namespace tmpl
{
  template<int I>
  struct C
  {
    template<int J>
    static void fn()
    { }

    template<>
    void fn<0>()
    { }

    template<>
    void fn<0>()                // already defined
    { }

    template<int J>
    static int var;

    template<>
    inline int var<0> = 0;

    template<>
    inline int var<0> = 0;      // already defined
  };

  C<0> c0;
}
#endif
