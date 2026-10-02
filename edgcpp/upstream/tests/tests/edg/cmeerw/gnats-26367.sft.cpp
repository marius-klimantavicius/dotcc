//type:fp
//options:--c++11:--ms_c++20

namespace minimal
{
  template<typename ... B> using P = void (*)(B ...);
  struct C {
    template<typename ... A>
    void f(P<A ...>);
  };
  template<typename ... A>
  void C::f(void (*)(A ...))  // Previously a spurious error.  Now okay.
  { }
}

namespace pack_only
{
  template<typename ... A>
  struct C
  {
    template<typename T> using P = void (T::*)(A ...);
    template<typename T> void f(P<T>);
  };

  template<typename ... A> template<typename T>
  void C<A ...>::f(void (T::*)(A ...))
  { }
}

namespace non_pack_and_pack
{
  template<typename A1, typename ... A>
  struct C
  {
    template<typename T>
    using P = void (T::*)(A1, A ...);

    template<typename T>
    void f(P<T>);
  };

  template<typename A1, typename ... A>
  template<typename T>
  void C<A1, A ...>::f(void (T::*)(A1, A ...))
  { }
}

namespace pack_and_non_pack
{
  template<typename A1, typename ... A>
  struct C
  {
    template<typename T>
    using P = void (T::*)(A ..., A1);

    template<typename T>
    void f(P<T>);
  };

  template<typename A1, typename ... A>
  template<typename T>
  void C<A1, A ...>::f(void (T::*)(A ..., A1))
  { }
}

namespace alias_with_pack
{
  template<typename ... A>
  struct C {
    template<typename T, typename ... B> using P = void (T::*)(B ...);
    template<typename T> void f(P<T, A ...>);
  };
  template<typename ... A> template<typename T>
  void C<A ...>::f(void (T::*)(A ...))
  { }
}

namespace out_of_class_alias
{
  template<typename T, typename ... B> using P = void (T::*)(B ...);

  template<typename ... A>
  struct C {
    template<typename T> void f(P<T, A ...>);
  };
  template<typename ... A> template<typename T>
  void C<A ...>::f(void (T::*)(A ...))
  { }
}

namespace non_template_class
{
  template<typename T, typename ... B> using P = void (T::*)(B ...);

  struct C
  {
    template<typename T, typename ... A>
    void f(P<T, A ...>);
  };

  template<typename T, typename ... A>
  void C::f(void (T::*)(A ...))
  { }
}

namespace non_template_class_with_ptr
{
  template<typename T, typename ... B> using P = void (T::*)(B *...);

  struct C
  {
    template<typename T, typename ... A>
    void f(P<T, A ...>);
  };

  template<typename T, typename ... A>
  void C::f(void (T::*)(A *...))
  { }
}
