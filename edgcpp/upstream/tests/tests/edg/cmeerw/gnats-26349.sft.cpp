//type:fp
//options:--c++11:--c++20:--ms_c++20

namespace minimal {
  struct B {
  protected:
    enum E { E1 };
  };
  struct D : B {
    using B::E;
  };
  auto e = D::E::E1;
}

namespace friend_class
{
  class C {
    friend class B;
    enum E { E1 };
  };
  struct B {
    using E = C::E;
  };
  struct D : B {
    static const E e = E::E1;   // Previously: inaccessible
  };
}

namespace with_using_decl
{
  struct B
  {
  protected:
    enum E { E1 };
  };

  struct D : B
  {
    using B::E;
  };

  auto e = D::E::E1;
}

namespace in_namespace_scope
{
  namespace ns
  {
    enum E { E1 };

    enum class EC { E1 };
  }

  namespace ns2
  {
    using E = ns::E;
    using EC = ns::EC;

    E f()
    {
      return E::E1;
    }

    EC fc()
    {
      return EC::E1;
    }

    E g()
    {
      return ns::E1;
    }

    E h()
    {
      return ns::E::E1;
    }

    EC hc()
    {
      return ns::EC::E1;
    }
  }
}

namespace in_class_scope
{
  class C
  {
    friend class B;

    enum E { E1 };

    enum class EC { E1 };
  };

  struct B
  {
    using E = C::E;
    using EC = C::EC;

    E f()
    {
      return E::E1;
    }

    EC fc()
    {
      return EC::E1;
    }

    E g()
    {
      return C::E1;
    }

    E h()
    {
      return C::E::E1;
    }

    EC hc()
    {
      return C::EC::E1;
    }
  };

  struct D : public B
  {
    E f()
    {
      return E::E1;
    }

    EC fc()
    {
      return EC::E1;
    }
  };
}
