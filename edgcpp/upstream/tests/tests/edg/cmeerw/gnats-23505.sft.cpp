//type:fn
//options:--c++14;fp:--g++ --clang --ms_compatibility:--ms_c++14 --microsoft_version=1903:--ms_c++17 --microsoft_version=1930

namespace mbr
{
  template<int I>
  struct C
  {
    static const int var;

    template<int J>
    static const int tvar;
  };

  template<>
  const int C<0>::var;          // missing initializer in MSVC mode

  template<>
  const int C<0>::var = 2;      // already defined in MSVC mode

  template<> template<>
  const int C<0>::tvar<0>;      // missing initializer in MSVC >= 1910

  template<> template<>
  const int C<0>::tvar<0> = 2;
}

namespace mbr_tmpl
{
  struct C
  {
    template<int J>
    static const int tvar;
  };

  template<>
  const int C::tvar<0>;         // missing initializer for MSVC >=1910

  template<>
  const int C::tvar<0> = 2;
}

namespace trivial_ctor
{
  struct B
  { };

  struct C
  {
    template<int J>
    static B bvar;

    template<>
    B bvar<0>;
  };

  template<>
  B C::bvar<0> = B();
}

namespace non_trivial_ctor
{
  struct B
  {
    B();
  };

  struct C
  {
    template<int J>
    static B bvar;

    template<>
    B bvar<0>;
  };

  template<>
  B C::bvar<0> = B();           // already defined for MSVC 1930
}
