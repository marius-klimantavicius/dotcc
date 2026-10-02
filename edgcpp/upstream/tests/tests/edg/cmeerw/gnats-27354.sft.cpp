//type:fp
//options:--c++11:--c++20:--c++20 --gn 140100:--c++20 --clang_version 190100:--ms_c++17 --microsoft_version 1927:--ms_c++20 --microsoft_version 1936 --ms_permissive:--ms_c++20 --microsoft_version 1936 --no_ms_permissive
//options_all:-w

namespace minimal
{
  template<typename T> struct B {
    enum E { };
  };
  template<typename T> struct C : public B<T> {
    void f(enum B<T>::E);
  };
  template<typename T>
  void C<T>::f(enum B<T>::E) { }
}

namespace NO_MEMBER_DECLARED
{
  template<typename T> struct B
  { };

  template<typename T> struct C : public B<T>
  {
    void f(typename B<T>::X);
  };

  template<typename T>
  void C<T>::f(typename B<T>::X)
  { }
}

namespace ENUM_MEMBER_DECLARED
{
  template<typename T> struct B
  {
    enum E { };
  };

  template<typename T> struct C : public B<T>
  {
    void f(typename B<T>::E);
  };

  template<typename T>
  void C<T>::f(typename B<T>::E)
  { }
}

namespace MULTIPLE_ENUM_MEMBERS_DECLARED
{
  template<typename T> struct B
  {
    enum E1 { };
    enum E2 { };
  };

  template<typename T> struct C : public B<T>
  {
    void f(typename B<T>::E1);
    void f(typename B<T>::E2);
  };

  template<typename T>
  void C<T>::f(typename B<T>::E1)
  { }

  template<typename T>
  void C<T>::f(typename B<T>::E2)
  { }
}

namespace CLASS_MEMBER_DECLARED
{
  template<typename T> struct B
  {
    class C { };
  };

  template<typename T> struct C : public B<T>
  {
    void f(typename B<T>::C);
  };

  template<typename T>
  void C<T>::f(typename B<T>::C)
  { }
}

namespace USE_AS_TEMPLATE_ARG
{
  template<typename> struct B
  {
    enum E { };
  };

  template<typename> struct D
  { };

  template<typename T> struct C : B<T>
  {
    void f(D<typename B<T>::E>);
  };

  template<typename U>
  void C<U>::f(D<typename B<U>::E>)
  { }
}

namespace FUNC_SCOPE_ENUMS
{
  template<typename, typename>
  struct is_same
  { static constexpr bool value = false; };

  template<typename T>
  struct is_same<T, T>
  { static constexpr bool value = true; };

  template<typename T>
  struct C
  { };

  template<typename T>
  void f()
  {
    {
      enum E { };
      C<E> c1;
    }

    {
      enum E { };
      using EE = E;
      C<E> c1;

      {
        enum E { };
        C<E> c2;

        static_assert(!is_same<E, EE>::value, "");
        static_assert(!is_same<decltype(c1), decltype(c2)>::value, "");
      }
    }
  }

  template void f<int>();
}

namespace UNNAMED_MEMBER_CLASS
{
  template<typename T> struct B
  {
    using E1 = struct { };
    using E2 = struct { };
  };

  template<typename T>
  struct C : public B<T>
  {
    void f(typename B<T>::E1) { }
    void f(typename B<T>::E2) { }
  };

  void f(C<int> c)
  {
    c.f(B<int>::E1());
    c.f(B<int>::E2());
  }
}

namespace UNNAMED_MEMBER_ENUM
{
  template<typename T> struct B
  {
    using E1 = enum { };
    using E2 = enum { };
  };

  template<typename T>
  struct C : public B<T>
  {
    void f(typename B<T>::E1) { }
    void f(typename B<T>::E2) { }
  };

  void f(C<int> c)
  {
    c.f(B<int>::E1());
    c.f(B<int>::E2());
  }
}

namespace UNNAMED_MEMBER_ENUM_DECLTYPE
{
  template<typename T>
  struct C
  {
    enum { E1 };
    enum { E2 };

    void f(decltype(E1)) { }
    void f(decltype(E2)) { }
  };

  void f(C<int> c)
  {
    c.f(C<int>::E1);
    c.f(C<int>::E2);
  }
}
