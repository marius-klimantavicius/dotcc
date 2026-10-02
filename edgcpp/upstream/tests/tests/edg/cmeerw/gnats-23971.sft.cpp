//type:fp
//options:--c++17 --strict:--c++20 --strict:--c++17 --clang:--c++20 --clang:--ms_c++17:--ms_c++20
enum CLS
{
  PRIMARY_TEMPLATE,
  PARTIAL_SPEC,
  EXPLICIT_SPEC,
};

struct expl_tag_t
{ };


namespace class_tmpl
{
  template<typename T>
  struct Outer
  {
    template<typename U>
    struct Inner
    {
      static constexpr CLS cls = PRIMARY_TEMPLATE;
    };

    template<typename U>
    struct Inner<U *>
    {
      static constexpr CLS cls = PARTIAL_SPEC;
    };

    template<>
    struct Inner<expl_tag_t>
    {
      static constexpr CLS cls = EXPLICIT_SPEC;
    };
  };

  static_assert(Outer<int>::Inner<int>::cls == PRIMARY_TEMPLATE);
  static_assert(Outer<int>::Inner<int *>::cls == PARTIAL_SPEC);
  static_assert(Outer<int>::Inner<expl_tag_t>::cls == EXPLICIT_SPEC);
}


namespace mbr_fn_tmpl
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

    template<typename U>
    constexpr CLS inner(U *)
    {
      return m != 0 ? CLS(-1) : PARTIAL_SPEC;
    }

    template<>
    constexpr CLS inner(expl_tag_t)
    {
      return m != 0 ? CLS(-1) : EXPLICIT_SPEC;
    }
  };

  constexpr int i = 1;

  static_assert(Outer<int>().inner(1) == PRIMARY_TEMPLATE);
  static_assert(Outer<int>().inner(&i) == PARTIAL_SPEC);
  static_assert(Outer<int>().inner(expl_tag_t()) == EXPLICIT_SPEC);
}


namespace cv_qual_mbr_fn_tmpl
{
  template<typename T>
  struct Outer
  {
    int m;

    constexpr Outer()
      : m(0)
    { }

    template<typename U>
    constexpr CLS inner(U) const
    {
      return m != 0 ? CLS(-1) : PRIMARY_TEMPLATE;
    }

    template<typename U>
    constexpr CLS inner(U *) const
    {
      return m != 0 ? CLS(-1) : PARTIAL_SPEC;
    }

    template<>
    constexpr CLS inner(expl_tag_t) const
    {
      return m != 0 ? CLS(-1) : EXPLICIT_SPEC;
    }
  };

  constexpr int i = 1;

  static_assert(Outer<int>().inner(1) == PRIMARY_TEMPLATE);
  static_assert(Outer<int>().inner(&i) == PARTIAL_SPEC);
  static_assert(Outer<int>().inner(expl_tag_t()) == EXPLICIT_SPEC);
}


namespace static_mbr_fn_tmpl
{
  template<typename T>
  struct Outer
  {
    template<typename U>
    static constexpr CLS inner(U)
    {
      return PRIMARY_TEMPLATE;
    }

    template<typename U>
    static constexpr CLS inner(U *)
    {
      return PARTIAL_SPEC;
    }

    template<>
    constexpr CLS inner(expl_tag_t)
    {
      return EXPLICIT_SPEC;
    }
  };

  constexpr int i = 1;

  static_assert(Outer<int>::inner(1) == PRIMARY_TEMPLATE);
  static_assert(Outer<int>::inner(&i) == PARTIAL_SPEC);
  static_assert(Outer<int>::inner(expl_tag_t()) == EXPLICIT_SPEC);
}


namespace static_mbr_data_tmpl
{
  template<typename T>
  struct Outer
  {
    template<typename U>
    static constexpr CLS inner = PRIMARY_TEMPLATE;

    template<typename U>
    static constexpr CLS inner<U *> = PARTIAL_SPEC;;

    template<>
    constexpr CLS inner<expl_tag_t> = EXPLICIT_SPEC;
  };

  static_assert(Outer<int>::inner<int> == PRIMARY_TEMPLATE);
  static_assert(Outer<int>::inner<int *> == PARTIAL_SPEC);
  static_assert(Outer<int>::inner<expl_tag_t> == EXPLICIT_SPEC);
}


namespace expl_spec_dpdt_defn
{
  template<bool B, typename T1, typename T2>
  struct choose_type
  {
    using type = T2;
  };

  template<typename T1, typename T2>
  struct choose_type<true, T1, T2>
  {
    using type = T1;
  };

  template<bool B, typename T1, typename T2>
  using choose_type_t = typename choose_type<B, T1, T2>::type;

  template<bool B>
  struct Outer
  {
    template<bool BB>
    struct Inner
    {
      static constexpr CLS cls = PRIMARY_TEMPLATE;
    };

    template<>
    struct Inner<B>
    {
      static constexpr CLS cls = EXPLICIT_SPEC;
    };

    template<bool BB>
    static constexpr CLS inner_var = PRIMARY_TEMPLATE;

    template<>
    constexpr CLS inner_var<B> = Inner<B>::cls;

    template<bool BB>
    static constexpr CLS inner_fn()
    {
      return PRIMARY_TEMPLATE;
    }

    template<>
    constexpr CLS inner_fn<B>()
    {
      return Inner<B>::cls;
    }

    template<typename U>
    static constexpr CLS inner_dpdt_fn(U u)
    {
      return PRIMARY_TEMPLATE;
    }

    template<>
    constexpr CLS inner_dpdt_fn(choose_type_t<B, int, char>)
    {
      return EXPLICIT_SPEC;
    }
  };

  static_assert(Outer<true>::Inner<true>::cls == EXPLICIT_SPEC);
  static_assert(Outer<true>::Inner<false>::cls == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::Inner<true>::cls == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::Inner<false>::cls == EXPLICIT_SPEC);

  static_assert(Outer<true>::inner_var<true> == EXPLICIT_SPEC);
  static_assert(Outer<true>::inner_var<false> == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_var<true> == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_var<false> == EXPLICIT_SPEC);

  static_assert(Outer<true>::inner_fn<true>() == EXPLICIT_SPEC);
  static_assert(Outer<true>::inner_fn<false>() == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_fn<true>() == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_fn<false>() == EXPLICIT_SPEC);

  static_assert(Outer<true>::inner_dpdt_fn(0) == EXPLICIT_SPEC);
  static_assert(Outer<true>::inner_dpdt_fn('c') == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_dpdt_fn(0) == PRIMARY_TEMPLATE);
  static_assert(Outer<false>::inner_dpdt_fn('c') == EXPLICIT_SPEC);
}
