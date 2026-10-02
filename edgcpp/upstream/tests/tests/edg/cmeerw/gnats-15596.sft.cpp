//type:fp
//options:--c++11:--c++20:--ms_c++20

namespace minimal
{
  struct B {
    template<typename T> B(T) noexcept;
  };
  struct D : B {
    using B::B;
  };
  static_assert(noexcept(D(1)), "Unexpected");
}

namespace non_tmpl
{
  struct B
  {
    B(int) noexcept(true);
  };

  struct D : B
  {
    using B::B;
  };

  static_assert(noexcept(D{1}), "");
  static_assert(noexcept(B{1}), "");
}

namespace templated_ctor
{
  template<typename T>
  struct B
  {
    B(int) noexcept(sizeof(T) != 0);
    B(long) noexcept(T::dont_instantiate);
  };

  struct D : B<int>
  {
    using B::B;
  };

  static_assert(noexcept(D{1}), "");
  static_assert(noexcept(B<int>{1}), "");
}

namespace template_ctor
{
  struct B
  {
    template<typename T>
    B(T) noexcept(sizeof(T) != 0);

    template<typename T>
    B(T *) noexcept(sizeof(T) != 0)
    { }
  };

  struct D : B
  {
    using B::B;
  };

  static_assert(noexcept(D{1}), "");
  static_assert(noexcept(B{1}), "");

  static_assert(noexcept(D{""}), "");
  static_assert(noexcept(B{""}), "");
}

namespace multiple_bases
{
  template<bool B>
  struct X
  {
    X() noexcept(B);
  };

  struct B
  {
    template<typename T>
    B(T) noexcept(sizeof(T) != 0);
  };

  template<typename T>
  struct D : B, T
  {
    using B::B;
  };

  static_assert( noexcept(D<X<true>>{1}), "");
  static_assert(!noexcept(D<X<false>>{1}), "");
  static_assert( noexcept(D<X<true>>{'c'}), "");
  static_assert(!noexcept(D<X<false>>{'c'}), "");
}
