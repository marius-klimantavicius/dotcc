//type:fn
//options:--c++17:--c++17 --clang_version=120000:--ms_c++17
namespace non_tmpl
{
  struct C
  {
    int m;

    template<typename U>
    static void impl_static_spec(U)
    {
      ++m;                      // reference to non-static member
    }

    template<>
    void impl_static_spec(int)
    {
      ++m;                      // reference to non-static member
    }

    template<typename U>
    static void expl_static_spec(U)
    {
      ++m;                      // reference to non-static member
    }

    template<>
    static void expl_static_spec(int) // ill-formed: "static" (accepted by MSVC)
    {
      ++m;                      // reference to non-static member
    }

    template<typename U>
    void non_static_spec(U)
    {
      ++m;
    }

    template<>
    void non_static_spec(int)
    {
      ++m;
    }

    template<typename U>
    static int impl_static_spec_var;

    template<>
    int impl_static_spec_var<int>;

    template<typename U>
    static int expl_static_spec_var;

    template<>
    static int expl_static_spec_var<int>; // ill-formed: "static" (accepted by MSVC and clang)
  };

  template<>
  void C::impl_static_spec(long)
  {
    ++m;                        // reference to non-static member
  }

  template<>
  static void C::expl_static_spec(long) // ill-formed: "static" (accepted by MSVC)
  {
    ++m;                        // reference to non-static member
  }

  template<>
  int C::impl_static_spec_var<long> = 1;

  template<>
  static int C::expl_static_spec_var<long> = 1; // ill-formed: "static"

  void foo()
  {
    C::impl_static_spec('a');
    C::impl_static_spec(1);

    C::expl_static_spec('a');
    C::expl_static_spec(1);

    C().non_static_spec('a');
    C().non_static_spec(1);

    ++C::impl_static_spec_var<char>;
    ++C::impl_static_spec_var<int>;

    ++C::expl_static_spec_var<char>;
    ++C::expl_static_spec_var<int>;
  }
}

namespace tmpl
{
  template<typename T>
  struct C
  {
    int m;

    template<typename U>
    static void impl_static_spec(U)
    {
      ++m;                      // reference to non-static member
    }

    template<>
    void impl_static_spec(int)
    {
      ++m;                      // reference to non-static member
    }

    template<typename U>
    static void expl_static_spec(U)
    {
      ++m;                      // reference to non-static member
    }

    template<>
    static void expl_static_spec(int) // ill-formed: "static" (accepted by MSVC)
    {
      ++m;                      // reference to non-static member
    }

    template<typename U>
    void non_static_spec(U)
    {
      ++m;
    }

    template<>
    void non_static_spec(int)
    {
      ++m;
    }

    template<typename U>
    static int impl_static_spec_var;

    template<>
    int impl_static_spec_var<int>;

    template<typename U>
    static int expl_static_spec_var;

    template<>
    static int expl_static_spec_var<int>; // ill-formed: "static" (accepted by MSVC and clang)
  };

  template<> template<>
  void C<long>::impl_static_spec(long)
  {
    ++m;                        // reference to non-static member
  }

  template<> template<>
  static void C<long>::expl_static_spec(long) // ill-formed: "static" (accepted by MSVC)
  {
    ++m;                        // reference to non-static member
  }

  template<> template<>
  int C<long>::impl_static_spec_var<long> = 1;

  template<> template<>
  static int C<long>::expl_static_spec_var<long> = 1; // ill-formed: "static"

  void foo()
  {
    C<int>::impl_static_spec('a');
    C<int>::impl_static_spec(1);

    C<int>::expl_static_spec('a');
    C<int>::expl_static_spec(1);

    C<int>().non_static_spec('a');
    C<int>().non_static_spec(1);

    ++C<int>::impl_static_spec_var<char>;
    ++C<int>::impl_static_spec_var<int>;

    ++C<int>::expl_static_spec_var<char>;
    ++C<int>::expl_static_spec_var<int>;
  }
}

namespace cls_tmpl_spec
{
  template<int I>
  struct C
  { };

  template<> struct C<0>
  { };

  template<> extern struct C<1> // ill-formed: "extern"
  { };

  template<> static struct C<2> // ill-formed: "static"
  { };
}
