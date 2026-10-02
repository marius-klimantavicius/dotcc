//type:fp
//options:--c++20:--ms_c++20
//options_all:-w

namespace minimal
{
  enum E { E1 };
  struct B {
    using E::E1;
  };
  struct D : B {
    void f() {
      E1;
    }
  };
}

namespace using_namespace_scope_enumerator
{
  namespace ns
  {
    enum E { E1 };
  }

  struct B
  {
    using ns::E::E1;
  };

  struct D : B
  {
    void f()
    {
      E1;
    }
  };
}

namespace using_namespace_scope_enumeration
{
  namespace ns
  {
    enum E { E1 };
  }

  struct B
  {
    using enum ns::E;
  };

  struct D : B
  {
    void f()
    {
      E1;
    }
  };
}

namespace using_class_scope_enumerator
{
  struct EB
  {
    enum E { E1 };
  };

  struct B : EB
  {
    using E::E1;
  };

  struct D1 : B
  {
    using B::E1;
  };

  struct D2 : D1
  {
    void f()
    {
      E1;
    }
  };
}

namespace using_class_scope_enumeration
{
  struct EB
  {
    enum E { E1 };
  };

  struct B : EB
  {
    using enum E;
  };

  struct D1 : B
  {
    using B::E1;
  };

  struct D2 : D1
  {
    void f()
    {
      E1;
    }
  };
}

namespace using_mbr_of_using_enum
{
  namespace ns
  {
    enum E { E1, E2 };
  }

  struct C
  {
    using ns::E::E1;
  };

  struct D1 : C
  {
    using C::E1;
  };

  struct D2 : D1
  {
    using D1::E1;
  };
}
