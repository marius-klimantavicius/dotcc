//type:fp
//options:--ms_c++17 --microsoft_version 1936:--ms_c++20 --microsoft_version 1936 --ms_permissive

namespace minimal
{
  template<typename T>
  struct C : B {};
  struct B {};
}

namespace ignore_inner_variable
{
  struct B
  { };

  namespace inner
  {
    int B;

    template<typename T>
    struct C : B                // accepted in MS permissive mode
    { };
  }
}

namespace ignore_inner_typedef
{
  struct B
  { };

  namespace inner
  {
    using B = int;

    template<typename T>
    struct C : B                // accepted in MS permissive mode
    { };
  }
}

namespace use_base_class
{
  template<typename T>
  struct C : X
  {
    void f()
    {
      type t = 0;
      g(t);
    }

    struct D : X
    {
      void f()
      {
        type t = 0;
        g(t);
      }
    };
  };

  struct X
  {
    using type = int;

    void g(int)
    { }
  };

  void f(C<void> c, C<void>::D d)
  {
    c.f();
    d.f();
  }
}
