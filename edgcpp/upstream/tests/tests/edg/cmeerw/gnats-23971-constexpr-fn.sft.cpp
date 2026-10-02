//type:fp
//options:--c++11 --strict;fn:--c++14 --strict:--c++17 --strict:--c++20 --strict:--c++23 --strict
enum CLS
{
  PRIMARY_TEMPLATE,
  EXPLICIT_SPEC,
};

struct expl_tag_t
{ };

namespace mbr_fn_tmpl_out_of_class_spec
{
  template<typename T>
  struct Outer
  {
    int m;

    constexpr Outer()
      : m(0)
    { }

    template<typename U>
    constexpr CLS inner(U)
    {
      return m != 0 ? CLS(-1) : PRIMARY_TEMPLATE;
    }
  };

  // known issue: we are not currently able to match this in C++11 mode (as we
  // don't apply an implicit const qualifier here before trying to match to the
  // primary template)
  template<>
  template<>
  constexpr CLS Outer<int>::inner(expl_tag_t)
  {
    return m != 0 ? CLS(-1) : EXPLICIT_SPEC;
  }

  static_assert(Outer<int>().inner(1) == PRIMARY_TEMPLATE, "Unexpected");
  static_assert(Outer<int>().inner(expl_tag_t()) == EXPLICIT_SPEC, "Unexpected");
}

namespace mbr_fn_tmpl_in_class_spec
{
  template<typename T>
  struct Outer
  {
    int m;

    constexpr Outer()
      : m(0)
    { }

    template<typename U>
    constexpr CLS inner(U)
    {
      return m != 0 ? CLS(-1) : PRIMARY_TEMPLATE;
    }

    template<>
    constexpr CLS inner(expl_tag_t)
    {
      return m != 0 ? CLS(-1) : EXPLICIT_SPEC;
    }
  };

  static_assert(Outer<int>().inner(1) == PRIMARY_TEMPLATE, "Unexpected");
  static_assert(Outer<int>().inner(expl_tag_t()) == EXPLICIT_SPEC, "Unexpected");
}

namespace static_mbr_fn_tmpl_out_of_class_spec
{
  template<typename T>
  struct Outer
  {
    template<typename U>
    static constexpr CLS inner(U)
    {
      return PRIMARY_TEMPLATE;
    }
  };

  template<>
  template<>
  constexpr CLS Outer<int>::inner(expl_tag_t)
  {
    return EXPLICIT_SPEC;
  }

  static_assert(Outer<int>::inner(1) == PRIMARY_TEMPLATE, "Unexpected");
  static_assert(Outer<int>::inner(expl_tag_t()) == EXPLICIT_SPEC, "Unexpected");
}

namespace static_mbr_fn_tmpl_in_class_spec
{
  template<typename T>
  struct Outer
  {
    template<typename U>
    static constexpr CLS inner(U)
    {
      return PRIMARY_TEMPLATE;
    }

    // known issue: we are not currently able to match this in C++11 mode (as
    // we apply an implicit const qualifier here before trying to match to the
    // primary template)
    template<>
    constexpr CLS inner(expl_tag_t)
    {
      return EXPLICIT_SPEC;
    }
  };

  static_assert(Outer<int>::inner(1) == PRIMARY_TEMPLATE, "Unexpected");
  static_assert(Outer<int>::inner(expl_tag_t()) == EXPLICIT_SPEC, "Unexpected");
}
