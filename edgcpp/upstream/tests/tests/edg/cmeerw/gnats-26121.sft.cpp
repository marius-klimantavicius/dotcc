//type:fp
//options:--c++20:--c++20 --gn 120100:--ms_c++20:--c++20 --clang_version 160000

namespace minimal
{
  template<int>
  struct B {
    B(const B&);
    B(B &&) requires false;
  };
  struct D : public B<0> { };
  D &&g();
  D d(g());
}

namespace non_tmpl_ctor
{
  template<int I>
  struct B
  {
    B() { }
    B(const B&) { }

    B(B &&) requires false;
  };

  struct D : public B<0>
  {
    D();
  };

  void f(D d)
  {
    D dd(static_cast<D &&>(d));
  }
}

namespace non_tmpl_assign
{
  template<int I>
  struct B
  {
    B() { }

    B &operator =(const B&)
    { return *this; }

    B &operator =(B &&) requires false;
  };

  struct D : public B<0>
  {
    D();
  };

  void f(D d)
  {
    d = D();
  }
}
