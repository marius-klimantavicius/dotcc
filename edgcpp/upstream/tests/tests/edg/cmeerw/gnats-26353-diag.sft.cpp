//type:fn
//options:--c++11
//options_all:-w

namespace parse_complete_class
{
  struct C
  {
    friend void f(const C &) noexcept(invalid); // error
  };
}

namespace parse_complete_tmpl_class
{
  template<typename T>
  struct C
  {
    friend void f(const C &) noexcept(invalid); // error
  };

  C<int> c;
}

namespace tmpl_parse_complete_class
{
  struct C
  {
    template<typename U>
    friend void f(const C &) noexcept(invalid); // may diagnose
  };
}

namespace tmpl_parse_complete_tmpl_class
{
  template<typename T>
  struct C
  {
    template<typename U>
    friend void f(const C &) noexcept(invalid); // may diagnose

    template<typename U>
    friend void g(const C &) noexcept(T::invalid); // may diagnose

    template<typename U>
    friend void h(const C &) noexcept(U::invalid);
  };

  C<int> c;
}

namespace declaration_matching
{
  template<typename T>
  struct C
  {
    friend void f(const C &, T t) noexcept(sizeof t != 4);
    friend void f(const C &, T t) noexcept(sizeof t == 4); // incompatible
  };

  C<int> c;
}
