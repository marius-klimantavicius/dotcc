//type:fp
//options:--c++11:--c++20:--c++20 --gn 130200:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename>
  struct C {
    template<typename> friend struct A;
    struct A;
  };
}

namespace non_type_tmpl_param
{
  template<typename T>
  struct C
  {
    template<T>
    friend struct A;
  };

  C<int> c;
}

namespace non_type_tmpl_param_with_access_check
{
  template<typename T>
  class C
  {
    int A;

    template<T>
    friend struct A;

    static const int m = 0;
  };

  C<int> c;

  template<int>
  struct A
  {
    static int f() { return C<int>::m; }
  };

  int i = A<1>::f();
}

namespace decl_scope
{
  namespace ns
  {
    struct C
    {
      template<typename>
      friend struct A;

      struct A;
    };
  }
}

namespace tmpl_decl_scope
{
  namespace ns
  {
    template<typename>
    struct C
    {
      template<typename>
      friend struct A;

      struct A;
    };

    template struct C<int>;
  }
}

namespace tmpl_decl_scope_access
{
  namespace ns
  {
    template<typename>
    struct C
    {
      template<typename>
      friend struct A;

    private:
      static const int value = 0;
    };

    template<typename>
    struct A
    {
      static const int value = C<int>::value;
    };

    template struct C<int>;
    template struct A<int>;
  }
}

namespace tmpl_decl_scope_existing_access
{
  namespace ns
  {
    template<typename>
    struct A;

    template<typename>
    struct C
    {
      template<typename>
      friend struct A;

    private:
      static const int value = 0;
    };

    template<typename>
    struct A
    {
      static const int value = C<int>::value;
    };

    template struct C<int>;
    template struct A<int>;
  }
}

namespace use_friend_template
{
  template<typename T>
  struct A
  {
    template<typename>
    friend struct C;
  };

  template<typename>
  struct C { };

  C<int> c;
}

namespace use_dpdt_base_class_friend
{
  template<typename T>
  struct A
  {
    template<typename>
    friend struct C;
  };

  template<typename T>
  struct B : A<T>
  { };

  template<typename>
  struct C { };

  C<int> c;
}

namespace template_non_type_template_parameter
{
  template<int>
  struct V;

  template<int V>
  struct A {
    template<int>
    friend struct V;            // clang complains
  };

  A<1> a;
}

namespace non_template_non_type_template_parameter
{
  struct V;

  template<int V>
  struct A {
    friend struct V;
  };

  A<1> a;
}
