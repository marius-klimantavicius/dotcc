//type:fn
//options:--c++11 --microsoft_version 1800;fp:--c++11 --microsoft_version 1900:--c++11 --microsoft_version 1936
//options_all:-w -tused

namespace copy_ctor
{
  struct C
  {
    C(const C &);
    C(int);
  };

  struct D
  {
    operator C() const;
    operator int() const;
  };

  void f(D d)
  {
    C c1 = d;
    C c2(d);
    C c3{d};
    C c4 = { d };
  }
}

namespace move_ctor
{
  struct C
  {
    C(C &&);
    C(int);
  };

  struct D
  {
    operator C() const;
    operator int() const;
  };

  void f(D d)
  {
    C c1 = d;
    C c2(d);                    // error: MSVC >= 1900
    C c3{d};                    // error: MSVC >= 1900
    C c4 = { d };               // error: MSVC >= 1900
  }
}
